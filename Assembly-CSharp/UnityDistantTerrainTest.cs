using System;
using UnityEngine;

public class UnityDistantTerrainTest
{
	public static void Create()
	{
		UnityDistantTerrainTest.Instance = new UnityDistantTerrainTest();
	}

	public void LoadTerrain()
	{
		if (!this.parentObj)
		{
			this.parentObj = new GameObject("DistantUnityTerrain");
		}
		UnityDistantTerrain.Config config = new UnityDistantTerrain.Config
		{
			DataWidth = this.hmWidth,
			DataHeight = this.hmHeight,
			DataTileSize = 512,
			DataSteps = 2,
			SplatSteps = 1,
			MetersPerHeightPix = 2,
			MaxHeight = 256,
			PixelError = 5
		};
		int num = Mathf.CeilToInt((float)(Mathf.Clamp(GamePrefs.GetInt(EnumGamePrefs.OptionsGfxTerrainQuality), 2, 4) * 512) / (float)config.ChunkWorldSize * 2f) + 1;
		int num2 = config.DataWidth / config.DataTileSize;
		int num3 = config.DataHeight / config.DataTileSize;
		TileAreaConfig tileAreaConfig = new TileAreaConfig
		{
			tileStart = new Vector2i(-num2 / 2, -num3 / 2),
			tileEnd = new Vector2i(Utils.FastMax(0, num2 / 2 - 1), Utils.FastMax(0, num3 / 2 - 1)),
			tileSizeInWorldUnits = config.ChunkWorldSize,
			bWrapAroundX = false,
			bWrapAroundZ = false
		};
		TileAreaConfig tileAreaConfig2 = tileAreaConfig;
		tileAreaConfig2.tileSizeInWorldUnits = config.DataTileSize;
		ITileArea<float[,]> tileArea = this.LoadTerrainHeightTiles(tileAreaConfig, this.HeightMap, config.DataWidth, config.DataHeight, config.DataTileSize);
		this.unityTerrain = new UnityDistantTerrain();
		this.unityTerrain.Init(this.parentObj, config, num, this.TerrainMaterial, this.WaterMaterial, this.WaterChunks16x16Width, this.WaterChunks16x16, tileArea, null, null, null);
	}

	public ITileArea<float[,]> LoadTerrainHeightTiles(TileAreaConfig _config, IBackedArray<ushort> _rawHeightMap, int _heightMapWidth, int _heightMapHeight, int _sliceAtPix)
	{
		if (PlatformOptimizations.FileBackedTerrainTiles)
		{
			TileFile<float> tileFile = HeightMapUtils.ConvertAndSliceUnityHeightmapQuarteredToFile(_rawHeightMap, _heightMapWidth, _heightMapHeight, _sliceAtPix);
			return new TileAreaCache<float>(_config, tileFile, 9);
		}
		float[,][,] array = HeightMapUtils.ConvertAndSliceUnityHeightmapQuartered(_rawHeightMap, _heightMapWidth, _heightMapHeight, _sliceAtPix);
		return new TileArea<float[,]>(_config, array);
	}

	public void FrameUpdate(EntityPlayerLocal _player)
	{
		if (this.unityTerrain != null)
		{
			this.unityTerrain.FrameUpdate(_player);
		}
	}

	public void OnChunkVisible(int _chunkX, int _chunkZ, bool _visible)
	{
		if (this.unityTerrain != null)
		{
			this.unityTerrain.OnChunkUpdate(_chunkX, _chunkZ, _visible);
		}
	}

	public void Cleanup()
	{
		if (this.unityTerrain != null)
		{
			this.unityTerrain.Cleanup();
			this.unityTerrain = null;
		}
		Object.Destroy(this.parentObj);
	}

	public const string cGameObjectName = "DistantUnityTerrain";

	public static UnityDistantTerrainTest Instance;

	public UnityDistantTerrain unityTerrain;

	public IBackedArray<ushort> HeightMap;

	public int hmWidth = 6144;

	public int hmHeight = 6144;

	public Material TerrainMaterial;

	public Material WaterMaterial;

	public int WaterChunks16x16Width;

	public byte[] WaterChunks16x16;

	public GameObject parentObj;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cTileSize = 512;
}
