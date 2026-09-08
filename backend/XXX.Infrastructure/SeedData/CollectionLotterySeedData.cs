using SqlSugar;
using XXX.Entity;

namespace XXX.Infrastructure.SeedData
{
    /// <summary>
    /// 图鉴和抽奖系统种子数据
    /// </summary>
    public static class CollectionLotterySeedData
    {
        public const string Version = "collection-lottery-v1-20260608";

        #region 文字图鉴系列

        public static List<TextCollectionSeriesEntity> BuildTextCollectionSeries(DateTime now)
        {
            return new List<TextCollectionSeriesEntity>
            {
                new()
                {
                    SeriesId = "text_五行",
                    Name = "五行真言",
                    Description = "收集金木水火土五个真言字符，领悟五行之力",
                    Icon = "🔥",
                    SortOrder = 1,
                    IsEnabled = true,
                    SeedKey = "text_五行",
                    IsBuiltIn = true,
                    BuiltInVersion = Version,
                    LastUpdateTime = now
                },
                new()
                {
                    SeriesId = "text_八卦",
                    Name = "八卦密文",
                    Description = "收集乾坤震巽坎离艮兑八个卦象字符",
                    Icon = "☯️",
                    SortOrder = 2,
                    IsEnabled = true,
                    SeedKey = "text_八卦",
                    IsBuiltIn = true,
                    BuiltInVersion = Version,
                    LastUpdateTime = now
                },
                new()
                {
                    SeriesId = "text_天干",
                    Name = "天干秘录",
                    Description = "收集甲乙丙丁戊己庚辛壬癸十个天干字符",
                    Icon = "📜",
                    SortOrder = 3,
                    IsEnabled = true,
                    SeedKey = "text_天干",
                    IsBuiltIn = true,
                    BuiltInVersion = Version,
                    LastUpdateTime = now
                },
                new()
                {
                    SeriesId = "text_地支",
                    Name = "地支残卷",
                    Description = "收集子丑寅卯辰巳午未申酉戌亥十二地支字符",
                    Icon = "📿",
                    SortOrder = 4,
                    IsEnabled = true,
                    SeedKey = "text_地支",
                    IsBuiltIn = true,
                    BuiltInVersion = Version,
                    LastUpdateTime = now
                },
                new()
                {
                    SeriesId = "text_仙道",
                    Name = "仙道箴言",
                    Description = "收集修仙真言，感悟大道至理",
                    Icon = "✨",
                    SortOrder = 5,
                    IsEnabled = true,
                    SeedKey = "text_仙道",
                    IsBuiltIn = true,
                    BuiltInVersion = Version,
                    LastUpdateTime = now
                }
            };
        }

