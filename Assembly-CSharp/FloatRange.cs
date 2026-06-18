using System;

public readonly struct FloatRange
{
	public FloatRange(float _min, float _max)
	{
		this.min = _min;
		this.max = _max;
	}

	public bool IsSet()
	{
		return this.min != 0f || this.max != 0f;
	}

	public float Random(GameRandom _rnd)
	{
		return _rnd.RandomRange(this.min, this.max);
	}

	public override string ToString()
	{
		return string.Concat(new string[]
		{
			"(",
			this.min.ToCultureInvariantString(),
			"-",
			this.max.ToCultureInvariantString(),
			")"
		});
	}

	public readonly float min;

	public readonly float max;
}
