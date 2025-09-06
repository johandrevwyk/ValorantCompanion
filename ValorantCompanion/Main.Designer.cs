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
            materialCard1 = new MaterialSkin.Controls.MaterialCard();
            btnInstaLock = new MaterialSkin.Controls.MaterialButton();
            btnMatch = new MaterialSkin.Controls.MaterialButton();
            btnDodge = new MaterialSkin.Controls.MaterialButton();
            materialCard2 = new MaterialSkin.Controls.MaterialCard();
            lblShopPrice4 = new MaterialSkin.Controls.MaterialLabel();
            lblShopPrice3 = new MaterialSkin.Controls.MaterialLabel();
            lblShopPrice2 = new MaterialSkin.Controls.MaterialLabel();
            lblShopPrice1 = new MaterialSkin.Controls.MaterialLabel();
            lblShop3 = new MaterialSkin.Controls.MaterialLabel();
            lblShop2 = new MaterialSkin.Controls.MaterialLabel();
            lblShop4 = new MaterialSkin.Controls.MaterialLabel();
            lblShop1 = new MaterialSkin.Controls.MaterialLabel();
            imgShop1 = new PictureBox();
            imgShop4 = new PictureBox();
            imgShop3 = new PictureBox();
            imgShop2 = new PictureBox();
            statusStrip1 = new StatusStrip();
            tsAuth = new ToolStripStatusLabel();
            ((System.ComponentModel.ISupportInitialize)imgCard).BeginInit();
            ((System.ComponentModel.ISupportInitialize)imgRank).BeginInit();
            materialCard1.SuspendLayout();
            materialCard2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)imgShop1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)imgShop4).BeginInit();
            ((System.ComponentModel.ISupportInitialize)imgShop3).BeginInit();
            ((System.ComponentModel.ISupportInitialize)imgShop2).BeginInit();
            statusStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // imgCard
            // 
            imgCard.Location = new Point(867, 79);
            imgCard.Name = "imgCard";
            imgCard.Size = new Size(214, 577);
            imgCard.SizeMode = PictureBoxSizeMode.StretchImage;
            imgCard.TabIndex = 0;
            imgCard.TabStop = false;
            // 
            // lblPlayerName
            // 
            lblPlayerName.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            lblPlayerName.Depth = 0;
            lblPlayerName.Font = new Font("Roboto Medium", 14F, FontStyle.Bold, GraphicsUnit.Pixel);
            lblPlayerName.FontType = MaterialSkin.MaterialSkinManager.fontType.Subtitle2;
            lblPlayerName.Location = new Point(0, 4);
            lblPlayerName.MouseState = MaterialSkin.MouseState.HOVER;
            lblPlayerName.Name = "lblPlayerName";
            lblPlayerName.Size = new Size(214, 19);
            lblPlayerName.TabIndex = 1;
            lblPlayerName.Text = "Player Name";
            lblPlayerName.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // imgRank
            // 
            imgRank.Location = new Point(76, 27);
            imgRank.Name = "imgRank";
            imgRank.Size = new Size(65, 65);
            imgRank.SizeMode = PictureBoxSizeMode.StretchImage;
            imgRank.TabIndex = 2;
            imgRank.TabStop = false;
            // 
            // materialCard1
            // 
            materialCard1.BackColor = Color.FromArgb(255, 255, 255);
            materialCard1.Controls.Add(lblPlayerName);
            materialCard1.Controls.Add(imgRank);
            materialCard1.Depth = 0;
            materialCard1.ForeColor = Color.FromArgb(222, 0, 0, 0);
            materialCard1.Location = new Point(867, 556);
            materialCard1.Margin = new Padding(14);
            materialCard1.MouseState = MaterialSkin.MouseState.HOVER;
            materialCard1.Name = "materialCard1";
            materialCard1.Padding = new Padding(14);
            materialCard1.Size = new Size(214, 100);
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
            // materialCard2
            // 
            materialCard2.BackColor = Color.FromArgb(255, 255, 255);
            materialCard2.Controls.Add(lblShopPrice4);
            materialCard2.Controls.Add(lblShopPrice3);
            materialCard2.Controls.Add(lblShopPrice2);
            materialCard2.Controls.Add(lblShopPrice1);
            materialCard2.Controls.Add(lblShop3);
            materialCard2.Controls.Add(lblShop2);
            materialCard2.Controls.Add(lblShop4);
            materialCard2.Controls.Add(lblShop1);
            materialCard2.Controls.Add(imgShop1);
            materialCard2.Controls.Add(imgShop4);
            materialCard2.Controls.Add(imgShop3);
            materialCard2.Controls.Add(imgShop2);
            materialCard2.Depth = 0;
            materialCard2.ForeColor = Color.FromArgb(222, 0, 0, 0);
            materialCard2.Location = new Point(22, 668);
            materialCard2.Margin = new Padding(14);
            materialCard2.MouseState = MaterialSkin.MouseState.HOVER;
            materialCard2.Name = "materialCard2";
            materialCard2.Padding = new Padding(14);
            materialCard2.Size = new Size(1059, 134);
            materialCard2.TabIndex = 8;
            materialCard2.Paint += materialCard2_Paint;
            // 
            // lblShopPrice4
            // 
            lblShopPrice4.Depth = 0;
            lblShopPrice4.Font = new Font("Roboto", 12F, FontStyle.Regular, GraphicsUnit.Pixel);
            lblShopPrice4.FontType = MaterialSkin.MaterialSkinManager.fontType.Caption;
            lblShopPrice4.Location = new Point(803, 107);
            lblShopPrice4.MouseState = MaterialSkin.MouseState.HOVER;
            lblShopPrice4.Name = "lblShopPrice4";
            lblShopPrice4.Size = new Size(256, 14);
            lblShopPrice4.TabIndex = 10;
            lblShopPrice4.Text = "Price";
            lblShopPrice4.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lblShopPrice3
            // 
            lblShopPrice3.Depth = 0;
            lblShopPrice3.Font = new Font("Roboto", 12F, FontStyle.Regular, GraphicsUnit.Pixel);
            lblShopPrice3.FontType = MaterialSkin.MaterialSkinManager.fontType.Caption;
            lblShopPrice3.Location = new Point(541, 107);
            lblShopPrice3.MouseState = MaterialSkin.MouseState.HOVER;
            lblShopPrice3.Name = "lblShopPrice3";
            lblShopPrice3.Size = new Size(256, 14);
            lblShopPrice3.TabIndex = 9;
            lblShopPrice3.Text = "Price";
            lblShopPrice3.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lblShopPrice2
            // 
            lblShopPrice2.Depth = 0;
            lblShopPrice2.Font = new Font("Roboto", 12F, FontStyle.Regular, GraphicsUnit.Pixel);
            lblShopPrice2.FontType = MaterialSkin.MaterialSkinManager.fontType.Caption;
            lblShopPrice2.Location = new Point(279, 107);
            lblShopPrice2.MouseState = MaterialSkin.MouseState.HOVER;
            lblShopPrice2.Name = "lblShopPrice2";
            lblShopPrice2.Size = new Size(256, 14);
            lblShopPrice2.TabIndex = 8;
            lblShopPrice2.Text = "Price";
            lblShopPrice2.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lblShopPrice1
            // 
            lblShopPrice1.Depth = 0;
            lblShopPrice1.Font = new Font("Roboto", 12F, FontStyle.Regular, GraphicsUnit.Pixel);
            lblShopPrice1.FontType = MaterialSkin.MaterialSkinManager.fontType.Caption;
            lblShopPrice1.Location = new Point(17, 107);
            lblShopPrice1.MouseState = MaterialSkin.MouseState.HOVER;
            lblShopPrice1.Name = "lblShopPrice1";
            lblShopPrice1.Size = new Size(256, 14);
            lblShopPrice1.TabIndex = 7;
            lblShopPrice1.Text = "Price";
            lblShopPrice1.TextAlign = ContentAlignment.MiddleCenter;
            lblShopPrice1.Click += lblShopPrice1_Click;
            // 
            // lblShop3
            // 
            lblShop3.Depth = 0;
            lblShop3.Font = new Font("Roboto", 14F, FontStyle.Regular, GraphicsUnit.Pixel);
            lblShop3.Location = new Point(541, 84);
            lblShop3.MouseState = MaterialSkin.MouseState.HOVER;
            lblShop3.Name = "lblShop3";
            lblShop3.Size = new Size(256, 23);
            lblShop3.TabIndex = 6;
            lblShop3.Text = "Store Item";
            lblShop3.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lblShop2
            // 
            lblShop2.Depth = 0;
            lblShop2.Font = new Font("Roboto", 14F, FontStyle.Regular, GraphicsUnit.Pixel);
            lblShop2.Location = new Point(279, 84);
            lblShop2.MouseState = MaterialSkin.MouseState.HOVER;
            lblShop2.Name = "lblShop2";
            lblShop2.Size = new Size(256, 23);
            lblShop2.TabIndex = 5;
            lblShop2.Text = "Store Item";
            lblShop2.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lblShop4
            // 
            lblShop4.Depth = 0;
            lblShop4.Font = new Font("Roboto", 14F, FontStyle.Regular, GraphicsUnit.Pixel);
            lblShop4.Location = new Point(803, 84);
            lblShop4.MouseState = MaterialSkin.MouseState.HOVER;
            lblShop4.Name = "lblShop4";
            lblShop4.Size = new Size(256, 23);
            lblShop4.TabIndex = 5;
            lblShop4.Text = "Store Item";
            lblShop4.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lblShop1
            // 
            lblShop1.Depth = 0;
            lblShop1.Font = new Font("Roboto", 14F, FontStyle.Regular, GraphicsUnit.Pixel);
            lblShop1.Location = new Point(17, 84);
            lblShop1.MouseState = MaterialSkin.MouseState.HOVER;
            lblShop1.Name = "lblShop1";
            lblShop1.Size = new Size(256, 23);
            lblShop1.TabIndex = 4;
            lblShop1.Text = "Store Item";
            lblShop1.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // imgShop1
            // 
            imgShop1.Location = new Point(17, 17);
            imgShop1.Name = "imgShop1";
            imgShop1.Size = new Size(256, 64);
            imgShop1.SizeMode = PictureBoxSizeMode.StretchImage;
            imgShop1.TabIndex = 3;
            imgShop1.TabStop = false;
            // 
            // imgShop4
            // 
            imgShop4.Location = new Point(803, 17);
            imgShop4.Name = "imgShop4";
            imgShop4.Size = new Size(256, 64);
            imgShop4.SizeMode = PictureBoxSizeMode.StretchImage;
            imgShop4.TabIndex = 2;
            imgShop4.TabStop = false;
            imgShop4.Click += pictureBox3_Click;
            // 
            // imgShop3
            // 
            imgShop3.Location = new Point(541, 17);
            imgShop3.Name = "imgShop3";
            imgShop3.Size = new Size(256, 64);
            imgShop3.SizeMode = PictureBoxSizeMode.StretchImage;
            imgShop3.TabIndex = 1;
            imgShop3.TabStop = false;
            // 
            // imgShop2
            // 
            imgShop2.Location = new Point(279, 17);
            imgShop2.Name = "imgShop2";
            imgShop2.Size = new Size(256, 64);
            imgShop2.SizeMode = PictureBoxSizeMode.StretchImage;
            imgShop2.TabIndex = 0;
            imgShop2.TabStop = false;
            // 
            // statusStrip1
            // 
            statusStrip1.Items.AddRange(new ToolStripItem[] { tsAuth });
            statusStrip1.Location = new Point(3, 814);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Size = new Size(1088, 22);
            statusStrip1.TabIndex = 9;
            statusStrip1.Text = "statusStrip1";
            // 
            // tsAuth
            // 
            tsAuth.Name = "tsAuth";
            tsAuth.Size = new Size(81, 17);
            tsAuth.Text = "Auth Method:";
            // 
            // Main
            // 
            AutoScaleMode = AutoScaleMode.None;
            ClientSize = new Size(1094, 839);
            Controls.Add(statusStrip1);
            Controls.Add(materialCard2);
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
            materialCard2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)imgShop1).EndInit();
            ((System.ComponentModel.ISupportInitialize)imgShop4).EndInit();
            ((System.ComponentModel.ISupportInitialize)imgShop3).EndInit();
            ((System.ComponentModel.ISupportInitialize)imgShop2).EndInit();
            statusStrip1.ResumeLayout(false);
            statusStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private PictureBox imgCard;
        private MaterialSkin.Controls.MaterialLabel lblPlayerName;
        private PictureBox imgRank;
        private MaterialSkin.Controls.MaterialCard materialCard1;
        private MaterialSkin.Controls.MaterialButton btnInstaLock;
        private MaterialSkin.Controls.MaterialButton btnMatch;
        private MaterialSkin.Controls.MaterialButton btnDodge;
        private MaterialSkin.Controls.MaterialCard materialCard2;
        private PictureBox imgShop4;
        private PictureBox imgShop3;
        private PictureBox imgShop2;
        private PictureBox imgShop1;
        private MaterialSkin.Controls.MaterialLabel lblShop3;
        private MaterialSkin.Controls.MaterialLabel lblShop2;
        private MaterialSkin.Controls.MaterialLabel lblShop4;
        private MaterialSkin.Controls.MaterialLabel lblShop1;
        private MaterialSkin.Controls.MaterialLabel lblShopPrice4;
        private MaterialSkin.Controls.MaterialLabel lblShopPrice3;
        private MaterialSkin.Controls.MaterialLabel lblShopPrice2;
        private MaterialSkin.Controls.MaterialLabel lblShopPrice1;
        private StatusStrip statusStrip1;
        private ToolStripStatusLabel tsAuth;
    }
}