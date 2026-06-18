using System;
using UnityEngine;

public struct AIFocusBody : IFocusTarget
{
	public AIFocusBody(float target)
	{
		this.TargetYaw = target;
		this.TargetYawEntity = null;
		this.HasTargetYaw = true;
		this.ConditionDistance = default(AIFocusConditionDistance);
	}

	public AIFocusBody(EntityAlive target)
	{
		this.TargetYaw = 0f;
		this.TargetYawEntity = target;
		this.HasTargetYaw = false;
		this.ConditionDistance = default(AIFocusConditionDistance);
	}

	public bool TryGetValue(EntityAlive theEntity, out float value)
	{
		if (this.HasTargetYaw)
		{
			value = this.TargetYaw;
			return true;
		}
		if (this.TargetYawEntity != null)
		{
			value = theEntity.YawForTarget(this.TargetYawEntity);
			return true;
		}
		value = 0f;
		return false;
	}

	public static bool GetActiveFocusForPriority(EntityAlive theEntity, FocusPriority priority, AIFocus<AIFocusBody> focus, out float focusForPriority)
	{
		return focus.FocusTargets[(int)priority].TryGetValue(theEntity, out focusForPriority);
	}

	public static bool GetActiveFocus(EntityAlive theEntity, AIFocus<AIFocusBody> focus, out float activeFocus)
	{
		Vector3 position = theEntity.position;
		for (int i = 0; i < focus.FocusTargets.Length; i++)
		{
			ref AIFocusBody ptr = ref focus.FocusTargets[i];
			float num;
			if (ptr.TryGetValue(theEntity, out num) && !ptr.ConditionDistance.IsFocusDisabled(theEntity))
			{
				activeFocus = num;
				return true;
			}
		}
		activeFocus = 0f;
		return false;
	}

	public readonly EntityAlive TargetYawEntity;

	public readonly float TargetYaw;

	public readonly bool HasTargetYaw;

	public AIFocusConditionDistance ConditionDistance;
}
