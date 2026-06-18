using System;
using Audio;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ItemActionActivate : ItemAction
{
	public override void ExecuteAction(ItemActionData _actionData, bool _bReleased)
	{
		if (!_bReleased)
		{
			return;
		}
		if (Time.time - _actionData.lastUseTime < this.Delay)
		{
			return;
		}
		_actionData.lastUseTime = Time.time;
		EntityAlive holdingEntity = _actionData.invData.holdingEntity;
		if (EffectManager.GetValue(PassiveEffects.DisableItem, holdingEntity.inventory.holdingItemItemValue, 0f, holdingEntity, null, _actionData.invData.item.ItemTags, true, true, true, true, true, 1, true, false) > 0f)
		{
			_actionData.lastUseTime = Time.time + 1f;
			Manager.PlayInsidePlayerHead("twitch_no_attack", -1, 0f, false, false);
			return;
		}
		if (this.soundStart != null)
		{
			_actionData.invData.holdingEntity.PlayOneShot(this.soundStart, this.Sound_in_head, false, false, null, 1f);
		}
		_actionData.invData.holdingEntity.inventory.holdingItem.OnHoldingItemActivated(_actionData.invData.holdingEntity.inventory.holdingItemData);
	}

	public override bool AllowConcurrentActions()
	{
		return false;
	}

	public override bool IsActionRunning(ItemActionData _actionData)
	{
		return _actionData.HasExecuted;
	}

	public override void CancelAction(ItemActionData _actionData)
	{
		base.CancelAction(_actionData);
		_actionData.HasExecuted = false;
		_actionData.lastUseTime = Time.time;
	}
}
