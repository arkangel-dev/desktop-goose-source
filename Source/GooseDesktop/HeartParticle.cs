using System;
using SamEngine;

namespace GooseDesktop
{
	// Token: 0x0200000E RID: 14
	internal struct HeartParticle
	{
		// Token: 0x04000054 RID: 84
		public Vector2 position;

		// Token: 0x04000055 RID: 85
		public float deathTime;

		// Token: 0x04000056 RID: 86
		private const float lifetime = 3f;

		// Token: 0x04000057 RID: 87
		private const float velY = 150f;
	}
}
