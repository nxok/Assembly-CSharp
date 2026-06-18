using System;
using System.Collections;
using System.Collections.Generic;

namespace SharpEXR
{
	public class OffsetTable : IEnumerable<uint>, IEnumerable
	{
		public List<uint> Offsets { get; set; }

		public OffsetTable()
		{
			this.Offsets = new List<uint>();
		}

		public OffsetTable(int capacity)
		{
			this.Offsets = new List<uint>(capacity);
		}

		public void Read(IEXRReader reader, int count)
		{
			for (int i = 0; i < count; i++)
			{
				this.Offsets.Add(reader.ReadUInt32());
				reader.ReadUInt32();
			}
		}

		public IEnumerator<uint> GetEnumerator()
		{
			return this.Offsets.GetEnumerator();
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}
	}
}
