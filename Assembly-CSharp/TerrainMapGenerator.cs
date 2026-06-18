using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class TerrainMapGenerator
{
	public void GenerateTerrain(IChunkProvider _chunkProvider)
	{
		World world = GameManager.Instance.World;
		this.xs = world.GetPrimaryPlayer().GetBlockPosition().x - 512;
		this.zs = world.GetPrimaryPlayer().GetBlockPosition().z - 512;
		MicroStopwatch microStopwatch = new MicroStopwatch();
		long num = microStopwatch.ElapsedMilliseconds;
		int num2 = 1;
		HashSetLong hashSetLong = new HashSetLong();
		for (int i = 0; i < 1025; i++)
		{
			for (int j = 0; j < 1025; j++)
			{
				int num3 = (this.xs + i) * num2;
				int num4 = (this.zs + j) * num2;
				long num5 = WorldChunkCache.MakeChunkKey(num3 / 16, num4 / 16);
				if (!hashSetLong.Contains(num5))
				{
					hashSetLong.Add(num5);
					_chunkProvider.RequestChunk(num3 / 16, num4 / 16);
				}
			}
		}
		Log.Out("Request chunks: " + (microStopwatch.ElapsedMilliseconds - num).ToString());
		num = microStopwatch.ElapsedMilliseconds;
		bool flag = false;
		do
		{
			flag = false;
			foreach (long num6 in hashSetLong)
			{
				if (GameManager.Instance.World.GetChunkSync(num6) == null)
				{
					flag = true;
					break;
				}
			}
			Thread.Sleep(300);
		}
		while (flag);
		Log.Out("Generate chunks: " + (microStopwatch.ElapsedMilliseconds - num).ToString());
		num = microStopwatch.ElapsedMilliseconds;
		for (int k = 0; k < 1025; k++)
		{
			for (int l = 0; l < 1025; l++)
			{
				int num7 = (this.xs + k) * num2;
				int num8 = (this.zs + l) * num2;
				long num9 = WorldChunkCache.MakeChunkKey(num7 / 16, num8 / 16);
				Chunk chunk = (Chunk)GameManager.Instance.World.GetChunkSync(num9);
				int num10 = World.toBlockXZ(num7);
				int num11 = World.toBlockXZ(num8);
				BiomeDefinition biome = world.Biomes.GetBiome(chunk.GetBiomeId(num10, num11));
				this.biomeDefs[k + l * 1025] = biome;
				this.heights[k + l * 1025] = (int)chunk.GetHeight(num10, num11);
				this.blockValues[k + l * 1025] = chunk.GetBlock(num10, this.heights[k + l * 1025], num11);
				sbyte density = chunk.GetDensity(num10, this.heights[k + l * 1025], num11);
				this.densitySub[k + l * 1025] = 1f - (float)density / -128f;
				this.subbiome[k + l * 1025] = (int)biome.m_Id;
			}
		}
		Log.Out("Cache data: " + (microStopwatch.ElapsedMilliseconds - num).ToString());
		num = microStopwatch.ElapsedMilliseconds;
		for (int m = 0; m < 1024; m++)
		{
			for (int n = 0; n < 1024; n++)
			{
				BiomeDefinition biomeDefinition = this.biomeDefs[m + n * 1025];
				if (biomeDefinition != null && biomeDefinition.m_Layers.Count != 0)
				{
					int num12 = this.heights[m + n * 1025];
					int num13 = this.subbiome[m + n * 1025];
					float num14 = this.biomeIntens[m + n * 1025];
					BlockValue blockValue = this.blockValues[m + n * 1025];
					Vector3 vector = Vector3.up;
					Color color = BlockLiquidv2.Color;
					bool flag2 = world.IsWater(m, num12, n);
					if (!flag2)
					{
						vector = this.calcNormal(m, n, 0, 1, 1, 0);
						if (m > 0 && n > 0)
						{
							Vector3 vector2 = this.calcNormal(m, n, 0, -1, -1, 0);
							Vector3 vector3 = this.calcNormal(m, n, 1, 1, 1, -1);
							Vector3 vector4 = this.calcNormal(m, n, -1, -1, -1, 1);
							vector = (vector + vector2 + vector3 + vector4) * 0.25f;
							vector = vector.normalized;
						}
					}
					vector..ctor(vector.z, vector.x, vector.y);
					this.normalArray[m + n * 1024] = new Color((vector.x + 1f) / 2f, (vector.y + 1f) / 2f, (vector.z + 1f) / 2f, 1f);
					if (!flag2)
					{
						color = blockValue.Block.GetMapColor(blockValue, vector, num12);
					}
					this.colArray[m + n * 1024] = color;
					this.colArray[m + n * 1024] = new Color(this.colArray[m + n * 1024].r, this.colArray[m + n * 1024].g, this.colArray[m + n * 1024].b, 1f);
				}
			}
		}
		Log.Out("Create image: " + (microStopwatch.ElapsedMilliseconds - num).ToString());
		num = microStopwatch.ElapsedMilliseconds;
		Texture2D texture2D = new Texture2D(1024, 1024);
		texture2D.SetPixels(this.colArray);
		texture2D.Apply();
		TextureUtils.SaveTexture(texture2D, "Map.png");
		Object.Destroy(texture2D);
		GCUtils.UnloadAndCollectStart();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 calcNormal(int x, int z, int _xAdd1, int _zAdd1, int _xAdd2, int _zAdd2)
	{
		Vector3 up = Vector3.up;
		float num = (float)this.heights[x + z * 1025] - this.densitySub[x + z * 1025];
		float num2 = (float)this.heights[x + _xAdd1 + (z + _zAdd1) * 1025] - this.densitySub[x + _xAdd1 + (z + _zAdd1) * 1025];
		float num3 = (float)this.heights[x + _xAdd2 + (z + _zAdd2) * 1025] - this.densitySub[x + _xAdd2 + (z + _zAdd2) * 1025];
		Vector3 vector = new Vector3((float)_xAdd1, num3, (float)_zAdd1) - new Vector3(0f, num, 0f);
		Vector3 vector2 = new Vector3((float)_xAdd2, num2, (float)_zAdd2) - new Vector3(0f, num, 0f);
		return Vector3.Cross(vector, vector2).normalized;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int xs;

	[PublicizedFrom(EAccessModifier.Private)]
	public int zs;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int w = 1024;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int h = 1024;

	[PublicizedFrom(EAccessModifier.Private)]
	public Color[] colArray = new Color[1048576];

	[PublicizedFrom(EAccessModifier.Private)]
	public Color[] normalArray = new Color[1048576];

	[PublicizedFrom(EAccessModifier.Private)]
	public int[] heights = new int[1050625];

	[PublicizedFrom(EAccessModifier.Private)]
	public float[] densitySub = new float[1050625];

	[PublicizedFrom(EAccessModifier.Private)]
	public float[] biomeIntens = new float[1050625];

	[PublicizedFrom(EAccessModifier.Private)]
	public BlockValue[] blockValues = new BlockValue[1050625];

	[PublicizedFrom(EAccessModifier.Private)]
	public BiomeDefinition[] biomeDefs = new BiomeDefinition[1050625];

	[PublicizedFrom(EAccessModifier.Private)]
	public int[] subbiome = new int[1050625];
}
