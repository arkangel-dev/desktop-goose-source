namespace GooseDesktop
{
	// Token: 0x02000008 RID: 8
	public partial class Form1 : global::System.Windows.Forms.Form
	{
		// Token: 0x0600003B RID: 59 RVA: 0x0000246E File Offset: 0x0000066E
		protected override void Dispose(bool disposing)
		{
			if (disposing && this.components != null)
			{
				this.components.Dispose();
			}
			base.Dispose(disposing);
		}

		// Token: 0x0600003C RID: 60 RVA: 0x00002FA4 File Offset: 0x000011A4
		private void InitializeComponent()
		{
			base.SuspendLayout();
			base.AutoScaleDimensions = new global::System.Drawing.SizeF(8f, 16f);
			base.AutoScaleMode = global::System.Windows.Forms.AutoScaleMode.Font;
			base.ClientSize = new global::System.Drawing.Size(282, 253);
			base.Name = "Form1";
			this.Text = "Form1";
			base.Load += new global::System.EventHandler(this.Form1_Load);
			base.ResumeLayout(false);
		}

		// Token: 0x0400000F RID: 15
		private global::System.ComponentModel.IContainer components;
	}
}
