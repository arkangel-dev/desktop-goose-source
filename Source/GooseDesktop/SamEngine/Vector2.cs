using System;

namespace SamEngine
{
	// Token: 0x02000006 RID: 6
	public struct Vector2
	{
		// Token: 0x0600002A RID: 42 RVA: 0x00002303 File Offset: 0x00000503
		public Vector2(float _x, float _y)
		{
			this.x = _x;
			this.y = _y;
		}

		// Token: 0x0600002B RID: 43 RVA: 0x00002313 File Offset: 0x00000513
		public static Vector2 operator +(Vector2 a, Vector2 b)
		{
			return new Vector2(a.x + b.x, a.y + b.y);
		}

		// Token: 0x0600002C RID: 44 RVA: 0x00002334 File Offset: 0x00000534
		public static Vector2 operator -(Vector2 a, Vector2 b)
		{
			return new Vector2(a.x - b.x, a.y - b.y);
		}

		// Token: 0x0600002D RID: 45 RVA: 0x00002355 File Offset: 0x00000555
		public static Vector2 operator -(Vector2 a)
		{
			return a * -1f;
		}

		// Token: 0x0600002E RID: 46 RVA: 0x00002362 File Offset: 0x00000562
		public static Vector2 operator *(Vector2 a, Vector2 b)
		{
			return new Vector2(a.x * b.x, a.y * b.y);
		}

		// Token: 0x0600002F RID: 47 RVA: 0x00002383 File Offset: 0x00000583
		public static Vector2 operator *(Vector2 a, float b)
		{
			return new Vector2(a.x * b, a.y * b);
		}

		// Token: 0x06000030 RID: 48 RVA: 0x0000239A File Offset: 0x0000059A
		public static Vector2 operator /(Vector2 a, float b)
		{
			return new Vector2(a.x / b, a.y / b);
		}

		// Token: 0x06000031 RID: 49 RVA: 0x000023B1 File Offset: 0x000005B1
		public static Vector2 GetFromAngleDegrees(float angle)
		{
			return new Vector2((float)Math.Cos((double)(angle * 0.0174532924f)), (float)Math.Sin((double)(angle * 0.0174532924f)));
		}

		// Token: 0x06000032 RID: 50 RVA: 0x00002EEC File Offset: 0x000010EC
		public static float Distance(Vector2 a, Vector2 b)
		{
			Vector2 vector = new Vector2(a.x - b.x, a.y - b.y);
			return (float)Math.Sqrt((double)(vector.x * vector.x + vector.y * vector.y));
		}

		// Token: 0x06000033 RID: 51 RVA: 0x000023D4 File Offset: 0x000005D4
		public static Vector2 Lerp(Vector2 a, Vector2 b, float p)
		{
			return new Vector2(SamMath.Lerp(a.x, b.x, p), SamMath.Lerp(a.y, b.y, p));
		}

		// Token: 0x06000034 RID: 52 RVA: 0x000023FF File Offset: 0x000005FF
		public static float Dot(Vector2 a, Vector2 b)
		{
			return a.x * b.x + a.y * b.y;
		}

		// Token: 0x06000035 RID: 53 RVA: 0x00002F3C File Offset: 0x0000113C
		public static Vector2 Normalize(Vector2 a)
		{
			if (a.x == 0f && a.y == 0f)
			{
				return Vector2.zero;
			}
			float num = (float)Math.Sqrt((double)(a.x * a.x + a.y * a.y));
			return new Vector2(a.x / num, a.y / num);
		}

		// Token: 0x06000036 RID: 54 RVA: 0x0000241C File Offset: 0x0000061C
		public static float Magnitude(Vector2 a)
		{
			return (float)Math.Sqrt((double)(a.x * a.x + a.y * a.y));
		}

		// Token: 0x0400000C RID: 12
		public float x;

		// Token: 0x0400000D RID: 13
		public float y;

		// Token: 0x0400000E RID: 14
		public static readonly Vector2 zero = new Vector2(0f, 0f);
	}
}
