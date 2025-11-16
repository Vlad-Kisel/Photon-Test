using System;

namespace _Project.Code.Infrastructure.Network
{
    public class DefaultModifierApplier : IModifierApplier
    {
        public void Apply<TStat, TStatState>(IModifiableStat<TStatState> stat,in TStatState modifier,ModifierOperationType type)
        {
            var operation = type switch
            {
                ModifierOperationType.Add => (BinaryStatOperation)((a, b) => a + b),
                ModifierOperationType.Multiply => (a, b) => a * b,
                _ => throw new ArgumentOutOfRangeException()
            };

            stat.Apply(in modifier, operation);
        }
    }
}