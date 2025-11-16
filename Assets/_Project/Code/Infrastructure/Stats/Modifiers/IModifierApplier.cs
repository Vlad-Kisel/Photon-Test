namespace _Project.Code.Infrastructure.Network
{
    public interface IModifierApplier
    {
        public void Apply<TStat, TStatState>(IModifiableStat<TStatState> stat, in TStatState modifier,
            ModifierOperationType type);
    }
}