using System;
using System.IO;
using System.Media;
using System.Runtime.InteropServices;
using System.Text;
using GooseDesktop.Properties;
using SamEngine;

namespace GooseDesktop
{
	// Token: 0x0200000B RID: 11
	internal static class Sound
	{
		// Token: 0x06000042 RID: 66 RVA: 0x000031F0 File Offset: 0x000013F0
		public static void Init()
		{
			Sound.honkBiteSoundPlayer = new Sound.Mp3Player(Sound.honkSources[0], "honkPlayer");
			Sound.patSoundPool = new SoundPlayer[Sound.patSources.Length];
			for (int i = 0; i < Sound.patSources.Length; i++)
			{
				Sound.patSoundPool[i] = new SoundPlayer(Sound.patSources[i]);
				Sound.patSoundPool[i].Load();
			}
			Sound.environmentSoundsPlayer = new Sound.Mp3Player(Program.GetPathToFileInAssembly("Assets/Sound/NotEmbedded/MudSquith.mp3"), "assortedEnvironment");
			string pathToFileInAssembly = Program.GetPathToFileInAssembly("Assets/Sound/Music/Music.mp3");
			if (File.Exists(pathToFileInAssembly))
			{
				Sound.musicPlayer = new Sound.Mp3Player(pathToFileInAssembly, "musicPlayer");
				Sound.musicPlayer.loop = true;
				Sound.musicPlayer.SetVolume(0.5f);
				Sound.musicPlayer.Play();
			}
		}

		// Token: 0x06000043 RID: 67 RVA: 0x000032B8 File Offset: 0x000014B8
		public static void PlayPat()
		{
			int num = (int)(SamMath.Rand.NextDouble() * (double)Sound.patSoundPool.Length);
			SoundPlayer soundPlayer = Sound.patSoundPool[num];
			if (soundPlayer.Stream.CanSeek)
			{
				soundPlayer.Stream.Seek(0L, SeekOrigin.Begin);
			}
			soundPlayer.Play();
		}

		// Token: 0x06000044 RID: 68 RVA: 0x0000330C File Offset: 0x0000150C
		public static void HONCC()
		{
			int num = (int)(SamMath.Rand.NextDouble() * (double)Sound.honkSources.Length);
			Sound.honkBiteSoundPlayer.Pause();
			Sound.honkBiteSoundPlayer.Dispose();
			Sound.honkBiteSoundPlayer.ChangeFile(Sound.honkSources[num]);
			Sound.honkBiteSoundPlayer.SetVolume(0.8f);
			Sound.honkBiteSoundPlayer.Play();
		}

		// Token: 0x06000045 RID: 69 RVA: 0x000024D6 File Offset: 0x000006D6
		public static void CHOMP()
		{
			Sound.honkBiteSoundPlayer.Pause();
			Sound.honkBiteSoundPlayer.Dispose();
			Sound.honkBiteSoundPlayer.ChangeFile(Sound.biteSource);
			Sound.honkBiteSoundPlayer.SetVolume(0.07f);
			Sound.honkBiteSoundPlayer.Play();
		}

		// Token: 0x06000046 RID: 70 RVA: 0x00002514 File Offset: 0x00000714
		public static void PlayMudSquith()
		{
			Sound.environmentSoundsPlayer.Restart();
			Sound.environmentSoundsPlayer.Play();
		}

		// Token: 0x04000015 RID: 21
		public static Sound.Mp3Player honkBiteSoundPlayer;

		// Token: 0x04000016 RID: 22
		public static Sound.Mp3Player musicPlayer;

		// Token: 0x04000017 RID: 23
		public static Sound.Mp3Player environmentSoundsPlayer;

		// Token: 0x04000018 RID: 24
		private static readonly Stream[] patSources = new Stream[]
		{
			Resources.Pat1,
			Resources.Pat2,
			Resources.Pat3
		};

		// Token: 0x04000019 RID: 25
		private static SoundPlayer[] patSoundPool;

		// Token: 0x0400001A RID: 26
		private static readonly string[] honkSources = new string[]
		{
			Program.GetPathToFileInAssembly("Assets/Sound/NotEmbedded/Honk1.mp3"),
			Program.GetPathToFileInAssembly("Assets/Sound/NotEmbedded/Honk2.mp3"),
			Program.GetPathToFileInAssembly("Assets/Sound/NotEmbedded/Honk3.mp3"),
			Program.GetPathToFileInAssembly("Assets/Sound/NotEmbedded/Honk4.mp3")
		};

		// Token: 0x0400001B RID: 27
		private static readonly string biteSource = Program.GetPathToFileInAssembly("Assets/Sound/NotEmbedded/BITE.mp3");

		// Token: 0x02000016 RID: 22
		public class Mp3Player
		{
			// Token: 0x0600007D RID: 125 RVA: 0x00002759 File Offset: 0x00000959
			public Mp3Player(string filename, string playerAlias)
			{
				this.alias = playerAlias;
				Sound.Mp3Player.mciSendString(string.Format("open \"{0}\" type MPEGVideo alias {1}", filename, this.alias), null, 0, IntPtr.Zero);
			}

			// Token: 0x0600007E RID: 126 RVA: 0x000052B8 File Offset: 0x000034B8
			public void Play()
			{
				string text = "play {0}";
				text = string.Format(text, this.alias);
				if (this.loop)
				{
					text += " REPEAT";
				}
				Sound.Mp3Player.mciSendString(text, null, 0, IntPtr.Zero);
			}

			// Token: 0x0600007F RID: 127 RVA: 0x00002786 File Offset: 0x00000986
			public void Pause()
			{
				Sound.Mp3Player.mciSendString(string.Format("stop {0}", this.alias), null, 0, IntPtr.Zero);
			}

			// Token: 0x06000080 RID: 128 RVA: 0x000052FC File Offset: 0x000034FC
			public void SetVolume(float volume)
			{
				int num = (int)Math.Max(Math.Min(volume * 1000f, 1000f), 0f);
				Sound.Mp3Player.mciSendString(string.Format("setaudio {0} volume to {1}", this.alias, num), null, 0, IntPtr.Zero);
			}

			// Token: 0x06000081 RID: 129 RVA: 0x000027A5 File Offset: 0x000009A5
			public void Dispose()
			{
				Sound.Mp3Player.mciSendString(string.Format("close {0}", this.alias), null, 0, IntPtr.Zero);
			}

			// Token: 0x06000082 RID: 130 RVA: 0x000027C4 File Offset: 0x000009C4
			public void ChangeFile(string newFilePath)
			{
				Sound.Mp3Player.mciSendString(string.Format("open \"{0}\" type MPEGVideo alias {1}", newFilePath, this.alias), null, 0, IntPtr.Zero);
			}

			// Token: 0x06000083 RID: 131 RVA: 0x000027E4 File Offset: 0x000009E4
			public void Restart()
			{
				Sound.Mp3Player.mciSendString(string.Format("seek {0} to start", this.alias), null, 0, IntPtr.Zero);
			}

			// Token: 0x06000084 RID: 132
			[DllImport("winmm.dll")]
			private static extern long mciSendString(string strCommand, StringBuilder strReturn, int iReturnLength, IntPtr hWndCallback);

			// Token: 0x0400008B RID: 139
			public bool loop;

			// Token: 0x0400008C RID: 140
			private string alias;
		}
	}
}
