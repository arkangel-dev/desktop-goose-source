using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using SamEngine;

namespace GooseDesktop
{
	// Token: 0x0200000A RID: 10
	public static class MainGame
	{
		// Token: 0x0600003F RID: 63 RVA: 0x00003018 File Offset: 0x00001218
		public static void Init()
		{
			string pathToFileInAssembly = Program.GetPathToFileInAssembly("Assets/Images/Memes/");
			try
			{
				Directory.GetFiles(pathToFileInAssembly);
			}
			catch
			{
				MessageBox.Show("Warning: Some assets expected at the path: \n\n'" + pathToFileInAssembly + "' \n\ncannot be found. \n\nYour .exe should ideally be next to an Assets folder and config, all bundled together!\n\nPlease make sure you extracted the zip file, with the whole folder together, to a known location like Documents or Desktop- and we didn't end up somewhere random like AppData.\n\nGoose will still work, but he won't be able to use custom memes or any of that fanciness.\nHold ESC for several seconds to quit.");
			}
			GooseConfig.LoadConfig();
			Sound.Init();
			TheGoose.Init();
		}

		// Token: 0x06000040 RID: 64 RVA: 0x00003074 File Offset: 0x00001274
		public static void Update(Graphics g)
		{
			Time.TickTime();
			if (Program.GetAsyncKeyState(Keys.Escape) != 0)
			{
				MainGame.curQuitAlpha += 0.00216666679f;
			}
			else
			{
				MainGame.curQuitAlpha -= 0.0166666675f;
			}
			MainGame.curQuitAlpha = SamMath.Clamp(MainGame.curQuitAlpha, 0f, 1f);
			if (MainGame.curQuitAlpha > 0.2f)
			{
				float num = (MainGame.curQuitAlpha - 0.2f) / 0.8f;
				int num2 = (int)SamMath.Lerp(-15f, 10f, Easings.ExponentialEaseOut(num * 2f));
				SizeF sizeF = g.MeasureString("Continue Holding ESC to evict goose", MainGame.showCurQuitFont, int.MaxValue);
				g.FillRectangle(Brushes.LightBlue, new Rectangle(5, num2 - 5, (int)sizeF.Width + 10, (int)sizeF.Height + 10));
				g.FillRectangle(Brushes.LightPink, new Rectangle(5, num2 - 5, (int)SamMath.Lerp(0f, sizeF.Width + 10f, num), (int)sizeF.Height + 10));
				SolidBrush solidBrush = new SolidBrush(Color.FromArgb(255, (int)(256f * MainGame.curQuitAlpha), (int)(256f * MainGame.curQuitAlpha), (int)(256f * MainGame.curQuitAlpha)));
				g.DrawString("Continue holding ESC to evict goose", MainGame.showCurQuitFont, solidBrush, 10f, (float)num2);
				solidBrush.Dispose();
			}
			if (MainGame.curQuitAlpha > 0.99f)
			{
				Application.Exit();
			}
			TheGoose.Tick();
			TheGoose.Render(g);
		}

		// Token: 0x04000013 RID: 19
		private static float curQuitAlpha = 0f;

		// Token: 0x04000014 RID: 20
		private static Font showCurQuitFont = new Font("Arial", 12f, FontStyle.Bold);
	}
}
