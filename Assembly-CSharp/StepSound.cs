using System;

public class StepSound
{
	public StepSound(string _name)
	{
		this.name = _name;
	}

	public static StepSound FromString(string _name)
	{
		return new StepSound(_name);
	}

	public static StepSound stone = new StepSound("stone");

	public string name;
}
