using System;
using System.IO;
using UnityEngine;

public static class StreamUtils
{
	public static long ReadInt64(Stream clientStream)
	{
		return (long)(clientStream.ReadByte() & 255) | ((long)(clientStream.ReadByte() & 255) << 8) | ((long)(clientStream.ReadByte() & 255) << 16) | ((long)(clientStream.ReadByte() & 255) << 24) | ((long)(clientStream.ReadByte() & 255) << 32) | ((long)(clientStream.ReadByte() & 255) << 40) | ((long)(clientStream.ReadByte() & 255) << 48) | ((long)(clientStream.ReadByte() & 255) << 56);
	}

	public static void Write(Stream clientStream, long v)
	{
		clientStream.WriteByte((byte)(v & 255L));
		clientStream.WriteByte((byte)((v >> 8) & 255L));
		clientStream.WriteByte((byte)((v >> 16) & 255L));
		clientStream.WriteByte((byte)((v >> 24) & 255L));
		clientStream.WriteByte((byte)((v >> 32) & 255L));
		clientStream.WriteByte((byte)((v >> 40) & 255L));
		clientStream.WriteByte((byte)((v >> 48) & 255L));
		clientStream.WriteByte((byte)((v >> 56) & 255L));
	}

	public static int ReadInt32(Stream clientStream)
	{
		return 0 | (clientStream.ReadByte() & 255) | ((clientStream.ReadByte() & 255) << 8) | ((clientStream.ReadByte() & 255) << 16) | ((clientStream.ReadByte() & 255) << 24);
	}

	public static int ReadInt32(byte[] buffer, ref int offset)
	{
		int num = 0;
		int num2 = offset;
		offset = num2 + 1;
		int num3 = num | (int)buffer[num2];
		num2 = offset;
		offset = num2 + 1;
		int num4 = num3 | ((int)buffer[num2] << 8);
		num2 = offset;
		offset = num2 + 1;
		int num5 = num4 | ((int)buffer[num2] << 16);
		num2 = offset;
		offset = num2 + 1;
		return num5 | ((int)buffer[num2] << 24);
	}

	public static void Write(Stream clientStream, int v)
	{
		clientStream.WriteByte((byte)(v & 255));
		clientStream.WriteByte((byte)((v >> 8) & 255));
		clientStream.WriteByte((byte)((v >> 16) & 255));
		clientStream.WriteByte((byte)((v >> 24) & 255));
	}

	public static void Write(byte[] buffer, int v, ref int offset)
	{
		int num = offset;
		offset = num + 1;
		buffer[num] = (byte)(v & 255);
		num = offset;
		offset = num + 1;
		buffer[num] = (byte)((v >> 8) & 255);
		num = offset;
		offset = num + 1;
		buffer[num] = (byte)((v >> 16) & 255);
		num = offset;
		offset = num + 1;
		buffer[num] = (byte)((v >> 24) & 255);
	}

	public static ushort ReadUInt16(Stream clientStream)
	{
		return 0 | (ushort)(clientStream.ReadByte() & 255) | (ushort)((clientStream.ReadByte() & 255) << 8);
	}

	public static ushort ReadUInt16(byte[] buffer, ref int offset)
	{
		byte b = 0;
		int num = offset;
		offset = num + 1;
		ushort num2 = (ushort)(b | buffer[num]);
		num = offset;
		offset = num + 1;
		return num2 | (ushort)(buffer[num] << 8);
	}

	public static void Write(Stream clientStream, ushort v)
	{
		clientStream.WriteByte((byte)(v & 255));
		clientStream.WriteByte((byte)((v >> 8) & 255));
	}

	public static short ReadInt16(Stream clientStream)
	{
		return 0 | (short)(clientStream.ReadByte() & 255) | (short)((clientStream.ReadByte() & 255) << 8);
	}

