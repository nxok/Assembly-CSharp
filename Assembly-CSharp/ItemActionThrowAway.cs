using System;
using Audio;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ItemActionThrowAway : ItemAction
{
	public ItemActionThrowAway()
	{
		Texture2D texture2D = new Texture2D(1, 1);
		texture2D.SetPixel(0, 0, new Color(0f, 1f, 0f, 0.35f));
		texture2D.Apply();
		this.progressBarStyle = new GUIStyle();
		this.progressBarStyle.normal.background = texture2D;
	}

	public override void ReadFrom(DynamicProperties _props)
	{
		base.ReadFrom(_props);
		this.defaultThrowStrength = 1.1f;
		_props.ParseFloat("Throw_strength_default", ref this.defaultThrowStrength);
		this.maxThrowStrength = 5f;
		_props.ParseFloat("Throw_strength_max", ref this.maxThrowStrength);
		this.maxStrainTime = 2f;
		_props.ParseFloat("Max_strain_time", ref this.maxStrainTime);
	}

	public override ItemActionData CreateModifierData(ItemInventoryData _invData, int _indexInEntityOfAction)
	{
		return new ItemActionThrowAway.MyInventoryData(_invData, _indexInEntityOfAction);
	}

	public override void OnScreenOverlay(ItemActionData _actionData)
	{
		ItemActionThrowAway.MyInventoryData myInventoryData = (ItemActionThrowAway.MyInventoryData)_actionData;
		EntityPlayerLocal entityPlayerLocal = myInventoryData.invData.holdingEntity as EntityPlayerLocal;
		if (entityPlayerLocal != null)
		{
			LocalPlayerUI playerUI = entityPlayerLocal.PlayerUI;
			if (!myInventoryData.isCooldown && myInventoryData.m_bActivated && Time.time - myInventoryData.m_ActivateTime > 0.2f)
			{
				float num = Mathf.Min(this.maxStrainTime, Time.time - myInventoryData.m_ActivateTime) / this.maxStrainTime;
				XUiC_ThrowPower.Status(playerUI, num);
				return;
			}
			XUiC_ThrowPower.Status(playerUI, -1f);
		}
	}

	public override void StartHolding(ItemActionData _data)
	{
		this.originalType = _data.invData.holdingEntity.inventory.holdingItemItemValue.type;
		base.StartHolding(_data);
	}

	public override void StopHolding(ItemActionData _actionData)
	{
		base.StopHolding(_actionData);
		this.CancelAction(_actionData);
		ItemActionThrowAway.MyInventoryData myInventoryData = (ItemActionThrowAway.MyInventoryData)_actionData;
		myInventoryData.m_bActivated = false;
		EntityPlayerLocal entityPlayerLocal = myInventoryData.invData.holdingEntity as EntityPlayerLocal;
		if (entityPlayerLocal != null)
		{
			XUiC_ThrowPower.Status(entityPlayerLocal.PlayerUI, -1f);
		}
		myInventoryData.m_LastThrowTime = -1f;
	}

	public override void CancelAction(ItemActionData _actionData)
	{
		ItemActionThrowAway.MyInventoryData myInventoryData = (ItemActionThrowAway.MyInventoryData)_actionData;
		if (myInventoryData.m_bActivated)
		{
			myInventoryData.m_bActivated = false;
			myInventoryData.m_bCanceled = true;
			myInventoryData.m_bReleased = false;
		}
	}

	public override bool AllowConcurrentActions()
	{
		return false;
	}

	public override void ExecuteAction(ItemActionData _actionData, bool _bReleased)
	{
		ItemActionThrowAway.MyInventoryData myInventoryData = (ItemActionThrowAway.MyInventoryData)_actionData;
		EntityAlive holdingEntity = _actionData.invData.holdingEntity;
		if (!_bReleased)
		{
			if (!myInventoryData.m_bActivated)
			{
				myInventoryData.m_bActivated = true;
				myInventoryData.m_ActivateTime = Time.time;
			}
			return;
		}
		if (!myInventoryData.m_bActivated)
		{
			return;
		}
		if (myInventoryData.isCooldown)
		{
			return;
		}
		myInventoryData.m_bReleased = true;
		if (holdingEntity.inventory.holdingCount != 0)
		{
			holdingEntity.emodel.avatarController.TriggerEvent(AvatarController.itemThrownAwayTriggerHash);
		}
		if (Time.time - myInventoryData.m_ActivateTime < 0.2f || this.maxStrainTime == 0f)
		{
			myInventoryData.m_ThrowStrength = this.defaultThrowStrength;
		}
		else
		{
			myInventoryData.m_ThrowStrength = Mathf.Min(this.maxStrainTime, Time.time - myInventoryData.m_ActivateTime) / this.maxStrainTime * this.maxThrowStrength;
		}
		if (holdingEntity.inventory.holdingItemItemValue.Meta == 0 && EffectManager.GetValue(PassiveEffects.DisableItem, holdingEntity.inventory.holdingItemItemValue, 0f, holdingEntity, null, _actionData.invData.item.ItemTags, true, true, true, true, true, 1, true, false) > 0f)
		{
			myInventoryData.m_LastThrowTime = Time.time + 1f;
			myInventoryData.m_bActivated = false;
			Manager.PlayInsidePlayerHead("twitch_no_attack", -1, 0f, false, false);
			return;
		}
		myInventoryData.m_LastThrowTime = Time.time;
		myInventoryData.m_bActivated = false;
		myInventoryData.invData.holdingEntity.RightArmAnimationAttack = true;
		if (this.soundStart != null)
		{
			myInventoryData.invData.holdingEntity.PlayOneShot(this.soundStart, false, false, false, null, 1f);
		}
	}

	public override bool IsActionRunning(ItemActionData _actionData)
	{
		ItemActionThrowAway.MyInventoryData myInventoryData = (ItemActionThrowAway.MyInventoryData)_actionData;
		return myInventoryData.m_bActivated || (myInventoryData.m_LastThrowTime > 0f && Time.time - myInventoryData.m_LastThrowTime < 2f * AnimationDelayData.AnimationDelay[myInventoryData.invData.item.HoldType.Value].RayCast);
	}

	public override void OnHoldingUpdate(ItemActionData _actionData)
	{
		ItemActionThrowAway.MyInventoryData myInventoryData = (ItemActionThrowAway.MyInventoryData)_actionData;
		EntityAlive holdingEntity = _actionData.invData.holdingEntity;
		if (myInventoryData.isCooldown)
		{
			holdingEntity.emodel.avatarController.CancelEvent(AvatarController.itemThrownAwayTriggerHash);
			myInventoryData.isCooldown = Time.time - myInventoryData.m_LastThrowTime < this.Delay;
			if (myInventoryData.m_bActivated)
			{
				myInventoryData.m_ActivateTime = Time.time;
			}
		}
		if (holdingEntity.inventory.holdingItemItemValue.type != this.originalType)
		{
			holdingEntity.emodel.avatarController.CancelEvent(AvatarController.itemThrownAwayTriggerHash);
			myInventoryData.m_bActivated = false;
			myInventoryData.m_bReleased = false;
			return;
		}
		EntityPlayerLocal entityPlayerLocal = holdingEntity as EntityPlayerLocal;
		if (entityPlayerLocal != null && myInventoryData.m_bActivated)
		{
			entityPlayerLocal.StartTPCameraLockTimer();
		}
		if (myInventoryData.m_bReleased)
		{
			if (holdingEntity.inventory.holdingCount == 1)
			{
				holdingEntity.emodel.avatarController.TriggerEvent(AvatarController.itemThrownAwayTriggerHash);
			}
			float rayCast = AnimationDelayData.AnimationDelay[myInventoryData.invData.item.HoldType.Value].RayCast;
			if (myInventoryData.m_LastThrowTime <= 0f || Time.time - myInventoryData.m_LastThrowTime < rayCast)
			{
				return;
			}
			myInventoryData.m_LastThrowTime = Time.time;
			myInventoryData.m_bReleased = false;
			this.throwAway(myInventoryData);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void throwAway(ItemActionThrowAway.MyInventoryData _actionData)
	{
		ItemInventoryData invData = _actionData.invData;
		EntityAlive holdingEntity = invData.holdingEntity;
		EntityPlayerLocal entityPlayerLocal = holdingEntity as EntityPlayerLocal;
		bool flag = entityPlayerLocal != null && !entityPlayerLocal.bFirstPersonView;
		if (holdingEntity.inventory.holdingItemItemValue.Meta == 0 && EffectManager.GetValue(PassiveEffects.DisableItem, holdingEntity.inventory.holdingItemItemValue, 0f, holdingEntity, null, _actionData.invData.item.ItemTags, true, true, true, true, true, 1, true, false) > 0f)
		{
			_actionData.m_bActivated = false;
			return;
		}
		EntityPlayerLocal entityPlayerLocal2 = _actionData.invData.holdingEntity as EntityPlayerLocal;
		if (entityPlayerLocal2 != null && !entityPlayerLocal2.TPCameraCheckPassed)
		{
			return;
		}
		Vector3 lookVector = holdingEntity.GetLookVector();
		Vector3 headPosition = holdingEntity.getHeadPosition();
		Vector3 vector = ((EntityPlayerLocal)holdingEntity).GetCrosshairPosition3D(0f, 0f, headPosition);
		Ray ray;
		if (flag)
		{
			ray..ctor(headPosition, vector - Origin.position);
		}
		else
		{
			ray..ctor(vector - Origin.position, lookVector);
		}
		RaycastHit raycastHit;
		if (!Physics.Raycast(ray, ref raycastHit, 0.28f, -555274253))
		{
			vector += 0.23f * lookVector;
			vector -= headPosition;
			invData.gameManager.ItemDropServer(new ItemStack(holdingEntity.inventory.holdingItemItemValue, 1), vector, Vector3.zero, lookVector * _actionData.m_ThrowStrength, holdingEntity.entityId, 60f, true, -1);
			if (!holdingEntity.inventory.DecHoldingItem(1))
			{
				_actionData.invData.holdingEntity.emodel.avatarController.CancelEvent(AvatarController.itemThrownAwayTriggerHash);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public const float SHORT_CLICK_TIME = 0.2f;

	[PublicizedFrom(EAccessModifier.Protected)]
	public float defaultThrowStrength;

	[PublicizedFrom(EAccessModifier.Protected)]
	public float maxThrowStrength;

	[PublicizedFrom(EAccessModifier.Protected)]
	public float maxStrainTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public GUIStyle progressBarStyle;

	[PublicizedFrom(EAccessModifier.Private)]
	public int originalType;

	public class MyInventoryData : ItemActionData
	{
		public MyInventoryData(ItemInventoryData _invData, int _indexInEntityOfAction)
			: base(_invData, _indexInEntityOfAction)
		{
		}

		public float m_ActivateTime;

		public bool m_bActivated;

		public bool m_bReleased;

		public float m_LastThrowTime;

		public float m_ThrowStrength;

		public bool m_bCanceled;

		public bool isCooldown;
	}
}
