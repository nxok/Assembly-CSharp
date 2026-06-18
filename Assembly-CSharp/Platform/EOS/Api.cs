using System;
using System.IO;
using Epic.OnlineServices;
using Epic.OnlineServices.Connect;
using Epic.OnlineServices.Logging;
using Epic.OnlineServices.Platform;
using Epic.OnlineServices.Sanctions;
using UnityEngine;

namespace Platform.EOS
{
	public class Api : IPlatformApi
	{
		[PublicizedFrom(EAccessModifier.Private)]
		static Api()
		{
			string launchArgument = GameUtils.GetLaunchArgument("debugeos");
			if (launchArgument != null)
			{
				if (launchArgument == "verbose")
				{
					Api.DebugLevel = Api.EDebugLevel.Verbose;
					return;
				}
				Api.DebugLevel = Api.EDebugLevel.Normal;
			}
		}

		public void Init(IPlatform _owner)
		{
			this.owner = _owner;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void OnApplicationStateChanged(ApplicationState _applicationState)
		{
			if (this.PlatformInterface == null)
			{
				return;
			}
			ApplicationStatus applicationStatus;
			if (_applicationState != ApplicationState.Foreground)
			{
				if (_applicationState != ApplicationState.Suspended)
				{
					throw new ArgumentOutOfRangeException("_applicationState", _applicationState, "[EOS] OnApplicationStateChanged: ApplicationState is missing a conversion to a EOS.ApplicationStatus");
				}
				applicationStatus = 2;
			}
			else
			{
				applicationStatus = 3;
			}
			object lockObject = AntiCheatCommon.LockObject;
			lock (lockObject)
			{
				this.PlatformInterface.SetApplicationStatus(applicationStatus);
			}
		}

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

		public bool InitClientApis()
		{
			if (this.ClientApiStatus == EApiStatus.Ok)
			{
				return true;
			}
			EosCreds eosCreds = (GameManager.IsDedicatedServer ? EosCreds.ServerDeviceIdCredentials : EosCreds.ClientCredentials);
			this.initPlatform(eosCreds, eosCreds.ServerMode);
			return this.ClientApiStatus == EApiStatus.Ok;
		}

		public bool InitServerApis()
		{
			return this.InitClientApis();
		}

		public void ServerApiLoaded()
		{
		}

		public void Update()
		{
			if (this.ClientApiStatus != EApiStatus.Ok)
			{
				return;
			}
			this.tickDurationStopwatch.Restart();
			object lockObject = AntiCheatCommon.LockObject;
			lock (lockObject)
			{
				this.PlatformInterface.Tick();
			}
			long num = this.tickDurationStopwatch.ElapsedMicroseconds / 1000L;
			if (Api.DebugLevel != Api.EDebugLevel.Off && num > 5L)
			{
				Log.Warning(string.Format("[EOS] Tick took exceptionally long: {0} ms", num));
			}
		}

		public void Destroy()
		{
			if (this.ClientApiStatus != EApiStatus.Ok)
			{
				return;
			}
			this.ConnectInterface = null;
			object obj = AntiCheatCommon.LockObject;
			lock (obj)
			{
				this.PlatformInterface.Release();
			}
			this.PlatformInterface = null;
			obj = AntiCheatCommon.LockObject;
			lock (obj)
			{
				PlatformInterface.Shutdown();
			}
		}

		public float GetScreenBoundsValueFromSystem()
		{
			return 1f;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void initPlatform(EosCreds _creds, bool _serverMode)
		{
			InitializeOptions initializeOptions = default(InitializeOptions);
			initializeOptions.ProductName = "7 Days To Die";
			initializeOptions.ProductVersion = Constants.cVersionInformation.SerializableString;
			InitializeOptions initializeOptions2 = initializeOptions;
			Result result = 18;
			object obj;
			try
			{
				obj = AntiCheatCommon.LockObject;
				lock (obj)
				{
					result = PlatformInterface.Initialize(ref initializeOptions2);
				}
			}
			catch (DllNotFoundException ex)
			{
				this.ClientApiStatus = EApiStatus.PermanentError;
				Log.Error("[EOS] Native library or one of its dependencies not found (e.g. no Microsoft Visual C Redistributables 2022)");
				Log.Exception(ex);
				Application.Quit(1);
			}
			Log.Out(string.Format("[EOS] Initialize: {0}", result));
			LogLevel logLevel;
			switch (Api.DebugLevel)
			{
			case Api.EDebugLevel.Off:
				logLevel = 300;
				break;
			case Api.EDebugLevel.Normal:
				logLevel = 400;
				break;
			case Api.EDebugLevel.Verbose:
				logLevel = 600;
				break;
			default:
				throw new ArgumentOutOfRangeException("DebugLevel");
			}
			LogLevel logLevel2 = logLevel;
			obj = AntiCheatCommon.LockObject;
			lock (obj)
			{
				LoggingInterface.SetLogLevel(int.MaxValue, logLevel2);
				string launchArgument = GameUtils.GetLaunchArgument("debugeac");
				if (launchArgument != null)
				{
					LoggingInterface.SetLogLevel(24, (launchArgument == "verbose") ? 500 : 400);
				}
				else
				{
					LoggingInterface.SetLogLevel(24, 300);
				}
				if (logLevel2 == 600)
				{
					LoggingInterface.SetLogLevel(41, 500);
				}
				LoggingInterface.SetLogLevel(11, 200);
				LoggingInterface.SetLogLevel(12, 300);
				LoggingInterface.SetLogLevel(6, 200);
				LoggingInterface.SetLogLevel(1, 200);
				LoggingInterface.SetLogLevel(3, 300);
				LoggingInterface.SetLogLevel(14, 300);
				LoggingInterface.SetLogLevel(17, 300);
				LoggingInterface.SetCallback(new LogMessageFunc(this.logCallback));
			}
			this.PlatformInterface = this.createPlatformInterface(_creds, _serverMode);
			if (this.PlatformInterface == null)
			{
				this.ClientApiStatus = EApiStatus.PermanentError;
				Log.Error("[EOS] Failed to create platform");
				return;
			}
			IPlatform nativePlatform = PlatformManager.NativePlatform;
			if (((nativePlatform != null) ? nativePlatform.ApplicationState : null) != null)
			{
				PlatformManager.NativePlatform.ApplicationState.OnApplicationStateChanged += this.OnApplicationStateChanged;
			}
			obj = AntiCheatCommon.LockObject;
			lock (obj)
			{
				this.ConnectInterface = this.PlatformInterface.GetConnectInterface();
			}
			if (this.ConnectInterface == null)
			{
				this.ClientApiStatus = EApiStatus.PermanentError;
				Log.Error("[EOS] Failed to get connect interface");
				return;
			}
			obj = AntiCheatCommon.LockObject;
			lock (obj)
			{
				this.SanctionsInterface = this.PlatformInterface.GetSanctionsInterface();
			}
			if (this.SanctionsInterface == null)
			{
				this.ClientApiStatus = EApiStatus.PermanentError;
				Log.Error("[EOS] Failed to get sanctions interface");
				return;
			}
			this.ClientApiStatus = EApiStatus.Ok;
			Action action = this.clientApiInitialized;
			if (action == null)
			{
				return;
			}
			action();
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public PlatformInterface createPlatformInterface(EosCreds _creds, bool _serverMode)
		{
			WindowsOptions windowsOptions = default(WindowsOptions);
			windowsOptions.ProductId = _creds.ProductId;
			windowsOptions.SandboxId = _creds.SandboxId;
			ClientCredentials clientCredentials = default(ClientCredentials);
			clientCredentials.ClientId = _creds.ClientId;
			clientCredentials.ClientSecret = _creds.ClientSecret;
			windowsOptions.ClientCredentials = clientCredentials;
			windowsOptions.DeploymentId = _creds.DeploymentId;
			windowsOptions.EncryptionKey = "0000000000000000000000000000000000000000000000000000000000000000";
			windowsOptions.IsServer = _serverMode;
			windowsOptions.Flags = 2L;
			windowsOptions.Flags |= 4L;
			windowsOptions.RTCOptions = null;
			WindowsRTCOptions windowsRTCOptions = default(WindowsRTCOptions);
			WindowsRTCOptionsPlatformSpecificOptions windowsRTCOptionsPlatformSpecificOptions = default(WindowsRTCOptionsPlatformSpecificOptions);
			windowsRTCOptionsPlatformSpecificOptions.XAudio29DllPath = GameIO.GetGameDir("7DaysToDie_Data/Plugins/x86_64/xaudio2_9redist.dll");
			windowsRTCOptions.PlatformSpecificOptions = new WindowsRTCOptionsPlatformSpecificOptions?(windowsRTCOptionsPlatformSpecificOptions);
			windowsOptions.RTCOptions = new WindowsRTCOptions?(windowsRTCOptions);
			windowsOptions.CacheDirectory = GameIO.GetDeviceLocalUserGameDataDir();
			if (!Directory.Exists(windowsOptions.CacheDirectory))
			{
				Directory.CreateDirectory(windowsOptions.CacheDirectory);
			}
			object lockObject = AntiCheatCommon.LockObject;
			PlatformInterface platformInterface;
			lock (lockObject)
			{
				platformInterface = PlatformInterface.Create(ref windowsOptions);
			}
			return platformInterface;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void logCallback(ref LogMessage _message)
		{
			if (_message.Level == 300 && _message.Category == "LogHttp")
			{
				this.httpWarningCount++;
				if (this.httpWarningCount == 50)
				{
					this.httpNextTime = Time.unscaledTime + 600f;
					return;
				}
				if (this.httpWarningCount > 50)
				{
					if (Time.unscaledTime < this.httpNextTime)
					{
						return;
					}
					Log.Out(string.Format("[EOS] [LogHttp - Warning] Skipped {0} warnings within the last {1} seconds!", this.httpWarningCount - 50, 600f));
					this.httpWarningCount = 0;
				}
			}
			if (Api.DebugLevel == Api.EDebugLevel.Off && _message.Level == 300 && _message.Category == "LogEOSRTC" && _message.Message.StartsWith("TickTracker Ticks have been delayed.", StringComparison.Ordinal))
			{
				return;
			}
			string text = string.Format("[EOS] [{0} - {1}] {2}", _message.Category, _message.Level.ToStringCached<LogLevel>(), _message.Message);
			LogLevel level = _message.Level;
			if (level > 200)
			{
				if (level <= 400)
				{
					if (level == 300)
					{
						Log.Warning(text);
						return;
					}
					if (level != 400)
					{
						goto IL_016D;
					}
				}
				else if (level != 500 && level != 600)
				{
					goto IL_016D;
				}
				Log.Out(text);
				return;
			}
			if (level == null)
			{
				Log.Error(text);
				throw new ArgumentOutOfRangeException();
			}
			if (level == 100 || level == 200)
			{
				Log.Error(text);
				return;
			}
			IL_016D:
			throw new ArgumentOutOfRangeException();
		}

		public static readonly Api.EDebugLevel DebugLevel = Api.EDebugLevel.Off;

		[PublicizedFrom(EAccessModifier.Private)]
		public IPlatform owner;

		public PlatformInterface PlatformInterface;

		public ConnectInterface ConnectInterface;

		public SanctionsInterface SanctionsInterface;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly MicroStopwatch tickDurationStopwatch = new MicroStopwatch(false);

		[PublicizedFrom(EAccessModifier.Internal)]
		public readonly SanctionsCheck eosSanctionsCheck = new SanctionsCheck();

		[PublicizedFrom(EAccessModifier.Private)]
		public Action clientApiInitialized;

		[PublicizedFrom(EAccessModifier.Private)]
		public const int httpWarningLimit = 50;

		[PublicizedFrom(EAccessModifier.Private)]
		public const float httpWarningTimeout = 600f;

		[PublicizedFrom(EAccessModifier.Private)]
		public int httpWarningCount;

		[PublicizedFrom(EAccessModifier.Private)]
		public float httpNextTime;

		public enum EDebugLevel
		{
			Off,
			Normal,
			Verbose
		}
	}
}
