namespace SmartHomeScadaDashboard.Forms
{
    partial class DashboardForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.ListBox listDevices;
        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.Label lblHeader;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.listDevices = new System.Windows.Forms.ListBox();
            this.btnRefresh = new System.Windows.Forms.Button();
            this.lblHeader = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // lblHeader
            // 
            this.lblHeader.AutoSize = true;
            this.lblHeader.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Bold);
            this.lblHeader.Location = new System.Drawing.Point(20, 20);
            this.lblHeader.Text = "Smart Home SCADA Dashboard";
            // 
            // listDevices
            // 
            this.listDevices.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.listDevices.ItemHeight = 20;
            this.listDevices.Location = new System.Drawing.Point(25, 70);
            this.listDevices.Size = new System.Drawing.Size(450, 250);
            // 
            // btnRefresh
            // 
            this.btnRefresh.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.btnRefresh.Location = new System.Drawing.Point(25, 340);
            this.btnRefresh.Size = new System.Drawing.Size(100, 35);
            this.btnRefresh.Text = "Refresh";
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
            // 
            // DashboardForm
            // 
            this.ClientSize = new System.Drawing.Size(500, 400);
            this.Controls.Add(this.lblHeader);
            this.Controls.Add(this.listDevices);
            this.Controls.Add(this.btnRefresh);
            this.Text = "Smart Home SCADA Dashboard";
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}
