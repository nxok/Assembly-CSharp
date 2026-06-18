using System;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageEntityAddScoreServer : NetPackageEntityAddScoreClient
{
	public override NetPackageDirection PackageDirection
	{
		get
		{
			return NetPackageDirection.ToServer;
		}
	}

	public new NetPackageEntityAddScoreServer Setup(int _entityId, int _zombieKills, int _playerKills, int _otherTeamNumber, int _conditions)
	{
		base.Setup(_entityId, _zombieKills, _playerKills, _otherTeamNumber, _conditions);
		return this;
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world == null)
		{
			return;
		}
		_world.gameManager.AddScoreServer(this.entityId, this.zombieKills, this.playerKills, this.otherTeamNumber, this.conditions);
	}
}
