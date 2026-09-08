using XXX.Entity;

namespace XXX.Application.Services
{
    /// <summary>
    /// 灵宠生成辅助方法。
    /// </summary>
    internal static class PetGenerationHelper
    {
        /// <summary>
        /// 根据模板生成一只新的灵宠实例。
        /// </summary>
        internal static PetInstanceEntity GeneratePetInstanceFromTemplate(string playerId, PetTemplateEntity template, DateTime now)
        {
            var qualityMin = Math.Max(1, template.InitialQualityMin);
            var qualityMax = Math.Max(qualityMin, template.InitialQualityMax);
            var growthMin = Math.Max(0.1, template.GrowthRateMin);
            var growthMax = Math.Max(growthMin, template.GrowthRateMax);
            var skillPool = NormalizeSkillPool(template.SkillIds);
            var initialSkillCount = Math.Max(0, template.InitialSkillCount);

            return new PetInstanceEntity
            {
                InstanceId = Guid.NewGuid().ToString("N"),
                PlayerId = playerId,
                TemplateId = template.TemplateId,
                Name = template.Name,
                Level = 1,
                Exp = 0,
                XExp = CalculatePetRequiredExp(1),
                Quality = Random.Shared.Next(qualityMin, qualityMax + 1),
                GrowthRate = RandomDouble(growthMin, growthMax, 2),
                Type1 = RollCoreStat(template.MinType1, template.MaxType1, 100),
                Type2 = RollCoreStat(template.MinType2, template.MaxType2, 50),
                Type3 = RollCoreStat(template.MinType3, template.MaxType3, 10),
                Type4 = RollCoreStat(template.MinType4, template.MaxType4, ResolveDefaultMagicAttack(template.Type)),
                Type5 = RollCoreStat(template.MinType5, template.MaxType5, 5),
                Type6 = RollCoreStat(template.MinType6, template.MaxType6, ResolveDefaultMagicDefense(template.Type)),
                Type7 = RollCoreStat(template.MinType7, template.MaxType7, 10),
                Type8 = RollPercentStat(template.MinType8, template.MaxType8, 90),
                Type9 = RollPercentStat(template.MinType9, template.MaxType9, 0),
                Type10 = RollPercentStat(template.MinType10, template.MaxType10, 0),
                Type11 = RollPercentStat(template.MinType11, template.MaxType11, 150),
                Type12 = RollPercentStat(template.MinType12, template.MaxType12, 0),
                Type13 = RollPercentStat(template.MinType13, template.MaxType13, 0),
                Type14 = RollPercentStat(template.MinType14, template.MaxType14, 0),
                Type15 = RollPercentStat(template.MinType15, template.MaxType15, 0),
                Element = template.Element,
                Loyalty = 100,
                IsBound = !template.IsTradable,
                SkillIds = GenerateInitialSkillIds(skillPool, initialSkillCount),
                AcquiredTime = now,
                LastUpdateTime = now
            };
        }

        internal static int CalculatePetRequiredExp(int level)
        {
            return Math.Max(100, Math.Max(1, level) * 100);
        }

        private static List<string> NormalizeSkillPool(IEnumerable<string>? skillIds)
        {
            return (skillIds ?? Enumerable.Empty<string>())
                .Where(skillId => !string.IsNullOrWhiteSpace(skillId))
                .Select(skillId => skillId.Trim())
                .Distinct(StringComparer.Ordinal)
                .ToList();
        }

        private static List<string> GenerateInitialSkillIds(List<string> skillPool, int initialSkillCount)
        {
            if (skillPool.Count == 0 || initialSkillCount <= 0)
            {
                return [];
            }

            return skillPool
                .OrderBy(_ => Random.Shared.Next())
                .Take(Math.Min(initialSkillCount, skillPool.Count))
                .ToList();
        }

        private static int RollCoreStat(int? min, int? max, int fallbackValue)
        {
            var effectiveMin = min ?? fallbackValue;
            var effectiveMax = max ?? effectiveMin;
            if (effectiveMin <= 0 && effectiveMax <= 0)
            {
                effectiveMin = Math.Max(1, fallbackValue);
                effectiveMax = effectiveMin;
            }

            if (effectiveMax < effectiveMin)
            {
                effectiveMax = effectiveMin;
            }

            return Random.Shared.Next(Math.Max(1, effectiveMin), effectiveMax + 1);
        }

        private static float RollPercentStat(int? min, int? max, int fallbackValue)
        {
            var effectiveMin = min ?? fallbackValue;
            var effectiveMax = max ?? effectiveMin;
            if (effectiveMax < effectiveMin)
            {
                effectiveMax = effectiveMin;
            }

            return Random.Shared.Next(Math.Max(0, effectiveMin), effectiveMax + 1) / 100f;
        }

        private static double RandomDouble(double min, double max, int decimals)
        {
            var effectiveMin = Math.Max(0.1, min);
            var effectiveMax = Math.Max(effectiveMin, max);
            var value = effectiveMin + (Random.Shared.NextDouble() * (effectiveMax - effectiveMin));
            return Math.Round(value, decimals, MidpointRounding.AwayFromZero);
        }

        private static int ResolveDefaultMagicAttack(PetType petType)
        {
            return petType == PetType.Support ? 8 : 5;
        }

        private static int ResolveDefaultMagicDefense(PetType petType)
        {
            return petType == PetType.Defense ? 6 : 4;
        }
    }
}
