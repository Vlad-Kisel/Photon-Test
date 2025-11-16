namespace _Project.Code.Infrastructure.Network
{
    public class StatModifier<TStat, TState> : IStatModifier
        where TStat : IModifiableStat<TState>
    {
        private readonly TState _modifier;
        private readonly ModifierOperationType _operationType;
        private readonly IModifierApplier _calculator;

        public StatModifier(
            TState modifier,
            ModifierOperationType operationType,
            IModifierApplier calculator)
        {
            _modifier = modifier;
            _operationType = operationType;
            _calculator = calculator;
        }

        public void Apply(Stats stats)
        {
            IModifiableStat<TState> stat = stats.GetModifiableStat<TState>();
            
            _calculator.Apply<TStat, TState>(
                stat,
                in _modifier,
                _operationType
            );
        }
    }
}