	public static short ReadInt16(byte[] buffer, ref int offset)
	{
		byte b = 0;
		int num = offset;
		offset = num + 1;
		short num2 = (short)(b | buffer[num]);
		num = offset;
		offset = num + 1;
		return num2 | (short)(buffer[num] << 8);
	}

	public static void Write(Stream clientStream, short v)
	{
		clientStream.WriteByte((byte)(v & 255));
		clientStream.WriteByte((byte)((v >> 8) & 255));
	}

	public static byte ReadByte(Stream clientStream)
	{
		return (byte)clientStream.ReadByte();
	}

	public static byte ReadByte(byte[] buffer, ref int offset)
	{
		int num = offset;
		offset = num + 1;
		return buffer[num];
	}

	public static void Write(Stream clientStream, byte _b)
	{
		clientStream.WriteByte(_b);
	}

	public static void Write(BinaryWriter _bw, Vector3 _v)
	{
		_bw.Write(_v.x);
		_bw.Write(_v.y);
		_bw.Write(_v.z);
	}

	public static Vector3 ReadVector3(BinaryReader _br)
	{
		return new Vector3(_br.ReadSingle(), _br.ReadSingle(), _br.ReadSingle());
	}

	public static void Write(BinaryWriter _bw, Vector3i _v)
	{
		_bw.Write(_v.x);
		_bw.Write(_v.y);
		_bw.Write(_v.z);
	}

	public static Vector3i ReadVector3i(BinaryReader _br)
	{
		return new Vector3i(_br.ReadInt32(), _br.ReadInt32(), _br.ReadInt32());
	}

	public static void Write(BinaryWriter _bw, Vector2 _v)
	{
		_bw.Write(_v.x);
		_bw.Write(_v.y);
	}

	public static Vector2 ReadVector2(BinaryReader _br)
	{
		return new Vector2(_br.ReadSingle(), _br.ReadSingle());
	}

	public static void Write(BinaryWriter _bw, Vector2i _v)
	{
		_bw.Write(_v.x);
		_bw.Write(_v.y);
	}

	public static Vector2i ReadVector2i(BinaryReader _br)
	{
		return new Vector2i(_br.ReadInt32(), _br.ReadInt32());
	}

	public static void Write(BinaryWriter _bw, Quaternion _q)
	{
		_bw.Write(_q.x);
		_bw.Write(_q.y);
		_bw.Write(_q.z);
		_bw.Write(_q.w);
	}

	public static Quaternion ReadQuaterion(BinaryReader _br)
	{
		return new Quaternion(_br.ReadSingle(), _br.ReadSingle(), _br.ReadSingle(), _br.ReadSingle());
	}

	public static Color ReadColor(BinaryReader _br)
	{
		return new Color(_br.ReadSingle(), _br.ReadSingle(), _br.ReadSingle(), _br.ReadSingle());
	}

	public static void Write(BinaryWriter _bw, Color _c)
	{
		_bw.Write(_c.r);
		_bw.Write(_c.g);
		_bw.Write(_c.b);
		_bw.Write(_c.a);
	}

	public static Color ReadColor32(BinaryReader _br)
	{
		uint num = _br.ReadUInt32();
		return new Color(((num >> 24) & 255U) / 255f, ((num >> 16) & 255U) / 255f, ((num >> 8) & 255U) / 255f, (num & 255U) / 255f);
	}

	public static void WriteColor32(BinaryWriter _bw, Color _c)
	{
		uint num = ((uint)(_c.r * 255f) << 24) | ((uint)(_c.g * 255f) << 16) | ((uint)(_c.b * 255f) << 8) | (uint)(_c.a * 255f);
		_bw.Write(num);
	}

	public static void Write(BinaryWriter _bw, string _s)
	{
		_bw.Write(_s != null);
		if (_s != null)
		{
			_bw.Write(_s);
		}
	}

	public static string ReadString(BinaryReader _br)
	{
		if (!_br.ReadBoolean())
		{
			return null;
		}
		return _br.ReadString();
	}

