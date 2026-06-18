using System;
using System.Collections.Generic;
using GameEvent.SequenceActions;
using GameEvent.SequenceRequirements;
using UnityEngine;

public class GameEventActionSequence
{
	public GameEventVariables EventVariables
	{
		get
		{
			if (this.eventVariables == null)
			{
				this.eventVariables = new GameEventVariables();
			}
			return this.eventVariables;
		}
	}

	public bool HasTarget()
	{
		if (this.TargetType == GameEventActionSequence.TargetTypes.Entity)
		{
			return this.Target != null && !this.DeadCheck;
		}
		if (this.TargetType == GameEventActionSequence.TargetTypes.POI)
		{
			return this.POIPosition != Vector3i.zero;
		}
		return this.blockValue.type != GameManager.Instance.World.GetBlock(this.POIPosition).type || this.AllowWhileDead;
	}

	public bool DeadCheck
	{
		get
		{
			return !this.Target.IsAlive() && !this.AllowWhileDead;
		}
	}

	public void SetupTarget()
	{
		if (this.TargetType == GameEventActionSequence.TargetTypes.POI)
		{
			if (this.POIPosition != Vector3i.zero)
			{
				this.POIInstance = GameManager.Instance.GetDynamicPrefabDecorator().GetPrefabFromWorldPos(this.POIPosition.x, this.POIPosition.z);
				return;
			}
			EntityPlayer entityPlayer = this.Target as EntityPlayer;
			if (entityPlayer != null)
			{
				this.POIInstance = entityPlayer.prefab;
				if (this.POIInstance != null)
				{
					this.POIPosition = this.POIInstance.boundingBoxPosition;
					return;
				}
			}
		}
		else if (this.TargetType == GameEventActionSequence.TargetTypes.Entity)
		{
			EntityPlayer entityPlayer2 = this.Target as EntityPlayer;
			if (entityPlayer2 != null)
			{
				this.POIInstance = entityPlayer2.prefab;
				if (this.POIInstance != null)
				{
					this.POIPosition = this.POIInstance.boundingBoxPosition;
					return;
				}
			}
		}
		else if (this.TargetType == GameEventActionSequence.TargetTypes.Block)
		{
			if (this.POIPosition != Vector3i.zero)
			{
				this.POIInstance = GameManager.Instance.GetDynamicPrefabDecorator().GetPrefabFromWorldPos(this.POIPosition.x, this.POIPosition.z);
				return;
			}
			this.POIInstance = GameManager.Instance.GetDynamicPrefabDecorator().GetPrefabFromWorldPos((int)this.TargetPosition.x, (int)this.TargetPosition.z);
		}
	}

	public void StartSequence(GameEventManager manager)
	{
		this.StartTime = Time.time;
	}

	public void Init()
	{
		List<int> list = new List<int>();
		for (int i = 0; i < this.Actions.Count; i++)
		{
			if (!list.Contains(this.Actions[i].Phase))
			{
				list.Add(this.Actions[i].Phase);
			}
			this.Actions[i].SetActionKeyData(i, null, this.Name);
		}
		list.Sort();
		if (list.Count > 0)
		{
			this.PhaseMax = list[list.Count - 1] + 1;
		}
		else
		{
			this.PhaseMax = 0;
		}
		this.IsComplete = false;
	}

	public bool CanPerform(Entity player)
	{
		for (int i = 0; i < this.Requirements.Count; i++)
		{
			if (!this.Requirements[i].CanPerform(player))
			{
				return false;
			}
		}
		for (int j = 0; j < this.Actions.Count; j++)
		{
			if (!this.Actions[j].CanPerform(player))
			{
				return false;
			}
		}
		return true;
	}

	public void HandleVariablesForProperties(DynamicProperties properties)
	{
		if (properties == null)
		{
			return;
		}
		foreach (KeyValuePair<string, string> keyValuePair in properties.Params1)
		{
			if (this.Variables.ContainsKey(keyValuePair.Value))
			{
				properties.Values[keyValuePair.Key] = this.Variables[keyValuePair.Value];
			}
		}
	}

