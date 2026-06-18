using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.TextCore;

public class SignDataManager
{
	public static SignDataManager Instance
	{
		get
		{
			if (SignDataManager.m_Instance == null)
			{
				SignDataManager.m_Instance = new SignDataManager();
			}
			return SignDataManager.m_Instance;
		}
	}

	public static bool HasInstance
	{
		get
		{
			return SignDataManager.m_Instance != null;
		}
	}

	public IEnumerable<string> FontNames
	{
		get
		{
			return this.fontDataByName.Keys;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public SignDataManager()
	{
		if (GameManager.IsDedicatedServer)
		{
			return;
		}
		SignFontConfiguration signFontConfiguration = DataLoader.LoadAsset<SignFontConfiguration>("@:Fonts/SignFontConfig.asset", true);
		if (signFontConfiguration == null)
		{
			Log.Error("Failed to load font configuration asset from path: @:Fonts/SignFontConfig.asset");
		}
		else
		{
			int num = 0;
			int num2 = 0;
			int num3 = 0;
			foreach (SignFontConfiguration.FontInfo fontInfo in signFontConfiguration.fontInfos)
			{
				TMP_FontAsset fontAsset = fontInfo.fontAsset;
				if (((fontAsset != null) ? fontAsset.atlasTexture : null) != null)
				{
					if (num2 == 0 || num3 == 0)
					{
						num2 = fontInfo.fontAsset.atlasTexture.width;
						num3 = fontInfo.fontAsset.atlasTexture.height;
					}
					else if (num2 != fontInfo.fontAsset.atlasTexture.width || num3 != fontInfo.fontAsset.atlasTexture.height)
					{
						Log.Error("SignFontConfiguration asset references fonts with differing font atlas resolutions. This is not supported. Configuration asset prefabLocation: `@:Fonts/SignFontConfig.asset`.");
						continue;
					}
					this.fontDataByName[fontInfo.name] = new SignDataManager.SignFontData(num, fontInfo.fontAsset);
					num++;
				}
			}
			if (SystemInfo.supports2DArrayTextures)
			{
				this.fontAtlasTextureArray = new Texture2DArray(num2, num3, this.fontDataByName.Count, 1, false);
				this.fontAtlasTextureArray.name = "Sign Font Atlas";
				using (Dictionary<string, SignDataManager.SignFontData>.Enumerator enumerator2 = this.fontDataByName.GetEnumerator())
				{
					while (enumerator2.MoveNext())
					{
						KeyValuePair<string, SignDataManager.SignFontData> keyValuePair = enumerator2.Current;
						SignDataManager.SignFontData value = keyValuePair.Value;
						Graphics.CopyTexture(value.AtlasTexture, 0, this.fontAtlasTextureArray, value.AtlasIndex);
					}
					goto IL_063C;
				}
			}
			Log.Error("Texture arrays are not supported on this platform/GPU; sign fonts will not be rendered correctly.");
		}
		IL_063C:
		this.intermediateDataWrappers = new SignDataManager.IIntermediateDataWrapper[]
		{
			this.layerDescriptors, this.circleLayers, this.polygonLayers, this.charLayers, this.noiseLayers, this.transformWarps, this.skewWarps, this.bulgeWarps, this.twirlWarps, this.kaleidoWarps,
			this.perspectiveWarps, this.arcWarps, this.stretchWarps, this.gridWarps
		};
	}

	public static string GetLibraryNiceName(GlobalSignId signId)
	{
		if (!signId.IsValid)
		{
			return "-";
		}
		string libraryId = signId.libraryId;
		string text;
		if (!(libraryId == "[D]"))
		{
			if (!(libraryId == "[U]"))
			{
				text = signId.libraryId;
			}
			else
			{
				text = Localization.Get("lblSignLibraryUser", false, null);
			}
		}
		else
		{
			text = Localization.Get("lblSignLibraryDefault", false, null);
		}
		return text;
	}

	public static GlobalSignId ErrorSignId
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public static IEnumerator LoadDefaultLibrary(XmlFile file)
	{
		SignLibrary signLibrary = new SignLibrary();
		signLibrary.ReadXml(file);
		SignDataManager.Instance.TryRegisterLibrary("[D]", signLibrary, false);
		SignDataManager.ErrorSignId = SignDataManager.Instance.AddSignToLibrary("[I]", SignData.GetErrorSignData());
		yield break;
	}

	public void Cleanup()
	{
		foreach (KeyValuePair<GlobalSignId, SignDataManager.SignRenderingData> keyValuePair in this.renderingDataByID)
		{
			SignDataManager.SignRenderingData value = keyValuePair.Value;
			if (value != null)
			{
				value.Dispose();
			}
		}
		this.renderingDataByID.Clear();
		this.signLibraries.Clear();
		this.updateListenerMap.Clear();
	}

	public void RegisterListener(GlobalSignId id, ISignRenderingDataUpdateListener listener, bool exclusive = true)
	{
		if (exclusive)
		{
			this.updateListenerMap.DeregisterListener(listener);
		}
		this.updateListenerMap.RegisterListener(id, listener);
	}

	public void DeregisterListener(ISignRenderingDataUpdateListener listener)
	{
		this.updateListenerMap.DeregisterListener(listener);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool LibraryContainsName(string libraryName, string signName)
	{
		SignLibrary signLibrary;
		if (this.signLibraries.TryGetValue(libraryName, out signLibrary))
		{
			using (Dictionary<Guid, SignData>.ValueCollection.Enumerator enumerator = signLibrary.signs.Values.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					if (enumerator.Current.name == signName)
					{
						return true;
					}
				}
			}
			return false;
		}
		return false;
	}

	public string GetDuplicateName(string libraryName, string signName)
	{
		string text = signName;
		Match match = Regex.Match(signName, "^(.+) \\(\\d+\\)$");
		if (match.Success)
		{
			text = match.Groups[1].Value;
		}
		if (!this.LibraryContainsName(libraryName, text))
		{
			return text;
		}
		int num = 1;
		while (this.LibraryContainsName(libraryName, string.Format("{0} ({1})", text, num)))
		{
			num++;
		}
		return string.Format("{0} ({1})", text, num);
	}

	public bool MoveSignToLibrary(GlobalSignId signId, string libraryName, out GlobalSignId newId)
	{
		SignData signData;
		if (this.TryGetSignData(signId, out signData))
		{
			SignData signData2 = SignData.Duplicate(signData, "");
			newId = this.AddSignToLibrary(libraryName, signData2);
			return true;
		}
		newId = GlobalSignId.InvalidId;
		return false;
	}

	public GlobalSignId AddSignToLibrary(string libraryId, SignData sign)
	{
		SignLibrary signLibrary;
		if (!this.signLibraries.TryGetValue(libraryId, out signLibrary))
		{
			signLibrary = new SignLibrary();
			this.signLibraries[libraryId] = signLibrary;
		}
		signLibrary.signs[sign.guid] = sign;
		return new GlobalSignId(libraryId, sign.guid);
	}

	public bool TryDeleteSign(GlobalSignId signId)
	{
		this.TryClearRenderingData(signId);
		SignLibrary signLibrary;
		return this.signLibraries.TryGetValue(signId.libraryId, out signLibrary) && signLibrary.signs.Remove(signId.signGuid);
	}

	public bool MarkSignDirty(GlobalSignId signId)
	{
		SignData signData;
		if (this.updateListenerMap.HasListeners(signId) && this.TryGetSignData(signId, out signData))
		{
			this.UpdateRenderingData(signData, signId);
			foreach (ISignRenderingDataUpdateListener signRenderingDataUpdateListener in this.updateListenerMap.GetListeners(signId))
			{
				signRenderingDataUpdateListener.HandleRenderingDataUpdate();
			}
			return true;
		}
		return this.TryClearRenderingData(signId);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool TryClearRenderingData(GlobalSignId signId)
	{
		SignDataManager.SignRenderingData signRenderingData;
		if (this.renderingDataByID.TryGetValue(signId, out signRenderingData))
		{
			signRenderingData.Dispose();
			this.renderingDataByID.Remove(signId);
			return true;
		}
		return false;
	}

	public bool TryRegisterLibrary(string libraryId, SignLibrary library, bool replaceExisting = false)
	{
		SignLibrary signLibrary;
		if (this.signLibraries.TryGetValue(libraryId, out signLibrary))
		{
			if (!replaceExisting)
			{
				return false;
			}
			foreach (Guid guid in signLibrary.signs.Keys)
			{
				GlobalSignId globalSignId = new GlobalSignId(libraryId, guid);
				SignDataManager.SignRenderingData signRenderingData;
				if (this.renderingDataByID.TryGetValue(globalSignId, out signRenderingData))
				{
					signRenderingData.Dispose();
					this.renderingDataByID.Remove(globalSignId);
				}
			}
		}
		this.signLibraries[libraryId] = library;
		return true;
	}

	public IEnumerator RequestWorldSignDataFromServer()
	{
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			Log.Error("[SignDataManager] Only client can request to download Sign Data.");
			yield break;
		}
		if (this.signDataDownloadInProgress)
		{
			Log.Error("[SignDataManager] Sign Data download is already in progress.");
			yield break;
		}
		this.signDataDownloadInProgress = true;
		this.signDataDownloadComplete = false;
		SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageSignDataRequest>(), false);
		while (this.signDataDownloadInProgress)
		{
			if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsConnected)
			{
				this.signDataDownloadInProgress = false;
				this.signDataDownloadComplete = false;
				yield break;
			}
			yield return null;
		}
		yield break;
	}

