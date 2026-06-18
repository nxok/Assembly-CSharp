using System;
using UnityEngine;

public class BlockProjectileMoveScript : ProjectileMoveScript
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public override void checkCollision()
	{
		GameManager instance = GameManager.Instance;
		if (!instance)
		{
			return;
		}
		if (instance.World == null)
		{
			return;
		}
		Vector3 vector;
		if (this.isOnIdealPos)
		{
			vector = base.transform.position;
		}
		else
		{
			vector = this.idealPosition;
		}
		Vector3 vector2 = vector - this.previousPosition;
		float magnitude = vector2.magnitude;
		if (magnitude < 0.04f)
		{
			return;
		}
		Ray ray;
		ray..ctor(this.previousPosition, vector2);
		this.previousPosition = vector;
		int num = 0;
		EntityAlive entityAlive = (EntityAlive)this.firingEntity;
		if (entityAlive != null && entityAlive.emodel != null)
		{
			num = entityAlive.GetModelLayer();
			entityAlive.SetModelLayer(2, false, null);
		}
		this.hitMask = 32;
		bool flag = Voxel.Raycast(instance.World, ray, magnitude, -538750989, this.hitMask, 0f);
		if (entityAlive != null && entityAlive.emodel != null)
		{
			entityAlive.SetModelLayer(num, false, null);
		}
		if (flag && (GameUtils.IsBlockOrTerrain(Voxel.voxelRayHitInfo.tag) || Voxel.voxelRayHitInfo.tag.StartsWith("E_")))
		{
			base.enabled = false;
			Object.Destroy(base.transform.gameObject);
			Transform transform = Voxel.voxelRayHitInfo.transform;
			string text = null;
			if (Voxel.voxelRayHitInfo.tag.StartsWith("E_BP_"))
			{
				text = Voxel.voxelRayHitInfo.tag.Substring("E_BP_".Length).ToLower();
				transform = GameUtils.GetHitRootTransform(Voxel.voxelRayHitInfo.tag, Voxel.voxelRayHitInfo.transform);
			}
			if (Voxel.voxelRayHitInfo.tag.StartsWith("E_"))
			{
				Entity component = transform.GetComponent<Entity>();
				if (component == null)
				{
					return;
				}
				DamageSourceEntity damageSourceEntity = new DamageSourceEntity(EnumDamageSource.External, EnumDamageTypes.Piercing, this.ProjectileOwnerID)
				{
					AttackingItem = this.itemValueProjectile
				};
				int num2 = (int)this.GetProjectileDamageEntity();
				bool flag2 = component.IsDead();
				component.DamageEntity(damageSourceEntity, num2, false, 1f);
				if (this.itemActionProjectile.BuffActions != null)
				{
					EntityAlive entityAlive2 = component as EntityAlive;
					if (entityAlive2 != null)
					{
						string text2 = ((text != null) ? GameUtils.GetChildTransformPath(entityAlive2.transform, Voxel.voxelRayHitInfo.transform) : null);
						ItemAction.ExecuteBuffActions(this.itemActionProjectile.BuffActions, -1, entityAlive2, false, damageSourceEntity.GetEntityDamageBodyPart(entityAlive2), text2);
					}
				}
				if (!flag2 && component.IsDead())
				{
					EntityPlayer entityPlayer = instance.World.GetEntity(this.ProjectileOwnerID) as EntityPlayer;
					if (entityPlayer != null && EntityClass.list.ContainsKey(component.entityClass))
					{
						float value = EffectManager.GetValue(PassiveEffects.ElectricalTrapXP, entityPlayer.inventory.holdingItemItemValue, 0f, entityPlayer, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false);
						if (value > 0f)
						{
							entityPlayer.AddKillXP(component as EntityAlive, this.itemValueProjectile, value);
						}
					}
				}
			}
			if (this.itemActionProjectile.Explosion.ParticleIndex > 0)
			{
				Vector3 vector3 = Voxel.voxelRayHitInfo.hit.pos - vector2.normalized * 0.1f;
				Vector3i vector3i = World.worldToBlockPos(vector3);
				if (!instance.World.GetBlock(vector3i).isair)
				{
					BlockFace blockFace;
					vector3i = Voxel.OneVoxelStep(vector3i, vector3, -vector2.normalized, out vector3, out blockFace);
				}
				instance.ExplosionServer(vector3, vector3i, Quaternion.identity, this.itemActionProjectile.Explosion, this.ProjectileOwnerID, 0f, false, this.itemValueProjectile);
			}
		}
	}

	public float GetProjectileDamageEntity()
	{
		return this.itemActionProjectile.GetDamageEntity(this.itemValueProjectile, null, 0);
	}
}
