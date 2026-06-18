using System;
using System.IO;
using System.Text;
using System.Threading;
using InControl;
using UnityEngine;

namespace Platform.Shared
{
	public class Utils : IUtils
	{
		public virtual void Init(IPlatform _owner)
		{
		}

		public virtual bool OpenBrowser(string _url)
		{
			return Utils.OpenSystemBrowser(_url);
		}

		public void ControllerDisconnected(InputDevice inputDevice)
		{
		}

		public virtual string GetPlatformLanguage()
		{
			if (this.platformLanguageCache == null)
			{
				string text = Application.systemLanguage.ToStringCached<SystemLanguage>().ToLower();
				string text2;
				if (!(text == "chinesesimplified"))
				{
					if (!(text == "chinesetraditional"))
					{
						if (!(text == "korean"))
						{
							text2 = text;
						}
						else
						{
							text2 = "koreana";
						}
					}
					else
					{
						text2 = "tchinese";
					}
				}
				else
				{
					text2 = "schinese";
				}
				text = text2;
				this.platformLanguageCache = text;
			}
			return this.platformLanguageCache;
		}

		public virtual string GetAppLanguage()
		{
			return this.GetPlatformLanguage();
		}

		public virtual string GetCountry()
		{
			return "??";
		}

		public virtual void ClearTempFiles()
		{
			Utils.TryDeleteTempCacheContents();
		}

		public virtual string GetTempFileName(string prefix = "", string suffix = "")
		{
			return Utils.GetRandomTempCacheFileName(prefix, suffix);
		}

		[PublicizedFrom(EAccessModifier.Internal)]
		public static void TryDeleteTempCacheContents()
		{
			Utils.TryDeleteTempDirectoryContentsExceptCrashes(PlatformApplicationManager.Application.temporaryCachePath);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static void TryDeleteTempDirectoryContentsExceptCrashes(string path)
		{
			try
			{
				if (Directory.Exists(path))
				{
					foreach (FileSystemInfo fileSystemInfo in new DirectoryInfo(path).EnumerateFileSystemInfos())
					{
						try
						{
							if (!fileSystemInfo.Name.EqualsCaseInsensitive("Crashes"))
							{
								DirectoryInfo directoryInfo = fileSystemInfo as DirectoryInfo;
								if (directoryInfo != null)
								{
									directoryInfo.Delete(true);
								}
								else
								{
									fileSystemInfo.Delete();
								}
							}
						}
						catch (Exception ex)
						{
							Log.Warning(string.Concat(new string[]
							{
								"[Platform.Shared.Utils] Could not delete '",
								fileSystemInfo.Name,
								"' from temp cache. ",
								ex.GetType().FullName,
								": ",
								ex.Message
							}));
						}
					}
				}
			}
			catch (Exception ex2)
			{
				Log.Warning("[Platform.Shared.Utils] Could not delete contents of temp cache. " + ex2.GetType().FullName + ": " + ex2.Message);
			}
		}

		[PublicizedFrom(EAccessModifier.Internal)]
		public static string GetRandomTempCacheFileName(string prefix, string suffix)
		{
			return Utils.GetRandomFileName(PlatformApplicationManager.Application.temporaryCachePath, prefix, suffix);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static string GetRandomFileName(string parentDir, string prefix, string suffix)
		{
			for (int i = 0; i < 100; i++)
			{
				string randomName = Utils.GetRandomName(prefix, suffix);
				string text = Path.Join(parentDir, randomName);
				if (!File.Exists(text))
				{
					using (File.Open(text, FileMode.OpenOrCreate))
					{
						return text;
					}
				}
			}
			throw new IOException(string.Format("Failed to create a temporary file after {0} attempts.", 100));
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static string GetRandomName(string prefix, string suffix)
		{
			Random value = Utils.RandLocal.Value;
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(prefix);
			for (int i = 0; i < 16; i++)
			{
				stringBuilder.Append("0123456789ABCDEFGHIJKLMNOPabcdefghijklmnop"[value.Next("0123456789ABCDEFGHIJKLMNOPabcdefghijklmnop".Length)]);
			}
			stringBuilder.Append(suffix);
			return stringBuilder.ToString();
		}

		public virtual string GetCrossplayPlayerIcon(EPlayGroup _playGroup, bool _fetchGenericIcons, EPlatformIdentifier _nativePlatform)
		{
			return string.Empty;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static int Seed = Environment.TickCount;

		public static readonly ThreadLocal<Random> RandLocal = new ThreadLocal<Random>(() => new Random(Interlocked.Increment(ref Utils.Seed)));

		[PublicizedFrom(EAccessModifier.Private)]
		public string platformLanguageCache;
	}
}
