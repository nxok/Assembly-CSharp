using System;

public abstract class BaseDialogAction
{
	public string ID { get; set; }

	public string Value { get; set; }

	public Dialog OwnerDialog { get; set; }

	public DialogResponse Owner { get; set; }

	public virtual BaseDialogAction.ActionTypes ActionType
	{
		get
		{
			return BaseDialogAction.ActionTypes.AddBuff;
		}
	}

	public BaseDialogAction()
	{
		this.ID = "";
		this.Value = "";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void CopyValues(BaseDialogAction action)
	{
		action.ID = this.ID;
		action.Value = this.Value;
	}

	public virtual void SetupAction()
	{
	}

	public virtual void PerformAction(EntityPlayer player)
	{
	}

	public virtual BaseDialogAction Clone()
	{
		return null;
	}

	public enum ActionTypes
	{
		AddBuff,
		AddItem,
		AddQuest,
		CompleteQuest,
		Trader,
		Voice
	}
}
