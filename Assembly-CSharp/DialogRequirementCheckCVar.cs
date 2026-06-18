using System;
using System.Globalization;
using UnityEngine.Scripting;

[Preserve]
public class DialogRequirementCheckCVar : BaseDialogRequirement
{
	public override BaseDialogRequirement.RequirementTypes RequirementType
	{
		get
		{
			return BaseDialogRequirement.RequirementTypes.CVar;
		}
	}

	public override bool CheckRequirement(EntityPlayer player, EntityNPC talkingTo)
	{
		int num = (int)player.GetCVar(base.ID);
		LocalPlayerUI.GetUIForPlayer(player as EntityPlayerLocal);
		return num == StringParsers.ParseSInt32(base.Value, 0, -1, NumberStyles.Integer);
	}
}
