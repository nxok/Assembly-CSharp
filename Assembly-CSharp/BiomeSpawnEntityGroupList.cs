using System;
using System.Collections.Generic;

public class BiomeSpawnEntityGroupList
{
	public BiomeSpawnEntityGroupData Find(int _idHash)
	{
		for (int i = 0; i < this.list.Count; i++)
		{
			if (this.list[i].idHash == _idHash)
			{
				return this.list[i];
			}
		}
		return null;
	}

	public List<BiomeSpawnEntityGroupData> list = new List<BiomeSpawnEntityGroupData>();
}
