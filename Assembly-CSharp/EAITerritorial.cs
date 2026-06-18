using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class EAITerritorial : EAIBase
{
	public EAITerritorial()
	{
		this.MutexBits = 1;
	}

	public override void SetData(Dictionary<string, string> data)
	{
		base.SetData(data);
	}

	public override bool CanExecute()
	{
		if (this.theEntity.isWithinHomeDistanceCurrentPosition())
		{
			return false;
		}
		ChunkCoordinates homePosition = this.theEntity.getHomePosition();
		Vector3 vector = RandomPositionGenerator.CalcTowards(this.theEntity, 5, 15, 7, homePosition.position.ToVector3());
		if (vector.Equals(Vector3.zero))
		{
			return false;
		}
		this.movePos = vector;
		return true;
	}

	public override bool Continue()
	{
		return !this.theEntity.getNavigator().noPathAndNotPlanningOne();
	}

	public override void Start()
	{
		this.theEntity.FindPath(this.movePos, this.theEntity.GetMoveSpeed(), false, this);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 movePos;
}
