using System;

namespace SharpEXR
{
	public struct EXRVersion
	{
		public EXRVersion(int version, bool multiPart, bool longNames, bool nonImageParts, bool isSingleTiled = false)
		{
			this.Value = (EXRVersionFlags)(version & 255);
			if (version == 1)
			{
				if (multiPart || nonImageParts)
				{
					throw new EXRFormatException("Invalid or corrupt EXR version: Version 1 EXR files cannot be multi part or have non image parts.");
				}
				if (isSingleTiled)
				{
					this.Value |= EXRVersionFlags.IsSinglePartTiled;
				}
				if (longNames)
				{
					this.Value |= EXRVersionFlags.LongNames;
				}
			}
			else
			{
				if (isSingleTiled)
				{
					this.Value |= EXRVersionFlags.IsSinglePartTiled;
				}
				if (longNames)
				{
					this.Value |= EXRVersionFlags.LongNames;
				}
				if (nonImageParts)
				{
					this.Value |= EXRVersionFlags.NonImageParts;
				}
				if (multiPart)
				{
					this.Value |= EXRVersionFlags.MultiPart;
				}
			}
			this.Verify();
		}

		public EXRVersion(int value)
		{
			this.Value = (EXRVersionFlags)value;
			this.Verify();
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void Verify()
		{
			if (this.IsSinglePartTiled && (this.IsMultiPart || this.HasNonImageParts))
			{
				throw new EXRFormatException("Invalid or corrupt EXR version: Version's single part bit was set, but multi part and/or non image data bits were also set.");
			}
		}

		public int Version
		{
			get
			{
				return (int)(this.Value & (EXRVersionFlags)255);
			}
		}

		public bool IsSinglePartTiled
		{
			get
			{
				return this.Value.HasFlag(EXRVersionFlags.IsSinglePartTiled);
			}
		}

		public bool HasLongNames
		{
			get
			{
				return this.Value.HasFlag(EXRVersionFlags.LongNames);
			}
		}

		public bool HasNonImageParts
		{
			get
			{
				return this.Value.HasFlag(EXRVersionFlags.NonImageParts);
			}
		}

		public bool IsMultiPart
		{
			get
			{
				return this.Value.HasFlag(EXRVersionFlags.MultiPart);
			}
		}

		public int MaxNameLength
		{
			get
			{
				if (!this.HasLongNames)
				{
					return 31;
				}
				return 255;
			}
		}

		public readonly EXRVersionFlags Value;
	}
}
