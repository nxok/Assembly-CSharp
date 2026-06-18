using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using Platform;
using Unity.Burst;
using Unity.Collections;
using UnityEngine;

[BurstCompile(CompileSynchronously = true)]
public class ChunkProviderGenerateWorldFromRaw : ChunkProviderGenerateWorld
{
	public ChunkProviderGenerateWorldFromRaw(string _levelName, PathAbstractions.AbstractedLocation _worldLocation, bool _bClientMode = false, bool _bFixedWaterLevel = false)
		: base(_levelName, _worldLocation, _bClientMode)
	{
		this.bFixedWaterLevel = _bFixedWaterLevel;
	}

	public override IEnumerator Init(World _world)
	{
		ChunkProviderGenerateWorldFromRaw.<>c__DisplayClass17_0 CS$<>8__locals1 = new ChunkProviderGenerateWorldFromRaw.<>c__DisplayClass17_0();
		yield return this.<>n__0(_world);
		this.world = _world;
		string worldPath = this.worldLocation.FullPath;
		base.WorldInfo = GameUtils.WorldInfo.LoadWorldInfo(this.worldLocation);
		if (base.WorldInfo != null)
		{
			this.heightMapWidth = base.WorldInfo.HeightmapSize.x;
			this.heightMapHeight = base.WorldInfo.HeightmapSize.y;
			this.heightMapScale = base.WorldInfo.Scale;
			this.bFixedWaterLevel = base.WorldInfo.FixedWaterLevel;
		}
		MicroStopwatch ms = new MicroStopwatch();
		string dtmFilename = this.getFilenameDTM();
		if (!SdFile.Exists(dtmFilename + ".raw"))
		{
			if (SdFile.Exists(dtmFilename + ".tga"))
			{
				int num;
				int num2;
				Color32[] array = TGALoader.LoadTGAAsArray(dtmFilename + ".tga", out num, out num2, null);
				if (array != null)
				{
					float[,] array2 = HeightMapUtils.ConvertDTMToHeightData(array, this.heightMapWidth, this.heightMapHeight, true);
					HeightMapUtils.SaveHeightMapRAW(this.getFilenameDTM() + ".raw", this.heightMapWidth, this.heightMapHeight, array2);
					HeightMapUtils.SaveHeightMapRAW(dtmFilename + ".raw", this.heightMapWidth, this.heightMapHeight, array2);
				}
				Log.Out("GenWorldFromRaw tga to dtm took " + ms.ElapsedMilliseconds.ToString() + "ms");
				ms.ResetAndRestart();
			}
			else
			{
				if (!SdFile.Exists(dtmFilename + ".png"))
				{
					throw new FileNotFoundException(string.Format("No height data found for world '{0}'", this.worldLocation));
				}
				float[,] heightDataFromImageFile = HeightMapUtils.GetHeightDataFromImageFile(dtmFilename + ".png");
				HeightMapUtils.SmoothTerrain(7, heightDataFromImageFile);
				HeightMapUtils.SaveHeightMapRAW(dtmFilename + ".raw", this.heightMapWidth, this.heightMapHeight, heightDataFromImageFile);
				Log.Out("GenWorldFromRaw png to dtm took " + ms.ElapsedMilliseconds.ToString() + "ms");
				ms.ResetAndRestart();
			}
		}
		yield return this.calcWorldFileCrcs(worldPath);
		CS$<>8__locals1.worldDecoratorPoiFromImage = null;
		CS$<>8__locals1.splat3Tex = null;
		CS$<>8__locals1.splat4Tex = null;
		CS$<>8__locals1.splat3Half = null;
		CS$<>8__locals1.splat4Half = null;
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer && this.filesNeedProcessing(worldPath, dtmFilename))
		{
			yield return this.processFiles(worldPath, delegate(Texture2D _splat3Tex, Texture2D _splat3Half, Texture2D _splat4Tex, Texture2D _splat4Half, WorldDecoratorPOIFromImage _poiFromImage)
			{
				CS$<>8__locals1.splat3Tex = _splat3Tex;
				CS$<>8__locals1.splat3Half = _splat3Half;
				CS$<>8__locals1.splat4Tex = _splat4Tex;
				CS$<>8__locals1.splat4Half = _splat4Half;
				CS$<>8__locals1.worldDecoratorPoiFromImage = _poiFromImage;
			});
		}
		else
		{
			XUiC_ProgressWindow.SetText(LocalPlayerUI.primaryUI, Localization.Get("uiLoadHeightmap", false, null), true);
			this.loadDTM(null);
			Log.Out("GenWorldFromRaw load dtm took " + ms.ElapsedMilliseconds.ToString() + "ms");
			ms.ResetAndRestart();
		}
		XUiC_ProgressWindow.SetText(LocalPlayerUI.primaryUI, Localization.Get("uiLoadBiomes", false, null), true);
		this.m_BiomeProvider = new WorldBiomeProviderFromImage(this.levelName, this.worldLocation.FullPath, this.world.Biomes, this.heightMapWidth * this.heightMapScale);
		yield return this.m_BiomeProvider.InitData();
		Log.Out("GenWorldFromRaw biomes took " + ms.ElapsedMilliseconds.ToString() + "ms");
		yield return GCUtils.UnloadAndCollectCo();
		ms.ResetAndRestart();
		XUiC_ProgressWindow.SetText(LocalPlayerUI.primaryUI, Localization.Get("uiLoadSplats", false, null), true);
		string text = worldPath + "/splat3_processed.png";
		if (SdFile.Exists(text) && CS$<>8__locals1.splat3Tex == null)
		{
			CS$<>8__locals1.splat3Tex = TextureUtils.LoadTexture(text, 0, false, false, null);
		}
		yield return null;
		text = worldPath + "/splat4_processed.png";
		if (SdFile.Exists(text) && CS$<>8__locals1.splat4Tex == null)
		{
			CS$<>8__locals1.splat4Tex = TextureUtils.LoadTexture(text, 0, false, false, null);
		}
		int num3 = this.GetWorldSize().x / 8;
		int num4 = this.GetWorldSize().y / 8;
		this.procBiomeMask1 = new Texture2D(num3, num4, 4, false);
		this.procBiomeMask2 = new Texture2D(num3, num4, 4, false);
		NativeArray<Color32> pixelData = this.procBiomeMask1.GetPixelData<Color32>(0);
		NativeArray<Color32> pixelData2 = this.procBiomeMask2.GetPixelData<Color32>(0);
		Vector3i vector3i;
		Vector3i vector3i2;
		this.GetWorldExtent(out vector3i, out vector3i2);
		for (int i = vector3i.z; i < vector3i2.z; i += 8)
		{
			int num5 = (i - vector3i.z) / 8 * num3;
			for (int j = vector3i.x; j < vector3i2.x; j += 8)
			{
				BiomeDefinition biomeAt = this.m_BiomeProvider.GetBiomeAt(j, i);
				if (biomeAt != null)
				{
					Color32 color = default(Color32);
					Color32 color2 = default(Color32);
					int num6 = (j - vector3i.x) / 8 + num5;
					switch (biomeAt.m_Id)
					{
					case 1:
						color.r = byte.MaxValue;
						break;
					case 3:
						color.g = byte.MaxValue;
						break;
					case 5:
						color2.r = byte.MaxValue;
						break;
					case 8:
						color.a = byte.MaxValue;
						break;
					case 9:
						color.b = byte.MaxValue;
						break;
					}
					pixelData[num6] = color;
					pixelData2[num6] = color2;
				}
			}
		}
		yield return null;
		this.procBiomeMask1.filterMode = 1;
		this.procBiomeMask1.Apply(false, true);
		this.procBiomeMask2.filterMode = 1;
		this.procBiomeMask2.Apply(false, true);
		Log.Out("GenWorldFromRaw shader control textures took " + ms.ElapsedMilliseconds.ToString() + "ms");
		ms.ResetAndRestart();
		this.m_TerrainGenerator = new TerrainFromRaw();
		if (this.heightMap == null)
		{
			this.heightMap = new HeightMap(this.heightMapWidth, this.heightMapHeight, 255f, this.heightData, this.heightMapWidth * this.heightMapScale);
		}
		yield return null;
		if (CS$<>8__locals1.worldDecoratorPoiFromImage == null)
		{
			ChunkProviderGenerateWorldFromRaw.<>c__DisplayClass17_0 CS$<>8__locals2 = CS$<>8__locals1;
			string levelName = this.levelName;
			string fullPath = this.worldLocation.FullPath;
			DynamicPrefabDecorator dynamicPrefabDecorator = this.GetDynamicPrefabDecorator();
			int num7 = this.heightMapWidth;
			int num8 = this.heightMapHeight;
			Texture2D splat3Tex = CS$<>8__locals1.splat3Tex;
			bool flag = false;
			Texture2D splat4Tex = CS$<>8__locals1.splat4Tex;
			CS$<>8__locals2.worldDecoratorPoiFromImage = new WorldDecoratorPOIFromImage(levelName, fullPath, dynamicPrefabDecorator, num7, num8, splat3Tex, flag, this.heightMapScale, this.heightMap, splat4Tex);
			this.m_Decorators.Add(CS$<>8__locals1.worldDecoratorPoiFromImage);
			yield return CS$<>8__locals1.worldDecoratorPoiFromImage.InitData();
		}
		this.m_Decorators.Add(new WorldDecoratorBlocksFromBiome(this.m_BiomeProvider, this.GetDynamicPrefabDecorator()));
		yield return null;
		this.poiFromImage = this.m_Decorators[0] as WorldDecoratorPOIFromImage;
		((TerrainFromRaw)this.m_TerrainGenerator).Init(this.heightMap, this.m_BiomeProvider, this.world.Seed);
		yield return null;
		string text2 = (_world.IsEditor() ? null : GameIO.GetSaveGameRegionDir());
		if (!this.bClientMode)
		{
			this.m_RegionFileManager = new RegionFileManager(text2, text2, 0, !_world.IsEditor());
			this.eventPrefabs = new EventPrefabs(_world, this.prefabDecorator, this.m_RegionFileManager);
		}
		MultiBlockManager.Instance.Initialize(this.m_RegionFileManager);
		Log.Out("GenWorldFromRaw misc took " + ms.ElapsedMilliseconds.ToString() + "ms");
		ms.ResetAndRestart();
		yield return null;
		if (GameOptionsManager.GetTextureQuality(-1) > 0)
		{
			Object.Destroy(CS$<>8__locals1.splat3Tex);
			Object.Destroy(CS$<>8__locals1.splat4Tex);
			text = worldPath + "/splat3_half.png";
			if (SdFile.Exists(text))
			{
				if (CS$<>8__locals1.splat3Half == null)
				{
					CS$<>8__locals1.splat3Half = TextureUtils.LoadTexture(text, 0, false, false, null);
				}
				yield return null;
				this.splats[0] = CS$<>8__locals1.splat3Half;
				this.splats[0].filterMode = 1;
			}
			yield return null;
			text = worldPath + "/splat4_half.png";
			if (SdFile.Exists(text))
			{
				if (CS$<>8__locals1.splat4Half == null)
				{
					CS$<>8__locals1.splat4Half = TextureUtils.LoadTexture(text, 0, false, false, null);
				}
				yield return null;
				this.splats[1] = CS$<>8__locals1.splat4Half;
				this.splats[1].filterMode = 1;
			}
		}
		else
		{
			Object.Destroy(CS$<>8__locals1.splat3Half);
			Object.Destroy(CS$<>8__locals1.splat4Half);
			this.splats[0] = CS$<>8__locals1.splat3Tex;
			this.splats[0].filterMode = 1;
			this.splats[1] = CS$<>8__locals1.splat4Tex;
			this.splats[1].filterMode = 1;
		}
		yield return null;
		Texture2D tex = this.splats[0];
		if (!(DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5).IsCurrent())
		{
			Texture2D tex2 = new Texture2D(tex.width, tex.height, 2, false);
			yield return ChunkProviderGenerateWorldFromRaw.RoadSmooth(tex, tex2);
			this.splats[0] = tex2;
			Object.Destroy(tex);
			tex2 = null;
		}
		else
		{
			tex.Compress(true);
		}
		tex.Apply(false, true);
		yield return null;
		this.splats[1].Compress(true);
		this.splats[1].Apply(false, true);
		Log.Out("GenWorldFromRaw splats took " + ms.ElapsedMilliseconds.ToString() + "ms");
		ms.ResetAndRestart();
		yield return GCUtils.UnloadAndCollectCo();
		yield break;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Color32 ARGB32ToColor(TextureUtils.ColorARGB32 _color)
	{
		return new Color32(_color.r, _color.g, _color.b, _color.a);
	}

