using System;
using SandboxOptions;
using UnityEngine.Scripting;

namespace Twitch
{
	[Preserve]
	public class TwitchRequirementSandboxBool : BaseTwitchOperationRequirement
	{
		public override bool CanPerform(Entity target)
		{
			if (SandboxOptionManager.GetBool(this.sandboxOption))
			{
				return !this.Invert;
			}
			return this.Invert;
		}

		public override void ParseProperties(DynamicProperties properties)
		{
			base.ParseProperties(properties);
			string text = "";
			properties.ParseString(TwitchRequirementSandboxBool.PropSandbox, ref text);
			this.sandboxOption = Enum.Parse<global::SandboxOptions.SandboxOptions>(text);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public global::SandboxOptions.SandboxOptions sandboxOption;

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropSandbox = "sandbox_option";
	}
}
