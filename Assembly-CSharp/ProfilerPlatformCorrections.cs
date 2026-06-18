using System;
using System.Text;
using Unity.Profiling;

public static class ProfilerPlatformCorrections
{
	public static IMetric Graphics(string header, string usedOrReserved)
	{
		return new ProfilerRecorderMetric
		{
			Header = header,
			recorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "Gfx " + usedOrReserved + " Memory", 1, 24)
		};
	}

	public static IMetric Native(string header, string usedOrReserved)
	{
		return new ProfilerPlatformCorrections.NativeDefault(header, usedOrReserved);
	}

	public static IMetric TotalTracked(string header, string usedOrReserved)
	{
		return new ProfilerRecorderMetric
		{
			Header = header,
			recorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "Total " + usedOrReserved + " Memory", 1, 24)
		};
	}

	public class NativeDefault : IMetric
	{
		public string Header { get; set; }

		public NativeDefault(string header, string usedOrReserved)
		{
			this.Header = header;
			this.graphics = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "Gfx " + usedOrReserved + " Memory", 1, 24);
			this.total = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "Total " + usedOrReserved + " Memory", 1, 24);
			this.managed = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC " + usedOrReserved + " Memory", 1, 24);
		}

		public void AppendLastValue(StringBuilder builder)
		{
			double num = this.total.LastValueAsDouble - this.graphics.LastValueAsDouble - this.managed.LastValueAsDouble;
			builder.AppendFormat("{0:F2}", num * 9.5367431640625E-07);
		}

		public void Cleanup()
		{
			this.graphics.Dispose();
			this.total.Dispose();
			this.managed.Dispose();
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public ProfilerRecorder graphics;

		[PublicizedFrom(EAccessModifier.Private)]
		public ProfilerRecorder total;

		[PublicizedFrom(EAccessModifier.Private)]
		public ProfilerRecorder managed;
	}

	public class TotalTrackedPS5 : IMetric
	{
		public string Header { get; set; }

		public TotalTrackedPS5(string header, string usedOrReserved)
		{
			this.Header = header;
			this.graphics = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "Gfx " + usedOrReserved + " Memory", 1, 24);
			this.total = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "Total " + usedOrReserved + " Memory", 1, 24);
		}

		public void AppendLastValue(StringBuilder builder)
		{
			double num = this.total.LastValueAsDouble - this.graphics.LastValueAsDouble;
			builder.AppendFormat("{0:F2}", num * 9.5367431640625E-07);
		}

		public void Cleanup()
		{
			this.graphics.Dispose();
			this.total.Dispose();
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public ProfilerRecorder graphics;

		[PublicizedFrom(EAccessModifier.Private)]
		public ProfilerRecorder total;
	}
}
