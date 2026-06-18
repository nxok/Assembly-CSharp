using System;
using UnityEngine.Scripting;

namespace GameEvent.SequenceActions
{
	[Preserve]
	public class ActionModifyCVar : ActionBaseClientAction
	{
		public override void OnClientPerform(Entity target)
		{
			EntityAlive entityAlive = target as EntityAlive;
			if (entityAlive != null)
			{
				float floatValue = GameEventManager.GetFloatValue(entityAlive, this.valueText, 0f);
				switch (this.operationType)
				{
				case ActionModifyCVar.OperationTypes.Set:
					entityAlive.Buffs.SetCustomVar(this.cvar, floatValue, true, CVarOperation.set);
					return;
				case ActionModifyCVar.OperationTypes.Add:
					entityAlive.Buffs.SetCustomVar(this.cvar, entityAlive.Buffs.GetCustomVar(this.cvar) + floatValue, true, CVarOperation.set);
					return;
				case ActionModifyCVar.OperationTypes.Subtract:
					entityAlive.Buffs.SetCustomVar(this.cvar, entityAlive.Buffs.GetCustomVar(this.cvar) - floatValue, true, CVarOperation.set);
					return;
				case ActionModifyCVar.OperationTypes.Multiply:
					entityAlive.Buffs.SetCustomVar(this.cvar, entityAlive.Buffs.GetCustomVar(this.cvar) * floatValue, true, CVarOperation.set);
					return;
				case ActionModifyCVar.OperationTypes.PercentAdd:
					entityAlive.Buffs.SetCustomVar(this.cvar, entityAlive.Buffs.GetCustomVar(this.cvar) + entityAlive.Buffs.GetCustomVar(this.cvar) * floatValue, true, CVarOperation.set);
					return;
				case ActionModifyCVar.OperationTypes.PercentSubtract:
					entityAlive.Buffs.SetCustomVar(this.cvar, entityAlive.Buffs.GetCustomVar(this.cvar) - entityAlive.Buffs.GetCustomVar(this.cvar) * floatValue, true, CVarOperation.set);
					break;
				default:
					return;
				}
			}
		}

		public override void ParseProperties(DynamicProperties properties)
		{
			base.ParseProperties(properties);
			properties.ParseString(ActionModifyCVar.PropValue, ref this.valueText);
			properties.ParseString(ActionModifyCVar.PropCvar, ref this.cvar);
			properties.ParseEnum<ActionModifyCVar.OperationTypes>(ActionModifyCVar.PropOperation, ref this.operationType);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override BaseAction CloneChildSettings()
		{
			return new ActionModifyCVar
			{
				cvar = this.cvar,
				valueText = this.valueText,
				operationType = this.operationType
			};
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public string valueText;

		[PublicizedFrom(EAccessModifier.Protected)]
		public string cvar = "";

		[PublicizedFrom(EAccessModifier.Protected)]
		public ActionModifyCVar.OperationTypes operationType;

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropValue = "value";

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropCvar = "cvar";

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropOperation = "operation";

		[PublicizedFrom(EAccessModifier.Protected)]
		public enum OperationTypes
		{
			Set,
			Add,
			Subtract,
			Multiply,
			PercentAdd,
			PercentSubtract
		}
	}
}
