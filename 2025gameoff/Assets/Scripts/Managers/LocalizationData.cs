using System.Collections.Generic;

public static class LocalizationData
{
    public static string[] LANGUAGES = { "zh", "en" };


    private static readonly Dictionary<string, Dictionary<string, string>> _languages
        = new Dictionary<string, Dictionary<string, string>>()
        {
            {
                "zh", new Dictionary<string, string>()
                {
                    { "language_name", "中文"},
                    { "welcome", "欢迎" },
                    { "start_game", "开始游戏" },
                    { "exit", "退出" },
                    { "settings", "设置" },
                    { "confirm", "确认" },
                    { "cancel", "取消" }
                }
            },
            {
                "en", new Dictionary<string, string>()
                {
                    { "language_name", "English"},
                    { "welcome", "Welcome" },
                    { "start_game", "Start Game" },
                    { "exit", "Exit" },
                    { "settings", "Settings" },
                    { "confirm", "Confirm" },
                    { "cancel", "Cancel" }
                }
            }
        };


    public static Dictionary<string, string> GetLanguage(string lang)
    {
        if (_languages.TryGetValue(lang, out var dict))
            return dict;
        else if (_languages.TryGetValue("en", out var fallback))
            return fallback;
        else
            return new Dictionary<string, string>();
    }
}