        public static List<TextCollectionItemEntity> BuildTextCollectionItems(DateTime now)
        {
            var items = new List<TextCollectionItemEntity>();

            // 五行真言
            foreach (var (ch, idx) in new[] { ("金", 0), ("木", 1), ("水", 2), ("火", 3), ("土", 4) })
            {
                items.Add(new TextCollectionItemEntity
                {
                    ItemId = $"text_五行_{ch}",
                    SeriesId = "text_五行",
                    Character = ch,
                    SlotIndex = idx,
                    SortOrder = idx,
                    IsEnabled = true,
                    SeedKey = $"text_五行_{ch}",
                    IsBuiltIn = true,
                    BuiltInVersion = Version,
                    LastUpdateTime = now
                });
            }

            // 八卦密文
            foreach (var (ch, idx) in new[] { ("乾", 0), ("坤", 1), ("震", 2), ("巽", 3), ("坎", 4), ("离", 5), ("艮", 6), ("兑", 7) })
            {
                items.Add(new TextCollectionItemEntity
                {
                    ItemId = $"text_八卦_{ch}",
                    SeriesId = "text_八卦",
                    Character = ch,
                    SlotIndex = idx,
                    SortOrder = idx,
                    IsEnabled = true,
                    SeedKey = $"text_八卦_{ch}",
                    IsBuiltIn = true,
                    BuiltInVersion = Version,
                    LastUpdateTime = now
                });
            }

            // 天干秘录
            foreach (var (ch, idx) in new[] { ("甲", 0), ("乙", 1), ("丙", 2), ("丁", 3), ("戊", 4), ("己", 5), ("庚", 6), ("辛", 7), ("壬", 8), ("癸", 9) })
            {
                items.Add(new TextCollectionItemEntity
                {
                    ItemId = $"text_天干_{ch}",
                    SeriesId = "text_天干",
                    Character = ch,
                    SlotIndex = idx,
                    SortOrder = idx,
                    IsEnabled = true,
                    SeedKey = $"text_天干_{ch}",
                    IsBuiltIn = true,
                    BuiltInVersion = Version,
                    LastUpdateTime = now
                });
            }

            // 地支残卷
            foreach (var (ch, idx) in new[] { ("子", 0), ("丑", 1), ("寅", 2), ("卯", 3), ("辰", 4), ("巳", 5), ("午", 6), ("未", 7), ("申", 8), ("酉", 9), ("戌", 10), ("亥", 11) })
            {
                items.Add(new TextCollectionItemEntity
                {
                    ItemId = $"text_地支_{ch}",
                    SeriesId = "text_地支",
                    Character = ch,
                    SlotIndex = idx,
                    SortOrder = idx,
                    IsEnabled = true,
                    SeedKey = $"text_地支_{ch}",
                    IsBuiltIn = true,
                    BuiltInVersion = Version,
                    LastUpdateTime = now
                });
            }

            // 仙道箴言
            foreach (var (ch, idx) in new[] { ("道", 0), ("法", 1), ("天", 2), ("地", 3), ("人", 4) })
            {
                items.Add(new TextCollectionItemEntity
                {
                    ItemId = $"text_仙道_{ch}",
                    SeriesId = "text_仙道",
                    Character = ch,
                    SlotIndex = idx,
                    SortOrder = idx,
                    IsEnabled = true,
                    SeedKey = $"text_仙道_{ch}",
                    IsBuiltIn = true,
                    BuiltInVersion = Version,
                    LastUpdateTime = now
                });
            }

            return items;
        }

        public static List<TextCollectionBonusEntity> BuildTextCollectionBonuses(DateTime now)
        {
            return new List<TextCollectionBonusEntity>
            {
                // 五行真言 - 物理攻击加成
                new()
                {
                    BonusId = "text_五行_bonus_1",
                    SeriesId = "text_五行",
                    AttrType = "Type3",
                    AttrValue = 50,
                    ValueType = 0,
                    SortOrder = 1,
                    SeedKey = "text_五行_bonus_1",
                    IsBuiltIn = true,
                    BuiltInVersion = Version,
                    LastUpdateTime = now
                },
                // 八卦密文 - 法术攻击加成
                new()
                {
                    BonusId = "text_八卦_bonus_1",
                    SeriesId = "text_八卦",
                    AttrType = "Type4",
                    AttrValue = 80,
                    ValueType = 0,
                    SortOrder = 1,
                    SeedKey = "text_八卦_bonus_1",
                    IsBuiltIn = true,
                    BuiltInVersion = Version,
                    LastUpdateTime = now
                },
                // 天干秘录 - 暴击率加成
                new()
                {
                    BonusId = "text_天干_bonus_1",
                    SeriesId = "text_天干",
                    AttrType = "Type10",
                    AttrValue = 0.05f,
                    ValueType = 0,
                    SortOrder = 1,
                    SeedKey = "text_天干_bonus_1",
                    IsBuiltIn = true,
                    BuiltInVersion = Version,
                    LastUpdateTime = now
                },
                // 地支残卷 - 最大血量加成
                new()
                {
                    BonusId = "text_地支_bonus_1",
                    SeriesId = "text_地支",
                    AttrType = "Type1",
                    AttrValue = 500,
                    ValueType = 0,
                    SortOrder = 1,
                    SeedKey = "text_地支_bonus_1",
                    IsBuiltIn = true,
                    BuiltInVersion = Version,
                    LastUpdateTime = now
                },
                // 仙道箴言 - 速度加成
                new()
                {
                    BonusId = "text_仙道_bonus_1",
                    SeriesId = "text_仙道",
                    AttrType = "Type7",
                    AttrValue = 30,
                    ValueType = 0,
                    SortOrder = 1,
                    SeedKey = "text_仙道_bonus_1",
                    IsBuiltIn = true,
                    BuiltInVersion = Version,
                    LastUpdateTime = now
                }
            };
        }

