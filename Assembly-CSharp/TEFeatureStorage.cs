using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Audio;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class TEFeatureStorage : TEFeatureAbs, ITileEntityLootable, ITileEntity, ILockTarget, IInventory
{
	public float LootStageMod
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public float LootStageBonus
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public override void Init(TileEntityComposite _parent, TileEntityFeatureData _featureData)
	{
		base.Init(_parent, _featureData);
		this.lockFeature = base.Parent.GetFeature<TEFeatureLockable>();
		this.lockpickFeature = base.Parent.GetFeature<TEFeatureLockPickable>();
		DynamicProperties props = _featureData.Props;
		if (!props.Values.ContainsKey(TEFeatureStorage.PropLootList))
		{
			Log.Error("Block with name " + base.Parent.TeData.Block.GetBlockName() + " does not have a loot list");
		}
		else
		{
			this.lootListName = props.Values[TEFeatureStorage.PropLootList];
		}
		float num = 0f;
		float num2 = 0f;
		props.ParseFloat(TEFeatureStorage.PropLootStageMod, ref num);
		props.ParseFloat(TEFeatureStorage.PropLootStageBonus, ref num2);
		props.ParseBool(TEFeatureStorage.PropIsJammed, ref this.isJammed);
		props.ParseBool(TEFeatureStorage.PropIsQuestLoot, ref this.isQuestLoot);
		this.LootStageMod = num;
		this.LootStageBonus = num2;
		for (int i = 1; i < 99; i++)
		{
			string text = TEFeatureStorage.PropAlternateLootList + i.ToString();
			if (!props.Values.ContainsKey(text))
			{
				break;
			}
			string text2 = "";
			if (props.Params1.ContainsKey(text))
			{
				text2 = props.Params1[text];
			}
			if (!string.IsNullOrEmpty(text2))
			{
				FastTags<TagGroup.Global> fastTags = FastTags<TagGroup.Global>.Parse(text2);
				if (this.AlternateLootList == null)
				{
					this.AlternateLootList = new List<TEFeatureStorage.AlternateLootEntry>();
				}
				this.AlternateLootList.Add(new TEFeatureStorage.AlternateLootEntry
				{
					tag = fastTags,
					lootEntry = props.Values[text]
				});
			}
		}
		this.SetContainerSize(LootContainer.GetLootContainer(this.lootListName, true).size, true);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void CopyFromInternal(TileEntityComposite _other)
	{
		ITileEntityLootable tileEntityLootable;
		if (_other.TryGetSelfOrFeature(out tileEntityLootable))
		{
			this.lootListName = tileEntityLootable.lootListName;
			this.containerSize = tileEntityLootable.GetContainerSize();
			this.items = ItemStack.Clone(tileEntityLootable.items);
			this.worldTimeTouched = tileEntityLootable.worldTimeTouched;
			this.bTouched = tileEntityLootable.bTouched;
		}
	}

	public override void OnDestroy()
	{
		base.OnDestroy();
		if (!GameManager.IsDedicatedServer)
		{
			XUiC_LootWindowGroup.CloseIfOpenAtPos(base.ToWorldPos(), null);
		}
	}

	public override void OnAdded(Vector3i _blockPos, BlockValue _blockValue)
	{
		base.OnAdded(_blockPos, _blockValue);
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer && base.Parent.Owner != null)
		{
			this.worldTimeTouched = GameManager.Instance.World.GetWorldTime();
			this.SetEmpty();
		}
	}

	public override void UpgradeDowngradeFrom(TileEntityComposite _other)
	{
		base.UpgradeDowngradeFrom(_other);
		ITileEntityLootable feature = _other.GetFeature<ITileEntityLootable>();
		if (feature != null)
		{
			this.bTouched = feature.bTouched;
			this.worldTimeTouched = feature.worldTimeTouched;
			this.migrateItemsFromOtherContainer(feature);
		}
	}

	public override void ReplacedBy(BlockValue _bvOld, BlockValue _bvNew, TileEntity _teNew)
	{
		base.ReplacedBy(_bvOld, _bvNew, _teNew);
		ITileEntityLootable tileEntityLootable;
		if (!_teNew.TryGetSelfOrFeature(out tileEntityLootable))
		{
			GameManager.Instance.DropContentOfLootContainerServer(_bvOld, base.ToWorldPos(), this);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void migrateItemsFromOtherContainer(ITileEntityLootable _other)
	{
		this.items = ItemStack.Clone(_other.items, 0, this.containerSize.x * this.containerSize.y);
		if (this.items.Length < _other.items.Length)
		{
			List<ItemStack> list = new List<ItemStack>();
			for (int i = this.items.Length; i < _other.items.Length; i++)
			{
				list.Add(_other.items[i]);
			}
			Vector3 vector = base.ToWorldCenterPos();
			vector.y += 0.9f;
			GameManager.Instance.DropContentInLootContainerServer(-1, "DroppedLootContainer", vector, list.ToArray(), true, null);
		}
		if (_other.HasSlotLocksSupport && _other.SlotLocks != null)
		{
			this.SlotLocks = _other.SlotLocks.Clone();
			this.SlotLocks.Length = this.items.Length;
			return;
		}
		this.SlotLocks = new PackedBoolArray(this.items.Length);
	}

	public override void Reset(FastTags<TagGroup.Global> _questTags)
	{
		base.Reset(_questTags);
		if (this.bPlayerStorage)
		{
			return;
		}
		this.bTouched = false;
		this.bWasTouched = false;
		for (int i = 0; i < this.items.Length; i++)
		{
			this.items[i].Clear();
		}
		if (this.AlternateLootList != null)
		{
			for (int j = 0; j < this.AlternateLootList.Count; j++)
			{
				if (_questTags.Test_AnySet(this.AlternateLootList[j].tag))
				{
					this.lootListName = this.AlternateLootList[j].lootEntry;
					break;
				}
			}
		}
		base.SetModified();
	}

	public override void UpdateTick(World _world)
	{
		base.UpdateTick(_world);
		if (base.Parent.PlayerPlaced)
		{
			return;
		}
		if (!this.bTouched || this.bPlayerStorage || !this.IsEmpty() || GamePrefs.GetInt(EnumGamePrefs.LootRespawnDays) <= 0)
		{
			return;
		}
		int num = GameUtils.WorldTimeToTotalHours(this.worldTimeTouched);
		if ((GameUtils.WorldTimeToTotalHours(_world.worldTime) - num) / 24 >= GamePrefs.GetInt(EnumGamePrefs.LootRespawnDays))
		{
			this.bWasTouched = false;
			this.bTouched = false;
			base.SetModified();
			return;
		}
		if (this.entityTempList == null)
		{
			this.entityTempList = new List<Entity>();
		}
		else
		{
			this.entityTempList.Clear();
		}
		_world.GetEntitiesInBounds(typeof(EntityPlayer), new Bounds(base.ToWorldPos().ToVector3(), Vector3.one * 16f), this.entityTempList);
		if (this.entityTempList.Count > 0)
		{
			this.worldTimeTouched = _world.worldTime;
			base.SetModified();
			return;
		}
	}

	public override string GetActivationText(WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, EntityAlive _entityFocusing, string _activateHotkeyMarkup, string _focusedTileEntityName)
	{
		base.GetActivationText(_world, _blockPos, _blockValue, _entityFocusing, _activateHotkeyMarkup, _focusedTileEntityName);
		if (this.lockpickFeature != null && this.lockpickFeature.NeedsLockpicking())
		{
			return string.Format(Localization.Get("tooltipLocked", false, null), _activateHotkeyMarkup, _focusedTileEntityName);
		}
		if (this.lockFeature != null && this.lockFeature.IsLocked() && !this.lockFeature.IsUserAllowed(PlatformManager.InternalLocalUserIdentifier))
		{
			return string.Format(Localization.Get("tooltipJammed", false, null), _activateHotkeyMarkup, _focusedTileEntityName);
		}
		if (this.isJammed)
		{
			return string.Format(Localization.Get("tooltipJammed", false, null), _activateHotkeyMarkup, _focusedTileEntityName);
		}
		if (this.isQuestLoot)
		{
			return string.Format(Localization.Get("lootTooltipTouched", false, null), _activateHotkeyMarkup, _focusedTileEntityName);
		}
		if (!this.bTouched)
		{
			return string.Format(Localization.Get("lootTooltipNew", false, null), _activateHotkeyMarkup, _focusedTileEntityName);
		}
		if (this.IsEmpty())
		{
			return string.Format(Localization.Get("lootTooltipEmpty", false, null), _activateHotkeyMarkup, _focusedTileEntityName);
		}
		return string.Format(Localization.Get("lootTooltipTouched", false, null), _activateHotkeyMarkup, _focusedTileEntityName);
	}

	public override void InitBlockActivationCommands(Action<BlockActivationCommand, TileEntityComposite.EBlockCommandOrder, TileEntityFeatureData> _addCallback)
	{
		base.InitBlockActivationCommands(_addCallback);
		_addCallback(new BlockActivationCommand("Search", "search", true, false, null), TileEntityComposite.EBlockCommandOrder.Normal, base.FeatureData);
	}

	public override bool OnBlockActivated(ReadOnlySpan<char> _commandName, WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, EntityPlayerLocal _player)
	{
		base.OnBlockActivated(_commandName, _world, _blockPos, _blockValue, _player);
		if (!base.CommandIs(_commandName, "Search"))
		{
			return false;
		}
		if ((this.lockFeature != null && this.lockFeature.IsLocked() && !this.lockFeature.IsUserAllowed(PlatformManager.InternalLocalUserIdentifier)) || this.isJammed)
		{
			Manager.BroadcastPlayByLocalPlayer(_blockPos.ToVector3() + Vector3.one * 0.5f, "Misc/locked");
			return false;
		}
		_player.AimingGun = false;
		this.bWasTouched = this.bTouched;
		LockManager.Instance.LockRequestLocal(this, null, 0);
		return true;
	}

	public string lootListName { get; set; }

	public bool bPlayerStorage
	{
		get
		{
			return base.Parent.Owner != null || this.playerStorage;
		}
		set
		{
			this.playerStorage = value;
		}
	}

	public PreferenceTracker preferences { get; set; }

	public ulong worldTimeTouched { get; set; }

	public bool bTouched
	{
		get
		{
			return this.bPlayerStorage || this.internalTouched;
		}
		set
		{
			this.internalTouched = value;
		}
	}

	public bool bWasTouched { get; set; }

	public ItemStack[] items
	{
		get
		{
			ItemStack[] array;
			if ((array = this.itemsArr) == null)
			{
				array = (this.itemsArr = ItemStack.CreateArray(this.containerSize.x * this.containerSize.y));
			}
			return array;
		}
		set
		{
			this.itemsArr = value;
		}
	}

	public virtual Vector2i GetContainerSize()
	{
		return this.containerSize;
	}

	public virtual void SetContainerSize(Vector2i _containerSize, bool _clearItems = true)
	{
		this.containerSize = _containerSize;
		if (!_clearItems)
		{
			return;
		}
		if (this.containerSize.x * this.containerSize.y != this.items.Length)
		{
			this.items = ItemStack.CreateArray(this.containerSize.x * this.containerSize.y);
			return;
		}
		for (int i = 0; i < this.items.Length; i++)
		{
			this.items[i] = ItemStack.Empty;
		}
	}

	public void UpdateSlot(int _idx, ItemStack _item)
	{
		this.items[_idx] = _item.Clone();
		base.Parent.NotifyListeners();
	}

	public void RemoveItem(ItemValue _itemValue)
	{
		for (int i = 0; i < this.items.Length; i++)
		{
			if (this.items[i].itemValue.ItemClass == _itemValue.ItemClass)
			{
				this.UpdateSlot(i, ItemStack.Empty);
			}
		}
	}

	public int RemoveItems(ItemValue _itemValue, int _count)
	{
		int num = _count;
		int num2 = 0;
		while (_count > 0 && num2 < this.items.Length - 1)
		{
			if (this.items[num2].itemValue.ItemClass == _itemValue.ItemClass)
			{
				if (_itemValue.ItemClass.CanStack())
				{
					int count = this.items[num2].count;
					int num3 = ((count >= _count) ? _count : count);
					this.items[num2].count -= num3;
					_count -= num3;
					if (this.items[num2].count <= 0)
					{
						this.items[num2].Clear();
					}
				}
				else
				{
					this.items[num2].Clear();
					_count--;
				}
			}
			num2++;
		}
		base.Parent.NotifyListeners();
		return num - _count;
	}

	public bool IsEmpty()
	{
		for (int i = 0; i < this.items.Length; i++)
		{
			if (!this.items[i].IsEmpty())
			{
				return false;
			}
		}
		return true;
	}

	public void SetEmpty()
	{
		for (int i = 0; i < this.items.Length; i++)
		{
			this.items[i].Clear();
		}
		PackedBoolArray slotLocks = this.SlotLocks;
		if (slotLocks != null)
		{
			slotLocks.Clear();
		}
		base.Parent.NotifyListeners();
		this.bTouched = true;
		base.SetModified();
	}

	public bool ShouldDestroyOnClose()
	{
		LootContainer.DestroyOnClose destroyOnClose = LootContainer.GetLootContainer(this.lootListName, true).destroyOnClose;
		return destroyOnClose == LootContainer.DestroyOnClose.True || (destroyOnClose == LootContainer.DestroyOnClose.Empty && this.IsEmpty());
	}

	public bool HasSlotLocksSupport
	{
		get
		{
			return true;
		}
	}

	public PackedBoolArray SlotLocks { get; set; }

	public bool AddItem(ItemStack _itemStack)
	{
		for (int i = 0; i < this.items.Length; i++)
		{
			if (this.items[i].IsEmpty())
			{
				this.UpdateSlot(i, _itemStack);
				base.SetModified();
				return true;
			}
		}
		return false;
	}

	[return: TupleElementNames(new string[] { "anyMoved", "allMoved" })]
	public ValueTuple<bool, bool> TryStackItem(int _startIndex, ItemStack _itemStack)
	{
		bool flag = false;
		int count = _itemStack.count;
		for (int i = _startIndex; i < this.items.Length; i++)
		{
			int count2 = _itemStack.count;
			if (_itemStack.itemValue.type == this.items[i].itemValue.type && this.items[i].CanStackPartly(ref count2))
			{
				this.items[i].count += count2;
				_itemStack.count -= count2;
				if (_itemStack.count == 0)
				{
					break;
				}
			}
		}
		if (_itemStack.count != count)
		{
			flag = true;
			base.SetModified();
			base.Parent.NotifyListeners();
		}
		return new ValueTuple<bool, bool>(flag, _itemStack.count == 0);
	}

	public bool HasItem(ItemValue _item)
	{
		for (int i = 0; i < this.items.Length; i++)
		{
			if (this.items[i].itemValue.type == _item.type)
			{
				return true;
			}
		}
		return false;
	}

	public int CountItem(ItemClass _itemClass)
	{
		int num = 0;
		for (int i = 0; i < this.items.Length; i++)
		{
			if (this.items[i].itemValue.ItemClass == _itemClass)
			{
				num += this.items[i].count;
			}
		}
		return num;
	}

	public override void Read(PooledBinaryReader _br, TileEntity.StreamModeRead _eStreamMode)
	{
		base.Read(_br, _eStreamMode);
		int num;
		if (_eStreamMode != TileEntity.StreamModeRead.Persistency)
		{
			num = 18;
		}
		else if (base.Parent.UseLocalVersioning())
		{
			num = (int)_br.ReadUInt16();
		}
		else
		{
			num = base.Parent.GetLegacyForkVersion();
		}
		if (_br.ReadBoolean())
		{
			this.lootListName = _br.ReadString();
		}
		this.containerSize = new Vector2i
		{
			x = (int)_br.ReadUInt16(),
			y = (int)_br.ReadUInt16()
		};
		this.bTouched = _br.ReadBoolean();
		this.worldTimeTouched = (ulong)_br.ReadUInt32();
		this.bPlayerStorage = _br.ReadBoolean();
		int num2 = Math.Min((int)_br.ReadInt16(), this.containerSize.x * this.containerSize.y);
		if (base.Parent.IsUserAccessing())
		{
			ItemStack empty = ItemStack.Empty;
			for (int i = 0; i < num2; i++)
			{
				empty.Read(_br);
			}
		}
		else
		{
			if (this.containerSize.x * this.containerSize.y != this.items.Length)
			{
				this.items = ItemStack.CreateArray(this.containerSize.x * this.containerSize.y);
			}
			for (int j = 0; j < num2; j++)
			{
				this.items[j].Clear();
				this.items[j].Read(_br);
			}
		}
		if (_br.ReadBoolean())
		{
			this.preferences = new PreferenceTracker(-1);
			this.preferences.Read(_br);
		}
		if (num >= 12)
		{
			this.SlotLocks = new PackedBoolArray(0);
			this.SlotLocks.Read(_br);
			return;
		}
		this.SlotLocks = new PackedBoolArray(this.items.Length);
	}

	public override void Write(PooledBinaryWriter _bw, TileEntity.StreamModeWrite _eStreamMode)
	{
		base.Write(_bw, _eStreamMode);
		if (_eStreamMode == TileEntity.StreamModeWrite.Persistency)
		{
			_bw.Write(18);
		}
		bool flag = !string.IsNullOrEmpty(this.lootListName);
		_bw.Write(flag);
		if (flag)
		{
			_bw.Write(this.lootListName);
		}
		_bw.Write((ushort)this.containerSize.x);
		_bw.Write((ushort)this.containerSize.y);
		_bw.Write(this.bTouched);
		_bw.Write((uint)this.worldTimeTouched);
		_bw.Write(this.bPlayerStorage);
		_bw.Write((short)this.items.Length);
		ItemStack[] items = this.items;
		for (int i = 0; i < items.Length; i++)
		{
			items[i].Clone().Write(_bw);
		}
		bool flag2 = this.preferences != null;
		_bw.Write(flag2);
		if (flag2)
		{
			this.preferences.Write(_bw);
		}
		if (this.SlotLocks == null)
		{
			this.SlotLocks = new PackedBoolArray(this.items.Length);
		}
		this.SlotLocks.Write(_bw);
	}

	public override bool CanLockLocally(ILockContext _context, ushort _channel)
	{
		return LocalPlayerUI.GetUIForPrimaryPlayer() != null;
	}

	public override void OnLockedServer(bool _success, int _lockingPlayerID, ILockContext _context, ushort _channel)
	{
		if (_success)
		{
			this.PopulateTE(_lockingPlayerID);
		}
	}

	public override void OnLockedLocal(bool _success, ILockContext _context, ushort _channel)
	{
		this.ShowUI(_success);
	}

	public override void OnUnlockedServer(int _unlockingPlayerId, ushort _channel)
	{
		GameManager.Instance.CheckDestroyTileEntity(this, base.ToWorldPos());
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void PopulateTE(int _playerThatOpened)
	{
		BlockValue block = GameManager.Instance.World.GetBlock(base.ToWorldPos());
		GameManager.Instance.lootManager.LootContainerOpened(this, _playerThatOpened, block.Block.Tags);
		this.bTouched = true;
		base.SetModified();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ShowUI(bool _lockGranted)
	{
		LocalPlayerUI uiforPrimaryPlayer = LocalPlayerUI.GetUIForPrimaryPlayer();
		if (!_lockGranted)
		{
			GameManager.ShowTooltip(uiforPrimaryPlayer.entityPlayer, Localization.Get("ttNoInteractItem", false, null), string.Empty, "ui_denied", null, false, false, 0f);
			return;
		}
		BlockValue block = GameManager.Instance.World.GetBlock(base.ToWorldPos());
		string localizedBlockName = block.Block.GetLocalizedBlockName();
		((XUiC_LootWindowGroup)uiforPrimaryPlayer.xui.FindWindowGroupByName(XUiC_LootWindowGroup.ID)).OpenLooting(localizedBlockName, this);
		LootContainer lootContainer = LootContainer.GetLootContainer(this.lootListName, true);
		if (lootContainer != null && uiforPrimaryPlayer.entityPlayer != null)
		{
			lootContainer.ExecuteBuffActions(uiforPrimaryPlayer.entityPlayer.entityId, uiforPrimaryPlayer.entityPlayer);
		}
		if (!GameManager.Instance.World.IsEditor() && !this.bTouched)
		{
			EntityPlayerLocal entityPlayer = uiforPrimaryPlayer.entityPlayer;
			entityPlayer.MinEventContext.TileEntity = this;
			entityPlayer.MinEventContext.BlockValue = block;
			entityPlayer.FireEvent(MinEventTypes.onSelfOpenLootContainer, true);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const int Version = 18;

	public static string PropLootList = "LootList";

	public static string PropAlternateLootList = "AlternateLootList";

	public static string PropLootStageMod = "LootStageMod";

	public static string PropLootStageBonus = "LootStageBonus";

	public static string PropIsJammed = "IsJammed";

	public static string PropIsQuestLoot = "IsQuestLoot";

	[PublicizedFrom(EAccessModifier.Private)]
	public TEFeatureLockable lockFeature;

	[PublicizedFrom(EAccessModifier.Private)]
	public TEFeatureLockPickable lockpickFeature;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isJammed;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isQuestLoot;

	public List<TEFeatureStorage.AlternateLootEntry> AlternateLootList;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<Entity> entityTempList;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2i containerSize = Vector2i.one;

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemStack[] itemsArr;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool playerStorage;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool internalTouched;

	public struct AlternateLootEntry
	{
		public FastTags<TagGroup.Global> tag;

		public string lootEntry;
	}
}
