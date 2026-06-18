using System;
using SandboxOptions;
using UnityEngine.Scripting;

namespace Twitch
{
	[Preserve]
	public class TwitchRequirementSandboxInt : BaseTwitchOperationRequirement
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		public override object LeftSide(Entity target)
		{
			return SandboxOptionManager.GetFloat(this.sandboxOption);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override object RightSide(Entity target)
		{
			return this.value;
		}

		public override void ParseProperties(DynamicProperties properties)
		{
			base.ParseProperties(properties);
			string text = "";
			properties.ParseString(TwitchRequirementSandboxInt.PropSandbox, ref text);
			this.sandboxOption = Enum.Parse<global::SandboxOptions.SandboxOptions>(text);
			properties.ParseInt(TwitchRequirementSandboxInt.PropValue, ref this.value);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public global::SandboxOptions.SandboxOptions sandboxOption;

		[PublicizedFrom(EAccessModifier.Protected)]
		public int value;

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropSandbox = "sandbox_option";

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropValue = "value";
	}
}
