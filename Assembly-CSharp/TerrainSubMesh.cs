using System;
using System.Collections.Generic;
using UnityEngine;

public struct TerrainSubMesh
{
	public TerrainSubMesh(List<TerrainSubMesh> _others, int _minSize = 0)
	{
		this.others = _others;
		this.textureIds = new ArrayDynamicFast<int>(TerrainSubMesh.vertexColors.Length);
		this.triangles = new ArrayListMP<int>(MemoryPools.poolInt, _minSize);
		this.needToAdd = new ArrayListMP<int>(MemoryPools.poolInt, 0);
	}

	public bool Contains(IList<int> _texIds)
	{
		for (int i = 0; i < _texIds.Count; i++)
		{
			if (_texIds[i] != -1 && this.textureIds.Contains(_texIds[i]) == -1)
			{
				return false;
			}
		}
		return true;
	}

	public bool CanAdd(IList<int> _texIds)
	{
		this.needToAdd.Clear();
		for (int i = 0; i < _texIds.Count; i++)
		{
			if (_texIds[i] != -1 && this.textureIds.Contains(_texIds[i]) == -1)
			{
				this.needToAdd.Add(_texIds[i]);
			}
		}
		if (this.needToAdd.Count == 0)
		{
			return true;
		}
		if (this.needToAdd.Count <= TerrainSubMesh.vertexColors.Length - this.textureIds.Count)
		{
			this.Add(this.needToAdd);
			return true;
		}
		return false;
	}

	public void Add(int[] _texIds)
	{
		foreach (int num in _texIds)
		{
			if (num != -1)
			{
				int num2 = -1;
				int num3 = 0;
				while (num2 == -1 && num3 < this.others.Count)
				{
					num2 = this.others[num3].textureIds.Contains(num);
					if (num2 != -1 && this.textureIds.DataAvail[num2])
					{
						num2 = -1;
					}
					num3++;
				}
				this.textureIds.Add(num2, num);
			}
		}
	}

	public void Add(ArrayListMP<int> _texIds)
	{
		for (int i = 0; i < _texIds.Count; i++)
		{
			int num = _texIds[i];
			if (num != -1)
			{
				int num2 = -1;
				int num3 = 0;
				while (num2 == -1 && num3 < this.others.Count)
				{
					num2 = this.others[num3].textureIds.Contains(num);
					if (num2 != -1 && this.textureIds.DataAvail[num2])
					{
						num2 = -1;
					}
					num3++;
				}
				this.textureIds.Add(num2, num);
			}
		}
	}

	public Color GetColorForTextureId(int _texId)
	{
		int num = this.textureIds.Contains(_texId);
		if (num != -1)
		{
			return TerrainSubMesh.vertexColors[num];
		}
		return TerrainSubMesh.vertexColors[0];
	}

	public int GetTextureIdCount()
	{
		return this.textureIds.Count;
	}

	public int GetTextureId(int _idx)
	{
		for (int i = 0; i < this.textureIds.Size; i++)
		{
			if (this.textureIds.DataAvail[i] && _idx == 0)
			{
				return this.textureIds.Data[i];
			}
			_idx--;
		}
		return 0;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Color[] vertexColors = new Color[]
	{
		new Color(0f, 0f, 0f, 0f),
		new Color(1f, 0f, 0f, 0f),
		new Color(0f, 1f, 0f, 0f)
	};

	public ArrayDynamicFast<int> textureIds;

	public ArrayListMP<int> triangles;

	[PublicizedFrom(EAccessModifier.Private)]
	public ArrayListMP<int> needToAdd;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<TerrainSubMesh> others;
}
