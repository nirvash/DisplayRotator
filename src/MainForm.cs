using System.Runtime.InteropServices;
using static DisplayRotator.Win32Api;
using System.Resources;
using System.Globalization;

namespace DisplayRotator
{
    public class MainForm : Form
    {
        // ホットキー関連の修飾キー（このアプリ内でのみ使用）
        private const int MOD_ALT = 0x0001;
        private const int MOD_CONTROL = 0x0002;
        private const int MOD_SHIFT = 0x0004;
        private const int MOD_WIN = 0x0008;

        private NotifyIcon? notifyIcon;
        private ContextMenuStrip? contextMenu;
        private SettingsManager _settingsManager = new();
        private ResourceManager resourceManager;

        public MainForm()
        {
            InitializeComponent();
            resourceManager = new ResourceManager("DisplayRotator.Properties.Resources", typeof(MainForm).Assembly);
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
                SetForegroundWindow(new HandleRef(this, this.Handle));
                contextMenu.Show(new Point(x, y));
            };

            // フォームの設定
            this.WindowState = FormWindowState.Minimized;
            this.ShowInTaskbar = false;
        }

        private void ShowShortcutSettings()
        {
            using var form = new ShortcutSettingsForm(_settingsManager);
            form.Icon = new Icon(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "display.ico"));  // アイコンを設定
            form.ShowInTaskbar = true;  // タスクバーに表示
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
                (name: resourceManager.GetString("DefaultRotation", CultureInfo.CurrentCulture), id: RotationConstants.DMDO_DEFAULT),
                (name: resourceManager.GetString("Rotate90", CultureInfo.CurrentCulture), id: RotationConstants.DMDO_90),
                (name: resourceManager.GetString("Rotate180", CultureInfo.CurrentCulture), id: RotationConstants.DMDO_180),
                (name: resourceManager.GetString("Rotate270", CultureInfo.CurrentCulture), id: RotationConstants.DMDO_270)
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

            if (_settingsManager.IsEnabled(RotationConstants.SWITCH_PRIMARY_DISPLAY))
            {
                contextMenu?.Items.Add("-");
                var menuItem = new ToolStripMenuItem(resourceManager.GetString("SwitchPrimaryDisplay", CultureInfo.CurrentCulture));
                menuItem.Click += (s, e) => SwitchPrimaryDisplay();
                var shortcut = _settingsManager.GetShortcut(RotationConstants.SWITCH_PRIMARY_DISPLAY);
                menuItem.Text = shortcut.HasValue
                    ? $"{menuItem.Text} ({shortcut})"
                    : menuItem.Text;
                contextMenu?.Items.Add(menuItem);
            }

