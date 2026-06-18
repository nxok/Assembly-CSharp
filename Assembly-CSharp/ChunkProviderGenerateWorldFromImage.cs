using System;
using System.Collections;
using System.Diagnostics;
using UnityEngine;

public class ChunkProviderGenerateWorldFromImage : ChunkProviderGenerateWorld
{
	public ChunkProviderGenerateWorldFromImage(string _levelName, PathAbstractions.AbstractedLocation _worldLocation, bool _bLoadDTM = true)
		: base(_levelName, _worldLocation, false)
	{
		this.bLoadDTM = _bLoadDTM;
	}

	public override void Cleanup()
	{
		HeightMap heightMap = this.heightMap;
		if (heightMap != null)
		{
			heightMap.Dispose();
		}
		this.heightMap = null;
		IBackedArray<ushort> backedArray = this.rawdata;
		if (backedArray != null)
		{
			backedArray.Dispose();
		}
		this.rawdata = null;
		base.Cleanup();
	}

	public override IEnumerator Init(World _world)
	{
		if (this.bLoadDTM)
		{
			yield return this.<>n__0(_world);
		}
		else
		{
			this.world = _world;
		}
		Stopwatch sw = new Stopwatch();
		sw.Start();
		if (this.bLoadDTM)
		{
			this.loadDTM();
		}
		Log.Out("Loading and parsing of dtm.png took " + sw.ElapsedMilliseconds.ToString() + "ms");
		sw.Reset();
		sw.Start();
		HeightMap heightMap = this.heightMap;
		if (heightMap != null)
		{
			heightMap.Dispose();
		}
		this.heightMap = new HeightMap(this.m_Dtm.DimX, this.m_Dtm.DimY, 255f, this.rawdata, 0);
		this.m_BiomeProvider = new WorldBiomeProviderFromImage(this.levelName, this.worldLocation.FullPath, this.world.Biomes, 4096);
		WorldDecoratorPOIFromImage worldDecoratorPOIFromImage = new WorldDecoratorPOIFromImage(this.levelName, this.worldLocation.FullPath, this.GetDynamicPrefabDecorator(), this.m_Dtm.DimX, this.m_Dtm.DimY, null, false, 1, this.heightMap, null);
		this.m_Decorators.Add(worldDecoratorPOIFromImage);
		yield return worldDecoratorPOIFromImage.InitData();
		this.m_Decorators.Add(new WorldDecoratorBlocksFromBiome(this.m_BiomeProvider, this.GetDynamicPrefabDecorator()));
		Log.Out("Loading and parsing of generator took " + sw.ElapsedMilliseconds.ToString() + "ms");
		sw.Reset();
		sw.Start();
		if (this.bLoadDTM)
		{
			this.m_TerrainGenerator = new TerrainFromDTM();
			((TerrainFromDTM)this.m_TerrainGenerator).Init(this.m_Dtm, this.m_BiomeProvider, this.levelName, this.worldLocation.FullPath, this.world.Seed);
			string text = (_world.IsEditor() ? null : GameIO.GetSaveGameRegionDir());
			this.m_RegionFileManager = new RegionFileManager(text, text, 0, !_world.IsEditor());
			MultiBlockManager.Instance.Initialize(this.m_RegionFileManager);
		}
		yield return null;
		yield break;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public unsafe void loadDTM()
	{
		Texture2D texture2D = null;
		if (SdFile.Exists(this.worldLocation.FullPath + "/dtm.png"))
		{
			texture2D = TextureUtils.LoadTexture(this.worldLocation.FullPath + "/dtm.png", 0, false, false, null);
			Log.Out("Loading local DTM");
		}
		else if (SdFile.Exists(this.worldLocation.FullPath + "/dtm.tga"))
		{
			texture2D = TextureUtils.LoadTexture(this.worldLocation.FullPath + "/dtm.tga", 0, false, false, null);
			Log.Out("Loading local DTM");
		}
		else
		{
			texture2D = Resources.Load("Data/Worlds/" + this.levelName + "/dtm", typeof(Texture2D)) as Texture2D;
		}
		Log.Out("DTM image size w= " + texture2D.width.ToString() + ", h = " + texture2D.height.ToString());
		Color[] pixels = texture2D.GetPixels();
		this.m_Dtm = new ArrayWithOffset<byte>(texture2D.width, texture2D.height);
		IBackedArray<ushort> backedArray = this.rawdata;
		if (backedArray != null)
		{
			backedArray.Dispose();
		}
		this.rawdata = BackedArrays.Create<ushort>(pixels.Length);
		int width = texture2D.width;
		int height = texture2D.height;
		for (int i = 0; i < height; i++)
		{
			Span<ushort> span2;
			using (this.rawdata.GetSpan(i * width, width, out span2))
			{
				for (int j = 0; j < width; j++)
				{
					this.m_Dtm[j + this.m_Dtm.MinPos.x, i + this.m_Dtm.MinPos.y] = (byte)(pixels[width * i + j].grayscale * 255f);
					*span2[j] = (ushort)((int)((byte)(pixels[width * i + j].grayscale * 255f)) | ((int)((byte)((pixels[width * i + j].grayscale * 255f - 255f) * 255f)) << 8));
				}
			}
		}
		Resources.UnloadAsset(texture2D);
	}

	public override void ReloadAllChunks()
	{
		this.loadDTM();
		((TerrainFromDTM)this.m_TerrainGenerator).Init(this.m_Dtm, this.m_BiomeProvider, this.levelName, this.worldLocation.FullPath, this.world.Seed);
		base.ReloadAllChunks();
	}

	public override EnumChunkProviderId GetProviderId()
	{
		return EnumChunkProviderId.GenerateFromDtm;
	}

	public override bool GetWorldExtent(out Vector3i _minSize, out Vector3i _maxSize)
	{
		_minSize = new Vector3i(this.m_Dtm.MinPos.x, 0, this.m_Dtm.MinPos.y);
		_maxSize = new Vector3i(this.m_Dtm.MaxPos.x, 255, this.m_Dtm.MaxPos.y);
		return true;
	}

	public override int GetPOIBlockIdOverride(int _x, int _z)
	{
		WorldDecoratorPOIFromImage worldDecoratorPOIFromImage = (WorldDecoratorPOIFromImage)this.m_Decorators[0];
		WorldGridCompressedData<byte> poi = worldDecoratorPOIFromImage.m_Poi;
		int num = _x / worldDecoratorPOIFromImage.worldScale;
		int num2 = _z / worldDecoratorPOIFromImage.worldScale;
		if (!poi.Contains(num, num2))
		{
			return 0;
		}
		byte data = poi.GetData(num, num2);
		if (data == 0 || data == 255)
		{
			return 0;
		}
		PoiMapElement poiForColor = this.world.Biomes.getPoiForColor((uint)data);
		if (poiForColor == null)
		{
			return 0;
		}
		return poiForColor.m_BlockValue.type;
	}

	public override float GetPOIHeightOverride(int x, int z)
	{
		WorldDecoratorPOIFromImage worldDecoratorPOIFromImage = (WorldDecoratorPOIFromImage)this.m_Decorators[0];
		WorldGridCompressedData<byte> poi = worldDecoratorPOIFromImage.m_Poi;
		byte data;
		if (!poi.Contains(x / worldDecoratorPOIFromImage.worldScale, z / worldDecoratorPOIFromImage.worldScale) || (data = poi.GetData(x / worldDecoratorPOIFromImage.worldScale, z / worldDecoratorPOIFromImage.worldScale)) == 255 || data == 0)
		{
			return 0f;
		}
		PoiMapElement poiForColor = this.world.Biomes.getPoiForColor((uint)data);
		if (poiForColor == null)
		{
			return 0f;
		}
		if (!poiForColor.m_BlockValue.Block.blockMaterial.IsLiquid)
		{
			return 0f;
		}
		return (float)poiForColor.m_YPosFill;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public ArrayWithOffset<byte> m_Dtm;

	public IBackedArray<ushort> rawdata;

	[PublicizedFrom(EAccessModifier.Private)]
	public HeightMap heightMap;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bLoadDTM;
}
