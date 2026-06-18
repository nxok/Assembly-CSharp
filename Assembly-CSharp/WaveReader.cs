using System;
using System.IO;

public class WaveReader
{
	public WaveReader(string _fileName)
	{
		this.filename = _fileName;
		this.buffer = MemoryPools.poolByte.Alloc(8192);
		using (BinaryReader binaryReader = new BinaryReader(SdFile.Open(this.filename, FileMode.Open, FileAccess.Read, FileShare.Read)))
		{
			string text = new string(binaryReader.ReadChars(88));
			this.position = (this.dataStartPos = text.IndexOf("data") + 8);
		}
	}

	public int Position
	{
		set
		{
			this.position = this.dataStartPos + value;
		}
	}

	public void Read(float[] data, int count)
	{
		using (BinaryReader binaryReader = new BinaryReader(SdFile.Open(this.filename, FileMode.Open, FileAccess.Read, FileShare.Read)))
		{
			binaryReader.BaseStream.Position = (long)this.position;
			binaryReader.Read(this.buffer, 0, 8192);
			for (int i = 0; i < data.Length; i++)
			{
				short num = BitConverter.ToInt16(this.buffer, 2 * i);
				data[i] = (float)num / 32767f;
			}
		}
	}

	public void Cleanup()
	{
		MemoryPools.poolByte.Free(this.buffer);
		this.buffer = null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string filename;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int bufferSize = 8192;

	[PublicizedFrom(EAccessModifier.Private)]
	public byte[] buffer;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly int dataStartPos;

	[PublicizedFrom(EAccessModifier.Private)]
	public int position;
}
