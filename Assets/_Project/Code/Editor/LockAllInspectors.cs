using UnityEditor;
using UnityEngine;
using System;
using System.Reflection;

public static class LockAllInspectors
{
    [MenuItem("Tools/Inspector/Toggle Lock %&#i")] // Ctrl + Alt + Shift + I
    public static void Lock()
    {
        SetLockState(true);
    }

    [MenuItem("Tools/Inspector/Unlock All Inspectors %#u")] // Ctrl + Shift + U
    public static void Unlock()
    {
        SetLockState(false);
    }

    private static void SetLockState(bool locked)
    {
        Type inspectorType = typeof(Editor).Assembly.GetType("UnityEditor.InspectorWindow");
        if (inspectorType == null)
        {
            Debug.LogError("InspectorWindow type not found");
            return;
        }

        PropertyInfo isLockedProperty =
            inspectorType.GetProperty("isLocked", BindingFlags.Instance | BindingFlags.Public);

        UnityEngine.Object[] inspectors = Resources.FindObjectsOfTypeAll(inspectorType);

        foreach (var inspector in inspectors)
        {
            isLockedProperty?.SetValue(inspector, locked, null);
        }
    }
}