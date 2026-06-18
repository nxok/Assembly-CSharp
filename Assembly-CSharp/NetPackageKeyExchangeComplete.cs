using System;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageKeyExchangeComplete : NetPackage
{
	public override bool AllowedBeforeAuth
	{
		get
		{
			return true;
		}
	}

	public NetPackageKeyExchangeComplete Setup(bool _wasSuccessful)
	{
		this.wasSuccessful = _wasSuccessful;
		return this;
	}

	public override void read(PooledBinaryReader _br)
	{
		this.wasSuccessful = _br.ReadBoolean();
	}

	public override void write(PooledBinaryWriter _bw)
	{
		base.write(_bw);
		_bw.Write(this.wasSuccessful);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		SingletonMonoBehaviour<ConnectionManager>.Instance.AntiCheatEncryptionAuthServer.CompleteKeyExchange(base.Sender, this.wasSuccessful);
	}

	public override int GetLength()
	{
		return 1;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool wasSuccessful;
}