        #endregion

        #region 图片图鉴系列

        public static List<ImageCollectionSeriesEntity> BuildImageCollectionSeries(DateTime now)
        {
            return new List<ImageCollectionSeriesEntity>
            {
                new()
                {
                    SeriesId = "img_灵兽",
                    Name = "灵兽图鉴",
                    Description = "收集各种灵兽图鉴，了解它们的习性与能力",
                    Icon = "🐉",
                    SortOrder = 1,
                    IsEnabled = true,
                    SeedKey = "img_灵兽",
                    IsBuiltIn = true,
                    BuiltInVersion = Version,
                    LastUpdateTime = now
                },
                new()
                {
                    SeriesId = "img_法宝",
                    Name = "法宝图鉴",
                    Description = "收集上古法宝图鉴，参悟神器奥秘",
                    Icon = "⚔️",
                    SortOrder = 2,
                    IsEnabled = true,
                    SeedKey = "img_法宝",
                    IsBuiltIn = true,
                    BuiltInVersion = Version,
                    LastUpdateTime = now
                },
                new()
                {
                    SeriesId = "img_仙境",
                    Name = "仙境图鉴",
                    Description = "收集天下仙境图鉴，领略修仙世界的壮丽",
                    Icon = "🏔️",
                    SortOrder = 3,
                    IsEnabled = true,
                    SeedKey = "img_仙境",
                    IsBuiltIn = true,
                    BuiltInVersion = Version,
                    LastUpdateTime = now
                }
            };
        }

        public static List<ImageCollectionItemEntity> BuildImageCollectionItems(DateTime now)
        {
            var items = new List<ImageCollectionItemEntity>();

            // 灵兽图鉴
            var beasts = new[] { ("青龙", "qinglong"), ("白虎", "baihu"), ("朱雀", "zhuque"), ("玄武", "xuanwu"), ("麒麟", "qilin") };
            foreach (var (name, key) in beasts)
            {
                items.Add(new ImageCollectionItemEntity
                {
                    ItemId = $"img_灵兽_{key}",
                    SeriesId = "img_灵兽",
                    ImageName = name,
                    ThumbUrl = $"/images/collection/beasts/{key}_thumb.png",
                    OriginalUrl = $"/images/collection/beasts/{key}.png",
                    SortOrder = items.Count,
                    IsEnabled = true,
                    SeedKey = $"img_灵兽_{key}",
                    IsBuiltIn = true,
                    BuiltInVersion = Version,
                    LastUpdateTime = now
                });
            }

            // 法宝图鉴
            var weapons = new[] { ("轩辕剑", "xuanyuan"), ("东皇钟", "donghuang"), ("炼妖炉", "lianyao"), ("昆仑镜", "kunlun"), ("女娲石", "nvwa") };
            foreach (var (name, key) in weapons)
            {
                items.Add(new ImageCollectionItemEntity
                {
                    ItemId = $"img_法宝_{key}",
                    SeriesId = "img_法宝",
                    ImageName = name,
                    ThumbUrl = $"/images/collection/weapons/{key}_thumb.png",
                    OriginalUrl = $"/images/collection/weapons/{key}.png",
                    SortOrder = items.Count,
                    IsEnabled = true,
                    SeedKey = $"img_法宝_{key}",
                    IsBuiltIn = true,
                    BuiltInVersion = Version,
                    LastUpdateTime = now
                });
            }

            // 仙境图鉴
            var realms = new[] { ("蓬莱仙岛", "penglai"), ("昆仑仙境", "kunlun"), ("瑶池圣境", "yaochi"), ("方寸灵山", "fangcun"), ("天墉城", "tianyong") };
            foreach (var (name, key) in realms)
            {
                items.Add(new ImageCollectionItemEntity
                {
                    ItemId = $"img_仙境_{key}",
                    SeriesId = "img_仙境",
                    ImageName = name,
                    ThumbUrl = $"/images/collection/realms/{key}_thumb.png",
                    OriginalUrl = $"/images/collection/realms/{key}.png",
                    SortOrder = items.Count,
                    IsEnabled = true,
                    SeedKey = $"img_仙境_{key}",
                    IsBuiltIn = true,
                    BuiltInVersion = Version,
                    LastUpdateTime = now
                });
            }

            return items;
        }

