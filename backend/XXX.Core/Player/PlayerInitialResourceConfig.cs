using XXX.Entity;

namespace XXX.Player
{
    /// <summary>
    /// 玩家初始资源运行时配置目录。
    /// </summary>
    public static class PlayerInitialResourceConfig
    {
        private static PlayerInitialResourceConfigEntity? _current;

        public static bool HasRuntimeConfig => _current != null;

        public static int StartLevel => _current?.StartLevel
            ?? throw new InvalidOperationException("Player initial resource config is missing. Please run startup seed sync before creating players.");

        public static long StartExp => _current?.StartExp
            ?? throw new InvalidOperationException("Player initial resource config is missing. Please run startup seed sync before creating players.");

        public static long StartGold => _current?.StartGold
            ?? throw new InvalidOperationException("Player initial resource config is missing. Please run startup seed sync before creating players.");

        public static long StartSpiritStone => _current?.StartSpiritStone
            ?? throw new InvalidOperationException("Player initial resource config is missing. Please run startup seed sync before creating players.");

        public static void ReplaceConfig(PlayerInitialResourceConfigEntity? config)
        {
            _current = config;
        }

        public static void ClearRuntimeConfig()
        {
            _current = null;
        }
    }
}
