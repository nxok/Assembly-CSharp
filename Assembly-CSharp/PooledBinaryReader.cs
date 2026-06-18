using System;
using System.IO;
using System.Text;
using System.Threading;
using JetBrains.Annotations;
using UnityEngine;

public class PooledBinaryReader : BinaryReader, IBinaryReaderOrWriter, IMemoryPoolableObject, IDisposable
{
	public override Stream BaseStream
	{
		get
		{
			return this.baseStream;
		}
	}

	public Encoding Encoding
	{
		get
		{
			return this.encoding;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			this.encoding = value;
			this.decoder = null;
		}
	}

	public PooledBinaryReader()
		: base(Stream.Null)
	{
		this.Encoding = new UTF8Encoding(false, false);
		Interlocked.Increment(ref PooledBinaryReader.INSTANCES_CREATED);
		Interlocked.Increment(ref PooledBinaryReader.INSTANCES_LIVE);
		if (PooledBinaryReader.INSTANCES_LIVE > PooledBinaryReader.INSTANCES_MAX)
		{
			Interlocked.Exchange(ref PooledBinaryReader.INSTANCES_MAX, PooledBinaryReader.INSTANCES_LIVE);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public ~PooledBinaryReader()
	{
		Interlocked.Decrement(ref PooledBinaryReader.INSTANCES_LIVE);
	}

	public void SetBaseStream(Stream _stream)
	{
		if (_stream != null && !_stream.CanRead)
		{
			throw new ArgumentException("The stream doesn't support reading.");
		}
		this.baseStream = _stream;
		this.decoder = null;
	}

	public override int PeekChar()
	{
		if (this.baseStream == null)
		{
			throw new IOException("Stream is invalid");
		}
		if (!this.baseStream.CanSeek)
		{
			return -1;
		}
		int num;
		bool flag = this.ReadCharBytes(this.charBuffer, 0, 1, out num) != 0;
		this.baseStream.Position -= (long)num;
		if (!flag)
		{
			return -1;
		}
		return (int)this.charBuffer[0];
	}

	public override int Read()
	{
		if (this.Read(this.charBuffer, 0, 1) == 0)
		{
			return -1;
		}
		return (int)this.charBuffer[0];
	}

	public override bool ReadBoolean()
	{
		return this.ReadByte() > 0;
	}

	public override byte ReadByte()
	{
		if (this.baseStream == null)
		{
			throw new IOException("Stream is invalid");
		}
		int num = this.baseStream.ReadByte();
		if (num == -1)
		{
			throw new EndOfStreamException();
		}
		return (byte)num;
	}

	public override sbyte ReadSByte()
	{
		return (sbyte)this.ReadByte();
	}

	public override char ReadChar()
	{
		int num = this.Read();
		if (num == -1)
		{
			throw new EndOfStreamException();
		}
		return (char)num;
	}

	public override short ReadInt16()
	{
		this.FillBuffer(2);
		return (short)((int)this.buffer[0] | ((int)this.buffer[1] << 8));
	}

	public override ushort ReadUInt16()
	{
		this.FillBuffer(2);
		return (ushort)((int)this.buffer[0] | ((int)this.buffer[1] << 8));
	}

	public override int ReadInt32()
	{
		this.FillBuffer(4);
		return (int)this.buffer[0] | ((int)this.buffer[1] << 8) | ((int)this.buffer[2] << 16) | ((int)this.buffer[3] << 24);
	}

	public override uint ReadUInt32()
	{
		this.FillBuffer(4);
		return (uint)((int)this.buffer[0] | ((int)this.buffer[1] << 8) | ((int)this.buffer[2] << 16) | ((int)this.buffer[3] << 24));
	}

	public override long ReadInt64()
	{
		this.FillBuffer(8);
		uint num = (uint)((int)this.buffer[0] | ((int)this.buffer[1] << 8) | ((int)this.buffer[2] << 16) | ((int)this.buffer[3] << 24));
		return (long)(((ulong)((int)this.buffer[4] | ((int)this.buffer[5] << 8) | ((int)this.buffer[6] << 16) | ((int)this.buffer[7] << 24)) << 32) | (ulong)num);
	}

	public override ulong ReadUInt64()
	{
		this.FillBuffer(8);
		uint num = (uint)((int)this.buffer[0] | ((int)this.buffer[1] << 8) | ((int)this.buffer[2] << 16) | ((int)this.buffer[3] << 24));
		return ((ulong)((int)this.buffer[4] | ((int)this.buffer[5] << 8) | ((int)this.buffer[6] << 16) | ((int)this.buffer[7] << 24)) << 32) | (ulong)num;
	}

	public override float ReadSingle()
	{
		this.FillBuffer(4);
		return BitConverterLE.ToSingle(this.buffer, 0);
	}

	public override double ReadDouble()
	{
		this.FillBuffer(8);
		return BitConverterLE.ToDouble(this.buffer, 0);
	}

	public unsafe override decimal ReadDecimal()
	{
		this.FillBuffer(16);
		decimal num;
		byte* ptr = (byte*)(&num);
		if (BitConverter.IsLittleEndian)
		{
			for (int i = 0; i < 16; i++)
			{
				if (i < 4)
				{
					ptr[i + 8] = this.buffer[i];
				}
				else if (i < 8)
				{
					ptr[i + 8] = this.buffer[i];
				}
				else if (i < 12)
				{
					ptr[i - 4] = this.buffer[i];
				}
				else if (i < 16)
				{
					ptr[i - 12] = this.buffer[i];
				}
			}
		}
		else
		{
			for (int j = 0; j < 16; j++)
			{
				if (j < 4)
				{
					ptr[11 - j] = this.buffer[j];
				}
				else if (j < 8)
				{
					ptr[19 - j] = this.buffer[j];
				}
				else if (j < 12)
				{
					ptr[15 - j] = this.buffer[j];
				}
				else if (j < 16)
				{
					ptr[15 - j] = this.buffer[j];
				}
			}
		}
		return num;
	}

	public override string ReadString()
	{
		int num = this.Read7BitEncodedInt();
		if (num < 0)
		{
			throw new IOException("Invalid binary file (string len < 0)");
		}
		if (num == 0)
		{
			return string.Empty;
		}
		this.stringBuilder.Length = 0;
		this.stringBuilder.EnsureCapacity(num);
		do
		{
			int num2 = ((num <= 128) ? num : 128);
			this.FillBuffer(num2);
			if (this.decoder == null)
			{
				this.decoder = this.encoding.GetDecoder();
			}
			int chars = this.decoder.GetChars(this.buffer, 0, num2, this.charBuffer, 0);
			this.stringBuilder.Append(this.charBuffer, 0, chars);
			num -= num2;
		}
		while (num > 0);
		return this.stringBuilder.ToString();
	}

	public override int Read(char[] _buffer, int _index, int _count)
	{
		if (this.baseStream == null)
		{
			throw new IOException("Stream is invalid");
		}
		if (_buffer == null)
		{
			throw new ArgumentNullException("_buffer", "_buffer is null");
		}
		if (_index < 0)
		{
			throw new ArgumentOutOfRangeException("_index", "_index is less than 0");
		}
		if (_count < 0)
		{
			throw new ArgumentOutOfRangeException("_count", "_count is less than 0");
		}
		if (_buffer.Length - _index < _count)
		{
			throw new ArgumentException("buffer is too small");
		}
		int num;
		return this.ReadCharBytes(_buffer, _index, _count, out num);
	}

	[Obsolete("char[] ReadChars (int) allocates memory. Try using int Read (char[], int, int) instead.")]
	public override char[] ReadChars(int _count)
	{
		if (_count < 0)
		{
			throw new ArgumentOutOfRangeException("_count", "_count is less than 0");
		}
		if (_count == 0)
		{
			return new char[0];
		}
		char[] array = new char[_count];
		int num = this.Read(array, 0, _count);
		if (num == 0)
		{
			throw new EndOfStreamException();
		}
		if (num != array.Length)
		{
			char[] array2 = new char[num];
			Array.Copy(array, 0, array2, 0, num);
			return array2;
		}
		return array;
	}

	public override int Read(Span<char> _buffer)
	{
		if (this.baseStream == null)
		{
			throw new IOException("Stream is invalid");
		}
		int num;
		return this.ReadCharBytes(_buffer, out num);
	}

	public override int Read(Span<byte> _buffer)
	{
		if (this.baseStream == null)
		{
			throw new IOException("Stream is invalid");
		}
		return this.baseStream.Read(_buffer);
	}

	public override int Read(byte[] _buffer, int _index, int _count)
	{
		if (this.baseStream == null)
		{
			throw new IOException("Stream is invalid");
		}
		if (_buffer == null)
		{
			throw new ArgumentNullException("_buffer", "_buffer is null");
		}
		if (_index < 0)
		{
			throw new ArgumentOutOfRangeException("_index", "_index is less than 0");
		}
		if (_count < 0)
		{
			throw new ArgumentOutOfRangeException("_count", "_count is less than 0");
		}
		if (_buffer.Length - _index < _count)
		{
			throw new ArgumentException("buffer is too small");
		}
		return this.baseStream.Read(_buffer, _index, _count);
	}

	[Obsolete("byte[] ReadBytes (int) allocates memory. Try using int Read (byte[], int, int) instead.")]
	public override byte[] ReadBytes(int _count)
	{
		if (this.baseStream == null)
		{
			throw new IOException("Stream is invalid");
		}
		if (_count < 0)
		{
			throw new ArgumentOutOfRangeException("_count", "_count is less than 0");
		}
		byte[] array = new byte[_count];
		int i;
		int num;
		for (i = 0; i < _count; i += num)
		{
			num = this.baseStream.Read(array, i, _count - i);
			if (num == 0)
			{
				break;
			}
		}
		if (i != _count)
		{
			byte[] array2 = new byte[i];
			Buffer.BlockCopy(array, 0, array2, 0, i);
			return array2;
		}
		return array;
	}

	public int Read7BitEncodedSignedInt()
	{
		int num = 0;
		int i = 0;
		byte b = this.ReadByte();
		num |= (int)(b & 63);
		i += 6;
		bool flag = (b & 64) > 0;
		if ((b & 128) != 0)
		{
			while (i < 32)
			{
				b = this.ReadByte();
				num |= (int)(b & 127) << i;
				i += 7;
				if ((b & 128) == 0)
				{
					if (!flag)
					{
						return num;
					}
					return -num;
				}
			}
			throw new FormatException("Illegal encoding for 7 bit encoded int");
		}
		if (!flag)
		{
			return num;
		}
		return -num;
	}

	public new int Read7BitEncodedInt()
	{
		int num = 0;
		int i = 0;
		while (i < 35)
		{
			byte b = this.ReadByte();
			num |= (int)(b & 127) << i;
			i += 7;
			if ((b & 128) == 0)
			{
				return num;
			}
		}
		throw new FormatException("Illegal encoding for 7 bit encoded int");
	}

	[MustUseReturnValue]
	public PooledBinaryReader.StreamReadSizeMarker ReadSizeMarker(PooledBinaryWriter.EMarkerSize _markerSize)
	{
		long position = this.baseStream.Position;
		uint num;
		switch (_markerSize)
		{
		case PooledBinaryWriter.EMarkerSize.UInt8:
			num = (uint)this.ReadByte();
			goto IL_004C;
		case PooledBinaryWriter.EMarkerSize.UInt16:
			num = (uint)this.ReadUInt16();
			goto IL_004C;
		case PooledBinaryWriter.EMarkerSize.UInt32:
			num = this.ReadUInt32();
			goto IL_004C;
		}
		throw new ArgumentOutOfRangeException("_markerSize");
		IL_004C:
		uint num2 = num;
		return new PooledBinaryReader.StreamReadSizeMarker(position, num2);
	}

	public bool ValidateSizeMarker(ref PooledBinaryReader.StreamReadSizeMarker _sizeMarker, out uint _bytesReceived, bool _fixPosition = true)
	{
		long num = this.baseStream.Position - _sizeMarker.Position;
		_bytesReceived = (uint)num;
		if (num == (long)((ulong)_sizeMarker.ExpectedSize))
		{
			return true;
		}
		if (_fixPosition)
		{
			this.baseStream.Position = _sizeMarker.Position + (long)((ulong)_sizeMarker.ExpectedSize);
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int ReadCharBytes(char[] _targetBuffer, int _targetIndex, int _count, out int _bytesRead)
	{
		return this.ReadCharBytes(_targetBuffer.AsSpan(_targetIndex, _count), out _bytesRead);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int ReadCharBytes(Span<char> _targetBuffer, out int _bytesRead)
	{
		int i = 0;
		_bytesRead = 0;
		while (i < _targetBuffer.Length)
		{
			int num = 0;
			int chars;
			do
			{
				int num2 = this.baseStream.ReadByte();
				if (num2 == -1)
				{
					return i;
				}
				this.buffer[num++] = (byte)num2;
				_bytesRead++;
				chars = this.encoding.GetChars(this.buffer.AsSpan(0, num), _targetBuffer.Slice(i));
			}
			while (chars <= 0);
			i++;
		}
		return i;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void FillBuffer(int _numBytes)
	{
		if (this.baseStream == null)
		{
			throw new IOException("Stream is invalid");
		}
		int num;
		for (int i = 0; i < _numBytes; i += num)
		{
			num = this.baseStream.Read(this.buffer, i, _numBytes - i);
			if (num == 0)
			{
				throw new EndOfStreamException();
			}
		}
	}

	public void Flush()
	{
		if (this.baseStream != null)
		{
			this.baseStream.Flush();
		}
	}

	public override void Close()
	{
	}

	public void Reset()
	{
		this.stringBuilder.Length = 0;
		this.baseStream = null;
	}

	public void Cleanup()
	{
		this.Reset();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void Dispose(bool _disposing)
	{
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void System.IDisposable.Dispose()
	{
		MemoryPools.poolBinaryReader.FreeSync(this);
	}

	public bool ReadWrite(bool _value)
	{
		return this.ReadBoolean();
	}

	public byte ReadWrite(byte _value)
	{
		return this.ReadByte();
	}

	public sbyte ReadWrite(sbyte _value)
	{
		return this.ReadSByte();
	}

	public char ReadWrite(char _value)
	{
		return this.ReadChar();
	}

	public short ReadWrite(short _value)
	{
		return this.ReadInt16();
	}

	public ushort ReadWrite(ushort _value)
	{
		return this.ReadUInt16();
	}

	public int ReadWrite(int _value)
	{
		return this.ReadInt32();
	}

	public uint ReadWrite(uint _value)
	{
		return this.ReadUInt32();
	}

	public long ReadWrite(long _value)
	{
		return this.ReadInt64();
	}

	public ulong ReadWrite(ulong _value)
	{
		return this.ReadUInt64();
	}

	public float ReadWrite(float _value)
	{
		return this.ReadSingle();
	}

	public double ReadWrite(double _value)
	{
		return this.ReadDouble();
	}

	public decimal ReadWrite(decimal _value)
	{
		return this.ReadDecimal();
	}

	public string ReadWrite(string _value)
	{
		return this.ReadString();
	}

	public void ReadWrite(byte[] _buffer, int _index, int _count)
	{
		this.Read(_buffer, _index, _count);
	}

	public Vector3 ReadWrite(Vector3 _value)
	{
		Vector3 vector = _value;
		vector.x = this.ReadSingle();
		vector.y = this.ReadSingle();
		vector.z = this.ReadSingle();
		return vector;
	}

	public static int INSTANCES_LIVE;

	public static int INSTANCES_MAX;

	public static int INSTANCES_CREATED;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int BYTE_BUFFER_SIZE = 128;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int CHAR_BUFFER_SIZE = 128;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly byte[] buffer = new byte[128];

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly char[] charBuffer = new char[128];

	[PublicizedFrom(EAccessModifier.Private)]
	public Decoder decoder;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly StringBuilder stringBuilder = new StringBuilder(128);

	[PublicizedFrom(EAccessModifier.Private)]
	public Stream baseStream;

	[PublicizedFrom(EAccessModifier.Private)]
	public Encoding encoding;

	public readonly struct StreamReadSizeMarker
	{
		public StreamReadSizeMarker(long _position, uint _expectedSize)
		{
			this.Position = _position;
			this.ExpectedSize = _expectedSize;
		}

		public readonly long Position;

		public readonly uint ExpectedSize;
	}
}
