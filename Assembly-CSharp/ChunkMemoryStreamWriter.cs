using System;
using System.IO;

[PublicizedFrom(EAccessModifier.Internal)]
public class ChunkMemoryStreamWriter : MemoryStream
{
	public ChunkMemoryStreamWriter()
		: this(new byte[512000])
	{
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public ChunkMemoryStreamWriter(byte[] _buffer)
		: base(_buffer)
	{
		this.buffer = _buffer;
	}

	public void Init(RegionFileAccessMultipleChunks _regionFileAccess, string _dir, int _x, int _z, string _ext)
	{
		this.dir = _dir;
		this.chunkX = _x;
		this.chunkZ = _z;
		this.ext = _ext;
		this.regionFileAccess = _regionFileAccess;
		this.Seek(0L, SeekOrigin.Begin);
	}

	public override void Close()
	{
		this.regionFileAccess.Write(this.dir, this.chunkX, this.chunkZ, this.ext, this.buffer, (int)this.Position);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string dir;

	[PublicizedFrom(EAccessModifier.Private)]
	public int chunkX;

	[PublicizedFrom(EAccessModifier.Private)]
	public int chunkZ;

	[PublicizedFrom(EAccessModifier.Private)]
	public string ext;

	[PublicizedFrom(EAccessModifier.Private)]
	public RegionFileAccessMultipleChunks regionFileAccess;

	[PublicizedFrom(EAccessModifier.Private)]
	public byte[] buffer;
}
