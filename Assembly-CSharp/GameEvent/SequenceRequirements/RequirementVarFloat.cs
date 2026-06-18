using System;
using UnityEngine.Scripting;

namespace GameEvent.SequenceRequirements
{
	[Preserve]
	public class RequirementVarFloat : BaseOperationRequirement
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		public override void OnInit()
		{
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override object LeftSide(Entity target)
		{
			int num = 0;
			this.Owner.EventVariables.ParseVarInt(this.varName, ref num);
			return num;
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override object RightSide(Entity target)
		{
			return GameEventManager.GetFloatValue(target as EntityAlive, this.valueText, 0f);
		}

		public override void ParseProperties(DynamicProperties properties)
		{
			base.ParseProperties(properties);
			properties.ParseString(RequirementVarFloat.PropVarName, ref this.varName);
			properties.ParseString(RequirementVarFloat.PropValue, ref this.valueText);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override BaseRequirement CloneChildSettings()
		{
			return new RequirementVarFloat
			{
				Invert = this.Invert,
				operation = this.operation,
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
