using AutoMapper;
using XXX.Application.DTOs;
using XXX.Battle;
using XXX.Entity;

namespace XXX.Application.Mappings
{
    /// <summary>
    /// AutoMapper映射配置
    /// </summary>
    public class MappingProfile : Profile
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public MappingProfile()
        {
            // 玩家映射
            CreateMap<UserEntity, PlayerDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.GID))
                .ForMember(dest => dest.CurrentTitle, opt => opt.MapFrom(src => src.CurrentTitle))
                .ForMember(dest => dest.Profession, opt => opt.MapFrom(src => PlayerProfessionCatalog.Normalize(src.Profession, PlayerProfessionCatalog.Warrior)))
                .ForMember(dest => dest.ProfessionName, opt => opt.MapFrom(src => PlayerProfessionCatalog.GetDisplayName(src.Profession)))
                .ForMember(dest => dest.HP, opt => opt.MapFrom(src => src.Type1))  // MaxHP
                .ForMember(dest => dest.MaxHP, opt => opt.MapFrom(src => src.Type1))
                .ForMember(dest => dest.MP, opt => opt.MapFrom(src => src.Type2))  // MaxMP
                .ForMember(dest => dest.MaxMP, opt => opt.MapFrom(src => src.Type2))
                .ForMember(dest => dest.Attack, opt => opt.MapFrom(src => src.Type3))
                .ForMember(dest => dest.MagicAttack, opt => opt.MapFrom(src => src.Type4))
                .ForMember(dest => dest.Defense, opt => opt.MapFrom(src => src.Type5))
                .ForMember(dest => dest.MagicDefense, opt => opt.MapFrom(src => src.Type6))
                .ForMember(dest => dest.Speed, opt => opt.MapFrom(src => src.Type7))
                .ForMember(dest => dest.HitRate, opt => opt.MapFrom(src => src.Type8))
                .ForMember(dest => dest.DodgeRate, opt => opt.MapFrom(src => src.Type9))
                .ForMember(dest => dest.CritRate, opt => opt.MapFrom(src => src.Type10))
                .ForMember(dest => dest.CritDamage, opt => opt.MapFrom(src => src.Type11))
                .ForMember(dest => dest.ComboRate, opt => opt.MapFrom(src => src.Type12))
                .ForMember(dest => dest.CounterRate, opt => opt.MapFrom(src => src.Type13))
                .ForMember(dest => dest.ArmorBreak, opt => opt.MapFrom(src => src.Type14))
                .ForMember(dest => dest.BonusDamage, opt => opt.MapFrom(src => src.Type15))
                .ForMember(dest => dest.SpiritRoot, opt => opt.MapFrom(src => BuildSpiritRootDto(src.Element)));

            CreateMap<UserEntity, PlayerDetailDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.GID))
                .ForMember(dest => dest.CurrentTitle, opt => opt.MapFrom(src => src.CurrentTitle))
                .ForMember(dest => dest.Profession, opt => opt.MapFrom(src => PlayerProfessionCatalog.Normalize(src.Profession, PlayerProfessionCatalog.Warrior)))
                .ForMember(dest => dest.ProfessionName, opt => opt.MapFrom(src => PlayerProfessionCatalog.GetDisplayName(src.Profession)))
                .ForMember(dest => dest.HP, opt => opt.MapFrom(src => src.Type1))
                .ForMember(dest => dest.MaxHP, opt => opt.MapFrom(src => src.Type1))
                .ForMember(dest => dest.MP, opt => opt.MapFrom(src => src.Type2))
                .ForMember(dest => dest.MaxMP, opt => opt.MapFrom(src => src.Type2))
                .ForMember(dest => dest.Attack, opt => opt.MapFrom(src => src.Type3))
                .ForMember(dest => dest.MagicAttack, opt => opt.MapFrom(src => src.Type4))
                .ForMember(dest => dest.Defense, opt => opt.MapFrom(src => src.Type5))
                .ForMember(dest => dest.MagicDefense, opt => opt.MapFrom(src => src.Type6))
                .ForMember(dest => dest.Speed, opt => opt.MapFrom(src => src.Type7))
                .ForMember(dest => dest.HitRate, opt => opt.MapFrom(src => src.Type8))
                .ForMember(dest => dest.DodgeRate, opt => opt.MapFrom(src => src.Type9))
                .ForMember(dest => dest.CritRate, opt => opt.MapFrom(src => src.Type10))
                .ForMember(dest => dest.CritDamage, opt => opt.MapFrom(src => src.Type11))
                .ForMember(dest => dest.ComboRate, opt => opt.MapFrom(src => src.Type12))
                .ForMember(dest => dest.CounterRate, opt => opt.MapFrom(src => src.Type13))
                .ForMember(dest => dest.ArmorBreak, opt => opt.MapFrom(src => src.Type14))
                .ForMember(dest => dest.BonusDamage, opt => opt.MapFrom(src => src.Type15))
                .ForMember(dest => dest.SpiritRoot, opt => opt.MapFrom(src => BuildSpiritRootDto(src.Element)))
                .ForMember(dest => dest.BattleStats, opt => opt.MapFrom(src => new PlayerBattleStatsDto
                {
                    TotalBattles = src.TotalBattles,
                    WinBattles = src.WinBattles,
                    TotalKills = src.TotalKills,
                    MaxCombo = src.MaxCombo
                }))
                .ForMember(dest => dest.CurrencyStats, opt => opt.MapFrom(src => new PlayerCurrencyStatsDto
                {
                    TotalGoldEarned = src.TotalGoldEarned,
                    TotalGoldSpent = src.TotalGoldSpent,
                    TotalExpEarned = src.TotalExpEarned
                }));
        }

        /// <summary>
        /// 中文注释：
        /// 把底层 Element 枚举统一转换成前端可直接展示的灵根结构。
        /// 这里集中做有两个好处：
        /// 1. 前端不会再各自维护一份名称、图标、描述和克制表。
        /// 2. 如果后续灵根平衡调整，只改这一处即可。
        /// </summary>
        private static PlayerSpiritRootDto BuildSpiritRootDto(Element element)
        {
            var normalizedElement = element == Element.None ? Element.Metal : element;

            var counters = Enum.GetValues<Element>()
                .Where(target => target != normalizedElement && target != Element.None)
                .Select(target => new
                {
                    Target = target,
                    Modifier = ElementRelation.GetModifier(normalizedElement, target)
                })
                .Select(item => new PlayerSpiritRootCounterDto
                {
                    Type = GetElementTypeKey(item.Target),
                    Name = GetElementDisplayName(item.Target),
                    Icon = GetElementIcon(item.Target),
                    Value = (int)Math.Round(item.Modifier * 100, MidpointRounding.AwayFromZero)
                })
                .ToList();

            return new PlayerSpiritRootDto
            {
                Type = GetElementTypeKey(normalizedElement),
                Name = GetElementDisplayName(normalizedElement),
                Icon = GetElementIcon(normalizedElement),
                Description = GetElementDescription(normalizedElement),
                Counters = counters
            };
        }

        /// <summary>
        /// 中文注释：
        /// 前端样式类名和接口字段都使用这个英文键，避免直接暴露枚举整数导致耦合。
        /// </summary>
        private static string GetElementTypeKey(Element element)
        {
            return element switch
            {
                Element.Metal => "metal",
                Element.Wood => "wood",
                Element.Water => "water",
                Element.Fire => "fire",
                Element.Earth => "earth",
                Element.Wind => "wind",
                Element.Ice => "ice",
                Element.Thunder => "thunder",
                _ => "none"
            };
        }

        private static string GetElementDisplayName(Element element)
        {
            return element switch
            {
                Element.Metal => "金",
                Element.Wood => "木",
                Element.Water => "水",
                Element.Fire => "火",
                Element.Earth => "土",
                Element.Wind => "风",
                Element.Ice => "冰",
                Element.Thunder => "雷",
                _ => "无"
            };
        }

        private static string GetElementIcon(Element element)
        {
            return element switch
            {
                Element.Metal => "⚔️",
                Element.Wood => "🌿",
                Element.Water => "💧",
                Element.Fire => "🔥",
                Element.Earth => "🏔️",
                Element.Wind => "🌪️",
                Element.Ice => "❄️",
                Element.Thunder => "⚡",
                _ => "○"
            };
        }

        private static string GetElementDescription(Element element)
        {
            return element switch
            {
                Element.Metal => "金灵根锋锐凌厉，擅长破敌先机。",
                Element.Wood => "木灵根生机绵长，偏向持续与回复。",
                Element.Water => "水灵根灵动沉稳，擅长周旋与控制。",
                Element.Fire => "火灵根爆发猛烈，适合强攻速决。",
                Element.Earth => "土灵根厚重坚韧，偏向稳守与承伤。",
                Element.Wind => "风灵根来去迅疾，擅长速度与机动。",
                Element.Ice => "冰灵根寒意逼人，擅长压制与减速。",
                Element.Thunder => "雷灵根威势迅猛，兼具爆发与穿透。",
                _ => "尚未觉醒灵根。"
            };
        }
    }
}
