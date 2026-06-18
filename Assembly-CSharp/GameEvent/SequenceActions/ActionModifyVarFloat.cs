using System;
using UnityEngine.Scripting;

namespace GameEvent.SequenceActions
{
	[Preserve]
	public class ActionModifyVarFloat : ActionBaseClientAction
	{
		public override BaseAction.ActionCompleteStates PerformTargetAction(Entity target)
		{
			float floatValue = GameEventManager.GetFloatValue(target as EntityAlive, this.valueText, 0f);
			base.Owner.EventVariables.ModifyEventVariable(this.varName, this.operationType, floatValue, float.MinValue, float.MaxValue);
			return BaseAction.ActionCompleteStates.Complete;
		}

		public override void ParseProperties(DynamicProperties properties)
		{
			base.ParseProperties(properties);
			properties.ParseString(ActionModifyVarFloat.PropValue, ref this.valueText);
			properties.ParseString(ActionModifyVarFloat.PropVarName, ref this.varName);
			properties.ParseEnum<GameEventVariables.OperationTypes>(ActionModifyVarFloat.PropOperation, ref this.operationType);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override BaseAction CloneChildSettings()
		{
			return new ActionModifyVarFloat
			{
				varName = this.varName,
				valueText = this.valueText,
				operationType = this.operationType
			};
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public string valueText = "";

		[PublicizedFrom(EAccessModifier.Protected)]
		public string varName = "";

		[PublicizedFrom(EAccessModifier.Protected)]
		public GameEventVariables.OperationTypes operationType;

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropValue = "value";

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropVarName = "var_name";

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropOperation = "operation";
	}
}
