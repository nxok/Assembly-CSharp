using System;

public readonly struct IntRange
{
	public IntRange(int _min, int _max)
	{
		this.min = _min;
		this.max = _max;
	}

	public bool IsSet()
	{
		return this.min != 0 || this.max != 0;
	}

	public int Random(GameRandom _rnd)
	{
		return _rnd.RandomRange(this.min, this.max);
	}

	public int RandomInclusive(GameRandom _rnd)
	{
		return _rnd.RandomRange(this.min, this.max + 1);
	}

	public override string ToString()
	{
		return string.Concat(new string[]
		{
			"(",
			this.min.ToString(),
			"-",
			this.max.ToString(),
			")"
		});
	}

	public readonly int min;

	public readonly int max;
}
