using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ItemActionSpawnEntity : ItemAction
{
	public override ItemActionData CreateModifierData(ItemInventoryData _invData, int _indexInEntityOfAction)
	{
		return new ItemActionSpawnEntity.ItemActionDataSpawnEntity(_invData, _indexInEntityOfAction);
	}

	public override void ReadFrom(DynamicProperties _props)
	{
		base.ReadFrom(_props);
		this.animType = _props.GetInt("AnimType");
		this.animWait = _props.GetFloat("AnimWait");
		this.soundWarn = _props.GetString("SoundWarn");
		this.soundAttack = _props.GetString("SoundAttack");
		this.entityToSpawn = _props.GetString("Entity");
		_props.ParseVec("EntityOffset", ref this.entityOffset);
	}

	public override void StartHolding(ItemActionData _actionData)
	{
	}

	public override void StopHolding(ItemActionData _actionData)
	{
	}

	public override void OnHoldingUpdate(ItemActionData _actionData)
	{
		ItemActionSpawnEntity.ItemActionDataSpawnEntity itemActionDataSpawnEntity = (ItemActionSpawnEntity.ItemActionDataSpawnEntity)_actionData;
		itemActionDataSpawnEntity.stateTime += 0.05f;
		ItemActionSpawnEntity.ItemActionDataSpawnEntity.State state = itemActionDataSpawnEntity.state;
		if (state != ItemActionSpawnEntity.ItemActionDataSpawnEntity.State.Anim)
		{
			if (state != ItemActionSpawnEntity.ItemActionDataSpawnEntity.State.Spawn)
			{
				return;
			}
			this.Spawn(itemActionDataSpawnEntity);
			itemActionDataSpawnEntity.state = ItemActionSpawnEntity.ItemActionDataSpawnEntity.State.End;
			return;
		}
		else
		{
			if (itemActionDataSpawnEntity.stateTime < this.animWait)
			{
				return;
			}
			itemActionDataSpawnEntity.state = ItemActionSpawnEntity.ItemActionDataSpawnEntity.State.Spawn;
			return;
		}
	}

	public override void CancelAction(ItemActionData _actionData)
	{
		((ItemActionSpawnEntity.ItemActionDataSpawnEntity)_actionData).state = ItemActionSpawnEntity.ItemActionDataSpawnEntity.State.None;
	}

	public override void ExecuteAction(ItemActionData _actionData, bool _bReleased)
	{
		EntityAlive holdingEntity = _actionData.invData.holdingEntity;
		if (!holdingEntity)
		{
			return;
		}
		ItemActionSpawnEntity.ItemActionDataSpawnEntity itemActionDataSpawnEntity = (ItemActionSpawnEntity.ItemActionDataSpawnEntity)_actionData;
		if (!_bReleased)
		{
			if (itemActionDataSpawnEntity.state == ItemActionSpawnEntity.ItemActionDataSpawnEntity.State.None)
			{
				itemActionDataSpawnEntity.state = ItemActionSpawnEntity.ItemActionDataSpawnEntity.State.Anim;
				itemActionDataSpawnEntity.stateTime = 0f;
				holdingEntity.StartAnimAction(this.animType + 3000);
				holdingEntity.PlayOneShot(this.soundWarn, false, false, false, null, 1f);
			}
			return;
		}
		itemActionDataSpawnEntity.state = ItemActionSpawnEntity.ItemActionDataSpawnEntity.State.None;
	}

	public override bool IsActionRunning(ItemActionData _actionData)
	{
		return ((ItemActionSpawnEntity.ItemActionDataSpawnEntity)_actionData).state > ItemActionSpawnEntity.ItemActionDataSpawnEntity.State.None;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Spawn(ItemActionData _actionData)
	{
		EntityAlive holdingEntity = _actionData.invData.holdingEntity;
		if (!holdingEntity)
		{
			return;
		}
		if (holdingEntity.IsAttackValid())
		{
			Vector3 vector = holdingEntity.getHeadPosition();
			vector += holdingEntity.qrotation * this.entityOffset;
			Entity entity = EntityFactory.CreateEntity(EntityClass.GetId(this.entityToSpawn), vector, new Vector3(0f, holdingEntity.rotation.y, 0f));
			entity.SetSpawnerSource(EnumSpawnerSource.StaticSpawner);
			GameManager.Instance.World.SpawnEntityInWorld(entity);
			EntityAlive entityAlive = entity as EntityAlive;
			if (entityAlive != null)
			{
				entityAlive.SetAttackTarget(holdingEntity.GetAttackTarget(), 600);
			}
			holdingEntity.PlayOneShot(this.soundAttack, false, false, false, null, 1f);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int animType;

	[PublicizedFrom(EAccessModifier.Private)]
	public float animWait;

	[PublicizedFrom(EAccessModifier.Private)]
	public string soundWarn;

	[PublicizedFrom(EAccessModifier.Private)]
	public string soundAttack;

	[PublicizedFrom(EAccessModifier.Private)]
	public string entityToSpawn;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 entityOffset;

	[PublicizedFrom(EAccessModifier.Protected)]
	public class ItemActionDataSpawnEntity : ItemActionAttackData
	{
		public ItemActionDataSpawnEntity(ItemInventoryData _invData, int _indexInEntityOfAction)
			: base(_invData, _indexInEntityOfAction)
		{
		}

		public ItemActionSpawnEntity.ItemActionDataSpawnEntity.State state;

		public float stateTime;

		public enum State
		{
			None,
			Anim,
			Spawn,
			End
		}
	}
}
