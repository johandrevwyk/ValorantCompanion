namespace ValorantCompanion
{
    partial class InstaLock
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
            flowAgent = new FlowLayoutPanel();
            SuspendLayout();
            // 
            // flowAgent
            // 
            flowAgent.AutoScroll = true;
            flowAgent.Dock = DockStyle.Fill;
            flowAgent.Location = new Point(3, 64);
            flowAgent.Name = "flowAgent";
            flowAgent.Size = new Size(1067, 626);
            flowAgent.TabIndex = 0;
            // 
            // InstaLock
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1073, 693);
            Controls.Add(flowAgent);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "InstaLock";
            SizeGripStyle = SizeGripStyle.Hide;
            Text = "Insta Lock";
            ResumeLayout(false);
        }

        #endregion

        private FlowLayoutPanel flowAgents;
    }
}