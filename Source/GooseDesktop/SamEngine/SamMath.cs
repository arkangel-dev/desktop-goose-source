using System;

namespace SamEngine
{
	// Token: 0x02000004 RID: 4
	public static class SamMath
	{
		// Token: 0x06000023 RID: 35 RVA: 0x00002272 File Offset: 0x00000472
		public static float RandomRange(float min, float max)
		{
			return min + (float)SamMath.Rand.NextDouble() * (max - min);
		}

		// Token: 0x06000024 RID: 36 RVA: 0x00002285 File Offset: 0x00000485
		public static float Lerp(float a, float b, float p)
		{
			return a * (1f - p) + b * p;
		}

		// Token: 0x06000025 RID: 37 RVA: 0x00002294 File Offset: 0x00000494
		public static float Clamp(float a, float min, float max)
		{
			return Math.Min(Math.Max(a, min), max);
		}

		// Token: 0x04000007 RID: 7
		public const float Deg2Rad = 0.0174532924f;

		// Token: 0x04000008 RID: 8
		public const float Rad2Deg = 57.2957764f;

		// Token: 0x04000009 RID: 9
		public static Random Rand = new Random();
	}
}
