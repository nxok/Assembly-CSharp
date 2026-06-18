using System;
using System.Collections.Generic;
using Platform;
using Quests.Requirements;
using SandboxOptions;

public class QuestClass
{
	public string ID
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public string Name { get; set; }

	public string GroupName { get; set; }

	public string SubTitle { get; set; }

	public string Description { get; set; }

	public string Offer { get; set; }

	public string Difficulty { get; set; }

	public string Icon { get; set; }

	public bool Repeatable { get; set; }

	public bool Shareable { get; set; }

	public string Category { get; set; }

	public string StatementText { get; set; }

	public string ResponseText { get; set; }

	public string CompleteText { get; set; }

	public byte CurrentVersion { get; set; }

	public byte HighestPhase { get; set; }

	public byte QuestFaction { get; set; }

	public byte DifficultyTier { get; set; }

	public bool LoginRallyReset { get; set; }

	public bool ReturnToQuestGiver { get; set; }

	public string UniqueKey { get; set; }

	public string QuestType { get; set; }

	public bool AddsToTierComplete { get; set; }

	public static QuestClass NewClass(string id)
	{
		if (QuestClass.s_Quests.ContainsKey(id))
		{
			return null;
		}
		QuestClass questClass = new QuestClass(id.ToLower());
		QuestClass.s_Quests[id] = questClass;
		return questClass;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public QuestClass(string id)
	{
		this.ID = id;
		this.Difficulty = "veryeasy";
		this.HighestPhase = 1;
		this.AddsToTierComplete = true;
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public static QuestClass GetQuest(string questID)
	{
		if (!QuestClass.s_Quests.ContainsKey(questID))
		{
			return null;
		}
		return QuestClass.s_Quests[questID];
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public DynamicProperties AssignValuesFrom(QuestClass oldQuest)
	{
		DynamicProperties dynamicProperties = new DynamicProperties();
		HashSet<string> hashSet = new HashSet<string>();
		if (oldQuest.Properties != null)
		{
			dynamicProperties.CopyFrom(oldQuest.Properties, hashSet);
		}
		for (int i = 0; i < oldQuest.Requirements.Count; i++)
		{
			BaseRequirement baseRequirement = oldQuest.Requirements[i].Clone();
			baseRequirement.Properties = new DynamicProperties();
			if (oldQuest.Requirements[i].Properties != null)
			{
				baseRequirement.Properties.CopyFrom(oldQuest.Requirements[i].Properties, null);
			}
			baseRequirement.Owner = this;
			this.Requirements.Add(baseRequirement);
		}
		for (int j = 0; j < oldQuest.Actions.Count; j++)
		{
			BaseQuestAction baseQuestAction = oldQuest.Actions[j].Clone();
			baseQuestAction.Properties = new DynamicProperties();
			if (oldQuest.Actions[j].Properties != null)
			{
				baseQuestAction.Properties.CopyFrom(oldQuest.Actions[j].Properties, null);
			}
			baseQuestAction.Owner = this;
			this.Actions.Add(baseQuestAction);
		}
		for (int k = 0; k < oldQuest.Objectives.Count; k++)
		{
			BaseObjective baseObjective = oldQuest.Objectives[k].Clone();
			baseObjective.Properties = new DynamicProperties();
			if (oldQuest.Objectives[k].Properties != null)
			{
				baseObjective.Properties.CopyFrom(oldQuest.Objectives[k].Properties, null);
			}
			if (oldQuest.Objectives[k].Phase > this.HighestPhase)
			{
				this.HighestPhase = oldQuest.Objectives[k].Phase;
			}
			baseObjective.OwnerQuestClass = this;
			this.Objectives.Add(baseObjective);
		}
		for (int l = 0; l < oldQuest.Events.Count; l++)
		{
			QuestEvent questEvent = oldQuest.Events[l].Clone();
			questEvent.Properties = new DynamicProperties();
			if (oldQuest.Events[l].Properties != null)
			{
				questEvent.Properties.CopyFrom(oldQuest.Events[l].Properties, null);
			}
			questEvent.Owner = this;
			this.Events.Add(questEvent);
		}
		return dynamicProperties;
	}

	public static Quest CreateQuest(string ID)
	{
		QuestClass quest = QuestClass.GetQuest(ID);
		if (quest != null)
		{
			return quest.CreateQuest();
		}
		return null;
	}

	public Quest CreateQuest()
	{
		Quest quest = new Quest(this.ID);
		quest.CurrentQuestVersion = this.CurrentVersion;
		quest.Tracked = false;
		quest.FinishTime = 0UL;
		quest.QuestFaction = this.QuestFaction;
		if (!this.ExtraTags.IsEmpty)
		{
			quest.QuestTags |= this.ExtraTags;
		}
		for (int i = 0; i < this.Actions.Count; i++)
		{
			BaseQuestAction baseQuestAction = this.Actions[i].Clone();
			baseQuestAction.OwnerQuest = quest;
			quest.Actions.Add(baseQuestAction);
		}
		for (int j = 0; j < this.Requirements.Count; j++)
		{
			BaseRequirement baseRequirement = this.Requirements[j].Clone();
			baseRequirement.OwnerQuest = quest;
			quest.Requirements.Add(baseRequirement);
		}
		for (int k = 0; k < this.Objectives.Count; k++)
		{
			BaseObjective baseObjective = this.Objectives[k].Clone();
			baseObjective.OwnerQuest = quest;
			quest.Objectives.Add(baseObjective);
		}
		int num = 0;
		for (int l = 0; l < this.Rewards.Count; l++)
		{
			BaseReward baseReward = this.Rewards[l].Clone();
			baseReward.OwnerQuest = quest;
			quest.Rewards.Add(baseReward);
			if (!baseReward.isChainReward && baseReward.isChosenReward && !baseReward.isFixedLocation)
			{
				num++;
			}
		}
		return quest;
	}

	public void ResetObjectives()
	{
		for (int i = 0; i < this.Objectives.Count; i++)
		{
			this.Objectives[i].ResetObjective();
		}
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public BaseQuestAction AddAction(BaseQuestAction action)
	{
		if (action != null)
		{
			this.Actions.Add(action);
		}
		return action;
	}

	public QuestEvent AddEvent(QuestEvent questEvent)
	{
		if (questEvent != null)
		{
			this.Events.Add(questEvent);
		}
		return questEvent;
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public BaseRequirement AddRequirement(BaseRequirement requirement)
	{
		if (requirement != null)
		{
			this.Requirements.Add(requirement);
		}
		return requirement;
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public BaseObjective AddObjective(BaseObjective objective)
	{
		if (objective != null)
		{
			objective.OwnerQuestClass = this;
			this.Objectives.Add(objective);
		}
		return objective;
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public BaseReward AddReward(BaseReward reward)
	{
		if (reward != null)
		{
			this.Rewards.Add(reward);
		}
		return reward;
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public BaseQuestCriteria AddCriteria(BaseQuestCriteria criteria)
	{
		if (criteria != null)
		{
			this.Criteria.Add(criteria);
		}
		return criteria;
	}

	public bool CheckCriteriaQuestGiver(EntityNPC entityNPC)
	{
		for (int i = 0; i < this.Criteria.Count; i++)
		{
			if (this.Criteria[i].CriteriaType == BaseQuestCriteria.CriteriaTypes.QuestGiver && !this.Criteria[i].CheckForQuestGiver(entityNPC))
			{
				return false;
			}
		}
		return true;
	}

	public bool CheckCriteriaOffer(EntityPlayer player)
	{
		for (int i = 0; i < this.Criteria.Count; i++)
		{
			if (this.Criteria[i].CriteriaType == BaseQuestCriteria.CriteriaTypes.QuestGiver && !this.Criteria[i].CheckForPlayer(player))
			{
				return false;
			}
		}
		return true;
	}

	public bool CanActivate()
	{
		if (EntityFactory.EnemySpawnMode)
		{
			return true;
		}
		for (int i = 0; i < this.Objectives.Count; i++)
		{
			if (this.Objectives[i].RequiresZombies)
			{
				return false;
			}
		}
		return true;
	}

	public string GetCurrentHint(int phase)
	{
		if (this.QuestHintRequirement != global::SandboxOptions.SandboxOptions.Max && !SandboxOptionManager.GetBool(this.QuestHintRequirement))
		{
			return "";
		}
		phase--;
		if (this.QuestHints == null || phase >= this.QuestHints.Count)
		{
			return "";
		}
		string text = this.QuestHints[phase];
		if (PlatformManager.NativePlatform.Input.CurrentInputStyle != PlayerInputManager.InputStyle.Keyboard)
		{
			string text2 = text + "_alt";
			if (Localization.Exists(text2, false))
			{
				return Localization.Get(text2, false, null);
			}
		}
		return Localization.Get(text, false, null);
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public void Init()
	{
		if (this.Properties.Values.ContainsKey(QuestClass.PropCategory))
		{
			this.Category = this.Properties.Values[QuestClass.PropCategory];
		}
		if (this.Properties.Values.ContainsKey(QuestClass.PropCategoryKey))
		{
			this.Category = Localization.Get(this.Properties.Values[QuestClass.PropCategoryKey], false, null);
		}
		if (this.Properties.Values.ContainsKey(QuestClass.PropGroupName))
		{
			this.GroupName = this.Properties.Values[QuestClass.PropGroupName];
		}
		if (this.Properties.Values.ContainsKey(QuestClass.PropGroupNameKey))
		{
			this.GroupName = Localization.Get(this.Properties.Values[QuestClass.PropGroupNameKey], false, null);
		}
		if (this.Properties.Values.ContainsKey(QuestClass.PropName))
		{
			this.Name = this.Properties.Values[QuestClass.PropName];
		}
		if (this.Properties.Values.ContainsKey(QuestClass.PropNameKey))
		{
			this.Name = Localization.Get(this.Properties.Values[QuestClass.PropNameKey], false, null);
		}
		if (this.Properties.Values.ContainsKey(QuestClass.PropSubtitle))
		{
			this.SubTitle = this.Properties.Values[QuestClass.PropSubtitle];
		}
		if (this.Properties.Values.ContainsKey(QuestClass.PropSubtitleKey))
		{
			this.SubTitle = Localization.Get(this.Properties.Values[QuestClass.PropSubtitleKey], false, null);
		}
		if (this.Properties.Values.ContainsKey(QuestClass.PropDescription))
		{
			this.Description = this.Properties.Values[QuestClass.PropDescription];
		}
		if (this.Properties.Values.ContainsKey(QuestClass.PropDescriptionKey))
		{
			this.Description = Localization.Get(this.Properties.Values[QuestClass.PropDescriptionKey], false, null);
		}
		if (this.Properties.Values.ContainsKey(QuestClass.PropOffer))
		{
			this.Offer = this.Properties.Values[QuestClass.PropOffer];
		}
		if (this.Properties.Values.ContainsKey(QuestClass.PropOfferKey))
		{
			this.Offer = Localization.Get(this.Properties.Values[QuestClass.PropOfferKey], false, null);
		}
		if (this.Properties.Values.ContainsKey(QuestClass.PropIcon))
		{
			this.Icon = this.Properties.Values[QuestClass.PropIcon];
		}
		if (this.Properties.Values.ContainsKey(QuestClass.PropRepeatable))
		{
			bool flag;
			StringParsers.TryParseBool(this.Properties.Values[QuestClass.PropRepeatable], out flag);
			this.Repeatable = flag;
		}
		if (this.Properties.Values.ContainsKey(QuestClass.PropShareable))
		{
			bool flag2;
			StringParsers.TryParseBool(this.Properties.Values[QuestClass.PropShareable], out flag2);
			this.Shareable = flag2;
		}
		if (this.Properties.Values.ContainsKey(QuestClass.PropDifficulty))
		{
			this.Difficulty = Localization.Get(string.Format("difficulty_{0}", this.Properties.Values[QuestClass.PropDifficulty]), false, null);
		}
		if (this.Properties.Values.ContainsKey(QuestClass.PropCompletionType))
		{
			this.CompletionType = EnumUtils.Parse<QuestClass.CompletionTypes>(this.Properties.Values[QuestClass.PropCompletionType], false);
		}
		if (this.Properties.Values.ContainsKey(QuestClass.PropCurrentVersion))
		{
			this.CurrentVersion = Convert.ToByte(this.Properties.Values[QuestClass.PropCurrentVersion]);
		}
		if (this.Properties.Values.ContainsKey(QuestClass.PropStatementText))
		{
			this.StatementText = this.Properties.Values[QuestClass.PropStatementText];
		}
		if (this.Properties.Values.ContainsKey(QuestClass.PropResponseText))
		{
			this.ResponseText = this.Properties.Values[QuestClass.PropResponseText];
		}
		if (this.Properties.Values.ContainsKey(QuestClass.PropCompleteText))
		{
			this.CompleteText = this.Properties.Values[QuestClass.PropCompleteText];
		}
		if (this.Properties.Values.ContainsKey(QuestClass.PropStatementKey))
		{
			this.StatementText = Localization.Get(this.Properties.Values[QuestClass.PropStatementKey], false, null);
		}
		if (this.Properties.Values.ContainsKey(QuestClass.PropResponseKey))
		{
			this.ResponseText = Localization.Get(this.Properties.Values[QuestClass.PropResponseKey], false, null);
		}
		if (this.Properties.Values.ContainsKey(QuestClass.PropCompleteKey))
		{
			this.CompleteText = Localization.Get(this.Properties.Values[QuestClass.PropCompleteKey], false, null);
		}
		if (this.Properties.Values.ContainsKey(QuestClass.PropQuestFaction))
		{
			this.QuestFaction = Convert.ToByte(this.Properties.Values[QuestClass.PropQuestFaction]);
		}
		if (this.Properties.Values.ContainsKey(QuestClass.PropDifficultyTier))
		{
			this.DifficultyTier = Convert.ToByte(this.Properties.Values[QuestClass.PropDifficultyTier]);
		}
		if (this.Properties.Values.ContainsKey(QuestClass.PropLoginRallyReset))
		{
			this.LoginRallyReset = Convert.ToBoolean(this.Properties.Values[QuestClass.PropLoginRallyReset]);
		}
		if (this.Properties.Values.ContainsKey(QuestClass.PropUniqueKey))
		{
			this.UniqueKey = this.Properties.Values[QuestClass.PropUniqueKey];
		}
		else
		{
			this.UniqueKey = "";
		}
		if (this.Properties.Values.ContainsKey(QuestClass.PropReturnToQuestGiver))
		{
			this.ReturnToQuestGiver = StringParsers.ParseBool(this.Properties.Values[QuestClass.PropReturnToQuestGiver], 0, -1, true);
		}
		else
		{
			this.ReturnToQuestGiver = true;
		}
		if (this.Properties.Values.ContainsKey(QuestClass.PropQuestType))
		{
			this.QuestType = this.Properties.Values[QuestClass.PropQuestType];
		}
		else
		{
			this.QuestType = "";
		}
		if (this.Properties.Values.ContainsKey(QuestClass.PropAddsToTierComplete))
		{
			this.AddsToTierComplete = StringParsers.ParseBool(this.Properties.Values[QuestClass.PropAddsToTierComplete], 0, -1, true);
		}
		this.Properties.ParseInt(QuestClass.PropRewardChoicesCount, ref this.RewardChoicesCount);
		if (this.Properties.Values.ContainsKey(QuestClass.PropExtraTags))
		{
			string text = this.Properties.Values[QuestClass.PropExtraTags];
			if (text != "")
			{
				this.ExtraTags = FastTags<TagGroup.Global>.Parse(text);
			}
		}
		if (this.Properties.Values.ContainsKey(QuestClass.PropFilterTags))
		{
			string text2 = this.Properties.Values[QuestClass.PropFilterTags];
			if (text2 != "")
			{
				this.FilterTags = FastTags<TagGroup.Global>.Parse(text2);
			}
		}
		string text3 = "";
		this.Properties.ParseString(QuestClass.PropQuestHints, ref text3);
		if (text3 != "")
		{
			if (this.QuestHints == null)
			{
				this.QuestHints = new List<string>();
			}
			this.QuestHints.AddRange(text3.Split(',', StringSplitOptions.None));
		}
		string text4 = null;
		this.Properties.ParseString(QuestClass.PropQuestHintRequirement, ref text4);
		if (text4 != null)
		{
			Enum.TryParse<global::SandboxOptions.SandboxOptions>(text4, out this.QuestHintRequirement);
		}
		this.Properties.ParseFloat(QuestClass.PropQuestGameStageMod, ref this.GameStageMod);
		this.Properties.ParseFloat(QuestClass.PropQuestGameStageBonus, ref this.GameStageBonus);
		this.Properties.ParseFloat(QuestClass.PropSpawnMultiplier, ref this.SpawnMultiplier);
		this.Properties.ParseBool(QuestClass.PropResetTraderQuests, ref this.ResetTraderQuests);
		this.Properties.ParseBool(QuestClass.PropSingleQuest, ref this.SingleQuest);
		this.Properties.ParseBool(QuestClass.PropAlwaysAllow, ref this.AlwaysAllow);
		this.Properties.ParseBool(QuestClass.PropAllowRemove, ref this.AllowRemove);
		if (this.Properties.Values.ContainsKey(QuestClass.PropMaxQuestCount))
		{
			this.Properties.ParseInt(QuestClass.PropMaxQuestCount, ref this.MaxQuestCount);
		}
	}

	public void HandleVariablesForProperties(DynamicProperties properties)
	{
		foreach (KeyValuePair<string, string> keyValuePair in properties.Params1)
		{
			if (this.Variables.ContainsKey(keyValuePair.Value))
			{
				properties.Values[keyValuePair.Key] = this.Variables[keyValuePair.Value];
			}
		}
	}

	public void HandleTemplateInit()
	{
		for (int i = 0; i < this.Actions.Count; i++)
		{
			this.HandleVariablesForProperties(this.Actions[i].Properties);
			this.Actions[i].ParseProperties(this.Actions[i].Properties);
		}
		for (int j = 0; j < this.Events.Count; j++)
		{
			this.HandleVariablesForProperties(this.Events[j].Properties);
			this.Events[j].ParseProperties(this.Events[j].Properties);
			for (int k = 0; k < this.Events[j].Actions.Count; k++)
			{
				BaseQuestAction baseQuestAction = this.Events[j].Actions[k];
				this.HandleVariablesForProperties(baseQuestAction.Properties);
				baseQuestAction.ParseProperties(baseQuestAction.Properties);
			}
		}
		for (int l = 0; l < this.Objectives.Count; l++)
		{
			BaseObjective baseObjective = this.Objectives[l];
			this.HandleVariablesForProperties(baseObjective.Properties);
			baseObjective.ParseProperties(baseObjective.Properties);
			if (baseObjective.Modifiers != null)
			{
				for (int m = 0; m < baseObjective.Modifiers.Count; m++)
				{
					BaseObjectiveModifier baseObjectiveModifier = baseObjective.Modifiers[m];
					this.HandleVariablesForProperties(baseObjectiveModifier.Properties);
					baseObjectiveModifier.ParseProperties(baseObjectiveModifier.Properties);
				}
			}
		}
	}

	public static Dictionary<string, QuestClass> s_Quests = new CaseInsensitiveStringDictionary<QuestClass>();

	public static string PropCategory = "category";

	public static string PropCategoryKey = "category_key";

	public static string PropGroupName = "group_name";

	public static string PropGroupNameKey = "group_name_key";

	public static string PropName = "name";

	public static string PropNameKey = "name_key";

	public static string PropSubtitle = "subtitle";

	public static string PropSubtitleKey = "subtitle_key";

	public static string PropDescription = "description";

	public static string PropDescriptionKey = "description_key";

	public static string PropOffer = "offer";

	public static string PropOfferKey = "offer_key";

	public static string PropIcon = "icon";

	public static string PropRepeatable = "repeatable";

	public static string PropShareable = "shareable";

	public static string PropDifficulty = "difficulty";

	public static string PropCompletionType = "completiontype";

	public static string PropCurrentVersion = "currentversion";

	public static string PropStatementText = "statement_text";

	public static string PropResponseText = "response_text";

	public static string PropCompleteText = "completion_text";

	public static string PropStatementKey = "statement_key";

	public static string PropResponseKey = "response_key";

	public static string PropCompleteKey = "completion_key";

	public static string PropVariations = "variations";

	public static string PropQuestFaction = "quest_faction";

	public static string PropDifficultyTier = "difficulty_tier";

	public static string PropLoginRallyReset = "login_rally_reset";

	public static string PropUniqueKey = "unique_key";

	public static string PropReturnToQuestGiver = "return_to_quest_giver";

	public static string PropQuestType = "quest_type";

	public static string PropAddsToTierComplete = "add_to_tier_complete";

	public static string PropRewardChoicesCount = "reward_choices_count";

	public static string PropExtraTags = "extra_tags";

	public static string PropFilterTags = "filter_tags";

	public static string PropQuestStage = "quest_stage";

	public static string PropQuestHints = "quest_hints";

	public static string PropQuestHintRequirement = "quest_hint_requirement";

	public static string PropQuestGameStageMod = "gamestage_mod";

	public static string PropQuestGameStageBonus = "gamestage_bonus";

	public static string PropSpawnMultiplier = "spawn_multiplier";

	public static string PropResetTraderQuests = "reset_trader_quests";

	public static string PropSingleQuest = "single_quest";

	public static string PropAlwaysAllow = "always_allow";

	public static string PropAllowRemove = "allow_remove";

	public static string PropMaxQuestCount = "max_quest_count";

	public List<string> QuestHints;

	public global::SandboxOptions.SandboxOptions QuestHintRequirement = global::SandboxOptions.SandboxOptions.Max;

	public int RewardChoicesCount = 2;

	public FastTags<TagGroup.Global> ExtraTags = FastTags<TagGroup.Global>.none;

	public FastTags<TagGroup.Global> FilterTags = FastTags<TagGroup.Global>.none;

	public float GameStageMod;

	public float GameStageBonus;

	public float SpawnMultiplier = 1f;

	public bool ResetTraderQuests;

	public bool SingleQuest;

	public bool AlwaysAllow;

	public bool AllowRemove = true;

	public int MaxQuestCount;

	public QuestClass.CompletionTypes CompletionType;

	public List<BaseQuestCriteria> Criteria = new List<BaseQuestCriteria>();

	public List<BaseQuestAction> Actions = new List<BaseQuestAction>();

	public List<QuestEvent> Events = new List<QuestEvent>();

	public List<BaseRequirement> Requirements = new List<BaseRequirement>();

	public List<BaseObjective> Objectives = new List<BaseObjective>();

	public List<BaseReward> Rewards = new List<BaseReward>();

	public Dictionary<string, string> Variables = new Dictionary<string, string>();

	public DynamicProperties Properties = new DynamicProperties();

	public enum CompletionTypes
	{
		AutoComplete,
		TurnIn
	}
}