	public static IEnumerator RoadSmooth(Texture2D _tex, Texture2D _smoothedTex)
	{
		int width = _tex.width;
		int height = _tex.height;
		int num = width * height;
		NativeArray<TextureUtils.ColorARGB32> pixelData = _tex.GetPixelData<TextureUtils.ColorARGB32>(0);
		NativeArray<Color32> colorsNA = new NativeArray<Color32>(num, 4, 0);
		for (int i = 0; i < num; i++)
		{
			colorsNA[i] = ChunkProviderGenerateWorldFromRaw.ARGB32ToColor(pixelData[i]);
		}
		NativeArray<Color32> smoothColorsNA = new NativeArray<Color32>(num, 4, 0);
		int num2;
		for (int pass = 2; pass > 0; pass = num2)
		{
			ChunkProviderGenerateWorldFromRaw.RoadSmooth(in colorsNA, ref smoothColorsNA, width, height);
			if (pass > 1)
			{
				smoothColorsNA.CopyTo(colorsNA);
			}
			yield return null;
			num2 = pass - 1;
		}
		Color32[] array = smoothColorsNA.ToArray();
		_smoothedTex.SetPixels32(array, 0);
		_smoothedTex.Apply();
		colorsNA.Dispose();
		smoothColorsNA.Dispose();
		yield return null;
		yield break;
	}

