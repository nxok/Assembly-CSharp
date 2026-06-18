using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using JetBrains.Annotations;
using UnityEngine;

public class PooledBinaryWriter : BinaryWriter, IBinaryReaderOrWriter, IMemoryPoolableObject, IDisposable
{
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
			this.encoder = null;
			this.maxBytesPerChar = this.encoding.GetMaxByteCount(1);
		}
	}

	public PooledBinaryWriter()
	{
		this.Encoding = new UTF8Encoding(false, false);
		Interlocked.Increment(ref PooledBinaryWriter.INSTANCES_CREATED);
		Interlocked.Increment(ref PooledBinaryWriter.INSTANCES_LIVE);
		if (PooledBinaryWriter.INSTANCES_LIVE > PooledBinaryWriter.INSTANCES_MAX)
		{
			Interlocked.Exchange(ref PooledBinaryWriter.INSTANCES_MAX, PooledBinaryWriter.INSTANCES_LIVE);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public ~PooledBinaryWriter()
	{
		Interlocked.Decrement(ref PooledBinaryWriter.INSTANCES_LIVE);
	}

	public void SetBaseStream(Stream _stream)
	{
		if (_stream != null && !_stream.CanWrite)
		{
			throw new ArgumentException("Stream does not support writing or already closed.");
		}
		this.OutStream = _stream;
		this.encoder = null;
	}

	public override Stream BaseStream
	{
		get
		{
			return this.OutStream;
		}
	}

	public override void Flush()
	{
		this.OutStream.Flush();
	}

	public override long Seek(int _offset, SeekOrigin _origin)
	{
		return this.OutStream.Seek((long)_offset, _origin);
	}

	public override void Write(bool _value)
	{
		this.buffer[0] = ((!_value) ? 0 : 1);
		this.OutStream.Write(this.buffer, 0, 1);
	}

	public override void Write(byte _value)
	{
		this.OutStream.WriteByte(_value);
	}

	public override void Write(byte[] _buffer)
	{
		if (_buffer == null)
		{
			throw new ArgumentNullException("_buffer");
		}
		this.OutStream.Write(_buffer, 0, _buffer.Length);
	}

	public override void Write(byte[] _buffer, int _index, int _count)
	{
		if (_buffer == null)
		{
			throw new ArgumentNullException("_buffer");
		}
		this.OutStream.Write(_buffer, _index, _count);
	}

	public override void Write(ReadOnlySpan<byte> _buffer)
	{
		this.OutStream.Write(_buffer);
	}

	public override void Write(char _ch)
	{
		this.charBuffer[0] = _ch;
		int bytes = this.encoding.GetBytes(this.charBuffer, 0, 1, this.buffer, 0);
		this.OutStream.Write(this.buffer, 0, bytes);
	}

	public override void Write(char[] _chars)
	{
		this.Write(_chars, 0, _chars.Length);
	}

	public override void Write(char[] _chars, int _index, int _count)
	{
		if (_chars == null)
		{
			throw new ArgumentNullException("_chars");
		}
		this.Write(_chars.AsSpan(_index, _count));
	}

	public override void Write(ReadOnlySpan<char> _chars)
	{
		int num;
		for (int i = 0; i < _chars.Length; i += num)
		{
			num = Math.Min(128 / this.maxBytesPerChar, _chars.Length - i);
			int bytes = this.encoding.GetBytes(_chars.Slice(i, num), this.buffer);
			this.OutStream.Write(this.buffer, 0, bytes);
		}
	}

	public unsafe override void Write(decimal _value)
	{
		byte* ptr = (byte*)(&_value);
		if (BitConverter.IsLittleEndian)
		{
			for (int i = 0; i < 16; i++)
			{
				if (i < 4)
				{
					this.buffer[i + 12] = ptr[i];
				}
				else if (i < 8)
				{
					this.buffer[i + 4] = ptr[i];
				}
				else if (i < 12)
				{
					this.buffer[i - 8] = ptr[i];
				}
				else
				{
					this.buffer[i - 8] = ptr[i];
				}
			}
		}
		else
		{
			for (int j = 0; j < 16; j++)
			{
				if (j < 4)
				{
					this.buffer[15 - j] = ptr[j];
				}
				else if (j < 8)
				{
					this.buffer[15 - j] = ptr[j];
				}
				else if (j < 12)
				{
					this.buffer[11 - j] = ptr[j];
				}
				else
				{
					this.buffer[19 - j] = ptr[j];
				}
			}
		}
		this.OutStream.Write(this.buffer, 0, 16);
	}

	public override void Write(double _value)
	{
		BitConverterLE.GetBytes(_value, this.buffer);
		this.OutStream.Write(this.buffer, 0, 8);
	}

	public override void Write(short _value)
	{
		this.buffer[0] = (byte)_value;
		this.buffer[1] = (byte)(_value >> 8);
		this.OutStream.Write(this.buffer, 0, 2);
	}

	public override void Write(int _value)
	{
		this.buffer[0] = (byte)_value;
		this.buffer[1] = (byte)(_value >> 8);
		this.buffer[2] = (byte)(_value >> 16);
		this.buffer[3] = (byte)(_value >> 24);
		this.OutStream.Write(this.buffer, 0, 4);
	}

	public override void Write(long _value)
	{
		int i = 0;
		int num = 0;
		while (i < 8)
		{
			this.buffer[i] = (byte)(_value >> num);
			i++;
			num += 8;
		}
		this.OutStream.Write(this.buffer, 0, 8);
	}

	public override void Write(sbyte _value)
	{
		this.buffer[0] = (byte)_value;
		this.OutStream.Write(this.buffer, 0, 1);
	}

	public override void Write(float _value)
	{
		BitConverterLE.GetBytes(_value, this.buffer);
		this.OutStream.Write(this.buffer, 0, 4);
	}

	public unsafe override void Write(string _value)
	{
		if (_value == null)
		{
			throw new ArgumentNullException("_value");
		}
		if (this.encoder == null)
		{
			this.encoder = this.encoding.GetEncoder();
		}
		int byteCount;
		fixed (string text = _value)
		{
			char* ptr = text;
			if (ptr != null)
			{
				ptr += RuntimeHelpers.OffsetToStringData / 2;
			}
			byteCount = this.encoder.GetByteCount(ptr, _value.Length, true);
		}
		this.Write7BitEncodedInt(byteCount);
		int num = 128 / this.maxBytesPerChar;
		int num2 = 0;
		int num3;
		for (int i = _value.Length; i > 0; i -= num3)
		{
			num3 = ((i <= num) ? i : num);
			int bytes;
			fixed (string text = _value)
			{
				char* ptr2 = text;
				if (ptr2 != null)
				{
					ptr2 += RuntimeHelpers.OffsetToStringData / 2;
				}
				byte[] array;
				byte* ptr3;
				if ((array = this.buffer) == null || array.Length == 0)
				{
					ptr3 = null;
				}
				else
				{
					ptr3 = &array[0];
				}
				bytes = this.encoder.GetBytes((char*)(void*)((UIntPtr)((void*)ptr2) + num2 * 2), num3, ptr3, 128, num3 == i);
				array = null;
			}
			this.OutStream.Write(this.buffer, 0, bytes);
			num2 += num3;
		}
	}

	public override void Write(ushort _value)
	{
		this.buffer[0] = (byte)_value;
		this.buffer[1] = (byte)(_value >> 8);
		this.OutStream.Write(this.buffer, 0, 2);
	}

	public override void Write(uint _value)
	{
		this.buffer[0] = (byte)_value;
		this.buffer[1] = (byte)(_value >> 8);
		this.buffer[2] = (byte)(_value >> 16);
		this.buffer[3] = (byte)(_value >> 24);
		this.OutStream.Write(this.buffer, 0, 4);
	}

	public override void Write(ulong _value)
	{
		int i = 0;
		int num = 0;
		while (i < 8)
		{
			this.buffer[i] = (byte)(_value >> num);
			i++;
			num += 8;
		}
		this.OutStream.Write(this.buffer, 0, 8);
	}

	public void Write7BitEncodedSignedInt(int _value)
	{
		long num = (long)_value;
		bool flag = num < 0L;
		num = Math.Abs(num);
		long num2 = (num >> 6) & 33554431L;
		byte b = (byte)(num & 63L);
		if (num2 != 0L)
		{
			b |= 128;
		}
		if (flag)
		{
			b |= 64;
		}
		this.Write(b);
		for (num = num2; num != 0L; num = num2)
		{
			num2 = (num >> 7) & 16777215L;
			b = (byte)(num & 127L);
			if (num2 != 0L)
			{
				b |= 128;
			}
			this.Write(b);
		}
	}

	public new void Write7BitEncodedInt(int _value)
	{
		do
		{
			int num = (_value >> 7) & 16777215;
			byte b = (byte)(_value & 127);
			if (num != 0)
			{
				b |= 128;
			}
			this.Write(b);
			_value = num;
		}
		while (_value != 0);
	}

	[MustUseReturnValue]
	public PooledBinaryWriter.StreamWriteSizeMarker ReserveSizeMarker(PooledBinaryWriter.EMarkerSize _markerSize)
	{
		long position = this.OutStream.Position;
		Array.Clear(this.buffer, 0, (int)_markerSize);
		this.OutStream.Write(this.buffer, 0, (int)_markerSize);
		return new PooledBinaryWriter.StreamWriteSizeMarker(position, _markerSize);
	}

	public void FinalizeSizeMarker(ref PooledBinaryWriter.StreamWriteSizeMarker _sizeMarker)
	{
		long position = this.OutStream.Position;
		long num = position - _sizeMarker.Position;
		if (num < 0L)
		{
			throw new Exception(string.Format("FinalizeMarker position ({0}) before Reserved position ({1})", position, _sizeMarker.Position));
		}
		uint num2;
		switch (_sizeMarker.MarkerSize)
		{
		case PooledBinaryWriter.EMarkerSize.UInt8:
			num2 = 255U;
			goto IL_007D;
		case PooledBinaryWriter.EMarkerSize.UInt16:
			num2 = 65535U;
			goto IL_007D;
		case PooledBinaryWriter.EMarkerSize.UInt32:
			num2 = uint.MaxValue;
			goto IL_007D;
		}
		throw new ArgumentOutOfRangeException("MarkerSize");
		IL_007D:
		long num3 = (long)((ulong)num2);
		if (num > num3)
		{
			throw new Exception(string.Format("Marked size ({0}) exceeding marker type ({1} maximum ({2})", num, _sizeMarker.MarkerSize.ToStringCached<PooledBinaryWriter.EMarkerSize>(), num3));
		}
		this.OutStream.Position = _sizeMarker.Position;
		switch (_sizeMarker.MarkerSize)
		{
		case PooledBinaryWriter.EMarkerSize.UInt8:
			this.Write((byte)num);
			break;
		case PooledBinaryWriter.EMarkerSize.UInt16:
			this.Write((ushort)num);
			break;
		case PooledBinaryWriter.EMarkerSize.UInt32:
			this.Write((uint)num);
			break;
		}
		this.OutStream.Position = position;
	}

	public override void Close()
	{
	}

	public void Reset()
	{
		this.SetBaseStream(null);
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
		MemoryPools.poolBinaryWriter.FreeSync(this);
	}

	public bool ReadWrite(bool _value)
	{
		this.Write(_value);
		return _value;
	}

	public byte ReadWrite(byte _value)
	{
		this.Write(_value);
		return _value;
	}

	public sbyte ReadWrite(sbyte _value)
	{
		this.Write(_value);
		return _value;
	}

	public char ReadWrite(char _value)
	{
		this.Write(_value);
		return _value;
	}

	public short ReadWrite(short _value)
	{
		this.Write(_value);
		return _value;
	}

	public ushort ReadWrite(ushort _value)
	{
		this.Write(_value);
		return _value;
	}

	public int ReadWrite(int _value)
	{
		this.Write(_value);
		return _value;
	}

	public uint ReadWrite(uint _value)
	{
		this.Write(_value);
		return _value;
	}

	public long ReadWrite(long _value)
	{
		this.Write(_value);
		return _value;
	}

	public ulong ReadWrite(ulong _value)
	{
		this.Write(_value);
		return _value;
	}

	public float ReadWrite(float _value)
	{
		this.Write(_value);
		return _value;
	}

	public double ReadWrite(double _value)
	{
		this.Write(_value);
		return _value;
	}

	public decimal ReadWrite(decimal _value)
	{
		this.Write(_value);
		return _value;
	}

	public string ReadWrite(string _value)
	{
		this.Write(_value);
		return _value;
	}

	public void ReadWrite(byte[] _buffer, int _index, int _count)
	{
		this.Write(_buffer, _index, _count);
	}

	public Vector3 ReadWrite(Vector3 _value)
	{
		this.Write(_value.x);
		this.Write(_value.y);
		this.Write(_value.z);
		return _value;
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
	public int maxBytesPerChar;

	[PublicizedFrom(EAccessModifier.Private)]
	public Encoder encoder;

	[PublicizedFrom(EAccessModifier.Private)]
	public Encoding encoding;

	public enum EMarkerSize
	{
		UInt8 = 1,
		UInt16,
		UInt32 = 4
	}

	public readonly struct StreamWriteSizeMarker
	{
		public StreamWriteSizeMarker(long _position, PooledBinaryWriter.EMarkerSize _markerSize)
		{
			this.Position = _position;
			this.MarkerSize = _markerSize;
		}

		public readonly long Position;

		public readonly PooledBinaryWriter.EMarkerSize MarkerSize;
	}
}
