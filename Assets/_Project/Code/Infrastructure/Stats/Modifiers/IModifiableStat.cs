namespace _Project.Code.Infrastructure.Network
{
    public interface IModifiableStat<TStatState> : IStat
    {
        void Apply(in TStatState modifier, BinaryStatOperation operation);
    }
}