using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace DisplayRotator
{
    public class SettingsManager
    {
        private Dictionary<int, Keys> _shortcuts = new();
        private const string FilePath = "shortcuts.txt";
        private Dictionary<int, bool> _enabledSettings = new();

        public SettingsManager()
        {
            LoadSettings();
            InitializeDefaultSettings();
        }

        public void SetShortcut(int rotationId, Keys key)
        {
            _shortcuts[rotationId] = key;
            SaveSettings();
        }

        public void RemoveShortcut(int rotationId)
        {
            _shortcuts.Remove(rotationId);
            SaveSettings();
        }

        public Keys? GetShortcut(int rotationId)
        {
            return _shortcuts.TryGetValue(rotationId, out var key) ? key : null;
        }

        public string GetShortcutText(int rotationId)
        {
            return GetShortcut(rotationId)?.ToString() ?? "未設定";
        }

        public bool IsEnabled(int rotationId)
        {
            return _enabledSettings.TryGetValue(rotationId, out var enabled) && enabled;
        }

        public void SetEnabled(int rotationId, bool enabled)
        {
            _enabledSettings[rotationId] = enabled;
            SaveSettings();
        }

        private void SaveSettings()
        {
            var settings = new Settings { Shortcuts = _shortcuts, EnabledSettings = _enabledSettings };
            settings.Save();
        }

        private void LoadSettings()
        {
            var settings = Settings.Load();
            _shortcuts = settings.Shortcuts;
            _enabledSettings = settings.EnabledSettings;
        }

        private void InitializeDefaultSettings()
        {
            var rotations = new[] {
                RotationConstants.DMDO_DEFAULT,
                RotationConstants.DMDO_90,
                RotationConstants.DMDO_180,
                RotationConstants.DMDO_270
            };

            foreach (var rotation in rotations)
            {
                if (!_enabledSettings.ContainsKey(rotation))
                {
                    _enabledSettings[rotation] = true;
                }
            }

            SaveSettings();
        }
    }
}