	public void SendSignDataToClient(ClientInfo client)
	{
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsClient)
		{
			Log.Error("[SignDataManager] Only server can send Sign Data to clients.");
			return;
		}
		List<KeyValuePair<string, SignData>> list = new List<KeyValuePair<string, SignData>>();
		foreach (KeyValuePair<string, SignLibrary> keyValuePair in this.EnumeratePublicSignLibraries())
		{
			foreach (SignData signData in keyValuePair.Value.signs.Values)
			{
				list.Add(new KeyValuePair<string, SignData>(keyValuePair.Key, signData));
			}
		}
		if (list.Count == 0)
		{
			client.SendPackage(NetPackageManager.GetPackage<NetPackageSignDataResponse>().Setup(null, true));
			return;
		}
		using (PooledExpandableMemoryStream pooledExpandableMemoryStream = MemoryPools.poolMemoryStream.AllocSync(true))
		{
			using (PooledBinaryWriter pooledBinaryWriter = MemoryPools.poolBinaryWriter.AllocSync(false))
			{
				pooledBinaryWriter.SetBaseStream(pooledExpandableMemoryStream);
				int i = 0;
				while (i < list.Count)
				{
					pooledExpandableMemoryStream.SetLength(0L);
					PooledBinaryWriter.StreamWriteSizeMarker streamWriteSizeMarker = pooledBinaryWriter.ReserveSizeMarker(PooledBinaryWriter.EMarkerSize.UInt32);
					while (i < list.Count && pooledExpandableMemoryStream.Length < 1048576L)
					{
						pooledBinaryWriter.Write(list[i].Key);
						list[i].Value.Write(pooledBinaryWriter);
						i++;
					}
					pooledBinaryWriter.FinalizeSizeMarker(ref streamWriteSizeMarker);
					byte[] array = pooledExpandableMemoryStream.ToArray();
					client.SendPackage(NetPackageManager.GetPackage<NetPackageSignDataResponse>().Setup(array, i >= list.Count));
				}
			}
		}
	}

	public void ProcessSignDataBatchReceived(byte[] data, bool isLastBatch)
	{
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			Log.Error("[SignDataManager] Only client can download Sign Data.");
			return;
		}
		try
		{
			if (data != null)
			{
				using (PooledBinaryReader pooledBinaryReader = MemoryPools.poolBinaryReader.AllocSync(false))
				{
					using (MemoryStream memoryStream = new MemoryStream(data))
					{
						pooledBinaryReader.SetBaseStream(memoryStream);
						PooledBinaryReader.StreamReadSizeMarker streamReadSizeMarker = pooledBinaryReader.ReadSizeMarker(PooledBinaryWriter.EMarkerSize.UInt32);
						long num = streamReadSizeMarker.Position + (long)((ulong)streamReadSizeMarker.ExpectedSize);
						while (memoryStream.Position < num)
						{
							string text = pooledBinaryReader.ReadString();
							SignData signData = SignData.Read(pooledBinaryReader);
							this.AddSignToLibrary(text, signData);
						}
					}
				}
			}
		}
		catch
		{
			Log.Error("[SignDataManager] Failed to apply sign data batch from server");
		}
		finally
		{
			if (isLastBatch)
			{
				this.signDataDownloadInProgress = false;
				this.signDataDownloadComplete = true;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerable<KeyValuePair<string, SignLibrary>> EnumeratePublicSignLibraries()
	{
		foreach (KeyValuePair<string, SignLibrary> keyValuePair in this.signLibraries)
		{
			if (!keyValuePair.Key.Equals("[I]") || !keyValuePair.Key.Equals("[U]"))
			{
				yield return keyValuePair;
			}
		}
		Dictionary<string, SignLibrary>.Enumerator enumerator = default(Dictionary<string, SignLibrary>.Enumerator);
		yield break;
		yield break;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool TryGetRenderingData(GlobalSignId signId, out SignDataManager.SignRenderingData signRenderingData)
	{
		if (!this.renderingDataByID.TryGetValue(signId, out signRenderingData))
		{
			SignLibrary signLibrary;
			SignData signData;
			if (!this.signLibraries.TryGetValue(signId.libraryId, out signLibrary) || !signLibrary.signs.TryGetValue(signId.signGuid, out signData))
			{
				return false;
			}
			signRenderingData = this.UpdateRenderingData(signData, signId);
		}
		return true;
	}

	public bool TryApplyRenderingData(GlobalSignId signId, SignDataManager.RenderingDataPatcher patcher, List<SignRenderer> signRenderers, SignCanvas.SignBlendMode blendMode = SignCanvas.SignBlendMode.Cutout, Camera targetCamera = null)
	{
		SignDataManager.SignRenderingData signRenderingData;
		if (!this.TryGetRenderingData(signId, out signRenderingData))
		{
			return false;
		}
		Vector4 vector;
		vector..ctor(2f, 2f, -1f, -1f);
		signRenderingData.materialPropertyBlock.SetVector(SignShaderIDs._AtlasArray_ST, vector);
		if (patcher != null)
		{
			patcher(signRenderingData.materialPropertyBlock);
		}
		if (signRenderingData.materialPropertyBlock == null)
		{
			Log.Error("Unexpected case in Sign Data Manager: signRenderingData.materialPropertyBlock is null.");
		}
		foreach (SignRenderer signRenderer in signRenderers)
		{
			if (signRenderer == null)
			{
				Log.Warning("Unexpected case in Sign Data Manager: signRenderer is null");
			}
			else if (signRenderer.Renderer == null)
			{
				Log.Warning("Unexpected case in Sign Data Manager: signRenderer.Renderer is null");
			}
			else
			{
				signRenderer.SetRenderParameters(signRenderingData.materialPropertyBlock, blendMode, targetCamera);
			}
		}
		return true;
	}

	public bool TryApplyRenderingData(GlobalSignId signId, float canvasAspect, Material material, SignData.SignLayer isolateLayer = null, SignUIStyle style = SignUIStyle.Standard)
	{
		SignDataManager.SignRenderingData signRenderingData;
		if (!this.TryGetRenderingData(signId, out signRenderingData))
		{
			return false;
		}
		float num = 1f / canvasAspect;
		Mathf.Max(canvasAspect, num);
		if (this.fontAtlasTextureArray != null)
		{
			material.SetTexture(SignShaderIDs._AtlasArray, this.fontAtlasTextureArray);
		}
		SignDataManager.IIntermediateDataWrapper[] array = this.intermediateDataWrappers;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].ApplyToMaterial(signRenderingData, material);
		}
		Vector4 vector;
		vector..ctor(2f, 2f, -1f, -1f);
		material.SetVector(SignShaderIDs._AtlasArray_ST, vector);
		SignDataManager.RuntimeRenderingInfo runtimeRenderingInfo;
		if (isolateLayer != null && signRenderingData.renderingInfoByLayer.TryGetValue(isolateLayer, out runtimeRenderingInfo))
		{
			material.SetInteger(SignShaderIDs._BaseLayer, runtimeRenderingInfo.BaseDescriptorIndex);
			material.SetInteger(SignShaderIDs._LayerCount, runtimeRenderingInfo.DescriptorCount);
		}
		else
		{
			material.SetInteger(SignShaderIDs._BaseLayer, 0);
			material.SetInteger(SignShaderIDs._LayerCount, signRenderingData.layerCount);
		}
		material.SetInteger(SignShaderIDs._UIStyle, (int)style);
		material.SetFloat(SignShaderIDs._CanvasRotation, 0f);
		return true;
	}

	public bool TryGetSignComplexityInfo(GlobalSignId signId, out SignComplexityInfo complexityInfo)
	{
		SignDataManager.SignRenderingData signRenderingData;
		if (this.TryGetRenderingData(signId, out signRenderingData))
		{
			complexityInfo = new SignComplexityInfo(signRenderingData.totalComplexity, signRenderingData.complexityByLayer, signRenderingData.stackInfo);
			return true;
		}
		complexityInfo = SignComplexityInfo.Invalid;
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public SignDataManager.SignRenderingData UpdateRenderingData(SignData signData, GlobalSignId id)
	{
		SignDataManager.<>c__DisplayClass91_0 CS$<>8__locals1;
		CS$<>8__locals1.<>4__this = this;
		CS$<>8__locals1.signData = signData;
		CS$<>8__locals1.id = id;
		this.<UpdateRenderingData>g__ClearIntermediateLists|91_0(ref CS$<>8__locals1);
		if (this.renderingDataByID.TryGetValue(CS$<>8__locals1.id, out CS$<>8__locals1.signRenderingData))
		{
			CS$<>8__locals1.signRenderingData.Reset();
		}
		else
		{
			CS$<>8__locals1.signRenderingData = new SignDataManager.SignRenderingData();
			this.renderingDataByID[CS$<>8__locals1.id] = CS$<>8__locals1.signRenderingData;
		}
		CS$<>8__locals1.compStackIndex = 0;
		CS$<>8__locals1.uvStackIndex = 0;
		CS$<>8__locals1.signRenderingData.stackInfo = this.<UpdateRenderingData>g__ProcessLayers|91_1(CS$<>8__locals1.signData.layers, SignData.SignTransform.Identity, this.colorOpStack, default(SignDataManager.GroupOffsets), ref CS$<>8__locals1);
		CS$<>8__locals1.signRenderingData.layerCount = this.layerDescriptors.List.Count;
		if (this.fontAtlasTextureArray != null)
		{
			CS$<>8__locals1.signRenderingData.materialPropertyBlock.SetTexture(SignShaderIDs._AtlasArray, this.fontAtlasTextureArray);
		}
		CS$<>8__locals1.signRenderingData.materialPropertyBlock.SetInteger(SignShaderIDs._BaseLayer, 0);
		CS$<>8__locals1.signRenderingData.materialPropertyBlock.SetInteger(SignShaderIDs._LayerCount, CS$<>8__locals1.signRenderingData.layerCount);
		SignDataManager.IIntermediateDataWrapper[] array = this.intermediateDataWrappers;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].UpdateBuffer(CS$<>8__locals1.signRenderingData);
		}
		this.<UpdateRenderingData>g__ClearIntermediateLists|91_0(ref CS$<>8__locals1);
		return CS$<>8__locals1.signRenderingData;
	}

	public void AddFilteredSignIdsToList(string libraryId, string nameFilter, List<GlobalSignId> signIds)
	{
		SignLibrary signLibrary;
		if (!this.signLibraries.TryGetValue(libraryId, out signLibrary))
		{
			Log.Error("Failed to retrieve sign IDs. Library '" + libraryId + "' not found.");
			return;
		}
		foreach (SignData signData in signLibrary.signs.Values)
		{
			if (string.IsNullOrEmpty(nameFilter) || signData.name.ContainsCaseInsensitive(nameFilter))
			{
				GlobalSignId globalSignId = new GlobalSignId(libraryId, signData.guid);
				signIds.Add(globalSignId);
			}
		}
	}

	public bool TryGetSignData(GlobalSignId signId, out SignData signData)
	{
		SignLibrary signLibrary;
		if (!this.signLibraries.TryGetValue(signId.libraryId, out signLibrary))
		{
			signData = null;
			return false;
		}
		return signLibrary.signs.TryGetValue(signId.signGuid, out signData);
	}

	public bool HasSignLibrary(string libraryId)
	{
		return this.signLibraries.ContainsKey(libraryId);
	}

	public bool TryGetSignLibrary(string libraryId, out SignLibrary library)
	{
		return this.signLibraries.TryGetValue(libraryId, out library);
	}

	[CompilerGenerated]
	[PublicizedFrom(EAccessModifier.Private)]
	public void <UpdateRenderingData>g__ClearIntermediateLists|91_0(ref SignDataManager.<>c__DisplayClass91_0 A_1)
	{
		this.colorOpStack.Clear();
		SignDataManager.IIntermediateDataWrapper[] array = this.intermediateDataWrappers;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].ClearStagingList();
		}
	}

	[CompilerGenerated]
	[PublicizedFrom(EAccessModifier.Private)]
	public LayerStackInfo <UpdateRenderingData>g__ProcessLayers|91_1(List<SignData.SignLayer> layers, SignData.SignTransform parentTransform, List<SignDataManager.ColorOperation> colorOps, SignDataManager.GroupOffsets parentOffsets = default(SignDataManager.GroupOffsets), ref SignDataManager.<>c__DisplayClass91_0 A_5)
	{
		int num = 0;
		LayerStackInfo layerStackInfo = new LayerStackInfo(A_5.compStackIndex, A_5.uvStackIndex);
		foreach (SignData.SignLayer signLayer in layers)
		{
			LayerComplexityInfo layerComplexityInfo = default(LayerComplexityInfo);
			layerComplexityInfo.BaseOffset = A_5.signRenderingData.totalComplexity;
			layerComplexityInfo.StartCompStackIndex = A_5.compStackIndex;
			layerComplexityInfo.MaxCompStackIndex = A_5.compStackIndex;
			layerComplexityInfo.StartUVStackIndex = A_5.uvStackIndex;
			layerComplexityInfo.MaxUVStackIndex = A_5.uvStackIndex;
			layerComplexityInfo.GroupDepth = 0;
			int count = this.layerDescriptors.List.Count;
			int totalDraws = A_5.signRenderingData.totalDraws;
			int totalWarps = A_5.signRenderingData.totalWarps;
			bool flag = false;
			SignData.SignRenderSettings.Mode mode = signLayer.renderSettings.mode;
			if (mode != SignData.SignRenderSettings.Mode.ColorOnly && mode - SignData.SignRenderSettings.Mode.ColorAndMask <= 2)
			{
				this.layerDescriptors.List.Add(new SignDataManager.LayerDescriptor
				{
					type = 3,
					index = (int)signLayer.renderSettings.mode
				});
				flag = true;
				A_5.signRenderingData.totalComplexity += 0.5f;
				int compStackIndex = A_5.compStackIndex;
				A_5.compStackIndex = compStackIndex + 1;
				layerComplexityInfo.MaxCompStackIndex = A_5.compStackIndex;
			}
			SignData.GroupSignLayer groupSignLayer = signLayer as SignData.GroupSignLayer;
			if (groupSignLayer == null)
			{
				SignData.TextSignLayer textSignLayer = signLayer as SignData.TextSignLayer;
				if (textSignLayer == null)
				{
					SignData.PolygonSignLayer polygonSignLayer = signLayer as SignData.PolygonSignLayer;
					if (polygonSignLayer == null)
					{
						SignData.NoiseSignLayer noiseSignLayer = signLayer as SignData.NoiseSignLayer;
						if (noiseSignLayer != null)
						{
							if (signLayer.HasTransformOrWarps || parentTransform != SignData.SignTransform.Identity)
							{
								this.<UpdateRenderingData>g__PushWarps|91_5(parentTransform, signLayer, ref A_5);
								layerComplexityInfo.MaxUVStackIndex = A_5.uvStackIndex;
								this.<UpdateRenderingData>g__ProcessNoiseLayer|91_3(noiseSignLayer, colorOps, parentOffsets, ref A_5);
								this.<UpdateRenderingData>g__PopWarps|91_6(ref A_5);
							}
							else
							{
								this.<UpdateRenderingData>g__ProcessNoiseLayer|91_3(noiseSignLayer, colorOps, parentOffsets, ref A_5);
							}
						}
					}
					else if (signLayer.HasTransformOrWarps || parentTransform != SignData.SignTransform.Identity)
					{
						this.<UpdateRenderingData>g__PushWarps|91_5(parentTransform, signLayer, ref A_5);
						layerComplexityInfo.MaxUVStackIndex = A_5.uvStackIndex;
						this.<UpdateRenderingData>g__ProcessPolygonLayer|91_2(polygonSignLayer, colorOps, parentOffsets, ref A_5);
						this.<UpdateRenderingData>g__PopWarps|91_6(ref A_5);
					}
					else
					{
						this.<UpdateRenderingData>g__ProcessPolygonLayer|91_2(polygonSignLayer, colorOps, parentOffsets, ref A_5);
					}
				}
				else if (signLayer.HasTransformOrWarps || parentTransform != SignData.SignTransform.Identity)
				{
					this.<UpdateRenderingData>g__PushWarps|91_5(parentTransform, signLayer, ref A_5);
					layerComplexityInfo.MaxUVStackIndex = A_5.uvStackIndex;
					this.<UpdateRenderingData>g__ProcessTextLayer|91_4(textSignLayer, colorOps, parentOffsets, ref A_5);
					this.<UpdateRenderingData>g__PopWarps|91_6(ref A_5);
				}
				else
				{
					this.<UpdateRenderingData>g__ProcessTextLayer|91_4(textSignLayer, colorOps, parentOffsets, ref A_5);
				}
			}
			else
			{
				SignDataManager.GroupOffsets groupOffsets = parentOffsets.WithOffsets(groupSignLayer.softnessOffset, groupSignLayer.dilateOffset, groupSignLayer.offsetTarget);
				Color color = groupSignLayer.renderSettings.color;
				bool flag2 = (groupSignLayer.colorMode == SignData.GroupSignLayer.ColorMode.Multiply && color == Color.white) || (groupSignLayer.colorMode == SignData.GroupSignLayer.ColorMode.Blend && color.a == 0f);
				if (!flag2)
				{
					colorOps.Add(new SignDataManager.ColorOperation
					{
						color = color,
						mode = groupSignLayer.colorMode
					});
				}
				if (groupSignLayer.HasWarps || !groupSignLayer.transform.HasUniformScale)
				{
					this.<UpdateRenderingData>g__PushWarps|91_5(parentTransform, signLayer, ref A_5);
					LayerStackInfo layerStackInfo2 = this.<UpdateRenderingData>g__ProcessLayers|91_1(groupSignLayer.layers, SignData.SignTransform.Identity, colorOps, groupOffsets, ref A_5);
					layerComplexityInfo.MaxCompStackIndex = layerStackInfo2.MaxCompStackIndex;
					layerComplexityInfo.MaxUVStackIndex = layerStackInfo2.MaxUVStackIndex;
					layerComplexityInfo.GroupDepth = layerStackInfo2.GroupDepth + 1;
					this.<UpdateRenderingData>g__PopWarps|91_6(ref A_5);
				}
				else
				{
					LayerStackInfo layerStackInfo3 = this.<UpdateRenderingData>g__ProcessLayers|91_1(groupSignLayer.layers, parentTransform * groupSignLayer.transform, colorOps, groupOffsets, ref A_5);
					layerComplexityInfo.MaxCompStackIndex = layerStackInfo3.MaxCompStackIndex;
					layerComplexityInfo.MaxUVStackIndex = layerStackInfo3.MaxUVStackIndex;
					layerComplexityInfo.GroupDepth = layerStackInfo3.GroupDepth + 1;
				}
				if (!flag2)
				{
					colorOps.RemoveAt(colorOps.Count - 1);
				}
			}
			if (flag)
			{
				this.layerDescriptors.List.Add(new SignDataManager.LayerDescriptor
				{
					type = 4,
					index = -1
				});
				num++;
				A_5.signRenderingData.totalComplexity += 0.55f;
			}
			else if (num > 0)
			{
				this.layerDescriptors.List.Add(new SignDataManager.LayerDescriptor
				{
					type = 5,
					index = num
				});
				A_5.signRenderingData.totalComplexity += 0.5f + 0.05f * (float)num;
				A_5.compStackIndex -= num;
				num = 0;
			}
			int num2 = this.layerDescriptors.List.Count - count;
			layerComplexityInfo.EndCompStackIndex = A_5.compStackIndex;
			layerComplexityInfo.EndUVStackIndex = A_5.uvStackIndex;
			layerComplexityInfo.Complexity = A_5.signRenderingData.totalComplexity - layerComplexityInfo.BaseOffset;
			layerComplexityInfo.WarpCount = A_5.signRenderingData.totalWarps - totalWarps;
			layerComplexityInfo.DrawCount = A_5.signRenderingData.totalDraws - totalDraws;
			layerComplexityInfo.DescriptorCount = num2;
			A_5.signRenderingData.renderingInfoByLayer[signLayer] = new SignDataManager.RuntimeRenderingInfo(count, num2);
			A_5.signRenderingData.complexityByLayer[signLayer] = layerComplexityInfo;
			layerStackInfo.MaxCompStackIndex = Mathf.Max(layerStackInfo.MaxCompStackIndex, layerComplexityInfo.MaxCompStackIndex);
			layerStackInfo.MaxUVStackIndex = Mathf.Max(layerStackInfo.MaxUVStackIndex, layerComplexityInfo.MaxUVStackIndex);
			layerStackInfo.GroupDepth = Mathf.Max(layerStackInfo.GroupDepth, layerComplexityInfo.GroupDepth);
		}
		if (num > 0)
		{
			this.layerDescriptors.List.Add(new SignDataManager.LayerDescriptor
			{
				type = 5,
				index = num
			});
			A_5.signRenderingData.totalComplexity += 0.5f + 0.05f * (float)num;
			A_5.compStackIndex -= num;
			num = 0;
		}
		return layerStackInfo;
	}

	[CompilerGenerated]
	[PublicizedFrom(EAccessModifier.Private)]
	public void <UpdateRenderingData>g__PushWarps|91_5(SignData.SignTransform parentTransform, SignData.SignLayer layer, ref SignDataManager.<>c__DisplayClass91_0 A_3)
	{
		this.layerDescriptors.List.Add(new SignDataManager.LayerDescriptor
		{
			type = 2,
			index = -1
		});
		A_3.signRenderingData.totalComplexity += 0.5f;
		int uvStackIndex = A_3.uvStackIndex;
		A_3.uvStackIndex = uvStackIndex + 1;
		this.layerDescriptors.List.Add(new SignDataManager.LayerDescriptor
		{
			type = 10,
			index = this.transformWarps.List.Count
		});
		SignData.SignTransform signTransform = parentTransform * layer.transform;
		this.transformWarps.List.Add(new SignDataManager.TransformWarp
		{
			position = signTransform.position,
			rotation = signTransform.rotation * 0.017453292f,
			scale = signTransform.scale
		});
		A_3.signRenderingData.totalComplexity += 1.3f;
		A_3.signRenderingData.totalWarps++;
		for (int i = layer.warps.Count - 1; i >= 0; i--)
		{
			SignData.SignWarp signWarp = layer.warps[i];
			SignData.SkewWarp skewWarp = signWarp as SignData.SkewWarp;
			if (skewWarp == null)
			{
				SignData.BulgeWarp bulgeWarp = signWarp as SignData.BulgeWarp;
				if (bulgeWarp == null)
				{
					SignData.TwirlWarp twirlWarp = signWarp as SignData.TwirlWarp;
					if (twirlWarp == null)
					{
						SignData.KaleidoWarp kaleidoWarp = signWarp as SignData.KaleidoWarp;
						if (kaleidoWarp == null)
						{
							SignData.PerspectiveWarp perspectiveWarp = signWarp as SignData.PerspectiveWarp;
							if (perspectiveWarp == null)
							{
								SignData.ArcWarp arcWarp = signWarp as SignData.ArcWarp;
								if (arcWarp == null)
								{
									SignData.StretchWarp stretchWarp = signWarp as SignData.StretchWarp;
									if (stretchWarp == null)
									{
										SignData.GridWarp gridWarp = signWarp as SignData.GridWarp;
										if (gridWarp == null)
										{
											Log.Error(string.Format("Unsupported layer warp type at index {0}.", i));
										}
										else
										{
											this.layerDescriptors.List.Add(new SignDataManager.LayerDescriptor
											{
												type = 18,
												index = this.gridWarps.List.Count
											});
											this.gridWarps.List.Add(new SignDataManager.GridWarp
											{
												offset = gridWarp.offset,
												rotation = 0.017453292f * gridWarp.rotation,
												scale = gridWarp.scale,
												mode = (int)gridWarp.mode,
												aspect = gridWarp.aspect,
												shift = gridWarp.shift
											});
											float num;
											switch (gridWarp.mode)
											{
											case SignData.GridWarp.Mode.Column:
												num = 1.26f;
												break;
											case SignData.GridWarp.Mode.Rectangle:
												num = 1.46f;
												break;
											case SignData.GridWarp.Mode.Hex:
												num = 1.66f;
												break;
											default:
												num = 1.46f;
												break;
											}
											float num2 = num;
											A_3.signRenderingData.totalComplexity += 0.5f + num2;
										}
									}
									else
									{
										this.layerDescriptors.List.Add(new SignDataManager.LayerDescriptor
										{
											type = 17,
											index = this.stretchWarps.List.Count
										});
										this.stretchWarps.List.Add(new SignDataManager.StretchWarp
										{
											position = stretchWarp.offset,
											rotation = 0.017453292f * stretchWarp.rotation,
											distance = stretchWarp.distance,
											width = stretchWarp.distance * stretchWarp.width,
											exponent = stretchWarp.exponent
										});
										A_3.signRenderingData.totalComplexity += 2f;
									}
								}
								else
								{
									this.layerDescriptors.List.Add(new SignDataManager.LayerDescriptor
									{
										type = 16,
										index = this.arcWarps.List.Count
									});
									this.arcWarps.List.Add(new SignDataManager.ArcWarp
									{
										rotation = 0.017453292f * arcWarp.rotation,
										radius = arcWarp.radius,
										width = arcWarp.width
									});
									A_3.signRenderingData.totalComplexity += 1.95f;
								}
							}
							else
							{
								this.layerDescriptors.List.Add(new SignDataManager.LayerDescriptor
								{
									type = 15,
									index = this.perspectiveWarps.List.Count
								});
								this.perspectiveWarps.List.Add(new SignDataManager.PerspectiveWarp
								{
									rotation = 0.017453292f * perspectiveWarp.rotation,
									strength = Mathf.Lerp(0.01f, 5f, Mathf.Pow(1f - perspectiveWarp.strength, 2f))
								});
								A_3.signRenderingData.totalComplexity += 1.96f;
							}
						}
						else
						{
							this.layerDescriptors.List.Add(new SignDataManager.LayerDescriptor
							{
								type = 14,
								index = this.kaleidoWarps.List.Count
							});
							this.kaleidoWarps.List.Add(new SignDataManager.KaleidoWarp
							{
								position = kaleidoWarp.offset * kaleidoWarp.offsetScale,
								angle = 0.017453292f * (360f / (float)kaleidoWarp.sides),
								offset = 0.017453292f * (kaleidoWarp.rotation + 360f)
							});
							A_3.signRenderingData.totalComplexity += 1.76f;
						}
					}
					else
					{
						this.layerDescriptors.List.Add(new SignDataManager.LayerDescriptor
						{
							type = 13,
							index = this.twirlWarps.List.Count
						});
						this.twirlWarps.List.Add(new SignDataManager.TwirlWarp
						{
							position = twirlWarp.offset,
							amount = 20f * twirlWarp.amount,
							frequency = 100f * twirlWarp.frequency
						});
						A_3.signRenderingData.totalComplexity += 1.45f;
					}
				}
				else
				{
					this.layerDescriptors.List.Add(new SignDataManager.LayerDescriptor
					{
						type = 12,
						index = this.bulgeWarps.List.Count
					});
					this.bulgeWarps.List.Add(new SignDataManager.BulgeWarp
					{
						position = bulgeWarp.offset,
						amount = bulgeWarp.amount
					});
					A_3.signRenderingData.totalComplexity += 1.78f;
				}
			}
			else
			{
				this.layerDescriptors.List.Add(new SignDataManager.LayerDescriptor
				{
					type = 11,
					index = this.skewWarps.List.Count
				});
				this.skewWarps.List.Add(new SignDataManager.SkewWarp
				{
					rotation = 0.017453292f * skewWarp.rotation,
					amount = skewWarp.amount
				});
				A_3.signRenderingData.totalComplexity += 1.475f;
			}
			A_3.signRenderingData.totalWarps++;
		}
	}

	[CompilerGenerated]
	[PublicizedFrom(EAccessModifier.Private)]
	public void <UpdateRenderingData>g__PopWarps|91_6(ref SignDataManager.<>c__DisplayClass91_0 A_1)
	{
		SignDataManager.LayerDescriptor layerDescriptor = this.layerDescriptors.List[this.layerDescriptors.List.Count - 1];
		if (layerDescriptor.type == 1)
		{
			layerDescriptor.index--;
			this.layerDescriptors.List[this.layerDescriptors.List.Count - 1] = layerDescriptor;
		}
		else
		{
			this.layerDescriptors.List.Add(new SignDataManager.LayerDescriptor
			{
				type = 1,
				index = -1
			});
			A_1.signRenderingData.totalComplexity += 0.5f;
		}
		int uvStackIndex = A_1.uvStackIndex;
		A_1.uvStackIndex = uvStackIndex - 1;
	}

	[CompilerGenerated]
	[PublicizedFrom(EAccessModifier.Private)]
	public void <UpdateRenderingData>g__ProcessPolygonLayer|91_2(SignData.PolygonSignLayer polygonLayer, List<SignDataManager.ColorOperation> colorOps, SignDataManager.GroupOffsets offsets, ref SignDataManager.<>c__DisplayClass91_0 A_4)
	{
		float num = Mathf.Max(0f, polygonLayer.softness + offsets.shapeSoftnessOffset);
		float num2 = polygonLayer.dilate + offsets.shapeDilateOffset;
		Color color = NGUITools.GammaToLinearSpace(SignDataManager.ColorOperation.EvaluateColor(polygonLayer.renderSettings.color, colorOps), true);
		if (polygonLayer.smoothness > 0.99f)
		{
			SignDataManager.LayerDescriptor layerDescriptor = new SignDataManager.LayerDescriptor
			{
				type = 102,
				index = this.circleLayers.List.Count
			};
			SignDataManager.CircleLayer circleLayer = new SignDataManager.CircleLayer
			{
				color = color,
				softness = num,
				dilate = num2,
				frequency = polygonLayer.frequency,
				mode = (int)polygonLayer.shapeMode
			};
			this.layerDescriptors.List.Add(layerDescriptor);
			this.circleLayers.List.Add(circleLayer);
			A_4.signRenderingData.totalComplexity += 1.5f;
			A_4.signRenderingData.totalDraws++;
			return;
		}
		SignDataManager.LayerDescriptor layerDescriptor2 = new SignDataManager.LayerDescriptor
		{
			type = 100,
			index = this.polygonLayers.List.Count
		};
		SignDataManager.PolygonLayer polygonLayer2 = new SignDataManager.PolygonLayer
		{
			color = color,
			sides = (float)polygonLayer.sides,
			smoothness = polygonLayer.smoothness,
			starify = polygonLayer.starify,
			softness = num,
			dilate = num2,
			frequency = polygonLayer.frequency,
			mode = (int)polygonLayer.shapeMode
		};
		this.layerDescriptors.List.Add(layerDescriptor2);
		this.polygonLayers.List.Add(polygonLayer2);
		A_4.signRenderingData.totalComplexity += 2.875f;
		A_4.signRenderingData.totalDraws++;
	}

	[CompilerGenerated]
	[PublicizedFrom(EAccessModifier.Private)]
	public void <UpdateRenderingData>g__ProcessNoiseLayer|91_3(SignData.NoiseSignLayer noiseLayer, List<SignDataManager.ColorOperation> colorOps, SignDataManager.GroupOffsets offsets, ref SignDataManager.<>c__DisplayClass91_0 A_4)
	{
		SignDataManager.LayerDescriptor layerDescriptor = new SignDataManager.LayerDescriptor
		{
			type = 103,
			index = this.noiseLayers.List.Count
		};
		float num = Mathf.Max(0f, noiseLayer.softness + offsets.noiseSoftnessOffset);
		float num2 = noiseLayer.dilate + offsets.noiseDilateOffset;
		SignDataManager.NoiseLayer noiseLayer2 = new SignDataManager.NoiseLayer
		{
			color = NGUITools.GammaToLinearSpace(SignDataManager.ColorOperation.EvaluateColor(noiseLayer.renderSettings.color, colorOps), true),
			offset = (float)(noiseLayer.seed * 10),
			detail = noiseLayer.detail,
			softness = num,
			dilate = num2,
			fade = noiseLayer.fade
		};
		this.layerDescriptors.List.Add(layerDescriptor);
		this.noiseLayers.List.Add(noiseLayer2);
		A_4.signRenderingData.totalComplexity += 1.5f + 1.1f * (float)noiseLayer.detail;
		A_4.signRenderingData.totalDraws++;
	}

	[CompilerGenerated]
	[PublicizedFrom(EAccessModifier.Private)]
	public void <UpdateRenderingData>g__ProcessTextLayer|91_4(SignData.TextSignLayer textLayer, List<SignDataManager.ColorOperation> colorOps, SignDataManager.GroupOffsets offsets, ref SignDataManager.<>c__DisplayClass91_0 A_4)
	{
		SignDataManager.SignFontData signFontData;
		if (textLayer.font == null || !this.fontDataByName.TryGetValue(textLayer.font, out signFontData))
		{
			Log.Error("Failed to find font data for font `" + textLayer.font + "`.");
			if (!this.fontDataByName.TryGetValue("LiberationSans", out signFontData))
			{
				Log.Error("Failed to load default fallback font `LiberationSans`.");
				return;
			}
		}
		int count = this.charLayers.List.Count;
		Color color = NGUITools.GammaToLinearSpace(SignDataManager.ColorOperation.EvaluateColor(textLayer.renderSettings.color, colorOps), true);
		Vector2 vector = Vector2.zero;
		Vector2 vector2 = Vector2.zero;
		float num = 1f - Mathf.Abs(90f - (textLayer.direction + 360f) % 180f) / 90f;
		float num2 = textLayer.direction * 0.017453292f;
		Vector2 vector3;
		vector3..ctor(Mathf.Cos(num2), -Mathf.Sin(num2));
		float num3 = Mathf.Max(0f, textLayer.spacing - 1f) * signFontData.LineHeight;
		float num4 = Mathf.Clamp01(textLayer.spacing);
		float num5 = Mathf.Clamp01(textLayer.softness + offsets.textSoftnessOffset);
		float num6 = Mathf.Clamp(textLayer.dilate + offsets.textDilateOffset, -1f, 1f);
		float num7 = num5 * (1f - Mathf.Abs(num6));
		float num8 = num6 * (1f - 0.5f * num5);
		foreach (char c in textLayer.text)
		{
			SignDataManager.CharAtlasData charAtlasData;
			if (!signFontData.TryGetCharAtlasData(c, out charAtlasData))
			{
				Log.Warning(string.Concat(new string[]
				{
					string.Format("Could not retrieve atlas data for character \"{0}\" in string \"{1}\",", c, textLayer.text),
					" found in sign \"",
					A_4.signData.name,
					"\" (library:\"",
					A_4.id.libraryId,
					"\"). \nPlease check \"TextSignLayer\" elements in the relevant _signs.xml for invalid characters, as they may not be visible in the Sign Editor tool."
				}));
			}
			else
			{
				float num9 = Mathf.Lerp(charAtlasData.charWidth, signFontData.LineHeight, num);
				float num10 = num4 * (num9 + num3);
				Vector2 quadOffset = charAtlasData.quadOffset;
				quadOffset.x *= vector3.x;
				vector2 = vector + vector3 * Mathf.Lerp(num9, 0f, num);
				if (c == ' ')
				{
					vector += vector3 * num10;
				}
				else
				{
					SignDataManager.LayerDescriptor layerDescriptor = new SignDataManager.LayerDescriptor
					{
						type = 101,
						index = this.charLayers.List.Count
					};
					SignDataManager.CharLayer charLayer = new SignDataManager.CharLayer
					{
						position = vector + quadOffset,
						scale = charAtlasData.quadSize,
						color = color,
						atlasIndex = signFontData.AtlasIndex,
						offsetSize = charAtlasData.uvOffsetSize,
						softness = num7,
						dilate = num8
					};
					this.layerDescriptors.List.Add(layerDescriptor);
					this.charLayers.List.Add(charLayer);
					A_4.signRenderingData.totalComplexity += 2.275f;
					A_4.signRenderingData.totalDraws++;
					vector += vector3 * num10;
				}
			}
		}
		Vector2 vector4 = -0.5f * vector2;
		for (int j = count; j < this.charLayers.List.Count; j++)
		{
			SignDataManager.CharLayer charLayer2 = this.charLayers.List[j];
			charLayer2.position += vector4;
			this.charLayers.List[j] = charLayer2;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const string fontConfigLoc = "@:Fonts/SignFontConfig.asset";

	[PublicizedFrom(EAccessModifier.Private)]
	public const float baseDescriptorComplexity = 0.5f;

	public const string defaultFontName = "LiberationSans";

	public const string defaultLibraryId = "[D]";

	public const string internalLibraryId = "[I]";

	public const string userLibraryId = "[U]";

	public const float cMaxComplexity = 600f;

	public const int cMaxCompStackIndex = 7;

	public const int cMaxUVStackIndex = 7;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<string, SignDataManager.SignFontData> fontDataByName = new Dictionary<string, SignDataManager.SignFontData>();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<string, SignLibrary> signLibraries = new Dictionary<string, SignLibrary>(100);

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<GlobalSignId, SignDataManager.SignRenderingData> renderingDataByID = new Dictionary<GlobalSignId, SignDataManager.SignRenderingData>(100);

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly SignDataManager.UpdateListenerMap updateListenerMap = new SignDataManager.UpdateListenerMap(100);

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly SignDataManager.IntermediateDataWrapper<SignDataManager.LayerDescriptor> layerDescriptors = new SignDataManager.IntermediateDataWrapper<SignDataManager.LayerDescriptor>(SignShaderIDs._LayerDescriptors, 100, (SignDataManager.SignRenderingData srd) => srd.descriptorBuffer, delegate(SignDataManager.SignRenderingData srd, ComputeBuffer b)
	{
		srd.descriptorBuffer = b;
	});

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly SignDataManager.IntermediateDataWrapper<SignDataManager.CircleLayer> circleLayers = new SignDataManager.IntermediateDataWrapper<SignDataManager.CircleLayer>(SignShaderIDs._CircleLayers, 100, (SignDataManager.SignRenderingData srd) => srd.circleBuffer, delegate(SignDataManager.SignRenderingData srd, ComputeBuffer b)
	{
		srd.circleBuffer = b;
	});

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly SignDataManager.IntermediateDataWrapper<SignDataManager.PolygonLayer> polygonLayers = new SignDataManager.IntermediateDataWrapper<SignDataManager.PolygonLayer>(SignShaderIDs._PolygonLayers, 100, (SignDataManager.SignRenderingData srd) => srd.polygonBuffer, delegate(SignDataManager.SignRenderingData srd, ComputeBuffer b)
	{
		srd.polygonBuffer = b;
	});

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly SignDataManager.IntermediateDataWrapper<SignDataManager.CharLayer> charLayers = new SignDataManager.IntermediateDataWrapper<SignDataManager.CharLayer>(SignShaderIDs._CharLayers, 100, (SignDataManager.SignRenderingData srd) => srd.charBuffer, delegate(SignDataManager.SignRenderingData srd, ComputeBuffer b)
	{
		srd.charBuffer = b;
	});

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly SignDataManager.IntermediateDataWrapper<SignDataManager.NoiseLayer> noiseLayers = new SignDataManager.IntermediateDataWrapper<SignDataManager.NoiseLayer>(SignShaderIDs._NoiseLayers, 100, (SignDataManager.SignRenderingData srd) => srd.noiseBuffer, delegate(SignDataManager.SignRenderingData srd, ComputeBuffer b)
	{
		srd.noiseBuffer = b;
	});

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly SignDataManager.IntermediateDataWrapper<SignDataManager.TransformWarp> transformWarps = new SignDataManager.IntermediateDataWrapper<SignDataManager.TransformWarp>(SignShaderIDs._TransformWarps, 100, (SignDataManager.SignRenderingData srd) => srd.transformBuffer, delegate(SignDataManager.SignRenderingData srd, ComputeBuffer b)
	{
		srd.transformBuffer = b;
	});

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly SignDataManager.IntermediateDataWrapper<SignDataManager.SkewWarp> skewWarps = new SignDataManager.IntermediateDataWrapper<SignDataManager.SkewWarp>(SignShaderIDs._SkewWarps, 100, (SignDataManager.SignRenderingData srd) => srd.skewBuffer, delegate(SignDataManager.SignRenderingData srd, ComputeBuffer b)
	{
		srd.skewBuffer = b;
	});

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly SignDataManager.IntermediateDataWrapper<SignDataManager.BulgeWarp> bulgeWarps = new SignDataManager.IntermediateDataWrapper<SignDataManager.BulgeWarp>(SignShaderIDs._BulgeWarps, 100, (SignDataManager.SignRenderingData srd) => srd.bulgeBuffer, delegate(SignDataManager.SignRenderingData srd, ComputeBuffer b)
	{
		srd.bulgeBuffer = b;
	});

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly SignDataManager.IntermediateDataWrapper<SignDataManager.TwirlWarp> twirlWarps = new SignDataManager.IntermediateDataWrapper<SignDataManager.TwirlWarp>(SignShaderIDs._TwirlWarps, 100, (SignDataManager.SignRenderingData srd) => srd.twirlBuffer, delegate(SignDataManager.SignRenderingData srd, ComputeBuffer b)
	{
		srd.twirlBuffer = b;
	});

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly SignDataManager.IntermediateDataWrapper<SignDataManager.KaleidoWarp> kaleidoWarps = new SignDataManager.IntermediateDataWrapper<SignDataManager.KaleidoWarp>(SignShaderIDs._KaleidoWarps, 100, (SignDataManager.SignRenderingData srd) => srd.kaleidoBuffer, delegate(SignDataManager.SignRenderingData srd, ComputeBuffer b)
	{
		srd.kaleidoBuffer = b;
	});

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly SignDataManager.IntermediateDataWrapper<SignDataManager.PerspectiveWarp> perspectiveWarps = new SignDataManager.IntermediateDataWrapper<SignDataManager.PerspectiveWarp>(SignShaderIDs._PerspectiveWarps, 100, (SignDataManager.SignRenderingData srd) => srd.perspectiveBuffer, delegate(SignDataManager.SignRenderingData srd, ComputeBuffer b)
	{
		srd.perspectiveBuffer = b;
	});

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly SignDataManager.IntermediateDataWrapper<SignDataManager.ArcWarp> arcWarps = new SignDataManager.IntermediateDataWrapper<SignDataManager.ArcWarp>(SignShaderIDs._ArcWarps, 100, (SignDataManager.SignRenderingData srd) => srd.arcBuffer, delegate(SignDataManager.SignRenderingData srd, ComputeBuffer b)
	{
		srd.arcBuffer = b;
	});

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly SignDataManager.IntermediateDataWrapper<SignDataManager.StretchWarp> stretchWarps = new SignDataManager.IntermediateDataWrapper<SignDataManager.StretchWarp>(SignShaderIDs._StretchWarps, 100, (SignDataManager.SignRenderingData srd) => srd.stretchBuffer, delegate(SignDataManager.SignRenderingData srd, ComputeBuffer b)
	{
		srd.stretchBuffer = b;
	});

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly SignDataManager.IntermediateDataWrapper<SignDataManager.GridWarp> gridWarps = new SignDataManager.IntermediateDataWrapper<SignDataManager.GridWarp>(SignShaderIDs._GridWarps, 100, (SignDataManager.SignRenderingData srd) => srd.gridBuffer, delegate(SignDataManager.SignRenderingData srd, ComputeBuffer b)
	{
		srd.gridBuffer = b;
	});

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly SignDataManager.IIntermediateDataWrapper[] intermediateDataWrappers;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<SignDataManager.ColorOperation> colorOpStack = new List<SignDataManager.ColorOperation>();

	[PublicizedFrom(EAccessModifier.Private)]
	public Texture2DArray fontAtlasTextureArray;

	[PublicizedFrom(EAccessModifier.Private)]
	public static SignDataManager m_Instance;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cSignSyncBatchBytes = 1048576;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool signDataDownloadInProgress;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool signDataDownloadComplete;

	[PublicizedFrom(EAccessModifier.Private)]
	public struct RuntimeRenderingInfo
	{
		public RuntimeRenderingInfo(int baseDescriptorIndex, int descriptorCount)
		{
			this.BaseDescriptorIndex = baseDescriptorIndex;
			this.DescriptorCount = descriptorCount;
		}

		public readonly int BaseDescriptorIndex;

		public readonly int DescriptorCount;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public class SignRenderingData : IDisposable
	{
		public SignRenderingData()
		{
			this.materialPropertyBlock = new MaterialPropertyBlock();
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public virtual void Dispose(bool disposing)
		{
			if (!this.disposedValue)
			{
				if (disposing)
				{
					MaterialPropertyBlock materialPropertyBlock = this.materialPropertyBlock;
					if (materialPropertyBlock != null)
					{
						materialPropertyBlock.Clear();
					}
					this.materialPropertyBlock = null;
					this.renderingInfoByLayer.Clear();
					this.complexityByLayer.Clear();
				}
				this.DisposeBuffers();
				this.disposedValue = true;
			}
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public ~SignRenderingData()
		{
			this.Dispose(false);
		}

		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		public void Reset()
		{
			this.layerCount = 0;
			this.totalComplexity = 0f;
			this.totalWarps = 0;
			this.totalDraws = 0;
			this.stackInfo = default(LayerStackInfo);
			MaterialPropertyBlock materialPropertyBlock = this.materialPropertyBlock;
			if (materialPropertyBlock != null)
			{
				materialPropertyBlock.Clear();
			}
			this.renderingInfoByLayer.Clear();
			this.complexityByLayer.Clear();
			this.DisposeBuffers();
		}

		public void DisposeBuffers()
		{
			ComputeBuffer computeBuffer = this.descriptorBuffer;
			if (computeBuffer != null)
			{
				computeBuffer.Dispose();
			}
			this.descriptorBuffer = null;
			ComputeBuffer computeBuffer2 = this.circleBuffer;
			if (computeBuffer2 != null)
			{
				computeBuffer2.Dispose();
			}
			this.circleBuffer = null;
			ComputeBuffer computeBuffer3 = this.polygonBuffer;
			if (computeBuffer3 != null)
			{
				computeBuffer3.Dispose();
			}
			this.polygonBuffer = null;
			ComputeBuffer computeBuffer4 = this.charBuffer;
			if (computeBuffer4 != null)
			{
				computeBuffer4.Dispose();
			}
			this.charBuffer = null;
			ComputeBuffer computeBuffer5 = this.noiseBuffer;
			if (computeBuffer5 != null)
			{
				computeBuffer5.Dispose();
			}
			this.noiseBuffer = null;
			ComputeBuffer computeBuffer6 = this.transformBuffer;
			if (computeBuffer6 != null)
			{
				computeBuffer6.Dispose();
			}
			this.transformBuffer = null;
			ComputeBuffer computeBuffer7 = this.skewBuffer;
			if (computeBuffer7 != null)
			{
				computeBuffer7.Dispose();
			}
			this.skewBuffer = null;
			ComputeBuffer computeBuffer8 = this.bulgeBuffer;
			if (computeBuffer8 != null)
			{
				computeBuffer8.Dispose();
			}
			this.bulgeBuffer = null;
			ComputeBuffer computeBuffer9 = this.twirlBuffer;
			if (computeBuffer9 != null)
			{
				computeBuffer9.Dispose();
			}
			this.twirlBuffer = null;
			ComputeBuffer computeBuffer10 = this.kaleidoBuffer;
			if (computeBuffer10 != null)
			{
				computeBuffer10.Dispose();
			}
			this.kaleidoBuffer = null;
			ComputeBuffer computeBuffer11 = this.perspectiveBuffer;
			if (computeBuffer11 != null)
			{
				computeBuffer11.Dispose();
			}
			this.perspectiveBuffer = null;
			ComputeBuffer computeBuffer12 = this.arcBuffer;
			if (computeBuffer12 != null)
			{
				computeBuffer12.Dispose();
			}
			this.arcBuffer = null;
			ComputeBuffer computeBuffer13 = this.stretchBuffer;
			if (computeBuffer13 != null)
			{
				computeBuffer13.Dispose();
			}
			this.stretchBuffer = null;
			ComputeBuffer computeBuffer14 = this.gridBuffer;
			if (computeBuffer14 != null)
			{
				computeBuffer14.Dispose();
			}
			this.gridBuffer = null;
		}

		public static ComputeBuffer DummyBuffer = new ComputeBuffer(1, 4);

		public MaterialPropertyBlock materialPropertyBlock;

		public int layerCount;

		public ComputeBuffer descriptorBuffer;

		public ComputeBuffer circleBuffer;

		public ComputeBuffer polygonBuffer;

		public ComputeBuffer charBuffer;

		public ComputeBuffer noiseBuffer;

		public ComputeBuffer transformBuffer;

		public ComputeBuffer skewBuffer;

		public ComputeBuffer bulgeBuffer;

		public ComputeBuffer twirlBuffer;

		public ComputeBuffer kaleidoBuffer;

		public ComputeBuffer perspectiveBuffer;

		public ComputeBuffer arcBuffer;

		public ComputeBuffer stretchBuffer;

		public ComputeBuffer gridBuffer;

		public readonly Dictionary<SignData.SignLayer, SignDataManager.RuntimeRenderingInfo> renderingInfoByLayer = new Dictionary<SignData.SignLayer, SignDataManager.RuntimeRenderingInfo>();

		public readonly Dictionary<SignData.SignLayer, LayerComplexityInfo> complexityByLayer = new Dictionary<SignData.SignLayer, LayerComplexityInfo>();

		public float totalComplexity;

		public int totalWarps;

		public int totalDraws;

		public LayerStackInfo stackInfo;

		[PublicizedFrom(EAccessModifier.Private)]
		public bool disposedValue;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public class UpdateListenerMap
	{
		public UpdateListenerMap(int capacity)
		{
			this._idsByListener = new Dictionary<ISignRenderingDataUpdateListener, HashSet<GlobalSignId>>(capacity);
			this._listenersById = new Dictionary<GlobalSignId, HashSet<ISignRenderingDataUpdateListener>>(capacity);
		}

		public void RegisterListener(GlobalSignId id, ISignRenderingDataUpdateListener listener)
		{
			HashSet<ISignRenderingDataUpdateListener> hashSet;
			if (!this._listenersById.TryGetValue(id, out hashSet))
			{
				hashSet = new HashSet<ISignRenderingDataUpdateListener>(5);
				this._listenersById[id] = hashSet;
			}
			hashSet.Add(listener);
			HashSet<GlobalSignId> hashSet2;
			if (!this._idsByListener.TryGetValue(listener, out hashSet2))
			{
				hashSet2 = new HashSet<GlobalSignId>(1);
				this._idsByListener[listener] = hashSet2;
			}
			hashSet2.Add(id);
		}

		public void DeregisterListener(ISignRenderingDataUpdateListener listener)
		{
			HashSet<GlobalSignId> hashSet;
			if (this._idsByListener.TryGetValue(listener, out hashSet))
			{
				foreach (GlobalSignId globalSignId in hashSet)
				{
					HashSet<ISignRenderingDataUpdateListener> hashSet2;
					if (this._listenersById.TryGetValue(globalSignId, out hashSet2))
					{
						hashSet2.Remove(listener);
						if (hashSet2.Count == 0)
						{
							this._listenersById.Remove(globalSignId);
						}
					}
				}
				this._idsByListener.Remove(listener);
			}
		}

		public IEnumerable<ISignRenderingDataUpdateListener> GetListeners(GlobalSignId id)
		{
			HashSet<ISignRenderingDataUpdateListener> hashSet;
			if (!this._listenersById.TryGetValue(id, out hashSet))
			{
				return Array.Empty<ISignRenderingDataUpdateListener>();
			}
			return hashSet;
		}

		public IEnumerable<GlobalSignId> GetIds(ISignRenderingDataUpdateListener listener)
		{
			HashSet<GlobalSignId> hashSet;
			if (!this._idsByListener.TryGetValue(listener, out hashSet))
			{
				return Array.Empty<GlobalSignId>();
			}
			return hashSet;
		}

		public void Clear()
		{
			this._listenersById.Clear();
			this._idsByListener.Clear();
		}

		public bool HasListeners(GlobalSignId signId)
		{
			return this._listenersById.ContainsKey(signId);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly Dictionary<GlobalSignId, HashSet<ISignRenderingDataUpdateListener>> _listenersById;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly Dictionary<ISignRenderingDataUpdateListener, HashSet<GlobalSignId>> _idsByListener;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public enum LayerTypeID
	{
		UvClear,
		UvPop,
		UvPush,
		MaskBegin,
		MaskFlipPhase,
		MaskEnd,
		WarpTransform = 10,
		WarpSkew,
		WarpBulge,
		WarpTwirl,
		WarpKaleido,
		WarpPerspective,
		WarpArc,
		WarpStretch,
		WarpGrid,
		Polygon = 100,
		Char,
		Circle,
		Noise
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public struct LayerDescriptor
	{
		public int type;

		public int index;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public struct CircleLayer
	{
		public Vector4 color;

		public float softness;

		public float dilate;

		public float frequency;

		public int mode;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public struct PolygonLayer
	{
		public Vector4 color;

		public float sides;

		public float smoothness;

		public float starify;

		public float softness;

		public float dilate;

		public float frequency;

		public int mode;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public struct CharLayer
	{
		public Vector2 position;

		public Vector2 scale;

		public Vector4 color;

		public int atlasIndex;

		public Vector4 offsetSize;

		public float softness;

		public float dilate;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public struct NoiseLayer
	{
		public Vector4 color;

		public float offset;

		public int detail;

		public float softness;

		public float dilate;

		public float fade;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public struct TransformWarp
	{
		public Vector2 position;

		public float rotation;

		public Vector2 scale;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public struct SkewWarp
	{
		public float rotation;

		public Vector2 amount;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public struct BulgeWarp
	{
		public Vector2 position;

		public float amount;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public struct TwirlWarp
	{
		public Vector2 position;

		public float amount;

		public float frequency;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public struct KaleidoWarp
	{
		public Vector2 position;

		public float angle;

		public float offset;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public struct PerspectiveWarp
	{
		public Vector3 rotation;

		public float strength;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public struct ArcWarp
	{
		public float rotation;

		public float radius;

		public float width;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public struct StretchWarp
	{
		public Vector2 position;

		public float rotation;

		public float distance;

		public float width;

		public float exponent;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public struct GridWarp
	{
		public Vector2 offset;

		public float rotation;

		public float scale;

		public int mode;

		public float aspect;

		public float shift;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public class CharAtlasData
	{
		public CharAtlasData(Vector2 quadOffset, Vector2 quadSize, Vector4 uvOffsetSize, float charWidth)
		{
			this.quadOffset = quadOffset;
			this.quadSize = quadSize;
			this.uvOffsetSize = uvOffsetSize;
			this.charWidth = charWidth;
		}

		public Vector2 quadOffset;

		public Vector2 quadSize;

		public Vector4 uvOffsetSize;

		public float charWidth;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public class SignFontData
	{
		public int AtlasIndex
		{
			get; [PublicizedFrom(EAccessModifier.Private)]
			set;
		}

		public Texture2D AtlasTexture
		{
			get; [PublicizedFrom(EAccessModifier.Private)]
			set;
		}

		public float LineHeight
		{
			get; [PublicizedFrom(EAccessModifier.Private)]
			set;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void ConstructAtlasDataMap(TMP_FontAsset fontAsset)
		{
			float num = (float)fontAsset.atlasPadding;
			float num2 = 1f / (float)fontAsset.atlasWidth;
			float num3 = 1f / (float)fontAsset.atlasHeight;
			float num4 = (float)(fontAsset.atlasWidth / fontAsset.atlasHeight);
			float num5 = 1f / (num3 * (float)fontAsset.creationSettings.pointSize);
			this.LineHeight = fontAsset.faceInfo.lineHeight * num4 * num2 * num5;
			foreach (KeyValuePair<uint, TMP_Character> keyValuePair in fontAsset.characterLookupTable)
			{
				GlyphRect glyphRect = keyValuePair.Value.glyph.glyphRect;
				Vector4 vector = default(Vector4);
				vector.x = num2 * ((float)glyphRect.x - num);
				vector.y = num3 * ((float)glyphRect.y - num);
				vector.z = num2 * ((float)glyphRect.width + 2f * num);
				vector.w = num3 * ((float)glyphRect.height + 2f * num);
				float num6 = keyValuePair.Value.glyph.metrics.horizontalAdvance * num4 * num2 * num5;
				Vector2 vector2 = new Vector2(0.5f * num6, (-0.5f * ((float)glyphRect.height + fontAsset.faceInfo.capLine) + keyValuePair.Value.glyph.metrics.horizontalBearingY) * num3 * num5);
				Vector2 vector3 = new Vector2(vector.z * num4, vector.w) * num5;
				SignDataManager.CharAtlasData charAtlasData = new SignDataManager.CharAtlasData(vector2, vector3, vector, num6);
				this.charToAtlasDataMap[keyValuePair.Key] = charAtlasData;
			}
		}

		public SignFontData(int atlasIndex, TMP_FontAsset fontAsset)
		{
			this.AtlasIndex = atlasIndex;
			this.AtlasTexture = fontAsset.atlasTexture;
			this.ConstructAtlasDataMap(fontAsset);
		}

		public bool TryGetCharAtlasData(char character, out SignDataManager.CharAtlasData atlasData)
		{
			return this.charToAtlasDataMap.TryGetValue((uint)character, out atlasData);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public Dictionary<uint, SignDataManager.CharAtlasData> charToAtlasDataMap = new Dictionary<uint, SignDataManager.CharAtlasData>();
	}

	public struct ColorOperation
	{
		public static Color EvaluateColor(Color leafColor, List<SignDataManager.ColorOperation> colorOps)
		{
			Color color = leafColor;
			for (int i = colorOps.Count - 1; i >= 0; i--)
			{
				SignDataManager.ColorOperation colorOperation = colorOps[i];
				switch (colorOperation.mode)
				{
				case SignData.GroupSignLayer.ColorMode.Multiply:
					color *= colorOperation.color;
					break;
				case SignData.GroupSignLayer.ColorMode.Blend:
				{
					float a = colorOperation.color.a;
					color..ctor(Mathf.Lerp(color.r, colorOperation.color.r, a), Mathf.Lerp(color.g, colorOperation.color.g, a), Mathf.Lerp(color.b, colorOperation.color.b, a), color.a);
					break;
				}
				case SignData.GroupSignLayer.ColorMode.Override:
					color = colorOperation.color;
					break;
				}
			}
			return color;
		}

		public Color color;

		public SignData.GroupSignLayer.ColorMode mode;
	}

	public struct GroupOffsets
	{
		public SignDataManager.GroupOffsets WithOffsets(float softness, float dilate, SignData.GroupSignLayer.OffsetTarget target)
		{
			SignDataManager.GroupOffsets groupOffsets = this;
			switch (target)
			{
			case SignData.GroupSignLayer.OffsetTarget.All:
				groupOffsets.shapeSoftnessOffset += softness;
				groupOffsets.shapeDilateOffset += dilate;
				groupOffsets.textSoftnessOffset += softness;
				groupOffsets.textDilateOffset += dilate;
				groupOffsets.noiseSoftnessOffset += softness;
				groupOffsets.noiseDilateOffset += dilate;
				break;
			case SignData.GroupSignLayer.OffsetTarget.Shapes:
				groupOffsets.shapeSoftnessOffset += softness;
				groupOffsets.shapeDilateOffset += dilate;
				break;
			case SignData.GroupSignLayer.OffsetTarget.Text:
				groupOffsets.textSoftnessOffset += softness;
				groupOffsets.textDilateOffset += dilate;
				break;
			case SignData.GroupSignLayer.OffsetTarget.Noise:
				groupOffsets.noiseSoftnessOffset += softness;
				groupOffsets.noiseDilateOffset += dilate;
				break;
			}
			return groupOffsets;
		}

		public float shapeSoftnessOffset;

		public float shapeDilateOffset;

		public float textSoftnessOffset;

		public float textDilateOffset;

		public float noiseSoftnessOffset;

		public float noiseDilateOffset;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public interface IIntermediateDataWrapper
	{
		void UpdateBuffer(SignDataManager.SignRenderingData signRenderingData);

		void ApplyToMaterial(SignDataManager.SignRenderingData signRenderingData, Material material);

		void ClearStagingList();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public class IntermediateDataWrapper<T> : SignDataManager.IIntermediateDataWrapper where T : struct
	{
		public IntermediateDataWrapper(int shaderPropertyId, int capacity, Func<SignDataManager.SignRenderingData, ComputeBuffer> getter, Action<SignDataManager.SignRenderingData, ComputeBuffer> setter)
		{
			this._shaderPropertyId = shaderPropertyId;
			this.List = new List<T>(capacity);
			this._get = getter;
			this._set = setter;
		}

		public void UpdateBuffer(SignDataManager.SignRenderingData signRenderingData)
		{
			ComputeBuffer computeBuffer = this._get(signRenderingData);
			if (this.List.Count > 0)
			{
				int num = SignDataManager.IntermediateDataWrapper<T>.NextPow2(this.List.Count);
				if (computeBuffer == null || computeBuffer.count != num)
				{
					if (computeBuffer != null)
					{
						computeBuffer.Dispose();
					}
					computeBuffer = new ComputeBuffer(num, SignDataManager.IntermediateDataWrapper<T>._stride)
					{
						name = string.Format("{0}:{1}", typeof(T).Name, num)
					};
					this._set(signRenderingData, computeBuffer);
				}
				computeBuffer.SetData<T>(this.List, 0, 0, this.List.Count);
			}
			else if (computeBuffer != null)
			{
				computeBuffer.Dispose();
				computeBuffer = null;
				this._set(signRenderingData, null);
			}
			signRenderingData.materialPropertyBlock.SetBuffer(this._shaderPropertyId, computeBuffer ?? SignDataManager.SignRenderingData.DummyBuffer);
		}

		public void ApplyToMaterial(SignDataManager.SignRenderingData signRenderingData, Material material)
		{
			material.SetBuffer(this._shaderPropertyId, this._get(signRenderingData) ?? SignDataManager.SignRenderingData.DummyBuffer);
		}

		public void ClearStagingList()
		{
			this.List.Clear();
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static int NextPow2(int v)
		{
			if (v <= 4)
			{
				return 4;
			}
			v--;
			v |= v >> 1;
			v |= v >> 2;
			v |= v >> 4;
			v |= v >> 8;
			v |= v >> 16;
			return v + 1;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly int _stride = Marshal.SizeOf(typeof(T));

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly int _shaderPropertyId;

		public readonly List<T> List;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly Func<SignDataManager.SignRenderingData, ComputeBuffer> _get;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly Action<SignDataManager.SignRenderingData, ComputeBuffer> _set;
	}

	public delegate void RenderingDataPatcher(MaterialPropertyBlock mpb);
}