        public static List<ImageCollectionBonusEntity> BuildImageCollectionBonuses(DateTime now)
        {
            return new List<ImageCollectionBonusEntity>
            {
                // 灵兽图鉴 - 生命值百分比加成
                new()
                {
                    BonusId = "img_灵兽_bonus_1",
                    SeriesId = "img_灵兽",
                    AttrType = "Type1",
                    AttrValue = 0.1f,
                    ValueType = 1,
                    SortOrder = 1,
                    SeedKey = "img_灵兽_bonus_1",
                    IsBuiltIn = true,
                    BuiltInVersion = Version,
                    LastUpdateTime = now
                },
                // 法宝图鉴 - 攻击百分比加成
                new()
                {
                    BonusId = "img_法宝_bonus_1",
                    SeriesId = "img_法宝",
                    AttrType = "Type3",
                    AttrValue = 0.15f,
                    ValueType = 1,
                    SortOrder = 1,
                    SeedKey = "img_法宝_bonus_1",
                    IsBuiltIn = true,
                    BuiltInVersion = Version,
                    LastUpdateTime = now
                },
                // 仙境图鉴 - 防御百分比加成
                new()
                {
                    BonusId = "img_仙境_bonus_1",
                    SeriesId = "img_仙境",
                    AttrType = "Type5",
                    AttrValue = 0.12f,
                    ValueType = 1,
                    SortOrder = 1,
                    SeedKey = "img_仙境_bonus_1",
                    IsBuiltIn = true,
                    BuiltInVersion = Version,
                    LastUpdateTime = now
                }
            };
        }

        #endregion

        #region 抽奖池和奖项

        public static List<LotteryPoolEntity> BuildLotteryPools(DateTime now)
        {
            return new List<LotteryPoolEntity>
            {
                // 文字图鉴抽奖池
                new()
                {
                    PoolId = "pool_text_collection",
                    Name = "真言抽奖",
                    LotteryType = 0,
                    CostType = 1, // 灵石
                    CostAmount = 100,
                    IsEnabled = true,
                    SupportSingle = true,
                    SupportTen = true,
                    DailyLimit = -1,
                    TotalLimit = -1,
                    SortOrder = 1,
                    SeedKey = "pool_text_collection",
                    IsBuiltIn = true,
                    BuiltInVersion = Version,
                    LastUpdateTime = now
                },
                // 图片图鉴抽奖池
                new()
                {
                    PoolId = "pool_image_collection",
                    Name = "灵图抽奖",
                    LotteryType = 1,
                    CostType = 1, // 灵石
                    CostAmount = 200,
                    IsEnabled = true,
                    SupportSingle = true,
                    SupportTen = true,
                    DailyLimit = -1,
                    TotalLimit = -1,
                    SortOrder = 2,
                    SeedKey = "pool_image_collection",
                    IsBuiltIn = true,
                    BuiltInVersion = Version,
                    LastUpdateTime = now
                },
                // 幸运抽奖池
                new()
                {
                    PoolId = "pool_lucky",
                    Name = "幸运转盘",
                    LotteryType = 2,
                    CostType = 1, // 灵石
                    CostAmount = 50,
                    IsEnabled = true,
                    SupportSingle = true,
                    SupportTen = true,
                    DailyLimit = 50,
                    TotalLimit = -1,
                    SortOrder = 3,
                    SeedKey = "pool_lucky",
                    IsBuiltIn = true,
                    BuiltInVersion = Version,
                    LastUpdateTime = now
                }
            };
        }

