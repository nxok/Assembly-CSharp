using System;
using System.Collections.Generic;
using System.Text;
using Unity.Profiling;

public class ProfilingMetricCapture
{
	public void AddDummy(string header)
	{
		this.metrics.Add(new ConstantValueMetric
		{
			Header = header,
			value = 0
		});
	}

	public void Add(string header, ProfilerRecorder recorder)
	{
		if (!recorder.IsRunning)
		{
			recorder.Start();
		}
		this.metrics.Add(new ProfilerRecorderMetric
		{
			recorder = recorder,
			Header = header
		});
	}

	public void Add(string header, CallbackMetric.GetLastValue callback)
	{
		this.metrics.Add(new CallbackMetric
		{
			callback = callback,
			Header = header
		});
	}

	public void Add(IMetric metric)
	{
		this.metrics.Add(metric);
	}

	public void Cleanup()
	{
		foreach (IMetric metric in this.metrics)
		{
			metric.Cleanup();
		}
		this.metrics.Clear();
	}

	public string GetCsvHeader()
	{
		for (int i = 0; i < this.metrics.Count; i++)
		{
			this.outputBuilder.Append(this.metrics[i].Header);
			if (i < this.metrics.Count - 1)
			{
				this.outputBuilder.Append(",");
			}
		}
		string text = this.outputBuilder.ToString();
		this.outputBuilder.Clear();
		return text;
	}

	public string GetLastValueCsv()
	{
		for (int i = 0; i < this.metrics.Count; i++)
		{
			this.metrics[i].AppendLastValue(this.outputBuilder);
			if (i < this.metrics.Count - 1)
			{
				this.outputBuilder.Append(",");
			}
		}
		string text = this.outputBuilder.ToString();
		this.outputBuilder.Clear();
		return text;
	}

	public string PrettyPrint()
	{
		for (int i = 0; i < this.metrics.Count; i++)
		{
			this.outputBuilder.AppendFormat("{0}: ", this.metrics[i].Header);
			this.metrics[i].AppendLastValue(this.outputBuilder);
			this.outputBuilder.AppendLine();
		}
		string text = this.outputBuilder.ToString();
		this.outputBuilder.Clear();
		return text;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public List<IMetric> metrics = new List<IMetric>();

	[PublicizedFrom(EAccessModifier.Private)]
	public StringBuilder outputBuilder = new StringBuilder();
}
