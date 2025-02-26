using System.Runtime.InteropServices;
using static DisplayRotator.Win32Api;

namespace DisplayRotator
{
    public class MainForm : Form
    {
        // Windows APIのインポート
        [DllImport("user32.dll")]
        public static extern bool EnumDisplaySettings(string? deviceName, int modeNum, ref DEVMODE devMode);

        [DllImport("user32.dll")]
        public static extern int ChangeDisplaySettings(ref DEVMODE devMode, int flags);

        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        // ホットキー関連の定数
        private const int MOD_ALT = 0x0001;
        private const int MOD_CONTROL = 0x0002;
        private const int MOD_SHIFT = 0x0004;
        private const int MOD_WIN = 0x0008;

        private NotifyIcon? notifyIcon;
        private ContextMenuStrip? contextMenu;
        private SettingsManager _settingsManager = new();

        public MainForm()
        {
            InitializeComponent();
            UpdateMenuItems();  // メニュー項目を初期化
            this.Hide();  // フォームを非表示に
            RegisterHotKeys();  // ホットキーを登録
        }

        private void InitializeComponent()
        {
            // コンテキストメニューの初期化
            contextMenu = new ContextMenuStrip();
            contextMenu.AutoClose = true;

            // 通知アイコンの設定
            notifyIcon = new NotifyIcon();
            notifyIcon.Icon = new Icon(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "display.ico"));
            notifyIcon.Text = "Display Rotator";
            notifyIcon.Visible = true;

            // マウスクリックイベントの設定
            notifyIcon.MouseClick += (s, e) =>
            {
                if (contextMenu.Visible)
                {
                    contextMenu.Close();
                    return;
                }

                Point mousePosition = Control.MousePosition;
                Rectangle workingArea = Screen.FromPoint(mousePosition).WorkingArea;

                // Y軸の位置計算（マウスの位置に応じて上下を決定）
                int x = mousePosition.X - (contextMenu.Width / 2);
                int y = mousePosition.Y > (workingArea.Top + workingArea.Height / 2)
                    ? mousePosition.Y - contextMenu.Height
                    : mousePosition.Y;

                // 画面からはみ出ないように調整
                x = Math.Max(workingArea.Left, Math.Min(workingArea.Right - contextMenu.Width, x));
                y = Math.Max(workingArea.Top, Math.Min(workingArea.Bottom - contextMenu.Height, y));

                // メニューを表示
                contextMenu.Show(new Point(x, y));
            };

            // フォームの設定
            this.WindowState = FormWindowState.Minimized;
            this.ShowInTaskbar = false;
        }

        private void ShowShortcutSettings()
        {
            using var form = new ShortcutSettingsForm(_settingsManager);
            if (form.ShowDialog() == DialogResult.OK)
            {
                // メニュー項目の表示を更新
                UpdateMenuItems();
                // ホットキーを再登録
                UnregisterHotKeys();
                RegisterHotKeys();
            }
        }

        private void UpdateMenuItems()
        {
            contextMenu?.Items.Clear();

            var rotations = new[] {
                (name: "標準 (0°)", id: RotationConstants.DMDO_DEFAULT),
                (name: "90°回転", id: RotationConstants.DMDO_90),
                (name: "180°回転", id: RotationConstants.DMDO_180),
                (name: "270°回転", id: RotationConstants.DMDO_270)
            };

            foreach (var rotation in rotations)
            {
                if (_settingsManager.IsEnabled(rotation.id))
                {
                    var menuItem = new ToolStripMenuItem(rotation.name);
                    menuItem.Click += (s, e) => RotateScreen(rotation.id);
                    var shortcut = _settingsManager.GetShortcut(rotation.id);
                    menuItem.Text = shortcut.HasValue
                        ? $"{rotation.name} ({shortcut})"
                        : rotation.name;
                    contextMenu?.Items.Add(menuItem);
                }
            }

            contextMenu?.Items.Add("-");
            contextMenu?.Items.Add("ショートカット設定", null, (s, e) => ShowShortcutSettings());
            contextMenu?.Items.Add("-");
            contextMenu?.Items.Add("終了", null, (s, e) => Application.Exit());
        }

        private void RegisterHotKeys()
        {
            var rotations = new[] {
                RotationConstants.DMDO_DEFAULT,
                RotationConstants.DMDO_90,
                RotationConstants.DMDO_180,
                RotationConstants.DMDO_270
            };

            for (int i = 0; i < rotations.Length; i++)
            {
                if (_settingsManager.IsEnabled(rotations[i]))
                {
                    var shortcut = _settingsManager.GetShortcut(rotations[i]);
                    if (shortcut.HasValue)
                    {
                        int modifiers = GetModifiers(shortcut.Value);
                        Keys key = shortcut.Value & Keys.KeyCode;
                        if (!RegisterHotKey(this.Handle, i, modifiers, (int)key))
                        {
                            MessageBox.Show($"ホットキーの登録に失敗しました: {shortcut.Value}", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
        }

        private void UnregisterHotKeys()
        {
            for (int i = 0; i <= 3; i++)
            {
                UnregisterHotKey(this.Handle, i);
            }
        }

        private int GetModifiers(Keys key)
        {
            int modifiers = 0;
            if ((key & Keys.Control) == Keys.Control) modifiers |= MOD_CONTROL;
            if ((key & Keys.Alt) == Keys.Alt) modifiers |= MOD_ALT;
            if ((key & Keys.Shift) == Keys.Shift) modifiers |= MOD_SHIFT;
            if ((key & Keys.LWin) == Keys.LWin || (key & Keys.RWin) == Keys.RWin) modifiers |= MOD_WIN;
            return modifiers;
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_HOTKEY)
            {
                int id = m.WParam.ToInt32();
                if (id >= 0 && id < 4) // 修正: id の範囲を確認
                {
                    RotateScreen(id);
                    return;
                }
            }
            base.WndProc(ref m);
        }

        private void RotateScreen(int orientation)
        {
            DEVMODE dm = new DEVMODE();
            dm.dmSize = (short)Marshal.SizeOf(typeof(DEVMODE));

            // 現在のディスプレイ設定を取得
            EnumDisplaySettings(null, ENUM_CURRENT_SETTINGS, ref dm);

            // 現在の向きを記録
            int currentOrientation = dm.dmDisplayOrientation;

            // 新しい向きを設定
            dm.dmDisplayOrientation = orientation;

            // 90度/270度回転の場合は幅と高さを入れ替え
            if ((orientation == RotationConstants.DMDO_90 || orientation == RotationConstants.DMDO_270) &&
                (currentOrientation == RotationConstants.DMDO_DEFAULT || currentOrientation == RotationConstants.DMDO_180))
            {
                int temp = dm.dmPelsHeight;
                dm.dmPelsHeight = dm.dmPelsWidth;
                dm.dmPelsWidth = temp;
            }
            else if ((orientation == RotationConstants.DMDO_DEFAULT || orientation == RotationConstants.DMDO_180) &&
                     (currentOrientation == RotationConstants.DMDO_90 || currentOrientation == RotationConstants.DMDO_270))
            {
                int temp = dm.dmPelsHeight;
                dm.dmPelsHeight = dm.dmPelsWidth;
                dm.dmPelsWidth = temp;
            }

            // 変更を適用
            ChangeDisplaySettings(ref dm, CDS_UPDATEREGISTRY);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            UnregisterHotKeys();
            if (notifyIcon != null)
            {
                notifyIcon.Visible = false;
                notifyIcon.Dispose();
            }
            base.OnFormClosing(e);
        }
    }
}