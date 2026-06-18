using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class EntityZombieDog : EntityEnemyAnimal
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public override void Awake()
	{
		base.Awake();
		Transform transform = base.transform.Find("Graphics/BlobShadowProjector");
		if (transform)
		{
			transform.gameObject.SetActive(false);
		}
	}

	public override void Init(int _entityClass, EntityInstanceAssets _assets, EModelInstanceAssets _eModelAssets)
	{
		base.Init(_entityClass, _assets, _eModelAssets);
		this.timeToDie = this.world.worldTime + 1800UL + (ulong)(22000f * this.rand.RandomFloat);
	}

	public override void OnUpdateLive()
	{
		base.OnUpdateLive();
		if (this.world.worldTime >= this.timeToDie && !this.isEntityRemote)
		{
			this.Kill(DamageResponse.New(true));
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool isDetailedHeadBodyColliders()
	{
		return true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override int GetMaxAttackTime()
	{
		return 30;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnEntityTargeted(EntityAlive target)
	{
		base.OnEntityTargeted(target);
	}

	public ulong timeToDie;
}
