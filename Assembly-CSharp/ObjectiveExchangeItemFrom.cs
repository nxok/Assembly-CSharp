using System;
using UnityEngine.Scripting;

[Preserve]
public class ObjectiveExchangeItemFrom : BaseObjective
{
	public override BaseObjective.ObjectiveValueTypes ObjectiveValueType
	{
		get
		{
			return BaseObjective.ObjectiveValueTypes.Number;
		}
	}

	public override void SetupObjective()
	{
		this.keyword = Localization.Get("ObjectiveExchangeItemFrom_keyword", false, null);
		this.expectedItem = ItemClass.GetItem(this.ID, false);
		this.expectedItemClass = ItemClass.GetItemClass(this.ID, false);
		this.exchangeCount = Convert.ToInt32(this.Value);
	}

	public override void SetupDisplay()
	{
		base.Description = string.Format(this.keyword, this.expectedItemClass.GetLocalizedItemName());
		this.StatusText = string.Format("{0}/{1}", base.CurrentValue, this.exchangeCount);
	}

	public override void AddHooks()
	{
		QuestEventManager.Current.ExchangeFromItem += this.Current_ExchangeItem;
	}

	public override void RemoveHooks()
	{
		QuestEventManager.Current.ExchangeFromItem -= this.Current_ExchangeItem;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Current_ExchangeItem(ItemStack itemStack)
	{
		if (base.Complete)
		{
			return;
		}
		if (itemStack.itemValue.type == this.expectedItem.type && base.OwnerQuest.CheckRequirements())
		{
			base.CurrentValue += (byte)itemStack.count;
			this.Refresh();
		}
	}

	public override void Refresh()
	{
		if (base.Complete)
		{
			return;
		}
		base.Complete = (int)base.CurrentValue >= this.exchangeCount;
		if (base.Complete)
		{
			base.OwnerQuest.RefreshQuestCompletion(QuestClass.CompletionTypes.AutoComplete, null, true, null);
		}
	}

	public override BaseObjective Clone()
	{
		ObjectiveExchangeItemFrom objectiveExchangeItemFrom = new ObjectiveExchangeItemFrom();
		this.CopyValues(objectiveExchangeItemFrom);
		return objectiveExchangeItemFrom;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemValue expectedItem = ItemValue.None;

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemClass expectedItemClass;

	[PublicizedFrom(EAccessModifier.Private)]
	public int exchangeCount;
}
