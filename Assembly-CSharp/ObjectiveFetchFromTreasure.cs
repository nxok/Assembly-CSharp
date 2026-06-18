using System;
using System.Globalization;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ObjectiveFetchFromTreasure : BaseObjective
{
	public override BaseObjective.ObjectiveValueTypes ObjectiveValueType
	{
		get
		{
			return BaseObjective.ObjectiveValueTypes.Boolean;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RemoveFetchItems()
	{
		if (this.expectedItemClass == null)
		{
			this.SetupExpectedItem();
		}
		XUi xui = LocalPlayerUI.GetUIForPlayer(base.OwnerQuest.OwnerJournal.OwnerPlayer).xui;
		XUiM_PlayerInventory playerInventory = xui.PlayerInventory;
		int num = 1;
		int num2 = 1;
		num2 -= playerInventory.Backpack.DecItem(this.expectedItem, num2, false, null);
		if (num2 > 0)
		{
			playerInventory.Toolbelt.DecItem(this.expectedItem, num2, false, null);
		}
		if (num != num2)
		{
			xui.CollectedItemList.RemoveItemStack(new ItemStack(this.expectedItem.Clone(), num - num2));
		}
	}

	public override void SetupObjective()
	{
		this.keyword = Localization.Get("ObjectiveFetchContainer_keyword", false, null);
		if (this.expectedItemClass == null)
		{
			this.SetupExpectedItem();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetupExpectedItem()
	{
		if (base.OwnerQuest.QuestCode == 0)
		{
			base.OwnerQuest.SetupQuestCode();
		}
		this.expectedItemClass = ItemClass.GetItemClass(ObjectiveFetchFromTreasure.questItemClassID, false);
		int id = this.expectedItemClass.Id;
		ushort num = StringParsers.ParseUInt16(this.ID, 0, -1, NumberStyles.Integer);
		this.expectedItemClass = ItemClassQuest.GetItemQuestById(num);
		this.expectedItem = new ItemValue(id, false);
		this.expectedItem.Seed = num;
		this.expectedItem.Meta = base.OwnerQuest.QuestCode;
		this.itemCount = 1;
	}

	public override void SetupDisplay()
	{
		base.Description = string.Format(this.keyword, this.expectedItemClass.GetLocalizedItemName());
		this.StatusText = "";
	}

	public override void HandleCompleted()
	{
		base.HandleCompleted();
		this.RemoveFetchItems();
	}

	public override void HandlePhaseCompleted()
	{
		base.HandlePhaseCompleted();
	}

	public override void HandleFailed()
	{
		base.HandleFailed();
		this.RemoveFetchItems();
	}

	public override void AddHooks()
	{
		LocalPlayerUI.GetUIForPlayer(base.OwnerQuest.OwnerJournal.OwnerPlayer);
		XUiM_PlayerInventory playerInventory = LocalPlayerUI.GetUIForPlayer(base.OwnerQuest.OwnerJournal.OwnerPlayer).xui.PlayerInventory;
		playerInventory.Backpack.OnBackpackItemsChangedInternal += this.Backpack_OnBackpackItemsChangedInternal;
		playerInventory.Toolbelt.OnToolbeltItemsChangedInternal += this.Toolbelt_OnToolbeltItemsChangedInternal;
		QuestEventManager.Current.ContainerOpened += this.Current_ContainerOpened;
		QuestEventManager.Current.ContainerClosed += this.Current_ContainerClosed;
		QuestEventManager.Current.BlockChange += this.Current_BlockChange;
		this.Refresh();
	}

	public override void RemoveObjectives()
	{
		QuestEventManager.Current.ContainerOpened -= this.Current_ContainerOpened;
		QuestEventManager.Current.ContainerClosed -= this.Current_ContainerClosed;
		QuestEventManager.Current.BlockChange -= this.Current_BlockChange;
	}

	public override void RemoveHooks()
	{
		base.OwnerQuest.RemoveMapObject();
		XUiM_PlayerInventory playerInventory = LocalPlayerUI.GetUIForPlayer(base.OwnerQuest.OwnerJournal.OwnerPlayer).xui.PlayerInventory;
		if (playerInventory != null)
		{
			playerInventory.Backpack.OnBackpackItemsChangedInternal -= this.Backpack_OnBackpackItemsChangedInternal;
			playerInventory.Toolbelt.OnToolbeltItemsChangedInternal -= this.Toolbelt_OnToolbeltItemsChangedInternal;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Backpack_OnBackpackItemsChangedInternal()
	{
		LocalPlayerUI uiforPlayer = LocalPlayerUI.GetUIForPlayer(base.OwnerQuest.OwnerJournal.OwnerPlayer);
		if (base.Complete || uiforPlayer.xui.PlayerInventory == null)
		{
			return;
		}
		this.Refresh();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Toolbelt_OnToolbeltItemsChangedInternal()
	{
		LocalPlayerUI uiforPlayer = LocalPlayerUI.GetUIForPlayer(base.OwnerQuest.OwnerJournal.OwnerPlayer);
		if (base.Complete || uiforPlayer.xui.PlayerInventory == null)
		{
			return;
		}
		this.Refresh();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Current_ContainerOpened(Vector3i containerLocation, ITileEntityLootable lootTE)
	{
		Vector3 zero = Vector3.zero;
		base.OwnerQuest.GetPositionData(out zero, Quest.PositionDataTypes.TreasurePoint);
		if ((float)containerLocation.x != zero.x || (float)containerLocation.z != zero.z)
		{
			return;
		}
		if (this.GetItemCount() >= 1)
		{
			return;
		}
		if (GameManager.Instance.World.GetBlock(containerLocation).Block.GetBlockName() == this.containerName && lootTE != null && !lootTE.HasItem(this.expectedItem))
		{
			this.hasOpened = true;
			lootTE.AddItem(new ItemStack(this.expectedItem, 1));
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Current_ContainerClosed(Vector3i containerLocation, ITileEntityLootable lootTE)
	{
		if (GameManager.Instance.World.GetBlock(containerLocation).Block.GetBlockName() == this.containerName && lootTE != null)
		{
			lootTE.RemoveItem(this.expectedItem);
			lootTE.SetModified();
			if (base.Complete)
			{
				if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
				{
					QuestEventManager.Current.FinishTreasureQuest(base.OwnerQuest.QuestCode, base.OwnerQuest.OwnerJournal.OwnerPlayer);
					return;
				}
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageQuestObjectiveUpdate>().Setup(NetPackageQuestObjectiveUpdate.QuestObjectiveEventTypes.TreasureComplete, base.OwnerQuest.OwnerJournal.OwnerPlayer.entityId, base.OwnerQuest.QuestCode), false);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Current_BlockChange(Block blockOld, Block blockNew, Vector3i blockPos)
	{
		if (base.Complete || !this.hasOpened)
		{
			return;
		}
		Vector3 vector;
		base.OwnerQuest.GetPositionData(out vector, Quest.PositionDataTypes.TreasurePoint);
		this.containerPos = new Vector3i(vector);
		if (blockPos != this.containerPos)
		{
			return;
		}
		Chunk chunk = GameManager.Instance.World.GetChunkFromWorldPos(blockPos) as Chunk;
		if (chunk != null && chunk.IsDisplayed)
		{
			string blockName = blockNew.GetBlockName();
			if (blockName != this.containerName && blockName != this.altContainerName)
			{
				base.OwnerQuest.CloseQuest(Quest.QuestState.Failed, null);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int GetItemCount()
	{
		XUiM_PlayerInventory playerInventory = LocalPlayerUI.GetUIForPlayer(base.OwnerQuest.OwnerJournal.OwnerPlayer).xui.PlayerInventory;
		this.expectedItem.Meta = base.OwnerQuest.QuestCode;
		return playerInventory.Backpack.GetItemCount(this.expectedItem, -1, base.OwnerQuest.QuestCode, true) + playerInventory.Toolbelt.GetItemCount(this.expectedItem, false, -1, base.OwnerQuest.QuestCode, true);
	}

	public override void Refresh()
	{
		if (base.Complete)
		{
			return;
		}
		this.currentCount = this.GetItemCount();
		if (this.currentCount > 1)
		{
			this.currentCount = 1;
		}
		this.SetupDisplay();
		if (this.currentCount != (int)base.CurrentValue)
		{
			base.CurrentValue = (byte)this.currentCount;
		}
		base.Complete = this.currentCount >= this.itemCount && base.OwnerQuest.CheckRequirements();
		if (base.Complete)
		{
			base.OwnerQuest.RefreshQuestCompletion(QuestClass.CompletionTypes.AutoComplete, null, true, null);
			this.RemoveHooks();
		}
	}

	public override BaseObjective Clone()
	{
		ObjectiveFetchFromTreasure objectiveFetchFromTreasure = new ObjectiveFetchFromTreasure();
		this.CopyValues(objectiveFetchFromTreasure);
		return objectiveFetchFromTreasure;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void CopyValues(BaseObjective objective)
	{
		base.CopyValues(objective);
		ObjectiveFetchFromTreasure objectiveFetchFromTreasure = (ObjectiveFetchFromTreasure)objective;
		objectiveFetchFromTreasure.containerName = this.containerName;
		objectiveFetchFromTreasure.hasOpened = this.hasOpened;
		objectiveFetchFromTreasure.containerName = this.containerName;
		objectiveFetchFromTreasure.altContainerName = this.altContainerName;
	}

	public override bool SetLocation(Vector3 pos, Vector3 size)
	{
		return true;
	}

	public override void ParseProperties(DynamicProperties properties)
	{
		base.ParseProperties(properties);
		if (properties.Values.ContainsKey(ObjectiveFetchFromTreasure.PropQuestItemID))
		{
			this.ID = properties.Values[ObjectiveFetchFromTreasure.PropQuestItemID];
		}
		if (properties.Values.ContainsKey(ObjectiveFetchFromTreasure.PropItemCount))
		{
			this.Value = properties.Values[ObjectiveFetchFromTreasure.PropItemCount];
		}
		if (properties.Values.ContainsKey(ObjectiveFetchFromTreasure.PropBlock))
		{
			this.containerName = properties.Values[ObjectiveFetchFromTreasure.PropBlock];
		}
		if (properties.Values.ContainsKey(ObjectiveFetchFromTreasure.PropAltBlock))
		{
			this.altContainerName = properties.Values[ObjectiveFetchFromTreasure.PropAltBlock];
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemValue expectedItem = ItemValue.None;

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemClass expectedItemClass;

	[PublicizedFrom(EAccessModifier.Private)]
	public int itemCount;

	[PublicizedFrom(EAccessModifier.Private)]
	public int currentCount;

	[PublicizedFrom(EAccessModifier.Private)]
	public string containerName = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public string altContainerName = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public bool hasOpened;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3i containerPos = Vector3i.zero;

	public static string questItemClassID = "questItem";

	public static string PropQuestItemID = "quest_item_ID";

	public static string PropItemCount = "item_count";

	public static string PropBlock = "block";

	public static string PropAltBlock = "alt_block";
}
