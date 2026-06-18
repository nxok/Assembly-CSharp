using System;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageShowToolbeltMessage : NetPackage
{
	public NetPackageShowToolbeltMessage Setup(string _toolbeltMessage, string _sound)
	{
		this.toolbeltMessage = _toolbeltMessage;
		this.sound = _sound;
		return this;
	}

	public override void read(PooledBinaryReader _br)
	{
		this.toolbeltMessage = _br.ReadString();
		this.sound = _br.ReadString();
	}

	public override void write(PooledBinaryWriter _bw)
	{
		base.write(_bw);
		_bw.Write(this.toolbeltMessage);
		_bw.Write(this.sound);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world == null)
		{
			return;
		}
		GameManager.ShowTooltip(_world.GetLocalPlayers()[0], this.toolbeltMessage, this.sound, null, null, false, false, 0f);
	}

	public override NetPackageDirection PackageDirection
	{
		get
		{
			return NetPackageDirection.ToClient;
		}
	}

	public override int GetLength()
	{
		return 80;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string toolbeltMessage;

	[PublicizedFrom(EAccessModifier.Private)]
	public string sound = "";
}
