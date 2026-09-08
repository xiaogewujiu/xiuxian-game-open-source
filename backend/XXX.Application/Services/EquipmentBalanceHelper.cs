using XXX.Entity;

using System.Text.Json;
using XXX.Application.DTOs;
using XXX.Equipment;

namespace XXX.Application.Services
{
    /// <summary>
    /// 装备数值辅助工具。
    /// </summary>
    /// <remarks>
    /// 负责装备模板解析、物法流派推断，以及单双攻防字段的统一读取口径。
    /// 这样装备相关服务和 DTO 映射不会各自维护一套不同的判断逻辑。
    /// </remarks>
    internal static class EquipmentBalanceHelper
    {
        /// <summary>
        /// 根据模板编号解析装备模板。
        /// </summary>
        /// <param name="templateId">模板编号字符串。</param>
        /// <returns>命中的装备模板；解析失败时返回空。</returns>
        public static EquipmentTemplate? ResolveTemplate(string? templateId)
        {
            if (!int.TryParse(templateId, out var parsedTemplateId))
            {
                return null;
            }

            return GameData.EquipmentTemplates.TryGetValue(parsedTemplateId, out var template)
                ? template
                : null;
        }

        /// <summary>
        /// 推断装备实例的战斗流派。
        /// </summary>
        /// <param name="entity">装备实例实体。</param>
        /// <param name="template">可选模板缓存。</param>
        /// <returns>物理、法术或中性流派。</returns>
        public static CombatStyle ResolveCombatStyle(EquipmentInstanceEntity entity, EquipmentTemplate? template = null)
        {
            // 中文注释：
            // 优先按模板判断物理/法术流派；
            // 如果是旧装备记录缺模板，就按实体上现有的攻类字段兜底推断。
            template ??= ResolveTemplate(entity.TemplateId);

            if (template != null)
            {
                return template.CombatStyle;
            }

            if (entity.BaseMagicAttack > 0 && entity.BasePhysicalAttack == 0)
            {
                return CombatStyle.Magic;
            }

            if (entity.BasePhysicalAttack > 0)
            {
                return CombatStyle.Physical;
            }

            return CombatStyle.Neutral;
        }

        /// <summary>
        /// 读取装备的物理攻击值。
        /// </summary>
        public static int GetPhysicalAttack(EquipmentInstanceEntity entity, EquipmentTemplate? template = null)
        {
            return entity.BasePhysicalAttack;
        }

        /// <summary>
        /// 读取装备的法术攻击值。
        /// </summary>
        public static int GetMagicAttack(EquipmentInstanceEntity entity, EquipmentTemplate? template = null)
        {
            return entity.BaseMagicAttack;
        }

        /// <summary>
        /// 读取装备的物理防御值。
        /// </summary>
        public static int GetPhysicalDefense(EquipmentInstanceEntity entity, EquipmentTemplate? template = null)
        {
            return entity.BasePhysicalDefense;
        }

        /// <summary>
        /// 读取装备的法术防御值。
        /// </summary>
        public static int GetMagicDefense(EquipmentInstanceEntity entity, EquipmentTemplate? template = null)
        {
            return entity.BaseMagicDefense;
        }

        /// <summary>
        /// 根据模板生成可落库的装备实例实体。
        /// </summary>
        /// <param name="playerId">所属玩家编号。</param>
        /// <param name="template">装备模板。</param>
        /// <param name="isLocked">是否锁定。</param>
        /// <returns>带随机基础数值的装备实例。</returns>
        public static EquipmentInstanceEntity CreateEntity(string playerId, EquipmentTemplate template, bool isLocked)
        {
            // 中文注释：
            // 生成装备实例时，会把模板区间随机成真实基础属性。
            // 后续强化、洗练都作用在这个实例上，而不是每次展示时重新抽值。
            var entity = new EquipmentInstanceEntity
            {
                InstanceId = Guid.NewGuid().ToString("N"),
                PlayerId = playerId,
                TemplateId = template.EquipmentId.ToString(),
                Name = template.Name,
                Slot = template.Slot,
                Quality = (int)template.Quality,
                EnhanceLevel = 0,
                IsEquipped = false,
                IsLocked = isLocked,
                AcquiredTime = DateTime.Now,
                LastUpdateTime = DateTime.Now
            };

            PopulateEntityAttributes(
                entity,
                RandomRange(template.MinType1, template.MaxType1),
                RandomRange(template.MinType2, template.MaxType2),
                RandomRange(template.MinType3, template.MaxType3),
                RandomRange(template.MinType4, template.MaxType4),
                RandomRange(template.MinType5, template.MaxType5),
                RandomRange(template.MinType6, template.MaxType6),
                RandomRange(template.MinType7, template.MaxType7),
                PercentFromIntRange(template.MinType8, template.MaxType8),
                PercentFromIntRange(template.MinType9, template.MaxType9),
                PercentFromIntRange(template.MinType10, template.MaxType10),
                PercentFromIntRange(template.MinType11, template.MaxType11),
                PercentFromIntRange(template.MinType12, template.MaxType12),
                PercentFromIntRange(template.MinType13, template.MaxType13),
                PercentFromIntRange(template.MinType14, template.MaxType14),
                PercentFromIntRange(template.MinType15, template.MaxType15));

            return entity;
        }

