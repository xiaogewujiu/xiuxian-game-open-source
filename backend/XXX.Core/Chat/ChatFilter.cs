namespace XXX.Chat
{
    /// <summary>
    /// 聊天过滤器类
    /// 负责消息内容过滤和敏感词检测
    /// </summary>
    public class ChatFilter
    {
        /// <summary>
        /// 敏感词列表
        /// Key: 敏感词, Value: 替换文本
        /// </summary>
        private static Dictionary<string, string> sensitiveWords = [];

        /// <summary>
        /// 禁止发送的词（直接拒绝）
        /// </summary>
        private static HashSet<string> bannedWords = [];

        /// <summary>
        /// 初始化敏感词库
        /// </summary>
        static ChatFilter()
        {
            InitializeSensitiveWords();
        }

        /// <summary>
        /// 初始化敏感词和禁词
        /// </summary>
        private static void InitializeSensitiveWords()
        {
            // 示例敏感词（实际应从配置文件或数据库读取）
            var words = new[]
            {
                "垃圾", "傻逼", "脑残", "白痴",
                "作弊", "外挂", "破解", "GM",
                "代练", "买金", "卖金"
            };

            foreach (var word in words)
            {
                sensitiveWords[word] = "***";
            }

            // 禁词（直接拒绝发送）
            var banned = new[]
            {
                "刷屏", "刷屏",
                "政治", "涉黄", "涉暴"
            };

            foreach (var word in banned)
            {
                bannedWords.Add(word);
            }
        }

        /// <summary>
        /// 检查消息是否包含敏感词
        /// </summary>
        /// <param name="message">消息内容</param>
        /// <returns>是否包含敏感词</returns>
        public static bool ContainsSensitiveWord(string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                return false;
            }

            // 检查禁词
            foreach (var bannedWord in bannedWords)
            {
                if (message.Contains(bannedWord))
                {
                    return true;
                }
            }

            // 检查敏感词
            foreach (var sensitiveWord in sensitiveWords.Keys)
            {
                if (message.Contains(sensitiveWord))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 替换敏感词
        /// </summary>
        /// <param name="message">消息内容</param>
        /// <returns>替换后的内容</returns>
        public static string ReplaceSensitiveWords(string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                return message;
            }

            string result = message;

            // 替换敏感词
            foreach (var kvp in sensitiveWords)
            {
                result = result.Replace(kvp.Key, kvp.Value);
            }

            return result;
        }

        /// <summary>
        /// 检查是否包含禁词
        /// </summary>
        /// <param name="message">消息内容</param>
        /// <returns>是否包含禁词</returns>
        public static bool ContainsBannedWord(string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                return false;
            }

            foreach (var bannedWord in bannedWords)
            {
                if (message.Contains(bannedWord))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 过滤消息内容
        /// </summary>
        /// <param name="message">原始消息</param>
        /// <returns>过滤后的消息和是否通过验证</returns>
        public static FilterResult FilterMessage(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return new FilterResult
                {
                    Pass = false,
                    FilteredMessage = message,
                    Reason = "消息不能为空"
                };
            }

            // 检查消息长度
            if (message.Length > ChatConfig.MaxMessageLength)
            {
                return new FilterResult
                {
                    Pass = false,
                    FilteredMessage = message,
                    Reason = $"消息过长，最多{ChatConfig.MaxMessageLength}个字符"
                };
            }

            // 检查禁词
            if (ContainsBannedWord(message))
            {
                return new FilterResult
                {
                    Pass = false,
                    FilteredMessage = message,
                    Reason = "消息包含禁止的内容"
                };
            }

            // 替换敏感词
            string filteredMessage = ReplaceSensitiveWords(message);

            // 检查是否仍有敏感词（替换后）
            if (ContainsSensitiveWord(filteredMessage))
            {
                return new FilterResult
                {
                    Pass = false,
                    FilteredMessage = filteredMessage,
                    Reason = "消息包含敏感词"
                };
            }

            return new FilterResult
            {
                Pass = true,
                FilteredMessage = filteredMessage,
                Reason = ""
            };
        }

        /// <summary>
        /// 检查刷屏行为
        /// </summary>
        /// <param name="playerId">玩家ID</param>
        /// <param name="recentMessages">最近发送的消息</param>
        /// <returns>是否疑似刷屏</returns>
        public static bool CheckSpam(string playerId, Queue<ChatMessage> recentMessages)
        {
            // 检查最近5条消息
            const int checkCount = 5;
            if (recentMessages.Count < checkCount)
            {
                return false;
            }

            // 获取最近的5条消息
            var recentList = recentMessages.ToArray().TakeLast(checkCount).ToList();

            // 检查是否都是相同内容
            var firstContent = recentList[0].Content;
            bool allSame = recentList.All(m => m.Content == firstContent);

            if (allSame)
            {
                return true;
            }

            // 检查短时间内发送消息数量
            var timeSpan = recentList.Last().SendTime - recentList.First().SendTime;
            if (timeSpan.TotalSeconds < 10) // 10秒内发送5条消息
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// 格式化消息（移除多余空格、换行等）
        /// </summary>
        /// <param name="message">原始消息</param>
        /// <returns>格式化后的消息</returns>
        public static string FormatMessage(string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                return message;
            }

            // 移除多余空格
            message = System.Text.RegularExpressions.Regex.Replace(message, @"\s+", " ");

            // 移除首尾空格
            message = message.Trim();

            return message;
        }

        /// <summary>
        /// 验证消息
        /// </summary>
        /// <param name="message">消息内容</param>
        /// <returns>验证结果</returns>
        public static ChatResult ValidateMessage(string message)
        {
            // 格式化消息
            string formattedMessage = FormatMessage(message);

            // 过滤消息
            var filterResult = FilterMessage(formattedMessage);

            if (!filterResult.Pass)
            {
                return ChatResult.Fail(GetResultCode(filterResult.Reason), filterResult.Reason);
            }

            return ChatResult.Ok("", filterResult.FilteredMessage);
        }

        /// <summary>
        /// 根据原因获取结果代码
        /// </summary>
        /// <param name="reason">原因</param>
        /// <returns>结果代码</returns>
        private static ChatResultCode GetResultCode(string reason)
        {
            if (reason.Contains("不能为空"))
            {
                return ChatResultCode.EmptyMessage;
            }
            else if (reason.Contains("过长"))
            {
                return ChatResultCode.MessageTooLong;
            }
            else if (reason.Contains("禁止"))
            {
                return ChatResultCode.ContainsSensitiveWord;
            }
            else if (reason.Contains("敏感词"))
            {
                return ChatResultCode.ContainsSensitiveWord;
            }
            else
            {
                return ChatResultCode.Success;
            }
        }

        /// <summary>
        /// 添加敏感词
        /// </summary>
        /// <param name="word">敏感词</param>
        /// <param name="replacement">替换文本</param>
        public static void AddSensitiveWord(string word, string replacement = "***")
        {
            if (!string.IsNullOrEmpty(word))
            {
                sensitiveWords[word] = replacement;
            }
        }

        /// <summary>
        /// 添加禁词
        /// </summary>
        /// <param name="word">禁词</param>
        public static void AddBannedWord(string word)
        {
            if (!string.IsNullOrEmpty(word))
            {
                bannedWords.Add(word);
            }
        }

        /// <summary>
        /// 获取所有敏感词
        /// </summary>
        /// <returns>敏感词列表</returns>
        public static Dictionary<string, string> GetAllSensitiveWords()
        {
            return new Dictionary<string, string>(sensitiveWords);
        }

        /// <summary>
        /// 获取所有禁词
        /// </summary>
        /// <returns>禁词列表</returns>
        public static HashSet<string> GetAllBannedWords()
        {
            return [.. bannedWords];
        }

        /// <summary>
        /// 清空所有敏感词
        /// </summary>
        public static void ClearAllWords()
        {
            sensitiveWords.Clear();
            bannedWords.Clear();
        }
    }

    /// <summary>
    /// 过滤结果类
    /// </summary>
    public class FilterResult
    {
        /// <summary>
        /// 是否通过验证
        /// </summary>
        public bool Pass { get; set; }

        /// <summary>
        /// 过滤后的消息
        /// </summary>
        public string FilteredMessage { get; set; } = string.Empty;

        /// <summary>
        /// 拒绝原因
        /// </summary>
        public string Reason { get; set; } = string.Empty;
    }
}
