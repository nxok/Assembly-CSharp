using System;
using System.Xml.Linq;
using SandboxOptions;
using UnityEngine.Scripting;

[Preserve]
public class LootEntryRequirementSandboxOption : BaseOperationLootEntryRequirement
{
	public override void Init(XElement e)
	{
		base.Init(e);
		string text = "";
		e.ParseAttribute("option", ref text);
		this.option = Enum.Parse<global::SandboxOptions.SandboxOptions>(text);
		e.ParseAttribute("value", ref this.value);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override float LeftSide(EntityPlayer player)
	{
		if (player != null)
		{
			return SandboxOptionManager.GetFloat(this.option);
		}
		return 0f;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override float RightSide(EntityPlayer player)
	{
		return this.value;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public global::SandboxOptions.SandboxOptions option;

	[PublicizedFrom(EAccessModifier.Protected)]
	public float value;
}
