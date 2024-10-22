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
            mapDisplayPictureBox = new PictureBox();
            mapDisplayGroupBox = new GroupBox();
            mapDisplayOpenMapImageLabel = new Label();
            mapImageGroupBox = new GroupBox();
            label1 = new Label();
            mapImageFilePathLabel = new Label();
            browseMapImageButton = new Button();
            mapConfigurationGroupBox = new GroupBox();
            progressLabel = new Label();
            label4 = new Label();
            automationModeGroupBox = new GroupBox();
            repackTileMapRadioButton = new RadioButton();
            unpackStitchMapRadioButton = new RadioButton();
            label3 = new Label();
            zoomLevelComboBox = new ComboBox();
            automateButton = new Button();
            label2 = new Label();
            groundLevelComboBox = new ComboBox();
            splitContainer1 = new SplitContainer();
            ((System.ComponentModel.ISupportInitialize)mapDisplayPictureBox).BeginInit();
            mapDisplayGroupBox.SuspendLayout();
            mapImageGroupBox.SuspendLayout();
            mapConfigurationGroupBox.SuspendLayout();
            automationModeGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            SuspendLayout();
            // 
            // copyrightInfoStr
            // 
            copyrightInfoStr.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            copyrightInfoStr.AutoSize = true;
            copyrightInfoStr.ForeColor = Color.Gray;
            copyrightInfoStr.Location = new Point(965, 6);
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
            versionStr.Location = new Point(902, 6);
            versionStr.Margin = new Padding(2, 0, 2, 0);
            versionStr.Name = "versionStr";
            versionStr.Size = new Size(48, 15);
            versionStr.TabIndex = 11;
            versionStr.Text = "Version:";
            // 
            // mapDisplayPictureBox
            // 
            mapDisplayPictureBox.Location = new Point(6, 19);
            mapDisplayPictureBox.Name = "mapDisplayPictureBox";
            mapDisplayPictureBox.Size = new Size(549, 441);
            mapDisplayPictureBox.SizeMode = PictureBoxSizeMode.StretchImage;
            mapDisplayPictureBox.TabIndex = 12;
            mapDisplayPictureBox.TabStop = false;
            mapDisplayPictureBox.MouseDown += MapDisplayPictureBox_MouseDown;
            mapDisplayPictureBox.MouseMove += MapDisplayPictureBox_MouseMove;
            mapDisplayPictureBox.MouseUp += MapDisplayPictureBox_MouseUp;
            // 
            // mapDisplayGroupBox
            // 
            mapDisplayGroupBox.Controls.Add(mapDisplayOpenMapImageLabel);
            mapDisplayGroupBox.Controls.Add(mapDisplayPictureBox);
            mapDisplayGroupBox.Dock = DockStyle.Fill;
            mapDisplayGroupBox.Location = new Point(0, 0);
            mapDisplayGroupBox.Name = "mapDisplayGroupBox";
            mapDisplayGroupBox.Size = new Size(561, 466);
            mapDisplayGroupBox.TabIndex = 13;
            mapDisplayGroupBox.TabStop = false;
            mapDisplayGroupBox.Text = "Map Display";
            // 
            // mapDisplayOpenMapImageLabel
            // 
            mapDisplayOpenMapImageLabel.Dock = DockStyle.Fill;
            mapDisplayOpenMapImageLabel.Location = new Point(3, 19);
            mapDisplayOpenMapImageLabel.Name = "mapDisplayOpenMapImageLabel";
            mapDisplayOpenMapImageLabel.Size = new Size(555, 444);
            mapDisplayOpenMapImageLabel.TabIndex = 3;
            mapDisplayOpenMapImageLabel.Text = "Open a map image to view map data...";
            mapDisplayOpenMapImageLabel.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // mapImageGroupBox
            // 
            mapImageGroupBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            mapImageGroupBox.Controls.Add(label1);
            mapImageGroupBox.Controls.Add(mapImageFilePathLabel);
            mapImageGroupBox.Controls.Add(browseMapImageButton);
            mapImageGroupBox.Location = new Point(14, 23);
            mapImageGroupBox.Name = "mapImageGroupBox";
            mapImageGroupBox.Size = new Size(1122, 72);
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
            browseMapImageButton.Size = new Size(1110, 23);
            browseMapImageButton.TabIndex = 0;
            browseMapImageButton.Text = "Browse";
            browseMapImageButton.UseVisualStyleBackColor = true;
            browseMapImageButton.Click += BrowseMapImageButton_Click;
            // 
            // mapConfigurationGroupBox
            // 
            mapConfigurationGroupBox.Controls.Add(progressLabel);
            mapConfigurationGroupBox.Controls.Add(label4);
            mapConfigurationGroupBox.Controls.Add(automationModeGroupBox);
            mapConfigurationGroupBox.Controls.Add(label3);
            mapConfigurationGroupBox.Controls.Add(zoomLevelComboBox);
            mapConfigurationGroupBox.Controls.Add(automateButton);
            mapConfigurationGroupBox.Controls.Add(label2);
            mapConfigurationGroupBox.Controls.Add(groundLevelComboBox);
            mapConfigurationGroupBox.Dock = DockStyle.Fill;
            mapConfigurationGroupBox.Location = new Point(0, 0);
            mapConfigurationGroupBox.Name = "mapConfigurationGroupBox";
            mapConfigurationGroupBox.Size = new Size(557, 466);
            mapConfigurationGroupBox.TabIndex = 15;
            mapConfigurationGroupBox.TabStop = false;
            mapConfigurationGroupBox.Text = "Map Configuration";
            // 
            // progressLabel
            // 
            progressLabel.AutoSize = true;
            progressLabel.Location = new Point(48, 78);
            progressLabel.Name = "progressLabel";
            progressLabel.Size = new Size(57, 15);
            progressLabel.TabIndex = 11;
            progressLabel.Text = "Waiting...";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(9, 78);
            label4.Name = "label4";
            label4.Size = new Size(42, 15);
            label4.TabIndex = 10;
            label4.Text = "Status:";
            // 
            // automationModeGroupBox
            // 
            automationModeGroupBox.Controls.Add(repackTileMapRadioButton);
            automationModeGroupBox.Controls.Add(unpackStitchMapRadioButton);
            automationModeGroupBox.Location = new Point(11, 22);
            automationModeGroupBox.Name = "automationModeGroupBox";
            automationModeGroupBox.Size = new Size(272, 52);
            automationModeGroupBox.TabIndex = 9;
            automationModeGroupBox.TabStop = false;
            automationModeGroupBox.Text = "Automation Mode";
            // 
            // repackTileMapRadioButton
            // 
            repackTileMapRadioButton.AutoSize = true;
            repackTileMapRadioButton.Location = new Point(138, 22);
            repackTileMapRadioButton.Name = "repackTileMapRadioButton";
            repackTileMapRadioButton.Size = new Size(113, 19);
            repackTileMapRadioButton.TabIndex = 10;
            repackTileMapRadioButton.TabStop = true;
            repackTileMapRadioButton.Text = "Repack/Tile Map";
            repackTileMapRadioButton.UseVisualStyleBackColor = true;
            // 
            // unpackStitchMapRadioButton
            // 
            unpackStitchMapRadioButton.AutoSize = true;
            unpackStitchMapRadioButton.Location = new Point(8, 22);
            unpackStitchMapRadioButton.Name = "unpackStitchMapRadioButton";
            unpackStitchMapRadioButton.Size = new Size(127, 19);
            unpackStitchMapRadioButton.TabIndex = 0;
            unpackStitchMapRadioButton.TabStop = true;
            unpackStitchMapRadioButton.Text = "Unpack/Stitch Map";
            unpackStitchMapRadioButton.UseVisualStyleBackColor = true;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(418, 32);
            label3.Name = "label3";
            label3.Size = new Size(72, 15);
            label3.TabIndex = 8;
            label3.Text = "Zoom Level:";
            // 
            // zoomLevelComboBox
            // 
            zoomLevelComboBox.FormattingEnabled = true;
            zoomLevelComboBox.Location = new Point(420, 50);
            zoomLevelComboBox.Name = "zoomLevelComboBox";
            zoomLevelComboBox.Size = new Size(125, 23);
            zoomLevelComboBox.TabIndex = 7;
            // 
            // automateButton
            // 
            automateButton.Location = new Point(11, 97);
            automateButton.Name = "automateButton";
            automateButton.Size = new Size(536, 23);
            automateButton.TabIndex = 4;
            automateButton.Text = "Automate!";
            automateButton.UseVisualStyleBackColor = true;
            automateButton.Click += AutomateButton_Click;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(286, 32);
            label2.Name = "label2";
            label2.Size = new Size(80, 15);
            label2.TabIndex = 6;
            label2.Text = "Ground Level:";
            // 
            // groundLevelComboBox
            // 
            groundLevelComboBox.FormattingEnabled = true;
            groundLevelComboBox.Location = new Point(289, 50);
            groundLevelComboBox.Name = "groundLevelComboBox";
            groundLevelComboBox.Size = new Size(125, 23);
            groundLevelComboBox.TabIndex = 5;
            // 
            // splitContainer1
            // 
            splitContainer1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            splitContainer1.IsSplitterFixed = true;
            splitContainer1.Location = new Point(14, 101);
            splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(mapDisplayGroupBox);
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(mapConfigurationGroupBox);
            splitContainer1.Size = new Size(1122, 466);
            splitContainer1.SplitterDistance = 561;
            splitContainer1.TabIndex = 13;
            // 
            // ERMapGenerator
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1149, 579);
            Controls.Add(splitContainer1);
            Controls.Add(mapImageGroupBox);
            Controls.Add(versionStr);
            Controls.Add(copyrightInfoStr);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Icon = (Icon)resources.GetObject("$this.Icon");
            Margin = new Padding(2);
            MaximizeBox = false;
            Name = "ERMapGenerator";
            Text = "ERMapGenerator";
            Shown += ERMapGenerator_Shown;
            ((System.ComponentModel.ISupportInitialize)mapDisplayPictureBox).EndInit();
            mapDisplayGroupBox.ResumeLayout(false);
            mapImageGroupBox.ResumeLayout(false);
            mapImageGroupBox.PerformLayout();
            mapConfigurationGroupBox.ResumeLayout(false);
            mapConfigurationGroupBox.PerformLayout();
            automationModeGroupBox.ResumeLayout(false);
            automationModeGroupBox.PerformLayout();
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private Label copyrightInfoStr;
        private Label versionStr;
        private PictureBox mapDisplayPictureBox;
        private GroupBox mapDisplayGroupBox;
        private GroupBox mapImageGroupBox;
        private Label mapImageFilePathLabel;
        private Button browseMapImageButton;
        private Label label1;
        private Label mapDisplayOpenMapImageLabel;
        private GroupBox mapConfigurationGroupBox;
        private SplitContainer splitContainer1;
        private Button automateButton;
        private Label label2;
        private ComboBox groundLevelComboBox;
        private Label label3;
        private ComboBox zoomLevelComboBox;
        private GroupBox automationModeGroupBox;
        private RadioButton repackTileMapRadioButton;
        private RadioButton unpackStitchMapRadioButton;
        private Label progressLabel;
        private Label label4;
    }
}