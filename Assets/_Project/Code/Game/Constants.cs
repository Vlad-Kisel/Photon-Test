namespace _Project.Code.Infrastructure.Network
{
    public static class Constants
    {
        // ----------------------------
        // General
        // ----------------------------
        public const int MaxPlayerCount = 50;
        public const int MaxEnemyCount = 100;

        // ----------------------------
        // Player Base Stats
        // ----------------------------
        public const float BasePlayerAttackSpeed = 2f;
        public const int BasePlayerDamage = 1;
    
        public const float BasePlayerSpeed = 2f;

        public const int MaxPlayerHealth = 1000;
        public const int StartPlayerHealth = 100;

        // ----------------------------
        // Enemy Base Stats
        // ----------------------------
        public const int MaxEnemyHealth = 100;
        public const int StartEnemyHealth = 100;

        public const int EnemyAttack = 20;

        // ----------------------------
        // Leveling
        // ----------------------------
        public const float LevelExperienceGrowthFactor = 1.5f;

        public const int StartPlayerLevel = 1;
        public const int StartPlayerExperience = 0;

        // ----------------------------
        // Default Stat States
        // ----------------------------
        public static readonly AttackStatState BasePlayerAttackStat = new()
        {
            AttackSpeed = BasePlayerAttackSpeed,
            Damage = BasePlayerDamage,
        };

        public static readonly MovementStatState BasePlayerMovementStat = new()
        {
            Speed = BasePlayerSpeed,
        };

        public static readonly HealthStatState BasePlayerHealthStat = new()
        {
            Current = StartPlayerHealth,
            Max = MaxPlayerHealth,
        };

        public static readonly LevelStatState BasePlayerLevelStat = new()
        {
            Level = StartPlayerLevel,
            Experience = StartPlayerExperience,
        };

    }
}