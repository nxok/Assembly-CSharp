using System;

namespace SharpEXR
{
	[Flags]
	public enum EXRVersionFlags
	{
		IsSinglePartTiled = 512,
		LongNames = 1024,
		NonImageParts = 2048,
		MultiPart = 4096
	}
}
