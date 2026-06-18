using System;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageConsoleCmdServer : NetPackage
{
	public NetPackageConsoleCmdServer Setup(string _cmd)
	{
		this.cmd = _cmd;
		return this;
	}

	public override void read(PooledBinaryReader _br)
	{
		this.cmd = _br.ReadString();
	}

	public override void write(PooledBinaryWriter _bw)
	{
		base.write(_bw);
		_bw.Write(this.cmd);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world == null)
		{
			return;
		}
		SingletonMonoBehaviour<ConnectionManager>.Instance.ServerConsoleCommand(base.Sender, this.cmd);
	}

	public override NetPackageDirection PackageDirection
	{
		get
		{
			return NetPackageDirection.ToServer;
		}
	}

	public override int GetLength()
	{
		return 30;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string cmd;
}