	public void ParseProperties(DynamicProperties properties)
	{
		this.Properties = properties;
		if (properties.Values.ContainsKey(GameEventActionSequence.PropAllowUserTrigger))
		{
			this.AllowUserTrigger = StringParsers.ParseBool(properties.Values[GameEventActionSequence.PropAllowUserTrigger], 0, -1, true);
		}
		properties.ParseEnum<GameEventActionSequence.ActionTypes>(GameEventActionSequence.PropActionType, ref this.ActionType);
		if (properties.Values.ContainsKey(GameEventActionSequence.PropAllowWhileDead))
		{
			this.AllowWhileDead = StringParsers.ParseBool(properties.Values[GameEventActionSequence.PropAllowWhileDead], 0, -1, true);
		}
		properties.ParseEnum<GameEventActionSequence.TargetTypes>(GameEventActionSequence.PropTargetType, ref this.TargetType);
		properties.ParseBool(GameEventActionSequence.PropRefundInactivity, ref this.RefundInactivity);
		properties.ParseBool(GameEventActionSequence.PropSingleInstance, ref this.SingleInstance);
		string text = "";
		properties.ParseString(GameEventActionSequence.PropCategory, ref text);
		if (text != "")
		{
			this.CategoryNames = text.Split(',', StringSplitOptions.None);
		}
	}

