namespace ERMapGenerator
{
    partial class ERMapGenerator
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ERMapGenerator));
            copyrightInfoStr = new Label();
            versionStr = new Label();
            mapConfigurationGroupBox = new GroupBox();
            label3 = new Label();
            zoomLevelComboBox = new ComboBox();
            label2 = new Label();
            groundLevelComboBox = new ComboBox();
            progressLabel = new Label();
            label4 = new Label();
            automateButton = new Button();
            automationModeTabControl = new TabControl();
            tabPage2 = new TabPage();
            drawTileDebugInfoCheckBox = new CheckBox();
            outputFolderGroupBox = new GroupBox();
            label7 = new Label();
            outputFolderPathLabel = new Label();
            outputFolderButton = new Button();
            gameModFolderGroupBox = new GroupBox();
            label5 = new Label();
            gameModFolderPathLabel = new Label();
            browseGameModFolderButton = new Button();
            tabPage1 = new TabPage();
            mapDisplayGroupBox = new GroupBox();
            mapDisplayOpenMapImageLabel = new Label();
            mapDisplayPictureBox = new PictureBox();
            mapImageGroupBox = new GroupBox();
            label1 = new Label();
            mapImageFilePathLabel = new Label();
            browseMapImageButton = new Button();
            mapConfigurationGroupBox.SuspendLayout();
            automationModeTabControl.SuspendLayout();
            tabPage2.SuspendLayout();
            outputFolderGroupBox.SuspendLayout();
            gameModFolderGroupBox.SuspendLayout();
            tabPage1.SuspendLayout();
            mapDisplayGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)mapDisplayPictureBox).BeginInit();
            mapImageGroupBox.SuspendLayout();
            SuspendLayout();
            // 
            // copyrightInfoStr
            // 
            copyrightInfoStr.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            copyrightInfoStr.AutoSize = true;
            copyrightInfoStr.ForeColor = Color.Gray;
            copyrightInfoStr.Location = new Point(393, 6);
            copyrightInfoStr.Margin = new Padding(2, 0, 2, 0);
            copyrightInfoStr.Name = "copyrightInfoStr";
            copyrightInfoStr.Size = new Size(174, 15);
            copyrightInfoStr.TabIndex = 1;
            copyrightInfoStr.Text = "© Pear, 2024 All rights reserved.";
            // 
            // versionStr
            // 
            versionStr.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            versionStr.AutoSize = true;
            versionStr.ForeColor = Color.Gray;
            versionStr.Location = new Point(330, 6);
            versionStr.Margin = new Padding(2, 0, 2, 0);
            versionStr.Name = "versionStr";
            versionStr.Size = new Size(48, 15);
            versionStr.TabIndex = 11;
            versionStr.Text = "Version:";
            // 
            // mapConfigurationGroupBox
            // 
            mapConfigurationGroupBox.Controls.Add(label3);
            mapConfigurationGroupBox.Controls.Add(zoomLevelComboBox);
            mapConfigurationGroupBox.Controls.Add(label2);
            mapConfigurationGroupBox.Controls.Add(groundLevelComboBox);
            mapConfigurationGroupBox.Location = new Point(12, 24);
            mapConfigurationGroupBox.Name = "mapConfigurationGroupBox";
            mapConfigurationGroupBox.Size = new Size(555, 78);
            mapConfigurationGroupBox.TabIndex = 15;
            mapConfigurationGroupBox.TabStop = false;
            mapConfigurationGroupBox.Text = "Map Configuration";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(138, 25);
            label3.Name = "label3";
            label3.Size = new Size(72, 15);
            label3.TabIndex = 8;
            label3.Text = "Zoom Level:";
            // 
            // zoomLevelComboBox
            // 
            zoomLevelComboBox.FormattingEnabled = true;
            zoomLevelComboBox.Location = new Point(140, 43);
            zoomLevelComboBox.Name = "zoomLevelComboBox";
            zoomLevelComboBox.Size = new Size(125, 23);
            zoomLevelComboBox.TabIndex = 7;
            zoomLevelComboBox.SelectedIndexChanged += zoomLevelComboBox_SelectedIndexChanged;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(6, 25);
            label2.Name = "label2";
            label2.Size = new Size(80, 15);
            label2.TabIndex = 6;
            label2.Text = "Ground Level:";
            // 
            // groundLevelComboBox
            // 
            groundLevelComboBox.FormattingEnabled = true;
            groundLevelComboBox.Location = new Point(9, 43);
            groundLevelComboBox.Name = "groundLevelComboBox";
            groundLevelComboBox.Size = new Size(125, 23);
            groundLevelComboBox.TabIndex = 5;
            groundLevelComboBox.SelectedIndexChanged += GroundLevelComboBox_SelectedIndexChanged;
            // 
            // progressLabel
            // 
            progressLabel.AutoSize = true;
            progressLabel.Location = new Point(49, 582);
            progressLabel.Name = "progressLabel";
            progressLabel.Size = new Size(57, 15);
            progressLabel.TabIndex = 11;
            progressLabel.Text = "Waiting...";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(10, 582);
            label4.Name = "label4";
            label4.Size = new Size(42, 15);
            label4.TabIndex = 10;
            label4.Text = "Status:";
            // 
            // automateButton
            // 
            automateButton.Location = new Point(12, 601);
            automateButton.Name = "automateButton";
            automateButton.Size = new Size(555, 23);
            automateButton.TabIndex = 4;
            automateButton.Text = "Automate!";
            automateButton.UseVisualStyleBackColor = true;
            automateButton.Click += AutomateButton_Click;
            // 
            // automationModeTabControl
            // 
            automationModeTabControl.Controls.Add(tabPage2);
            automationModeTabControl.Controls.Add(tabPage1);
            automationModeTabControl.Location = new Point(12, 106);
            automationModeTabControl.Name = "automationModeTabControl";
            automationModeTabControl.SelectedIndex = 0;
            automationModeTabControl.Size = new Size(555, 473);
            automationModeTabControl.TabIndex = 16;
            automationModeTabControl.SelectedIndexChanged += AutomationModeTabControl_SelectedIndexChanged;
            // 
            // tabPage2
            // 
            tabPage2.Controls.Add(drawTileDebugInfoCheckBox);
            tabPage2.Controls.Add(outputFolderGroupBox);
            tabPage2.Controls.Add(gameModFolderGroupBox);
            tabPage2.Location = new Point(4, 24);
            tabPage2.Name = "tabPage2";
            tabPage2.Padding = new Padding(3);
            tabPage2.Size = new Size(547, 445);
            tabPage2.TabIndex = 1;
            tabPage2.Text = "Unpack/Stitch Map";
            tabPage2.UseVisualStyleBackColor = true;
            // 
            // drawTileDebugInfoCheckBox
            // 
            drawTileDebugInfoCheckBox.AutoSize = true;
            drawTileDebugInfoCheckBox.Location = new Point(4, 157);
            drawTileDebugInfoCheckBox.Name = "drawTileDebugInfoCheckBox";
            drawTileDebugInfoCheckBox.Size = new Size(136, 19);
            drawTileDebugInfoCheckBox.TabIndex = 17;
            drawTileDebugInfoCheckBox.Text = "Draw Tile Debug Info";
            drawTileDebugInfoCheckBox.UseVisualStyleBackColor = true;
            // 
            // outputFolderGroupBox
            // 
            outputFolderGroupBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            outputFolderGroupBox.Controls.Add(label7);
            outputFolderGroupBox.Controls.Add(outputFolderPathLabel);
            outputFolderGroupBox.Controls.Add(outputFolderButton);
            outputFolderGroupBox.Location = new Point(3, 81);
            outputFolderGroupBox.Name = "outputFolderGroupBox";
            outputFolderGroupBox.Size = new Size(541, 72);
            outputFolderGroupBox.TabIndex = 16;
            outputFolderGroupBox.TabStop = false;
            outputFolderGroupBox.Text = "Output Folder";
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new Point(6, 49);
            label7.Name = "label7";
            label7.Size = new Size(34, 15);
            label7.TabIndex = 2;
            label7.Text = "Path:";
            // 
            // outputFolderPathLabel
            // 
            outputFolderPathLabel.AutoSize = true;
            outputFolderPathLabel.Location = new Point(37, 49);
            outputFolderPathLabel.Name = "outputFolderPathLabel";
            outputFolderPathLabel.Size = new Size(29, 15);
            outputFolderPathLabel.TabIndex = 1;
            outputFolderPathLabel.Text = "N/A";
            // 
            // outputFolderButton
            // 
            outputFolderButton.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            outputFolderButton.Location = new Point(6, 23);
            outputFolderButton.Name = "outputFolderButton";
            outputFolderButton.Size = new Size(532, 23);
            outputFolderButton.TabIndex = 0;
            outputFolderButton.Text = "Browse";
            outputFolderButton.UseVisualStyleBackColor = true;
            outputFolderButton.Click += OutputFolderButton_Click;
            // 
            // gameModFolderGroupBox
            // 
            gameModFolderGroupBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            gameModFolderGroupBox.Controls.Add(label5);
            gameModFolderGroupBox.Controls.Add(gameModFolderPathLabel);
            gameModFolderGroupBox.Controls.Add(browseGameModFolderButton);
            gameModFolderGroupBox.Location = new Point(3, 3);
            gameModFolderGroupBox.Name = "gameModFolderGroupBox";
            gameModFolderGroupBox.Size = new Size(541, 72);
            gameModFolderGroupBox.TabIndex = 15;
            gameModFolderGroupBox.TabStop = false;
            gameModFolderGroupBox.Text = "Game/Mod Folder";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(6, 49);
            label5.Name = "label5";
            label5.Size = new Size(34, 15);
            label5.TabIndex = 2;
            label5.Text = "Path:";
            // 
            // gameModFolderPathLabel
            // 
            gameModFolderPathLabel.AutoSize = true;
            gameModFolderPathLabel.Location = new Point(37, 49);
            gameModFolderPathLabel.Name = "gameModFolderPathLabel";
            gameModFolderPathLabel.Size = new Size(29, 15);
            gameModFolderPathLabel.TabIndex = 1;
            gameModFolderPathLabel.Text = "N/A";
            // 
            // browseGameModFolderButton
            // 
            browseGameModFolderButton.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            browseGameModFolderButton.Location = new Point(6, 23);
            browseGameModFolderButton.Name = "browseGameModFolderButton";
            browseGameModFolderButton.Size = new Size(532, 23);
            browseGameModFolderButton.TabIndex = 0;
            browseGameModFolderButton.Text = "Browse";
            browseGameModFolderButton.UseVisualStyleBackColor = true;
            browseGameModFolderButton.Click += BrowseGameModFolderButton_Click;
            // 
            // tabPage1
            // 
            tabPage1.Controls.Add(mapDisplayGroupBox);
            tabPage1.Controls.Add(mapImageGroupBox);
            tabPage1.Location = new Point(4, 24);
            tabPage1.Name = "tabPage1";
            tabPage1.Padding = new Padding(3);
            tabPage1.Size = new Size(547, 445);
            tabPage1.TabIndex = 0;
            tabPage1.Text = "Repack/Tile Map";
            tabPage1.UseVisualStyleBackColor = true;
            // 
            // mapDisplayGroupBox
            // 
            mapDisplayGroupBox.Controls.Add(mapDisplayOpenMapImageLabel);
            mapDisplayGroupBox.Controls.Add(mapDisplayPictureBox);
            mapDisplayGroupBox.Location = new Point(3, 81);
            mapDisplayGroupBox.Name = "mapDisplayGroupBox";
            mapDisplayGroupBox.Size = new Size(541, 361);
            mapDisplayGroupBox.TabIndex = 13;
            mapDisplayGroupBox.TabStop = false;
            mapDisplayGroupBox.Text = "Map Display";
            // 
            // mapDisplayOpenMapImageLabel
            // 
            mapDisplayOpenMapImageLabel.Dock = DockStyle.Fill;
            mapDisplayOpenMapImageLabel.Location = new Point(3, 19);
            mapDisplayOpenMapImageLabel.Name = "mapDisplayOpenMapImageLabel";
            mapDisplayOpenMapImageLabel.Size = new Size(535, 339);
            mapDisplayOpenMapImageLabel.TabIndex = 3;
            mapDisplayOpenMapImageLabel.Text = "Open a map image to view map data...";
            mapDisplayOpenMapImageLabel.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // mapDisplayPictureBox
            // 
            mapDisplayPictureBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            mapDisplayPictureBox.Location = new Point(6, 19);
            mapDisplayPictureBox.Name = "mapDisplayPictureBox";
            mapDisplayPictureBox.Size = new Size(529, 336);
            mapDisplayPictureBox.SizeMode = PictureBoxSizeMode.StretchImage;
            mapDisplayPictureBox.TabIndex = 12;
            mapDisplayPictureBox.TabStop = false;
            mapDisplayPictureBox.MouseDown += MapDisplayPictureBox_MouseDown;
            mapDisplayPictureBox.MouseMove += MapDisplayPictureBox_MouseMove;
            mapDisplayPictureBox.MouseUp += MapDisplayPictureBox_MouseUp;
            // 
            // mapImageGroupBox
            // 
            mapImageGroupBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            mapImageGroupBox.Controls.Add(label1);
            mapImageGroupBox.Controls.Add(mapImageFilePathLabel);
            mapImageGroupBox.Controls.Add(browseMapImageButton);
            mapImageGroupBox.Location = new Point(3, 3);
            mapImageGroupBox.Name = "mapImageGroupBox";
            mapImageGroupBox.Size = new Size(541, 72);
            mapImageGroupBox.TabIndex = 14;
            mapImageGroupBox.TabStop = false;
            mapImageGroupBox.Text = "Map Image";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(6, 49);
            label1.Name = "label1";
            label1.Size = new Size(34, 15);
            label1.TabIndex = 2;
            label1.Text = "Path:";
            // 
            // mapImageFilePathLabel
            // 
            mapImageFilePathLabel.AutoSize = true;
            mapImageFilePathLabel.Location = new Point(37, 49);
            mapImageFilePathLabel.Name = "mapImageFilePathLabel";
            mapImageFilePathLabel.Size = new Size(29, 15);
            mapImageFilePathLabel.TabIndex = 1;
            mapImageFilePathLabel.Text = "N/A";
            // 
            // browseMapImageButton
            // 
            browseMapImageButton.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            browseMapImageButton.Location = new Point(6, 23);
            browseMapImageButton.Name = "browseMapImageButton";
            browseMapImageButton.Size = new Size(529, 23);
            browseMapImageButton.TabIndex = 0;
            browseMapImageButton.Text = "Browse";
            browseMapImageButton.UseVisualStyleBackColor = true;
            browseMapImageButton.Click += BrowseMapImageButton_Click;
            // 
            // ERMapGenerator
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(577, 637);
            Controls.Add(progressLabel);
            Controls.Add(mapConfigurationGroupBox);
            Controls.Add(label4);
            Controls.Add(automationModeTabControl);
            Controls.Add(versionStr);
            Controls.Add(copyrightInfoStr);
            Controls.Add(automateButton);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Icon = (Icon)resources.GetObject("$this.Icon");
            Margin = new Padding(2);
            MaximizeBox = false;
            Name = "ERMapGenerator";
            Text = "ERMapGenerator";
            Shown += ERMapGenerator_Shown;
            mapConfigurationGroupBox.ResumeLayout(false);
            mapConfigurationGroupBox.PerformLayout();
            automationModeTabControl.ResumeLayout(false);
            tabPage2.ResumeLayout(false);
            tabPage2.PerformLayout();
            outputFolderGroupBox.ResumeLayout(false);
            outputFolderGroupBox.PerformLayout();
            gameModFolderGroupBox.ResumeLayout(false);
            gameModFolderGroupBox.PerformLayout();
            tabPage1.ResumeLayout(false);
            mapDisplayGroupBox.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)mapDisplayPictureBox).EndInit();
            mapImageGroupBox.ResumeLayout(false);
            mapImageGroupBox.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private Label copyrightInfoStr;
        private Label versionStr;
        private GroupBox mapConfigurationGroupBox;
        private Button automateButton;
        private Label label2;
        private ComboBox groundLevelComboBox;
        private Label label3;
        private ComboBox zoomLevelComboBox;
        private Label progressLabel;
        private Label label4;
        private TabControl automationModeTabControl;
        private TabPage tabPage1;
        private TabPage tabPage2;
        private GroupBox mapDisplayGroupBox;
        private Label mapDisplayOpenMapImageLabel;
        private PictureBox mapDisplayPictureBox;
        private GroupBox mapImageGroupBox;
        private Label label1;
        private Label mapImageFilePathLabel;
        private Button browseMapImageButton;
        private GroupBox gameModFolderGroupBox;
        private Label label5;
        private Label gameModFolderPathLabel;
        private Button browseGameModFolderButton;
        private GroupBox outputFolderGroupBox;
        private Label label7;
        private Label outputFolderPathLabel;
        private Button outputFolderButton;
        private CheckBox drawTileDebugInfoCheckBox;
    }
}