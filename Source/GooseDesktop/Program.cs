using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace GooseDesktop
{
	// Token: 0x0200000F RID: 15
	internal static class Program
	{
		// Token: 0x06000061 RID: 97
		[DllImport("user32.dll", SetLastError = true)]
		private static extern uint GetWindowLong(IntPtr hWnd, int nIndex);

		// Token: 0x06000062 RID: 98
		[DllImport("user32.dll")]
		private static extern int SetWindowLong(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

		// Token: 0x06000063 RID: 99
		[DllImport("user32.dll")]
		private static extern bool SetLayeredWindowAttributes(IntPtr hwnd, uint crKey, byte bAlpha, uint dwFlags);

		// Token: 0x06000064 RID: 100
		[DllImport("user32.dll")]
		private static extern int PeekMessage(out Program.NativeMessage message, IntPtr window, uint filterMin, uint filterMax, uint remove);

		// Token: 0x06000065 RID: 101
		[DllImport("user32.dll")]
		public static extern short GetAsyncKeyState(Keys vKey);

		// Token: 0x06000066 RID: 102 RVA: 0x00004EC4 File Offset: 0x000030C4
		[STAThread]
		private static void Main()
		{
			Program.mainForm = new Form();
			Program.mainForm.BackColor = Program.ColorKey;
			Program.mainForm.FormBorderStyle = FormBorderStyle.None;
			Program.mainForm.Size = Screen.PrimaryScreen.WorkingArea.Size;
			Program.mainForm.StartPosition = FormStartPosition.Manual;
			Program.mainForm.Location = new Point(0, 0);
			Program.mainForm.TopMost = true;
			Program.mainForm.AllowTransparency = true;
			Program.mainForm.BackColor = Program.ColorKey;
			Program.mainForm.TransparencyKey = Program.ColorKey;
			Program.mainForm.ShowIcon = false;
			Program.mainForm.ShowInTaskbar = false;
			Program.OriginalWindowStyle = (IntPtr)((long)((ulong)Program.GetWindowLong(Program.mainForm.Handle, -20)));
			Program.PassthruWindowStyle = (IntPtr)((long)((ulong)(Program.GetWindowLong(Program.mainForm.Handle, -20) | 524288U | 32U)));
			Program.SetWindowPassthru(true);
			Program.canvas = new BufferedPanel();
			Program.canvas.Dock = DockStyle.Fill;
			Program.canvas.BackColor = Color.Transparent;
			Program.canvas.BringToFront();
			Program.canvas.Paint += Program.Render;
			Program.mainForm.Controls.Add(Program.canvas);
			MainGame.Init();
			Application.Idle += Program.HandleApplicationIdle;
			Application.EnableVisualStyles();
			Application.Run(Program.mainForm);
		}

		// Token: 0x06000067 RID: 103 RVA: 0x000025CC File Offset: 0x000007CC
		private static void SetWindowPassthru(bool passthrough)
		{
			if (passthrough)
			{
				Program.SetWindowLong(Program.mainForm.Handle, -20, Program.PassthruWindowStyle);
				return;
			}
			Program.SetWindowLong(Program.mainForm.Handle, -20, Program.OriginalWindowStyle);
		}

		// Token: 0x06000068 RID: 104 RVA: 0x00002600 File Offset: 0x00000800
		public static string GetPathToFileInAssembly(string relativePath)
		{
			return Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), relativePath);
		}

		// Token: 0x06000069 RID: 105 RVA: 0x0000503C File Offset: 0x0000323C
		private static bool IsApplicationIdle()
		{
			Program.NativeMessage nativeMessage;
			return Program.PeekMessage(out nativeMessage, IntPtr.Zero, 0U, 0U, 0U) == 0;
		}

		// Token: 0x0600006A RID: 106 RVA: 0x00002617 File Offset: 0x00000817
		private static void HandleApplicationIdle(object sender, EventArgs e)
		{
			while (Program.IsApplicationIdle())
			{
				Program.mainForm.TopMost = true;
				Program.canvas.BringToFront();
				Program.canvas.Invalidate();
				Thread.Sleep(8);
			}
		}

		// Token: 0x0600006B RID: 107 RVA: 0x00002648 File Offset: 0x00000848
		private static void Render(object sender, PaintEventArgs e)
		{
			MainGame.Update(e.Graphics);
		}

		// Token: 0x0600006C RID: 108 RVA: 0x00002655 File Offset: 0x00000855
		public static void OpenSubform(Form f)
		{
			Program.mainForm.IsMdiContainer = true;
			f.MdiParent = Program.mainForm;
			f.Show();
		}

		// Token: 0x04000058 RID: 88
		public const int GWL_EXSTYLE = -20;

		// Token: 0x04000059 RID: 89
		private const int WS_EX_LAYERED = 524288;

		// Token: 0x0400005A RID: 90
		private const int WS_EX_TRANSPARENT = 32;

		// Token: 0x0400005B RID: 91
		private const int LWA_ALPHA = 2;

		// Token: 0x0400005C RID: 92
		private const int LWA_COLORKEY = 1;

		// Token: 0x0400005D RID: 93
		private static IntPtr OriginalWindowStyle;

		// Token: 0x0400005E RID: 94
		private static IntPtr PassthruWindowStyle;

		// Token: 0x0400005F RID: 95
		private static BufferedPanel canvas;

		// Token: 0x04000060 RID: 96
		public static Color ColorKey = Color.Coral;

		// Token: 0x04000061 RID: 97
		public static Form mainForm;

		// Token: 0x02000023 RID: 35
		public struct NativeMessage
		{
			// Token: 0x040000E1 RID: 225
			public IntPtr Handle;

			// Token: 0x040000E2 RID: 226
			public uint Message;

			// Token: 0x040000E3 RID: 227
			public IntPtr WParameter;

			// Token: 0x040000E4 RID: 228
			public IntPtr LParameter;

			// Token: 0x040000E5 RID: 229
			public uint Time;

			// Token: 0x040000E6 RID: 230
			public Point Location;
		}
	}
}
