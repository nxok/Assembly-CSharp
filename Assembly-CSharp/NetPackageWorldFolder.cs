using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Noemax.GZip;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageWorldFolder : NetPackage
{
	public override bool Compress
	{
		get
		{
			return false;
		}
	}

	public override int Channel
	{
		get
		{
			return 1;
		}
	}

	public NetPackageWorldFolder Setup(byte[] _data, int _seqNr, int _totalParts)
	{
		this.data = _data;
		this.seqNr = _seqNr;
		this.totalParts = _totalParts;
		return this;
	}

	public override void read(PooledBinaryReader _reader)
	{
		this.seqNr = _reader.ReadInt32();
		this.totalParts = _reader.ReadInt32();
		int num = _reader.ReadInt32();
		if (num >= 0)
		{
			this.data = _reader.ReadBytes(num);
		}
	}

	public override void write(PooledBinaryWriter _writer)
	{
		base.write(_writer);
		_writer.Write(this.seqNr);
		_writer.Write(this.totalParts);
		_writer.Write((this.data != null) ? this.data.Length : (-1));
		if (this.data != null)
		{
			_writer.Write(this.data);
		}
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			NetPackageWorldFolder.StartSendingPacketsToClient(base.Sender);
			return;
		}
		DateTime now = DateTime.Now;
		if (NetPackageWorldFolder.downloadStartTime > now)
		{
			NetPackageWorldFolder.downloadStartTime = now;
		}
		float num = (float)(this.seqNr + 1) / (float)this.totalParts;
		TimeSpan timeSpan = TimeSpan.FromSeconds((now - NetPackageWorldFolder.downloadStartTime).TotalSeconds / (double)num * (double)(1f - num));
		if ((double)num > 0.05)
		{
			XUiC_ProgressWindow.SetText(LocalPlayerUI.primaryUI, string.Format(Localization.Get("uiLoadDownloadingWorldEstimate", false, null), num, (int)timeSpan.TotalMinutes, timeSpan.Seconds), true);
		}
		else
		{
			XUiC_ProgressWindow.SetText(LocalPlayerUI.primaryUI, string.Format(Localization.Get("uiLoadDownloadingWorld", false, null), num), true);
		}
		NetPackageWorldFolder.ReceiveStream.Write(this.data, 0, this.data.Length);
		if (this.seqNr == this.totalParts - 1)
		{
			ThreadManager.StartCoroutine(NetPackageWorldFolder.uncompressWorld());
		}
	}

	public override int GetLength()
	{
		return 16 + ((this.data != null) ? this.data.Length : 0);
	}

	public override NetPackageDirection PackageDirection
	{
		get
		{
			return NetPackageDirection.Both;
		}
	}

	public static IEnumerator TestWorldValid(string _locationPath, Dictionary<string, uint> _worldFileHashes, Action<bool> _resultCallback)
	{
		byte[] buffer = new byte[8192];
		using (Dictionary<string, uint>.Enumerator enumerator = _worldFileHashes.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				NetPackageWorldFolder.<>c__DisplayClass17_0 CS$<>8__locals1 = new NetPackageWorldFolder.<>c__DisplayClass17_0();
				CS$<>8__locals1.hashEntry = enumerator.Current;
				string text = _locationPath + "/" + CS$<>8__locals1.hashEntry.Key;
				if (!SdFile.Exists(text))
				{
					Log.Out("World file {0} does not exist", new object[] { CS$<>8__locals1.hashEntry.Key });
					_resultCallback(false);
					yield break;
				}
				CS$<>8__locals1.validHash = true;
				yield return IOUtils.CalcCrcCoroutine(text, delegate(uint _crc)
				{
					if (_crc != CS$<>8__locals1.hashEntry.Value)
					{
						CS$<>8__locals1.validHash = false;
						Log.Out("World file {0} is different than server's version: received {1:X8}, calculated {2:X8}", new object[]
						{
							CS$<>8__locals1.hashEntry.Key,
							CS$<>8__locals1.hashEntry.Value,
							_crc
						});
						return;
					}
					CS$<>8__locals1.validHash = true;
				}, 15, buffer);
				if (!CS$<>8__locals1.validHash)
				{
					_resultCallback(false);
					yield break;
				}
				CS$<>8__locals1 = null;
			}
		}
		Dictionary<string, uint>.Enumerator enumerator = default(Dictionary<string, uint>.Enumerator);
		_resultCallback(true);
		yield break;
		yield break;
	}

	public static IEnumerator RequestWorld()
	{
		NetPackageWorldFolder.ReceiveStream = new MemoryStream();
		NetPackageWorldFolder.WorldReceivedAndUncompressed = false;
		NetPackageWorldFolder.downloadStartTime = DateTime.MaxValue;
		SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageWorldFolder>(), false);
		while (!NetPackageWorldFolder.WorldReceivedAndUncompressed && SingletonMonoBehaviour<ConnectionManager>.Instance.IsConnected)
		{
			yield return null;
		}
		yield break;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static IEnumerator uncompressWorld()
	{
		string worldFolder = GameIO.GetSaveGameLocalDir() + "/World";
		NetPackageWorldFolder.ReceiveStream.Position = 0L;
		DeflateInputStream zipStream = new DeflateInputStream(NetPackageWorldFolder.ReceiveStream);
		BinaryReader reader = new BinaryReader(zipStream);
		SdDirectory.CreateDirectory(worldFolder);
		int fileCount = reader.ReadInt32();
		MicroStopwatch mswCompressing = new MicroStopwatch();
		byte[] buffer = MemoryPools.poolByte.Alloc(4096);
		yield return null;
		int num3;
		for (int i = 0; i < fileCount; i = num3 + 1)
		{
			string text = reader.ReadString();
			long fileSize = reader.ReadInt64();
			if (text.StartsWith('.') || text.IndexOfAny(GameIO.ResourcePathSeparators) >= 0)
			{
				Log.Warning("Received world files contains file with parent path specifier or path separator: " + text);
				bool bWhileCopying = true;
				while (bWhileCopying)
				{
					if (fileSize <= 0L)
					{
						break;
					}
					int num = zipStream.Read(buffer, 0, (int)Math.Min(fileSize, (long)buffer.Length));
					if (num > 0)
					{
						fileSize -= (long)num;
					}
					else
					{
						bWhileCopying = false;
					}
					if (bWhileCopying && mswCompressing.ElapsedMilliseconds > (long)NetPackageWorldFolder.MaxTimePerFrame)
					{
						yield return null;
						mswCompressing.ResetAndRestart();
					}
				}
			}
			else
			{
				Stream fs = SdFile.Create(worldFolder + "/" + text);
				mswCompressing.ResetAndRestart();
				if (text.StartsWith("dtm", StringComparison.OrdinalIgnoreCase) && text.EndsWith(".raw", StringComparison.OrdinalIgnoreCase))
				{
					yield return ThreadManager.StartCoroutine(NetPackageWorldFolder.readDtmDelta(fs, zipStream, mswCompressing, fileSize));
				}
				else
				{
					bool bWhileCopying = true;
					while (bWhileCopying && fileSize > 0L)
					{
						int num2 = zipStream.Read(buffer, 0, (int)Math.Min(fileSize, (long)buffer.Length));
						if (num2 > 0)
						{
							fs.Write(buffer, 0, num2);
							fileSize -= (long)num2;
						}
						else
						{
							bWhileCopying = false;
						}
						if (bWhileCopying && mswCompressing.ElapsedMilliseconds > (long)NetPackageWorldFolder.MaxTimePerFrame)
						{
							yield return null;
							mswCompressing.ResetAndRestart();
						}
					}
				}
				fs.Dispose();
				yield return null;
				fs = null;
			}
			num3 = i;
		}
		MemoryPools.poolByte.Free(buffer);
		zipStream.Dispose();
		yield return null;
		SdFile.WriteAllBytes(worldFolder + "/completed", new byte[0]);
		NetPackageWorldFolder.WorldReceivedAndUncompressed = true;
		yield break;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static IEnumerator readDtmDelta(Stream _fs, DeflateInputStream _zipStream, MicroStopwatch _mswCompressing, long _fileSize)
	{
		int w = (int)Math.Sqrt((double)(_fileSize / 2L));
		int h = w;
		int lineBytes = w * 2;
		byte[] readLineData = new byte[lineBytes];
		byte[] writeLineData = new byte[lineBytes];
		MemoryStream readStream = new MemoryStream(readLineData);
		MemoryStream writeStream = new MemoryStream(writeLineData);
		int num4;
		for (int z = 0; z < h; z = num4 + 1)
		{
			_zipStream.Read(readLineData, 0, lineBytes);
			readStream.Position = 0L;
			writeStream.Position = 0L;
			int num = (int)StreamUtils.ReadUInt16(readStream);
			StreamUtils.Write(writeStream, (ushort)num);
			for (int i = 1; i < w; i++)
			{
				int num2 = (int)StreamUtils.ReadInt16(readStream);
				int num3 = num + num2;
				num = num3;
				if (num3 < 0 || num3 > 65535)
				{
					Log.Out("Current out of range: " + num3.ToString());
				}
				StreamUtils.Write(writeStream, (ushort)num3);
			}
			_fs.Write(writeLineData, 0, lineBytes);
			if (_mswCompressing.ElapsedMilliseconds > (long)NetPackageWorldFolder.MaxTimePerFrame)
			{
				yield return null;
				_mswCompressing.ResetAndRestart();
			}
			num4 = z;
		}
		yield break;
	}

	public static int MaxTimePerFrame
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			if (!GameManager.IsDedicatedServer)
			{
				return 5;
			}
			return 40;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void StartSendingPacketsToClient(ClientInfo _cInfo)
	{
		string @string = GamePrefs.GetString(EnumGamePrefs.GameWorld);
		if (NetPackageWorldFolder.CompressedWorldDataChunks == null || @string != NetPackageWorldFolder.CachedWorldName)
		{
			NetPackageWorldFolder.CompressedWorldDataChunks = null;
			NetPackageWorldFolder.CachedWorldName = null;
			object lockObj = NetPackageWorldFolder.LockObj;
			lock (lockObj)
			{
				if (NetPackageWorldFolder.PreparationCoroutine == null)
				{
					NetPackageWorldFolder.PreparationCoroutine = NetPackageWorldFolder.prepareWorldFolderData();
					ThreadManager.StartCoroutine(NetPackageWorldFolder.PreparationCoroutine);
				}
			}
		}
		ThreadManager.StartCoroutine(NetPackageWorldFolder.sendPacketsToClient(_cInfo));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static IEnumerator prepareWorldFolderData()
	{
		int @int = GamePrefs.GetInt(EnumGamePrefs.ServerMaxWorldTransferSpeedKiBs);
		if (@int > 0)
		{
			NetPackageWorldFolder.PACKET_SEND_DELAY = new WaitForSeconds(65536f / (float)(@int * 1024));
		}
		Log.Out("Preparing World chunks for clients");
		MemoryStream memStream = new MemoryStream();
		DeflateOutputStream zipStream = new DeflateOutputStream(memStream, 3);
		BinaryWriter writer = new BinaryWriter(zipStream);
		string worldFolder = PathAbstractions.Contextual.FindActiveWorldLocation().FullPath;
		List<string> worldFiles = GameUtils.GetWorldFilesToTransmitToClient(worldFolder);
		yield return null;
		byte[] buffer = MemoryPools.poolByte.Alloc(4096);
		MicroStopwatch mswCompressing = new MicroStopwatch();
		writer.Write(worldFiles.Count);
		int num2;
		for (int i = 0; i < worldFiles.Count; i = num2 + 1)
		{
			string text = worldFiles[i];
			string text2 = worldFolder + "/" + text;
			Stream fs = SdFile.OpenRead(text2);
			long length = fs.Length;
			writer.Write(text);
			writer.Write(length);
			mswCompressing.ResetAndRestart();
			if (text.StartsWith("dtm", StringComparison.OrdinalIgnoreCase) && text.EndsWith(".raw", StringComparison.OrdinalIgnoreCase))
			{
				yield return ThreadManager.StartCoroutine(NetPackageWorldFolder.writeDtmDelta(fs, zipStream, mswCompressing));
			}
			else
			{
				bool bWhileCopying = true;
				while (bWhileCopying)
				{
					int num = fs.Read(buffer, 0, buffer.Length);
					if (num > 0)
					{
						zipStream.Write(buffer, 0, num);
					}
					else
					{
						bWhileCopying = false;
					}
					if (bWhileCopying && mswCompressing.ElapsedMilliseconds > (long)NetPackageWorldFolder.MaxTimePerFrame)
					{
						yield return null;
						mswCompressing.ResetAndRestart();
					}
				}
			}
			fs.Dispose();
			yield return null;
			fs = null;
			num2 = i;
		}
		MemoryPools.poolByte.Free(buffer);
		buffer = null;
		mswCompressing = null;
		zipStream.Flush();
		memStream.Position = 0L;
		yield return null;
		long num3 = memStream.Length / 65536L;
		long num4 = memStream.Length % 65536L;
		if (num4 == 0L)
		{
			num4 = 65536L;
		}
		else
		{
			num3 += 1L;
		}
		List<byte[]> list = new List<byte[]>();
		int num5 = 0;
		while ((long)num5 < num3)
		{
			long num6 = 65536L;
			if ((long)num5 == num3 - 1L)
			{
				num6 = num4;
			}
			byte[] array = new byte[num6];
			memStream.Read(array, 0, (int)num6);
			list.Add(array);
			num5++;
		}
		if (memStream.Position != memStream.Length)
		{
			Log.Out("Wrong memStream Position after creating arrays: pos={0}, len={1}", new object[] { memStream.Position, memStream.Length });
		}
		Log.Out("World chunks size: {0} B, chunk count: {1}", new object[] { memStream.Length, list.Count });
		zipStream.Dispose();
		object lockObj = NetPackageWorldFolder.LockObj;
		lock (lockObj)
		{
			NetPackageWorldFolder.CachedWorldName = GamePrefs.GetString(EnumGamePrefs.GameWorld);
			NetPackageWorldFolder.CompressedWorldDataChunks = list;
			NetPackageWorldFolder.PreparationCoroutine = null;
			yield break;
		}
		yield break;
	}

	public static IEnumerator writeDtmDelta(Stream _sourceStream, Stream _targetStream, MicroStopwatch _mswCompressing)
	{
		int w = (int)Math.Sqrt((double)(_sourceStream.Length / 2L));
		int h = w;
		int lineBytes = w * 2;
		byte[] readLineData = new byte[lineBytes];
		byte[] writeLineData = new byte[lineBytes];
		MemoryStream readStream = new MemoryStream(readLineData);
		MemoryStream writeStream = new MemoryStream(writeLineData);
		int num4;
		for (int z = 0; z < h; z = num4 + 1)
		{
			readStream.Position = 0L;
			writeStream.Position = 0L;
			_sourceStream.Read(readLineData, 0, lineBytes);
			int num = (int)StreamUtils.ReadUInt16(readStream);
			StreamUtils.Write(writeStream, (ushort)num);
			for (int i = 1; i < w; i++)
			{
				ushort num2 = StreamUtils.ReadUInt16(readStream);
				int num3 = (int)num2 - num;
				num = (int)num2;
				if (num3 < -32768 || num3 > 32767)
				{
					Log.Out("Delta out of range: " + num3.ToString());
				}
				StreamUtils.Write(writeStream, (short)num3);
			}
			_targetStream.Write(writeLineData, 0, lineBytes);
			if (_mswCompressing != null && _mswCompressing.ElapsedMilliseconds > (long)NetPackageWorldFolder.MaxTimePerFrame)
			{
				yield return null;
				_mswCompressing.ResetAndRestart();
			}
			num4 = z;
		}
		yield break;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static IEnumerator sendPacketsToClient(ClientInfo _cInfo)
	{
		while (NetPackageWorldFolder.CompressedWorldDataChunks == null)
		{
			yield return null;
		}
		string cInfoString = _cInfo.ToString();
		Log.Out("Starting to send world to " + cInfoString + "...");
		int num;
		for (int i = 0; i < NetPackageWorldFolder.CompressedWorldDataChunks.Count; i = num + 1)
		{
			_cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageWorldFolder>().Setup(NetPackageWorldFolder.CompressedWorldDataChunks[i], i, NetPackageWorldFolder.CompressedWorldDataChunks.Count));
			yield return NetPackageWorldFolder.PACKET_SEND_DELAY;
			num = i;
		}
		Log.Out("Sending world to " + cInfoString + " done");
		yield break;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public byte[] data;

	[PublicizedFrom(EAccessModifier.Private)]
	public int seqNr;

	[PublicizedFrom(EAccessModifier.Private)]
	public int totalParts;

	[PublicizedFrom(EAccessModifier.Private)]
	public static DateTime downloadStartTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public static MemoryStream ReceiveStream;

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool WorldReceivedAndUncompressed;

	[PublicizedFrom(EAccessModifier.Private)]
	public static WaitForSeconds PACKET_SEND_DELAY = new WaitForSeconds(0.25f);

	[PublicizedFrom(EAccessModifier.Private)]
	public const int CHUNK_SIZE = 65536;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int DTM_BITMASK = 65520;

	[PublicizedFrom(EAccessModifier.Private)]
	public static List<byte[]> CompressedWorldDataChunks;

	[PublicizedFrom(EAccessModifier.Private)]
	public static string CachedWorldName;

	[PublicizedFrom(EAccessModifier.Private)]
	public static object LockObj = new object();

	[PublicizedFrom(EAccessModifier.Private)]
	public static IEnumerator PreparationCoroutine;
}
