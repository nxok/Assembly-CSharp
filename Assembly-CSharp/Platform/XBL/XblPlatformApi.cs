using System;
using System.Threading;
using Unity.Profiling;
using Unity.XGamingRuntime;
using Unity.XGamingRuntime.Interop;
using UnityEngine;
using UnityEngine.Profiling;

namespace Platform.XBL
{
	public abstract class XblPlatformApi : IPlatformApi
	{
		public abstract string SCID { get; }

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
					this.m_clientApiInitialized = (Action)Delegate.Combine(this.m_clientApiInitialized, value);
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
					this.m_clientApiInitialized = (Action)Delegate.Remove(this.m_clientApiInitialized, value);
				}
			}
		}

		public abstract void Init(IPlatform owner);

		public bool InitClientApis()
		{
			if (this.ClientApiStatus == EApiStatus.Ok)
			{
				return true;
			}
			if (Application.isEditor)
			{
				this.ClientApiStatus = EApiStatus.Ok;
				Action clientApiInitialized = this.m_clientApiInitialized;
				if (clientApiInitialized != null)
				{
					clientApiInitialized();
				}
				return true;
			}
			int num = SDK.XGameRuntimeInitialize();
			XblHelpers.LogHR(num, "Initialize Gaming Runtime.", false);
			if (HR.FAILED(num))
			{
				this.ClientApiStatus = EApiStatus.PermanentError;
				return false;
			}
			if (HR.FAILED(SDK.CreateDefaultTaskQueue()))
			{
				this.ClientApiStatus = EApiStatus.PermanentError;
				return false;
			}
			this.m_dispatchGXDKTaskQueueRunning = true;
			this.m_dispatchGXDKTaskQueueThread = new Thread(delegate
			{
				Log.Out("[XBL] Started Thread: " + Thread.CurrentThread.Name);
				try
				{
					while (this.m_dispatchGXDKTaskQueueRunning)
					{
						try
						{
							SDK.XTaskQueueDispatch(32U);
						}
						catch (ThreadInterruptedException)
						{
							break;
						}
						catch (Exception ex)
						{
							Log.Exception(ex);
						}
						finally
						{
						}
					}
				}
				finally
				{
					Profiler.EndThreadProfiling();
					Log.Out("[XBL] Stopped Thread: " + Thread.CurrentThread.Name);
				}
			})
			{
				Name = "GXDK Task Queue Dispatch Completion",
				IsBackground = true
			};
			this.m_dispatchGXDKTaskQueueThread.Start();
			int num2 = SDK.XBL.XblInitialize(this.SCID);
			XblHelpers.LogHR(num2, "Initialize Xbox Live.", false);
			if (HR.FAILED(num2))
			{
				return false;
			}
			Log.Out("[XBL] API loaded.");
			this.ClientApiStatus = EApiStatus.Ok;
			Action clientApiInitialized2 = this.m_clientApiInitialized;
			if (clientApiInitialized2 != null)
			{
				clientApiInitialized2();
			}
			return true;
		}

		public abstract bool InitServerApis();

		public abstract void ServerApiLoaded();

		public void Update()
		{
		}

		public void Destroy()
		{
			if (Application.isEditor)
			{
				return;
			}
			this.m_dispatchGXDKTaskQueueRunning = false;
			SDK.CloseDefaultXTaskQueue();
			SDK.XBL.XblCleanup(delegate(int hr)
			{
				XblHelpers.LogHR(hr, "Uninitialize Xbox Live.", false);
			});
			Thread dispatchGXDKTaskQueueThread = this.m_dispatchGXDKTaskQueueThread;
			if (dispatchGXDKTaskQueueThread != null)
			{
				dispatchGXDKTaskQueueThread.Interrupt();
			}
			Thread dispatchGXDKTaskQueueThread2 = this.m_dispatchGXDKTaskQueueThread;
			if (dispatchGXDKTaskQueueThread2 != null)
			{
				dispatchGXDKTaskQueueThread2.Join();
			}
			this.m_dispatchGXDKTaskQueueThread = null;
			SDK.XGameRuntimeUninitialize();
			XblHelpers.LogHR(0, "Uninitialize Gaming Runtime.", false);
			Log.Out("[XBL] API Destroyed.");
		}

		public float GetScreenBoundsValueFromSystem()
		{
			return 1f;
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public XblPlatformApi()
		{
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly ProfilerMarker s_TaskQueueDispatchMarker = new ProfilerMarker("XTaskQueueDispatch");

		[PublicizedFrom(EAccessModifier.Private)]
		public Thread m_dispatchGXDKTaskQueueThread;

		[PublicizedFrom(EAccessModifier.Private)]
		public bool m_dispatchGXDKTaskQueueRunning;

		[PublicizedFrom(EAccessModifier.Private)]
		public Action m_clientApiInitialized;
	}
}