	[BurstCompile(CompileSynchronously = true)]
	public static void RoadSmooth(in NativeArray<Color32> _colors, ref NativeArray<Color32> _colorsOut, int width, int height)
	{
		ChunkProviderGenerateWorldFromRaw.RoadSmooth_00005653$BurstDirectCall.Invoke(in _colors, ref _colorsOut, width, height);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void loadDTM(string _dtmFilename = null)
	{
		if (_dtmFilename == null)
		{
			_dtmFilename = this.getFilenameDTM();
		}
		IBackedArray<ushort> backedArray = this.heightData;
		if (backedArray != null)
		{
			backedArray.Dispose();
		}
		this.heightData = HeightMapUtils.LoadHeightMapRAW(_dtmFilename + ".raw", this.heightMapWidth, this.heightMapHeight, 1f, 250);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool filesNeedProcessing(string _worldPath, string _dtmFilename)
	{
		return !_dtmFilename.EndsWith("_processed") || !SdFile.Exists(_worldPath + "/splat3_processed.png") || !SdFile.Exists(_worldPath + "/splat4_processed.png") || !SdFile.Exists(_worldPath + "/splat3_half.png") || !SdFile.Exists(_worldPath + "/splat4_half.png") || !this.verifyFileHashes(_worldPath);
	}

	public long worldFileTotalSize
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator calcWorldFileCrcs(string _worldPath)
	{
		MicroStopwatch msw = new MicroStopwatch(true);
		this.worldFileTotalSize = 0L;
		this.worldFileCrcs.Clear();
		byte[] buffer = new byte[32768];
		string[] array = SdDirectory.GetFiles(_worldPath);
		for (int i = 0; i < array.Length; i++)
		{
			string text = array[i];
			string filename = GameIO.GetFilenameFromPath(text);
			this.worldFileTotalSize += GameIO.FileSize(text);
			yield return IOUtils.CalcCrcCoroutine(text, delegate(uint _hash)
			{
				this.worldFileCrcs.Add(filename, _hash);
			}, Constants.cMaxLoadTimePerFrameMillis, buffer);
		}
		array = null;
		Log.Out("Calculating world hashes took {0} ms (world size {1} MiB)", new object[]
		{
			msw.ElapsedMilliseconds,
			this.worldFileTotalSize / 1024L / 1024L
		});
		yield break;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void saveFileHashes(string _worldPath, Dictionary<string, uint> _worldFileCrcs)
	{
		using (Stream stream = SdFile.Open(_worldPath + "/checksums.txt", FileMode.Create, FileAccess.Write))
		{
			using (StreamWriter streamWriter = new StreamWriter(stream, Encoding.UTF8))
			{
				foreach (KeyValuePair<string, uint> keyValuePair in this.worldFileCrcs)
				{
					streamWriter.Write(keyValuePair.Key);
					streamWriter.Write("=");
					streamWriter.WriteLine(keyValuePair.Value);
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<string, uint> loadStoredFileHashes(string _worldPath)
	{
		if (!SdFile.Exists(_worldPath + "/checksums.txt"))
		{
			return null;
		}
		Dictionary<string, uint> dictionary = new CaseInsensitiveStringDictionary<uint>();
		using (StreamReader streamReader = SdFile.OpenText(_worldPath + "/checksums.txt"))
		{
			string text;
			while ((text = streamReader.ReadLine()) != null)
			{
				string[] array = text.Split('=', StringSplitOptions.None);
				if (array.Length != 2)
				{
					Log.Warning("Invalid line in checksums.txt: {0}", new object[] { text });
				}
				else
				{
					string text2 = array[0];
					uint num = StringParsers.ParseUInt32(array[1], 0, -1, NumberStyles.Integer);
					dictionary.Add(text2, num);
				}
			}
		}
		return dictionary;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool verifyFileHashes(string _worldPath)
	{
		Dictionary<string, uint> dictionary = this.loadStoredFileHashes(_worldPath);
		if (dictionary == null)
		{
			Log.Warning("No hashes for world");
			return false;
		}
		foreach (string text in ChunkProviderGenerateWorldFromRaw.FilesUsedForProcessing)
		{
			if (!dictionary.ContainsKey(text) && SdFile.Exists(_worldPath + "/" + text))
			{
				Log.Warning("Missing hash for " + text);
				return false;
			}
			if (this.worldFileCrcs.ContainsKey(text) && this.worldFileCrcs[text] != dictionary[text])
			{
				return false;
			}
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator processFiles(string _worldPath, ChunkProviderGenerateWorldFromRaw.ProcessFilesCallback _callback)
	{
		XUiC_ProgressWindow.SetText(LocalPlayerUI.primaryUI, Localization.Get("uiLoadProcessWorldHeightmap", false, null), true);
		MicroStopwatch yieldMs = new MicroStopwatch(true);
		MicroStopwatch ms = new MicroStopwatch(true);
		Log.Out("Processing world files");
		string processedFileName = _worldPath + "/dtm_processed.raw";
		this.loadDTM(_worldPath + "/dtm");
		ushort[] topTexMap = new ushort[this.heightMapWidth * this.heightMapScale * this.heightMapHeight * this.heightMapScale];
		yield return this.GetDynamicPrefabDecorator().CopyWorldPrefabHeightsIntoHeightMap(this.heightMapWidth, this.heightMapHeight, this.heightData, this.heightMapScale, topTexMap);
		ThreadManager.AddSingleTask(delegate(ThreadManager.TaskInfo _taskInfo)
		{
			HeightMapUtils.SaveHeightMapRAW(processedFileName, this.heightMapWidth, this.heightMapHeight, this.heightData);
		}, null, null, true);
		yield return null;
		XUiC_ProgressWindow.SetText(LocalPlayerUI.primaryUI, Localization.Get("uiLoadProcessWorldSplats", false, null), true);
		Texture2D splat3Tex = null;
		string text = _worldPath + "/splat3";
		if (SdFile.Exists(text + ".png"))
		{
			splat3Tex = TextureUtils.LoadTexture(text + ".png", 0, true, false, null);
		}
		else if (SdFile.Exists(text + ".tga"))
		{
			splat3Tex = TextureUtils.LoadTexture(text + ".tga", 0, false, false, null);
		}
		bool flag = splat3Tex.format == 5;
		if (!flag && splat3Tex.format != 4)
		{
			Log.Error("World's splat3 file is not in the correct format (needs to be either RGBA32 or ARGB32)!");
			yield break;
		}
		Texture2D splat4Tex = new Texture2D(this.heightMapWidth * this.heightMapScale, this.heightMapHeight * this.heightMapScale, 5, true, false);
		string text2 = _worldPath + "/splat4";
		bool splat4Loaded = false;
		if (SdFile.Exists(text2 + ".png"))
		{
			splat4Tex = TextureUtils.LoadTexture(text2 + ".png", 0, true, false, null);
			splat4Loaded = true;
		}
		else if (SdFile.Exists(text2 + ".tga"))
		{
			splat4Tex = TextureUtils.LoadTexture(text2 + ".tga", 0, false, false, null);
			splat4Loaded = true;
		}
		NativeArray<TextureUtils.ColorARGB32> splat4Cols = splat4Tex.GetRawTextureData<TextureUtils.ColorARGB32>();
		int num2;
		if (flag)
		{
			NativeArray<TextureUtils.ColorARGB32> splat3Cols = splat3Tex.GetRawTextureData<TextureUtils.ColorARGB32>();
			for (int i = 0; i < topTexMap.Length; i = num2 + 1)
			{
				if (i % Constants.cMaxLoadTimePixelsPerTest == 0 && yieldMs.ElapsedMilliseconds > (long)Constants.cMaxLoadTimePerFrameMillis)
				{
					yield return null;
					yieldMs.ResetAndRestart();
				}
				TextureUtils.ColorARGB32 colorARGB = splat3Cols[i];
				TextureUtils.ColorARGB32 colorARGB2 = default(TextureUtils.ColorARGB32);
				if (splat4Loaded)
				{
					colorARGB2 = splat4Cols[i];
				}
				ushort num = topTexMap[i];
				if (num <= 2)
				{
					if (num != 0)
					{
						if (num == 2)
						{
							colorARGB2.g = byte.MaxValue;
						}
					}
				}
				else
				{
					switch (num)
					{
					case 8:
						colorARGB.b = byte.MaxValue;
						break;
					case 9:
						break;
					case 10:
						colorARGB.r = byte.MaxValue;
						break;
					case 11:
						colorARGB.g = byte.MaxValue;
						break;
					default:
						if (num != 185)
						{
							if (num == 200)
							{
								colorARGB2.r = byte.MaxValue;
							}
						}
						else
						{
							colorARGB.a = byte.MaxValue;
						}
						break;
					}
				}
				splat3Cols[i] = colorARGB;
				splat4Cols[i] = colorARGB2;
				num2 = i;
			}
			splat3Cols = default(NativeArray<TextureUtils.ColorARGB32>);
		}
		else
		{
			NativeArray<Color32> splat3Cols2 = splat3Tex.GetRawTextureData<Color32>();
			for (int i = 0; i < topTexMap.Length; i = num2 + 1)
			{
				if (i % Constants.cMaxLoadTimePixelsPerTest == 0 && yieldMs.ElapsedMilliseconds > (long)Constants.cMaxLoadTimePerFrameMillis)
				{
					yield return null;
					yieldMs.ResetAndRestart();
				}
				Color32 color = splat3Cols2[i];
				TextureUtils.ColorARGB32 colorARGB3 = default(TextureUtils.ColorARGB32);
				if (splat4Loaded)
				{
					colorARGB3 = splat4Cols[i];
				}
				ushort num = topTexMap[i];
				if (num <= 2)
				{
					if (num != 0)
					{
						if (num == 2)
						{
							colorARGB3.g = byte.MaxValue;
						}
					}
				}
				else
				{
					switch (num)
					{
					case 8:
						color.b = byte.MaxValue;
						break;
					case 9:
						break;
					case 10:
						color.r = byte.MaxValue;
						break;
					case 11:
						color.g = byte.MaxValue;
						break;
					default:
						if (num != 185)
						{
							if (num == 200)
							{
								colorARGB3.r = byte.MaxValue;
							}
						}
						else
						{
							color.a = byte.MaxValue;
						}
						break;
					}
				}
				splat3Cols2[i] = color;
				splat4Cols[i] = colorARGB3;
				num2 = i;
			}
			splat3Cols2 = default(NativeArray<Color32>);
		}
		yield return null;
		if (this.heightMap == null)
		{
			this.heightMap = new HeightMap(this.heightMapWidth, this.heightMapHeight, 255f, this.heightData, this.heightMapWidth * this.heightMapScale);
		}
		splat4Tex.SetPixelData<TextureUtils.ColorARGB32>(splat4Cols.ToArray(), 0, 0);
		XUiC_ProgressWindow.SetText(LocalPlayerUI.primaryUI, Localization.Get("uiLoadProcessWorldDecos", false, null), true);
		string levelName = this.levelName;
		string fullPath = this.worldLocation.FullPath;
		DynamicPrefabDecorator dynamicPrefabDecorator = this.GetDynamicPrefabDecorator();
		int num3 = this.heightMapWidth;
		int num4 = this.heightMapHeight;
		Texture2D texture2D = splat3Tex;
		bool flag2 = false;
		Texture2D texture2D2 = (splat4Loaded ? splat4Tex : null);
		WorldDecoratorPOIFromImage worldDecoratorPoiFromImage = new WorldDecoratorPOIFromImage(levelName, fullPath, dynamicPrefabDecorator, num3, num4, texture2D, flag2, this.heightMapScale, this.heightMap, texture2D2);
		this.m_Decorators.Add(worldDecoratorPoiFromImage);
		yield return worldDecoratorPoiFromImage.InitData();
		GridCompressedData<byte> cols = worldDecoratorPoiFromImage.m_Poi.colors;
		int length = cols.width * cols.height;
		for (int i = 0; i < length; i = num2 + 1)
		{
			if (i % Constants.cMaxLoadTimePixelsPerTest == 0 && yieldMs.ElapsedMilliseconds > (long)Constants.cMaxLoadTimePerFrameMillis)
			{
				yield return null;
				yieldMs.ResetAndRestart();
			}
			if (cols.GetValue(i) >= 5)
			{
				TextureUtils.ColorARGB32 colorARGB4 = splat4Cols[i];
				if (Mathf.FloorToInt(this.heightMap.GetAt(i)) <= (int)(cols.GetValue(i) - 5 + 1))
				{
					colorARGB4.g = byte.MaxValue;
					if (cols.GetValue(i) > 5)
					{
						colorARGB4.b = cols.GetValue(i) - 5;
					}
				}
				splat4Cols[i] = colorARGB4;
			}
			num2 = i;
		}
		yield return null;
		splat3Tex.Apply(true, false);
		yield return null;
		splat4Tex.SetPixelData<TextureUtils.ColorARGB32>(splat4Cols.ToArray(), 0, 0);
		splat4Tex.Apply(true, false);
		yield return null;
		XUiC_ProgressWindow.SetText(LocalPlayerUI.primaryUI, Localization.Get("uiLoadProcessWorldWriting", false, null), true);
		SdFile.WriteAllBytes(_worldPath + "/splat3_processed.png", ImageConversion.EncodeToPNG(splat3Tex));
		yield return GCUtils.UnloadAndCollectCo();
		SdFile.WriteAllBytes(_worldPath + "/splat4_processed.png", ImageConversion.EncodeToPNG(splat4Tex));
		yield return GCUtils.UnloadAndCollectCo();
		Texture2D splat3Half = this.generateHalfResTexture(splat3Tex);
		Texture2D splat4Half = this.generateHalfResTexture(splat4Tex);
		yield return null;
		SdFile.WriteAllBytes(_worldPath + "/splat3_half.png", ImageConversion.EncodeToPNG(splat3Half));
		yield return GCUtils.UnloadAndCollectCo();
		SdFile.WriteAllBytes(_worldPath + "/splat4_half.png", ImageConversion.EncodeToPNG(splat4Half));
		yield return GCUtils.UnloadAndCollectCo();
		yield return this.calcWorldFileCrcs(_worldPath);
		this.saveFileHashes(_worldPath, this.worldFileCrcs);
		yield return GCUtils.UnloadAndCollectCo();
		Log.Out("Loading and creating dtm raw file took " + ms.ElapsedMilliseconds.ToString() + "ms");
		_callback(splat3Tex, splat3Half, splat4Tex, splat4Half, worldDecoratorPoiFromImage);
		yield break;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Texture2D generateHalfResTexture(Texture2D _tex)
	{
		if (_tex.mipmapCount < 1)
		{
			Log.Error("Attempted to generate half-res texture from a texture that does not have mip level 1. Returning the source texture instead.");
			return _tex;
		}
		Texture2D texture2D = new Texture2D(_tex.width >> 1, _tex.height >> 1, 5, false);
		texture2D.SetPixels(_tex.GetPixels(1));
		texture2D.Apply();
		return texture2D;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string getFilenameDTM()
	{
		string fullPath = this.worldLocation.FullPath;
		if (SdFile.Exists(fullPath + "/dtm_processed.raw"))
		{
			return fullPath + "/dtm_processed";
		}
		return fullPath + "/dtm";
	}

	public override void ReloadAllChunks()
	{
		this.loadDTM(null);
		((TerrainFromRaw)this.m_TerrainGenerator).Init(this.heightMap, this.m_BiomeProvider, this.world.Seed);
		base.ReloadAllChunks();
	}

	public override EnumChunkProviderId GetProviderId()
	{
		return EnumChunkProviderId.ChunkDataDriven;
	}

	public override bool GetWorldExtent(out Vector3i _minSize, out Vector3i _maxSize)
	{
		_minSize = new Vector3i(-this.heightMapWidth * this.heightMapScale / 2, 0, -this.heightMapHeight * this.heightMapScale / 2);
		_maxSize = new Vector3i(this.heightMapWidth * this.heightMapScale / 2, 255, this.heightMapHeight * this.heightMapScale / 2);
		return true;
	}

	public override Vector2i GetWorldSize()
	{
		return new Vector2i(this.heightMapWidth * this.heightMapScale, this.heightMapHeight * this.heightMapScale);
	}

	public override int GetPOIBlockIdOverride(int x, int z)
	{
		if (this.poiFromImage == null)
		{
			return 0;
		}
		WorldGridCompressedData<byte> poi = this.poiFromImage.m_Poi;
		byte data;
		if (!poi.Contains(x, z) || (data = poi.GetData(x, z)) == 255 || data == 0)
		{
			return 0;
		}
		PoiMapElement poiForColor = this.world.Biomes.getPoiForColor((uint)data);
		if (poiForColor == null || (this.bFixedWaterLevel && poiForColor.m_BlockValue.Block.blockMaterial.IsLiquid))
		{
			return 0;
		}
		return poiForColor.m_BlockValue.type;
	}

	public unsafe override IEnumerator FillOccupiedMap(int width, int height, DecoOccupiedMap _occupiedMap, List<PrefabInstance> overridePOIList = null)
	{
		MicroStopwatch mswYields = new MicroStopwatch();
		EnumDecoOccupied[] occupiedMap = _occupiedMap.GetData();
		int length = this.heightData.Length;
		mswYields.ResetAndRestart();
		int num3;
		if (this.m_Decorators[0] is WorldDecoratorPOIFromImage)
		{
			WorldDecoratorPOIFromImage worldDecoratorPOIFromImage = (WorldDecoratorPOIFromImage)this.m_Decorators[0];
			WorldGridCompressedData<byte> poi = worldDecoratorPOIFromImage.m_Poi;
			for (int y = 0; y < height; y = num3)
			{
				int num = y * width;
				int num2 = num + width;
				for (int i = num; i < num2; i++)
				{
					byte value = poi.colors.GetValue(i);
					if (value != 0 && value != 255)
					{
						occupiedMap[i] = EnumDecoOccupied.POI;
					}
				}
				if (mswYields.ElapsedMilliseconds > (long)Constants.cMaxLoadTimePerFrameMillis)
				{
					yield return null;
					mswYields.ResetAndRestart();
				}
				num3 = y + 1;
			}
			poi = null;
		}
		yield return null;
		DynamicPrefabDecorator dynamicPrefabDecorator = this.GetDynamicPrefabDecorator();
		if (dynamicPrefabDecorator != null)
		{
			List<PrefabInstance> list = new List<PrefabInstance>();
			dynamicPrefabDecorator.GetWorldPrefabs(list);
			overridePOIList = list;
		}
		if (overridePOIList != null)
		{
			for (int j = 0; j < overridePOIList.Count; j++)
			{
				PrefabInstance prefabInstance = overridePOIList[j];
				_occupiedMap.SetArea(prefabInstance.boundingBoxPosition.x, prefabInstance.boundingBoxPosition.z, EnumDecoOccupied.POI, prefabInstance.boundingBoxSize.x, prefabInstance.boundingBoxSize.z);
			}
		}
		if (WorldBiomes.Instance.GetTotalBluffsCount() > 0)
		{
			IBiomeProvider biomeProvider = GameManager.Instance.World.ChunkCache.ChunkProvider.GetBiomeProvider();
			int num4 = -width / 2;
			int num5 = width / 2;
			int num6 = -height / 2;
			int num7 = height / 2;
			GameRandom gameRandom = Utils.RandomFromSeedOnPos(0, 0, GameManager.Instance.World.Seed);
			int num8 = 0;
			using (IBackedArrayView<ushort> backedArrayView = BackedArrays.CreateSingleView<ushort>(this.heightData, BackedArrayHandleMode.ReadWrite, 0))
			{
				for (int k = num6; k < num7; k++)
				{
					for (int l = num4; l < num5; l++)
					{
						float num9;
						BiomeDefinition biomeAt = biomeProvider.GetBiomeAt(l, k, out num9);
						if (biomeAt != null)
						{
							for (int m = 0; m < biomeAt.m_DecoBluffs.Count; m++)
							{
								BiomeBluffDecoration biomeBluffDecoration = biomeAt.m_DecoBluffs[m];
								if (gameRandom.RandomFloat <= biomeBluffDecoration.m_Prob)
								{
									ChunkProviderGenerateWorldFromRaw.Bluff bluff;
									if (!this.bluffs.TryGetValue(biomeAt.m_DecoBluffs[m].m_sName, out bluff))
									{
										bluff = ChunkProviderGenerateWorldFromRaw.Bluff.Load(biomeAt.m_DecoBluffs[m].m_sName);
										this.bluffs.Add(biomeAt.m_DecoBluffs[m].m_sName, bluff);
									}
									int num10 = gameRandom.RandomRange(3);
									Vector3i vector3i = new Vector3i(bluff.width, 1, bluff.height);
									for (int n = 0; n < num10; n++)
									{
										int x = vector3i.x;
										vector3i.x = vector3i.z;
										vector3i.z = x;
									}
									if (!_occupiedMap.CheckArea(l, k, EnumDecoOccupied.Stop_BigDeco, vector3i.x, vector3i.z))
									{
										_occupiedMap.SetArea(l, k, EnumDecoOccupied.Perimeter, vector3i.x, vector3i.z);
										float num11 = biomeAt.m_DecoBluffs[m].m_MinScale + gameRandom.RandomFloat * (biomeAt.m_DecoBluffs[m].m_MaxScale - biomeAt.m_DecoBluffs[m].m_MinScale);
										for (int num12 = 0; num12 < bluff.height; num12++)
										{
											for (int num13 = 0; num13 < bluff.width; num13++)
											{
												int num14;
												switch (num10)
												{
												case 1:
													num14 = num12 + (bluff.width - num13 - 1) * bluff.height;
													break;
												case 2:
													num14 = bluff.width - num13 - 1 + (bluff.height - num12 - 1) * bluff.width;
													break;
												case 3:
													num14 = bluff.height - num12 - 1 + num13 * bluff.height;
													break;
												default:
													num14 = num13 + num12 * bluff.width;
													break;
												}
												float num15 = bluff.data[num14] * num11;
												int num16 = (num13 + l + width / 2 + (num12 + k + height / 2) * width) % length;
												float num17 = (float)backedArrayView[num16] * 0.0038910506f + num15;
												if (num17 > 246f)
												{
													num17 = 246f;
												}
												backedArrayView[num16] = (ushort)(num17 / 0.0038910506f);
											}
										}
										num8++;
									}
								}
							}
						}
					}
				}
				GameRandomManager.Instance.FreeGameRandom(gameRandom);
			}
		}
		yield return null;
		mswYields.ResetAndRestart();
		int bigSlope = (int)(Mathf.Sin(1.134464f) / Mathf.Cos(1.134464f) / 1.5140275E-05f);
		int smallSlope = (int)(Mathf.Sin(0.83775806f) / Mathf.Cos(0.83775806f) / 1.5140275E-05f);
		ReadOnlyMemory<ushort> heightsRowMemory;
		IDisposable heightsRowMemoryHandle = this.heightData.GetReadOnlyMemory(0, width, out heightsRowMemory);
		IDisposable heightsNextRowMemoryHandle = null;
		int heightM = height - 1;
		int widthM = width - 1;
		for (int y = 0; y < heightM; y = num3)
		{
			int num18 = y * width;
			ReadOnlyMemory<ushort> readOnlyMemory;
			heightsNextRowMemoryHandle = this.heightData.GetReadOnlyMemory(num18 + width, width, out readOnlyMemory);
			ReadOnlySpan<ushort> span = heightsRowMemory.Span;
			ReadOnlySpan<ushort> span2 = readOnlyMemory.Span;
			Span<EnumDecoOccupied> span3 = occupiedMap.AsSpan(num18, widthM);
			int num19 = (int)(*span[0]);
			for (int num20 = 0; num20 < widthM; num20++)
			{
				int num21 = (int)(*span[num20 + 1]);
				int num22 = (int)(*span2[num20]);
				int num23 = num19 - num21;
				int num24 = num19 - num22;
				num19 = num21;
				int num25 = num23 * num23 + num24 * num24;
				if (num25 > smallSlope)
				{
					if (num25 > bigSlope)
					{
						*span3[num20] = EnumDecoOccupied.BigSlope;
					}
					else
					{
						*span3[num20] = EnumDecoOccupied.SmallSlope;
					}
				}
			}
			IDisposable disposable = heightsRowMemoryHandle;
			if (disposable != null)
			{
				disposable.Dispose();
			}
			heightsRowMemory = readOnlyMemory;
			heightsRowMemoryHandle = heightsNextRowMemoryHandle;
			if (mswYields.ElapsedMilliseconds > (long)Constants.cMaxLoadTimePerFrameMillis)
			{
				yield return null;
				mswYields.ResetAndRestart();
			}
			num3 = y + 1;
		}
		IDisposable disposable2 = heightsRowMemoryHandle;
		if (disposable2 != null)
		{
			disposable2.Dispose();
		}
		IDisposable disposable3 = heightsNextRowMemoryHandle;
		if (disposable3 != null)
		{
			disposable3.Dispose();
		}
		yield break;
	}

	public override float GetPOIHeightOverride(int x, int z)
	{
		WorldDecoratorPOIFromImage worldDecoratorPOIFromImage = this.m_Decorators[0] as WorldDecoratorPOIFromImage;
		if (worldDecoratorPOIFromImage == null)
		{
			return 0f;
		}
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

	public float GetHeight(int x, int z)
	{
		float num;
		try
		{
			num = this.heightMap.GetAt(x, z);
		}
		catch (Exception)
		{
			Log.Error("Get Height Error x: {0} z: {1}", new object[] { x, z });
			num = 0f;
		}
		return num;
	}

	public override void Cleanup()
	{
		base.Cleanup();
		for (int i = 0; i < this.splats.Length; i++)
		{
			Object.Destroy(this.splats[i]);
		}
		Object.Destroy(this.procBiomeMask1);
		Object.Destroy(this.procBiomeMask2);
		HeightMap heightMap = this.heightMap;
		if (heightMap != null)
		{
			heightMap.Dispose();
		}
		this.heightMap = null;
		IBackedArray<ushort> backedArray = this.heightData;
		if (backedArray != null)
		{
			backedArray.Dispose();
		}
		this.heightData = null;
		IBiomeProvider biomeProvider = this.m_BiomeProvider;
		if (biomeProvider == null)
		{
			return;
		}
		biomeProvider.Cleanup();
	}

	public void GetWaterChunks16x16(out int _water16x16ChunksW, out byte[] _water16x16Chunks)
	{
		WorldDecoratorPOIFromImage worldDecoratorPOIFromImage = this.m_Decorators[0] as WorldDecoratorPOIFromImage;
		if (worldDecoratorPOIFromImage == null)
		{
			_water16x16ChunksW = 0;
			_water16x16Chunks = null;
			return;
		}
		worldDecoratorPOIFromImage.GetWaterChunks16x16(out _water16x16ChunksW, out _water16x16Chunks);
	}

	public override ChunkProtectionLevel GetChunkProtectionLevel(Vector3i worldPos)
	{
		return this.m_RegionFileManager.GetChunkProtectionLevelForWorldPos(worldPos);
	}

	[BurstCompile(CompileSynchronously = true)]
	[PublicizedFrom(EAccessModifier.Internal)]
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void RoadSmooth$BurstManaged(in NativeArray<Color32> _colors, ref NativeArray<Color32> _colorsOut, int width, int height)
	{
		int num = width - 1;
		int num2 = 0;
		for (int i = 0; i < height; i++)
		{
			for (int j = 0; j < width; j++)
			{
				NativeArray<Color32> nativeArray = _colors;
				Color32 color = nativeArray[num2];
				if (j > 0 && j < num && i > 0 && i < num && (color.b == 0 || color.g > 0))
				{
					float num3 = (float)color.g;
					float num4 = num3;
					nativeArray = _colors;
					num3 = num4 + (float)nativeArray[num2 - width].g * 1f;
					float num5 = num3;
					nativeArray = _colors;
					num3 = num5 + (float)nativeArray[num2 - 1].g * 1f;
					float num6 = num3;
					nativeArray = _colors;
					num3 = num6 + (float)nativeArray[num2 + 1].g * 1f;
					float num7 = num3;
					nativeArray = _colors;
					num3 = num7 + (float)nativeArray[num2 + width].g * 1f;
					num3 *= 0.2f;
					if (color.r == 0)
					{
						num3 = (1f - Mathf.Pow(1f - num3 / 255f, 2f)) * 255f;
						color.g = (byte)Utils.FastMax((float)color.g, num3);
					}
					else
					{
						color.g = (byte)(num3 * 0.5f);
					}
				}
				if (color.r + color.g > 0)
				{
					color.a = 0;
				}
				_colorsOut[num2++] = color;
			}
		}
	}

	public const string cRawProcessed = "_processed";

	public const string cHalf = "_half";

	public const int cMaxHeight = 255;

	[PublicizedFrom(EAccessModifier.Private)]
	public int heightMapWidth = 4096;

	[PublicizedFrom(EAccessModifier.Private)]
	public int heightMapHeight = 4096;

	[PublicizedFrom(EAccessModifier.Private)]
	public int heightMapScale = 1;

	public const float hmFac = 0.0038910506f;

	public IBackedArray<ushort> heightData;

	public WorldDecoratorPOIFromImage poiFromImage;

	[PublicizedFrom(EAccessModifier.Private)]
	public HeightMap heightMap;

	public Texture2D[] splats = new Texture2D[7];

	public Texture2D procBiomeMask1;

	public Texture2D procBiomeMask2;

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<string, ChunkProviderGenerateWorldFromRaw.Bluff> bluffs = new Dictionary<string, ChunkProviderGenerateWorldFromRaw.Bluff>();

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bFixedWaterLevel;

	public readonly Dictionary<string, uint> worldFileCrcs = new CaseInsensitiveStringDictionary<uint>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly string[] FilesUsedForProcessing = new string[] { "prefabs.xml", "dtm.raw", "splat3.tga", "splat3.png", "water_info.xml", "main.ttw", "biomes.png" };

	[PublicizedFrom(EAccessModifier.Private)]
	public class Bluff
	{
		public static ChunkProviderGenerateWorldFromRaw.Bluff Load(string _name)
		{
			Texture2D texture2D = TextureUtils.LoadTexture(GameIO.GetGameDir("Data/Bluffs") + "/" + _name + ".tga", 0, false, false, null);
			Color32[] pixels = texture2D.GetPixels32();
			ChunkProviderGenerateWorldFromRaw.Bluff bluff = new ChunkProviderGenerateWorldFromRaw.Bluff();
			bluff.width = texture2D.width;
			bluff.height = texture2D.height;
			bluff.data = new float[bluff.width * bluff.height];
			for (int i = pixels.Length - 1; i >= 0; i--)
			{
				bluff.data[i] = (float)pixels[i].r;
			}
			Object.Destroy(texture2D);
			return bluff;
		}

		public int width;

		public int height;

		public float[] data;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public delegate void ProcessFilesCallback(Texture2D _splat3Tex, Texture2D _splat3Visual, Texture2D _splat4Tex, Texture2D _splat4Visual, WorldDecoratorPOIFromImage _decoratorPoiFromImage);

	[PublicizedFrom(EAccessModifier.Internal)]
	public delegate void RoadSmooth_00005653$PostfixBurstDelegate(in NativeArray<Color32> _colors, ref NativeArray<Color32> _colorsOut, int width, int height);

	[PublicizedFrom(EAccessModifier.Internal)]
	public static class RoadSmooth_00005653$BurstDirectCall
	{
		[BurstDiscard]
		[PublicizedFrom(EAccessModifier.Private)]
		public static void GetFunctionPointerDiscard(ref IntPtr A_0)
		{
			if (ChunkProviderGenerateWorldFromRaw.RoadSmooth_00005653$BurstDirectCall.Pointer == 0)
			{
				ChunkProviderGenerateWorldFromRaw.RoadSmooth_00005653$BurstDirectCall.Pointer = BurstCompiler.GetILPPMethodFunctionPointer2(ChunkProviderGenerateWorldFromRaw.RoadSmooth_00005653$BurstDirectCall.DeferredCompilation, methodof(ChunkProviderGenerateWorldFromRaw.RoadSmooth$BurstManaged(ref NativeArray<Color32>, ref NativeArray<Color32>, int, int)).MethodHandle, typeof(ChunkProviderGenerateWorldFromRaw.RoadSmooth_00005653$PostfixBurstDelegate).TypeHandle);
			}
			A_0 = ChunkProviderGenerateWorldFromRaw.RoadSmooth_00005653$BurstDirectCall.Pointer;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static IntPtr GetFunctionPointer()
		{
			IntPtr intPtr = (IntPtr)0;
			ChunkProviderGenerateWorldFromRaw.RoadSmooth_00005653$BurstDirectCall.GetFunctionPointerDiscard(ref intPtr);
			return intPtr;
		}

		public static void Constructor()
		{
			ChunkProviderGenerateWorldFromRaw.RoadSmooth_00005653$BurstDirectCall.DeferredCompilation = BurstCompiler.CompileILPPMethod2(methodof(ChunkProviderGenerateWorldFromRaw.RoadSmooth(ref NativeArray<Color32>, ref NativeArray<Color32>, int, int)).MethodHandle);
		}

		public static void Initialize()
		{
		}

		// Note: this type is marked as 'beforefieldinit'.
		[PublicizedFrom(EAccessModifier.Private)]
		static RoadSmooth_00005653$BurstDirectCall()
		{
			ChunkProviderGenerateWorldFromRaw.RoadSmooth_00005653$BurstDirectCall.Constructor();
		}

		public static void Invoke(in NativeArray<Color32> _colors, ref NativeArray<Color32> _colorsOut, int width, int height)
		{
			if (BurstCompiler.IsEnabled)
			{
				IntPtr functionPointer = ChunkProviderGenerateWorldFromRaw.RoadSmooth_00005653$BurstDirectCall.GetFunctionPointer();
				if (functionPointer != 0)
				{
					calli(System.Void(Unity.Collections.NativeArray`1<UnityEngine.Color32>&,Unity.Collections.NativeArray`1<UnityEngine.Color32>&,System.Int32,System.Int32), ref _colors, ref _colorsOut, width, height, functionPointer);
					return;
				}
			}
			ChunkProviderGenerateWorldFromRaw.RoadSmooth$BurstManaged(in _colors, ref _colorsOut, width, height);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static IntPtr Pointer;

		[PublicizedFrom(EAccessModifier.Private)]
		public static IntPtr DeferredCompilation;
	}
}
