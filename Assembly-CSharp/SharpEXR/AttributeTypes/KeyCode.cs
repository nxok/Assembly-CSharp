using System;

namespace SharpEXR.AttributeTypes
{
	public struct KeyCode
	{
		public KeyCode(int filmMfcCode, int filmType, int prefix, int count, int perfOffset, int perfsPerFrame, int perfsPerCount)
		{
			this.FilmMfcCode = filmMfcCode;
			this.FilmType = filmType;
			this.Prefix = prefix;
			this.Count = count;
			this.PerfOffset = perfOffset;
			this.PerfsPerFrame = perfsPerFrame;
			this.PerfsPerCount = perfsPerCount;
		}

		public readonly int FilmMfcCode;

		public readonly int FilmType;

		public readonly int Prefix;

		public readonly int Count;

		public readonly int PerfOffset;

		public readonly int PerfsPerFrame;

		public readonly int PerfsPerCount;
	}
}
