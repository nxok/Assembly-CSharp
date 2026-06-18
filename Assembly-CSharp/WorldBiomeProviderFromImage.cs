using System;
using System.Collections;
using Unity.Collections;
using UnityEngine;

public class WorldBiomeProviderFromImage : IBiomeProvider
{
	public WorldBiomeProviderFromImage(string _levelName, string _worldPath, WorldBiomes _biomes, int _worldSize = 4096)
	{
		this.worldName = _levelName;
		this.worldPath = _worldPath;
		this.m_Biomes = _biomes;
		this.worldSize = _worldSize;
		this.worldSizeHalf = _worldSize / 2;
	}

	public IEnumerator InitData()
	{
		string text = this.worldPath + "/biomes";
		Texture2D biomesTex;
		if (SdFile.Exists(text + ".tga"))
		{
			biomesTex = TextureUtils.LoadTexture(text + ".tga", 0, false, false, null);
		}
		else
		{
			biomesTex = TextureUtils.LoadTexture(text + ".png", 0, false, false, null);
		}
		yield return null;
		this.biomeMapWidth = biomesTex.width;
		this.biomeMapHeight = biomesTex.height;
		this.biomesMapWidthHalf = biomesTex.width / 2;
		this.biomesMapHeightHalf = biomesTex.height / 2;
		Log.Out("Biomes image size w= " + biomesTex.width.ToString() + ", h = " + biomesTex.height.ToString());
		yield return null;
		BiomeImageLoader biomesLoader = new BiomeImageLoader(biomesTex, this.m_Biomes.GetBiomeMap());
		yield return biomesLoader.Load();
		this.m_BiomeMap = biomesLoader.biomeMap;
		biomesLoader = default(BiomeImageLoader);
		yield return null;
		this.biomesScaleDiv = this.worldSize / biomesTex.width;
		Object.Destroy(biomesTex);
		this.noiseGen = new PerlinNoise(this.worldName.GetStableHashCode());
		yield return null;
		Texture2D texture2D = null;
		text = this.worldPath + "/radiation";
		if (SdFile.Exists(text + ".png"))
		{
			texture2D = TextureUtils.LoadTexture(text + ".png", 0, false, false, null);
		}
		else if (SdFile.Exists(text + ".tga"))
		{
			texture2D = TextureUtils.LoadTexture(text + ".tga", 0, false, false, null);
		}
		if (texture2D != null)
		{
			this.radiationMapSize = texture2D.width;
			this.radiationMapScale = this.worldSize / this.radiationMapSize;
			if (texture2D.width <= 512 && texture2D.height <= 512)
			{
				this.radiationMapSmall = new byte[texture2D.width * texture2D.height];
				if (texture2D.format == 4)
				{
					using (NativeArray<Color32> pixelData = texture2D.GetPixelData<Color32>(0))
					{
						for (int i = 0; i < pixelData.Length; i++)
						{
							this.radiationMapSmall[i] = pixelData[i].r;
						}
						goto IL_037A;
					}
				}
				using (NativeArray<TextureUtils.ColorARGB32> pixelData2 = texture2D.GetPixelData<TextureUtils.ColorARGB32>(0))
				{
					for (int j = 0; j < pixelData2.Length; j++)
					{
						this.radiationMapSmall[j] = pixelData2[j].r;
					}
					goto IL_037A;
				}
			}
			Log.Out("Radiation ignored {0}", new object[] { this.radiationMapSize });
			IL_037A:
			Object.Destroy(texture2D);
		}
		yield break;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void FillRadiationResult<T>(NativeArray<T> radPixs, byte[,][,] result, Func<T, byte> processColor) where T : struct
	{
		for (int i = 0; i < result.GetLength(0); i++)
		{
			for (int j = 0; j < result.GetLength(1); j++)
			{
				result[j, i] = new byte[512, 512];
				for (int k = 0; k < 512; k++)
				{
					for (int l = 0; l < 512; l++)
					{
						int num = (i * 512 + k) * this.radiationMapSize + (j * 512 + l);
						T t = radPixs[num];
						byte b = processColor(t);
						result[j, i][l, k] = b;
					}
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public unsafe void FillRadiationFileBackedArray<T>(NativeArray<T> radPixs, FileBackedArray<byte> fba, Func<T, byte> processColor) where T : struct
	{
		for (int i = 0; i < this.radiationTilesZ; i++)
		{
			for (int j = 0; j < this.radiationTilesX; j++)
			{
				int num = i * 512 * 512 * this.radiationTilesX + j * 512 * 512;
				int num2 = 0;
				Span<byte> span2;
				using (fba.GetSpan(num, 262144, out span2))
				{
					for (int k = 0; k < 512; k++)
					{
						for (int l = 0; l < 512; l++)
						{
							int num3 = (i * 512 + l) * this.radiationMapSize + (j * 512 + k);
							T t = radPixs[num3];
							byte b = processColor(t);
							*span2[num2] = b;
							num2++;
						}
					}
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public byte ProcessColor(Color32 pixel)
	{
		byte b = 0;
		if (pixel.g > 0)
		{
			b = 1;
		}
		if (pixel.b > 0)
		{
			b = 2;
		}
		if (pixel.r > 0)
		{
			b = 3;
		}
		return b;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public byte ProcessColor(TextureUtils.ColorARGB32 pixel)
	{
		byte b = 0;
		if (pixel.g > 0)
		{
			b = 1;
		}
		if (pixel.b > 0)
		{
			b = 2;
		}
		if (pixel.r > 0)
		{
			b = 3;
		}
		return b;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RadiationTileArrayFileFromTexture(FileBackedArray<byte> fba, Texture2D radiationTexture)
	{
		if (radiationTexture.format == 4)
		{
			using (NativeArray<Color32> pixelData = radiationTexture.GetPixelData<Color32>(0))
			{
				this.FillRadiationFileBackedArray<Color32>(pixelData, fba, new Func<Color32, byte>(this.ProcessColor));
				return;
			}
		}
		using (NativeArray<TextureUtils.ColorARGB32> pixelData2 = radiationTexture.GetPixelData<TextureUtils.ColorARGB32>(0))
		{
			this.FillRadiationFileBackedArray<TextureUtils.ColorARGB32>(pixelData2, fba, new Func<TextureUtils.ColorARGB32, byte>(this.ProcessColor));
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public byte[,][,] RadiationTileArrayFromTexture(Texture2D radiationTexture)
	{
		int num = radiationTexture.width / 512;
		int num2 = radiationTexture.height / 512;
		byte[,][,] array = new byte[num, num2][,];
		if (radiationTexture.format == 4)
		{
			using (NativeArray<Color32> pixelData = radiationTexture.GetPixelData<Color32>(0))
			{
				this.FillRadiationResult<Color32>(pixelData, array, new Func<Color32, byte>(this.ProcessColor));
				return array;
			}
		}
		using (NativeArray<TextureUtils.ColorARGB32> pixelData2 = radiationTexture.GetPixelData<TextureUtils.ColorARGB32>(0))
		{
			this.FillRadiationResult<TextureUtils.ColorARGB32>(pixelData2, array, new Func<TextureUtils.ColorARGB32, byte>(this.ProcessColor));
		}
		return array;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void worldCoordsToTileCoords(int worldX, int worldZ, TileAreaConfig tileAreaConfig, out int tileX, out int tileZ, out int posX, out int posZ)
	{
		tileX = (worldX + this.worldSizeHalf) / 512 + tileAreaConfig.tileStart.x;
		tileZ = (worldZ + this.worldSizeHalf) / 512 + tileAreaConfig.tileStart.y;
		posX = worldX - tileX * 512;
		posZ = worldZ - tileZ * 512;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public ITileArea<byte[,]> LoadRadiationMap(Texture2D radiationTex, TileAreaConfig tileAreaConfig)
	{
		byte[,][,] array = this.RadiationTileArrayFromTexture(radiationTex);
		return new TileArea<byte[,]>(tileAreaConfig, array);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public ITileArea<byte[,]> LoadRadiationMapToFile(Texture2D radiationTex, TileAreaConfig tileAreaConfig)
	{
		FileBackedArray<byte> fileBackedArray = new FileBackedArray<byte>(radiationTex.width * radiationTex.height);
		this.RadiationTileArrayFileFromTexture(fileBackedArray, radiationTex);
		TileFile<byte> tileFile = new TileFile<byte>(fileBackedArray, 512, this.radiationTilesX, this.radiationTilesZ);
		return new TileAreaCache<byte>(tileAreaConfig, tileFile, 9);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void loadSplatMaps(string _levelName, int _worldWidth)
	{
		string fullPath = PathAbstractions.WorldsSearchPaths.GetLocation(_levelName, null, null).FullPath;
		string text = fullPath + "/splat1.png";
		if (!SdFile.Exists(text))
		{
			return;
		}
		Texture2D texture2D = TextureUtils.LoadTexture(text, 0, false, false, null);
		Color32[] pixels = texture2D.GetPixels32();
		Color32[] array = null;
		Color32[] array2 = null;
		this.splatW = texture2D.width;
		int height = texture2D.height;
		this.splatScaleDiv = _worldWidth / this.splatW;
		if (Application.isEditor)
		{
			Object.DestroyImmediate(texture2D);
		}
		else
		{
			Object.Destroy(texture2D);
		}
		this.cntSplatChannels += 4;
		string text2 = fullPath + "/splat2.png";
		if (SdFile.Exists(text2))
		{
			Texture2D texture2D2 = TextureUtils.LoadTexture(text2, 0, false, false, null);
			array = texture2D2.GetPixels32();
			if (Application.isEditor)
			{
				Object.DestroyImmediate(texture2D2);
			}
			else
			{
				Object.Destroy(texture2D2);
			}
			this.cntSplatChannels += 4;
		}
		string text3 = fullPath + "/splat3.png";
		if (SdFile.Exists(text3))
		{
			Texture2D texture2D3 = TextureUtils.LoadTexture(text3, 0, false, false, null);
			array2 = texture2D3.GetPixels32();
			if (Application.isEditor)
			{
				Object.DestroyImmediate(texture2D3);
			}
			else
			{
				Object.Destroy(texture2D3);
			}
			this.cntSplatChannels += 4;
		}
		this.splatMapMaxValue = new byte[pixels.Length];
		Color32 color;
		color..ctor(0, 0, 0, 0);
		Color32 color2;
		color2..ctor(0, 0, 0, 0);
		Color32 color3;
		color3..ctor(0, 0, 0, 0);
		for (int i = pixels.Length - 1; i >= 0; i--)
		{
			color = pixels[i];
			if (this.cntSplatChannels > 4)
			{
				color2 = array[i];
			}
			if (this.cntSplatChannels > 8)
			{
				color3 = array2[i];
			}
			int num = 0;
			if (color.r >= color.g && color.r >= color.b && color.r >= color.a && color.r >= color2.r && color.r >= color2.g && color.r >= color2.b && color.r >= color2.a && color.r >= color3.r && color.r >= color3.g && color.r >= color3.b && color.r >= color3.a)
			{
				num = 0;
			}
			else if (color.g >= color.r && color.g >= color.b && color.g >= color.a && color.g >= color2.r && color.g >= color2.g && color.g >= color2.b && color.g >= color2.a && color.g >= color3.r && color.g >= color3.g && color.g >= color3.b && color.g >= color3.a)
			{
				num = 1;
			}
			else if (color.b >= color.r && color.b >= color.g && color.b >= color.a && color.b >= color2.r && color.b >= color2.g && color.b >= color2.b && color.b >= color2.a && color.b >= color3.r && color.b >= color3.g && color.b >= color3.b && color.b >= color3.a)
			{
				num = 2;
			}
			else if (color.a >= color.r && color.a >= color.g && color.a >= color.b && color.a >= color2.r && color.a >= color2.g && color.a >= color2.b && color.a >= color2.a && color.a >= color3.r && color.a >= color3.g && color.a >= color3.b && color.a >= color3.a)
			{
				num = 3;
			}
			else if (color2.r >= color2.g && color2.r >= color2.b && color2.r >= color2.a && color2.r >= color.r && color2.r >= color.g && color2.r >= color.b && color2.r >= color.a && color2.r >= color3.r && color2.r >= color3.g && color2.r >= color3.b && color2.r >= color3.a)
			{
				num = 4;
			}
			else if (color2.g >= color2.r && color2.g >= color2.b && color2.g >= color2.a && color2.g >= color.r && color2.g >= color.g && color2.g >= color.b && color2.g >= color.a && color2.g >= color3.r && color2.g >= color3.g && color2.g >= color3.b && color2.g >= color3.a)
			{
				num = 5;
			}
			else if (color2.b >= color2.r && color2.b >= color2.g && color2.b >= color2.a && color2.b >= color.r && color2.b >= color.g && color2.b >= color.b && color2.b >= color.a && color2.b >= color3.r && color2.b >= color3.g && color2.b >= color3.b && color2.b >= color3.a)
			{
				num = 6;
			}
			else if (color2.a >= color2.r && color2.a >= color2.g && color2.a >= color2.b && color2.a >= color.r && color2.a >= color.g && color2.a >= color.b && color2.a >= color.a && color2.a >= color3.r && color2.a >= color3.g && color2.a >= color3.b && color2.a >= color3.a)
			{
				num = 7;
			}
			else if (color3.r >= color3.g && color3.r >= color3.b && color3.r >= color3.a && color3.r >= color.r && color3.r >= color.g && color3.r >= color.b && color3.r >= color.a && color3.r >= color2.r && color3.r >= color2.g && color3.r >= color2.b && color3.r >= color2.a)
			{
				num = 8;
			}
			else if (color3.g >= color3.r && color3.g >= color3.b && color3.g >= color3.a && color3.g >= color.r && color3.g >= color.g && color3.g >= color.b && color3.g >= color.a && color3.g >= color2.r && color3.g >= color2.g && color3.g >= color2.b && color3.g >= color2.a)
			{
				num = 9;
			}
			else if (color3.b >= color3.r && color3.b >= color3.g && color3.b >= color3.a && color3.b >= color.r && color3.b >= color.g && color3.b >= color.b && color3.b >= color.a && color3.b >= color2.r && color3.b >= color2.g && color3.b >= color2.b && color3.b >= color3.a)
			{
				num = 10;
			}
			else if (color3.a >= color3.r && color3.a >= color3.g && color3.a >= color3.b && color3.a >= color.r && color3.a >= color.g && color3.a >= color.b && color3.a >= color.a && color3.a >= color2.r && color3.a >= color2.g && color3.a >= color2.b && color3.a >= color2.a)
			{
				num = 11;
			}
			this.splatMapMaxValue[i] = (byte)num;
		}
	}

	public string GetWorldName()
	{
		return this.worldName;
	}

	public void Init(int _seed, string _worldName, WorldBiomes _biomes, string _params1, string _params2)
	{
	}

	public BiomeDefinition GetBiomeAt(int x, int z, out float _intensity)
	{
		_intensity = 1f;
		return this.GetBiomeAt(x, z);
	}

	public BiomeDefinition GetBiomeAt(int x, int z)
	{
		if (this.biomesScaleDiv == 0)
		{
			return null;
		}
		int num = x / this.biomesScaleDiv + this.biomesMapWidthHalf;
		if (num < 0 || num >= this.m_BiomeMap.width)
		{
			return null;
		}
		int num2 = z / this.biomesScaleDiv + this.biomesMapHeightHalf;
		if (num2 < 0 || num2 >= this.m_BiomeMap.height)
		{
			return null;
		}
		byte value = this.m_BiomeMap.GetValue(num, num2);
		if (value == 255)
		{
			return null;
		}
		return this.m_Biomes.GetBiome(value);
	}

	public BiomeDefinition GetBiomeOrSubAt(int x, int z)
	{
		BiomeDefinition biomeDefinition = this.GetBiomeAt(x, z);
		if (biomeDefinition != null)
		{
			int subBiomeIdxAt = this.GetSubBiomeIdxAt(biomeDefinition, x, 0, z);
			if (subBiomeIdxAt >= 0)
			{
				biomeDefinition = biomeDefinition.subbiomes[subBiomeIdxAt];
			}
		}
		return biomeDefinition;
	}

	public int GetSubBiomeIdxAt(BiomeDefinition bd, int _x, int _y, int _z)
	{
		double num = -3.4028234663852886E+38;
		double num2 = 0.0;
		Vector2 vector = Vector2.zero;
		for (int i = 0; i < bd.subbiomes.Count; i++)
		{
			BiomeDefinition biomeDefinition = bd.subbiomes[i];
			if (num != (double)biomeDefinition.noiseFreq || vector != biomeDefinition.noiseOffset)
			{
				num = (double)biomeDefinition.noiseFreq;
				vector = biomeDefinition.noiseOffset;
				num2 = (double)((float)this.noiseGen.FBM((double)((float)_x + vector.x), (double)((float)_z + vector.y), num));
				num2 = num2 * 0.5 + 0.5;
			}
			if (num2 >= (double)biomeDefinition.noiseMin && num2 < (double)biomeDefinition.noiseMax)
			{
				return i;
			}
		}
		return -1;
	}

	public Vector2i GetSize()
	{
		return new Vector2i(this.biomeMapWidth, this.biomeMapHeight);
	}

	public float GetHumidityAt(int x, int z)
	{
		return 0f;
	}

	public float GetTemperatureAt(int x, int z)
	{
		return 0f;
	}

	public float GetRadiationAt(int x, int z)
	{
		if (this.radiationMapSmall == null)
		{
			return 0f;
		}
		int num = (x + this.worldSizeHalf) / this.radiationMapScale + (z + this.worldSizeHalf) / this.radiationMapScale * this.radiationMapSize;
		if (num >= 0 && num < this.radiationMapSmall.Length)
		{
			return (float)this.radiationMapSmall[num];
		}
		return 0f;
	}

	public BlockValue GetTopmostBlockValue(int xWorld, int zWorld)
	{
		this.bvReturn.type = 0;
		if (xWorld < -this.worldSizeHalf || xWorld >= this.worldSizeHalf || zWorld < -this.worldSizeHalf || zWorld >= this.worldSizeHalf)
		{
			return this.bvReturn;
		}
		if (this.cntSplatChannels > 0)
		{
			switch (this.splatMapMaxValue[(xWorld + this.worldSizeHalf) / this.splatScaleDiv + (zWorld + this.worldSizeHalf) / this.splatScaleDiv * this.splatW])
			{
			case 0:
				this.bvReturn.type = 14;
				break;
			case 1:
				this.bvReturn.type = 1;
				break;
			case 2:
				this.bvReturn.type = 9;
				break;
			case 3:
				this.bvReturn.type = 8;
				break;
			case 4:
				this.bvReturn.type = 12;
				break;
			case 5:
				this.bvReturn.type = 13;
				break;
			case 6:
				this.bvReturn.type = 16;
				break;
			case 7:
				this.bvReturn.type = 11;
				break;
			case 8:
				this.bvReturn.type = 3;
				break;
			case 9:
				this.bvReturn.type = 29;
				break;
			case 10:
				this.bvReturn.type = 28;
				break;
			case 11:
				this.bvReturn.type = 2;
				break;
			}
		}
		return this.bvReturn;
	}

	public void Cleanup()
	{
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public GridCompressedData<byte> m_BiomeMap;

	[PublicizedFrom(EAccessModifier.Private)]
	public WorldBiomes m_Biomes;

	[PublicizedFrom(EAccessModifier.Private)]
	public PerlinNoise noiseGen;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly string worldName;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly string worldPath;

	[PublicizedFrom(EAccessModifier.Private)]
	public int biomeMapWidth;

	[PublicizedFrom(EAccessModifier.Private)]
	public int biomesMapWidthHalf;

	[PublicizedFrom(EAccessModifier.Private)]
	public int biomeMapHeight;

	[PublicizedFrom(EAccessModifier.Private)]
	public int biomesMapHeightHalf;

	[PublicizedFrom(EAccessModifier.Private)]
	public int biomesScaleDiv;

	[PublicizedFrom(EAccessModifier.Private)]
	public int radiationMapSize;

	[PublicizedFrom(EAccessModifier.Private)]
	public int radiationMapScale;

	[PublicizedFrom(EAccessModifier.Private)]
	public byte[] radiationMapSmall;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cRadiationTileSize = 512;

	[PublicizedFrom(EAccessModifier.Private)]
	public int radiationTilesX;

	[PublicizedFrom(EAccessModifier.Private)]
	public int radiationTilesZ;

	[PublicizedFrom(EAccessModifier.Private)]
	public byte[] splatMapMaxValue;

	[PublicizedFrom(EAccessModifier.Private)]
	public int cntSplatChannels;

	[PublicizedFrom(EAccessModifier.Private)]
	public int splatScaleDiv;

	[PublicizedFrom(EAccessModifier.Private)]
	public int splatW;

	[PublicizedFrom(EAccessModifier.Private)]
	public int worldSize;

	[PublicizedFrom(EAccessModifier.Private)]
	public int worldSizeHalf;

	[PublicizedFrom(EAccessModifier.Private)]
	public BlockValue bvReturn = BlockValue.Air;
}
