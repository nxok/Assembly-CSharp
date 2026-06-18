using System;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageEntitySetSkillLevelServer : NetPackageEntitySetSkillLevelClient
{
	public override NetPackageDirection PackageDirection
	{
		get
		{
			return NetPackageDirection.ToServer;
		}
	}

	public new NetPackageEntitySetSkillLevelServer Setup(int _entityId, string skill, int _experience)
	{
		base.Setup(_entityId, skill, _experience);
		return this;
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world == null)
		{
			return;
		}
		EntityPlayer entityPlayer = (EntityPlayer)_world.GetEntity(this.entityId);
		if (entityPlayer == null)
		{
			return;
		}
		if (entityPlayer.isEntityRemote)
		{
			entityPlayer.Progression.GetProgressionValue(this.skill).Level = this.level;
		}
	}
}
