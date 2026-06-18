using System;
using System.Text;

public class ConstantValueMetric : IMetric
{
	public string Header { get; set; }

	public void AppendLastValue(StringBuilder builder)
	{
		builder.Append(this.value);
	}

	public void Cleanup()
	{
	}

	public int value;
}
