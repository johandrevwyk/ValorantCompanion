namespace ValorantCompanion
{
    partial class Auth
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
            lblstatus = new MaterialSkin.Controls.MaterialLabel();
            SuspendLayout();
            // 
            // lblstatus
            // 
            lblstatus.AutoSize = true;
            lblstatus.Depth = 0;
            lblstatus.Font = new Font("Roboto", 14F, FontStyle.Regular, GraphicsUnit.Pixel);
            lblstatus.Location = new Point(21, 75);
            lblstatus.MouseState = MaterialSkin.MouseState.HOVER;
            lblstatus.Name = "lblstatus";
            lblstatus.Size = new Size(319, 19);
            lblstatus.TabIndex = 0;
            lblstatus.Text = "Authenticating with Riot Games and Valorant";
            // 
            // Auth
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(365, 108);
            Controls.Add(lblstatus);
            Name = "Auth";
            Load += Auth_Load_1;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private MaterialSkin.Controls.MaterialLabel lblstatus;
    }
}
