using System;
using UnityEngine;

public readonly struct AIFocusAim : IFocusTarget
{
	public AIFocusAim(Entity target, AIAimFocusOffset targetOffset)
	{
		this.Target = target;
		this.TargetOffset = targetOffset;
		this.ConditionDistance = default(AIFocusConditionDistance);
	}

	public static bool GetActiveFocus(Entity theEntity, AIFocus<AIFocusAim> focus, out Vector3 activeFocus)
	{
		Vector3 position = theEntity.position;
		for (int i = 0; i < focus.FocusTargets.Length; i++)
		{
			Entity target = focus.FocusTargets[i].Target;
			if (target && !focus.FocusTargets[i].ConditionDistance.IsFocusDisabled(theEntity))
			{
				Vector3 vector = target.position;
				switch (focus.FocusTargets[i].TargetOffset)
				{
				case AIAimFocusOffset.Belly:
					vector = target.getBellyPosition();
					goto IL_00A7;
				case AIAimFocusOffset.Chest:
					vector = target.getChestPosition();
					goto IL_00A7;
				case AIAimFocusOffset.Head:
					vector = target.getHeadPosition();
					goto IL_00A7;
				}
				vector = focus.FocusTargets[i].Target.position;
				IL_00A7:
				activeFocus = vector;
				return true;
			}
		}
		activeFocus = default(Vector3);
		return false;
	}

	public readonly Entity Target;

	public readonly AIAimFocusOffset TargetOffset;

	public readonly AIFocusConditionDistance ConditionDistance;
}
