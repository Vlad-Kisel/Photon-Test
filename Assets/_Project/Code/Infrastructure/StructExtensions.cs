namespace _Project.Code.Infrastructure.Network
{
    public static class StructExtensions
    {
        public static bool IsDefault<T>(this T value) where T : struct
        {
            return value.Equals(default(T));
        }
        
        public static bool IsNotDefault<T>(this T value) where T : struct
        {
            return !value.Equals(default(T));
        }
    }
}