using System;

public struct HsvColor
{
	public float normalizedH
	{
		get
		{
			return (float)this.H / 360f;
		}
		set
		{
			this.H = (double)value * 360.0;
		}
	}

	public float normalizedS
	{
		get
		{
			return (float)this.S;
		}
		set
		{
			this.S = (double)value;
		}
	}

	public float normalizedV
	{
		get
		{
			return (float)this.V;
		}
		set
		{
			this.V = (double)value;
		}
	}

	public HsvColor(double h, double s, double v)
	{
		this.H = h;
		this.S = s;
		this.V = v;
	}

	public override string ToString()
	{
		return string.Concat(new string[]
		{
			"{",
			this.H.ToCultureInvariantString("f2"),
			",",
			this.S.ToCultureInvariantString("f2"),
			",",
			this.V.ToCultureInvariantString("f2"),
			"}"
		});
	}

	public double H;

	public double S;

	public double V;
}
