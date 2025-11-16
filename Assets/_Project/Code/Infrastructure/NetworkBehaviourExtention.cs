using Fusion;

namespace _Project.Code.Infrastructure.Network
{
    public static class NetworkBehaviourExtention
    {
        public static bool IsValid(this NetworkBehaviour networkBehaviour)
        {
            return networkBehaviour != null && networkBehaviour.Object != null && networkBehaviour.Object.IsValid;
        }
    }
}