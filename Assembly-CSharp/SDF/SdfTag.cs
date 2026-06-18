using System;
using System.IO;

namespace SDF
{
	public abstract class SdfTag
	{
		public SdfTagType TagType { get; set; }

		public string Name { get; set; }

		public object Value { get; set; }

		public abstract void WritePayload(BinaryWriter bw);

		[PublicizedFrom(EAccessModifier.Protected)]
		public SdfTag()
		{
		}
	}
}