	public static void Write7BitEncodedSignedInt(Stream _stream, int _value)
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
		StreamUtils.Write(_stream, b);
		for (num = num2; num != 0L; num = num2)
		{
			num2 = (num >> 7) & 16777215L;
			b = (byte)(num & 127L);
			if (num2 != 0L)
			{
				b |= 128;
			}
			StreamUtils.Write(_stream, b);
		}
	}

	public static void Write7BitEncodedInt(Stream _stream, int _value)
	{
		do
		{
			int num = (_value >> 7) & 16777215;
			byte b = (byte)(_value & 127);
			if (num != 0)
			{
				b |= 128;
			}
			StreamUtils.Write(_stream, b);
			_value = num;
		}
		while (_value != 0);
	}

	public static int Read7BitEncodedSignedInt(Stream _stream)
	{
		int num = 0;
		int i = 0;
		byte b = (byte)(_stream.ReadByte() & 255);
		num |= (int)(b & 63);
		i += 6;
		bool flag = (b & 64) > 0;
		if ((b & 128) != 0)
		{
			while (i < 32)
			{
				b = (byte)(_stream.ReadByte() & 255);
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

	public static int Read7BitEncodedInt(Stream _stream)
	{
		int num = 0;
		int i = 0;
		while (i < 35)
		{
			byte b = (byte)(_stream.ReadByte() & 255);
			num |= (int)(b & 127) << i;
			i += 7;
			if ((b & 128) == 0)
			{
				return num;
			}
		}
		throw new FormatException("Illegal encoding for 7 bit encoded int");
	}

	public unsafe static Guid ReadGuid(BinaryReader _br)
	{
		Span<byte> span = new Span<byte>(stackalloc byte[(UIntPtr)16], 16);
		if (_br.Read(span) != 16)
		{
			throw new EndOfStreamException("Failed to read 16 bytes for Guid");
		}
		return new Guid(span);
	}

	public unsafe static void Write(BinaryWriter _bw, Guid _guid)
	{
		Span<byte> span = new Span<byte>(stackalloc byte[(UIntPtr)16], 16);
		if (!_guid.TryWriteBytes(span))
		{
			throw new InvalidOperationException("Failed to write Guid to span");
		}
		_bw.Write(span);
	}

	public static void StreamCopy(Stream _source, Stream _destination, byte[] _tempBuf = null, bool _bFlush = true)
	{
		byte[] array = _tempBuf ?? MemoryPools.poolByte.Alloc(4096);
		bool flag = true;
		while (flag)
		{
			int num = _source.Read(array, 0, array.Length);
			if (num > 0)
			{
				_destination.Write(array, 0, num);
			}
			else
			{
				if (_bFlush)
				{
					_destination.Flush();
				}
				flag = false;
			}
		}
		if (_tempBuf == null)
		{
			MemoryPools.poolByte.Free(array);
		}
	}

	public static void StreamCopy(Stream _source, Stream _destination, int _length, byte[] _tempBuf = null, bool _bFlush = true)
	{
		byte[] array = _tempBuf ?? MemoryPools.poolByte.Alloc(4096);
		bool flag = true;
		while (flag)
		{
			int num = _source.Read(array, 0, Math.Min(_length, array.Length));
			if (num > 0)
			{
				_destination.Write(array, 0, num);
				_length -= num;
			}
			else
			{
				if (_bFlush)
				{
					_destination.Flush();
				}
				flag = false;
			}
		}
		if (_tempBuf == null)
		{
			MemoryPools.poolByte.Free(array);
		}
	}

	public static void WriteStreamToFile(Stream _source, string _fileName)
	{
		using (Stream stream = SdFile.Create(_fileName))
		{
			StreamUtils.StreamCopy(_source, stream, null, true);
		}
	}

	public static void WriteStreamToFile(Stream _source, string _fileName, int _length)
	{
		using (Stream stream = SdFile.Create(_fileName))
		{
			StreamUtils.StreamCopy(_source, stream, _length, null, true);
		}
	}
}
