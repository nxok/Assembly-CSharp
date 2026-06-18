using System;
using UnityEngine;

public class WaterDebugRenderer : IMemoryPoolableObject
{
	public void SetChunkOrigin(Vector3 _origin)
	{
		this.chunkOrigin = _origin;
		for (int i = 0; i < this.numActiveLayers; i++)
		{
			int num = this.activeLayers[i];
			Vector3 vector = this.chunkOrigin + Vector3.up * (float)num * 16f;
			this.layers[num].SetLayerOrigin(vector);
		}
	}

	public void SetWater(int _x, int _y, int _z, float mass)
	{
		int num = _y / 16;
		int num2 = _y % 16;
		this.GetOrCreateLayer(num).SetWater(_x, num2, _z, mass);
	}

	public void LoadFromChunk(Chunk chunk)
	{
		this.SetChunkOrigin(chunk.GetWorldPos());
		for (int i = 0; i < 16; i++)
		{
			for (int j = 0; j < 16; j++)
			{
				for (int k = 0; k < 256; k++)
				{
					float num = (float)chunk.GetWater(i, k, j).GetMass();
					if (num > 195f)
					{
						this.SetWater(i, k, j, num);
					}
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public WaterDebugRendererLayer GetOrCreateLayer(int layerIndex)
	{
		WaterDebugRendererLayer waterDebugRendererLayer = this.layers[layerIndex];
		if (waterDebugRendererLayer == null)
		{
			waterDebugRendererLayer = WaterDebugPools.layerPool.AllocSync(false);
			Vector3 vector = this.chunkOrigin + Vector3.up * (float)layerIndex * 16f;
			waterDebugRendererLayer.SetLayerOrigin(vector);
			this.layers[layerIndex] = waterDebugRendererLayer;
			this.activeLayers[this.numActiveLayers] = layerIndex;
			this.numActiveLayers++;
			Array.Sort<int>(this.activeLayers, 0, this.numActiveLayers);
		}
		return waterDebugRendererLayer;
	}

	public void Draw()
	{
		for (int i = 0; i < this.numActiveLayers; i++)
		{
			int num = this.activeLayers[i];
			this.layers[num].Draw();
		}
	}

	public void Clear()
	{
		for (int i = 0; i < this.numActiveLayers; i++)
		{
			int num = this.activeLayers[i];
			WaterDebugRendererLayer waterDebugRendererLayer = this.layers[num];
			WaterDebugPools.layerPool.FreeSync(waterDebugRendererLayer);
			this.layers[num] = null;
			this.activeLayers[i] = 0;
		}
		this.numActiveLayers = 0;
	}

	public void Cleanup()
	{
		this.Clear();
	}

	public void Reset()
	{
		this.Clear();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const int numLayers = 16;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 chunkOrigin = Vector3.zero;

	[PublicizedFrom(EAccessModifier.Private)]
	public WaterDebugRendererLayer[] layers = new WaterDebugRendererLayer[16];

	[PublicizedFrom(EAccessModifier.Private)]
	public int[] activeLayers = new int[16];

	[PublicizedFrom(EAccessModifier.Private)]
	public int numActiveLayers;
}
