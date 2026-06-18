using System;
using System.Collections.Generic;
using UnityEngine;

public class EntitySeeCache
{
	public EntitySeeCache(EntityAlive _e)
	{
		this.theEntity = _e;
	}

	public bool CanSee(Entity _e)
	{
		if (_e == null)
		{
			return false;
		}
		if (this.positiveCache.Contains(_e.entityId))
		{
			return true;
		}
		if (this.negativeCache.Contains(_e.entityId))
		{
			return false;
		}
		bool flag = this.theEntity.CanEntityBeSeen(_e, true);
		if (flag)
		{
			this.positiveCache.Add(_e.entityId);
			if (_e.IsClientControlled())
			{
				this.lastTimeSeenAPlayer = Time.time;
				return flag;
			}
		}
		else
		{
			this.negativeCache.Add(_e.entityId);
		}
		return flag;
	}

	public float GetLastTimePlayerSeen()
	{
		return this.lastTimeSeenAPlayer;
	}

	public void SetLastTimePlayerSeen()
	{
		this.lastTimeSeenAPlayer = Time.time;
	}

	public void SetCanSee(Entity _e)
	{
		this.positiveCache.Add(_e.entityId);
	}

	public void Clear()
	{
		this.positiveCache.Clear();
		this.negativeCache.Clear();
	}

	public void ClearIfExpired()
	{
		int num = this.ticksSinceLastClear + 1;
		this.ticksSinceLastClear = num;
		if (num >= 30)
		{
			this.ticksSinceLastClear = 0;
			this.Clear();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityAlive theEntity;

	[PublicizedFrom(EAccessModifier.Private)]
	public HashSet<int> positiveCache = new HashSet<int>();

	[PublicizedFrom(EAccessModifier.Private)]
	public HashSet<int> negativeCache = new HashSet<int>();

	[PublicizedFrom(EAccessModifier.Private)]
	public int ticksSinceLastClear;

	[PublicizedFrom(EAccessModifier.Private)]
	public float lastTimeSeenAPlayer;
}
