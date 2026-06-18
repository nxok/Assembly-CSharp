using System;
using System.Text;

public class CallbackMetric : IMetric
{
	public string Header { get; set; }

	public void AppendLastValue(StringBuilder builder)
	{
		builder.Append(this.callback());
	}

	public void Cleanup()
	{
	}

	public CallbackMetric.GetLastValue callback;

	public delegate string GetLastValue();
}
