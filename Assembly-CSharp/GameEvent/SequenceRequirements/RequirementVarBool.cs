using System;
using UnityEngine.Scripting;

namespace GameEvent.SequenceRequirements
{
	[Preserve]
	public class RequirementVarBool : BaseRequirement
	{
		public override bool CanPerform(Entity target)
		{
			if (target is EntityAlive)
			{
				bool flag = false;
				this.Owner.EventVariables.ParseBool(this.varName, ref flag);
				if (flag == StringParsers.ParseBool(this.valueText, 0, -1, true))
				{
					return !this.Invert;
				}
			}
			return this.Invert;
		}

		public override void ParseProperties(DynamicProperties properties)
		{
			base.ParseProperties(properties);
			properties.ParseString(RequirementVarBool.PropVarName, ref this.varName);
			properties.ParseString(RequirementVarBool.PropValue, ref this.valueText);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override BaseRequirement CloneChildSettings()
		{
			return new RequirementVarBool
			{
				Invert = this.Invert,
				varName = this.varName,
				valueText = this.valueText
			};
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public string varName;

		[PublicizedFrom(EAccessModifier.Protected)]
		public string valueText;

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropVarName = "var_name";

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropValue = "value";
	}
}
