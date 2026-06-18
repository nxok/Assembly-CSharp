using System;

namespace XMLData
{
	public class Range<TValue>
	{
		public Range()
		{
		}

		public Range(bool _hasMin, TValue _min, bool _hasMax, TValue _max)
		{
			this.hasMin = _hasMin;
			this.hasMax = _hasMax;
			this.min = _min;
			this.max = _max;
		}

		public override string ToString()
		{
			return string.Format("{0}-{1}", this.hasMin ? this.min.ToString() : "*", this.hasMax ? this.max.ToString() : "*");
		}

		public bool hasMin;

		public bool hasMax;

		public TValue min;

		public TValue max;
	}
}
