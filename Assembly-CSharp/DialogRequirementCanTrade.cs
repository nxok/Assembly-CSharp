using System;
using SandboxOptions;
using UnityEngine.Scripting;

[Preserve]
public class DialogRequirementCanTrade : BaseDialogRequirement
{
	public override BaseDialogRequirement.RequirementTypes RequirementType
	{
		get
		{
			return BaseDialogRequirement.RequirementTypes.CanTrade;
		}
	}

	public override bool CheckRequirement(EntityPlayer player, EntityNPC talkingTo)
	{
		return SandboxOptionManager.GetBool(global::SandboxOptions.SandboxOptions.TradersEnabled);
	}
}
