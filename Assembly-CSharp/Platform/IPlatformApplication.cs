using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Platform.Shared;
using UnityEngine;

namespace Platform
{
	public interface IPlatformApplication
	{
		Resolution[] SupportedResolutions { get; }

		[TupleElementNames(new string[] { "width", "height", "fullScreenMode" })]
		ValueTuple<int, int, FullScreenMode> ScreenOptions
		{
			[return: TupleElementNames(new string[] { "width", "height", "fullScreenMode" })]
			get;
		}

		void SetResolution(int width, int height, FullScreenMode fullscreen);

		string temporaryCachePath { get; }

		void RestartProcess(params string[] argv);

		public static IPlatformApplication Create()
		{
			return new PlatformApplicationStandalone();
		}

		public static string JoinAndEscapeArgv(params string[] args)
		{
			if (args != null)
			{
				return string.Join<string>(' ', args.Select(new Func<string, string>(IPlatformApplication.EscapeArg)));
			}
			return null;
		}

		public static string EscapeArg(string arg)
		{
			if (arg.Length > 0 && arg.AsSpan().IndexOfAny(" \t\n\v\"") < 0)
			{
				return arg;
			}
			if (arg.Length <= 0)
			{
				return "\"\"";
			}
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append('"');
			int num = 0;
			int num2;
			for (;;)
			{
				num2 = 0;
				while (num < arg.Length && arg[num] == '\\')
				{
					num++;
					num2++;
				}
				if (num >= arg.Length)
				{
					break;
				}
				if (arg[num] == '"')
				{
					stringBuilder.Append('\\', num2 * 2 + 1);
					stringBuilder.Append(arg[num]);
				}
				else
				{
					stringBuilder.Append('\\', num2);
					stringBuilder.Append(arg[num]);
				}
				num++;
			}
			stringBuilder.Append('\\', num2 * 2);
			stringBuilder.Append('"');
			return stringBuilder.ToString();
		}

		RefreshRate GetCurrentRefreshRate()
		{
			return Screen.currentResolution.refreshRateRatio;
		}

		Resolution GetCurrentResolution()
		{
			return Screen.currentResolution;
		}

		EnumGamePrefs VSyncCountPref
		{
			get
			{
				return EnumGamePrefs.OptionsGfxVsync;
			}
		}
	}
}
