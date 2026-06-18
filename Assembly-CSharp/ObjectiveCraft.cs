using System;
using UnityEngine.Scripting;

[Preserve]
public class ObjectiveCraft : BaseObjective
{
	public override BaseObjective.ObjectiveValueTypes ObjectiveValueType
	{
		get
		{
			return BaseObjective.ObjectiveValueTypes.Number;
		}
	}

	public override void SetupQuestTag()
	{
		base.OwnerQuest.AddQuestTag(QuestEventManager.craftingTag);
	}

	public override void SetupObjective()
	{
		this.keyword = Localization.Get("ObjectiveCraft_keyword", false, null);
		this.expectedItem = ItemClass.GetItem(this.ID, false);
		this.expectedItemClass = ItemClass.GetItemClass(this.ID, false);
		this.itemCount = Convert.ToInt32(this.Value);
	}

	public override void SetupDisplay()
	{
		string text = ((this.ID != "" && this.ID != null) ? Localization.Get(this.ID, false, null) : "Any Item");
		base.Description = string.Format(this.keyword, text);
		this.StatusText = string.Format("{0}/{1}", base.CurrentValue, this.itemCount);
	}

	public override void AddHooks()
	{
		QuestEventManager.Current.CraftItem -= this.Current_CraftItem;
		QuestEventManager.Current.CraftItem += this.Current_CraftItem;
	}

	public override void RemoveHooks()
	{
		QuestEventManager.Current.CraftItem -= this.Current_CraftItem;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Current_CraftItem(ItemStack stack)
	{
		if (base.Complete)
		{
			return;
		}
		if (stack.itemValue.type == this.expectedItem.type && base.OwnerQuest.CheckRequirements())
		{
			base.CurrentValue += (byte)stack.count;
			this.Refresh();
		}
	}

	public override void Refresh()
	{
		if (base.Complete)
		{
			return;
		}
		base.Complete = (int)base.CurrentValue >= this.itemCount;
		if (base.Complete)
		{
			base.OwnerQuest.RefreshQuestCompletion(QuestClass.CompletionTypes.AutoComplete, null, true, null);
		}
	}

	public override BaseObjective Clone()
	{
		ObjectiveCraft objectiveCraft = new ObjectiveCraft();
		this.CopyValues(objectiveCraft);
		return objectiveCraft;
	}

	public override void ParseProperties(DynamicProperties properties)
	{
		base.ParseProperties(properties);
		if (properties.Values.ContainsKey(ObjectiveCraft.PropItem))
		{
			this.ID = properties.Values[ObjectiveCraft.PropItem];
		}
		if (properties.Values.ContainsKey(ObjectiveCraft.PropCount))
		{
			this.Value = properties.Values[ObjectiveCraft.PropCount];
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemValue expectedItem = ItemValue.None;

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemClass expectedItemClass;

	[PublicizedFrom(EAccessModifier.Private)]
	public int itemCount;

	public static string PropItem = "item";

	public static string PropCount = "count";
}
