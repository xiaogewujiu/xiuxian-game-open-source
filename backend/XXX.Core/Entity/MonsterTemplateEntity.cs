using SqlSugar;
using System.Text.Json;

namespace XXX.Entity
{
    /// <summary>
    /// 怪物模板落库实体。
    /// </summary>
    [SugarTable("MonsterTemplates")]
    public class MonsterTemplateEntity : BaseAttributesTemplate
    {
        [SugarColumn(IsPrimaryKey = true, Length = 100)]
        public string MonsterId { get; set; } = string.Empty;

        [SugarColumn(Length = 100, IsNullable = false)]
        public string Name { get; set; } = string.Empty;

        [SugarColumn(IsNullable = false)]
        public int Level { get; set; }

        [SugarColumn(IsNullable = false)]
        public int ExpRewardMin { get; set; }

        [SugarColumn(IsNullable = false)]
        public int ExpRewardMax { get; set; }

        [SugarColumn(IsNullable = false)]
        public int GoldRewardMin { get; set; }

        [SugarColumn(IsNullable = false)]
        public int GoldRewardMax { get; set; }

        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
        public string? SkillIdsJson { get; set; }

        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
        public string? PassiveIdsJson { get; set; }

        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
        public string? ItemDropsJson { get; set; }

        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
        public string? EquipmentDropsJson { get; set; }

        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
        public string? CollectionDropsJson { get; set; }

        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
        public string? ElementPoolJson { get; set; }

        [SugarColumn(Length = 200, IsNullable = true)]
        public string? SeedKey { get; set; }

        [SugarColumn(IsNullable = false, DefaultValue = "0")]
        public bool IsBuiltIn { get; set; } = false;

        [SugarColumn(Length = 100, IsNullable = true)]
        public string? BuiltInVersion { get; set; }

        [SugarColumn(IsNullable = false)]
        public DateTime LastUpdateTime { get; set; } = DateTime.Now;

        [SugarColumn(IsIgnore = true)]
        public List<string> SkillIds
        {
            get => DeserializeList<string>(SkillIdsJson);
            set => SkillIdsJson = SerializeList(value);
        }

        [SugarColumn(IsIgnore = true)]
        public List<string> PassiveIds
        {
            get => DeserializeList<string>(PassiveIdsJson);
            set => PassiveIdsJson = SerializeList(value);
        }

        [SugarColumn(IsIgnore = true)]
        public List<DropItem> ItemDrops
        {
            get => DeserializeList<DropItem>(ItemDropsJson);
            set => ItemDropsJson = SerializeList(value);
        }

        [SugarColumn(IsIgnore = true)]
        public List<DropEquipment> EquipmentDrops
        {
            get => DeserializeList<DropEquipment>(EquipmentDropsJson);
            set => EquipmentDropsJson = SerializeList(value);
        }

        [SugarColumn(IsIgnore = true)]
        public List<DropCollection> CollectionDrops
        {
            get => DeserializeList<DropCollection>(CollectionDropsJson);
            set => CollectionDropsJson = SerializeList(value);
        }

        [SugarColumn(IsIgnore = true)]
        public List<Element> ElementPool
        {
            get => DeserializeList<Element>(ElementPoolJson);
            set => ElementPoolJson = SerializeList(value);
        }

        private static List<T> DeserializeList<T>(string? json)
        {
            return string.IsNullOrWhiteSpace(json)
                ? []
                : JsonSerializer.Deserialize<List<T>>(json) ?? [];
        }

        private static string SerializeList<T>(List<T>? items)
        {
            return JsonSerializer.Serialize(items ?? []);
        }
    }
}
