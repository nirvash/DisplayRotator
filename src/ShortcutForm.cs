using System;
using System.Drawing;
using System.Windows.Forms;

namespace DisplayRotator
{
    public partial class ShortcutForm : Form
    {
        private Keys _shortcutKey;
        private Keys _selectedKey = Keys.None;

        public Keys ShortcutKey => _shortcutKey;

        public ShortcutForm(Keys? currentShortcut = null)
        {
            InitializeComponent();

            // 修飾キーボタンのデフォルト色を設定
            foreach (Button btn in modifierPanel.Controls)
            {
                btn.BackColor = SystemColors.Control;
                btn.FlatStyle = FlatStyle.Flat;
                btn.FlatAppearance.BorderColor = Color.Gray;
            }

            if (currentShortcut.HasValue)
            {
                LoadCurrentShortcut(currentShortcut.Value);
            }

            UpdateShortcutDisplay();
        }

        private void LoadCurrentShortcut(Keys shortcut)
        {
            // 修飾キーの状態を設定
            SetModifierButtonState(btnCtrl, (shortcut & Keys.Control) == Keys.Control);
            SetModifierButtonState(btnAlt, (shortcut & Keys.Alt) == Keys.Alt);
            SetModifierButtonState(btnShift, (shortcut & Keys.Shift) == Keys.Shift);
            SetModifierButtonState(btnWin, (shortcut & Keys.LWin) == Keys.LWin);

            // キー部分の設定
            _selectedKey = shortcut & Keys.KeyCode;
            if (_selectedKey != Keys.None)
            {
                txtKey.Text = _selectedKey.ToString();
            }
        }

        private void SetModifierButtonState(Button button, bool isSelected)
        {
            button.BackColor = isSelected ? Color.LightSkyBlue : SystemColors.Control;
            button.FlatAppearance.BorderColor = isSelected ? Color.DodgerBlue : Color.Gray;
        }

        private void ModifierButton_Click(object? sender, EventArgs e)
        {
            if (sender is Button btn)
            {
                bool isSelected = btn.BackColor != Color.LightSkyBlue;
                SetModifierButtonState(btn, isSelected);
                UpdateShortcutDisplay();
            }
        }

        private void TxtKey_KeyDown(object? sender, KeyEventArgs e)
        {
            e.SuppressKeyPress = true;

            // 修飾キーのみの場合は無視
            if (IsModifierKey(e.KeyCode)) return;

            _selectedKey = e.KeyCode;
            txtKey.Text = _selectedKey.ToString();
            UpdateShortcutDisplay();
        }

        private void TxtKey_KeyUp(object? sender, KeyEventArgs e)
        {
            e.SuppressKeyPress = true;
        }

        private bool IsModifierKey(Keys key)
        {
            return key == Keys.ControlKey ||
                   key == Keys.Alt ||
                   key == Keys.ShiftKey ||
                   key == Keys.LWin ||
                   key == Keys.RWin;
        }

        private void UpdateShortcutDisplay()
        {
            _shortcutKey = Keys.None;
            if (btnCtrl.BackColor == Color.LightSkyBlue) _shortcutKey |= Keys.Control;
            if (btnAlt.BackColor == Color.LightSkyBlue) _shortcutKey |= Keys.Alt;
            if (btnShift.BackColor == Color.LightSkyBlue) _shortcutKey |= Keys.Shift;
            if (btnWin.BackColor == Color.LightSkyBlue) _shortcutKey |= Keys.LWin;

            if (_selectedKey != Keys.None)
            {
                _shortcutKey |= _selectedKey;
            }

            lblShortcut.Text = _shortcutKey.ToString();
        }

        private void btnSave_Click(object? sender, EventArgs e)
        {
            if (_shortcutKey != Keys.None)
            {
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                MessageBox.Show("有効なショートカットを設定してください。");
            }
        }
    }
}