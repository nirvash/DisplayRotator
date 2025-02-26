using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Resources;
using System.Globalization;

namespace DisplayRotator
{
    public class ShortcutSettingsForm : Form
    {
        private readonly SettingsManager _settingsManager;
        private readonly TableLayoutPanel _layout;
        private readonly Dictionary<int, (Label label, Button clearButton, CheckBox displayCheckBox)> _controls = new();
        private readonly ResourceManager resourceManager;

        public ShortcutSettingsForm(SettingsManager settingsManager)
        {
            _settingsManager = settingsManager;
            _layout = new TableLayoutPanel();
            resourceManager = new ResourceManager("DisplayRotator.Properties.Resources", typeof(ShortcutSettingsForm).Assembly);
            InitializeComponents();
            LoadDisplaySettings();

            // マウス位置を取得
            Point mousePosition = Control.MousePosition;

            // 画面の作業領域を取得
            Rectangle workingArea = Screen.FromPoint(mousePosition).WorkingArea;

            // フォームの位置をマウス位置を中心に設定
            int x = mousePosition.X - (this.Width / 2);
            int y = mousePosition.Y - (this.Height / 2);

            // 画面からはみ出ないように調整
            if (x < workingArea.Left) x = workingArea.Left;
            if (x + this.Width > workingArea.Right) x = workingArea.Right - this.Width;
            if (y < workingArea.Top) y = workingArea.Top;
            if (y + this.Height > workingArea.Bottom) y = workingArea.Bottom - this.Height;

            // フォームの位置を設定
            this.StartPosition = FormStartPosition.Manual;
            this.Location = new Point(x, y);
        }

