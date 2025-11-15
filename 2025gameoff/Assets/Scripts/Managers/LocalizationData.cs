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
                    { "language", "语言"},
                    {"windowed", "窗口" },
                    {"fullscreen", "全屏" },
                    { "resolution", "分辨率" },
                    { "master_volume", "主音量" },
                    { "music_volume", "背景音乐" },
                    { "effect_volume", "音效音量" },
                    { "default", "恢复默认" },
                    { "display_mode", "显示模式"},


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
                    { "language", "Language"},
                    {"windowed", "Windowed" },
                    {"fullscreen", "Fullscreen" },
                    { "resolution", "Resolution" },
                    { "master_volume", "Master Volume" },
                    { "music_volume", "Music Volume" },
                    { "effect_volume", "Effect Volume" },
                    { "default", "Default" },
                    { "display_mode", "Display Mode"},

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
