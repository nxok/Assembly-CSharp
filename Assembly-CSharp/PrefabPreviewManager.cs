using System;
using System.Collections.Generic;
using Platform;
using UnityEngine;
using WorldGenerationEngineFinal;

public class PrefabPreviewManager
{
	public static WorldBuilder WorldBuilder
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			XUiC_WorldGenerationWindow instance = XUiC_WorldGenerationWindow.Instance;
			if (instance == null)
			{
				return null;
			}
			return instance.WorldBuilder;
		}
	}

	public PrefabPreviewManager(XUiC_WorldGenerationPreview _previewWindow)
	{
		this.previewWindow = _previewWindow;
	}

	public void InitPrefabs()
	{
		if (this.Initialized)
		{
			return;
		}
		this.prefabsAround.Clear();
		PrefabPreviewManager.WorldBuilder.PrefabManager.GetPrefabsAround(this.previewWindow.GetCameraPosition(), 100000f, this.prefabsAround);
		this.UpdatePrefabsAround(this.prefabsAround);
		this.Initialized = true;
	}

	public void RemovePrefabs()
	{
		this.ClearDisplayedPrefabs();
		this.displayedPrefabs.Clear();
		this.prefabsAround.Clear();
		this.Initialized = false;
	}

	public void Update()
	{
		if (Time.time - this.lastDisplayUpdate < 2f)
		{
			return;
		}
		this.lastDisplayUpdate = Time.time;
		this.ForceUpdate();
	}

	public void ForceUpdate()
	{
		this.UpdateDisplay();
		if (Time.time - this.lastTime < 2f)
		{
			return;
		}
		this.lastTime = Time.time;
		if (PrefabPreviewManager.WorldBuilder == null)
		{
			return;
		}
		if (XUiC_WorldGenerationWindow.Instance == null)
		{
			return;
		}
		if (PrefabPreviewManager.WorldBuilder.PrefabManager.UsedPrefabsWorld == null)
		{
			return;
		}
		this.InitPrefabs();
	}

	public void ClearDisplayedPrefabs()
	{
		if (this.displayedPrefabs == null || this.displayedPrefabs.Count == 0)
		{
			return;
		}
		List<int> list = new List<int>();
		foreach (KeyValuePair<int, PrefabPreviewManager.PrefabGameObject> keyValuePair in this.displayedPrefabs)
		{
			list.Add(keyValuePair.Key);
		}
		foreach (int num in list)
		{
			if (!(this.displayedPrefabs[num].Go == null))
			{
				MeshFilter[] componentsInChildren = this.displayedPrefabs[num].Go.GetComponentsInChildren<MeshFilter>();
				for (int i = 0; i < componentsInChildren.Length; i++)
				{
					Object.Destroy(componentsInChildren[i].mesh);
				}
				Renderer[] componentsInChildren2 = this.displayedPrefabs[num].Go.GetComponentsInChildren<Renderer>();
				for (int j = 0; j < componentsInChildren2.Length; j++)
				{
					Object.Destroy(componentsInChildren2[j].material);
				}
				Object.Destroy(this.displayedPrefabs[num].Go);
				this.displayedPrefabs.Remove(num);
			}
		}
	}

	public void UpdatePrefabsAround(Dictionary<int, PrefabDataInstance> _prefabsAround)
	{
		foreach (KeyValuePair<int, PrefabDataInstance> keyValuePair in _prefabsAround)
		{
			PrefabDataInstance value = keyValuePair.Value;
			if (!this.displayedPrefabs.ContainsKey(value.id))
			{
				string name = value.location.Name;
				if (PathAbstractions.PrefabImpostersSearchPaths.GetLocation(name, null, null).Type != PathAbstractions.EAbstractedLocationType.None)
				{
					PrefabPreviewManager.PrefabGameObject prefabGameObject = new PrefabPreviewManager.PrefabGameObject
					{
						PrefabInstance = value
					};
					this.displayedPrefabs.Add(value.id, prefabGameObject);
				}
			}
		}
		List<int> list = new List<int>();
		foreach (KeyValuePair<int, PrefabPreviewManager.PrefabGameObject> keyValuePair2 in this.displayedPrefabs)
		{
			if (!_prefabsAround.ContainsKey(keyValuePair2.Key))
			{
				list.Add(keyValuePair2.Key);
			}
		}
		foreach (int num in list)
		{
			if (!(this.displayedPrefabs[num].Go == null))
			{
				MeshFilter[] componentsInChildren = this.displayedPrefabs[num].Go.GetComponentsInChildren<MeshFilter>();
				for (int i = 0; i < componentsInChildren.Length; i++)
				{
					Object.Destroy(componentsInChildren[i].mesh);
				}
				Renderer[] componentsInChildren2 = this.displayedPrefabs[num].Go.GetComponentsInChildren<Renderer>();
				for (int j = 0; j < componentsInChildren2.Length; j++)
				{
					Object.Destroy(componentsInChildren2[j].material);
				}
				Object.Destroy(this.displayedPrefabs[num].Go);
				this.displayedPrefabs.Remove(num);
			}
		}
	}

	public void UpdateDisplay()
	{
		if (XUiC_WorldGenerationWindow.Instance.PreviewQualityLevel == XUiC_WorldGenerationWindow.PreviewQuality.NoPreview)
		{
			return;
		}
		MicroStopwatch microStopwatch = new MicroStopwatch();
		if (this.parentTransform == null)
		{
			this.parentTransform = new GameObject("PrefabsLOD").transform;
			this.parentTransform.gameObject.layer = 11;
		}
		foreach (KeyValuePair<int, PrefabPreviewManager.PrefabGameObject> keyValuePair in this.displayedPrefabs)
		{
			PrefabPreviewManager.PrefabGameObject value = keyValuePair.Value;
			PrefabDataInstance prefabInstance = value.PrefabInstance;
			Vector3 vector = prefabInstance.boundingBoxPosition.ToVector3();
			Vector3 vector2 = prefabInstance.boundingBoxPosition.ToVector3();
			Vector3 vector3 = prefabInstance.boundingBoxSize.ToVector3();
			if (prefabInstance.rotation % 2 == 0)
			{
				vector += new Vector3((float)prefabInstance.boundingBoxSize.x * 0.5f, 0f, (float)prefabInstance.boundingBoxSize.z * 0.5f);
				vector2 += new Vector3((float)prefabInstance.boundingBoxSize.x * 0.5f, 0f, (float)prefabInstance.boundingBoxSize.z * 0.5f);
			}
			else
			{
				vector += new Vector3((float)prefabInstance.boundingBoxSize.z * 0.5f, 0f, (float)prefabInstance.boundingBoxSize.x * 0.5f);
				vector2 += new Vector3((float)prefabInstance.boundingBoxSize.z * 0.5f, 0f, (float)prefabInstance.boundingBoxSize.x * 0.5f);
				vector3..ctor(vector3.z, vector3.y, vector3.x);
			}
			Vector3 zero = Vector3.zero;
			Quaternion quaternion = Quaternion.identity;
			switch (prefabInstance.rotation)
			{
			case 0:
				zero..ctor(-0.5f, 0f, -0.5f);
				break;
			case 1:
				zero..ctor(0.5f, 0f, -0.5f);
				quaternion = Quaternion.Euler(0f, 270f, 0f);
				break;
			case 2:
				zero..ctor(0.5f, 0f, 0.5f);
				quaternion = Quaternion.Euler(0f, 180f, 0f);
				break;
			case 3:
				zero..ctor(-0.5f, 0f, 0.5f);
				quaternion = Quaternion.Euler(0f, 90f, 0f);
				break;
			}
			if (Utils.FastAbs(vector.x - (float)((int)vector.x)) > 0.001f)
			{
				vector.x += zero.x;
			}
			if (Utils.FastAbs(vector.z - (float)((int)vector.z)) > 0.001f)
			{
				vector.z += zero.z;
			}
			float num = 0f;
			Utils.DrawBounds(new Bounds(vector2 + new Vector3(0f, (float)prefabInstance.boundingBoxSize.y * 0.5f + 0.1f + num, 0f) - Origin.position, vector3), Color.green, 2f);
			if (!value.Go)
			{
				XUiC_WorldGenerationWindow.PreviewQuality previewQuality = XUiC_WorldGenerationWindow.Instance.PreviewQualityLevel;
				if (((DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5).IsCurrent() || SystemInfo.systemMemorySize < 1300) && previewQuality > XUiC_WorldGenerationWindow.PreviewQuality.Default)
				{
					previewQuality = XUiC_WorldGenerationWindow.PreviewQuality.Default;
				}
				GameObject gameObject;
				if (previewQuality == XUiC_WorldGenerationWindow.PreviewQuality.Highest)
				{
					string name = prefabInstance.location.Name;
					gameObject = SimpleMeshFile.ReadGameObject(PathAbstractions.PrefabImpostersSearchPaths.GetLocation(name, null, null), 0f, null, true, false, null, null);
					if (gameObject == null)
					{
						continue;
					}
					Renderer[] array = gameObject.GetComponentsInChildren<Renderer>();
					for (int i = 0; i < array.Length; i++)
					{
						array[i].gameObject.layer = 11;
					}
				}
				else if (previewQuality >= XUiC_WorldGenerationWindow.PreviewQuality.Low && previewQuality <= XUiC_WorldGenerationWindow.PreviewQuality.High)
				{
					gameObject = new GameObject();
					if (!prefabInstance.prefab.Name.Contains("rwg_tile") && !prefabInstance.prefab.Name.Contains("part_driveway"))
					{
						Transform transform = GameObject.CreatePrimitive(3).transform;
						transform.SetParent(gameObject.transform);
						transform.localPosition = new Vector3(0f, (float)(prefabInstance.boundingBoxSize.y / 2), 0f);
						transform.localScale = prefabInstance.boundingBoxSize.ToVector3();
						if ((int)(prefabInstance.previewColor.r + prefabInstance.previewColor.g + prefabInstance.previewColor.b) != 765)
						{
							transform.GetComponent<Renderer>().material.color = prefabInstance.previewColor;
						}
						Renderer[] array = gameObject.GetComponentsInChildren<Renderer>();
						for (int i = 0; i < array.Length; i++)
						{
							array[i].gameObject.layer = 11;
						}
					}
				}
				else
				{
					gameObject = new GameObject();
				}
				value.Go = gameObject;
				gameObject.layer = 11;
				Transform transform2 = gameObject.transform;
				transform2.name = prefabInstance.location.Name;
				transform2.SetParent(this.parentTransform, false);
				Vector3 vector4 = vector + new Vector3(0f, 0.01f + num - 4f, 0f) - Origin.position;
				transform2.SetPositionAndRotation(vector4, quaternion);
				GameObject gameObject2 = new GameObject(prefabInstance.prefab.Name);
				Transform transform3 = gameObject2.transform;
				transform3.SetParent(transform2);
				transform3.rotation = Quaternion.Euler(90f, gameObject.transform.rotation.eulerAngles.y, 0f);
				transform3.localPosition = new Vector3(0f, (float)(prefabInstance.boundingBoxSize.y + prefabInstance.prefab.yOffset) + 0.25f, 0f);
				gameObject2.layer = 11;
				Vector2i vector2i = new Vector2i(((int)vector.x + PrefabPreviewManager.WorldBuilder.WorldSize / 2) / 150, ((int)vector.z + PrefabPreviewManager.WorldBuilder.WorldSize / 2) / 150);
				TextMesh textMesh = NGUITools.AddMissingComponent<TextMesh>(gameObject2);
				textMesh.alignment = 1;
				textMesh.anchor = 4;
				textMesh.fontSize = (prefabInstance.prefab.Name.Contains("trader") ? 100 : 20);
				textMesh.color = (prefabInstance.prefab.Name.Contains("trader") ? Color.red : Color.green);
				textMesh.text = string.Concat(new string[]
				{
					prefabInstance.prefab.Name,
					Environment.NewLine,
					string.Format("pos {0}{1}", prefabInstance.boundingBoxPosition, Environment.NewLine),
					string.Format("yoffset {0}{1}", prefabInstance.prefab.yOffset, Environment.NewLine),
					string.Format("rots to north {0}, total left {1}{2}", prefabInstance.prefab.rotationToFaceNorth, prefabInstance.rotation, Environment.NewLine),
					string.Format("tile pos {0}{1}", vector2i, Environment.NewLine),
					string.Format("score {0}", prefabInstance.prefab.DensityScore)
				});
				if (microStopwatch.ElapsedMilliseconds > 50L)
				{
					this.lastDisplayUpdate = 0f;
					return;
				}
			}
		}
		foreach (KeyValuePair<int, PrefabPreviewManager.PrefabGameObject> keyValuePair2 in this.displayedPrefabs)
		{
			if (!(keyValuePair2.Value.Go == null))
			{
				Transform transform4 = keyValuePair2.Value.Go.transform;
				for (int j = 0; j < transform4.childCount; j++)
				{
					transform4.GetChild(j).gameObject.SetActive(true);
				}
			}
		}
	}

	public void ClearOldPreview()
	{
		this.RemovePrefabs();
	}

	public void Cleanup()
	{
		this.RemovePrefabs();
		if (!this.parentTransform)
		{
			return;
		}
		Object.Destroy(this.parentTransform.gameObject);
		this.parentTransform = null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const float PrefabYPosition = 4f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float DisplayUpdateDelay = 2f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float PrefabListUpdateDelay = 2f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int LodPoiDistance = 100000;

	public bool ReadyToDisplay;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_WorldGenerationPreview previewWindow;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<int, PrefabDataInstance> prefabsAround = new Dictionary<int, PrefabDataInstance>();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<int, PrefabPreviewManager.PrefabGameObject> displayedPrefabs = new Dictionary<int, PrefabPreviewManager.PrefabGameObject>();

	[PublicizedFrom(EAccessModifier.Private)]
	public Transform parentTransform;

	[PublicizedFrom(EAccessModifier.Private)]
	public float lastTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public float lastDisplayUpdate;

	public bool Initialized;

	[PublicizedFrom(EAccessModifier.Private)]
	public class PrefabGameObject
	{
		public PrefabDataInstance PrefabInstance;

		public GameObject Go;
	}
}