            contextMenu?.Items.Add("-");
            contextMenu?.Items.Add(resourceManager.GetString("ShortcutSettings", CultureInfo.CurrentCulture), null, (s, e) => ShowShortcutSettings());
            contextMenu?.Items.Add("-");
            contextMenu?.Items.Add(resourceManager.GetString("Exit", CultureInfo.CurrentCulture), null, (s, e) => Application.Exit());
        }

        private void RegisterHotKeys()
        {
            var rotations = new[] {
                RotationConstants.DMDO_DEFAULT,
                RotationConstants.DMDO_90,
                RotationConstants.DMDO_180,
                RotationConstants.DMDO_270,
                RotationConstants.SWITCH_PRIMARY_DISPLAY
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
                            MessageBox.Show(resourceManager.GetString("HotKeyRegistrationFailed", CultureInfo.CurrentCulture) + $": {shortcut.Value}",
                                            resourceManager.GetString("Error", CultureInfo.CurrentCulture),
                                            MessageBoxButtons.OK,
                                            MessageBoxIcon.Error);
                        }
                    }
                }
            }
        }

        private void UnregisterHotKeys()
        {
            for (int i = 0; i <= 4; i++)
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
                // Use the number of rotation hotkeys for the range check
                int rotationHotkeyCount = 4; // DMDO_DEFAULT, DMDO_90, DMDO_180, DMDO_270
                if (id >= 0 && id < rotationHotkeyCount)
                {
                    RotateScreen(id);
                    return;
                }
                else if (id == RotationConstants.SWITCH_PRIMARY_DISPLAY)
                {
                    SwitchPrimaryDisplay();
                    return;
                }
            }
            base.WndProc(ref m);
        }

        private void RotateScreen(int orientation)
        {
            var dm = new DEVMODE();
            dm.dmSize = (short)Marshal.SizeOf(typeof(DEVMODE));

            // 現在のディスプレイ設定（アクティブ ディスプレイ）を取得
            Logger.Info($"[Rotate] START orientation={orientation}");
            if (!EnumDisplaySettings(null, ENUM_CURRENT_SETTINGS, ref dm))
            {
                Logger.Error("[Rotate] EnumDisplaySettings failed (null device)");
                MessageBox.Show("Failed to get current display settings", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            int currentOrientation = dm.dmDisplayOrientation;
            Logger.Info($"[Rotate] Current orient={currentOrientation} width={dm.dmPelsWidth} height={dm.dmPelsHeight} bpp={dm.dmBitsPerPel} freq={dm.dmDisplayFrequency}");

            // 新しい向きを設定
            dm.dmDisplayOrientation = orientation;

            // 回転に応じて幅・高さを必要なら入れ替える
            bool needSwap =
                (orientation == RotationConstants.DMDO_90 || orientation == RotationConstants.DMDO_270) !=
                (currentOrientation == RotationConstants.DMDO_90 || currentOrientation == RotationConstants.DMDO_270);

            if (needSwap)
            {
                int tmp = dm.dmPelsWidth;
                dm.dmPelsWidth = dm.dmPelsHeight;
                dm.dmPelsHeight = tmp;
                Logger.Info("[Rotate] Swapped width/height for 90/270 transition");
            }

            // 変更対象フィールドを明示
            dm.dmFields |= DM_DISPLAYORIENTATION | DM_PELSWIDTH | DM_PELSHEIGHT;
            Logger.Info($"[Rotate] Apply newOrient={dm.dmDisplayOrientation} width={dm.dmPelsWidth} height={dm.dmPelsHeight} fields=0x{dm.dmFields:X}");

            // 1) レジストリ更新を伴う適用（デバイス指定なし＝現在のディスプレイ）
            int result = ChangeDisplaySettingsEx(null, ref dm, IntPtr.Zero, CDS_UPDATEREGISTRY, IntPtr.Zero);
            if (result != DISP_CHANGE_SUCCESSFUL)
            {
                Logger.Error($"[Rotate] Stage1 failed result={result} win32err={Marshal.GetLastWin32Error()}");
                MessageBox.Show($"Failed to change display settings (Ex). Result: {result}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // 2) 最終確認の呼び出し（NULL DEVMODE）で OS に確定させる
            result = ChangeDisplaySettingsEx(null, IntPtr.Zero, IntPtr.Zero, 0, IntPtr.Zero);
            if (result != DISP_CHANGE_SUCCESSFUL)
            {
                Logger.Error($"[Rotate] Confirm failed result={result} win32err={Marshal.GetLastWin32Error()}");
                MessageBox.Show($"Display settings confirmation failed. Result: {result}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                Logger.Info("[Rotate] SUCCESS");
            }
        }

        private void SwitchPrimaryDisplay() {
            if (Screen.AllScreens.Length < 2) return;
            Logger.Info("[SwitchPrimary] START");

            // 列挙
            var devices = new List<(DISPLAY_DEVICE dev, DEVMODE mode, bool isPrimary)>();
            for (uint i = 0; ; i++) {
                var dd = new DISPLAY_DEVICE { cb = Marshal.SizeOf<DISPLAY_DEVICE>() };
                if (!EnumDisplayDevices(null, i, ref dd, 0)) break;
                if ((dd.StateFlags & DISPLAY_DEVICE_ATTACHED_TO_DESKTOP) == 0) continue;
                var dm = new DEVMODE { dmSize = (short)Marshal.SizeOf<DEVMODE>() };
                if (!EnumDisplaySettings(dd.DeviceName, ENUM_CURRENT_SETTINGS, ref dm)) continue;
                bool primary = (dd.StateFlags & DISPLAY_DEVICE_PRIMARY_DEVICE) != 0;
                devices.Add((dd, dm, primary));
                Logger.Info($"[SwitchPrimary] Enum name={dd.DeviceName} primary={primary} pos=({dm.dmPositionX},{dm.dmPositionY}) size={dm.dmPelsWidth}x{dm.dmPelsHeight}");
            }
            if (devices.Count < 2) { Logger.Info("[SwitchPrimary] Only one display"); return; }

            var current = devices.FirstOrDefault(d => d.isPrimary);
            var next = devices.FirstOrDefault(d => !d.isPrimary);
            if (next.dev.DeviceName == null || current.dev.DeviceName == null) { Logger.Error("[SwitchPrimary] Device name missing"); return; }
            Logger.Info($"[SwitchPrimary] currentPrimary={current.dev.DeviceName} nextPrimary={next.dev.DeviceName}");

            int offsetX = next.mode.dmPositionX;
            int offsetY = next.mode.dmPositionY;

            // 新プライマリを (0,0) へ移動しつつ一度でプライマリ化（フォールバックなし）
            var primaryDm = next.mode; primaryDm.dmSize = (short)Marshal.SizeOf<DEVMODE>();
            primaryDm.dmPositionX = primaryDm.dmPositionX - offsetX;
            primaryDm.dmPositionY = primaryDm.dmPositionY - offsetY;
            primaryDm.dmFields = DM_POSITION;
            Logger.Info($"[SwitchPrimary] SetPrimary+Move device={next.dev.DeviceName} pos=({primaryDm.dmPositionX},{primaryDm.dmPositionY}) size={primaryDm.dmPelsWidth}x{primaryDm.dmPelsHeight}");
            int rPrimary = ChangeDisplaySettingsEx(next.dev.DeviceName, ref primaryDm, IntPtr.Zero, CDS_UPDATEREGISTRY | CDS_NORESET | CDS_SET_PRIMARY, IntPtr.Zero);
            if (rPrimary != DISP_CHANGE_SUCCESSFUL) {
                Logger.Error($"[SwitchPrimary] SetPrimary+Move failed result={rPrimary} err={Marshal.GetLastWin32Error()}");
                MessageBox.Show($"Failed to set primary {next.dev.DeviceName} result={rPrimary}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // 他ディスプレイ再配置
            foreach (var entry in devices.Where(d => d.dev.DeviceName != next.dev.DeviceName)) {
                var dm = entry.mode; dm.dmSize = (short)Marshal.SizeOf<DEVMODE>();
                dm.dmPositionX = dm.dmPositionX - offsetX;
                dm.dmPositionY = dm.dmPositionY - offsetY;
                dm.dmFields = DM_POSITION;
                Logger.Info($"[SwitchPrimary] Reposition {entry.dev.DeviceName} pos=({dm.dmPositionX},{dm.dmPositionY}) size={dm.dmPelsWidth}x{dm.dmPelsHeight}");
                int r = ChangeDisplaySettingsEx(entry.dev.DeviceName, ref dm, IntPtr.Zero, CDS_UPDATEREGISTRY | CDS_NORESET, IntPtr.Zero);
                if (r != DISP_CHANGE_SUCCESSFUL) {
                    Logger.Error($"[SwitchPrimary] Reposition failed {entry.dev.DeviceName} r={r} err={Marshal.GetLastWin32Error()}");
                    MessageBox.Show($"Failed to reposition {entry.dev.DeviceName} r={r}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            int confirm = ChangeDisplaySettingsEx(null, IntPtr.Zero, IntPtr.Zero, 0, IntPtr.Zero);
            if (confirm != DISP_CHANGE_SUCCESSFUL) {
                Logger.Error($"[SwitchPrimary] Confirm failed r={confirm} err={Marshal.GetLastWin32Error()}");
                MessageBox.Show($"Display config confirmation failed r={confirm}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            } else {
                Logger.Info("[SwitchPrimary] SUCCESS");
            }
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
