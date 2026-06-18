using System;
using System.Collections.Generic;
using Audio;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ItemActionLauncher : ItemActionRanged
{
	public override ItemActionData CreateModifierData(ItemInventoryData _invData, int _indexInEntityOfAction)
	{
		return new ItemActionLauncher.ItemActionDataLauncher(_invData, _indexInEntityOfAction);
	}

	public override void ReadFrom(DynamicProperties _props)
	{
		base.ReadFrom(_props);
	}

	public override void StartHolding(ItemActionData _actionData)
	{
		base.StartHolding(_actionData);
		this.DeleteProjectiles(_actionData);
		ItemActionLauncher.ItemActionDataLauncher itemActionDataLauncher = (ItemActionLauncher.ItemActionDataLauncher)_actionData;
		if (_actionData.invData.itemValue.Meta != 0 && this.GetMaxAmmoCount(itemActionDataLauncher) != 0)
		{
			for (int i = 0; i < _actionData.invData.itemValue.Meta; i++)
			{
				itemActionDataLauncher.projectileTs.Add(this.instantiateProjectile(_actionData, default(Vector3)));
			}
		}
	}

	public override void StopHolding(ItemActionData _data)
	{
		base.StopHolding(_data);
		this.DeleteProjectiles(_data);
	}

	public void DeleteProjectiles(ItemActionData _actionData)
	{
		ItemActionLauncher.ItemActionDataLauncher itemActionDataLauncher = (ItemActionLauncher.ItemActionDataLauncher)_actionData;
		for (int i = 0; i < itemActionDataLauncher.projectileTs.Count; i++)
		{
			Transform transform = itemActionDataLauncher.projectileTs[i];
			if (transform)
			{
				Object.Destroy(transform.gameObject);
			}
		}
		itemActionDataLauncher.projectileTs.Clear();
	}

	public override void ReloadGun(ItemActionData _actionData)
	{
		ItemActionLauncher.ItemActionDataLauncher itemActionDataLauncher = (ItemActionLauncher.ItemActionDataLauncher)_actionData;
		if (itemActionDataLauncher != null)
		{
			itemActionDataLauncher.isReloadRequested = false;
			Manager.StopSequence(_actionData.invData.holdingEntity, ((ItemActionRanged.ItemActionDataRanged)_actionData).SoundStart);
			if (!_actionData.invData.holdingEntity.isEntityRemote)
			{
				_actionData.invData.holdingEntity.OnReloadStart();
			}
		}
	}

	public override void CancelReload(ItemActionData _actionData, bool holsterWeapon)
	{
		base.CancelReload(_actionData, holsterWeapon);
		ItemActionLauncher.ItemActionDataLauncher itemActionDataLauncher = (ItemActionLauncher.ItemActionDataLauncher)_actionData;
		this.ClampAmmoCount(itemActionDataLauncher);
	}

	public override void SwapAmmoType(EntityAlive _entity, int _selectedIndex = -1)
	{
		ItemActionLauncher.ItemActionDataLauncher itemActionDataLauncher = (ItemActionLauncher.ItemActionDataLauncher)_entity.inventory.holdingItemData.actionData[0];
		this.ClampAmmoCount(itemActionDataLauncher);
		base.SwapAmmoType(_entity, _selectedIndex);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ClampAmmoCount(ItemActionLauncher.ItemActionDataLauncher actionData)
	{
		int maxAmmoCount = this.GetMaxAmmoCount(actionData);
		int num = actionData.projectileTs.Count - maxAmmoCount;
		if (num > 0)
		{
			for (int i = maxAmmoCount; i < actionData.projectileTs.Count; i++)
			{
				if (actionData.projectileTs[i] != null)
				{
					Object.Destroy(actionData.projectileTs[i].gameObject);
				}
			}
			actionData.projectileTs.RemoveRange(maxAmmoCount, num);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override Vector3 fireShot(int _shotIdx, ItemActionRanged.ItemActionDataRanged _actionData, ref bool hitEntity)
	{
		hitEntity = true;
		return Vector3.zero;
	}

	public Transform instantiateProjectile(ItemActionData _actionData, Vector3 _positionOffset = default(Vector3))
	{
		EntityAlive holdingEntity = _actionData.invData.holdingEntity;
		ItemValue holdingItemItemValue = holdingEntity.inventory.holdingItemItemValue;
		ItemValue item = ItemClass.GetItem(this.MagazineItemNames[(int)holdingItemItemValue.SelectedAmmoTypeIndex], false);
		this.LastProjectileType = item;
		ItemClass forId = ItemClass.GetForId(item.type);
		if (forId == null)
		{
			return null;
		}
		ItemActionLauncher.ItemActionDataLauncher itemActionDataLauncher = (ItemActionLauncher.ItemActionDataLauncher)_actionData;
		itemActionDataLauncher.lastAttackStrainPercent = itemActionDataLauncher.strainPercent;
		ItemValue itemValue = new ItemValue(forId.Id, false);
		Transform transform = forId.CloneModel(_actionData.invData.world, itemValue, Vector3.zero, null, BlockShape.MeshPurpose.World, default(TextureFullArray));
		Transform transform2 = itemActionDataLauncher.projectileJointT;
		if (!transform2)
		{
			transform2 = holdingEntity.emodel.GetRightHandTransform();
		}
		if (transform2)
		{
			transform.SetParent(transform2, false);
			transform.SetLocalPositionAndRotation(_positionOffset, Quaternion.identity);
			transform.localScale = Vector3.one;
		}
		else
		{
			transform.parent = null;
			transform.localScale = Vector3.one;
		}
		Utils.SetLayerRecursively(transform.gameObject, transform2 ? transform2.gameObject.layer : 0);
		ProjectileMoveScript projectileMoveScript = transform.gameObject.AddComponent<ProjectileMoveScript>();
		projectileMoveScript.itemProjectile = forId;
		projectileMoveScript.itemValueProjectile = itemValue;
		projectileMoveScript.itemValueLauncher = holdingItemItemValue;
		projectileMoveScript.itemActionProjectile = (ItemActionProjectile)((forId.Actions[0] is ItemActionProjectile) ? forId.Actions[0] : forId.Actions[1]);
		projectileMoveScript.ProjectileOwnerID = holdingEntity.entityId;
		projectileMoveScript.actionData = itemActionDataLauncher;
		transform.gameObject.SetActive(true);
		return transform;
	}

	public override void ItemActionEffects(GameManager _gameManager, ItemActionData _actionData, int _firingState, Vector3 _startPos, Vector3 _direction, int _userData = 0)
	{
		base.ItemActionEffects(_gameManager, _actionData, _firingState, _startPos, _direction, _userData);
		if (_firingState == 0)
		{
			return;
		}
		ItemActionLauncher.ItemActionDataLauncher itemActionDataLauncher = (ItemActionLauncher.ItemActionDataLauncher)_actionData;
		int num = this.GetBurstCount(_actionData);
		if (num <= 0)
		{
			return;
		}
		for (int i = itemActionDataLauncher.projectileTs.Count - 1; i >= 0; i--)
		{
			Transform transform = itemActionDataLauncher.projectileTs[i];
			if (transform)
			{
				transform.GetComponent<ProjectileMoveScript>().Fire(_startPos, this.getDirectionOffset(itemActionDataLauncher, _direction, i), _actionData.invData.holdingEntity, this.hitmaskOverride, 0f, false);
			}
			itemActionDataLauncher.projectileTs.RemoveAt(i);
			if (--num <= 0)
			{
				break;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void ConsumeAmmo(ItemActionData _actionData)
	{
		_actionData.invData.itemValue.Meta -= this.GetBurstCount(_actionData);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override int GetActionEffectsValues(ItemActionData _actionData, out Vector3 _startPos, out Vector3 _direction)
	{
		ItemActionLauncher.ItemActionDataLauncher itemActionDataLauncher = (ItemActionLauncher.ItemActionDataLauncher)_actionData;
		Ray lookRay = itemActionDataLauncher.invData.holdingEntity.GetLookRay();
		_startPos = lookRay.origin;
		_direction = lookRay.direction;
		_direction = this.getDirectionOffset(itemActionDataLauncher, _direction, 0);
		return 0;
	}

	public ItemValue LastProjectileType;

	public class ItemActionDataLauncher : ItemActionRanged.ItemActionDataRanged
	{
		public ItemActionDataLauncher(ItemInventoryData _invData, int _indexInEntityOfAction)
			: base(_invData, _indexInEntityOfAction)
		{
			this.projectileJointT = ((_invData.model != null) ? _invData.model.FindInChilds("ProjectileJoint", false) : null);
			this.projectileTs = new List<Transform>();
		}

		public Transform projectileJointT;

		public List<Transform> projectileTs;

		public float strainPercent = 1f;

		public float lastAttackStrainPercent;
	}
}
