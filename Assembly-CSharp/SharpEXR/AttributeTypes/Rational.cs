using System;

namespace SharpEXR.AttributeTypes
{
	public struct Rational
	{
		public Rational(int numerator, uint denominator)
		{
			this.Numerator = numerator;
			this.Denominator = denominator;
		}

		public override string ToString()
		{
			return string.Format("{0}/{1}", this.Numerator, this.Denominator);
		}

		public double Value
		{
			get
			{
				return (double)this.Numerator / this.Denominator;
			}
		}

		public readonly int Numerator;

		public readonly uint Denominator;
	}
}
