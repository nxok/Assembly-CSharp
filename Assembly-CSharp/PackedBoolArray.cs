using System;
using System.IO;

public class PackedBoolArray
{
	public int Length
	{
		get
		{
			return this.length;
		}
		set
		{
			if (value == this.length)
			{
				return;
			}
			if (value == 0)
			{
				this.data = null;
				this.length = value;
				return;
			}
			if (this.data == null)
			{
				this.data = new byte[PackedBoolArray.calcArraySize(value)];
				this.length = value;
				return;
			}
			int num = PackedBoolArray.calcArraySize(this.length);
			int num2 = PackedBoolArray.calcArraySize(value);
			if (num2 > num)
			{
				byte[] array = new byte[num2];
				Array.Copy(this.data, array, num);
				this.data = array;
				this.length = value;
				return;
			}
			if (num2 < num)
			{
				byte[] array2 = new byte[num2];
				Array.Copy(this.data, array2, num2);
				this.data = array2;
			}
			this.length = num2 * 8;
			for (int i = value; i < num2 * 8; i++)
			{
				this[i] = false;
			}
			this.length = value;
		}
	}

	public int ByteSize
	{
		get
		{
			return PackedBoolArray.calcArraySize(this.Length);
		}
	}

	public bool this[int _i]
	{
		get
		{
			this.validateIndex(_i);
			return ((int)this.data[_i / 8] & (1 << _i % 8)) != 0;
		}
		set
		{
			this.validateIndex(_i);
			if (value)
			{
				byte[] array = this.data;
				int num = _i / 8;
				array[num] |= (byte)(1 << _i % 8);
				return;
			}
			byte[] array2 = this.data;
			int num2 = _i / 8;
			array2[num2] &= (byte)(~(byte)(1 << _i % 8));
		}
	}

	public PackedBoolArray(int _length = 0)
	{
		this.Length = _length;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void validateIndex(int _i)
	{
		if (_i < 0)
		{
			throw new IndexOutOfRangeException(string.Format("Index ({0}) needs to be non-negative", _i));
		}
		if (_i >= this.length)
		{
			throw new IndexOutOfRangeException(string.Format("Index ({0}) needs to be lower than length ({1})", _i, this.Length));
		}
	}

	public PackedBoolArray Clone()
	{
		PackedBoolArray packedBoolArray = new PackedBoolArray(this.Length);
		for (int i = 0; i < PackedBoolArray.calcArraySize(this.Length); i++)
		{
			packedBoolArray.data[i] = this.data[i];
		}
		return packedBoolArray;
	}

	public void Write(Stream _targetStream)
	{
		StreamUtils.Write7BitEncodedInt(_targetStream, this.Length);
		if (this.Length > 0)
		{
			_targetStream.Write(this.data, 0, this.ByteSize);
		}
	}

	public void Write(BinaryWriter _targetWriter)
	{
		StreamUtils.Write7BitEncodedInt(_targetWriter.BaseStream, this.Length);
		if (this.Length > 0)
		{
			_targetWriter.Write(this.data);
		}
	}

	public void Write(PooledBinaryWriter _targetWriter)
	{
		_targetWriter.Write7BitEncodedInt(this.Length);
		if (this.Length > 0)
		{
			_targetWriter.Write(this.data);
		}
	}

	public void Read(Stream _sourceStream)
	{
		this.Length = StreamUtils.Read7BitEncodedInt(_sourceStream);
		if (this.Length > 0)
		{
			_sourceStream.Read(this.data, 0, this.ByteSize);
		}
	}

	public void Read(BinaryReader _sourceReader)
	{
		this.Length = StreamUtils.Read7BitEncodedInt(_sourceReader.BaseStream);
		if (this.Length > 0)
		{
			_sourceReader.Read(this.data, 0, this.ByteSize);
		}
	}

	public void Read(PooledBinaryReader _sourceReader)
	{
		this.Length = _sourceReader.Read7BitEncodedInt();
		if (this.Length > 0)
		{
			_sourceReader.Read(this.data, 0, this.ByteSize);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static int calcArraySize(int _length)
	{
		return (_length + 7) / 8;
	}

	public void Clear()
	{
		for (int i = 0; i < this.ByteSize; i++)
		{
			this.data[i] = 0;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int length;

	[PublicizedFrom(EAccessModifier.Private)]
	public byte[] data;
}
