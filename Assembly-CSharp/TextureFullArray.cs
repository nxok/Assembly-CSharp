using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

public struct TextureFullArray : IEquatable<TextureFullArray>
{
	public unsafe long this[int index]
	{
		get
		{
			if (index < 0 || index >= 1)
			{
				throw new IndexOutOfRangeException(string.Format("Index {0} is outside of the valid range of min: 0, max: {1}.", index, 1));
			}
			return *((ref this.values.FixedElementField) + (IntPtr)index * 8);
		}
		set
		{
			if (index < 0 || index >= 1)
			{
				throw new IndexOutOfRangeException(string.Format("Index {0} is outside of the valid range of min: 0, max: {1}.", index, 1));
			}
			*((ref this.values.FixedElementField) + (IntPtr)index * 8) = value;
		}
	}

	public TextureFullArray(long _fillValue)
	{
		this.Fill(_fillValue);
	}

	public static TextureFullArray Default
	{
		get
		{
			return TextureFullArray._default;
		}
	}

	public unsafe void Fill(long _fillValue)
	{
		for (int i = 0; i < 1; i++)
		{
			*((ref this.values.FixedElementField) + (IntPtr)i * 8) = _fillValue;
		}
	}

	public bool IsDefault
	{
		get
		{
			return this.Equals(TextureFullArray._default);
		}
	}

	public unsafe void Read(BinaryReader _br, int count = 1)
	{
		int i;
		for (i = 0; i < count; i++)
		{
			long num = _br.ReadInt64();
			if (i < 1)
			{
				*((ref this.values.FixedElementField) + (IntPtr)i * 8) = num;
			}
		}
		while (i < 1)
		{
			*((ref this.values.FixedElementField) + (IntPtr)i * 8) = this.values.FixedElementField;
			i++;
		}
	}

	public unsafe void Write(BinaryWriter _bw)
	{
		for (int i = 0; i < 1; i++)
		{
			_bw.Write(*((ref this.values.FixedElementField) + (IntPtr)i * 8));
		}
	}

	public unsafe bool Equals(TextureFullArray other)
	{
		for (int i = 0; i < 1; i++)
		{
			if (*((ref this.values.FixedElementField) + (IntPtr)i * 8) != *((ref other.values.FixedElementField) + (IntPtr)i * 8))
			{
				return false;
			}
		}
		return true;
	}

	public override bool Equals(object obj)
	{
		if (obj is TextureFullArray)
		{
			TextureFullArray textureFullArray = (TextureFullArray)obj;
			return this.Equals(textureFullArray);
		}
		return false;
	}

	public override int GetHashCode()
	{
		int num = 17;
		for (int i = 0; i < 1; i++)
		{
			num = num * 31 + ((ref this.values.FixedElementField) + (IntPtr)i * 8).GetHashCode();
		}
		return num;
	}

	public static bool operator ==(TextureFullArray left, TextureFullArray right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(TextureFullArray left, TextureFullArray right)
	{
		return !left.Equals(right);
	}

	[FixedBuffer(typeof(long), 1)]
	[PublicizedFrom(EAccessModifier.Private)]
	public TextureFullArray.<values>e__FixedBuffer values;

	public static readonly TextureFullArray _default = new TextureFullArray(0L);

	[CompilerGenerated]
	[UnsafeValueType]
	[StructLayout(LayoutKind.Sequential, Size = 8)]
	public struct <values>e__FixedBuffer
	{
		public long FixedElementField;
	}
}
