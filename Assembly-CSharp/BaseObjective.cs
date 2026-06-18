using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public abstract class BaseObjective
{
	public event ObjectiveValueChanged ValueChanged;

	public byte CurrentVersion { get; set; }

	public BaseObjective.ObjectiveStates ObjectiveState { get; set; }

	public bool Complete
	{
		get
		{
			return this.ObjectiveState == BaseObjective.ObjectiveStates.Complete || this.ObjectiveState == BaseObjective.ObjectiveStates.Warning;
		}
		set
		{
			if (value)
			{
				this.ObjectiveState = BaseObjective.ObjectiveStates.Complete;
				this.DisableModifiers();
			}
		}
	}

	public virtual bool useUpdateLoop
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return false;
		}
	}

	public QuestClass OwnerQuestClass { get; set; }

	public Quest OwnerQuest { get; set; }

	public byte Phase { get; set; }

	public BaseObjective()
	{
		this.ObjectiveState = BaseObjective.ObjectiveStates.NotStarted;
		this.Phase = 1;
	}

	public virtual BaseObjective.ObjectiveValueTypes ObjectiveValueType
	{
		get
		{
			return BaseObjective.ObjectiveValueTypes.Boolean;
		}
	}

	public virtual bool PlayObjectiveComplete
	{
		get
		{
			return true;
		}
	}

	public virtual bool RequiresZombies
	{
		get
		{
			return false;
		}
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public void ChangeStatus(bool isSuccess)
	{
		this.ObjectiveState = (isSuccess ? BaseObjective.ObjectiveStates.Complete : BaseObjective.ObjectiveStates.Failed);
		if (isSuccess)
		{
			this.OwnerQuest.RallyMarkerActivated = true;
			this.OwnerQuest.RemoveMapObject();
			this.OwnerQuest.Tracked = true;
			this.OwnerQuest.OwnerJournal.TrackedQuest = this.OwnerQuest;
			this.OwnerQuest.OwnerJournal.RefreshTracked();
			this.OwnerQuest.OwnerJournal.ActiveQuest = this.OwnerQuest;
			this.OwnerQuest.RefreshQuestCompletion(QuestClass.CompletionTypes.AutoComplete, null, true, null);
			return;
		}
		this.OwnerQuest.CloseQuest(Quest.QuestState.Failed, null);
	}

	public virtual bool UpdateUI
	{
		get
		{
			return false;
		}
	}

	public virtual bool NeedsNPCSetPosition
	{
		get
		{
			return false;
		}
	}

	public string Description
	{
		get
		{
			if (!this.displaySetup)
			{
				this.SetupDisplay();
				this.displaySetup = true;
			}
			return this.description;
		}
		[PublicizedFrom(EAccessModifier.Protected)]
		set
		{
			this.description = value;
		}
	}

	public virtual string StatusText
	{
		get
		{
			if (!this.displaySetup)
			{
				this.SetupDisplay();
				this.displaySetup = true;
			}
			return this.statusText;
		}
		[PublicizedFrom(EAccessModifier.Protected)]
		set
		{
			this.statusText = value;
		}
	}

	public byte CurrentValue
	{
		get
		{
			return this.currentValue;
		}
		set
		{
			this.currentValue = value;
			this.SetupDisplay();
			if (this.ValueChanged != null)
			{
				this.ValueChanged();
			}
		}
	}

	public bool Optional { get; set; }

	public virtual bool AlwaysComplete
	{
		get
		{
			return false;
		}
	}

	public virtual bool ShowInQuestLog
	{
		get
		{
			return true;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void CopyValues(BaseObjective objective)
	{
		objective.ID = this.ID;
		objective.Value = this.Value;
		objective.Optional = this.Optional;
		objective.currentValue = this.currentValue;
		objective.Phase = this.Phase;
		objective.NavObjectName = this.NavObjectName;
		objective.HiddenObjective = this.HiddenObjective;
		objective.ForcePhaseFinish = this.ForcePhaseFinish;
		if (this.Modifiers != null)
		{
			for (int i = 0; i < this.Modifiers.Count; i++)
			{
				objective.AddModifier(this.Modifiers[i].Clone());
			}
		}
	}

	public virtual void HandleVariables()
	{
		this.ID = this.OwnerQuest.ParseVariable(this.ID);
		this.Value = this.OwnerQuest.ParseVariable(this.Value);
	}

	public virtual void SetupQuestTag()
	{
	}

	public virtual void SetupObjective()
	{
	}

	public virtual void SetupDisplay()
	{
	}

	public virtual bool SetupPosition(EntityNPC ownerNPC = null, EntityPlayer player = null, List<Vector2> usedPOILocations = null, int entityIDforQuests = -1)
	{
		return false;
	}

	public virtual bool SetupActivationList(Vector3 prefabPos, List<Vector3i> activateList)
	{
		return false;
	}

	public virtual void SetPosition(Vector3 position, Vector3 size)
	{
	}

	public virtual void SetPosition(Quest.PositionDataTypes dataType, Vector3i position)
	{
	}

	public void HandleAddHooks()
	{
		this.AddHooks();
		if (!this.Complete && this.Modifiers != null)
		{
			for (int i = 0; i < this.Modifiers.Count; i++)
			{
				this.Modifiers[i].OwnerObjective = this;
				this.Modifiers[i].HandleAddHooks();
			}
		}
		if (this.useUpdateLoop)
		{
			QuestEventManager.Current.AddObjectiveToBeUpdated(this);
		}
	}

	public void HandleRemoveHooks()
	{
		this.RemoveHooks();
		this.RemoveNavObject();
		if (this.Modifiers != null)
		{
			for (int i = 0; i < this.Modifiers.Count; i++)
			{
				this.Modifiers[i].HandleRemoveHooks();
			}
		}
		if (this.useUpdateLoop)
		{
			QuestEventManager.Current.RemoveObjectiveToBeUpdated(this);
		}
	}

	public virtual void AddHooks()
	{
	}

	public virtual void AddNavObject(Vector3 position)
	{
		if (this.NavObjectName != "")
		{
			this.NavObject = NavObjectManager.Instance.RegisterNavObject(this.NavObjectName, position, "", false, -1, null);
		}
	}

	public virtual void RemoveHooks()
	{
	}

	public virtual void RemoveNavObject()
	{
		if (this.NavObject != null)
		{
			NavObjectManager.Instance.UnRegisterNavObject(this.NavObject);
			this.NavObject = null;
		}
	}

	public virtual void Refresh()
	{
	}

	public virtual void RemoveObjectives()
	{
	}

	public virtual void HandleCompleted()
	{
	}

	public virtual void HandlePhaseCompleted()
	{
	}

	public virtual void HandleFailed()
	{
	}

	public virtual void ResetObjective()
	{
		this.CurrentValue = 0;
	}

	public virtual void Read(BinaryReader _br)
	{
		this.CurrentVersion = _br.ReadByte();
		this.currentValue = _br.ReadByte();
	}

	public virtual void Write(BinaryWriter _bw)
	{
		_bw.Write(BaseObjective.FileVersion);
		_bw.Write(this.CurrentValue);
	}

	public virtual BaseObjective Clone()
	{
		return null;
	}

	public void HandleUpdate(float deltaTime)
	{
		if (this.Phase == this.OwnerQuest.CurrentPhase)
		{
			this.Update(deltaTime);
		}
	}

	public virtual void Update(float deltaTime)
	{
		if (Time.time > this.updateTime)
		{
			this.updateTime = Time.time + 1f;
			switch (this.CurrentValue)
			{
			case 0:
				this.UpdateState_NeedSetup();
				return;
			case 1:
				this.UpdateState_WaitingForServer();
				return;
			case 2:
				this.UpdateState_Update();
				return;
			case 3:
				this.UpdateState_Completed();
				QuestEventManager.Current.RemoveObjectiveToBeUpdated(this);
				break;
			default:
				return;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void UpdateState_NeedSetup()
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void UpdateState_WaitingForServer()
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void UpdateState_Update()
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void UpdateState_Completed()
	{
	}

	public virtual bool SetLocation(Vector3 pos, Vector3 size)
	{
		return false;
	}

	public virtual string ParseBinding(string bindingName)
	{
		return "";
	}

	public virtual void ParseProperties(DynamicProperties properties)
	{
		this.Properties = properties;
		this.OwnerQuestClass.HandleVariablesForProperties(properties);
		if (properties.Values.ContainsKey(BaseObjective.PropID))
		{
			this.ID = properties.Values[BaseObjective.PropID];
		}
		if (properties.Values.ContainsKey(BaseObjective.PropValue))
		{
			this.Value = properties.Values[BaseObjective.PropValue];
		}
		if (properties.Values.ContainsKey(BaseObjective.PropPhase))
		{
			this.Phase = Convert.ToByte(properties.Values[BaseObjective.PropPhase]);
			if (this.Phase > this.OwnerQuestClass.HighestPhase)
			{
				this.OwnerQuestClass.HighestPhase = this.Phase;
			}
		}
		if (properties.Values.ContainsKey(BaseObjective.PropOptional))
		{
			bool flag;
			StringParsers.TryParseBool(properties.Values[BaseObjective.PropOptional], out flag);
			this.Optional = flag;
		}
		if (properties.Values.ContainsKey(BaseObjective.PropNavObject))
		{
			this.NavObjectName = properties.Values[BaseObjective.PropNavObject];
		}
		if (properties.Values.ContainsKey(BaseObjective.PropHidden))
		{
			this.HiddenObjective = StringParsers.ParseBool(properties.Values[BaseObjective.PropHidden], 0, -1, true);
		}
		properties.ParseBool(BaseObjective.PropForcePhaseFinish, ref this.ForcePhaseFinish);
	}

	public void AddModifier(BaseObjectiveModifier modifier)
	{
		if (this.Modifiers == null)
		{
			this.Modifiers = new List<BaseObjectiveModifier>();
		}
		this.Modifiers.Add(modifier);
		modifier.OwnerObjective = this;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void DisableModifiers()
	{
		if (this.Modifiers != null)
		{
			for (int i = 0; i < this.Modifiers.Count; i++)
			{
				this.Modifiers[i].HandleRemoveHooks();
			}
		}
	}

	public static byte FileVersion = 0;

	public static string PropID = "id";

	public static string PropValue = "value";

	public static string PropPhase = "phase";

	public static string PropOptional = "optional";

	public static string PropNavObject = "nav_object";

	public static string PropHidden = "hidden";

	public static string PropForcePhaseFinish = "force_phase_finish";

	public string ID;

	public string Value;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool displaySetup;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string keyword = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public string description = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public string statusText = "";

	public bool HiddenObjective;

	public bool ForcePhaseFinish;

	[PublicizedFrom(EAccessModifier.Protected)]
	public NavObject NavObject;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string NavObjectName = "";

	public List<BaseObjectiveModifier> Modifiers;

	public DynamicProperties Properties;

	[PublicizedFrom(EAccessModifier.Protected)]
	public byte currentValue;

	[PublicizedFrom(EAccessModifier.Private)]
	public float updateTime;

	public enum ObjectiveStates
	{
		NotStarted,
		InProgress,
		Warning,
		Complete,
		Failed
	}

	public enum ObjectiveTypes
	{
		AnimalKill,
		Assemble,
		BlockPickup,
		BlockPlace,
		BlockUpgrade,
		Buff,
		ExchangeItemFrom,
		Fetch,
		FetchKeep,
		CraftItem,
		Repair,
		Scrap,
		SkillsPurchased,
		Time,
		Wear,
		WindowOpen,
		ZombieKill
	}

	public enum ObjectiveValueTypes
	{
		Boolean,
		Number,
		Time,
		Distance
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public enum UpdateStates
	{
		NeedSetup,
		WaitingForServer,
		Update,
		Completed
	}
}
