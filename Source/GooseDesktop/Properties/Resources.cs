using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Resources;
using System.Runtime.CompilerServices;

namespace GooseDesktop.Properties
{
	// Token: 0x02000011 RID: 17
	[CompilerGenerated]
	[GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
	[DebuggerNonUserCode]
	internal class Resources
	{
		// Token: 0x0600006F RID: 111 RVA: 0x00002456 File Offset: 0x00000656
		internal Resources()
		{
		}

		// Token: 0x17000001 RID: 1
		// (get) Token: 0x06000070 RID: 112 RVA: 0x0000268E File Offset: 0x0000088E
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		internal static ResourceManager ResourceManager
		{
			get
			{
				if (Resources.resourceMan == null)
				{
					Resources.resourceMan = new ResourceManager("GooseDesktop.Properties.Resources", typeof(Resources).Assembly);
				}
				return Resources.resourceMan;
			}
		}

		// Token: 0x17000002 RID: 2
		// (get) Token: 0x06000071 RID: 113 RVA: 0x000026BA File Offset: 0x000008BA
		// (set) Token: 0x06000072 RID: 114 RVA: 0x000026C1 File Offset: 0x000008C1
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		internal static CultureInfo Culture
		{
			get
			{
				return Resources.resourceCulture;
			}
			set
			{
				Resources.resourceCulture = value;
			}
		}

		// Token: 0x17000003 RID: 3
		// (get) Token: 0x06000073 RID: 115 RVA: 0x000026C9 File Offset: 0x000008C9
		internal static UnmanagedMemoryStream Pat1
		{
			get
			{
				return Resources.ResourceManager.GetStream("Pat1", Resources.resourceCulture);
			}
		}

		// Token: 0x17000004 RID: 4
		// (get) Token: 0x06000074 RID: 116 RVA: 0x000026DF File Offset: 0x000008DF
		internal static UnmanagedMemoryStream Pat2
		{
			get
			{
				return Resources.ResourceManager.GetStream("Pat2", Resources.resourceCulture);
			}
		}

		// Token: 0x17000005 RID: 5
		// (get) Token: 0x06000075 RID: 117 RVA: 0x000026F5 File Offset: 0x000008F5
		internal static UnmanagedMemoryStream Pat3
		{
			get
			{
				return Resources.ResourceManager.GetStream("Pat3", Resources.resourceCulture);
			}
		}

		// Token: 0x04000062 RID: 98
		private static ResourceManager resourceMan;

		// Token: 0x04000063 RID: 99
		private static CultureInfo resourceCulture;
	}
}
