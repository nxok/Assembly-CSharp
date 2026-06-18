using System;
using UnityEngine.Scripting;

[Preserve]
public class ObjectiveGameEvent : BaseObjective
{
	public override bool useUpdateLoop
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return true;
		}
	}

	public override void SetupObjective()
	{
		this.keyword = Localization.Get("ObjectiveAssemble_keyword", false, null);
	}

	public override void SetupDisplay()
	{
		base.Description = "Test Game Event";
		this.StatusText = "";
	}

	public override void AddHooks()
	{
		GameEventManager gameEventManager = GameEventManager.Current;
		gameEventManager.GameEventCompleted += this.Current_GameEventCompleted;
		gameEventManager.GameEventDenied += this.Current_GameEventDenied;
	}

	public override void RemoveHooks()
	{
		GameEventManager gameEventManager = GameEventManager.Current;
		gameEventManager.GameEventCompleted -= this.Current_GameEventCompleted;
		gameEventManager.GameEventDenied -= this.Current_GameEventDenied;
	}

	public override void Update(float updateTime)
	{
		switch (this.GameEventState)
		{
		case ObjectiveGameEvent.GameEventStates.Start:
		{
			EntityPlayer ownerPlayer = base.OwnerQuest.OwnerJournal.OwnerPlayer;
			GameEventManager.Current.HandleAction(this.gameEventID, ownerPlayer, ownerPlayer, false, "", this.gameEventTag, false, true, "", null);
			this.GameEventState = ObjectiveGameEvent.GameEventStates.Waiting;
			break;
		}
		case ObjectiveGameEvent.GameEventStates.Waiting:
		case ObjectiveGameEvent.GameEventStates.Complete:
			break;
		default:
			return;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Current_GameEventCompleted(string _gameEventID, int _targetEntityID, string _extraData, string _tag)
	{
		if (this.gameEventID == _gameEventID && _tag == this.gameEventTag && _targetEntityID == base.OwnerQuest.OwnerJournal.OwnerPlayer.entityId && base.OwnerQuest.CheckRequirements())
		{
			base.CurrentValue = 1;
			this.Refresh();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Current_GameEventDenied(string gameEventID, int targetEntityID, string extraData, string tag)
	{
	}

	public override void Refresh()
	{
		if (base.Complete)
		{
			return;
		}
		base.Complete = base.CurrentValue == 1;
		if (base.Complete)
		{
			base.OwnerQuest.RefreshQuestCompletion(QuestClass.CompletionTypes.AutoComplete, null, true, null);
		}
	}

	public override BaseObjective Clone()
	{
		ObjectiveGameEvent objectiveGameEvent = new ObjectiveGameEvent();
		this.CopyValues(objectiveGameEvent);
		objectiveGameEvent.gameEventID = this.gameEventID;
		objectiveGameEvent.gameEventTag = this.gameEventTag;
		return objectiveGameEvent;
	}

	public override void ParseProperties(DynamicProperties properties)
	{
		base.ParseProperties(properties);
		properties.ParseString(ObjectiveGameEvent.PropGameEventID, ref this.gameEventID);
		properties.ParseString(ObjectiveGameEvent.PropGameEventTag, ref this.gameEventTag);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public string gameEventID = "";

	[PublicizedFrom(EAccessModifier.Protected)]
	public string gameEventTag = "quest";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropGameEventID = "event";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropGameEventTag = "event_tag";

	[PublicizedFrom(EAccessModifier.Protected)]
	public ObjectiveGameEvent.GameEventStates GameEventState;

	[PublicizedFrom(EAccessModifier.Protected)]
	public enum GameEventStates
	{
		Start,
		Waiting,
		Complete
	}
}
