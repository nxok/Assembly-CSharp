using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class EAISetNearestEntityAsTargetSorter : IComparer<Entity>
{
	public EAISetNearestEntityAsTargetSorter(Entity _entity)
	{
		this.theEntity = _entity;
	}

	public int Compare(Entity _e, Entity _e2)
	{
		float distanceSq = this.theEntity.GetDistanceSq(_e);
		float distanceSq2 = this.theEntity.GetDistanceSq(_e2);
		if (distanceSq < distanceSq2)
		{
			return -1;
		}
		if (distanceSq > distanceSq2)
		{
			return 1;
		}
		return 0;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Entity theEntity;
}
