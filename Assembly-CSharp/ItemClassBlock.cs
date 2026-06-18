using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ItemClassBlock : ItemClass
{
	public override bool IsLimitedItem
	{
		get
		{
			return this.GetBlock().IsLimited;
		}
	}

	public ItemClassBlock()
	{
		this.HoldType = new DataItem<int>(7);
		AnimationDelayData.AnimationDelay[this.HoldType.Value] = new AnimationDelayData.AnimationDelays(0, 0f, 0f, 0.31f, 0.31f, true);
		this.Stacknumber = new DataItem<int>(500);
	}

	public override void Init()
	{
		base.Init();
		Block block = Block.list[base.Id];
		this.DescriptionKey = block.DescriptionKey;
		this.MadeOfMaterial = block.blockMaterial;
		if (block.CustomIcon != null)
		{
			this.CustomIcon = new DataItem<string>(block.CustomIcon);
		}
		this.NoScrapping = block.NoScrapping;
		this.CustomIconTint = block.CustomIconTint;
		this.SortOrder = block.SortOrder;
		this.CreativeMode = block.CreativeMode;
		this.TraderStageTemplate = block.TraderStageTemplate;
		this.SoundPickup = block.SoundPickup;
		this.SoundPlace = block.SoundPlace;
		this.IsLimited = block.IsLimited;
	}

	public override bool IsActionRunning(ItemInventoryData _data)
	{
		return Time.time - (_data as ItemClassBlock.ItemBlockInventoryData).lastBuildTime < Constants.cBuildIntervall;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override ItemInventoryData createItemInventoryData(ItemStack _itemStack, IGameManager _gameManager, EntityAlive _holdingEntity, int _slotIdx)
	{
		return new ItemClassBlock.ItemBlockInventoryData(this, _itemStack, _gameManager, _holdingEntity, _slotIdx);
	}

	public override void StopHolding(ItemInventoryData _data, Transform _modelTransform)
	{
	}

	public override bool IsBlock()
	{
		return true;
	}

	public override bool IsProp()
	{
		return this.GetBlock().IsProp;
	}

	public override Block GetBlock()
	{
		return Block.list[base.Id];
	}

	public override string GetItemName()
	{
		return this.GetBlock().GetBlockName();
	}

	public override string GetLocalizedItemName()
	{
		return this.GetBlock().GetLocalizedBlockName();
	}

	public override bool HasAnyTags(FastTags<TagGroup.Global> _tags)
	{
		return this.GetBlock().Tags.Test_AnySet(_tags);
	}

	public override bool HasAllTags(FastTags<TagGroup.Global> _tags)
	{
		return this.GetBlock().Tags.Test_AllSet(_tags);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public BlockValue GetBlockValueFromItemValue(ItemValue _itemValue)
	{
		Block block = this.GetBlock();
		if (block.SelectAlternates)
		{
			return block.GetAltBlockValue(_itemValue.Meta);
		}
		return _itemValue.ToBlockValue(false);
	}

	public override Transform CloneModel(GameObject _go, World _world, BlockValue _blockValue, Vector3[] _vertices, Vector3 _position, Transform _parent, BlockShape.MeshPurpose _purpose, TextureFullArray _textureFullArray = default(TextureFullArray))
	{
		return ItemClassBlock.CreateMesh(_go, _world, _blockValue, _vertices, _position, _parent, _purpose, _textureFullArray);
	}

	public override Transform CloneModel(World _world, ItemValue _itemValue, Vector3 _position, Transform _parent, BlockShape.MeshPurpose _purpose, TextureFullArray _textureFullArray = default(TextureFullArray))
	{
		return ItemClassBlock.CreateMesh(null, _world, this.GetBlockValueFromItemValue(_itemValue), null, _position, _parent, _purpose, _textureFullArray);
	}

	public static Transform CreateMesh(GameObject _go, World _world, BlockValue _blockValue, Vector3[] _vertices, Vector3 _worldPos, Transform _parent, BlockShape.MeshPurpose _purpose, TextureFullArray _textureFullArray = default(TextureFullArray))
	{
		if (_purpose == BlockShape.MeshPurpose.Drop)
		{
			GameObject gameObject = Object.Instantiate<GameObject>(LoadManager.LoadAsset<GameObject>("@:Other/Items/Misc/sack_droppedPrefab.prefab", null, null, false, true, false).Asset);
			Transform transform = gameObject.transform;
			transform.SetParent(_parent, false);
			transform.localPosition = Vector3.zero;
			transform.localRotation = Quaternion.identity;
			return gameObject.transform;
		}
		Block block = _blockValue.Block;
		BlockShapeModelEntity blockShapeModelEntity = block.shape as BlockShapeModelEntity;
		if (blockShapeModelEntity != null)
		{
			if (_go == null)
			{
				_go = new GameObject();
				_go.transform.SetParent(_parent, false);
			}
			blockShapeModelEntity.CloneModel(_blockValue, _go.transform);
			return _go.transform;
		}
		int meshIndex = (int)block.MeshIndex;
		Transform transform2 = ItemClassBlock.CreateMeshOfType(_go, _world, _blockValue, _vertices, _worldPos, _parent, _purpose, _textureFullArray, meshIndex);
		if (meshIndex == 0 && (_purpose == BlockShape.MeshPurpose.Preview || _purpose == BlockShape.MeshPurpose.Local || _purpose == BlockShape.MeshPurpose.SimplifiedCollisionOnly))
		{
			Transform transform3 = ItemClassBlock.CreateMeshOfType(_go, _world, _blockValue, _vertices, _worldPos, _parent, _purpose, _textureFullArray, 2);
			if (transform3)
			{
				if (transform2)
				{
					transform3.SetParent(transform2, false);
				}
				else
				{
					transform2 = transform3;
				}
			}
		}
		return transform2;
	}

	public static Transform CreateMeshOfType(GameObject _go, World _world, BlockValue _blockValue, Vector3[] _vertices, Vector3 _worldPos, Transform _parent, BlockShape.MeshPurpose _purpose, TextureFullArray _textureFullArray, int _meshIndex)
	{
		Vector3i vector3i = World.worldToBlockPos(_worldPos);
		byte b;
		byte b2;
		_world.GetSunAndBlockColors(vector3i, out b, out b2);
		VoxelMesh voxelMesh = VoxelMesh.Create(_meshIndex, MeshDescription.meshes[_meshIndex].meshType, 1);
		VoxelMesh[] array = new VoxelMesh[MeshDescription.meshes.Length];
		array[_meshIndex] = voxelMesh;
		Block block = _blockValue.Block;
		block.shape.renderFull(vector3i, _blockValue, ItemClassBlock.renderOffsetV, _vertices, new LightingAround(b, b2, 0), _textureFullArray, array, _purpose);
		if (_purpose == BlockShape.MeshPurpose.SimplifiedCollisionOnly)
		{
			block.shape.renderFull(vector3i, _blockValue, ItemClassBlock.renderOffsetV, _vertices, new LightingAround(b, b2, 0), _textureFullArray, array, _purpose);
			if (!_go)
			{
				_go = new GameObject();
				_go.transform.SetParent(_parent, false);
			}
			_go.name = "Block_" + _blockValue.type.ToString();
			MeshCollider orAddComponent = _go.GetOrAddComponent<MeshCollider>();
			Mesh mesh;
			voxelMesh.CopyToColliders(orAddComponent, out mesh);
			orAddComponent.sharedMesh = mesh;
			return _go.transform;
		}
		if (voxelMesh.m_Vertices.Count == 0)
		{
			return null;
		}
		if (!_go)
		{
			_go = new GameObject();
			_go.transform.SetParent(_parent, false);
			_go.AddComponent<UpdateLightOnChunkMesh>();
		}
		_go.name = "Block_" + _blockValue.type.ToString();
		MeshFilter meshFilter;
		MeshRenderer meshRenderer;
		VoxelMesh.CreateMeshFilter(_meshIndex, 0, _go, "Item", false, out meshFilter, out meshRenderer);
		if (meshFilter != null)
		{
			voxelMesh.CopyToMesh(meshFilter, meshRenderer, 0, null);
		}
		return _go.transform;
	}

	public override ItemClass.EnumCrosshairType GetCrosshairType(ItemInventoryData _holdingData)
	{
		return ItemClass.EnumCrosshairType.Plus;
	}

	public override RenderCubeType GetFocusType(ItemInventoryData _data)
	{
		return RenderCubeType.FullBlockBothSides;
	}

	public override float GetFocusRange()
	{
		return Constants.cDigAndBuildDistance;
	}

	public override void ExecuteAction(int _actionIdx, ItemInventoryData _data, bool _bReleased, PlayerActionsLocal _playerActions)
	{
		EntityPlayerLocal entityPlayerLocal = _data.holdingEntity as EntityPlayerLocal;
		if (entityPlayerLocal != null && !entityPlayerLocal.bFirstPersonView)
		{
			entityPlayerLocal.StartTPCameraLockTimer();
		}
		if (_actionIdx == 0)
		{
			GameManager.Instance.GetActiveBlockTool().ExecuteAttackAction(_data, _bReleased, _playerActions);
			return;
		}
		GameManager.Instance.GetActiveBlockTool().ExecuteUseAction(_data, _bReleased, _playerActions);
	}

	public override bool IsFocusBlockInside()
	{
		return false;
	}

	public override Vector3 GetDroppedCorrectionRotation()
	{
		return Vector3.zero;
	}

	public override Vector3 GetCorrectionRotation()
	{
		return new Vector3(90f, 0f, 0f);
	}

	public override Vector3 GetCorrectionPosition()
	{
		return Vector3.zero;
	}

	public override Vector3 GetCorrectionScale()
	{
		return new Vector3(0.1f, 0.1f, 0.1f);
	}

	public override bool CanHold()
	{
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const byte cNoRotation = 128;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Vector3 renderOffsetV = new Vector3(-0.5f, -0.5f, -0.5f);

	public class ItemBlockInventoryData : ItemInventoryData
	{
		public ItemBlockInventoryData(ItemClass _item, ItemStack _itemStack, IGameManager _gameManager, EntityAlive _holdingEntity, int _slotIdx)
			: base(_item, _itemStack, _gameManager, _holdingEntity, _slotIdx)
		{
			this.lastBuildTime = 0f;
			this.rotation = 128;
			Block block = Block.list[_item.Id];
			if (block.HandleFace != BlockFace.None)
			{
				this.mode = BlockPlacement.EnumRotationMode.ToFace;
			}
			if (block.BlockPlacementHelper != BlockPlacement.None)
			{
				this.mode = BlockPlacement.EnumRotationMode.Auto;
			}
		}

		public BlockPlacement.EnumPlacement Placement
		{
			get
			{
				BlockPlacement.EnumPlacement placement = this.m_placement;
				bool flag = placement == BlockPlacement.EnumPlacement.Voxel || placement != BlockPlacement.EnumPlacement.Free || this.item.IsProp();
				bool flag2 = flag;
				if (!LaunchPrefs.EnableProps.Value || !flag2)
				{
					return BlockPlacement.EnumPlacement.Voxel;
				}
				return this.m_placement;
			}
			set
			{
				this.m_placement = value;
			}
		}

		public float lastBuildTime;

		public byte rotation;

		[PublicizedFrom(EAccessModifier.Private)]
		public BlockPlacement.EnumPlacement m_placement;

		public BlockPlacement.EnumRotationMode mode = BlockPlacement.EnumRotationMode.Simple;

		public int localRot;

		public int damage;

		public PropTransform propTransform = PropTransform.identity;
	}
}
