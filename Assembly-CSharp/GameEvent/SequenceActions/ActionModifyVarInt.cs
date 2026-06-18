using System;
using UnityEngine.Scripting;

namespace GameEvent.SequenceActions
{
	[Preserve]
	public class ActionModifyVarInt : ActionBaseClientAction
	{
		public override BaseAction.ActionCompleteStates PerformTargetAction(Entity target)
		{
			int intValue = GameEventManager.GetIntValue(target as EntityAlive, this.valueText, 0);
			base.Owner.EventVariables.ModifyEventVariable(this.varName, this.operationType, intValue, this.minValue, this.maxValue);
			return BaseAction.ActionCompleteStates.Complete;
		}

		public override void ParseProperties(DynamicProperties properties)
		{
			base.ParseProperties(properties);
			properties.ParseString(ActionModifyVarInt.PropValue, ref this.valueText);
			properties.ParseString(ActionModifyVarInt.PropVarName, ref this.varName);
			properties.ParseEnum<GameEventVariables.OperationTypes>(ActionModifyVarInt.PropOperation, ref this.operationType);
			properties.ParseInt(ActionModifyVarInt.PropMinValue, ref this.minValue);
			properties.ParseInt(ActionModifyVarInt.PropMaxValue, ref this.maxValue);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override BaseAction CloneChildSettings()
		{
			return new ActionModifyVarInt
			{
				varName = this.varName,
				valueText = this.valueText,
				operationType = this.operationType,
				minValue = this.minValue,
				maxValue = this.maxValue
			};
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public string valueText = "";

		[PublicizedFrom(EAccessModifier.Protected)]
		public string varName = "";

		[PublicizedFrom(EAccessModifier.Protected)]
		public GameEventVariables.OperationTypes operationType;

		[PublicizedFrom(EAccessModifier.Protected)]
		public int minValue = int.MinValue;

		[PublicizedFrom(EAccessModifier.Protected)]
		public int maxValue = int.MaxValue;

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropValue = "value";

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropVarName = "var_name";

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropOperation = "operation";

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropMinValue = "min_value";

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropMaxValue = "min_value";
	}
}
