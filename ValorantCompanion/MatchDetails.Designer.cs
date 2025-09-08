namespace ValorantCompanion
{
    partial class MatchDetails
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
            flowPlayers = new FlowLayoutPanel();
            imgMap = new PictureBox();
            lblMapName = new MaterialSkin.Controls.MaterialLabel();
            materialCard1 = new MaterialSkin.Controls.MaterialCard();
            lblMode = new MaterialSkin.Controls.MaterialLabel();
            lblStart = new MaterialSkin.Controls.MaterialLabel();
            lblServer = new MaterialSkin.Controls.MaterialLabel();
            ((System.ComponentModel.ISupportInitialize)imgMap).BeginInit();
            materialCard1.SuspendLayout();
            SuspendLayout();
            // 
            // flowPlayers
            // 
            flowPlayers.Location = new Point(6, 80);
            flowPlayers.Name = "flowPlayers";
            flowPlayers.Size = new Size(405, 858);
            flowPlayers.TabIndex = 0;
            // 
            // imgMap
            // 
            imgMap.Location = new Point(17, 17);
            imgMap.Name = "imgMap";
            imgMap.Size = new Size(213, 123);
            imgMap.SizeMode = PictureBoxSizeMode.StretchImage;
            imgMap.TabIndex = 1;
            imgMap.TabStop = false;
            // 
            // lblMapName
            // 
            lblMapName.BackColor = Color.Transparent;
            lblMapName.Depth = 0;
            lblMapName.Font = new Font("Roboto", 34F, FontStyle.Bold, GraphicsUnit.Pixel);
            lblMapName.FontType = MaterialSkin.MaterialSkinManager.fontType.H4;
            lblMapName.Location = new Point(17, 143);
            lblMapName.MouseState = MaterialSkin.MouseState.HOVER;
            lblMapName.Name = "lblMapName";
            lblMapName.Size = new Size(212, 51);
            lblMapName.TabIndex = 6;
            lblMapName.Text = "Map Name";
            lblMapName.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // materialCard1
            // 
            materialCard1.BackColor = Color.FromArgb(255, 255, 255);
            materialCard1.Controls.Add(lblMode);
            materialCard1.Controls.Add(lblStart);
            materialCard1.Controls.Add(lblServer);
            materialCard1.Controls.Add(lblMapName);
            materialCard1.Controls.Add(imgMap);
            materialCard1.Depth = 0;
            materialCard1.ForeColor = Color.FromArgb(222, 0, 0, 0);
            materialCard1.Location = new Point(417, 80);
            materialCard1.Margin = new Padding(14);
            materialCard1.MouseState = MaterialSkin.MouseState.HOVER;
            materialCard1.Name = "materialCard1";
            materialCard1.Padding = new Padding(14);
            materialCard1.Size = new Size(246, 272);
            materialCard1.TabIndex = 7;
            // 
            // lblMode
            // 
            lblMode.BackColor = Color.Transparent;
            lblMode.Depth = 0;
            lblMode.Font = new Font("Roboto", 14F, FontStyle.Regular, GraphicsUnit.Pixel);
            lblMode.Location = new Point(0, 187);
            lblMode.MouseState = MaterialSkin.MouseState.HOVER;
            lblMode.Name = "lblMode";
            lblMode.Size = new Size(246, 24);
            lblMode.TabIndex = 9;
            lblMode.Text = "Mode";
            lblMode.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lblStart
            // 
            lblStart.Depth = 0;
            lblStart.Font = new Font("Roboto", 12F, FontStyle.Regular, GraphicsUnit.Pixel);
            lblStart.FontType = MaterialSkin.MaterialSkinManager.fontType.Caption;
            lblStart.Location = new Point(17, 230);
            lblStart.MouseState = MaterialSkin.MouseState.HOVER;
            lblStart.Name = "lblStart";
            lblStart.Size = new Size(212, 19);
            lblStart.TabIndex = 8;
            lblStart.Text = "Starting Side";
            lblStart.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lblServer
            // 
            lblServer.Depth = 0;
            lblServer.Font = new Font("Roboto", 12F, FontStyle.Regular, GraphicsUnit.Pixel);
            lblServer.FontType = MaterialSkin.MaterialSkinManager.fontType.Caption;
            lblServer.Location = new Point(0, 211);
            lblServer.MouseState = MaterialSkin.MouseState.HOVER;
            lblServer.Name = "lblServer";
            lblServer.Size = new Size(246, 19);
            lblServer.TabIndex = 7;
            lblServer.Text = "Server Name";
            lblServer.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // MatchDetails
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(730, 944);
            Controls.Add(materialCard1);
            Controls.Add(flowPlayers);
            FormBorderStyle = FormBorderStyle.Sizable;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "MatchDetails";
            SizeGripStyle = SizeGripStyle.Hide;
            Text = "Match Details";
            ((System.ComponentModel.ISupportInitialize)imgMap).EndInit();
            materialCard1.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private FlowLayoutPanel flowPlayers;
        private PictureBox imgMap;
        private MaterialSkin.Controls.MaterialLabel lblMapName;
        private MaterialSkin.Controls.MaterialCard materialCard1;
        private MaterialSkin.Controls.MaterialLabel lblServer;
        private MaterialSkin.Controls.MaterialLabel lblStart;
        private MaterialSkin.Controls.MaterialLabel lblMode;
    }
}