using System;

[AttributeUsage(AttributeTargets.Field)]
public class Documentation : Attribute
{
	public string Text { get; }

	public Documentation(string value)
	{
		this.Text = value;
	}
}
