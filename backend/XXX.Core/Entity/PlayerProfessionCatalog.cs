namespace XXX.Entity
{
    /// <summary>
    /// 玩家战斗职业目录。
    /// </summary>
    public static class PlayerProfessionCatalog
    {
        public const string Warrior = "warrior";

        public const string Mage = "mage";

        public const string Body = "body";

        public const string All = "all";

        private static readonly IReadOnlyList<string> PlayableProfessions = new[]
        {
            Warrior,
            Mage,
            Body
        };

        public static IReadOnlyList<string> GetPlayableProfessions() => PlayableProfessions;

        public static string Normalize(string? profession, string fallback = Warrior)
        {
            var normalized = (profession ?? string.Empty).Trim().ToLowerInvariant();
            return PlayableProfessions.Contains(normalized, StringComparer.Ordinal)
                ? normalized
                : fallback;
        }

        public static bool IsPlayable(string? profession)
        {
            var normalized = (profession ?? string.Empty).Trim().ToLowerInvariant();
            return PlayableProfessions.Contains(normalized, StringComparer.Ordinal);
        }

        public static List<string> NormalizeAllowedProfessions(IEnumerable<string>? professions)
        {
            var normalized = (professions ?? Enumerable.Empty<string>())
                .Select(item => (item ?? string.Empty).Trim().ToLowerInvariant())
                .Where(item => item == All || PlayableProfessions.Contains(item, StringComparer.Ordinal))
                .Distinct(StringComparer.Ordinal)
                .ToList();

            if (normalized.Count == 0 || normalized.Contains(All, StringComparer.Ordinal))
            {
                return [All];
            }

            if (PlayableProfessions.All(item => normalized.Contains(item, StringComparer.Ordinal)))
            {
                return [All];
            }

            return normalized;
        }

        public static bool Allows(string? profession, IEnumerable<string>? allowedProfessions)
        {
            var normalizedAllowed = NormalizeAllowedProfessions(allowedProfessions);
            if (normalizedAllowed.Contains(All, StringComparer.Ordinal))
            {
                return true;
            }

            var normalizedProfession = Normalize(profession);
            return normalizedAllowed.Contains(normalizedProfession, StringComparer.Ordinal);
        }

        public static string GetDisplayName(string? profession)
        {
            return NormalizeLabel((profession ?? string.Empty).Trim().ToLowerInvariant());
        }

        public static string GetAllowedDisplayName(IEnumerable<string>? professions)
        {
            var normalized = NormalizeAllowedProfessions(professions);
            if (normalized.Contains(All, StringComparer.Ordinal))
            {
                return GetDisplayName(All);
            }

            return string.Join(" / ", normalized.Select(GetDisplayName));
        }

        private static string NormalizeLabel(string profession)
        {
            return profession switch
            {
                Warrior => "战",
                Mage => "法",
                Body => "体",
                All => "全职业",
                _ => "战"
            };
        }
    }
}
