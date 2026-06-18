using System;
using System.Collections.Generic;

public class QuestLockInstance
{
	public QuestLockInstance(int lockedByEntityID)
	{
		this.AddQuester(lockedByEntityID);
		this.LockedOutUntil = 0UL;
		this.IsLocked = true;
	}

	public void AddQuester(int entityID)
	{
		if (!this.LockedByEntities.Contains(entityID))
		{
			this.LockedByEntities.Add(entityID);
		}
	}

	public void AddQuesters(int[] entityIDs)
	{
		for (int i = 0; i < entityIDs.Length; i++)
		{
			if (!this.LockedByEntities.Contains(entityIDs[i]))
			{
				this.LockedByEntities.Add(entityIDs[i]);
			}
		}
	}

	public void RemoveQuester(int entityID)
	{
		if (this.LockedByEntities.Contains(entityID))
		{
			this.LockedByEntities.Remove(entityID);
		}
		if (this.LockedByEntities.Count == 0)
		{
			this.SetUnlocked();
		}
	}

	public void SetUnlocked()
	{
		if (this.IsLocked)
		{
			this.IsLocked = false;
			if (!GameUtils.IsPlaytesting())
			{
				this.LockedOutUntil = GameManager.Instance.World.GetWorldTime() + 2000UL;
			}
		}
	}

	public bool CheckQuestLock()
	{
		return !this.IsLocked && GameManager.Instance.World.GetWorldTime() > this.LockedOutUntil;
	}

	public bool IsLocked = true;

	public List<int> LockedByEntities = new List<int>();

	public ulong LockedOutUntil;
}
