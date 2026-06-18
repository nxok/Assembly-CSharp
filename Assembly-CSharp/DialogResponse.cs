using System;
using System.Collections.Generic;

public class DialogResponse : BaseStatement
{
	public DialogResponse(string newID)
	{
		this.ID = newID;
		this.HeaderName = string.Format("Response : {0}", newID);
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public static DialogResponse NextStatementEntry(string nextStatementID)
	{
		DialogResponse.nextStatementEntry.NextStatementID = nextStatementID;
		DialogResponse.nextStatementEntry.Text = "[" + Localization.Get("xuiNext", false, null) + "]";
		return DialogResponse.nextStatementEntry;
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public void AddRequirement(BaseDialogRequirement requirement)
	{
		this.RequirementList.Add(requirement);
	}

	public string GetRequiredDescription(EntityPlayer player)
	{
		if (this.RequirementList.Count == 0)
		{
			return "";
		}
		return this.RequirementList[0].GetRequiredDescription(player);
	}

	public List<BaseDialogRequirement> RequirementList = new List<BaseDialogRequirement>();

	public string ReturnStatementID = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public static DialogResponse nextStatementEntry = new DialogResponse("__nextStatementEntry");
}
