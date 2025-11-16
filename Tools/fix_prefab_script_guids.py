#!/usr/bin/env python3
"""
Restore MonoBehaviour script GUIDs in prefabs by matching serialized field names
to scripts. Run from repo root. Requires .cs and .cs.meta to exist for each script.
"""
import os
import re
from pathlib import Path
from typing import Dict, List, Optional, Set, Tuple

# Unity reserved keys for MonoBehaviour (not script-specific fields)
MONO_RESERVED = {
    "m_ObjectHideFlags", "m_CorrespondingSourceObject", "m_PrefabInstance",
    "m_PrefabAsset", "m_GameObject", "m_Enabled", "m_EditorHideFlags",
    "m_Script", "m_Name", "m_EditorClassIdentifier",
}

ASSETS = Path(__file__).resolve().parent.parent / "Assets"
SCRIPT_GUID_RE = re.compile(r"guid:\s*([a-fA-F0-9]{32})")


def get_serialized_fields_from_cs(cs_path: Path) -> Set[str]:
    """Extract serialized field names from a C# file (SerializeField or public)."""
    text = cs_path.read_text(encoding="utf-8", errors="ignore")
    fields = set()
    # Match: [SerializeField] / [field: SerializeField] ... (public|private|protected) ... Type FieldName ;
    # or: public ... FieldName ;
    for line in text.splitlines():
        stripped = line.strip()
        if not stripped or stripped.startswith("//"):
            continue
        # Serialized: has [SerializeField] or [field: SerializeField], or is public (and not NonSerialized)
        has_serialize = "SerializeField" in line and "NonSerialized" not in line
        is_public = " public " in line or line.strip().startswith("public ")
        if not (has_serialize or (is_public and "NonSerialized" not in line)):
            continue
        # Field name: last identifier before ; or => or {
        m = re.search(r"\s+([a-zA-Z_][a-zA-Z0-9_]*)\s*[;=>{]", line)
        if m:
            fields.add(m.group(1))
    return fields


def get_guid_from_meta(meta_path: Path) -> Optional[str]:
    with open(meta_path, encoding="utf-8", errors="ignore") as f:
        for line in f:
            if line.startswith("guid:"):
                return line.split(":", 1)[1].strip()
    return None


def index_scripts() -> Dict[frozenset, List[Tuple[str, str]]]:
    """Build map: frozenset(field_names) -> [(script_path, guid), ...]"""
    index = {}
    for root, _dirs, files in os.walk(ASSETS):
        root = Path(root)
        for f in files:
            if not f.endswith(".cs"):
                continue
            cs_path = root / f
            meta_path = root / (f + ".meta")
            if not meta_path.is_file():
                continue
            guid = get_guid_from_meta(meta_path)
            if not guid:
                continue
            fields = get_serialized_fields_from_cs(cs_path)
            if not fields:
                continue
            key = frozenset(fields)
            rel = cs_path.relative_to(ASSETS)
            entry = (str(rel), guid)
            if key not in index:
                index[key] = []
            if entry not in index[key]:
                index[key].append(entry)
    return index


def extract_mono_blocks(content: str):
    """Yield (block_start_line_ix, block_text, block_lines)."""
    lines = content.splitlines(keepends=True)
    i = 0
    while i < len(lines):
        if lines[i].startswith("--- !u!114 "):
            start = i
            block = []
            i += 1
            while i < len(lines) and not lines[i].startswith("--- "):
                block.append(lines[i])
                i += 1
            yield start, "".join(block), block
            continue
        i += 1


def get_custom_keys_from_block(block_lines: List[str]) -> Set[str]:
    keys = set()
    for line in block_lines:
        # Top-level key: exactly two leading spaces, then key:
        m = re.match(r"^  ([a-zA-Z_][a-zA-Z0-9_]*)\s*:", line)
        if m:
            key = m.group(1)
            if key not in MONO_RESERVED:
                keys.add(key)
    return keys


def get_script_guid_from_block(block_lines: List[str]) -> Optional[str]:
    for line in block_lines:
        if "m_Script:" in line:
            g = SCRIPT_GUID_RE.search(line)
            if g:
                return g.group(1)
    return None


def replace_guid_in_block(block_lines: List[str], new_guid: str) -> List[str]:
    out = []
    for line in block_lines:
        if "m_Script:" in line and "guid:" in line:
            line = SCRIPT_GUID_RE.sub(f"guid: {new_guid}", line)
        out.append(line)
    return out


def process_prefab(prefab_path: Path, script_index: Dict[frozenset, List[Tuple[str, str]]]) -> Tuple[bool, List]:
    """Returns (changed, list of (component_info, new_guid or None))."""
    content = prefab_path.read_text(encoding="utf-8", errors="ignore")
    lines = content.splitlines(keepends=True)
    changes = []
    modified = False

    for start_ix, block_text, block_lines in extract_mono_blocks(content):
        custom_keys = get_custom_keys_from_block(block_lines)
        if not custom_keys:
            continue
        old_guid = get_script_guid_from_block(block_lines)
        if not old_guid:
            continue
        key = frozenset(custom_keys)
        candidates = script_index.get(key)
        if not candidates:
            continue
        # Prefer script from same folder or _Project
        prefab_rel = str(prefab_path.relative_to(ASSETS))
        best = candidates[0]
        for c in candidates:
            if "_Project" in c[0] and "_Project" in prefab_rel:
                best = c
                break
        new_guid = best[1]
        if new_guid != old_guid:
            new_block = replace_guid_in_block(block_lines, new_guid)
            for j, new_line in enumerate(new_block):
                lines[start_ix + 1 + j] = new_line
            modified = True
            changes.append((key, old_guid, new_guid, best[0]))

    if modified:
        prefab_path.write_text("".join(lines), encoding="utf-8", newline="")

    return modified, changes


def main():
    print("Indexing scripts...")
    script_index = index_scripts()
    print(f"Indexed {sum(len(v) for v in script_index.values())} scripts by field sets.")

    prefabs = list(ASSETS.rglob("*.prefab"))
    print(f"Processing {len(prefabs)} prefabs...")
    fixed_count = 0
    for p in prefabs:
        try:
            changed, details = process_prefab(p, script_index)
            if changed:
                fixed_count += 1
                rel = p.relative_to(ASSETS)
                print(f"  Fixed: {rel}")
                for _k, old_g, new_g, script in details:
                    print(f"    -> {script} (guid {new_g})")
        except Exception as e:
            print(f"  Error {p}: {e}")

    print(f"Done. Updated {fixed_count} prefab(s).")


if __name__ == "__main__":
    main()
