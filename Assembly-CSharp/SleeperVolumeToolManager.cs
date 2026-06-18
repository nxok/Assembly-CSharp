using System;
using System.Collections.Generic;
using PrefabVolumes;
using UnityEngine;

public static class SleeperVolumeToolManager
{
	public static SelectionBox SelectedSleeperBox
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			SelectionBox selection = SelectionBoxManager.Instance.Selection;
			if (selection == null)
			{
				return null;
			}
			if (selection.Category != SelectionBoxManager.Instance.CategorySleeperVolume)
			{
				return null;
			}
			return selection;
		}
	}

	public static void RegisterSleeperBlock(BlockValue _bv, Transform _prefabTrans, Vector3i _position)
	{
		BlockSleeper blockSleeper = _bv.Block as BlockSleeper;
		if (blockSleeper == null)
		{
			Log.Warning("SleeperVolumeToolManager RegisterSleeperBlock not sleeper {0}", new object[] { _bv });
			return;
		}
		if (SleeperVolumeToolManager.typeMats.Count == 0)
		{
			Shader shader = Shader.Find("Game/UI/Sleeper");
			foreach (Color color in SleeperVolumeToolManager.typeColors)
			{
				Material material = new Material(shader);
				material.renderQueue = 4001;
				material.color = color;
				SleeperVolumeToolManager.typeMats.Add(material);
			}
		}
		Vector3i worldPos = GameManager.Instance.World.ChunkCache.GetChunkFromWorldPos(_position).GetWorldPos();
		SleeperVolumeToolManager.BlockData blockData = new SleeperVolumeToolManager.BlockData
		{
			block = blockSleeper,
			prefabT = _prefabTrans,
			position = _position
		};
		_prefabTrans.position = _prefabTrans.position + _position.ToVector3() + Vector3.one * 0.5f + Vector3.up * 0.01f;
		if (SleeperVolumeToolManager.GroupGameObject == null)
		{
			SleeperVolumeToolManager.GroupGameObject = new GameObject();
			SleeperVolumeToolManager.GroupGameObject.name = "SleeperVolumeToolManagerPrefabs";
		}
		_prefabTrans.parent = SleeperVolumeToolManager.GroupGameObject.transform;
		List<SleeperVolumeToolManager.BlockData> list;
		if (!SleeperVolumeToolManager.sleepers.TryGetValue(worldPos, out list))
		{
			list = new List<SleeperVolumeToolManager.BlockData>();
			SleeperVolumeToolManager.sleepers.Add(worldPos, list);
		}
		list.Add(blockData);
		SleeperVolumeToolManager.UpdateSleeperBlockVisuals(blockData, SleeperVolumeToolManager.SelectedSleeperBox);
	}

	public static void UnRegisterSleeperBlock(Vector3i _position)
	{
		if (SleeperVolumeToolManager.sleepers == null)
		{
			return;
		}
		Vector3i worldPos = GameManager.Instance.World.ChunkCache.GetChunkFromWorldPos(_position).GetWorldPos();
		List<SleeperVolumeToolManager.BlockData> list;
		if (SleeperVolumeToolManager.sleepers.TryGetValue(worldPos, out list))
		{
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].position == _position)
				{
					Object.Destroy(list[i].prefabT.gameObject);
					list.RemoveAt(i);
					return;
				}
			}
		}
	}

	public static void CleanUp()
	{
		if (SleeperVolumeToolManager.sleepers != null)
		{
			foreach (KeyValuePair<Vector3i, List<SleeperVolumeToolManager.BlockData>> keyValuePair in SleeperVolumeToolManager.sleepers)
			{
				Vector3i vector3i;
				List<SleeperVolumeToolManager.BlockData> list;
				keyValuePair.Deconstruct(out vector3i, out list);
				foreach (SleeperVolumeToolManager.BlockData blockData in list)
				{
					Transform prefabT = blockData.prefabT;
					if (prefabT)
					{
						Object.Destroy(prefabT.gameObject);
					}
				}
			}
			SleeperVolumeToolManager.sleepers.Clear();
		}
		SleeperVolumeToolManager.ClearSleeperVolumes();
	}

	public static void RegisterSleeperVolume(SelectionBox _selBox)
	{
		if (_selBox == null)
		{
			return;
		}
		if (!(_selBox.UserData is PrefabSleeperVolume))
		{
			return;
		}
		if (!SleeperVolumeToolManager.registeredSleeperVolumes.Contains(_selBox))
		{
			SleeperVolumeToolManager.registeredSleeperVolumes.Add(_selBox);
		}
	}

	public static void UnRegisterSleeperVolume(SelectionBox _selBox)
	{
		SleeperVolumeToolManager.registeredSleeperVolumes.Remove(_selBox);
	}

	public static void ClearSleeperVolumes()
	{
		SleeperVolumeToolManager.registeredSleeperVolumes.Clear();
	}

	public static void CheckKeys()
	{
		if (!Input.GetKeyDown(93))
		{
			return;
		}
		if (!SleeperVolumeToolManager.SelectedSleeperBox)
		{
			return;
		}
		PrefabSleeperVolume prefabSleeperVolume = SleeperVolumeToolManager.SelectedSleeperBox.UserData as PrefabSleeperVolume;
		if (prefabSleeperVolume == null)
		{
			return;
		}
		int num;
		PrefabInstance prefabInstance;
		if (!PrefabVolumeManager.GetPrefabIdAndVolumeId(SleeperVolumeToolManager.SelectedSleeperBox.name, out num, out prefabInstance))
		{
			return;
		}
		PrefabSleeperVolume prefabSleeperVolume2 = prefabSleeperVolume.CloneGeneric();
		if (InputUtils.ShiftKeyPressed)
		{
			prefabSleeperVolume2.groupId = 0;
			PrefabVolumeManager.Instance.UpdatePropertiesServer(prefabInstance.id, num, prefabSleeperVolume2);
			Log.Out(string.Format("Set sleeper volume {0} to group ID {1}", prefabSleeperVolume.startPos, 0));
			return;
		}
		short num2 = -1;
		if (SleeperVolumeToolManager.previousSelectionBox != null)
		{
			PrefabSleeperVolume prefabSleeperVolume3 = SleeperVolumeToolManager.previousSelectionBox.UserData as PrefabSleeperVolume;
			if (prefabSleeperVolume3 != null)
			{
				num2 = prefabSleeperVolume3.groupId;
				int num3;
				PrefabInstance prefabInstance2;
				if (num2 == 0 && PrefabVolumeManager.GetPrefabIdAndVolumeId(SleeperVolumeToolManager.previousSelectionBox.name, out num3, out prefabInstance2))
				{
					num2 = prefabInstance2.prefab.FindSleeperVolumeFreeGroupId();
					PrefabSleeperVolume prefabSleeperVolume4 = prefabSleeperVolume3.CloneGeneric();
					prefabSleeperVolume4.groupId = num2;
					PrefabVolumeManager.Instance.UpdatePropertiesServer(prefabInstance2.id, num3, prefabSleeperVolume4);
					Log.Out(string.Format("Set sleeper volume {0} to new group ID {1}", prefabSleeperVolume3.startPos, num2));
				}
			}
		}
		if (num2 >= 0)
		{
			prefabSleeperVolume2.groupId = num2;
			PrefabVolumeManager.Instance.UpdatePropertiesServer(prefabInstance.id, num, prefabSleeperVolume2);
			Log.Out(string.Format("Set sleeper volume {0} to group ID {1}", prefabSleeperVolume.startPos, num2));
		}
	}

	public static void SetVisible(bool _visible)
	{
		if (_visible)
		{
			SleeperVolumeToolManager.SelectionChanged(null);
			return;
		}
		SleeperVolumeToolManager.hideSleeperBlocks();
	}

	public static void SelectionChanged(SelectionBox _selBox)
	{
		if (_selBox)
		{
			if (SleeperVolumeToolManager.currentSelectionBox && _selBox != SleeperVolumeToolManager.currentSelectionBox)
			{
				SleeperVolumeToolManager.previousSelectionBox = SleeperVolumeToolManager.currentSelectionBox;
			}
			SleeperVolumeToolManager.currentSelectionBox = _selBox;
		}
		SleeperVolumeToolManager.UpdateAllVisuals();
	}

	public static void UpdateAllVisuals()
	{
		SleeperVolumeToolManager.UpdateAllSleeperBlockVisuals();
		SleeperVolumeToolManager.UpdateSleeperVolumeColors();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void UpdateSleeperVolumeColors()
	{
		int num = 0;
		if (SleeperVolumeToolManager.SelectedSleeperBox)
		{
			num = (int)((PrefabSleeperVolume)SleeperVolumeToolManager.SelectedSleeperBox.UserData).groupId;
		}
		SelectionCategory categorySleeperVolume = SelectionBoxManager.Instance.CategorySleeperVolume;
		foreach (SelectionBox selectionBox in SleeperVolumeToolManager.registeredSleeperVolumes)
		{
			PrefabSleeperVolume prefabSleeperVolume = (PrefabSleeperVolume)selectionBox.UserData;
			if (prefabSleeperVolume.groupId != 0)
			{
				if ((int)prefabSleeperVolume.groupId == num)
				{
					selectionBox.SetAllFacesColor(SleeperVolumeToolManager.groupSelectedColor, true);
				}
				else
				{
					selectionBox.SetAllFacesColor(SleeperVolumeToolManager.groupColors[(int)prefabSleeperVolume.groupId % SleeperVolumeToolManager.groupColors.Length], true);
				}
			}
			else
			{
				selectionBox.SetAllFacesColor(categorySleeperVolume.colInactive, true);
			}
		}
		if (SleeperVolumeToolManager.SelectedSleeperBox)
		{
			SleeperVolumeToolManager.SelectedSleeperBox.SetAllFacesColor(categorySleeperVolume.colActive, true);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void hideSleeperBlocks()
	{
		if (SleeperVolumeToolManager.sleepers == null)
		{
			return;
		}
		foreach (KeyValuePair<Vector3i, List<SleeperVolumeToolManager.BlockData>> keyValuePair in SleeperVolumeToolManager.sleepers)
		{
			Vector3i vector3i;
			List<SleeperVolumeToolManager.BlockData> list;
			keyValuePair.Deconstruct(out vector3i, out list);
			foreach (SleeperVolumeToolManager.BlockData blockData in list)
			{
				Transform prefabT = blockData.prefabT;
				if (prefabT)
				{
					prefabT.gameObject.SetActive(false);
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void UpdateAllSleeperBlockVisuals()
	{
		if (SleeperVolumeToolManager.sleepers == null)
		{
			return;
		}
		SelectionBox selectedSleeperBox = SleeperVolumeToolManager.SelectedSleeperBox;
		foreach (KeyValuePair<Vector3i, List<SleeperVolumeToolManager.BlockData>> keyValuePair in SleeperVolumeToolManager.sleepers)
		{
			Vector3i vector3i;
			List<SleeperVolumeToolManager.BlockData> list;
			keyValuePair.Deconstruct(out vector3i, out list);
			foreach (SleeperVolumeToolManager.BlockData blockData in list)
			{
				SleeperVolumeToolManager.UpdateSleeperBlockVisuals(blockData, selectedSleeperBox);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void UpdateSleeperBlockVisuals(SleeperVolumeToolManager.BlockData _data, SelectionBox _selectedBox)
	{
		Transform prefabT = _data.prefabT;
		if (!SelectionBoxManager.Instance.CategorySleeperVolume.IsVisible() || (_selectedBox == null && !SleeperVolumeToolManager.xRayOn))
		{
			prefabT.gameObject.SetActive(false);
			return;
		}
		Vector3i vector3i = Vector3i.min;
		Vector3i vector3i2 = Vector3i.min;
		if (_selectedBox != null)
		{
			vector3i = Vector3i.FromVector3Rounded(_selectedBox.bounds.min);
			vector3i2 = Vector3i.FromVector3Rounded(_selectedBox.bounds.max);
		}
		Vector3i position = _data.position;
		if (position.x >= vector3i.x && position.x < vector3i2.x && position.y >= vector3i.y && position.y < vector3i2.y && position.z >= vector3i.z && position.z < vector3i2.z)
		{
			int num = 0;
			DynamicPrefabDecorator dynamicPrefabDecorator = GameManager.Instance.World.ChunkCache.ChunkProvider.GetDynamicPrefabDecorator();
			if (dynamicPrefabDecorator == null)
			{
				return;
			}
			PrefabInstance prefabFromWorldPosInside = dynamicPrefabDecorator.GetPrefabFromWorldPosInside(position.x, position.z);
			if (prefabFromWorldPosInside == null)
			{
				Log.Error("No prefab found for SleeperBlock position!\n" + StackTraceUtility.ExtractStackTrace());
				return;
			}
			Vector3i vector3i3 = position - prefabFromWorldPosInside.boundingBoxPosition;
			PrefabSleeperVolume prefabSleeperVolume = prefabFromWorldPosInside.prefab.FindSleeperVolume(vector3i3);
			if (prefabSleeperVolume != null && prefabSleeperVolume.isPriority)
			{
				num = 1;
			}
			if (_data.block.spawnMode == BlockSleeper.eMode.Bandit)
			{
				num = 4;
			}
			if (_data.block.spawnMode == BlockSleeper.eMode.Infested)
			{
				num = 5;
			}
			prefabT.gameObject.SetActive(true);
			SleeperVolumeToolManager.SetMats(prefabT, SleeperVolumeToolManager.typeMats[num]);
			return;
		}
		else
		{
			if (!SleeperVolumeToolManager.InAnyVolume(position))
			{
				prefabT.gameObject.SetActive(true);
				SleeperVolumeToolManager.SetMats(prefabT, SleeperVolumeToolManager.typeMats[2]);
				return;
			}
			if (SleeperVolumeToolManager.xRayOn && _selectedBox == null)
			{
				prefabT.gameObject.SetActive(true);
				SleeperVolumeToolManager.SetMats(prefabT, SleeperVolumeToolManager.typeMats[3]);
				return;
			}
			prefabT.gameObject.SetActive(false);
			return;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool InAnyVolume(Vector3i _pos)
	{
		Vector3i zero = Vector3i.zero;
		Vector3i zero2 = Vector3i.zero;
		foreach (SelectionBox selectionBox in SleeperVolumeToolManager.registeredSleeperVolumes)
		{
			zero.RoundToInt(selectionBox.bounds.min);
			zero2.RoundToInt(selectionBox.bounds.max);
			if (_pos.x >= zero.x && _pos.x < zero2.x && _pos.y >= zero.y && _pos.y < zero2.y && _pos.z >= zero.z && _pos.z < zero2.z)
			{
				return true;
			}
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void SetMats(Transform _t, Material _mat)
	{
		int num = (SleeperVolumeToolManager.xRayOn ? (-200000000) : (-200000));
		_mat.SetInt("_Offset", num);
		MeshRenderer[] componentsInChildren = _t.GetComponentsInChildren<MeshRenderer>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].sharedMaterial = _mat;
		}
	}

	public static bool XRayMode
	{
		get
		{
			return SleeperVolumeToolManager.xRayOn;
		}
		set
		{
			if (SleeperVolumeToolManager.xRayOn == value)
			{
				return;
			}
			SleeperVolumeToolManager.xRayOn = value;
			int num = (SleeperVolumeToolManager.xRayOn ? (-200000000) : (-200000));
			foreach (Material material in SleeperVolumeToolManager.typeMats)
			{
				material.SetInt("_Offset", num);
			}
			SleeperVolumeToolManager.UpdateAllSleeperBlockVisuals();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static GameObject GroupGameObject;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Dictionary<Vector3i, List<SleeperVolumeToolManager.BlockData>> sleepers = new Dictionary<Vector3i, List<SleeperVolumeToolManager.BlockData>>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly List<SelectionBox> registeredSleeperVolumes = new List<SelectionBox>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool xRayOn = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cActiveIndex = 0;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cPriorityIndex = 1;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cNoVolumeIndex = 2;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cDarkIndex = 3;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cBanditIndex = 4;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cInfestedIndex = 5;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Color[] typeColors = new Color[]
	{
		new Color(1f, 0.6f, 0.1f),
		new Color(0.7f, 0.7f, 0.7f),
		new Color(1f, 0.1f, 1f),
		new Color(0.02f, 0.02f, 0.02f),
		new Color(0.1f, 1f, 0.1f),
		new Color(1f, 0.1f, 0.1f)
	};

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly List<Material> typeMats = new List<Material>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Color groupSelectedColor = new Color(0.9f, 0.9f, 1f, 0.4f);

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Color[] groupColors = new Color[]
	{
		new Color(1f, 0.2f, 0.2f, 0.4f),
		new Color(1f, 0.6f, 0.2f, 0.4f),
		new Color(1f, 1f, 0.2f, 0.4f),
		new Color(0.6f, 1f, 0.2f, 0.4f),
		new Color(0.2f, 1f, 0.2f, 0.4f),
		new Color(0.2f, 1f, 0.6f, 0.4f),
		new Color(0.2f, 1f, 1f, 0.4f),
		new Color(0.2f, 0.6f, 1f, 0.4f)
	};

	[PublicizedFrom(EAccessModifier.Private)]
	public static SelectionBox currentSelectionBox;

	[PublicizedFrom(EAccessModifier.Private)]
	public static SelectionBox previousSelectionBox;

	[PublicizedFrom(EAccessModifier.Private)]
	public class BlockData
	{
		public BlockSleeper block;

		public Transform prefabT;

		public Vector3i position;
	}
}
