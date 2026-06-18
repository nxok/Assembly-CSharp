using System;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageEAC : NetPackage
{
	public override bool AllowedBeforeAuth
	{
		get
		{
			return true;
		}
	}

	public NetPackageEAC Setup(int _size, byte[] _data)
	{
		this.data = new byte[_size];
		Array.Copy(_data, this.data, _size);
		return this;
	}

	public override void read(PooledBinaryReader _reader)
	{
		int num = _reader.ReadInt32();
		this.data = new byte[num];
		for (int i = 0; i < this.data.Length; i++)
		{
			this.data[i] = _reader.ReadByte();
		}
	}

	public override void write(PooledBinaryWriter _writer)
	{
		base.write(_writer);
		_writer.Write(this.data.Length);
		for (int i = 0; i < this.data.Length; i++)
		{
			_writer.Write(this.data[i]);
		}
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			IAntiCheatServer antiCheatServer = PlatformManager.MultiPlatform.AntiCheatServer;
			if (antiCheatServer == null)
			{
				return;
			}
			antiCheatServer.HandleMessageFromClient(base.Sender, this.data);
			return;
		}
		else
		{
			IAntiCheatClient antiCheatClient = PlatformManager.MultiPlatform.AntiCheatClient;
			if (antiCheatClient == null)
			{
				return;
			}
			antiCheatClient.HandleMessageFromServer(this.data);
			return;
		}
	}

	public override int GetLength()
	{
		return 4 + ((this.data != null) ? this.data.Length : 0);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public byte[] data;
}
