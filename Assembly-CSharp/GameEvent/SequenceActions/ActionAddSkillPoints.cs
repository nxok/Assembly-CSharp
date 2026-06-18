using System;
using UnityEngine.Scripting;

namespace GameEvent.SequenceActions
{
	[Preserve]
	public class ActionAddSkillPoints : ActionBaseClientAction
	{
		public override void OnClientPerform(Entity target)
		{
			EntityPlayer entityPlayer = target as EntityPlayer;
			if (entityPlayer != null)
			{
				int intValue = GameEventManager.GetIntValue(entityPlayer, this.skillPointsText, 1);
				if (intValue <= 0)
				{
					return;
				}
				entityPlayer.Progression.SkillPoints += intValue;
			}
		}

		public override void ParseProperties(DynamicProperties properties)
		{
			base.ParseProperties(properties);
			properties.ParseString(ActionAddSkillPoints.PropSkillPoints, ref this.skillPointsText);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override BaseAction CloneChildSettings()
		{
			return new ActionAddSkillPoints
			{
				skillPointsText = this.skillPointsText
			};
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public string skillPointsText = "";

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropSkillPoints = "skill_points";
	}
}
