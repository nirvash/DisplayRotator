namespace DisplayRotator
{
    partial class ShortcutForm
    {
        private System.ComponentModel.IContainer components = null;
        private Button btnCtrl;
        private Button btnAlt;
        private Button btnShift;
        private Button btnWin;
        private TextBox txtKey;
        private Label lblShortcut;
        private Button btnSave;
        private FlowLayoutPanel modifierPanel;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();

            // Initialize controls
            btnCtrl = new Button();
            btnAlt = new Button();
            btnShift = new Button();
            btnWin = new Button();
            txtKey = new TextBox();
            lblShortcut = new Label();
            btnSave = new Button();
            modifierPanel = new FlowLayoutPanel();

            // Form settings
            this.Text = "ショートカット設定";
            this.Size = new System.Drawing.Size(400, 200);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;

            // Modifier panel setup
            modifierPanel.FlowDirection = FlowDirection.LeftToRight;
            modifierPanel.Location = new System.Drawing.Point(12, 12);
            modifierPanel.Size = new System.Drawing.Size(370, 35);
            modifierPanel.WrapContents = false;

            // Setup modifier buttons
            var modifiers = new[] {
                (btn: btnCtrl, text: "Ctrl"),
                (btn: btnAlt, text: "Alt"),
                (btn: btnShift, text: "Shift"),
                (btn: btnWin, text: "Win")
            };

            foreach (var (btn, text) in modifiers)
            {
                btn.Size = new System.Drawing.Size(80, 30);
                btn.Text = text;
                btn.FlatStyle = FlatStyle.System;
                modifierPanel.Controls.Add(btn);
            }

            // Key input textbox setup
            txtKey.Location = new System.Drawing.Point(12, 55);
            txtKey.Size = new System.Drawing.Size(370, 23);
            txtKey.ReadOnly = true;
            txtKey.Text = "キーを押してください";

            // Shortcut label setup
            lblShortcut.AutoSize = true;
            lblShortcut.Location = new System.Drawing.Point(12, 90);

            // Save button setup
            btnSave.Text = "保存";
            btnSave.Location = new System.Drawing.Point(302, 120);
            btnSave.Size = new System.Drawing.Size(80, 30);

            // Add controls to form
            this.Controls.AddRange(new Control[] {
                modifierPanel, txtKey, lblShortcut, btnSave
            });

            // Wire up events
            btnCtrl.Click += ModifierButton_Click;
            btnAlt.Click += ModifierButton_Click;
            btnShift.Click += ModifierButton_Click;
            btnWin.Click += ModifierButton_Click;
            txtKey.KeyDown += TxtKey_KeyDown;
            txtKey.KeyUp += TxtKey_KeyUp;
            btnSave.Click += btnSave_Click;
        }
    }
}