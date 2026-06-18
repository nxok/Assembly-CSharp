using System;
using System.Diagnostics;
using System.Threading;
using Unity.XGamingRuntime.Interop;

namespace Platform.XBL.Save
{
	public sealed class SizeTracker : IDisposable
	{
		[Conditional("DEBUG_SAVE_DATA_MANAGER")]
		[PublicizedFrom(EAccessModifier.Private)]
		public static void LogTrace(string text)
		{
			Log.Out("[XBL: SizeTracker] " + text);
		}

		public SizeTracker(long max, long used, SizeTrackerGetRemainingQuotaAsync getRemainingQuotaAsync, bool shouldUpdateSizesOnEstimate)
		{
			this.m_maxBytes = max;
			this.m_getRemainingQuotaAsync = getRemainingQuotaAsync;
			this.m_shouldUpdateSizesOnEstimate = shouldUpdateSizesOnEstimate;
			this.m_sizes = new SaveDataSizes(max, max - used);
		}

		public void Dispose()
		{
			object @lock = this.m_lock;
			lock (@lock)
			{
				this.m_disposed = true;
			}
		}

		public SaveDataSizes Sizes
		{
			get
			{
				object @lock = this.m_lock;
				SaveDataSizes sizes;
				lock (@lock)
				{
					sizes = this.m_sizes;
				}
				return sizes;
			}
		}

		public void UpdateUsedEstimate(long deltaUsed)
		{
			object @lock = this.m_lock;
			lock (@lock)
			{
				if (this.m_disposed)
				{
					return;
				}
				this.m_sizes = new SaveDataSizes(this.m_sizes.Total, Math.Max(this.m_sizes.Remaining - deltaUsed, 0L));
			}
			if (this.m_shouldUpdateSizesOnEstimate)
			{
				this.RefreshAsync();
			}
		}

		public void RefreshSync()
		{
			if (this.m_disposed)
			{
				return;
			}
			long num = Interlocked.Increment(ref this.m_refreshCountExpected);
			this.m_getRemainingQuotaAsync(new SizeTrackerGetRemainingQuotaCompleted(this.XGameSaveGetRemainingQuotaAsyncCompleted));
			while (Interlocked.Read(ref this.m_refreshCount) < num)
			{
				Thread.Sleep(16);
			}
		}

		public void RefreshAsync()
		{
			object @lock = this.m_lock;
			lock (@lock)
			{
				if (this.m_disposed)
				{
					return;
				}
			}
			long num = Interlocked.Read(ref this.m_refreshCountExpected);
			if (Interlocked.Read(ref this.m_refreshCount) >= num)
			{
				return;
			}
			if (Interlocked.CompareExchange(ref this.m_refreshCountExpected, num + 1L, num) == num)
			{
				this.m_getRemainingQuotaAsync(new SizeTrackerGetRemainingQuotaCompleted(this.XGameSaveGetRemainingQuotaAsyncCompleted));
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void XGameSaveGetRemainingQuotaAsyncCompleted(int hrXGameSaveGetRemainingQuota, long remaining)
		{
			if (HR.FAILED(hrXGameSaveGetRemainingQuota))
			{
				GameCoreSaveHelpers.NonTraceLogHR(hrXGameSaveGetRemainingQuota, "SizeTracker#XGameSaveGetRemainingQuotaAsync");
				Interlocked.Increment(ref this.m_refreshCount);
				return;
			}
			object obj = this.m_lock;
			lock (obj)
			{
				if (this.m_disposed)
				{
					Interlocked.Increment(ref this.m_refreshCount);
					return;
				}
			}
			long total = this.m_sizes.Total;
			long total2 = this.m_sizes.Total;
			obj = this.m_lock;
			lock (obj)
			{
				if (this.m_disposed)
				{
					Interlocked.Increment(ref this.m_refreshCount);
					return;
				}
				this.m_sizes = new SaveDataSizes(total2, remaining);
			}
			Interlocked.Increment(ref this.m_refreshCount);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public const int DefaultMaxSaveDataBytes = 1073741824;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly long m_maxBytes;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly SizeTrackerGetRemainingQuotaAsync m_getRemainingQuotaAsync;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly bool m_shouldUpdateSizesOnEstimate;

		[PublicizedFrom(EAccessModifier.Private)]
		public bool m_disposed;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly object m_lock = new object();

		[PublicizedFrom(EAccessModifier.Private)]
		public long m_refreshCount;

		[PublicizedFrom(EAccessModifier.Private)]
		public long m_refreshCountExpected;

		[PublicizedFrom(EAccessModifier.Private)]
		public SaveDataSizes m_sizes;
	}
}
