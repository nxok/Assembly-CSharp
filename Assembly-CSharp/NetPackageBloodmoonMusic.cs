using System;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageBloodmoonMusic : NetPackage
{
	public override NetPackageDirection PackageDirection
	{
		get
		{
			return NetPackageDirection.ToClient;
		}
	}

	public NetPackageBloodmoonMusic Setup(bool _isBloodmoonMusicEligible)
	{
		this.IsBloodMoonMusicEligible = _isBloodmoonMusicEligible;
		return this;
	}

	public override void read(PooledBinaryReader _reader)
	{
		this.IsBloodMoonMusicEligible = _reader.ReadBoolean();
	}

	public override void write(PooledBinaryWriter _writer)
	{
		base.write(_writer);
		_writer.Write(this.IsBloodMoonMusicEligible);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (GameManager.Instance.World != null && GameManager.Instance.World.dmsConductor != null)
		{
			GameManager.Instance.World.dmsConductor.IsBloodmoonMusicEligible = this.IsBloodMoonMusicEligible;
		}
	}

	public override int GetLength()
	{
		return 1;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool IsBloodMoonMusicEligible;
}
