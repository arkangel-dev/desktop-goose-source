using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace GooseDesktop
{
	// Token: 0x02000009 RID: 9
	public static class GooseConfig
	{
		// Token: 0x0600003D RID: 61 RVA: 0x0000248D File Offset: 0x0000068D
		public static void LoadConfig()
		{
			GooseConfig.settings = GooseConfig.ConfigSettings.ReadFileIntoConfig(GooseConfig.filePath);
		}

		// Token: 0x04000010 RID: 16
		private static string filePath = Program.GetPathToFileInAssembly("config.goos");

		// Token: 0x04000011 RID: 17
		public const int GOOSE_CONFIG_VERSION = 0;

		// Token: 0x04000012 RID: 18
		public static GooseConfig.ConfigSettings settings = null;

		// Token: 0x02000015 RID: 21
		public class ConfigSettings
		{
			// Token: 0x06000079 RID: 121 RVA: 0x0000505C File Offset: 0x0000325C
			public static GooseConfig.ConfigSettings ReadFileIntoConfig(string configGivenPath)
			{
				GooseConfig.ConfigSettings configSettings = new GooseConfig.ConfigSettings();
				if (!File.Exists(configGivenPath))
				{
					MessageBox.Show("Can't find config.goos file! Creating a new one with default values");
					GooseConfig.ConfigSettings.WriteConfigToFile(configGivenPath, configSettings);
					return configSettings;
				}
				try
				{
					using (StreamReader streamReader = new StreamReader(configGivenPath))
					{
						Dictionary<string, string> dictionary = new Dictionary<string, string>();
						string text;
						while ((text = streamReader.ReadLine()) != null)
						{
							string[] array = text.Split(new char[]
							{
								'='
							});
							if (array.Length == 2)
							{
								dictionary.Add(array[0], array[1]);
							}
						}
						int num = -1;
						int.TryParse(dictionary["Version"], out num);
						if (num != 0)
						{
							MessageBox.Show("config.goos is for the wrong version! Creating a new one with default values!");
							File.Delete(configGivenPath);
							GooseConfig.ConfigSettings.WriteConfigToFile(configGivenPath, configSettings);
							return configSettings;
						}
						foreach (KeyValuePair<string, string> keyValuePair in dictionary)
						{
							FieldInfo field = typeof(GooseConfig.ConfigSettings).GetField(keyValuePair.Key);
							try
							{
								field.SetValue(configSettings, Convert.ChangeType(keyValuePair.Value, field.FieldType));
							}
							catch
							{
								MessageBox.Show("Loading config error: field " + field.Name + "'s value is not valid. Setting it to the default value.");
							}
						}
					}
				}
				catch
				{
					MessageBox.Show("config.goos corrupt! Creating a new one!");
					File.Delete(configGivenPath);
					GooseConfig.ConfigSettings.WriteConfigToFile(configGivenPath, configSettings);
					return configSettings;
				}
				return configSettings;
			}

			// Token: 0x0600007A RID: 122 RVA: 0x0000521C File Offset: 0x0000341C
			public static void WriteConfigToFile(string path, GooseConfig.ConfigSettings f)
			{
				using (StreamWriter streamWriter = File.CreateText(path))
				{
					streamWriter.Write(GooseConfig.ConfigSettings.GenerateTextFromSettings(f));
				}
			}

			// Token: 0x0600007B RID: 123 RVA: 0x00005258 File Offset: 0x00003458
			public static string GenerateTextFromSettings(GooseConfig.ConfigSettings f)
			{
				StringBuilder stringBuilder = new StringBuilder();
				foreach (FieldInfo fieldInfo in typeof(GooseConfig.ConfigSettings).GetFields())
				{
					stringBuilder.Append(string.Format("{0}={1}\n", fieldInfo.Name, fieldInfo.GetValue(f).ToString()));
				}
				return stringBuilder.ToString();
			}

			// Token: 0x04000086 RID: 134
			public int Version;

			// Token: 0x04000087 RID: 135
			public bool CanAttackAtRandom;

			// Token: 0x04000088 RID: 136
			public float MinWanderingTimeSeconds = 20f;

			// Token: 0x04000089 RID: 137
			public float MaxWanderingTimeSeconds = 40f;

			// Token: 0x0400008A RID: 138
			public float FirstWanderTimeSeconds = 20f;
		}
	}
}
