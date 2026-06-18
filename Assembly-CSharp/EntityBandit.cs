using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class EntityBandit : EntityHuman
{
	public override void PostInit()
	{
		string @string = EntityClass.list[this.entityClass].Properties.GetString(EntityClass.PropHandItem);
		if (@string.Length > 0)
		{
			base.SetInventorySlots(@string);
		}
		if (this.inventory.GetItem(0).IsEmpty())
		{
			ItemValue bareHandItemValue = this.inventory.GetBareHandItemValue();
			bareHandItemValue.Quality = (ushort)this.rand.RandomRange(1, 3);
			bareHandItemValue.UseTimes = (float)bareHandItemValue.MaxUseTimes * 0.7f - 1f;
			this.inventory.SetItem(0, bareHandItemValue, 1, true);
		}
		int num = 1;
		if (!this.inventory.GetItem(num).IsEmpty())
		{
			this.inventory.SetHoldingItemIdx(num);
		}
		if (this.moveHelper != null)
		{
			this.moveHelper.CanOpenDoors = true;
		}
	}

	public override void OnAddedToWorld()
	{
		base.OnAddedToWorld();
	}

	public override void SetupHandItem()
	{
		this.ShowHoldingItem(true);
	}

	public override bool UseHoldingItem(int _actionIndex, bool _isReleased)
	{
		if (!_isReleased)
		{
			ItemActionAttackData itemActionAttackData = this.inventory.holdingItemData.actionData[0] as ItemActionAttackData;
			if (itemActionAttackData != null)
			{
				ItemValue itemValue = itemActionAttackData.invData.itemValue;
				itemValue.UseTimes = (float)itemValue.MaxUseTimes * 0.8f - 1f;
				if (itemActionAttackData is ItemActionRanged.ItemActionDataRanged)
				{
					itemValue.Meta = 2;
				}
			}
		}
		return base.UseHoldingItem(_actionIndex, _isReleased);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void updateTasks()
	{
		base.updateTasks();
		Vector3 vector;
		if (AIFocusAim.GetActiveFocus(this, this.focusAim, out vector))
		{
			base.SetLookPosition(vector);
		}
	}

	public override bool GetAimTarget(out Vector3 aimTarget)
	{
		return AIFocusAim.GetActiveFocus(this, this.focusAim, out aimTarget) || base.GetAimTarget(out aimTarget);
	}

	public override bool GetHeadLookTarget(out Vector3 lookTarget, out bool requiresLineOfSight)
	{
		requiresLineOfSight = false;
		return AIFocusAim.GetActiveFocus(this, this.focusAim, out lookTarget) || base.GetHeadLookTarget(out lookTarget, out requiresLineOfSight);
	}

	public override bool CalcStrafeYawOffset(float _moveX, float _moveZ, ref float _desiredyaw, ref float _yawOffset)
	{
		float num;
		if (AIFocusBody.GetActiveFocus(this, this.focusBody, out num))
		{
			float num2 = Mathf.Atan2(_moveX, _moveZ) * 57.29578f;
			_desiredyaw = num;
			_yawOffset = MathUtils.NormalizeAxis(num2 - _desiredyaw);
			return true;
		}
		return false;
	}

	public readonly AIFocus<AIFocusBody> focusBody = new AIFocus<AIFocusBody>(true);

	public readonly AIFocus<AIFocusAim> focusAim = new AIFocus<AIFocusAim>(true);

	[PublicizedFrom(EAccessModifier.Private)]
	public enum InvSlots
	{
		Melee,
		Ranged,
		Thrown,
		Heal
	}
}
