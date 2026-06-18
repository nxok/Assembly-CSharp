using System;
using System.Collections.Generic;
using System.IO;

public class RegionFileV2 : RegionFileSectorBased
{
	public RegionFileV2(string _fullFilePath, int _rX, int _rZ, Stream _fileStream, int _version)
		: base(_fullFilePath, _rX, _rZ, _version)
	{
		try
		{
			if (_fileStream == null)
			{
				_fileStream = SdFile.Open(_fullFilePath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
				byte[] array = new byte[12288];
				array[0] = RegionFileSectorBased.FileHeaderMagicBytes[0];
				array[1] = RegionFileSectorBased.FileHeaderMagicBytes[1];
				array[2] = RegionFileSectorBased.FileHeaderMagicBytes[2];
				array[3] = (byte)_version;
				_fileStream.Write(array, 0, array.Length);
				_fileStream.Seek(0L, SeekOrigin.Begin);
			}
			_fileStream.Seek(4096L, SeekOrigin.Begin);
			_fileStream.Read(this.regionLocationHeader, 0, 4096);
			_fileStream.Read(this.regionTimestampHeader, 0, 4096);
			this.initSectorInfo();
			base.Length = _fileStream.Length;
		}
		finally
		{
			if (_fileStream != null)
			{
				_fileStream.Dispose();
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void initSectorInfo()
	{
		for (int i = 0; i < 32; i++)
		{
			for (int j = 0; j < 32; j++)
			{
				short num;
				byte b;
				this.GetLocationInfo(j, i, out num, out b);
				if (b > 0)
				{
					this.usedSectors[(int)num] = (int)b;
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int findFreeSectorOfSize(int _sectorLength)
	{
		int num2;
		lock (this)
		{
			int num = 3;
			foreach (KeyValuePair<int, int> keyValuePair in this.usedSectors)
			{
				if (keyValuePair.Key - num >= _sectorLength)
				{
					return num;
				}
				num = keyValuePair.Key + keyValuePair.Value;
			}
			num2 = num;
		}
		return num2;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void SetLocationInfo(int _cX, int _cZ, short _sectorOffset, byte _sectorLength)
	{
		lock (this)
		{
			base.SetLocationInfo(_cX, _cZ, _sectorOffset, _sectorLength);
			this.usedSectors[(int)_sectorOffset] = (int)_sectorLength;
		}
	}

	public override void SaveHeaderData()
	{
		lock (this)
		{
			using (Stream stream = SdFile.Open(this.fullFilePath, FileMode.Open, FileAccess.Write, FileShare.ReadWrite))
			{
				stream.Seek(4096L, SeekOrigin.Begin);
				stream.Write(this.regionLocationHeader, 0, 4096);
				stream.Write(this.regionTimestampHeader, 0, 4096);
				base.Length = stream.Length;
			}
		}
	}

	public override void ReadData(int _cX, int _cZ, Stream _targetStream)
	{
		lock (this)
		{
			short num;
			byte b;
			this.GetLocationInfo(_cX, _cZ, out num, out b);
			using (Stream stream = SdFile.Open(this.fullFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
			{
				long num2 = (long)(num * 4096);
				int num3 = (int)b * 4096;
				stream.Seek(num2, SeekOrigin.Begin);
				int num4 = StreamUtils.ReadInt32(stream);
				stream.Seek(12L, SeekOrigin.Current);
				long num5 = (long)num3 + num2;
				long position = _targetStream.Position;
				if (num4 > 0)
				{
					_targetStream.SetLength(0L);
				}
				if (num < 3)
				{
					Log.Error(string.Format("ChunkRead: R={0}/{1}, C={2}/{3}, Off={4}, Len={5}, DataLen={6}, TotalLen={7}, FOStart={8}, FSize={9}, FOEndExp={10}, FOEndRead={11}, TSPosBefore={12}", new object[]
					{
						this.regionX,
						this.regionZ,
						_cX,
						_cZ,
						num,
						b,
						num4,
						num4 + 16,
						num2,
						num3,
						num5,
						stream.Position,
						position
					}));
				}
				else
				{
					try
					{
						StreamUtils.StreamCopy(stream, _targetStream, num4, this.tempReadBuffer, true);
					}
					catch (NotSupportedException)
					{
						Log.Error(string.Format("ChunkRead: R={0}/{1}, C={2}/{3}, Off={4}, Len={5}, DataLen={6}, TotalLen={7}, FOStart={8}, FSize={9}, FOEndExp={10}, FOEndRead={11}, TSPosBefore={12}", new object[]
						{
							this.regionX,
							this.regionZ,
							_cX,
							_cZ,
							num,
							b,
							num4,
							num4 + 16,
							num2,
							num3,
							num5,
							stream.Position,
							position
						}));
						throw;
					}
				}
				base.Length = stream.Length;
			}
		}
	}

	public override void WriteData(int _cX, int _cZ, int _dataLength, byte _compression, byte[] _data, bool _saveHeaderToFile)
	{
		lock (this)
		{
			uint num = GameUtils.WorldTimeToTotalMinutes(GameManager.Instance.World.worldTime);
			int num2 = _dataLength + 16;
			byte b = (byte)Math.Ceiling((double)num2 / 4096.0);
			using (Stream stream = SdFile.Open(this.fullFilePath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
			{
				short num3;
				byte b2;
				this.GetLocationInfo(_cX, _cZ, out num3, out b2);
				if (num3 == 0 || b > b2)
				{
					if (num3 > 0)
					{
						this.usedSectors.Remove((int)num3);
					}
					num3 = (short)this.findFreeSectorOfSize((int)b);
					this.SetLocationInfo(_cX, _cZ, num3, b);
				}
				if (num3 < 3)
				{
					Log.Error(string.Format("Sector offset < 3: R={0}/{1}, C={2}/{3}, Off={4}, Len={5}, DataLen={6}", new object[] { this.regionX, this.regionZ, _cX, _cZ, num3, b, _dataLength }));
				}
				this.SetTimestampInfo(_cX, _cZ, num);
				long num4 = (long)(num3 * 4096);
				int num5 = (int)b * 4096;
				long num6 = (long)num5 + num4;
				stream.Seek(num4, SeekOrigin.Begin);
				StreamUtils.Write(stream, _dataLength);
				stream.Seek(12L, SeekOrigin.Current);
				stream.Write(_data, 0, _dataLength);
				stream.Write(RegionFileV2.emptyBytes, 0, num5 - num2);
				long position = stream.Position;
				if (position != num6)
				{
					Log.Error(string.Format("Wrong write end: R={0}/{1}, C={2}/{3}, Off={4}, Len={5}, DataLen={6}, TotalLen={7}, FOStart={8}, FSize={9}, FOEndExp={10}, FOEndFound={11}", new object[]
					{
						this.regionX, this.regionZ, _cX, _cZ, num3, b, _dataLength, num2, num4, num5,
						num6, position
					}));
				}
				base.Length = stream.Length;
			}
			if (_saveHeaderToFile)
			{
				this.SaveHeaderData();
			}
		}
	}

	public override void RemoveChunk(int _cX, int _cZ)
	{
		lock (this)
		{
			short num;
			byte b;
			this.GetLocationInfo(_cX, _cZ, out num, out b);
			if (num > 0 && b > 0)
			{
				this.usedSectors.Remove((int)num);
			}
			base.RemoveChunk(_cX, _cZ);
		}
	}

	public override void OptimizeLayout()
	{
		using (Stream stream = SdFile.Open(this.fullFilePath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
		{
			int num = 3;
			foreach (int num2 in this.usedSectors.Values)
			{
				num += num2;
			}
			int num3 = num * 4096;
			this.usedSectors.Clear();
			RegionFileV2.optimizerMemoryStream.Position = 0L;
			stream.Seek(0L, SeekOrigin.Begin);
			StreamUtils.StreamCopy(stream, RegionFileV2.optimizerMemoryStream, 12288, null, true);
			for (int i = 0; i < 32; i++)
			{
				for (int j = 0; j < 32; j++)
				{
					short num4;
					byte b;
					this.GetLocationInfo(j, i, out num4, out b);
					if (num4 > 0 && b > 0)
					{
						stream.Seek((long)(num4 * 4096), SeekOrigin.Begin);
						short num5 = (short)(RegionFileV2.optimizerMemoryStream.Position / 4096L);
						StreamUtils.StreamCopy(stream, RegionFileV2.optimizerMemoryStream, (int)b * 4096, null, true);
						this.SetLocationInfo(j, i, num5, b);
					}
				}
			}
			RegionFileV2.optimizerMemoryStream.Seek(4096L, SeekOrigin.Begin);
			RegionFileV2.optimizerMemoryStream.Write(this.regionLocationHeader, 0, 4096);
			stream.SetLength((long)num3);
			stream.Position = 0L;
			RegionFileV2.optimizerMemoryStream.Position = 0L;
			StreamUtils.StreamCopy(RegionFileV2.optimizerMemoryStream, stream, num3, null, true);
			base.Length = stream.Length;
		}
	}

	public override int ChunkCount()
	{
		int num = 0;
		using (SortedDictionary<int, int>.ValueCollection.Enumerator enumerator = this.usedSectors.Values.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				if (enumerator.Current > 0)
				{
					num++;
				}
			}
		}
		return num;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly SortedDictionary<int, int> usedSectors = new SortedDictionary<int, int>();

	[PublicizedFrom(EAccessModifier.Private)]
	public const int ReservedSectors = 3;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int ReservedBytesPerEntry = 16;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly byte[] emptyBytes = new byte[4096];

	[PublicizedFrom(EAccessModifier.Private)]
	public static MemoryStream optimizerMemoryStream = new MemoryStream(16789504);
}
