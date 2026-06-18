using System;
using System.Collections.Generic;
using UnityEngine;

public class FlatArea
{
	public int Height
	{
		get
		{
			return this.position.y;
		}
	}

	public int MinX
	{
		get
		{
			return this.position.x;
		}
	}

	public int MaxX
	{
		get
		{
			return this.position.x + this.size - 1;
		}
	}

	public int MinZ
	{
		get
		{
			return this.position.z;
		}
	}

	public int MaxZ
	{
		get
		{
			return this.position.z + this.size - 1;
		}
	}

	public Vector3 Center
	{
		get
		{
			return this.position + new Vector3((float)(this.size / 2), 0f, (float)(this.size / 2));
		}
	}

	public FlatArea(Vector3i _position, int _size)
	{
		this.position = _position;
		this.size = _size;
	}

	public bool IsValid(World world, BiomeFilterTypes biomeFilter = BiomeFilterTypes.AnyBiome, string[] biomeNames = null, ChunkProtectionLevel maxAllowedChunkProtectionLevel = ChunkProtectionLevel.NearLandClaim)
	{
		this.maxChunkProtectionLevel = ChunkProtectionLevel.None;
		int num = this.MinX >> 4 << 4;
		int num2 = this.MaxX >> 4 << 4;
		int num3 = this.MinZ >> 4 << 4;
		int num4 = this.MaxZ >> 4 << 4;
		for (int i = num; i <= num2; i += 16)
		{
			for (int j = num3; j <= num4; j += 16)
			{
				ChunkProtectionLevel chunkProtectionLevel = world.ChunkCache.ChunkProvider.GetChunkProtectionLevel(new Vector3i(i, this.position.y, j));
				if (chunkProtectionLevel > this.maxChunkProtectionLevel)
				{
					this.maxChunkProtectionLevel = chunkProtectionLevel;
				}
			}
		}
		if (this.maxChunkProtectionLevel > maxAllowedChunkProtectionLevel)
		{
			return false;
		}
		if (biomeFilter != BiomeFilterTypes.AnyBiome)
		{
			BiomeDefinition biomeInWorld = GameManager.Instance.World.GetBiomeInWorld((int)this.Center.x, (int)this.Center.z);
			if (biomeInWorld == null)
			{
				return false;
			}
			if (biomeFilter == BiomeFilterTypes.OnlyBiome)
			{
				if (biomeInWorld.m_sBiomeName != biomeNames[0])
				{
					return false;
				}
			}
			else if (biomeFilter == BiomeFilterTypes.ExcludeBiome)
			{
				bool flag = false;
				for (int k = 0; k < biomeNames.Length; k++)
				{
					if (biomeInWorld.m_sBiomeName == biomeNames[k])
					{
						flag = true;
						break;
					}
				}
				if (flag)
				{
					return false;
				}
			}
			else if (biomeFilter == BiomeFilterTypes.SameBiome && biomeInWorld.m_sBiomeName != biomeNames[0])
			{
				return false;
			}
		}
		return !world.ChunkCache.ChunkProvider.GetDynamicPrefabDecorator().HasPrefabsAtXZ(this.MinX, this.MaxX, this.MinZ, this.MaxZ);
	}

	public bool IsInArea(int _x, int _z)
	{
		return _x >= this.MinX && _x <= this.MaxX && _z >= this.MinZ && _z <= this.MaxZ;
	}

	public List<Vector2i> GetPositions()
	{
		List<Vector2i> list = new List<Vector2i>();
		for (int i = this.MinX; i <= this.MaxX; i++)
		{
			for (int j = this.MinZ; j <= this.MaxZ; j++)
			{
				list.Add(new Vector2i(i, j));
			}
		}
		return list;
	}

	public Vector3 GetRandomPosition(float margin = 0f)
	{
		return new Vector3(Random.Range((float)this.MinX + margin, (float)this.MaxX - margin + 1f), (float)this.Height, Random.Range((float)this.MinZ + margin, (float)this.MaxZ - margin + 1f));
	}

	public override bool Equals(object obj)
	{
		FlatArea flatArea = obj as FlatArea;
		return flatArea != null && this.position == flatArea.position && this.size == flatArea.size;
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}

	public Vector3i position;

	public int size;

	public ChunkProtectionLevel maxChunkProtectionLevel;
}
