using System;
using System.Collections.Generic;
using System.Diagnostics;

public class NetPackageMeasure
{
	public NetPackageMeasure(double _windowSizeSeconds)
	{
		this.timeWindowTicks = (long)(_windowSizeSeconds * (double)Stopwatch.Frequency);
		this.timer.Start();
	}

	public void SamplePackages(List<NetPackage> _packages)
	{
		long num = 0L;
		foreach (NetPackage netPackage in _packages)
		{
			num += (long)netPackage.GetLength();
		}
		this.AddSample(num);
	}

	public void AddSample(long _totalBytes)
	{
		this.samples.AddLast(new NetPackageMeasure.Sample(this.timer.ElapsedTicks, _totalBytes));
		this.totalSent += _totalBytes;
	}

	public void RecalculateTotals()
	{
		this.timer.Stop();
		while (this.samples.First != null && Math.Abs(this.timer.ElapsedTicks - this.samples.First.Value.timestamp) > this.timeWindowTicks)
		{
			NetPackageMeasure.Sample value = this.samples.First.Value;
			this.totalSent -= value.totalBytesSent;
			this.samples.RemoveFirst();
		}
		this.timer.Start();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public long timeWindowTicks;

	[PublicizedFrom(EAccessModifier.Private)]
	public LinkedList<NetPackageMeasure.Sample> samples = new LinkedList<NetPackageMeasure.Sample>();

	[PublicizedFrom(EAccessModifier.Private)]
	public Stopwatch timer = new Stopwatch();

	public long totalSent;

	public struct Sample
	{
		public Sample(long _timestamp, long _totalBytesSent)
		{
			this.timestamp = _timestamp;
			this.totalBytesSent = _totalBytesSent;
		}

		public long totalBytesSent;

		public long timestamp;
	}
}
