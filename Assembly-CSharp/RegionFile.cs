using System;
using System.IO;

public abstract class RegionFile
{
	public long Length
	{
		get; [PublicizedFrom(EAccessModifier.Protected)]
		set;
	}

	public static string ConstructFullFilePath(string dir, int rX, int rZ, string ext)
	{
		return string.Format("{0}/r.{1}.{2}.{3}", new object[] { dir, rX, rZ, ext });
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public RegionFile(string fullFilePath, int rX, int rZ)
	{
		this.regionX = rX;
		this.regionZ = rZ;
		this.fullFilePath = fullFilePath;
	}

	public virtual void Close()
	{
	}

	public void GetPositionAndPath(out int regionX, out int regionZ, out string fullFilePath)
	{
		regionX = this.regionX;
		regionZ = this.regionZ;
		fullFilePath = this.fullFilePath;
	}

	public abstract void SaveHeaderData();

	public abstract void GetTimestampInfo(int cX, int cZ, out uint timeStamp);

	public abstract void SetTimestampInfo(int cX, int cZ, uint timeStamp);

	public abstract bool HasChunk(int _cX, int _cZ);

	public abstract int GetChunkByteCount(int _cX, int _cZ);

	public abstract void ReadData(int cX, int cZ, Stream _targetStream);

	public abstract void WriteData(int _cX, int _cZ, int _dataLength, byte _compression, byte[] _data, bool _saveHeaderToFile);

	public abstract void RemoveChunk(int cX, int cZ);

	public abstract void OptimizeLayout();

	public abstract int ChunkCount();

	[PublicizedFrom(EAccessModifier.Protected)]
	public static short ToShort(short byte1, short byte2)
	{
		int num = ((int)byte2 << 8) + (int)byte1;
		short num2;
		try
		{
			num2 = Convert.ToInt16(num);
		}
		catch
		{
			num2 = 0;
		}
		return num2;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public static void FromShort(short number, out byte byte1, out byte byte2)
	{
		byte2 = (byte)(number >> 8);
		byte1 = (byte)(number & 255);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public readonly byte[] tempReadBuffer = new byte[4096];

	[PublicizedFrom(EAccessModifier.Protected)]
	public readonly string fullFilePath;

	[PublicizedFrom(EAccessModifier.Protected)]
	public readonly int regionX;

	[PublicizedFrom(EAccessModifier.Protected)]
	public readonly int regionZ;
}