        public static List<LotteryPrizeEntity> BuildLotteryPrizes(DateTime now)
        {
            var prizes = new List<LotteryPrizeEntity>();

            // ===== 文字图鉴抽奖池奖项 =====
            // 每个文字系列的字符，概率均分
            var textSeries = new[] { "text_五行", "text_八卦", "text_天干", "text_地支", "text_仙道" };
            var textItemCount = new Dictionary<string, int>
            {
                ["text_五行"] = 5,
                ["text_八卦"] = 8,
                ["text_天干"] = 10,
                ["text_地支"] = 12,
                ["text_仙道"] = 5
            };

            int prizeIdx = 0;
            foreach (var seriesId in textSeries)
            {
                prizes.Add(new LotteryPrizeEntity
                {
                    PrizeId = $"prize_text_{seriesId}",
                    PoolId = "pool_text_collection",
                    RewardType = 3, // 文字图鉴
                    RewardTargetId = seriesId,
                    RewardAmount = 1,
                    Probability = 1500, // 15%
                    SortOrder = prizeIdx++,
                    IsEnabled = true,
                    SeedKey = $"prize_text_{seriesId}",
                    IsBuiltIn = true,
                    BuiltInVersion = Version,
                    LastUpdateTime = now
                });
            }

            // 金币奖励
            prizes.Add(new LotteryPrizeEntity
            {
                PrizeId = "prize_text_gold",
                PoolId = "pool_text_collection",
                RewardType = 0, // 金币
                RewardAmount = 500,
                Probability = 1500, // 15%
                SortOrder = prizeIdx++,
                IsEnabled = true,
                SeedKey = "prize_text_gold",
                IsBuiltIn = true,
                BuiltInVersion = Version,
                LastUpdateTime = now
            });

            // 灵石奖励
            prizes.Add(new LotteryPrizeEntity
            {
                PrizeId = "prize_text_spirit",
                PoolId = "pool_text_collection",
                RewardType = 1, // 灵石
                RewardAmount = 20,
                Probability = 500, // 5%
                SortOrder = prizeIdx++,
                IsEnabled = true,
                SeedKey = "prize_text_spirit",
                IsBuiltIn = true,
                BuiltInVersion = Version,
                LastUpdateTime = now
            });

            // 谢谢惠顾
            prizes.Add(new LotteryPrizeEntity
            {
                PrizeId = "prize_text_thanks",
                PoolId = "pool_text_collection",
                RewardType = 5, // 谢谢惠顾
                RewardAmount = 1,
                Probability = 2500, // 25%
                SortOrder = prizeIdx++,
                IsEnabled = true,
                SeedKey = "prize_text_thanks",
                IsBuiltIn = true,
                BuiltInVersion = Version,
                LastUpdateTime = now
            });

            // ===== 图片图鉴抽奖池奖项 =====
            var imgSeries = new[] { "img_灵兽", "img_法宝", "img_仙境" };
            prizeIdx = 0;

            foreach (var seriesId in imgSeries)
            {
                prizes.Add(new LotteryPrizeEntity
                {
                    PrizeId = $"prize_img_{seriesId}",
                    PoolId = "pool_image_collection",
                    RewardType = 4, // 图片图鉴
                    RewardTargetId = seriesId,
                    RewardAmount = 1,
                    Probability = 2000, // 20%
                    SortOrder = prizeIdx++,
                    IsEnabled = true,
                    SeedKey = $"prize_img_{seriesId}",
                    IsBuiltIn = true,
                    BuiltInVersion = Version,
                    LastUpdateTime = now
                });
            }

            // 金币奖励
            prizes.Add(new LotteryPrizeEntity
            {
                PrizeId = "prize_img_gold",
                PoolId = "pool_image_collection",
                RewardType = 0,
                RewardAmount = 1000,
                Probability = 1500, // 15%
                SortOrder = prizeIdx++,
                IsEnabled = true,
                SeedKey = "prize_img_gold",
                IsBuiltIn = true,
                BuiltInVersion = Version,
                LastUpdateTime = now
            });

            // 灵石奖励
            prizes.Add(new LotteryPrizeEntity
            {
                PrizeId = "prize_img_spirit",
                PoolId = "pool_image_collection",
                RewardType = 1,
                RewardAmount = 50,
                Probability = 1000, // 10%
                SortOrder = prizeIdx++,
                IsEnabled = true,
                SeedKey = "prize_img_spirit",
                IsBuiltIn = true,
                BuiltInVersion = Version,
                LastUpdateTime = now
            });

            // 谢谢惠顾
            prizes.Add(new LotteryPrizeEntity
            {
                PrizeId = "prize_img_thanks",
                PoolId = "pool_image_collection",
                RewardType = 5,
                RewardAmount = 1,
                Probability = 1500, // 15%
                SortOrder = prizeIdx++,
                IsEnabled = true,
                SeedKey = "prize_img_thanks",
                IsBuiltIn = true,
                BuiltInVersion = Version,
                LastUpdateTime = now
            });

            // ===== 幸运抽奖池奖项 =====
            prizeIdx = 0;

            // 大奖 - 灵石
            prizes.Add(new LotteryPrizeEntity
            {
                PrizeId = "prize_lucky_jackpot",
                PoolId = "pool_lucky",
                RewardType = 1,
                RewardAmount = 500,
                Probability = 100, // 1%
                SortOrder = prizeIdx++,
                IsEnabled = true,
                SeedKey = "prize_lucky_jackpot",
                IsBuiltIn = true,
                BuiltInVersion = Version,
                LastUpdateTime = now
            });

            // 二等奖 - 灵石
            prizes.Add(new LotteryPrizeEntity
            {
                PrizeId = "prize_lucky_second",
                PoolId = "pool_lucky",
                RewardType = 1,
                RewardAmount = 100,
                Probability = 500, // 5%
                SortOrder = prizeIdx++,
                IsEnabled = true,
                SeedKey = "prize_lucky_second",
                IsBuiltIn = true,
                BuiltInVersion = Version,
                LastUpdateTime = now
            });

            // 三等奖 - 金币
            prizes.Add(new LotteryPrizeEntity
            {
                PrizeId = "prize_lucky_third",
                PoolId = "pool_lucky",
                RewardType = 0,
                RewardAmount = 2000,
                Probability = 1500, // 15%
                SortOrder = prizeIdx++,
                IsEnabled = true,
                SeedKey = "prize_lucky_third",
                IsBuiltIn = true,
                BuiltInVersion = Version,
                LastUpdateTime = now
            });

            // 四等奖 - 金币
            prizes.Add(new LotteryPrizeEntity
            {
                PrizeId = "prize_lucky_fourth",
                PoolId = "pool_lucky",
                RewardType = 0,
                RewardAmount = 500,
                Probability = 3000, // 30%
                SortOrder = prizeIdx++,
                IsEnabled = true,
                SeedKey = "prize_lucky_fourth",
                IsBuiltIn = true,
                BuiltInVersion = Version,
                LastUpdateTime = now
            });

            // 谢谢惠顾
            prizes.Add(new LotteryPrizeEntity
            {
                PrizeId = "prize_lucky_thanks",
                PoolId = "pool_lucky",
                RewardType = 5,
                RewardAmount = 1,
                Probability = 4900, // 49%
                SortOrder = prizeIdx++,
                IsEnabled = true,
                SeedKey = "prize_lucky_thanks",
                IsBuiltIn = true,
                BuiltInVersion = Version,
                LastUpdateTime = now
            });

            return prizes;
        }

        #endregion
    }
}
