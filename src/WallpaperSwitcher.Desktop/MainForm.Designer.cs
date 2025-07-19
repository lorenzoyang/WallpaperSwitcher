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
            wallpaperFoldersGroupBox = new GroupBox();
            browseFolder3Button = new Button();
            browseFolder1Button = new Button();
            browseFolder2Button = new Button();
            folder3TextBox = new TextBox();
            folder2TextBox = new TextBox();
            folder1TextBox = new TextBox();
            thirdFolderLabel = new Label();
            secondFolderLabel = new Label();
            firstFolderLabel = new Label();
            currentFolderComboBox = new ComboBox();
            selectFolderLabel = new Label();
            wallpaperSwitchingGroupBox = new GroupBox();
            prevShortCutTextBox = new TextBox();
            prevWallpaperLabel = new Label();
            prevWallpaperButton = new Button();
            nextShortCutTextBox = new TextBox();
            nextWallpaperLabel = new Label();
            nextWallpaperButton = new Button();
            saveButton = new Button();
            wallpaperFoldersGroupBox.SuspendLayout();
            wallpaperSwitchingGroupBox.SuspendLayout();
            SuspendLayout();
            // 
            // wallpaperFoldersGroupBox
            // 
            wallpaperFoldersGroupBox.Controls.Add(browseFolder3Button);
            wallpaperFoldersGroupBox.Controls.Add(browseFolder1Button);
            wallpaperFoldersGroupBox.Controls.Add(browseFolder2Button);
            wallpaperFoldersGroupBox.Controls.Add(folder3TextBox);
            wallpaperFoldersGroupBox.Controls.Add(folder2TextBox);
            wallpaperFoldersGroupBox.Controls.Add(folder1TextBox);
            wallpaperFoldersGroupBox.Controls.Add(thirdFolderLabel);
            wallpaperFoldersGroupBox.Controls.Add(secondFolderLabel);
            wallpaperFoldersGroupBox.Controls.Add(firstFolderLabel);
            wallpaperFoldersGroupBox.Controls.Add(currentFolderComboBox);
            wallpaperFoldersGroupBox.Controls.Add(selectFolderLabel);
            wallpaperFoldersGroupBox.Location = new Point(42, 37);
            wallpaperFoldersGroupBox.Name = "wallpaperFoldersGroupBox";
            wallpaperFoldersGroupBox.Size = new Size(894, 378);
            wallpaperFoldersGroupBox.TabIndex = 0;
            wallpaperFoldersGroupBox.TabStop = false;
            wallpaperFoldersGroupBox.Text = "Wallpaper Folders";
            // 
            // browseFolder3Button
            // 
            browseFolder3Button.Location = new Point(730, 295);
            browseFolder3Button.Name = "browseFolder3Button";
            browseFolder3Button.Size = new Size(150, 39);
            browseFolder3Button.TabIndex = 10;
            browseFolder3Button.Text = "Browse...";
            browseFolder3Button.UseVisualStyleBackColor = true;
            // 
            // browseFolder1Button
            // 
            browseFolder1Button.Location = new Point(730, 142);
            browseFolder1Button.Name = "browseFolder1Button";
            browseFolder1Button.Size = new Size(150, 39);
            browseFolder1Button.TabIndex = 9;
            browseFolder1Button.Text = "Browse...";
            browseFolder1Button.UseVisualStyleBackColor = true;
            browseFolder1Button.Click += button2_Click;
            // 
            // browseFolder2Button
            // 
            browseFolder2Button.Location = new Point(730, 216);
            browseFolder2Button.Name = "browseFolder2Button";
            browseFolder2Button.Size = new Size(150, 39);
            browseFolder2Button.TabIndex = 8;
            browseFolder2Button.Text = "Browse...";
            browseFolder2Button.UseVisualStyleBackColor = true;
            // 
            // folder3TextBox
            // 
            folder3TextBox.Location = new Point(210, 295);
            folder3TextBox.Name = "folder3TextBox";
            folder3TextBox.Size = new Size(486, 39);
            folder3TextBox.TabIndex = 7;
            // 
            // folder2TextBox
            // 
            folder2TextBox.Location = new Point(210, 216);
            folder2TextBox.Name = "folder2TextBox";
            folder2TextBox.Size = new Size(486, 39);
            folder2TextBox.TabIndex = 6;
            folder2TextBox.TextChanged += textBox2_TextChanged;
            // 
            // folder1TextBox
            // 
            folder1TextBox.Location = new Point(210, 142);
            folder1TextBox.Name = "folder1TextBox";
            folder1TextBox.Size = new Size(486, 39);
            folder1TextBox.TabIndex = 5;
            folder1TextBox.TextChanged += textBox1_TextChanged;
            // 
            // thirdFolderLabel
            // 
            thirdFolderLabel.AutoSize = true;
            thirdFolderLabel.Location = new Point(27, 302);
            thirdFolderLabel.Name = "thirdFolderLabel";
            thirdFolderLabel.Size = new Size(106, 32);
            thirdFolderLabel.TabIndex = 4;
            thirdFolderLabel.Text = "Folder 3:";
            // 
            // secondFolderLabel
            // 
            secondFolderLabel.AutoSize = true;
            secondFolderLabel.Location = new Point(27, 219);
            secondFolderLabel.Name = "secondFolderLabel";
            secondFolderLabel.Size = new Size(106, 32);
            secondFolderLabel.TabIndex = 3;
            secondFolderLabel.Text = "Folder 2:";
            secondFolderLabel.Click += label2_Click;
            // 
            // firstFolderLabel
            // 
            firstFolderLabel.AutoSize = true;
            firstFolderLabel.Location = new Point(27, 145);
            firstFolderLabel.Name = "firstFolderLabel";
            firstFolderLabel.Size = new Size(106, 32);
            firstFolderLabel.TabIndex = 2;
            firstFolderLabel.Text = "Folder 1:";
            firstFolderLabel.Click += label1_Click;
            // 
            // currentFolderComboBox
            // 
            currentFolderComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            currentFolderComboBox.FormattingEnabled = true;
            currentFolderComboBox.Location = new Point(210, 66);
            currentFolderComboBox.Name = "currentFolderComboBox";
            currentFolderComboBox.Size = new Size(670, 40);
            currentFolderComboBox.TabIndex = 1;
            // 
            // selectFolderLabel
            // 
            selectFolderLabel.AutoSize = true;
            selectFolderLabel.Location = new Point(27, 66);
            selectFolderLabel.Name = "selectFolderLabel";
            selectFolderLabel.Size = new Size(152, 32);
            selectFolderLabel.TabIndex = 0;
            selectFolderLabel.Text = "Select Folder";
            // 
            // wallpaperSwitchingGroupBox
            // 
            wallpaperSwitchingGroupBox.Controls.Add(prevShortCutTextBox);
            wallpaperSwitchingGroupBox.Controls.Add(prevWallpaperLabel);
            wallpaperSwitchingGroupBox.Controls.Add(prevWallpaperButton);
            wallpaperSwitchingGroupBox.Controls.Add(nextShortCutTextBox);
            wallpaperSwitchingGroupBox.Controls.Add(nextWallpaperLabel);
            wallpaperSwitchingGroupBox.Controls.Add(nextWallpaperButton);
            wallpaperSwitchingGroupBox.Location = new Point(42, 447);
            wallpaperSwitchingGroupBox.Name = "wallpaperSwitchingGroupBox";
            wallpaperSwitchingGroupBox.Size = new Size(894, 234);
            wallpaperSwitchingGroupBox.TabIndex = 1;
            wallpaperSwitchingGroupBox.TabStop = false;
            wallpaperSwitchingGroupBox.Text = "Wallpaper Switching";
            // 
            // prevShortCutTextBox
            // 
            prevShortCutTextBox.Location = new Point(453, 154);
            prevShortCutTextBox.Name = "prevShortCutTextBox";
            prevShortCutTextBox.Size = new Size(427, 39);
            prevShortCutTextBox.TabIndex = 5;
            // 
            // prevWallpaperLabel
            // 
            prevWallpaperLabel.AutoSize = true;
            prevWallpaperLabel.Location = new Point(313, 160);
            prevWallpaperLabel.Name = "prevWallpaperLabel";
            prevWallpaperLabel.Size = new Size(109, 32);
            prevWallpaperLabel.TabIndex = 4;
            prevWallpaperLabel.Text = "Shortcut:";
            // 
            // prevWallpaperButton
            // 
            prevWallpaperButton.Location = new Point(27, 154);
            prevWallpaperButton.Name = "prevWallpaperButton";
            prevWallpaperButton.Size = new Size(244, 44);
            prevWallpaperButton.TabIndex = 3;
            prevWallpaperButton.Text = "Previous Wallpaper";
            prevWallpaperButton.UseVisualStyleBackColor = true;
            prevWallpaperButton.Click += button1_Click;
            // 
            // nextShortCutTextBox
            // 
            nextShortCutTextBox.Location = new Point(453, 67);
            nextShortCutTextBox.Name = "nextShortCutTextBox";
            nextShortCutTextBox.Size = new Size(427, 39);
            nextShortCutTextBox.TabIndex = 2;
            // 
            // nextWallpaperLabel
            // 
            nextWallpaperLabel.AutoSize = true;
            nextWallpaperLabel.Location = new Point(313, 70);
            nextWallpaperLabel.Name = "nextWallpaperLabel";
            nextWallpaperLabel.Size = new Size(109, 32);
            nextWallpaperLabel.TabIndex = 1;
            nextWallpaperLabel.Text = "Shortcut:";
            nextWallpaperLabel.Click += label1_Click_1;
            // 
            // nextWallpaperButton
            // 
            nextWallpaperButton.Location = new Point(27, 64);
            nextWallpaperButton.Name = "nextWallpaperButton";
            nextWallpaperButton.Size = new Size(244, 44);
            nextWallpaperButton.TabIndex = 0;
            nextWallpaperButton.Text = "Next Wallpaper";
            nextWallpaperButton.UseVisualStyleBackColor = true;
            nextWallpaperButton.Click += nextWallpaperButton_Click;
            // 
            // saveButton
            // 
            saveButton.Location = new Point(743, 709);
            saveButton.Name = "saveButton";
            saveButton.Size = new Size(193, 46);
            saveButton.TabIndex = 2;
            saveButton.Text = "Save";
            saveButton.UseVisualStyleBackColor = true;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(13F, 32F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(974, 785);
            Controls.Add(saveButton);
            Controls.Add(wallpaperSwitchingGroupBox);
            Controls.Add(wallpaperFoldersGroupBox);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            Name = "MainForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Wallpaper Switcher";
            wallpaperFoldersGroupBox.ResumeLayout(false);
            wallpaperFoldersGroupBox.PerformLayout();
            wallpaperSwitchingGroupBox.ResumeLayout(false);
            wallpaperSwitchingGroupBox.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private GroupBox wallpaperFoldersGroupBox;
        private ComboBox currentFolderComboBox;
        private Label thirdFolderLabel;
        private Label secondFolderLabel;
        private Label firstFolderLabel;
        private Label selectFolderLabel;
        private TextBox folder3TextBox;
        private TextBox folder2TextBox;
        private TextBox folder1TextBox;
        private Button browseFolder3Button;
        private Button browseFolder1Button;
        private Button browseFolder2Button;
        private GroupBox wallpaperSwitchingGroupBox;
        private TextBox nextShortCutTextBox;
        private Label nextWallpaperLabel;
        private Button nextWallpaperButton;
        private Button prevWallpaperButton;
        private TextBox prevShortCutTextBox;
        private Label prevWallpaperLabel;
        private Button saveButton;
    }
}
