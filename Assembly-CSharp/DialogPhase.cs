using System;
using System.Collections.Generic;

public class DialogPhase : BaseDialogItem
{
	public string StartStatementID { get; set; }

	public string StartResponseID { get; set; }

	public DialogPhase(string newID)
	{
		this.ID = newID;
		this.HeaderName = string.Format("Phase : {0}", newID);
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public void AddRequirement(BaseDialogRequirement requirement)
	{
		this.RequirementList.Add(requirement);
	}

	public override string ToString()
	{
		return this.HeaderName;
	}

	public List<BaseDialogRequirement> RequirementList = new List<BaseDialogRequirement>();
}
