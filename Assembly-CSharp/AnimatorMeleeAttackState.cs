using System;
using System.Collections;
using UnityEngine;

public class AnimatorMeleeAttackState : StateMachineBehaviour
{
	public AnimatorMeleeAttackState()
	{
		AnimatorMeleeAttackState.FistHoldHash = Animator.StringToHash("fistHold");
		AnimatorMeleeAttackState.FpvFistHoldHash = Animator.StringToHash("fpvFistHold");
	}

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (this.playingImpact)
		{
			return;
		}
		this.hasFired = false;
		this.actionIndex = animator.GetInteger(AvatarController.itemActionIndexHash);
		AnimationEventBridge component = animator.GetComponent<AnimationEventBridge>();
		this.entity = component.entity;
		if (this.actionIndex < 0 || this.actionIndex >= this.entity.inventory.holdingItemData.actionData.Count || this.entity.inventory.holdingItemData.actionData[this.actionIndex] == null)
		{
			return;
		}
		AnimatorClipInfo[] array = animator.GetNextAnimatorClipInfo(layerIndex);
		if (array.Length == 0)
		{
			array = animator.GetCurrentAnimatorClipInfo(layerIndex);
			if (array.Length == 0)
			{
				return;
			}
		}
		AnimationClip clip = array[0].clip;
		float length = clip.length;
		this.attacksPerMinute = (float)((int)(60f / length));
		FastTags<TagGroup.Global> fastTags = ((this.actionIndex == 0) ? ItemActionAttack.PrimaryTag : ItemActionAttack.SecondaryTag);
		ItemValue holdingItemItemValue = this.entity.inventory.holdingItemItemValue;
		ItemClass itemClass = holdingItemItemValue.ItemClass;
		if (itemClass != null)
		{
			fastTags |= itemClass.ItemTags;
		}
		this.originalMeleeAttackSpeed = EffectManager.GetValue(PassiveEffects.AttacksPerMinute, holdingItemItemValue, this.attacksPerMinute, this.entity, null, fastTags, true, true, true, true, true, 1, true, false) / 60f * length;
		animator.SetFloat("MeleeAttackSpeed", this.originalMeleeAttackSpeed);
		ItemClass holdingItem = this.entity.inventory.holdingItem;
		holdingItem.Properties.ParseFloat((this.actionIndex == 0) ? "Action0" : "Action1", "RaycastTime", ref this.RaycastTime);
		float num = -1f;
		holdingItem.Properties.ParseFloat((this.actionIndex == 0) ? "Action0" : "Action1", "ImpactDuration", ref num);
		if (num >= 0f)
		{
			this.ImpactDuration = num * this.originalMeleeAttackSpeed;
		}
		holdingItem.Properties.ParseFloat((this.actionIndex == 0) ? "Action0" : "Action1", "ImpactPlaybackSpeed", ref this.ImpactPlaybackSpeed);
		if (this.originalMeleeAttackSpeed != 0f)
		{
			this.calculatedRaycastTime = this.RaycastTime / this.originalMeleeAttackSpeed;
			this.calculatedImpactDuration = this.ImpactDuration / this.originalMeleeAttackSpeed;
			this.calculatedImpactPlaybackSpeed = this.ImpactPlaybackSpeed / this.originalMeleeAttackSpeed;
		}
		else
		{
			this.calculatedRaycastTime = 0.001f;
			this.calculatedImpactDuration = 0.001f;
			this.calculatedImpactPlaybackSpeed = 0.001f;
		}
		this.iData = this.entity.inventory.holdingItemData.actionData[this.actionIndex] as ItemActionDynamicMelee.ItemActionDynamicMeleeData;
		if (this.iData != null)
		{
			this.iData.HasFinished = false;
		}
		GameManager.Instance.StartCoroutine(this.impactStart(animator, animator.GetNextAnimatorStateInfo(layerIndex), clip, layerIndex));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator impactStart(Animator animator, AnimatorStateInfo stateInfo, AnimationClip clip, int layerIndex)
	{
		yield return new WaitForSeconds(this.calculatedRaycastTime);
		if (!this.hasFired)
		{
			this.hasFired = true;
			if (this.entity != null && !this.entity.isEntityRemote && this.actionIndex >= 0)
			{
				ItemActionDynamicMelee.ItemActionDynamicMeleeData itemActionDynamicMeleeData = this.entity.inventory.holdingItemData.actionData[this.actionIndex] as ItemActionDynamicMelee.ItemActionDynamicMeleeData;
				if (itemActionDynamicMeleeData != null && itemActionDynamicMeleeData.Equals(this.iData) && (this.entity.inventory.holdingItem.Actions[this.actionIndex] as ItemActionDynamicMelee).Raycast(itemActionDynamicMeleeData))
				{
					animator.SetFloat("MeleeAttackSpeed", this.calculatedImpactPlaybackSpeed);
					this.playingImpact = true;
					GameManager.Instance.StartCoroutine(this.impactStop(animator, stateInfo, clip, layerIndex));
				}
			}
		}
		yield break;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator impactStop(Animator animator, AnimatorStateInfo stateInfo, AnimationClip clip, int layerIndex)
	{
		animator.Play(0, layerIndex, Mathf.Min(1f, this.calculatedRaycastTime * this.originalMeleeAttackSpeed / clip.length));
		yield return new WaitForSeconds(this.calculatedImpactDuration);
		animator.SetFloat("MeleeAttackSpeed", this.originalMeleeAttackSpeed);
		this.playingImpact = false;
		yield break;
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (this.entity != null && !this.entity.isEntityRemote && this.actionIndex >= 0)
		{
			ItemActionDynamicMelee.ItemActionDynamicMeleeData itemActionDynamicMeleeData = this.entity.inventory.holdingItemData.actionData[this.actionIndex] as ItemActionDynamicMelee.ItemActionDynamicMeleeData;
			if (itemActionDynamicMeleeData != null)
			{
				itemActionDynamicMeleeData.HasFinished = true;
				animator.SetFloat("MeleeAttackSpeed", this.originalMeleeAttackSpeed);
			}
		}
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		float normalizedTime = stateInfo.normalizedTime;
		if (float.IsInfinity(normalizedTime) || float.IsNaN(normalizedTime))
		{
			if (animator.HasState(layerIndex, AnimatorMeleeAttackState.FistHoldHash))
			{
				animator.Play(AnimatorMeleeAttackState.FistHoldHash, layerIndex, 1f);
				return;
			}
			if (animator.HasState(layerIndex, AnimatorMeleeAttackState.FpvFistHoldHash))
			{
				animator.Play(AnimatorMeleeAttackState.FpvFistHoldHash, layerIndex, 1f);
				return;
			}
			animator.Play(animator.GetNextAnimatorStateInfo(layerIndex).shortNameHash, layerIndex);
		}
	}

	public float RaycastTime = 0.3f;

	public float ImpactDuration = 0.01f;

	public float ImpactPlaybackSpeed = 1f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float calculatedRaycastTime;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float calculatedImpactDuration;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float calculatedImpactPlaybackSpeed;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool hasFired;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int actionIndex;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float originalMeleeAttackSpeed;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool playingImpact;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public EntityAlive entity;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float attacksPerMinute;

	public static int FistHoldHash = Animator.StringToHash("fistHold");

	public static int FpvFistHoldHash = Animator.StringToHash("fpvFistHold");

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public ItemActionDynamicMelee.ItemActionDynamicMeleeData iData;
}
