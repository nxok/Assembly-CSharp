using System;
using System.Collections.Generic;

public abstract class BaseDialogRequirement
{
	public string ID { get; set; }

	public string Value { get; set; }

	public string Tag { get; set; }

	public Dialog Owner { get; set; }

	public BaseDialogRequirement.RequirementVisibilityTypes RequirementVisibilityType { get; set; }

	public string Description
	{
		get; [PublicizedFrom(EAccessModifier.Protected)]
		set;
	}

	public string StatusText
	{
		get; [PublicizedFrom(EAccessModifier.Protected)]
		set;
	}

	public virtual List<string> GetRequirementIDTypes()
	{
		return null;
	}

	public virtual BaseDialogRequirement.RequirementTypes RequirementType
	{
		get
		{
			return BaseDialogRequirement.RequirementTypes.Buff;
		}
	}

	public virtual string GetRequiredDescription(EntityPlayer player)
	{
		return "";
	}

	public virtual void SetupRequirement()
	{
	}

	public virtual bool CheckRequirement(EntityPlayer player, EntityNPC talkingTo)
	{
		return false;
	}

	public virtual BaseDialogRequirement Clone()
	{
		return null;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public BaseDialogRequirement()
	{
	}

	public enum RequirementTypes
	{
		Buff,
		QuestStatus,
		QuestsAvailable,
		QuestTier,
		QuestTierHighest,
		QuestEditorTag,
		Skill,
		Admin,
		DroneState,
		DroneStateExclude,
		CVar,
		CanTrade
	}

	public enum RequirementVisibilityTypes
	{
		AlternateText,
		Hide
	}
}
