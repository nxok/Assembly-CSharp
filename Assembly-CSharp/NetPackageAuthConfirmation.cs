using System;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageAuthConfirmation : NetPackage
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

	public NetPackageAuthConfirmation Setup()
	{
		return this;
	}

	public override void read(PooledBinaryReader _reader)
	{
	}

	public override void write(PooledBinaryWriter _writer)
	{
		base.write(_writer);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			AuthFinalizer.Instance.ReplyReceived(base.Sender);
			return;
		}
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsConnected)
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageAuthConfirmation>().Setup(), false);
		}
	}

	public override int GetLength()
	{
		return 9;
	}
}
