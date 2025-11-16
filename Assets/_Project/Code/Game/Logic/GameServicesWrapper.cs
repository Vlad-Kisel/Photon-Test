namespace _Project.Code.Infrastructure.Network
{
    public class GameServicesWrapper
    {
        public GameLogic GameLogic { get; set; }

        public void Clear()
        {
            GameLogic = null;
        }
    }
}