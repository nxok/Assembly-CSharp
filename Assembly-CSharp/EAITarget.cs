using System;
using UnityEngine.Scripting;

[Preserve]
public abstract class EAITarget : EAIBase
{
	public void Init(EntityAlive _theEntity, float _maxXZDistance, bool _bNeedToSee)
	{
		base.Init(_theEntity);
		this.seeCounter = 0;
		this.maxXZDistance = _maxXZDistance;
		this.bNeedToSee = _bNeedToSee;
	}

	public override void Start()
	{
		this.seeCounter = 0;
	}

	public override bool Continue()
	{
		EntityAlive attackTarget = this.theEntity.GetAttackTarget();
		if (attackTarget == null)
		{
			return false;
		}
		if (!attackTarget.IsAlive())
		{
			return false;
		}
		if (this.maxXZDistance > 0f && this.theEntity.GetDistanceSq(attackTarget) > this.maxXZDistance * this.maxXZDistance)
		{
			return false;
		}
		if (this.bNeedToSee)
		{
			if (!this.theEntity.CanSee(attackTarget))
			{
				int num = this.seeCounter + 1;
				this.seeCounter = num;
				if (num > 600)
				{
					return false;
				}
			}
			else
			{
				this.seeCounter = 0;
			}
		}
		return true;
	}

	public override void Reset()
	{
		this.theEntity.SetAttackTarget(null, 0);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool check(EntityAlive _e)
	{
		if (_e == null)
		{
			return false;
		}
		if (_e == this.theEntity)
		{
			return false;
		}
		if (!_e.IsAlive())
		{
			return false;
		}
		if (_e.IsIgnoredByAI())
		{
			return false;
		}
		Vector3i vector3i = World.worldToBlockPos(_e.position);
		if (!this.theEntity.isWithinHomeDistance(vector3i.x, vector3i.y, vector3i.z))
		{
			return false;
		}
		if (this.bNeedToSee && !this.theEntity.CanSee(_e))
		{
			return false;
		}
		EntityPlayer entityPlayer = _e as EntityPlayer;
		return !(entityPlayer != null) || this.theEntity.CanSeeStealth(this.manager.GetSeeDistance(entityPlayer), entityPlayer.Stealth.lightLevel);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public EAITarget()
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public float maxXZDistance;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bNeedToSee;

	[PublicizedFrom(EAccessModifier.Private)]
	public int seeCounter;
}
