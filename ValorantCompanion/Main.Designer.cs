namespace ValorantCompanion
{
    partial class Main
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
            imgCard = new PictureBox();
            lblPlayerName = new MaterialSkin.Controls.MaterialLabel();
            imgRank = new PictureBox();
            lblRR = new MaterialSkin.Controls.MaterialLabel();
            materialCard1 = new MaterialSkin.Controls.MaterialCard();
            btnInstaLock = new MaterialSkin.Controls.MaterialButton();
            btnMatch = new MaterialSkin.Controls.MaterialButton();
            btnDodge = new MaterialSkin.Controls.MaterialButton();
            ((System.ComponentModel.ISupportInitialize)imgCard).BeginInit();
            ((System.ComponentModel.ISupportInitialize)imgRank).BeginInit();
            materialCard1.SuspendLayout();
            SuspendLayout();
            // 
            // imgCard
            // 
            imgCard.Location = new Point(807, 94);
            imgCard.Name = "imgCard";
            imgCard.Size = new Size(231, 603);
            imgCard.SizeMode = PictureBoxSizeMode.StretchImage;
            imgCard.TabIndex = 0;
            imgCard.TabStop = false;
            // 
            // lblPlayerName
            // 
            lblPlayerName.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            lblPlayerName.Depth = 0;
            lblPlayerName.Font = new Font("Roboto", 14F, FontStyle.Regular, GraphicsUnit.Pixel);
            lblPlayerName.Location = new Point(0, 1);
            lblPlayerName.MouseState = MaterialSkin.MouseState.HOVER;
            lblPlayerName.Name = "lblPlayerName";
            lblPlayerName.Size = new Size(231, 19);
            lblPlayerName.TabIndex = 1;
            lblPlayerName.Text = "Player Name";
            lblPlayerName.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // imgRank
            // 
            imgRank.Location = new Point(83, 22);
            imgRank.Name = "imgRank";
            imgRank.Size = new Size(65, 65);
            imgRank.SizeMode = PictureBoxSizeMode.StretchImage;
            imgRank.TabIndex = 2;
            imgRank.TabStop = false;
            // 
            // lblRR
            // 
            lblRR.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            lblRR.Depth = 0;
            lblRR.Font = new Font("Roboto", 14F, FontStyle.Regular, GraphicsUnit.Pixel);
            lblRR.Location = new Point(0, 89);
            lblRR.MouseState = MaterialSkin.MouseState.HOVER;
            lblRR.Name = "lblRR";
            lblRR.Size = new Size(231, 18);
            lblRR.TabIndex = 3;
            lblRR.Text = "0/100 RR";
            lblRR.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // materialCard1
            // 
            materialCard1.BackColor = Color.FromArgb(255, 255, 255);
            materialCard1.Controls.Add(lblPlayerName);
            materialCard1.Controls.Add(imgRank);
            materialCard1.Controls.Add(lblRR);
            materialCard1.Depth = 0;
            materialCard1.ForeColor = Color.FromArgb(222, 0, 0, 0);
            materialCard1.Location = new Point(807, 731);
            materialCard1.Margin = new Padding(14);
            materialCard1.MouseState = MaterialSkin.MouseState.HOVER;
            materialCard1.Name = "materialCard1";
            materialCard1.Padding = new Padding(14);
            materialCard1.Size = new Size(231, 110);
            materialCard1.TabIndex = 4;
            // 
            // btnInstaLock
            // 
            btnInstaLock.AutoSize = false;
            btnInstaLock.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnInstaLock.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            btnInstaLock.Depth = 0;
            btnInstaLock.HighEmphasis = true;
            btnInstaLock.Icon = null;
            btnInstaLock.Location = new Point(22, 79);
            btnInstaLock.Margin = new Padding(4, 6, 4, 6);
            btnInstaLock.MouseState = MaterialSkin.MouseState.HOVER;
            btnInstaLock.Name = "btnInstaLock";
            btnInstaLock.NoAccentTextColor = Color.Empty;
            btnInstaLock.Size = new Size(200, 36);
            btnInstaLock.TabIndex = 5;
            btnInstaLock.Text = "Insta Lock";
            btnInstaLock.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            btnInstaLock.UseAccentColor = false;
            btnInstaLock.UseVisualStyleBackColor = true;
            btnInstaLock.Click += btnInstaLock_Click;
            // 
            // btnMatch
            // 
            btnMatch.AutoSize = false;
            btnMatch.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnMatch.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            btnMatch.Depth = 0;
            btnMatch.HighEmphasis = true;
            btnMatch.Icon = null;
            btnMatch.Location = new Point(230, 79);
            btnMatch.Margin = new Padding(4, 6, 4, 6);
            btnMatch.MouseState = MaterialSkin.MouseState.HOVER;
            btnMatch.Name = "btnMatch";
            btnMatch.NoAccentTextColor = Color.Empty;
            btnMatch.Size = new Size(200, 36);
            btnMatch.TabIndex = 6;
            btnMatch.Text = "Match Details";
            btnMatch.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            btnMatch.UseAccentColor = false;
            btnMatch.UseVisualStyleBackColor = true;
            btnMatch.Click += btnMatch_Click;
            // 
            // btnDodge
            // 
            btnDodge.AutoSize = false;
            btnDodge.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnDodge.BackColor = Color.LightCoral;
            btnDodge.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            btnDodge.Depth = 0;
            btnDodge.HighEmphasis = true;
            btnDodge.Icon = null;
            btnDodge.Location = new Point(438, 79);
            btnDodge.Margin = new Padding(4, 6, 4, 6);
            btnDodge.MouseState = MaterialSkin.MouseState.HOVER;
            btnDodge.Name = "btnDodge";
            btnDodge.NoAccentTextColor = Color.Empty;
            btnDodge.Size = new Size(200, 36);
            btnDodge.TabIndex = 7;
            btnDodge.Text = "Dodge";
            btnDodge.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            btnDodge.UseAccentColor = false;
            btnDodge.UseVisualStyleBackColor = false;
            btnDodge.Click += btnDodge_Click;
            // 
            // Main
            // 
            AutoScaleMode = AutoScaleMode.None;
            ClientSize = new Size(1053, 859);
            Controls.Add(btnDodge);
            Controls.Add(btnMatch);
            Controls.Add(btnInstaLock);
            Controls.Add(materialCard1);
            Controls.Add(imgCard);
            FormBorderStyle = FormBorderStyle.Sizable;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "Main";
            SizeGripStyle = SizeGripStyle.Hide;
            Text = "Main Menu";
            Load += Main_Load;
            ((System.ComponentModel.ISupportInitialize)imgCard).EndInit();
            ((System.ComponentModel.ISupportInitialize)imgRank).EndInit();
            materialCard1.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private PictureBox imgCard;
        private MaterialSkin.Controls.MaterialLabel lblPlayerName;
        private PictureBox imgRank;
        private MaterialSkin.Controls.MaterialLabel lblRR;
        private MaterialSkin.Controls.MaterialCard materialCard1;
        private MaterialSkin.Controls.MaterialButton btnInstaLock;
        private MaterialSkin.Controls.MaterialButton btnMatch;
        private MaterialSkin.Controls.MaterialButton btnDodge;
    }
}