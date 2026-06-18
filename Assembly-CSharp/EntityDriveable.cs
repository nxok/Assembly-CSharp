using System;
using UnityEngine.Scripting;

[Preserve]
public class EntityDriveable : EntityVehicle
{
	public override void Init(int _entityClass, EntityInstanceAssets _assets, EModelInstanceAssets _eModelAssets)
	{
		base.Init(_entityClass, _assets, _eModelAssets);
		if (this.nativeCollider != null)
		{
			this.nativeCollider.enabled = true;
		}
	}
}
