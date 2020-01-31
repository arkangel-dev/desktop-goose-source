using System;
using System.Diagnostics;

namespace SamEngine
{
	// Token: 0x02000003 RID: 3
	public static class Time
	{
		// Token: 0x06000021 RID: 33 RVA: 0x00002257 File Offset: 0x00000457
		static Time()
		{
			Time.timeStopwatch.Start();
			Time.TickTime();
		}

		// Token: 0x06000022 RID: 34 RVA: 0x00002E68 File Offset: 0x00001068
		public static void TickTime()
		{
			Time.time = (float)Time.timeStopwatch.Elapsed.TotalSeconds;
		}

		// Token: 0x04000003 RID: 3
		public const int framerate = 120;

		// Token: 0x04000004 RID: 4
		public const float deltaTime = 0.008333334f;

		// Token: 0x04000005 RID: 5
		public static Stopwatch timeStopwatch = new Stopwatch();

		// Token: 0x04000006 RID: 6
		public static float time;
	}
}
