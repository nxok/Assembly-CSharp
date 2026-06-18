using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ObjectiveFetchFromContainer : ObjectiveBaseFetchContainer
{
	public override BaseObjective.ObjectiveValueTypes ObjectiveValueType
	{
		get
		{
			if (base.CurrentValue != 3)
			{
				return BaseObjective.ObjectiveValueTypes.Distance;
			}
			return BaseObjective.ObjectiveValueTypes.Boolean;
		}
	}

	public override bool UpdateUI
	{
		get
		{
			return base.ObjectiveState != BaseObjective.ObjectiveStates.Failed && base.CurrentValue != 3;
		}
	}

	public override void SetupQuestTag()
	{
		base.OwnerQuest.AddQuestTag((this.FetchMode == ObjectiveFetchFromContainer.FetchModeTypes.Standard) ? QuestEventManager.fetchTag : FastTags<TagGroup.Global>.Parse("hidden_cache"));
	}

	public override void SetupObjective()
	{
		this.keyword = Localization.Get("ObjectiveFetchContainer_keyword", false, null);
		if (this.expectedItemClass == null)
		{
			base.SetupExpectedItem();
		}
		base.OwnerQuest.AddQuestTag((this.FetchMode == ObjectiveFetchFromContainer.FetchModeTypes.Standard) ? QuestEventManager.fetchTag : FastTags<TagGroup.Global>.Parse("hidden_cache"));
	}

	public override void SetupDisplay()
	{
		base.Description = string.Format(this.keyword, this.expectedItemClass.GetLocalizedItemName());
		this.StatusText = "";
		this.nearbyKeyword = Localization.Get("ObjectiveNearby_keyword", false, null);
	}

	public override string StatusText
	{
		get
		{
			if (base.OwnerQuest.CurrentState == Quest.QuestState.InProgress)
			{
				if (this.FetchMode != ObjectiveFetchFromContainer.FetchModeTypes.Standard && this.distance < 10f)
				{
					return this.nearbyKeyword;
				}
				return ValueDisplayFormatters.Distance(this.distance);
			}
			else
			{
				if (base.OwnerQuest.CurrentState == Quest.QuestState.NotStarted)
				{
					return "";
				}
				if (base.ObjectiveState == BaseObjective.ObjectiveStates.Failed)
				{
					return Localization.Get("failed", false, null);
				}
				return Localization.Get("completed", false, null);
			}
		}
	}

	public override void HandleFailed()
	{
		base.HandleFailed();
		base.OwnerQuest.RemovePositionData((this.FetchMode == ObjectiveFetchFromContainer.FetchModeTypes.Standard) ? Quest.PositionDataTypes.FetchContainer : Quest.PositionDataTypes.HiddenCache);
		base.OwnerQuest.RemoveMapObject();
	}

	public override void AddHooks()
	{
		base.CurrentValue = 0;
		QuestEventManager.Current.AddObjectiveToBeUpdated(this);
		LocalPlayerUI.GetUIForPlayer(base.OwnerQuest.OwnerJournal.OwnerPlayer);
		XUiM_PlayerInventory playerInventory = LocalPlayerUI.GetUIForPlayer(base.OwnerQuest.OwnerJournal.OwnerPlayer).xui.PlayerInventory;
		playerInventory.Backpack.OnBackpackItemsChangedInternal += this.Backpack_OnBackpackItemsChangedInternal;
		playerInventory.Toolbelt.OnToolbeltItemsChangedInternal += this.Toolbelt_OnToolbeltItemsChangedInternal;
		QuestEventManager.Current.ContainerOpened += this.Current_ContainerOpened;
		QuestEventManager.Current.ContainerClosed += this.Current_ContainerClosed;
	}

	public override void RemoveObjectives()
	{
		QuestEventManager.Current.ContainerOpened -= this.Current_ContainerOpened;
		QuestEventManager.Current.ContainerClosed -= this.Current_ContainerClosed;
	}

	public override void RemoveHooks()
	{
		QuestEventManager.Current.RemoveObjectiveToBeUpdated(this);
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

	public override void SetPosition(Quest.PositionDataTypes dataType, Vector3i position)
	{
		if (base.Phase == base.OwnerQuest.CurrentPhase && ((this.FetchMode == ObjectiveFetchFromContainer.FetchModeTypes.Standard) ? Quest.PositionDataTypes.FetchContainer : Quest.PositionDataTypes.HiddenCache) == dataType)
		{
			this.FinalizePoint(position);
		}
	}

	public override void ResetObjective()
	{
		base.ResetObjective();
		Quest.PositionDataTypes positionDataTypes = ((this.FetchMode == ObjectiveFetchFromContainer.FetchModeTypes.Standard) ? Quest.PositionDataTypes.FetchContainer : Quest.PositionDataTypes.HiddenCache);
		base.OwnerQuest.RemovePositionData(positionDataTypes);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 GetPosition()
	{
		Quest.PositionDataTypes positionDataTypes = ((this.FetchMode == ObjectiveFetchFromContainer.FetchModeTypes.Standard) ? Quest.PositionDataTypes.FetchContainer : Quest.PositionDataTypes.HiddenCache);
		if (base.OwnerQuest.GetPositionData(out this.position, positionDataTypes))
		{
			base.OwnerQuest.HandleMapObject(positionDataTypes, this.NavObjectName, -1);
			base.CurrentValue = 2;
			return this.position;
		}
		Vector3 zero = Vector3.zero;
		base.OwnerQuest.GetPositionData(out zero, Quest.PositionDataTypes.POIPosition);
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			if (base.OwnerQuest.SharedOwnerID == -1)
			{
				QuestEventManager.Current.SetupFetchForMP(base.OwnerQuest.OwnerJournal.OwnerPlayer.entityId, zero, this.FetchMode, base.OwnerQuest.GetSharedWithIDList());
			}
			base.CurrentValue = 2;
		}
		else
		{
			if (base.OwnerQuest.SharedOwnerID == -1)
			{
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageQuestEvent>().Setup(NetPackageQuestEvent.QuestEventTypes.SetupFetch, base.OwnerQuest.OwnerJournal.OwnerPlayer.entityId, zero, this.FetchMode, base.OwnerQuest.GetSharedWithIDList()), false);
			}
			base.CurrentValue = 1;
		}
		return Vector3.zero;
	}

	public void FinalizePoint(Vector3i containerPos)
	{
		Quest.PositionDataTypes positionDataTypes = ((this.FetchMode == ObjectiveFetchFromContainer.FetchModeTypes.Standard) ? Quest.PositionDataTypes.FetchContainer : Quest.PositionDataTypes.HiddenCache);
		this.position = containerPos.ToVector3();
		base.OwnerQuest.SetPositionData(positionDataTypes, this.position);
		this.lootContainerPos = containerPos;
		base.OwnerQuest.HandleMapObject(positionDataTypes, this.NavObjectName, -1);
		base.CurrentValue = 2;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Current_ContainerOpened(Vector3i containerLocation, ITileEntityLootable lootTE)
	{
		if (base.GetItemCount(-2) >= 1)
		{
			return;
		}
		if (containerLocation == this.lootContainerPos && lootTE != null && !lootTE.HasItem(this.expectedItem))
		{
			lootTE.AddItem(new ItemStack(this.expectedItem, 1));
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Current_ContainerClosed(Vector3i containerLocation, ITileEntityLootable lootTE)
	{
		if (containerLocation == this.lootContainerPos && lootTE != null)
		{
			lootTE.RemoveItem(this.expectedItem);
			lootTE.SetModified();
		}
	}

	public override void Refresh()
	{
		if (base.Complete)
		{
			return;
		}
		this.currentCount = base.GetItemCount(-2);
		if (this.currentCount == 0)
		{
			return;
		}
		this.SetupDisplay();
		base.CurrentValue = 3;
		base.Complete = base.OwnerQuest.CheckRequirements();
		if (base.Complete)
		{
			base.OwnerQuest.RemovePositionData((this.FetchMode == ObjectiveFetchFromContainer.FetchModeTypes.Standard) ? Quest.PositionDataTypes.FetchContainer : Quest.PositionDataTypes.HiddenCache);
			base.OwnerQuest.RemoveMapObject();
			base.OwnerQuest.RefreshQuestCompletion(QuestClass.CompletionTypes.AutoComplete, null, true, null);
			this.RemoveHooks();
		}
	}

	public override BaseObjective Clone()
	{
		ObjectiveFetchFromContainer objectiveFetchFromContainer = new ObjectiveFetchFromContainer();
		this.CopyValues(objectiveFetchFromContainer);
		return objectiveFetchFromContainer;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void CopyValues(BaseObjective objective)
	{
		base.CopyValues(objective);
		ObjectiveFetchFromContainer objectiveFetchFromContainer = (ObjectiveFetchFromContainer)objective;
		objectiveFetchFromContainer.FetchMode = this.FetchMode;
		objectiveFetchFromContainer.defaultContainer = this.defaultContainer;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void UpdateState_NeedSetup()
	{
		this.GetPosition();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void UpdateState_Update()
	{
		Entity ownerPlayer = base.OwnerQuest.OwnerJournal.OwnerPlayer;
		this.position.y = 0f;
		Vector3 vector = ownerPlayer.position;
		vector.y = 0f;
		this.distance = Vector3.Distance(vector, this.position);
		if (this.world == null)
		{
			this.world = GameManager.Instance.World;
		}
		if (this.distance < 5f)
		{
			BlockValue block = this.world.GetBlock(this.lootContainerPos);
			if (block.Block.IndexName == null || !block.Block.IndexName.EqualsCaseInsensitive("fetchcontainer"))
			{
				this.world.SetBlockRPC(this.lootContainerPos, Block.GetBlockValue(this.defaultContainer, false));
			}
		}
	}

	public override void ParseProperties(DynamicProperties properties)
	{
		base.ParseProperties(properties);
		if (properties.Values.ContainsKey(ObjectiveFetchFromContainer.PropFetchMode))
		{
			this.FetchMode = EnumUtils.Parse<ObjectiveFetchFromContainer.FetchModeTypes>(properties.Values[ObjectiveFetchFromContainer.PropFetchMode], false);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public Vector3 position;

	public string nearbyKeyword = "";

	public static string PropFetchMode = "fetch_mode";

	public ObjectiveFetchFromContainer.FetchModeTypes FetchMode;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3i lootContainerPos;

	public enum FetchModeTypes
	{
		Standard,
		Hidden
	}
}
