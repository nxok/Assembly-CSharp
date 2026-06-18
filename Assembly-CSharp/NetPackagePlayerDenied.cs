using System;
using UnityEngine.Scripting;

[Preserve]
public class NetPackagePlayerDenied : NetPackage
{
	public override bool FlushQueue
	{
		get
		{
			return true;
		}
	}

	public override bool AllowedBeforeAuth
	{
		get
		{
			return true;
		}
	}

	public NetPackagePlayerDenied Setup(GameUtils.KickPlayerData _kickData)
	{
		this.kickData = _kickData;
		return this;
	}

	public override void read(PooledBinaryReader _reader)
	{
		this.kickData.reason = (GameUtils.EKickReason)_reader.ReadInt32();
		this.kickData.apiResponseEnum = _reader.ReadInt32();
		this.kickData.banUntil = DateTime.FromBinary(_reader.ReadInt64());
		this.kickData.customReason = _reader.ReadString();
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsClient)
		{
			ThreadManager.AddSingleTaskMainThread("PlayerDenied.ProcessPackage", delegate(object _taskInfo)
			{
				this.ProcessPackage(GameManager.Instance.World, GameManager.Instance);
			}, null);
		}
	}

	public override void write(PooledBinaryWriter _writer)
	{
		base.write(_writer);
		_writer.Write((int)this.kickData.reason);
		_writer.Write(this.kickData.apiResponseEnum);
		_writer.Write(this.kickData.banUntil.ToBinary());
		_writer.Write(this.kickData.customReason);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (!this.processed)
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.Disconnect();
			_callbacks.ShowMessagePlayerDenied(this.kickData);
			this.processed = true;
		}
	}

	public override int GetLength()
	{
		return 20;
	}

	public override NetPackageDirection PackageDirection
	{
		get
		{
			return NetPackageDirection.ToClient;
		}
	}

	public GameUtils.KickPlayerData kickData;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool processed;
}
