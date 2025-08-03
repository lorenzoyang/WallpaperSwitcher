namespace WallpaperSwitcher.Desktop
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
                _toolTip.Dispose();
                _trayIcon.Dispose();
                _globalHotkeyManager.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            foldersManagementGroupBox = new GroupBox();
            removeFolderComboBox = new ComboBox();
            removeFolderButton = new Button();
            removeFolderLabel = new Label();
            addFolderLabel = new Label();
            addFolderButton = new Button();
            browseFolderButton = new Button();
            addFolderTextBox = new TextBox();
            secondFolderLabel = new Label();
            currentFolderComboBox = new ComboBox();
            currentFolderLabel = new Label();
            nextWallpaperButton = new Button();
            modeComboBox = new ComboBox();
            foldersManagementGroupBox.SuspendLayout();
            SuspendLayout();
            // 
            // foldersManagementGroupBox
            // 
            foldersManagementGroupBox.BackColor = SystemColors.Window;
            foldersManagementGroupBox.Controls.Add(removeFolderComboBox);
            foldersManagementGroupBox.Controls.Add(removeFolderButton);
            foldersManagementGroupBox.Controls.Add(removeFolderLabel);
            foldersManagementGroupBox.Controls.Add(addFolderLabel);
            foldersManagementGroupBox.Controls.Add(addFolderButton);
            foldersManagementGroupBox.Controls.Add(browseFolderButton);
            foldersManagementGroupBox.Controls.Add(addFolderTextBox);
            foldersManagementGroupBox.Controls.Add(secondFolderLabel);
            foldersManagementGroupBox.Controls.Add(currentFolderComboBox);
            foldersManagementGroupBox.Controls.Add(currentFolderLabel);
            foldersManagementGroupBox.Font = new Font("Segoe UI", 10.875F, FontStyle.Bold, GraphicsUnit.Point, 0);
            foldersManagementGroupBox.Location = new Point(54, 48);
            foldersManagementGroupBox.Name = "foldersManagementGroupBox";
            foldersManagementGroupBox.Size = new Size(705, 536);
            foldersManagementGroupBox.TabIndex = 0;
            foldersManagementGroupBox.TabStop = false;
            foldersManagementGroupBox.Text = "Folder Management";
            // 
            // removeFolderComboBox
            // 
            removeFolderComboBox.BackColor = SystemColors.ControlLight;
            removeFolderComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            removeFolderComboBox.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            removeFolderComboBox.FormattingEnabled = true;
            removeFolderComboBox.Location = new Point(27, 426);
            removeFolderComboBox.Name = "removeFolderComboBox";
            removeFolderComboBox.Size = new Size(460, 40);
            removeFolderComboBox.TabIndex = 14;
            removeFolderComboBox.SelectedIndexChanged += removeFolderComboBox_SelectedIndexChanged;
            removeFolderComboBox.MouseEnter += removeFolderComboBox_MouseEnter;
            // 
            // removeFolderButton
            // 
            removeFolderButton.BackColor = SystemColors.ControlLightLight;
            removeFolderButton.Enabled = false;
            removeFolderButton.Font = new Font("Segoe UI", 10.125F, FontStyle.Regular, GraphicsUnit.Point, 0);
            removeFolderButton.Location = new Point(517, 425);
            removeFolderButton.Name = "removeFolderButton";
            removeFolderButton.Size = new Size(134, 41);
            removeFolderButton.TabIndex = 13;
            removeFolderButton.Text = "Remove";
            removeFolderButton.UseVisualStyleBackColor = false;
            removeFolderButton.Click += removeFolderButton_Click;
            // 
            // removeFolderLabel
            // 
            removeFolderLabel.AutoSize = true;
            removeFolderLabel.Font = new Font("Segoe UI Semibold", 10.875F, FontStyle.Bold, GraphicsUnit.Point, 0);
            removeFolderLabel.Location = new Point(27, 367);
            removeFolderLabel.Name = "removeFolderLabel";
            removeFolderLabel.Size = new Size(334, 40);
            removeFolderLabel.TabIndex = 12;
            removeFolderLabel.Text = "Select Folder to Remove";
            // 
            // addFolderLabel
            // 
            addFolderLabel.AutoSize = true;
            addFolderLabel.Font = new Font("Segoe UI Semibold", 10.875F, FontStyle.Bold, GraphicsUnit.Point, 0);
            addFolderLabel.Location = new Point(27, 203);
            addFolderLabel.Name = "addFolderLabel";
            addFolderLabel.Size = new Size(294, 40);
            addFolderLabel.TabIndex = 11;
            addFolderLabel.Text = "Add New Folder Path";
            // 
            // addFolderButton
            // 
            addFolderButton.BackColor = SystemColors.ControlLightLight;
            addFolderButton.Enabled = false;
            addFolderButton.Font = new Font("Segoe UI", 10.125F, FontStyle.Regular, GraphicsUnit.Point, 0);
            addFolderButton.Location = new Point(517, 268);
            addFolderButton.Name = "addFolderButton";
            addFolderButton.Size = new Size(134, 39);
            addFolderButton.TabIndex = 10;
            addFolderButton.Text = "Add";
            addFolderButton.UseVisualStyleBackColor = false;
            addFolderButton.Click += addFolderButton_Click;
            // 
            // browseFolderButton
            // 
            browseFolderButton.BackColor = SystemColors.ControlLightLight;
            browseFolderButton.Font = new Font("Segoe UI", 10.125F, FontStyle.Regular, GraphicsUnit.Point, 0);
            browseFolderButton.Location = new Point(353, 203);
            browseFolderButton.Name = "browseFolderButton";
            browseFolderButton.Size = new Size(134, 40);
            browseFolderButton.TabIndex = 9;
            browseFolderButton.Text = "Browse...";
            browseFolderButton.UseVisualStyleBackColor = false;
            browseFolderButton.Click += browseFolderButton_Click;
            // 
            // addFolderTextBox
            // 
            addFolderTextBox.BackColor = SystemColors.ControlLightLight;
            addFolderTextBox.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            addFolderTextBox.Location = new Point(27, 268);
            addFolderTextBox.Name = "addFolderTextBox";
            addFolderTextBox.ReadOnly = true;
            addFolderTextBox.Size = new Size(460, 39);
            addFolderTextBox.TabIndex = 5;
            addFolderTextBox.TextChanged += addFolderTextBox_TextChanged;
            // 
            // secondFolderLabel
            // 
            secondFolderLabel.AutoSize = true;
            secondFolderLabel.Location = new Point(27, 219);
            secondFolderLabel.Name = "secondFolderLabel";
            secondFolderLabel.Size = new Size(0, 40);
            secondFolderLabel.TabIndex = 3;
            // 
            // currentFolderComboBox
            // 
            currentFolderComboBox.BackColor = SystemColors.ControlLight;
            currentFolderComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            currentFolderComboBox.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            currentFolderComboBox.FormattingEnabled = true;
            currentFolderComboBox.Location = new Point(27, 115);
            currentFolderComboBox.Name = "currentFolderComboBox";
            currentFolderComboBox.Size = new Size(624, 40);
            currentFolderComboBox.TabIndex = 1;
            currentFolderComboBox.SelectedIndexChanged += currentFolderComboBox_SelectedIndexChanged;
            currentFolderComboBox.MouseEnter += currentFolderComboBox_MouseEnter;
            // 
            // currentFolderLabel
            // 
            currentFolderLabel.AutoSize = true;
            currentFolderLabel.Font = new Font("Segoe UI Semibold", 10.875F, FontStyle.Bold, GraphicsUnit.Point, 0);
            currentFolderLabel.Location = new Point(27, 66);
            currentFolderLabel.Name = "currentFolderLabel";
            currentFolderLabel.Size = new Size(348, 40);
            currentFolderLabel.TabIndex = 0;
            currentFolderLabel.Text = "Current Wallpaper Folder";
            // 
            // nextWallpaperButton
            // 
            nextWallpaperButton.BackColor = SystemColors.ControlLightLight;
            nextWallpaperButton.Enabled = false;
            nextWallpaperButton.Font = new Font("Segoe UI Semibold", 10.125F, FontStyle.Bold, GraphicsUnit.Point, 0);
            nextWallpaperButton.Location = new Point(522, 637);
            nextWallpaperButton.Name = "nextWallpaperButton";
            nextWallpaperButton.Size = new Size(237, 52);
            nextWallpaperButton.TabIndex = 0;
            nextWallpaperButton.Text = "Next Wallpaper";
            nextWallpaperButton.UseVisualStyleBackColor = false;
            nextWallpaperButton.Click += nextWallpaperButton_Click;
            // 
            // modeComboBox
            // 
            modeComboBox.BackColor = SystemColors.ControlLight;
            modeComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            modeComboBox.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            modeComboBox.FormattingEnabled = true;
            modeComboBox.Items.AddRange(new object[] { "Native Mode (System SlideShow)", "Custom Mode (Fast Switching)" });
            modeComboBox.Location = new Point(54, 644);
            modeComboBox.Name = "modeComboBox";
            modeComboBox.Size = new Size(394, 40);
            modeComboBox.TabIndex = 1;
            modeComboBox.SelectedIndexChanged += modeComboBox_SelectedIndexChanged;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(13F, 32F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.Window;
            ClientSize = new Size(814, 762);
            Controls.Add(modeComboBox);
            Controls.Add(nextWallpaperButton);
            Controls.Add(foldersManagementGroupBox);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            Name = "MainForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Wallpaper Switcher";
            FormClosing += MainForm_FormClosing;
            Load += MainForm_Load;
            foldersManagementGroupBox.ResumeLayout(false);
            foldersManagementGroupBox.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private GroupBox foldersManagementGroupBox;
        private ComboBox currentFolderComboBox;
        private Label secondFolderLabel;
        private Label currentFolderLabel;
        private Button browseFolderButton;
        private Button nextWallpaperButton;
        private Button addFolderButton;
        private Label addFolderLabel;
        private Label removeFolderLabel;
        private ComboBox removeFolderComboBox;
        private Button removeFolderButton;
        private TextBox addFolderTextBox;
        private ComboBox modeComboBox;
    }
}
