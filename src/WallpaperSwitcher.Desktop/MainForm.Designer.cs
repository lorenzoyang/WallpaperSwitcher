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
            wallpaperSwitchingGroupBox = new GroupBox();
            prevWallpaperButton = new Button();
            nextWallpaperButton = new Button();
            foldersManagementGroupBox.SuspendLayout();
            wallpaperSwitchingGroupBox.SuspendLayout();
            SuspendLayout();
            // 
            // foldersManagementGroupBox
            // 
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
            foldersManagementGroupBox.Location = new Point(54, 48);
            foldersManagementGroupBox.Name = "foldersManagementGroupBox";
            foldersManagementGroupBox.Size = new Size(705, 536);
            foldersManagementGroupBox.TabIndex = 0;
            foldersManagementGroupBox.TabStop = false;
            foldersManagementGroupBox.Text = "Folder Management";
            // 
            // removeFolderComboBox
            // 
            removeFolderComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            removeFolderComboBox.FormattingEnabled = true;
            removeFolderComboBox.Location = new Point(27, 431);
            removeFolderComboBox.Name = "removeFolderComboBox";
            removeFolderComboBox.Size = new Size(624, 40);
            removeFolderComboBox.TabIndex = 14;
            removeFolderComboBox.SelectedIndexChanged += removeFolderComboBox_SelectedIndexChanged;
            removeFolderComboBox.MouseEnter += removeFolderComboBox_MouseEnter;
            // 
            // removeFolderButton
            // 
            removeFolderButton.Enabled = false;
            removeFolderButton.Location = new Point(353, 359);
            removeFolderButton.Name = "removeFolderButton";
            removeFolderButton.Size = new Size(134, 40);
            removeFolderButton.TabIndex = 13;
            removeFolderButton.Text = "Remove";
            removeFolderButton.UseVisualStyleBackColor = true;
            removeFolderButton.Click += removeFolderButton_Click;
            // 
            // removeFolderLabel
            // 
            removeFolderLabel.AutoSize = true;
            removeFolderLabel.Location = new Point(27, 367);
            removeFolderLabel.Name = "removeFolderLabel";
            removeFolderLabel.Size = new Size(274, 32);
            removeFolderLabel.TabIndex = 12;
            removeFolderLabel.Text = "Select Folder to Remove";
            // 
            // addFolderLabel
            // 
            addFolderLabel.AutoSize = true;
            addFolderLabel.Location = new Point(27, 203);
            addFolderLabel.Name = "addFolderLabel";
            addFolderLabel.Size = new Size(239, 32);
            addFolderLabel.TabIndex = 11;
            addFolderLabel.Text = "Add New Folder Path";
            // 
            // addFolderButton
            // 
            addFolderButton.Enabled = false;
            addFolderButton.Location = new Point(517, 268);
            addFolderButton.Name = "addFolderButton";
            addFolderButton.Size = new Size(134, 40);
            addFolderButton.TabIndex = 10;
            addFolderButton.Text = "Add";
            addFolderButton.UseVisualStyleBackColor = true;
            addFolderButton.Click += addFolderButton_Click;
            // 
            // browseFolderButton
            // 
            browseFolderButton.Location = new Point(353, 203);
            browseFolderButton.Name = "browseFolderButton";
            browseFolderButton.Size = new Size(134, 39);
            browseFolderButton.TabIndex = 9;
            browseFolderButton.Text = "Browse...";
            browseFolderButton.UseVisualStyleBackColor = true;
            browseFolderButton.Click += browseFolderButton_Click;
            // 
            // addFolderTextBox
            // 
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
            secondFolderLabel.Size = new Size(0, 32);
            secondFolderLabel.TabIndex = 3;
            // 
            // currentFolderComboBox
            // 
            currentFolderComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
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
            currentFolderLabel.Location = new Point(27, 66);
            currentFolderLabel.Name = "currentFolderLabel";
            currentFolderLabel.Size = new Size(281, 32);
            currentFolderLabel.TabIndex = 0;
            currentFolderLabel.Text = "Current Wallpaper Folder";
            // 
            // wallpaperSwitchingGroupBox
            // 
            wallpaperSwitchingGroupBox.Controls.Add(prevWallpaperButton);
            wallpaperSwitchingGroupBox.Controls.Add(nextWallpaperButton);
            wallpaperSwitchingGroupBox.Location = new Point(54, 639);
            wallpaperSwitchingGroupBox.Name = "wallpaperSwitchingGroupBox";
            wallpaperSwitchingGroupBox.Size = new Size(705, 132);
            wallpaperSwitchingGroupBox.TabIndex = 1;
            wallpaperSwitchingGroupBox.TabStop = false;
            wallpaperSwitchingGroupBox.Text = "Wallpaper Switching";
            // 
            // prevWallpaperButton
            // 
            prevWallpaperButton.Enabled = false;
            prevWallpaperButton.Location = new Point(27, 49);
            prevWallpaperButton.Name = "prevWallpaperButton";
            prevWallpaperButton.Size = new Size(260, 52);
            prevWallpaperButton.TabIndex = 3;
            prevWallpaperButton.Text = "Previous Wallpaper";
            prevWallpaperButton.UseVisualStyleBackColor = true;
            prevWallpaperButton.Click += prevWallpaperButton_Click;
            // 
            // nextWallpaperButton
            // 
            nextWallpaperButton.Enabled = false;
            nextWallpaperButton.Location = new Point(391, 49);
            nextWallpaperButton.Name = "nextWallpaperButton";
            nextWallpaperButton.Size = new Size(260, 52);
            nextWallpaperButton.TabIndex = 0;
            nextWallpaperButton.Text = "Next Wallpaper";
            nextWallpaperButton.UseVisualStyleBackColor = true;
            nextWallpaperButton.Click += nextWallpaperButton_Click;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(13F, 32F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(814, 883);
            Controls.Add(wallpaperSwitchingGroupBox);
            Controls.Add(foldersManagementGroupBox);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            Name = "MainForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Wallpaper Switcher";
            FormClosing += MainForm_FormClosing;
            Load += MainForm_Load;
            foldersManagementGroupBox.ResumeLayout(false);
            foldersManagementGroupBox.PerformLayout();
            wallpaperSwitchingGroupBox.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private GroupBox foldersManagementGroupBox;
        private ComboBox currentFolderComboBox;
        private Label thirdFolderLabel;
        private Label secondFolderLabel;
        private Label currentFolderLabel;
        private TextBox folder3TextBox;
        private TextBox folder2TextBox;
        private Button browseFolder3Button;
        private Button browseFolderButton;
        private Button browseFolder2Button;
        private GroupBox wallpaperSwitchingGroupBox;
        private Button nextWallpaperButton;
        private Button prevWallpaperButton;
        private Button addFolderButton;
        private Label addFolderLabel;
        private Label removeFolderLabel;
        private ComboBox removeFolderComboBox;
        private Button removeFolderButton;
        private TextBox addFolderTextBox;
    }
}
