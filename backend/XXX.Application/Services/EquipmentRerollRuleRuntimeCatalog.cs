using XXX.Entity;

namespace XXX.Application.Services
{
    /// <summary>
    /// 装备洗练规则运行时目录。
    /// </summary>
    internal static class EquipmentRerollRuleRuntimeCatalog
    {
        private static EquipmentRerollSystemConfigEntity? _systemConfig;
        private static List<EquipmentEnhanceRuleEntity> _enhanceRules = [];
        private static List<EquipmentRerollCostRuleEntity> _rerollCostRules = [];
        private static List<EquipmentRerollSlotPoolConfigEntity> _slotPoolConfigs = [];
        private static List<EquipmentRerollTierConfigEntity> _tierConfigs = [];
        private static List<EquipmentRerollAttributeValueConfigEntity> _attributeValueConfigs = [];
        private static Dictionary<string, List<EquipmentRerollSlotPoolConfigEntity>> _slotPoolIndex = new(StringComparer.OrdinalIgnoreCase);
        private static Dictionary<string, EquipmentRerollAttributeValueConfigEntity> _attributeValueIndex = new(StringComparer.OrdinalIgnoreCase);

        public static void ReplaceConfigs(
            EquipmentRerollSystemConfigEntity? systemConfig,
            IEnumerable<EquipmentEnhanceRuleEntity> enhanceRules,
            IEnumerable<EquipmentRerollCostRuleEntity> rerollCostRules,
            IEnumerable<EquipmentRerollSlotPoolConfigEntity> slotPoolConfigs,
            IEnumerable<EquipmentRerollTierConfigEntity> tierConfigs,
            IEnumerable<EquipmentRerollAttributeValueConfigEntity> attributeValueConfigs)
        {
            _systemConfig = systemConfig;

            _enhanceRules = enhanceRules
                .Where(item => item != null && item.IsEnabled)
                .OrderBy(item => item.MinEquipmentLevel)
                .ThenBy(item => item.SortOrder)
                .ToList();

            _rerollCostRules = rerollCostRules
                .Where(item => item != null && item.IsEnabled)
                .OrderBy(item => item.MinEquipmentLevel)
                .ThenBy(item => item.SortOrder)
                .ToList();

            _slotPoolConfigs = slotPoolConfigs
                .Where(item => item != null && item.IsEnabled)
                .OrderBy(item => item.SortOrder)
                .ToList();

            _tierConfigs = tierConfigs
                .Where(item => item != null && item.IsEnabled)
                .OrderBy(item => item.SortOrder)
                .ToList();

            _attributeValueConfigs = attributeValueConfigs
                .Where(item => item != null && item.IsEnabled)
                .OrderBy(item => item.SortOrder)
                .ToList();

            _slotPoolIndex = new Dictionary<string, List<EquipmentRerollSlotPoolConfigEntity>>(StringComparer.OrdinalIgnoreCase);
            foreach (var config in _slotPoolConfigs)
            {
                var key = BuildPoolKey(config.Slot);
                if (!_slotPoolIndex.TryGetValue(key, out var list))
                {
                    list = [];
                    _slotPoolIndex[key] = list;
                }

                list.Add(config);
            }

            _attributeValueIndex = new Dictionary<string, EquipmentRerollAttributeValueConfigEntity>(StringComparer.OrdinalIgnoreCase);
            foreach (var config in _attributeValueConfigs)
            {
                var key = $"{config.AttributeType}:{config.Tier}";
                _attributeValueIndex[key] = config;
            }
        }

        public static void Clear()
        {
            _systemConfig = null;
            _enhanceRules = [];
            _rerollCostRules = [];
            _slotPoolConfigs = [];
            _tierConfigs = [];
            _attributeValueConfigs = [];
            _slotPoolIndex = new Dictionary<string, List<EquipmentRerollSlotPoolConfigEntity>>(StringComparer.OrdinalIgnoreCase);
            _attributeValueIndex = new Dictionary<string, EquipmentRerollAttributeValueConfigEntity>(StringComparer.OrdinalIgnoreCase);
        }

        public static EquipmentRerollSystemConfigEntity GetSystemConfig()
        {
            return _systemConfig
                ?? throw new InvalidOperationException("Equipment reroll system config has not been loaded. Please refresh the equipment-reroll domain first.");
        }

        public static EquipmentEnhanceRuleEntity GetEnhanceRule(int equipmentLevel)
        {
            var rule = _enhanceRules.FirstOrDefault(item =>
                equipmentLevel >= item.MinEquipmentLevel && equipmentLevel <= item.MaxEquipmentLevel);
            return rule ?? throw new InvalidOperationException($"装备等级 {equipmentLevel} 没有可用的强化规则。");
        }

        public static EquipmentRerollCostRuleEntity GetRerollCostRule(int equipmentLevel)
        {
            var rule = _rerollCostRules.FirstOrDefault(item =>
                equipmentLevel >= item.MinEquipmentLevel && equipmentLevel <= item.MaxEquipmentLevel);
            return rule ?? throw new InvalidOperationException($"装备等级 {equipmentLevel} 没有可用的洗炼消耗规则。");
        }

        public static List<EquipmentRerollSlotPoolConfigEntity> GetSlotPoolConfigs(EquipmentSlot slot)
        {
            if (_slotPoolConfigs.Count == 0)
            {
                throw new InvalidOperationException("Equipment reroll slot pool configs have not been loaded. Please refresh the equipment-reroll domain first.");
            }

            var key = BuildPoolKey(slot);
            return _slotPoolIndex.GetValueOrDefault(key) ?? [];
        }

        public static List<EquipmentRerollTierConfigEntity> GetTierConfigs()
        {
            if (_tierConfigs.Count == 0)
            {
                throw new InvalidOperationException("Equipment reroll tier configs have not been loaded. Please refresh the equipment-reroll domain first.");
            }

            return _tierConfigs;
        }

        public static EquipmentRerollAttributeValueConfigEntity? GetAttributeValueConfig(int attributeType, int tier)
        {
            var key = $"{attributeType}:{tier}";
            return _attributeValueIndex.GetValueOrDefault(key);
        }

        public static int CalculateStoneCost(EquipmentRerollCostRuleEntity rule, int lockedCount)
        {
            var safeLocked = Math.Max(0, lockedCount);
            return rule.MaterialCount + safeLocked * rule.ExtraMaterialPerLockedLine;
        }

        public static long CalculateGoldCost(EquipmentRerollCostRuleEntity rule, int lockedCount)
        {
            var safeLocked = Math.Max(0, lockedCount);
            return Math.Max(0, rule.GoldCost + (long)safeLocked * rule.ExtraGoldPerLockedLine);
        }

        private static string BuildPoolKey(EquipmentSlot slot)
        {
            return ((int)slot).ToString();
        }
    }
}
