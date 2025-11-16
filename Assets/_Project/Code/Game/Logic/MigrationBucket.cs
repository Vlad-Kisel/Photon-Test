using System;
using System.Collections.Generic;
using Fusion;

namespace _Project.Code.Infrastructure.Network
{
    public class MigrationBucket<T>
        where T : NetworkBehaviour
    {
        private readonly List<NetworkObject> _snapshots = new();
        private readonly List<T> _spawned = new();

        public IReadOnlyList<NetworkObject> Snapshots => _snapshots;
        public IReadOnlyList<T> Spawned => _spawned;

        public void Clear()
        {
            _snapshots.Clear();
            _spawned.Clear();
        }

        public void Collect(NetworkObject snapshot)
        {
            if (snapshot.TryGetBehaviour<T>(out _))
                _snapshots.Add(snapshot);
        }

        public void Resume(NetworkRunner runner, Predicate<T> filter = null)
        {
            foreach (var snapshot in _snapshots)
            {
                var shouldKeep = filter?.Invoke(snapshot.GetComponent<T>()) ?? true;

                if(!shouldKeep)
                    continue;
                
                var newNetworkObject = SpawnFromSnapshot(runner, snapshot);
                var behaviour = newNetworkObject.GetComponent<T>();
                
                behaviour.Object.CopyStateFrom(snapshot);
                _spawned.Add(behaviour);
            }
        }

        private NetworkObject SpawnFromSnapshot(
            NetworkRunner runner,
            NetworkObject snapshot)
        {
            var networkTransform = snapshot.GetComponent<NetworkTRSP>();

            var position = networkTransform.Data.Position;
            var rotation = networkTransform.Data.Rotation;

            return runner.Spawn(snapshot, position, rotation, snapshot.InputAuthority);
        }
    }
}