        private void InitializeComponents()
        {
            Text = resourceManager.GetString("ShortcutSettings", CultureInfo.CurrentCulture);
            MinimumSize = new System.Drawing.Size(400, 250);
            Size = MinimumSize;
            FormBorderStyle = FormBorderStyle.Sizable;  // リサイズ可能に変更
            MaximizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;

            _layout.Dock = DockStyle.Fill;
            _layout.ColumnCount = 4;
            _layout.RowCount = 5;
            // カラム幅の配分を変更（回転方向を短く、ショートカットを長く）
            _layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 50F));   // 表示: 固定幅
            _layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100F));  // 回転方向: 固定幅
            _layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));   // ショートカット: 残りのスペース
            _layout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));        // 操作ボタン: 自動サイズ
            _layout.Padding = new Padding(8);
            _layout.AutoSize = true;

            // 固定行の高さを設定
            for (int i = 0; i < 5; i++)
            {
                _layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 35F));
            }

            // ヘッダー行
            _layout.Controls.Add(new Label { Text = resourceManager.GetString("Display", CultureInfo.CurrentCulture), TextAlign = ContentAlignment.MiddleLeft }, 0, 0);
            _layout.Controls.Add(new Label { Text = resourceManager.GetString("RotationDirection", CultureInfo.CurrentCulture), TextAlign = ContentAlignment.MiddleLeft }, 1, 0);
            _layout.Controls.Add(new Label { Text = resourceManager.GetString("Shortcut", CultureInfo.CurrentCulture), TextAlign = ContentAlignment.MiddleLeft }, 2, 0);
            _layout.Controls.Add(new Label { Text = resourceManager.GetString("Operation", CultureInfo.CurrentCulture), TextAlign = ContentAlignment.MiddleLeft }, 3, 0);

            var rotations = new[]
            {
                (name: resourceManager.GetString("DefaultRotation", CultureInfo.CurrentCulture), id: RotationConstants.DMDO_DEFAULT),
                (name: resourceManager.GetString("Rotate90", CultureInfo.CurrentCulture), id: RotationConstants.DMDO_90),
                (name: resourceManager.GetString("Rotate180", CultureInfo.CurrentCulture), id: RotationConstants.DMDO_180),
                (name: resourceManager.GetString("Rotate270", CultureInfo.CurrentCulture), id: RotationConstants.DMDO_270)
            };

            for (int i = 0; i < rotations.Length; i++)
            {
                var rotation = rotations[i];
                var row = i + 1;

                // 表示チェックボックス
                var displayCheckBox = new CheckBox
                {
                    Checked = _settingsManager.IsEnabled(rotation.id),
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleCenter
                };

                // 回転方向ラベル
                var directionLabel = new Label
                {
                    Text = rotation.name,
                    TextAlign = ContentAlignment.MiddleLeft,
                    Dock = DockStyle.Fill
                };

                // 現在の設定
                var shortcutLabel = new Label
                {
                    Text = _settingsManager.GetShortcutText(rotation.id),
                    TextAlign = ContentAlignment.MiddleLeft,
                    Dock = DockStyle.Fill
                };

                // ボタンパネル
                var buttonPanel = new FlowLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    FlowDirection = FlowDirection.LeftToRight,
                    AutoSize = true,
                    WrapContents = false,
                    Margin = new Padding(0)
                };

                var setButton = new Button
                {
                    Text = resourceManager.GetString("Set", CultureInfo.CurrentCulture),
                    AutoSize = true,
                    Margin = new Padding(0, 0, 4, 0),
                    Height = 23,
                    Width = 60  // ボタン幅を固定
                };

                var clearButton = new Button
                {
                    Text = resourceManager.GetString("Clear", CultureInfo.CurrentCulture),
                    AutoSize = true,
                    Height = 23,
                    Width = 60,  // ボタン幅を固定
                    Enabled = _settingsManager.GetShortcut(rotation.id).HasValue
                };

                int capturedId = rotation.id; // キャプチャ用
                setButton.Click += (s, e) => SetShortcut(capturedId);
                clearButton.Click += (s, e) => ClearShortcut(capturedId);

                buttonPanel.Controls.AddRange(new Control[] { setButton, clearButton });

                _layout.Controls.Add(displayCheckBox, 0, row);
                _layout.Controls.Add(directionLabel, 1, row);
                _layout.Controls.Add(shortcutLabel, 2, row);
                _layout.Controls.Add(buttonPanel, 3, row);

                // コントロールを保存
                _controls[rotation.id] = (shortcutLabel, clearButton, displayCheckBox);
            }

            var bottomPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                FlowDirection = FlowDirection.RightToLeft,
                Height = 35,
                Padding = new Padding(8)
            };

            var closeButton = new Button { Text = resourceManager.GetString("Close", CultureInfo.CurrentCulture), DialogResult = DialogResult.OK };
            bottomPanel.Controls.Add(closeButton);

            Controls.Add(_layout);
            Controls.Add(bottomPanel);
        }

        private void LoadDisplaySettings()
        {
            foreach (var rotation in _controls.Keys)
            {
                var displaySetting = _settingsManager.IsEnabled(rotation);
                if (_controls.TryGetValue(rotation, out var controls))
                {
                    controls.displayCheckBox.Checked = displaySetting;
                }
            }
        }

        private void SaveDisplaySettings()
        {
            foreach (var rotation in _controls.Keys)
            {
                if (_controls.TryGetValue(rotation, out var controls))
                {
                    _settingsManager.SetEnabled(rotation, controls.displayCheckBox.Checked);
                }
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            SaveDisplaySettings();
            base.OnFormClosing(e);
        }

        private void SetShortcut(int rotationId)
        {
            var currentShortcut = _settingsManager.GetShortcut(rotationId);
            using var form = new ShortcutForm(currentShortcut);
            if (form.ShowDialog() == DialogResult.OK)
            {
                _settingsManager.SetShortcut(rotationId, form.ShortcutKey);
                if (_controls.TryGetValue(rotationId, out var controls))
                {
                    controls.label.Text = _settingsManager.GetShortcutText(rotationId);
                    controls.clearButton.Enabled = true;
                }
            }
        }

        private void ClearShortcut(int rotationId)
        {
            _settingsManager.RemoveShortcut(rotationId);
            if (_controls.TryGetValue(rotationId, out var controls))
            {
                controls.label.Text = resourceManager.GetString("NotSet", CultureInfo.CurrentCulture);
                controls.clearButton.Enabled = false;
            }
        }
    }
}
