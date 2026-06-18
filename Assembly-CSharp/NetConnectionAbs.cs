using System;
using System.Collections.Generic;
using System.IO;
using Noemax.GZip;
using UnityEngine.Profiling;

public abstract class NetConnectionAbs : INetConnection
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public NetConnectionAbs(int _channel, ClientInfo _clientInfo, INetworkClient _netClient, string _uniqueId)
	{
		this.channel = _channel;
		this.cInfo = _clientInfo;
		this.netClient = _netClient;
		this.isServer = _clientInfo != null;
		NetConnectionAbs.encryptedStreamReceived = false;
		this.connectionIdentifier = (this.isServer ? (_uniqueId + "_" + _channel.ToString()) : _channel.ToString());
		NetPackageLogger.BeginLog(this.isServer);
	}

	public void SetEncryptionModule(IEncryptionModule _module)
	{
		this.encryptionModule = _module;
	}

	public virtual void Disconnect(bool _kick)
	{
		if (!this.bDisconnected)
		{
			NetPackageLogger.EndLog();
		}
		this.bDisconnected = true;
		if (_kick && this.cInfo != null)
		{
			this.cInfo.disconnecting = true;
			SingletonMonoBehaviour<ConnectionManager>.Instance.DisconnectClient(this.cInfo, false, false);
		}
	}

	public virtual bool IsDisconnected()
	{
		return this.bDisconnected;
	}

	public virtual void GetPackages(List<NetPackage> _dstBuf)
	{
		_dstBuf.Clear();
		if (this.receivedPackages.Count == 0)
		{
			return;
		}
		List<NetPackage> list = this.receivedPackages;
		lock (list)
		{
			_dstBuf.AddRange(this.receivedPackages);
			this.receivedPackages.Clear();
		}
	}

	public virtual void AddToSendQueue(List<NetPackage> _packages)
	{
		for (int i = 0; i < _packages.Count; i++)
		{
			this.AddToSendQueue(_packages[i]);
		}
	}

	public virtual void UpgradeToFullConnection()
	{
		this.InitStreams(true);
		this.allowCompression = true;
	}

	public virtual NetConnectionStatistics GetStats()
	{
		return this.stats;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual bool Compress(bool _compress, MemoryStream _uncompressedSourceStream, DeflateOutputStream _zipTargetStream, MemoryStream _compressedTargetStream, byte[] _copyBuffer, int _packageCount)
	{
		if (_compress)
		{
			_compressedTargetStream.SetLength(0L);
			try
			{
				StreamUtils.StreamCopy(_uncompressedSourceStream, _zipTargetStream, _copyBuffer, true);
			}
			catch (Exception)
			{
				Log.Error(string.Concat(new string[]
				{
					"Compressed buffer size too small: Source stream size (",
					_uncompressedSourceStream.Length.ToString(),
					") > compressed stream capacity (",
					_compressedTargetStream.Capacity.ToString(),
					"), packages: ",
					_packageCount.ToString()
				}));
				throw;
			}
			_zipTargetStream.Restart();
			_compressedTargetStream.Position = 0L;
		}
		return _compress;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual bool Decompress(bool _compressed, MemoryStream _uncompressedTargetStream, DeflateInputStream _unzipSourceStream, byte[] _copyBuffer)
	{
		if (_compressed)
		{
			_uncompressedTargetStream.SetLength(0L);
			_unzipSourceStream.Restart();
			try
			{
				StreamUtils.StreamCopy(_unzipSourceStream, _uncompressedTargetStream, _copyBuffer, true);
			}
			catch (Exception ex)
			{
				Log.Exception(ex);
				throw;
			}
			_uncompressedTargetStream.Position = 0L;
		}
		return _compressed;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual bool EnableEncryptData()
	{
		return this.encryptionModule != null && (this.isServer || NetConnectionAbs.encryptedStreamReceived);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual bool ExpectEncryptedData()
	{
		return this.encryptionModule != null && this.isServer && this.cInfo.loginDone;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual bool Encrypt(MemoryStream _stream)
	{
		if (this.EnableEncryptData())
		{
			bool flag = this.encryptionModule.EncryptStream(this.cInfo, _stream);
			_stream.Position = 0L;
			return flag;
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual bool Decrypt(bool _bEncrypted, MemoryStream _stream)
	{
		if (_bEncrypted)
		{
			NetConnectionAbs.encryptedStreamReceived = true;
			bool flag = this.encryptionModule.DecryptStream(this.cInfo, _stream);
			_stream.Position = 0L;
			if (!flag)
			{
				if (this.isServer)
				{
					GameUtils.KickPlayerForClientInfo(this.cInfo, new GameUtils.KickPlayerData(GameUtils.EKickReason.EncryptionFailure, 0, default(DateTime), ""));
					return flag;
				}
				GameUtils.ForceDisconnect(new GameUtils.KickPlayerData(GameUtils.EKickReason.EncryptionFailure, 0, default(DateTime), ""));
			}
			return flag;
		}
		if (!this.ExpectEncryptedData())
		{
			return true;
		}
		Log.Error(string.Format("[NET] Client logged in but sent unencrypted message, dropping! {0}", this.cInfo));
		this.cInfo.loginDone = false;
		GameUtils.KickPlayerForClientInfo(this.cInfo, new GameUtils.KickPlayerData(GameUtils.EKickReason.EncryptionFailure, 0, default(DateTime), ""));
		return false;
	}

	public virtual void FlushSendQueue()
	{
	}

	public abstract void AddToSendQueue(NetPackage _package);

	[PublicizedFrom(EAccessModifier.Protected)]
	public abstract void InitStreams(bool _full);

	public abstract void AppendToReaderStream(byte[] _data, int _size);

	public const int PROCESSING_BUFFER_SIZE = 2097152;

	public const int COMPRESSED_BUFFER_SIZE = 2097152;

	[PublicizedFrom(EAccessModifier.Protected)]
	public const int PREAUTH_BUFFER_SIZE = 32768;

	[PublicizedFrom(EAccessModifier.Protected)]
	public readonly int channel;

	[PublicizedFrom(EAccessModifier.Protected)]
	public readonly ClientInfo cInfo;

	[PublicizedFrom(EAccessModifier.Protected)]
	public readonly INetworkClient netClient;

	[PublicizedFrom(EAccessModifier.Protected)]
	public readonly bool isServer;

	[PublicizedFrom(EAccessModifier.Private)]
	public IEncryptionModule encryptionModule;

	[PublicizedFrom(EAccessModifier.Protected)]
	public readonly string connectionIdentifier;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool fullConnection;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool allowCompression;

	[PublicizedFrom(EAccessModifier.Protected)]
	public static bool encryptedStreamReceived;

	[PublicizedFrom(EAccessModifier.Protected)]
	public volatile bool bDisconnected;

	[PublicizedFrom(EAccessModifier.Protected)]
	public readonly NetConnectionStatistics stats = new NetConnectionStatistics();

	[PublicizedFrom(EAccessModifier.Protected)]
	public readonly List<NetPackage> receivedPackages = new List<NetPackage>();

	[PublicizedFrom(EAccessModifier.Private)]
	public CustomSampler threadSamplerEncrypt = CustomSampler.Create("Encrypt", false);

	[PublicizedFrom(EAccessModifier.Private)]
	public CustomSampler threadSamplerDecrypt = CustomSampler.Create("Decrypt", false);
}
