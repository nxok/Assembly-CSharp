using System;
using UnityEngine.Scripting;

namespace GameEvent.SequenceRequirements
{
	[Preserve]
	public class RequirementCVar : BaseOperationRequirement
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		public override void OnInit()
		{
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override object LeftSide(Entity target)
		{
			EntityAlive entityAlive = target as EntityAlive;
			return (entityAlive != null) ? entityAlive.Buffs.GetCustomVar(this.cvar) : 0f;
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override object RightSide(Entity target)
		{
			return GameEventManager.GetFloatValue(target as EntityAlive, this.valueText, 0f);
		}

		public override void ParseProperties(DynamicProperties properties)
		{
			base.ParseProperties(properties);
			properties.ParseString(RequirementCVar.PropCvar, ref this.cvar);
			properties.ParseString(RequirementCVar.PropValue, ref this.valueText);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override BaseRequirement CloneChildSettings()
		{
			return new RequirementCVar
			{
				Invert = this.Invert,
				operation = this.operation,
				cvar = this.cvar,
				valueText = this.valueText
			};
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public string cvar = "";

		[PublicizedFrom(EAccessModifier.Protected)]
		public string valueText;

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropCvar = "cvar";

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropValue = "value";
	}
}
