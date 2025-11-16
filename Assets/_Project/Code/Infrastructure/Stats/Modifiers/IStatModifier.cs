namespace _Project.Code.Infrastructure.Network
{
    public interface IStatModifier
    {
        void Apply(Stats stats);
    }
}