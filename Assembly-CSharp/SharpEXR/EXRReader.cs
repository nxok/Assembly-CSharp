using System;
using System.IO;
using System.Text;

namespace SharpEXR
{
	public class EXRReader : IDisposable, IEXRReader
	{
		public EXRReader(Stream stream, bool leaveOpen = false)
			: this(new BinaryReader(stream, Encoding.ASCII, leaveOpen))
		{
		}

		public EXRReader(BinaryReader reader)
		{
			this.reader = reader;
		}

		public byte ReadByte()
		{
			return this.reader.ReadByte();
		}

		public int ReadInt32()
		{
			return this.reader.ReadInt32();
		}

		public uint ReadUInt32()
		{
			return this.reader.ReadUInt32();
		}

		public Half ReadHalf()
		{
			return Half.ToHalf(this.reader.ReadUInt16());
		}

		public float ReadSingle()
		{
			return this.reader.ReadSingle();
		}

		public double ReadDouble()
		{
			return this.reader.ReadDouble();
		}

		public string ReadNullTerminatedString(int maxLength)
		{
			long position = this.reader.BaseStream.Position;
			StringBuilder stringBuilder = new StringBuilder();
			byte b;
			while ((b = this.reader.ReadByte()) != 0)
			{
				if (this.reader.BaseStream.Position - position > (long)maxLength)
				{
					throw new EXRFormatException("Null terminated string exceeded maximum length of " + maxLength.ToString() + " bytes.");
				}
				stringBuilder.Append((char)b);
			}
			return stringBuilder.ToString();
		}

		public string ReadString()
		{
			int num = this.ReadInt32();
			return this.ReadString(num);
		}

		public string ReadString(int length)
		{
			StringBuilder stringBuilder = new StringBuilder();
			for (int i = 0; i < length; i++)
			{
				stringBuilder.Append((char)this.reader.ReadByte());
			}
			return stringBuilder.ToString();
		}

		public byte[] ReadBytes(int count)
		{
			return this.reader.ReadBytes(count);
		}

		public void CopyBytes(byte[] dest, int offset, int count)
		{
			if (this.reader.BaseStream.Read(dest, offset, count) != count)
			{
				throw new Exception("Less bytes read than expected");
			}
		}

		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public virtual void Dispose(bool disposing)
		{
			if (this.disposed)
			{
				return;
			}
			if (disposing)
			{
				try
				{
					this.reader.Dispose();
				}
				catch
				{
				}
			}
			this.disposed = true;
		}

		public int Position
		{
			get
			{
				return (int)this.reader.BaseStream.Position;
			}
			set
			{
				this.reader.BaseStream.Seek((long)value, SeekOrigin.Begin);
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public BinaryReader reader;

		[PublicizedFrom(EAccessModifier.Private)]
		public bool disposed;
	}
}
