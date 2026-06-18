using System;
using System.Text;
using Unity.Profiling;

public class ProfilerRecorderMetric : IMetric
{
	public string Header { get; set; }

	public void AppendLastValue(StringBuilder builder)
	{
		ProfilerUtils.AppendLastValue(this.recorder, builder);
	}

	public void Cleanup()
	{
		this.recorder.Dispose();
	}

	public ProfilerRecorder recorder;
}
