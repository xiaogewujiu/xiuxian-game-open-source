using XXX.Entity;

namespace XXX.Battle
{
    /// <summary>
    /// 灵根克制关系计算器。
    /// 运行时只认数据库矩阵，缺配时直接失败。
    /// </summary>
    public static class ElementRelation
    {
        private const int ExpectedRuleCount = 64;
        private static readonly Dictionary<(Element Attacker, Element Defender), float> DatabaseMatrix = new();
        private static readonly object SyncRoot = new();
        private static volatile bool _isLoaded;

        /// <summary>
        /// 获取克制修正系数。
        /// </summary>
        public static float GetModifier(Element attacker, Element defender)
        {
            if (attacker == Element.None || defender == Element.None)
            {
                return 0;
            }

            if (!_isLoaded || DatabaseMatrix.Count != ExpectedRuleCount)
            {
                throw new InvalidOperationException(
                    "Element relation rules have not been loaded completely. Please refresh the battle-rule domain first.");
            }

            if (DatabaseMatrix.TryGetValue((attacker, defender), out var modifier))
            {
                return modifier;
            }

            throw new InvalidOperationException(
                $"Element relation rule is missing for attacker={attacker}, defender={defender}. Please repair the battle-rule matrix first.");
        }

        /// <summary>
        /// 从数据库加载元素克制矩阵。
        /// </summary>
        public static void LoadFromDatabase(IEnumerable<ElementRelationRuleEntity> entries)
        {
            lock (SyncRoot)
            {
                DatabaseMatrix.Clear();
                foreach (var entry in entries ?? [])
                {
                    if (!Enum.IsDefined(typeof(Element), entry.AttackerElement) ||
                        !Enum.IsDefined(typeof(Element), entry.DefenderElement))
                    {
                        continue;
                    }

                    var attacker = (Element)entry.AttackerElement;
                    var defender = (Element)entry.DefenderElement;
                    if (attacker == Element.None || defender == Element.None)
                    {
                        continue;
                    }

                    DatabaseMatrix[(attacker, defender)] = (float)entry.Modifier;
                }

                if (DatabaseMatrix.Count != ExpectedRuleCount)
                {
                    _isLoaded = false;
                    throw new InvalidOperationException(
                        $"Element relation rules are incomplete after loading. Expected {ExpectedRuleCount} entries but got {DatabaseMatrix.Count}.");
                }

                _isLoaded = true;
            }
        }

        /// <summary>
        /// 获取灵根名称。
        /// </summary>
        public static string GetElementName(Element element)
        {
            return element switch
            {
                Element.None => "无灵根",
                Element.Metal => "金",
                Element.Wood => "木",
                Element.Water => "水",
                Element.Fire => "火",
                Element.Earth => "土",
                Element.Wind => "风",
                Element.Ice => "冰",
                Element.Thunder => "雷",
                _ => "未知"
            };
        }
    }
}
