namespace WallpaperSwitcher.Desktop
{
    partial class SettingsForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SettingsForm));
            nextWallpaperHk = new Label();
            nextWallpaperHkTextBox = new TextBox();
            nextWallpaperHkModifyButton = new Button();
            nextWallpaperHkSaveButton = new Button();
            folderHkComboBox = new ComboBox();
            folderHkTextBox = new TextBox();
            folderHkModifyButton = new Button();
            folderHkSaveButton = new Button();
            folderHkLabel = new Label();
            settingsFormSaveButton = new Button();
            settingsFormCancelButton = new Button();
            showNotificationCheckBox = new CheckBox();
            launchStartupCheckBox = new CheckBox();
            SuspendLayout();
            // 
            // nextWallpaperHk
            // 
            nextWallpaperHk.AutoSize = true;
            nextWallpaperHk.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            nextWallpaperHk.Location = new Point(37, 37);
            nextWallpaperHk.Name = "nextWallpaperHk";
            nextWallpaperHk.Size = new Size(269, 32);
            nextWallpaperHk.TabIndex = 0;
            nextWallpaperHk.Text = "Next Wallpaper Hotkey";
            // 
            // nextWallpaperHkTextBox
            // 
            nextWallpaperHkTextBox.Location = new Point(312, 34);
            nextWallpaperHkTextBox.Name = "nextWallpaperHkTextBox";
            nextWallpaperHkTextBox.ReadOnly = true;
            nextWallpaperHkTextBox.Size = new Size(357, 39);
            nextWallpaperHkTextBox.TabIndex = 1;
            // 
            // nextWallpaperHkModifyButton
            // 
            nextWallpaperHkModifyButton.Location = new Point(697, 34);
            nextWallpaperHkModifyButton.Name = "nextWallpaperHkModifyButton";
            nextWallpaperHkModifyButton.Size = new Size(118, 42);
            nextWallpaperHkModifyButton.TabIndex = 2;
            nextWallpaperHkModifyButton.Text = "Modify";
            nextWallpaperHkModifyButton.UseVisualStyleBackColor = true;
            // 
            // nextWallpaperHkSaveButton
            // 
            nextWallpaperHkSaveButton.Location = new Point(859, 37);
            nextWallpaperHkSaveButton.Name = "nextWallpaperHkSaveButton";
            nextWallpaperHkSaveButton.Size = new Size(118, 42);
            nextWallpaperHkSaveButton.TabIndex = 3;
            nextWallpaperHkSaveButton.Text = "Save";
            nextWallpaperHkSaveButton.UseVisualStyleBackColor = true;
            nextWallpaperHkSaveButton.Click += button2_Click;
            // 
            // folderHkComboBox
            // 
            folderHkComboBox.FormattingEnabled = true;
            folderHkComboBox.Location = new Point(235, 123);
            folderHkComboBox.Name = "folderHkComboBox";
            folderHkComboBox.Size = new Size(742, 40);
            folderHkComboBox.TabIndex = 4;
            // 
            // folderHkTextBox
            // 
            folderHkTextBox.Location = new Point(37, 193);
            folderHkTextBox.Name = "folderHkTextBox";
            folderHkTextBox.ReadOnly = true;
            folderHkTextBox.Size = new Size(632, 39);
            folderHkTextBox.TabIndex = 5;
            // 
            // folderHkModifyButton
            // 
            folderHkModifyButton.Location = new Point(697, 193);
            folderHkModifyButton.Name = "folderHkModifyButton";
            folderHkModifyButton.Size = new Size(118, 42);
            folderHkModifyButton.TabIndex = 6;
            folderHkModifyButton.Text = "Modify";
            folderHkModifyButton.UseVisualStyleBackColor = true;
            // 
            // folderHkSaveButton
            // 
            folderHkSaveButton.Location = new Point(859, 191);
            folderHkSaveButton.Name = "folderHkSaveButton";
            folderHkSaveButton.Size = new Size(118, 42);
            folderHkSaveButton.TabIndex = 7;
            folderHkSaveButton.Text = "Save";
            folderHkSaveButton.UseVisualStyleBackColor = true;
            folderHkSaveButton.Click += button2_Click_1;
            // 
            // folderHkLabel
            // 
            folderHkLabel.AutoSize = true;
            folderHkLabel.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            folderHkLabel.Location = new Point(37, 123);
            folderHkLabel.Name = "folderHkLabel";
            folderHkLabel.Size = new Size(168, 32);
            folderHkLabel.TabIndex = 8;
            folderHkLabel.Text = "Folder Hotkey\r\n";
            // 
            // settingsFormSaveButton
            // 
            settingsFormSaveButton.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            settingsFormSaveButton.Location = new Point(669, 318);
            settingsFormSaveButton.Name = "settingsFormSaveButton";
            settingsFormSaveButton.Size = new Size(146, 46);
            settingsFormSaveButton.TabIndex = 9;
            settingsFormSaveButton.Text = "Save All";
            settingsFormSaveButton.UseVisualStyleBackColor = true;
            // 
            // settingsFormCancelButton
            // 
            settingsFormCancelButton.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            settingsFormCancelButton.Location = new Point(831, 318);
            settingsFormCancelButton.Name = "settingsFormCancelButton";
            settingsFormCancelButton.Size = new Size(146, 46);
            settingsFormCancelButton.TabIndex = 10;
            settingsFormCancelButton.Text = "Cancel";
            settingsFormCancelButton.UseVisualStyleBackColor = true;
            // 
            // showNotificationCheckBox
            // 
            showNotificationCheckBox.AutoSize = true;
            showNotificationCheckBox.Location = new Point(37, 368);
            showNotificationCheckBox.Name = "showNotificationCheckBox";
            showNotificationCheckBox.Size = new Size(495, 36);
            showNotificationCheckBox.TabIndex = 11;
            showNotificationCheckBox.Text = "Show notification on first minimize to tray";
            showNotificationCheckBox.UseVisualStyleBackColor = true;
            // 
            // launchStartupCheckBox
            // 
            launchStartupCheckBox.AutoSize = true;
            launchStartupCheckBox.Location = new Point(37, 281);
            launchStartupCheckBox.Name = "launchStartupCheckBox";
            launchStartupCheckBox.Size = new Size(230, 36);
            launchStartupCheckBox.TabIndex = 12;
            launchStartupCheckBox.Text = "Launch at startup";
            launchStartupCheckBox.UseVisualStyleBackColor = true;
            // 
            // SettingsForm
            // 
            AutoScaleDimensions = new SizeF(13F, 32F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1029, 450);
            Controls.Add(launchStartupCheckBox);
            Controls.Add(showNotificationCheckBox);
            Controls.Add(settingsFormCancelButton);
            Controls.Add(settingsFormSaveButton);
            Controls.Add(folderHkLabel);
            Controls.Add(folderHkSaveButton);
            Controls.Add(folderHkModifyButton);
            Controls.Add(folderHkTextBox);
            Controls.Add(folderHkComboBox);
            Controls.Add(nextWallpaperHkSaveButton);
            Controls.Add(nextWallpaperHkModifyButton);
            Controls.Add(nextWallpaperHkTextBox);
            Controls.Add(nextWallpaperHk);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "SettingsForm";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Wallpaper Switcher Settings";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label nextWallpaperHk;
        private TextBox nextWallpaperHkTextBox;
        private Button nextWallpaperHkModifyButton;
        private Button nextWallpaperHkSaveButton;
        private ComboBox folderHkComboBox;
        private TextBox folderHkTextBox;
        private Button folderHkModifyButton;
        private Button folderHkSaveButton;
        private Label folderHkLabel;
        private Button settingsFormSaveButton;
        private Button settingsFormCancelButton;
        private CheckBox showNotificationCheckBox;
        private CheckBox launchStartupCheckBox;
    }
}