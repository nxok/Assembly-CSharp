using System;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Platform.Shared
{
	public class PlatformApplicationStandalone : IPlatformApplication
	{
		public Resolution[] SupportedResolutions
		{
			get
			{
				bool flag = false;
				if (this.lastResolutions != null)
				{
					Resolution[] resolutions = Screen.resolutions;
					if (this.lastResolutions.Length == resolutions.Length)
					{
						for (int i = 0; i < resolutions.Length; i++)
						{
							Resolution resolution = this.lastResolutions[i];
							Resolution resolution2 = resolutions[i];
							if (!resolution.Equals(resolution2))
							{
								flag = true;
								this.lastResolutions = resolutions;
								break;
							}
						}
					}
					else
					{
						this.lastResolutions = resolutions;
						flag = true;
					}
				}
				else
				{
					this.lastResolutions = Screen.resolutions;
					flag = true;
				}
				if (flag)
				{
					this.supportedResolutions = this.lastResolutions.Where((Resolution res) => res.width >= 640 && res.height >= 480).ToArray<Resolution>();
				}
				return this.supportedResolutions;
			}
		}

		[TupleElementNames(new string[] { "width", "height", "fullScreenMode" })]
		public ValueTuple<int, int, FullScreenMode> ScreenOptions
		{
			[return: TupleElementNames(new string[] { "width", "height", "fullScreenMode" })]
			get
			{
				FullScreenMode @int = PlayerPrefs.GetInt("Screenmanager Fullscreen mode", 3);
				if (PlayerPrefs.HasKey("Screenmanager Resolution Width") && PlayerPrefs.HasKey("Screenmanager Resolution Height"))
				{
					int int2 = PlayerPrefs.GetInt("Screenmanager Resolution Width");
					int int3 = PlayerPrefs.GetInt("Screenmanager Resolution Height");
					return new ValueTuple<int, int, FullScreenMode>(int2, int3, @int);
				}
				Resolution[] array = this.SupportedResolutions;
				if (array.Length > 1)
				{
					Resolution resolution = array[array.Length - 2];
					return new ValueTuple<int, int, FullScreenMode>(resolution.width, resolution.height, @int);
				}
				return new ValueTuple<int, int, FullScreenMode>(Screen.width, Screen.height, 3);
			}
		}

		public void SetResolution(int width, int height, FullScreenMode fullscreen)
		{
			if (width < 640 || height < 480 || width <= height)
			{
				fullscreen = 3;
				PlayerPrefs.SetInt("UnitySelectMonitor", 0);
			}
			if (height > width)
			{
				height = width;
			}
			PlayerPrefs.SetInt("Screenmanager Resolution Width", width);
			PlayerPrefs.SetInt("Screenmanager Resolution Height", height);
			PlayerPrefs.SetInt("Screenmanager Fullscreen mode", fullscreen);
			Screen.SetResolution(width, height, fullscreen);
		}

		public string temporaryCachePath
		{
			get
			{
				return Application.temporaryCachePath;
			}
		}

		public void RestartProcess(params string[] argv)
		{
			throw new NotImplementedException();
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public const string prefResolutionWidth = "Screenmanager Resolution Width";

		[PublicizedFrom(EAccessModifier.Private)]
		public const string prefResolutionHeight = "Screenmanager Resolution Height";

		[PublicizedFrom(EAccessModifier.Private)]
		public const string prefFullscreen = "Screenmanager Fullscreen mode";

		[PublicizedFrom(EAccessModifier.Private)]
		public const int minResWidth = 640;

		[PublicizedFrom(EAccessModifier.Private)]
		public const int minResHeight = 480;

		[PublicizedFrom(EAccessModifier.Private)]
		public Resolution[] lastResolutions;

		[PublicizedFrom(EAccessModifier.Private)]
		public Resolution[] supportedResolutions;
	}
}
