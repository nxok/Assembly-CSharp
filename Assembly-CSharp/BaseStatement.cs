using System;
using System.Collections.Generic;

public class BaseStatement : BaseDialogItem
{
	public string NextStatementID
	{
		get; [PublicizedFrom(EAccessModifier.Internal)]
		set;
	}

	public override string ToString()
	{
		return this.Text;
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public void AddAction(BaseDialogAction action)
	{
		this.Actions.Add(action);
	}

	public string Text;

	public List<BaseDialogAction> Actions = new List<BaseDialogAction>();
}
