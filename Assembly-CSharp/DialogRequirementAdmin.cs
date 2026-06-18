using System;
using UnityEngine.Scripting;

[Preserve]
public class DialogRequirementAdmin : BaseDialogRequirement
{
	public override BaseDialogRequirement.RequirementTypes RequirementType
	{
		get
		{
			return BaseDialogRequirement.RequirementTypes.Admin;
		}
	}

	public override void SetupRequirement()
	{
		string text = Localization.Get("RequirementAdmin_keyword", false, null);
		base.Description = text;
	}

	public override bool CheckRequirement(EntityPlayer player, EntityNPC talkingTo)
	{
		return GamePrefs.GetBool(EnumGamePrefs.DebugMenuEnabled);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string name = "";
}
