using System;
using UnityEngine.Scripting;

[Preserve]
public class ObjectiveAssemble : BaseObjective
{
	public override void SetupObjective()
	{
		this.keyword = Localization.Get("ObjectiveAssemble_keyword", false, null);
		this.expectedItem = ItemClass.GetItem(this.ID, false);
		this.expectedItemClass = ItemClass.GetItemClass(this.ID, false);
	}

	public override void SetupDisplay()
	{
		base.Description = string.Format(this.keyword, this.expectedItemClass.GetLocalizedItemName());
		this.StatusText = "";
	}

	public override void AddHooks()
	{
		QuestEventManager.Current.AssembleItem += this.Current_AssembleItem;
	}

	public override void RemoveHooks()
	{
		QuestEventManager.Current.AssembleItem -= this.Current_AssembleItem;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Current_AssembleItem(ItemStack stack)
	{
		if (stack.itemValue.type == this.expectedItem.type && base.OwnerQuest.CheckRequirements())
		{
			base.CurrentValue = 1;
			this.Refresh();
		}
	}

	public override void Refresh()
	{
		if (base.Complete)
		{
			return;
		}
		bool flag = base.CurrentValue == 1;
		base.Complete = flag;
		if (base.Complete)
		{
			base.OwnerQuest.RefreshQuestCompletion(QuestClass.CompletionTypes.AutoComplete, null, true, null);
		}
	}

	public override BaseObjective Clone()
	{
		ObjectiveAssemble objectiveAssemble = new ObjectiveAssemble();
		this.CopyValues(objectiveAssemble);
		return objectiveAssemble;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemValue expectedItem = ItemValue.None;

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemClass expectedItemClass;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool assembled;
}
