using System;
using System.Collections.Generic;

public class BaseQuestData
{
	public void AddSharedQuester(int _entityID)
	{
		if (!this.entityList.Contains(_entityID))
		{
			this.entityList.Add(_entityID);
			EntityPlayer entityPlayer = GameManager.Instance.World.GetEntity(_entityID) as EntityPlayer;
			if (entityPlayer != null)
			{
				this.OnAdd(entityPlayer);
			}
		}
	}

	public void RemoveSharedQuester(EntityPlayer _player)
	{
		if (this.entityList.Contains(_player.entityId))
		{
			this.entityList.Remove(_player.entityId);
		}
		if (this.entityList.Count == 0)
		{
			this.OnRemove(_player);
			this.RemoveFromDictionary();
		}
	}

	public bool ContainsEntity(int _entityID)
	{
		return this.entityList.Contains(_entityID);
	}

	public virtual void SetModifier(string _name)
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void RemoveFromDictionary()
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnCreated()
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnAdd(EntityPlayer player)
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnRemove(EntityPlayer player)
	{
	}

	public void Remove()
	{
		this.entityList.Clear();
		this.OnRemove(null);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public int questCode;

	[PublicizedFrom(EAccessModifier.Protected)]
	public List<int> entityList = new List<int>();
}
