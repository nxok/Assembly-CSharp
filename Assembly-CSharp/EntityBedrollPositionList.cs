using System;

public class EntityBedrollPositionList
{
	public EntityBedrollPositionList(EntityAlive _e)
	{
		this.theEntity = _e;
	}

	public Vector3i GetPos()
	{
		PersistentPlayerData data = this.GetData();
		if (data != null)
		{
			return data.BedrollPos;
		}
		return new Vector3i(0, int.MaxValue, 0);
	}

	public void Set(Vector3i _pos)
	{
		PersistentPlayerData data = this.GetData();
		if (data != null)
		{
			data.BedrollPos = _pos;
			data.ShowBedrollOnMap();
		}
	}

	public void Clear()
	{
		PersistentPlayerData data = this.GetData();
		if (data != null)
		{
			data.ClearBedroll();
		}
	}

	public int Count
	{
		get
		{
			if (this.GetPos().y == 2147483647)
			{
				return 0;
			}
			return 1;
		}
	}

	public Vector3i this[int _idx]
	{
		get
		{
			return this.GetPos();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public PersistentPlayerData GetData()
	{
		return GameManager.Instance.GetPersistentPlayerList().GetPlayerDataFromEntityID(this.theEntity.entityId);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityAlive theEntity;
}
