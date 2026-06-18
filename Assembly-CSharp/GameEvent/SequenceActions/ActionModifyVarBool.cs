using System;
using UnityEngine.Scripting;

namespace GameEvent.SequenceActions
{
	[Preserve]
	public class ActionModifyVarBool : ActionBaseClientAction
	{
		public override BaseAction.ActionCompleteStates PerformTargetAction(Entity target)
		{
			base.Owner.EventVariables.SetEventVariable(this.varName, StringParsers.ParseBool(this.valueText, 0, -1, true));
			return BaseAction.ActionCompleteStates.Complete;
		}

		public override void ParseProperties(DynamicProperties properties)
		{
			base.ParseProperties(properties);
			properties.ParseString(ActionModifyVarBool.PropValue, ref this.valueText);
			properties.ParseString(ActionModifyVarBool.PropVarName, ref this.varName);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override BaseAction CloneChildSettings()
		{
			return new ActionModifyVarBool
			{
				varName = this.varName,
				valueText = this.valueText
			};
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public string valueText = "";

		[PublicizedFrom(EAccessModifier.Protected)]
		public string varName = "";

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropValue = "value";

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropVarName = "var_name";
	}
}