	public void Update()
	{
		bool flag = false;
		int num = this.CurrentPhase;
		for (int i = 0; i < this.Actions.Count; i++)
		{
			if (this.Actions[i].Phase == this.CurrentPhase && !this.Actions[i].IsComplete)
			{
				flag = true;
				BaseAction.ActionCompleteStates actionCompleteStates;
				if (this.AllowRefunds && this.RefundInactivity && Time.time - this.StartTime > 60f)
				{
					actionCompleteStates = BaseAction.ActionCompleteStates.InCompleteRefund;
				}
				else
				{
					actionCompleteStates = this.Actions[i].PerformAction();
				}
				if (actionCompleteStates == BaseAction.ActionCompleteStates.Complete || (actionCompleteStates == BaseAction.ActionCompleteStates.InCompleteRefund && this.Actions[i].IgnoreRefund))
				{
					this.Actions[i].IsComplete = true;
					if (this.Actions[i].PhaseOnComplete != -1)
					{
						num = this.Actions[i].PhaseOnComplete;
					}
				}
				else if (actionCompleteStates == BaseAction.ActionCompleteStates.RequirementsNotMet)
				{
					this.Actions[i].IsComplete = true;
					if (this.Actions[i].PhaseOnDenied != -1)
					{
						num = this.Actions[i].PhaseOnDenied;
					}
				}
				else if (this.AllowRefunds && actionCompleteStates == BaseAction.ActionCompleteStates.InCompleteRefund)
				{
					if (this.ActionType == GameEventActionSequence.ActionTypes.TwitchAction)
					{
						if (this.Requester is EntityPlayerLocal)
						{
							GameEventManager.Current.HandleTwitchRefundNeeded(this.Name, this.Target.entityId, this.ExtraData, this.Tag);
						}
						else
						{
							SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageGameEventResponse>().Setup(this.Name, this.Target.entityId, this.ExtraData, this.Tag, NetPackageGameEventResponse.ResponseTypes.TwitchRefundNeeded, -1, -1, false, ""), false, this.Requester.entityId, -1, -1, null, 192, false);
						}
						this.IsComplete = true;
					}
					else
					{
						this.Actions[i].IsComplete = true;
					}
				}
			}
		}
		if (!flag)
		{
			this.CurrentPhase++;
		}
		else if (this.CurrentPhase != num)
		{
			this.CurrentPhase = num;
			for (int j = 0; j < this.Actions.Count; j++)
			{
				if (this.Actions[j].Phase >= this.CurrentPhase)
				{
					this.Actions[j].Reset();
				}
			}
		}
		if (this.CurrentPhase >= this.PhaseMax)
		{
			this.IsComplete = true;
			if (this.Requester != null)
			{
				if (this.Requester is EntityPlayerLocal)
				{
					GameEventManager.Current.HandleGameEventCompleted(this.Name, this.Target ? this.Target.entityId : (-1), this.ExtraData, this.Tag);
					return;
				}
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageGameEventResponse>().Setup(this.Name, this.Target ? this.Target.entityId : (-1), this.ExtraData, this.Tag, NetPackageGameEventResponse.ResponseTypes.Completed, -1, -1, false, ""), false, this.Requester.entityId, -1, -1, null, 192, false);
			}
		}
	}

	public void HandleClientPerform(EntityPlayer player, string key)
	{
		BaseAction baseAction = BaseAction.FindKey(key);
		if (baseAction != null)
		{
			baseAction.OnClientPerform(player);
		}
	}

	public void AddEntitiesToGroup(string groupName, List<Entity> entityList, bool twitchNegative)
	{
		for (int i = entityList.Count - 1; i >= 0; i--)
		{
			EntityPlayer entityPlayer = entityList[i] as EntityPlayer;
			if (entityPlayer != null)
			{
				EntityPlayer.TwitchActionsStates twitchActionsEnabled = entityPlayer.TwitchActionsEnabled;
				if (twitchActionsEnabled != EntityPlayer.TwitchActionsStates.Enabled && (twitchActionsEnabled == EntityPlayer.TwitchActionsStates.Disabled || twitchNegative))
				{
					entityList.RemoveAt(i);
				}
			}
		}
		if (entityList.Count == 0)
		{
			return;
		}
		if (this.EntityGroups == null)
		{
			this.EntityGroups = new Dictionary<string, List<Entity>>();
		}
		if (this.EntityGroups.ContainsKey(groupName))
		{
			this.EntityGroups[groupName] = entityList;
			return;
		}
		this.EntityGroups.Add(groupName, entityList);
	}

	public void AddEntityToGroup(string groupName, Entity entity)
	{
		if (this.ActionType == GameEventActionSequence.ActionTypes.TwitchAction && entity is EntityPlayer && (entity as EntityPlayer).TwitchActionsEnabled != EntityPlayer.TwitchActionsStates.Enabled)
		{
			return;
		}
		if (this.EntityGroups == null)
		{
			this.EntityGroups = new Dictionary<string, List<Entity>>();
		}
		if (!this.EntityGroups.ContainsKey(groupName))
		{
			this.EntityGroups.Add(groupName, new List<Entity>());
		}
		this.EntityGroups[groupName].Add(entity);
	}

	public List<Entity> GetEntityGroup(string groupName)
	{
		if (this.EntityGroups == null || !this.EntityGroups.ContainsKey(groupName))
		{
			return null;
		}
		return this.EntityGroups[groupName];
	}

	public int GetEntityGroupLiveCount(string groupName)
	{
		if (this.EntityGroups == null || !this.EntityGroups.ContainsKey(groupName))
		{
			return 0;
		}
		int num = 0;
		List<Entity> list = this.EntityGroups[groupName];
		for (int i = 0; i < list.Count; i++)
		{
			EntityAlive entityAlive = list[i] as EntityAlive;
			if (entityAlive != null && entityAlive.IsAlive())
			{
				num++;
			}
		}
		return num;
	}

	public void ClearEntityGroup(string groupName)
	{
		if (this.EntityGroups == null || !this.EntityGroups.ContainsKey(groupName))
		{
			return;
		}
		this.EntityGroups[groupName].Clear();
	}

	public GameEventActionSequence Clone()
	{
		GameEventActionSequence gameEventActionSequence = new GameEventActionSequence();
		gameEventActionSequence.Name = this.Name;
		gameEventActionSequence.PhaseMax = this.PhaseMax;
		gameEventActionSequence.CurrentPhase = this.CurrentPhase;
		gameEventActionSequence.AllowUserTrigger = this.AllowUserTrigger;
		gameEventActionSequence.AllowWhileDead = this.AllowWhileDead;
		gameEventActionSequence.ActionType = this.ActionType;
		gameEventActionSequence.CrateShare = this.CrateShare;
		gameEventActionSequence.TargetType = this.TargetType;
		gameEventActionSequence.SingleInstance = this.SingleInstance;
		gameEventActionSequence.RefundInactivity = this.RefundInactivity;
		for (int i = 0; i < this.Actions.Count; i++)
		{
			BaseAction baseAction = this.Actions[i].Clone();
			baseAction.Owner = gameEventActionSequence;
			gameEventActionSequence.Actions.Add(baseAction);
		}
		return gameEventActionSequence;
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public DynamicProperties AssignValuesFrom(GameEventActionSequence oldSeq)
	{
		DynamicProperties dynamicProperties = new DynamicProperties();
		HashSet<string> hashSet = new HashSet<string> { GameEventActionSequence.PropAllowUserTrigger };
		if (oldSeq.Properties != null)
		{
			dynamicProperties.CopyFrom(oldSeq.Properties, hashSet);
		}
		for (int i = 0; i < oldSeq.Requirements.Count; i++)
		{
			BaseRequirement baseRequirement = oldSeq.Requirements[i].Clone();
			baseRequirement.Properties = new DynamicProperties();
			if (oldSeq.Requirements[i].Properties != null)
			{
				baseRequirement.Properties.CopyFrom(oldSeq.Requirements[i].Properties, null);
			}
			baseRequirement.Owner = this;
			baseRequirement.Init();
			this.Requirements.Add(baseRequirement);
		}
		for (int j = 0; j < oldSeq.Actions.Count; j++)
		{
			BaseAction baseAction = oldSeq.Actions[j].HandleAssignFrom(this, oldSeq);
			this.Actions.Add(baseAction);
		}
		return dynamicProperties;
	}

	public void HandleTemplateInit()
	{
		for (int i = 0; i < this.Actions.Count; i++)
		{
			this.Actions[i].HandleTemplateInit(this);
		}
		for (int j = 0; j < this.Requirements.Count; j++)
		{
			this.HandleVariablesForProperties(this.Requirements[j].Properties);
			this.Requirements[j].ParseProperties(this.Requirements[j].Properties);
			this.Requirements[j].Init();
		}
	}

	public void SetRefundNeeded()
	{
		if (this.ActionType == GameEventActionSequence.ActionTypes.TwitchAction)
		{
			if (this.Requester is EntityPlayerLocal)
			{
				GameEventManager.Current.HandleTwitchRefundNeeded(this.Name, this.Target.entityId, this.ExtraData, this.Tag);
			}
			else
			{
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageGameEventResponse>().Setup(this.Name, this.Target.entityId, this.ExtraData, this.Tag, NetPackageGameEventResponse.ResponseTypes.TwitchRefundNeeded, -1, -1, false, ""), false, this.Requester.entityId, -1, -1, null, 192, false);
			}
			this.IsComplete = true;
		}
	}

	public string Name;

	public int PhaseMax = 1;

	public int CurrentPhase;

	public string ExtraData = "";

	public string Tag = "";

	public int ReservedSpawnCount;

	public GameEventActionSequence.ActionTypes ActionType;

	public bool AllowUserTrigger = true;

	public bool AllowWhileDead;

	public bool RefundInactivity = true;

	public bool CrateShare;

	public bool SingleInstance;

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropAllowUserTrigger = "allow_user_trigger";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropActionType = "action_type";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropAllowWhileDead = "allow_while_dead";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropTargetType = "target_type";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropRefundInactivity = "refund_inactivity";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropCategory = "category";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropSingleInstance = "single_instance";

	public Dictionary<string, List<Entity>> EntityGroups;

	public string[] CategoryNames;

	public List<BaseRequirement> Requirements = new List<BaseRequirement>();

	public List<BaseAction> Actions = new List<BaseAction>();

	public EntityPlayer Requester;

	public Entity Target;

	public Vector3 TargetPosition;

	public Vector3i POIPosition;

	public GameEventActionSequence.TargetTypes TargetType;

	public PrefabInstance POIInstance;

	public BlockValue blockValue;

	public int CurrentBossGroupID = -1;

	public bool IsComplete;

	public bool AllowRefunds = true;

	public bool TwitchActivated;

	public Dictionary<string, string> Variables = new Dictionary<string, string>();

	public GameEventVariables eventVariables;

	public DynamicProperties Properties;

	public float StartTime = -1f;

	public bool HasDespawn;

	public GameEventActionSequence OwnerSequence;

	public enum ActionTypes
	{
		TwitchAction,
		TwitchVote,
		Game
	}

	public enum TargetTypes
	{
		Entity,
		POI,
		Block
	}
}
