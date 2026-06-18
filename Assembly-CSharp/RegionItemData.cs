using System;
using UnityEngine.Scripting;

[Preserve]
public class RegionItemData
{
	public RegionItemData(int x, int z, int updateTime)
	{
		this.X = x;
		this.Z = z;
		this.UpdateTime = updateTime;
	}

	public void Update(int x, int z, int updateTime)
	{
		this.X = x;
		this.Z = z;
		this.UpdateTime = updateTime;
	}

	public void Update(DynamicMeshItem item)
	{
		this.X = item.WorldPosition.x;
		this.Z = item.WorldPosition.z;
		this.UpdateTime = item.UpdateTime;
	}

	public int X;

	public int Z;

	public int UpdateTime;
}
