using System;
using System.Collections.Generic;

public class CompanionGroup
{
	public EntityAlive this[int index]
	{
		get
		{
			return this.MemberList[index];
		}
	}

	public int Count
	{
		get
		{
			return this.MemberList.Count;
		}
	}

	public void Add(EntityAlive entity)
	{
		this.MemberList.Add(entity);
		OnCompanionGroupChanged onGroupChanged = this.OnGroupChanged;
		if (onGroupChanged == null)
		{
			return;
		}
		onGroupChanged();
	}

	public void Remove(EntityAlive entity)
	{
		this.MemberList.Remove(entity);
		OnCompanionGroupChanged onGroupChanged = this.OnGroupChanged;
		if (onGroupChanged == null)
		{
			return;
		}
		onGroupChanged();
	}

	public int IndexOf(EntityAlive entity)
	{
		return this.MemberList.IndexOf(entity);
	}

	public OnCompanionGroupChanged OnGroupChanged;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<EntityAlive> MemberList = new List<EntityAlive>();
}
