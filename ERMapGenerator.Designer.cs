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
            copyrightInfoStr = new Label();
            automationSetupGroupBox = new GroupBox();
            outputFolderGroupBox = new GroupBox();
            outputFolderButton = new Button();
            outputFolderPathLabel = new Label();
            label2 = new Label();
            gameModFolderGroupBox = new GroupBox();
            browseGameModFolderButton = new Button();
            gameModFolderPathLabel = new Label();
            label6 = new Label();
            automateButton = new Button();
            versionStr = new Label();
            progressLabel = new Label();
            statusLabel = new Label();
            automationSetupGroupBox.SuspendLayout();
            outputFolderGroupBox.SuspendLayout();
            gameModFolderGroupBox.SuspendLayout();
            SuspendLayout();
            // 
            // copyrightInfoStr
            // 
            copyrightInfoStr.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            copyrightInfoStr.AutoSize = true;
            copyrightInfoStr.ForeColor = Color.Gray;
            copyrightInfoStr.Location = new Point(372, 6);
            copyrightInfoStr.Margin = new Padding(2, 0, 2, 0);
            copyrightInfoStr.Name = "copyrightInfoStr";
            copyrightInfoStr.Size = new Size(174, 15);
            copyrightInfoStr.TabIndex = 1;
            copyrightInfoStr.Text = "© Pear, 2023 All rights reserved.";
            // 
            // automationSetupGroupBox
            // 
            automationSetupGroupBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            automationSetupGroupBox.Controls.Add(outputFolderGroupBox);
            automationSetupGroupBox.Controls.Add(gameModFolderGroupBox);
            automationSetupGroupBox.Location = new Point(8, 18);
            automationSetupGroupBox.Name = "automationSetupGroupBox";
            automationSetupGroupBox.Size = new Size(536, 174);
            automationSetupGroupBox.TabIndex = 3;
            automationSetupGroupBox.TabStop = false;
            automationSetupGroupBox.Text = "Automation Setup";
            // 
            // outputFolderGroupBox
            // 
            outputFolderGroupBox.Controls.Add(outputFolderButton);
            outputFolderGroupBox.Controls.Add(outputFolderPathLabel);
            outputFolderGroupBox.Controls.Add(label2);
            outputFolderGroupBox.Enabled = false;
            outputFolderGroupBox.Location = new Point(6, 100);
            outputFolderGroupBox.Name = "outputFolderGroupBox";
            outputFolderGroupBox.Size = new Size(524, 69);
            outputFolderGroupBox.TabIndex = 12;
            outputFolderGroupBox.TabStop = false;
            outputFolderGroupBox.Text = "Output Folder";
            // 
            // outputFolderButton
            // 
            outputFolderButton.Location = new Point(6, 20);
            outputFolderButton.Name = "outputFolderButton";
            outputFolderButton.Size = new Size(512, 25);
            outputFolderButton.TabIndex = 5;
            outputFolderButton.Text = "Browse";
            outputFolderButton.UseVisualStyleBackColor = true;
            outputFolderButton.Click += OutputFolderButton_Click;
            // 
            // outputFolderPathLabel
            // 
            outputFolderPathLabel.AutoSize = true;
            outputFolderPathLabel.Location = new Point(36, 47);
            outputFolderPathLabel.Name = "outputFolderPathLabel";
            outputFolderPathLabel.Size = new Size(29, 15);
            outputFolderPathLabel.TabIndex = 9;
            outputFolderPathLabel.Text = "N/A";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(4, 47);
            label2.Name = "label2";
            label2.Size = new Size(34, 15);
            label2.TabIndex = 8;
            label2.Text = "Path:";
            // 
            // gameModFolderGroupBox
            // 
            gameModFolderGroupBox.Controls.Add(browseGameModFolderButton);
            gameModFolderGroupBox.Controls.Add(gameModFolderPathLabel);
            gameModFolderGroupBox.Controls.Add(label6);
            gameModFolderGroupBox.Location = new Point(6, 25);
            gameModFolderGroupBox.Name = "gameModFolderGroupBox";
            gameModFolderGroupBox.Size = new Size(524, 69);
            gameModFolderGroupBox.TabIndex = 11;
            gameModFolderGroupBox.TabStop = false;
            gameModFolderGroupBox.Text = "Game/Mod Folder";
            // 
            // browseGameModFolderButton
            // 
            browseGameModFolderButton.Location = new Point(6, 20);
            browseGameModFolderButton.Name = "browseGameModFolderButton";
            browseGameModFolderButton.Size = new Size(512, 25);
            browseGameModFolderButton.TabIndex = 5;
            browseGameModFolderButton.Text = "Browse";
            browseGameModFolderButton.UseVisualStyleBackColor = true;
            browseGameModFolderButton.Click += BrowseGameModFolderButton_Click;
            // 
            // gameModFolderPathLabel
            // 
            gameModFolderPathLabel.AutoSize = true;
            gameModFolderPathLabel.Location = new Point(36, 47);
            gameModFolderPathLabel.Name = "gameModFolderPathLabel";
            gameModFolderPathLabel.Size = new Size(29, 15);
            gameModFolderPathLabel.TabIndex = 9;
            gameModFolderPathLabel.Text = "N/A";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(4, 47);
            label6.Name = "label6";
            label6.Size = new Size(34, 15);
            label6.TabIndex = 8;
            label6.Text = "Path:";
            // 
            // automateButton
            // 
            automateButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            automateButton.Enabled = false;
            automateButton.Location = new Point(8, 219);
            automateButton.Name = "automateButton";
            automateButton.Size = new Size(537, 25);
            automateButton.TabIndex = 7;
            automateButton.Text = "Automate!";
            automateButton.UseVisualStyleBackColor = true;
            automateButton.Click += AutomateButton_Click;
            // 
            // versionStr
            // 
            versionStr.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            versionStr.AutoSize = true;
            versionStr.ForeColor = Color.Gray;
            versionStr.Location = new Point(309, 6);
            versionStr.Margin = new Padding(2, 0, 2, 0);
            versionStr.Name = "versionStr";
            versionStr.Size = new Size(48, 15);
            versionStr.TabIndex = 11;
            versionStr.Text = "Version:";
            // 
            // progressLabel
            // 
            progressLabel.AutoSize = true;
            progressLabel.Enabled = false;
            progressLabel.Location = new Point(48, 197);
            progressLabel.Name = "progressLabel";
            progressLabel.Size = new Size(57, 15);
            progressLabel.TabIndex = 13;
            progressLabel.Text = "Waiting...";
            // 
            // statusLabel
            // 
            statusLabel.AutoSize = true;
            statusLabel.Enabled = false;
            statusLabel.Location = new Point(7, 197);
            statusLabel.Name = "statusLabel";
            statusLabel.Size = new Size(42, 15);
            statusLabel.TabIndex = 14;
            statusLabel.Text = "Status:";
            // 
            // ERMapGenerator
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(556, 254);
            Controls.Add(statusLabel);
            Controls.Add(progressLabel);
            Controls.Add(versionStr);
            Controls.Add(copyrightInfoStr);
            Controls.Add(automateButton);
            Controls.Add(automationSetupGroupBox);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Margin = new Padding(2);
            MaximizeBox = false;
            Name = "ERMapGenerator";
            Text = "ERMapGenerator";
            automationSetupGroupBox.ResumeLayout(false);
            outputFolderGroupBox.ResumeLayout(false);
            outputFolderGroupBox.PerformLayout();
            gameModFolderGroupBox.ResumeLayout(false);
            gameModFolderGroupBox.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private Label copyrightInfoStr;
        private GroupBox automationSetupGroupBox;
        private Button automateButton;
        private GroupBox gameModFolderGroupBox;
        private Button browseGameModFolderButton;
        private Label gameModFolderPathLabel;
        private Label label6;
        private Label versionStr;
        private GroupBox outputFolderGroupBox;
        private Button outputFolderButton;
        private Label outputFolderPathLabel;
        private Label label2;
        private Label progressLabel;
        private Label statusLabel;
    }
}