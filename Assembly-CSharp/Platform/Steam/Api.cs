using System;
using System.Text;
using Steamworks;

namespace Platform.Steam
{
	public class Api : IPlatformApi
	{
		public EApiStatus ClientApiStatus
		{
			get; [PublicizedFrom(EAccessModifier.Private)]
			set;
		} = EApiStatus.Uninitialized;

		public event Action ClientApiInitialized
		{
			add
			{
				lock (this)
				{
					this.clientApiInitialized = (Action)Delegate.Combine(this.clientApiInitialized, value);
					if (this.ClientApiStatus == EApiStatus.Ok)
					{
						value();
					}
				}
			}
			remove
			{
				lock (this)
				{
					this.clientApiInitialized = (Action)Delegate.Remove(this.clientApiInitialized, value);
				}
			}
		}

		public void Init(IPlatform _owner)
		{
		}

		public bool InitClientApis()
		{
			if (!Packsize.Test())
			{
				Log.Out("[Steamworks.NET] Packsize Test returned false, the wrong version of Steamworks.NET is being run in this platform.");
				this.ClientApiStatus = EApiStatus.PermanentError;
				return false;
			}
			if (!DllCheck.Test())
			{
				Log.Out("[Steamworks.NET] DllCheck Test returned false, One or more of the Steamworks binaries seems to be the wrong version.");
				this.ClientApiStatus = EApiStatus.PermanentError;
				return false;
			}
			try
			{
				if (!SteamAPI.Init())
				{
					Log.Out("[Steamworks.NET] SteamAPI_Init() failed. Refer to Valve's documentation or the comment above this line for more information.");
					this.ClientApiStatus = EApiStatus.TemporaryError;
					return false;
				}
			}
			catch (DllNotFoundException ex)
			{
				string text = "[Steamworks.NET] Could not load [lib]steam_api.dll/so/dylib. It's likely not in the correct location. Refer to the README for more details.\n";
				DllNotFoundException ex2 = ex;
				Log.Out(text + ((ex2 != null) ? ex2.ToString() : null));
				this.ClientApiStatus = EApiStatus.PermanentError;
				return false;
			}
			Log.Out("[Steamworks.NET] SteamAPI_Init() ok");
			SteamClient.SetWarningMessageHook(new SteamAPIWarningMessageHook_t(this.ExceptionThrown));
			SteamUtils.SetOverlayNotificationPosition(1);
			this.ClientApiStatus = EApiStatus.Ok;
			Action action = this.clientApiInitialized;
			if (action != null)
			{
				action();
			}
			return true;
		}

		public bool InitServerApis()
		{
			return true;
		}

		public void ServerApiLoaded()
		{
			if (this.ClientApiStatus != EApiStatus.Ok)
			{
				Action action = this.clientApiInitialized;
				if (action == null)
				{
					return;
				}
				action();
			}
		}

		public void Update()
		{
			if (this.ClientApiStatus == EApiStatus.Ok)
			{
				this.tickDurationStopwatch.Restart();
				SteamAPI.RunCallbacks();
				long num = this.tickDurationStopwatch.ElapsedMicroseconds / 1000L;
				if (num > 25L)
				{
					Log.Warning(string.Format("[Steam] Tick took exceptionally long: {0} ms", num));
				}
			}
		}

		public void Destroy()
		{
			if (this.ClientApiStatus == EApiStatus.Ok)
			{
				SteamAPI.Shutdown();
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void ExceptionThrown(int _severity, StringBuilder _message)
		{
			Log.Error("[Steamworks.NET] " + ((_severity == 0) ? "Info: " : "Warning: ") + ": " + ((_message != null) ? _message.ToString() : null));
		}

		public float GetScreenBoundsValueFromSystem()
		{
			return 1f;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public Action clientApiInitialized;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly MicroStopwatch tickDurationStopwatch = new MicroStopwatch(false);
	}
}
