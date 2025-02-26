using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;

namespace DisplayRotator
{
    public class Settings
    {
        private static readonly string SettingsPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "DisplayRotator",
            "settings.json"
        );

        public Dictionary<int, Keys> Shortcuts { get; set; } = new();
        public Dictionary<int, bool> EnabledSettings { get; set; } = new();

        public static Settings Load()
        {
            try
            {
                if (File.Exists(SettingsPath))
                {
                    var json = File.ReadAllText(SettingsPath);
                    return JsonSerializer.Deserialize<Settings>(json) ?? new Settings();
                }
            }
            catch
            {
                // エラー時は新規設定を返す
            }
            return new Settings();
        }

        public void Save()
        {
            try
            {
                var directory = Path.GetDirectoryName(SettingsPath);
                if (directory != null && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                var json = JsonSerializer.Serialize(this);
                File.WriteAllText(SettingsPath, json);
            }
            catch
            {
                // エラー処理（必要に応じて）
            }
        }
    }
}
