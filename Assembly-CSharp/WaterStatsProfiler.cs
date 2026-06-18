using System;
using System.Diagnostics;
using Unity.Profiling;

public static class WaterStatsProfiler
{
	public static void SampleTick(WaterStats stats)
	{
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ProfilerCounter<int> NumChunksProcessed = new ProfilerCounter<int>(ProfilerCategory.Scripts, "Num Chunks Processed", 3);

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ProfilerCounter<int> NumChunksActive = new ProfilerCounter<int>(ProfilerCategory.Scripts, "Num Chunks Active", 3);

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ProfilerCounter<int> NumFlowEvents = new ProfilerCounter<int>(ProfilerCategory.Scripts, "Num Flow Events", 3);

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ProfilerCounter<int> NumVoxelProcessed = new ProfilerCounter<int>(ProfilerCategory.Scripts, "Num Voxels Processed", 3);

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ProfilerCounter<int> NumVoxelsPutToSleep = new ProfilerCounter<int>(ProfilerCategory.Scripts, "Num Voxels Put To Sleep", 3);

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ProfilerCounter<int> NumVoxelsWokeUp = new ProfilerCounter<int>(ProfilerCategory.Scripts, "Num Voxels Woke Up", 3);

	public struct Timer
	{
		public Timer(string name)
		{
			this.stopwatch = new Stopwatch();
			this.counterValue = new ProfilerCounter<double>(ProfilerCategory.Scripts, name, 1);
		}

		public void Start()
		{
			this.stopwatch.Start();
		}

		public void Stop()
		{
			this.stopwatch.Stop();
		}

		public void Sample()
		{
			this.stopwatch.Reset();
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly double tickToNanos = 1000000000.0 / (double)Stopwatch.Frequency;

		[PublicizedFrom(EAccessModifier.Private)]
		public Stopwatch stopwatch;

		public ProfilerCounter<double> counterValue;
	}
}