        /// <summary>
        /// 根据已有装备实例生成可落库的装备实例实体。
        /// </summary>
        public static EquipmentInstanceEntity CreateEntity(string playerId, EquipmentInstance equipment, bool isLocked)
        {
            var template = equipment.Template ?? throw new InvalidOperationException("装备实例缺少模板信息。");
            var entity = new EquipmentInstanceEntity
            {
                InstanceId = string.IsNullOrWhiteSpace(equipment.InstanceId) ? Guid.NewGuid().ToString("N") : equipment.InstanceId,
                PlayerId = playerId,
                TemplateId = template.EquipmentId.ToString(),
                Name = string.IsNullOrWhiteSpace(equipment.Name) ? template.Name : equipment.Name,
                Slot = template.Slot,
                Quality = (int)(template.Quality == 0 ? equipment.Quality : template.Quality),
                EnhanceLevel = equipment.EnhanceLevel,
                IsEquipped = false,
                IsLocked = isLocked,
                AcquiredTime = DateTime.Now,
                LastUpdateTime = DateTime.Now
            };

            PopulateEntityAttributes(
                entity,
                equipment.Type1,
                equipment.Type2,
                equipment.Type3,
                equipment.Type4,
                equipment.Type5,
                equipment.Type6,
                equipment.Type7,
                equipment.Type8,
                equipment.Type9,
                equipment.Type10,
                equipment.Type11,
                equipment.Type12,
                equipment.Type13,
                equipment.Type14,
                equipment.Type15);

            return entity;
        }

        /// <summary>
        /// 在模板给定区间内生成随机数值。
        /// </summary>
        /// <param name="min">最小值。</param>
        /// <param name="max">最大值。</param>
        /// <returns>经过空值与上下限修正后的随机结果。</returns>
        public static int RandomRange(int? min, int? max)
        {
            // 中文注释：
            // 统一处理模板空值和上下限颠倒的坏配置，避免实例生成阶段直接抛异常。
            var minValue = min ?? 0;
            var maxValue = max ?? minValue;
            if (maxValue < minValue)
            {
                maxValue = minValue;
            }

            return Random.Shared.Next(minValue, maxValue + 1);
        }

        private static float PercentFromIntRange(int? min, int? max)
        {
            return RandomRange(min, max) / 100f;
        }

        private static void PopulateEntityAttributes(
            EquipmentInstanceEntity entity,
            int hp,
            int mp,
            int physicalAttack,
            int magicAttack,
            int physicalDefense,
            int magicDefense,
            int speed,
            float hitRate,
            float dodgeRate,
            float critRate,
            float critDamage,
            float comboRate,
            float counterRate,
            float armorBreak,
            float extraDamage)
        {
            entity.BaseHP = hp;
            entity.BaseMP = mp;
            entity.BasePhysicalAttack = physicalAttack;
            entity.BaseMagicAttack = magicAttack;
            entity.BasePhysicalDefense = physicalDefense;
            entity.BaseMagicDefense = magicDefense;
            entity.BonusStatsJson = SerializeBonusStats(speed, hitRate, dodgeRate, critRate, critDamage, comboRate, counterRate, armorBreak, extraDamage);
        }

        private static string? SerializeBonusStats(
            int speed,
            float hitRate,
            float dodgeRate,
            float critRate,
            float critDamage,
            float comboRate,
            float counterRate,
            float armorBreak,
            float extraDamage)
        {
            var bonusStats = new List<EquipmentBonusDto>();

            AppendBonusStat(bonusStats, AttributeType.Type7, speed, false);
            AppendBonusStat(bonusStats, AttributeType.Type8, hitRate, true);
            AppendBonusStat(bonusStats, AttributeType.Type9, dodgeRate, true);
            AppendBonusStat(bonusStats, AttributeType.Type10, critRate, true);
            AppendBonusStat(bonusStats, AttributeType.Type11, critDamage, true);
            AppendBonusStat(bonusStats, AttributeType.Type12, comboRate, true);
            AppendBonusStat(bonusStats, AttributeType.Type13, counterRate, true);
            AppendBonusStat(bonusStats, AttributeType.Type14, armorBreak, true);
            AppendBonusStat(bonusStats, AttributeType.Type15, extraDamage, true);

            return bonusStats.Count == 0 ? null : JsonSerializer.Serialize(bonusStats);
        }

        private static void AppendBonusStat(List<EquipmentBonusDto> bonusStats, AttributeType attributeType, double rawValue, bool isPercentage)
        {
            if (Math.Abs(rawValue) < 0.0001d)
            {
                return;
            }

            var displayValue = isPercentage
                ? (int)Math.Round(rawValue * 100d, MidpointRounding.AwayFromZero)
                : (int)Math.Round(rawValue, MidpointRounding.AwayFromZero);

            if (displayValue == 0)
            {
                return;
            }

            bonusStats.Add(new EquipmentBonusDto
            {
                StatType = attributeType.ToString(),
                Value = displayValue,
                RawValue = rawValue,
                IsPercentage = isPercentage,
                Description = RerollSystem.GetAttributeName(attributeType)
            });
        }
    }
}
