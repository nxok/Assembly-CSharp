using System;
using SandboxOptions;
using UnityEngine.Scripting;

namespace GameEvent.SequenceRequirements
{
	[Preserve]
	public class RequirementSandboxInt : BaseOperationRequirement
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		public override void OnInit()
		{
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override object LeftSide(Entity target)
		{
			return SandboxOptionManager.GetInt(this.Option);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override object RightSide(Entity target)
		{
			return GameEventManager.GetIntValue(target as EntityAlive, this.valueText, 0);
		}

		public override void ParseProperties(DynamicProperties properties)
		{
			base.ParseProperties(properties);
			properties.ParseEnum<global::SandboxOptions.SandboxOptions>(RequirementSandboxInt.PropSandboxOption, ref this.Option);
			properties.ParseString(RequirementSandboxInt.PropValue, ref this.valueText);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override BaseRequirement CloneChildSettings()
		{
			return new RequirementSandboxInt
			{
				Invert = this.Invert,
				operation = this.operation,
				Option = this.Option,
				valueText = this.valueText
			};
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public global::SandboxOptions.SandboxOptions Option = global::SandboxOptions.SandboxOptions.Max;

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropSandboxOption = "option";

		[PublicizedFrom(EAccessModifier.Protected)]
		public string valueText;

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropValue = "value";
	}
}
