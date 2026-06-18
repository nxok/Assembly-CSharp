using System;
using UnityEngine.Scripting;

[Preserve]
public class ObjectiveEntityKill : BaseObjective
{
	public override BaseObjective.ObjectiveValueTypes ObjectiveValueType
	{
		get
		{
			return BaseObjective.ObjectiveValueTypes.Number;
		}
	}

	public override bool RequiresZombies
	{
		get
		{
			return true;
		}
	}

	public override void SetupObjective()
	{
		this.neededKillCount = Convert.ToInt32(this.Value);
		if (this.ID != null)
		{
			string[] array = this.ID.Split(',', StringSplitOptions.None);
			if (array.Length > 1)
			{
				this.ID = array[0];
				this.entityNames = new string[array.Length - 1];
				for (int i = 1; i < array.Length; i++)
				{
					this.entityNames[i - 1] = array[i];
				}
			}
		}
	}

	public override void SetupDisplay()
	{
		this.keyword = Localization.Get("ObjectiveZombieKill_keyword", false, null);
		if (this.localizedName == "")
		{
			this.localizedName = ((this.ID != null && this.ID != "") ? Localization.Get(this.ID, false, null) : "Any Zombie");
		}
		base.Description = string.Format(this.keyword, this.localizedName);
		this.StatusText = string.Format("{0}/{1}", base.CurrentValue, this.neededKillCount);
	}

	public override void AddHooks()
	{
		QuestEventManager.Current.EntityKill += this.Current_EntityKill;
	}

	public override void RemoveHooks()
	{
		QuestEventManager.Current.EntityKill -= this.Current_EntityKill;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Current_EntityKill(EntityAlive killedBy, EntityAlive killedEntity)
	{
		if (base.Complete)
		{
			return;
		}
		bool flag = false;
		string entityClassName = killedEntity.EntityClass.entityClassName;
		if (this.targetTags.IsEmpty)
		{
			if (this.ID == null || entityClassName.EqualsCaseInsensitive(this.ID))
			{
				flag = true;
			}
			if (!flag && this.entityNames != null)
			{
				for (int i = 0; i < this.entityNames.Length; i++)
				{
					if (this.entityNames[i].EqualsCaseInsensitive(entityClassName))
					{
						flag = true;
						break;
					}
				}
			}
		}
		else
		{
			flag = killedEntity.EntityClass.Tags.Test_AnySet(this.targetTags);
		}
		if (flag && base.OwnerQuest.CheckRequirements())
		{
			byte currentValue = base.CurrentValue;
			base.CurrentValue = currentValue + 1;
			this.Refresh();
		}
	}

	public override void Refresh()
	{
		if (base.Complete)
		{
			return;
		}
		base.Complete = (int)base.CurrentValue >= this.neededKillCount;
		if (base.Complete)
		{
			base.OwnerQuest.RefreshQuestCompletion(QuestClass.CompletionTypes.AutoComplete, null, true, null);
		}
	}

	public override BaseObjective Clone()
	{
		ObjectiveEntityKill objectiveEntityKill = new ObjectiveEntityKill();
		this.CopyValues(objectiveEntityKill);
		objectiveEntityKill.localizedName = this.localizedName;
		objectiveEntityKill.targetTags = this.targetTags;
		return objectiveEntityKill;
	}

	public override void ParseProperties(DynamicProperties properties)
	{
		base.ParseProperties(properties);
		if (properties.Values.ContainsKey(ObjectiveEntityKill.PropObjectiveKey))
		{
			this.localizedName = Localization.Get(properties.Values[ObjectiveEntityKill.PropObjectiveKey], false, null);
		}
		properties.ParseString(ObjectiveEntityKill.PropEntityNames, ref this.ID);
		properties.ParseString(ObjectiveEntityKill.PropNeededCount, ref this.Value);
		string text = "";
		properties.ParseString(ObjectiveEntityKill.PropTargetTags, ref text);
		this.targetTags = FastTags<TagGroup.Global>.Parse(text);
	}

	public override string ParseBinding(string bindingName)
	{
		string id = this.ID;
		string value = this.Value;
		if (this.localizedName == "")
		{
			this.localizedName = ((id != null && id != "") ? Localization.Get(id, false, null) : "Any Zombie");
		}
		if (bindingName == "target")
		{
			return this.localizedName;
		}
		if (!(bindingName == "targetwithcount"))
		{
			return "";
		}
		return Convert.ToInt32(value).ToString() + " " + this.localizedName;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string localizedName = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public int neededKillCount;

	[PublicizedFrom(EAccessModifier.Private)]
	public string[] entityNames;

	[PublicizedFrom(EAccessModifier.Private)]
	public FastTags<TagGroup.Global> targetTags = FastTags<TagGroup.Global>.none;

	public static string PropObjectiveKey = "objective_name_key";

	public static string PropNeededCount = "needed_count";

	public static string PropEntityNames = "entity_names";

	public static string PropTargetTags = "target_tags";
}
