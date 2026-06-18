using System;
using UnityEngine.Scripting;

namespace GameEvent.SequenceActions
{
	[Preserve]
	public class ActionAddPlayerLevel : ActionBaseClientAction
	{
		public override void OnClientPerform(Entity target)
		{
			EntityPlayer entityPlayer = target as EntityPlayer;
			if (entityPlayer != null)
			{
				int intValue = GameEventManager.GetIntValue(entityPlayer, this.addedLevelsText, 1);
				for (int i = 0; i < intValue; i++)
				{
					entityPlayer.Progression.AddLevelExp(entityPlayer.Progression.ExpToNextLevel, "_xpOther", Progression.XPTypes.Other, false, i == intValue - 1, -1, null);
				}
			}
		}

		public override void ParseProperties(DynamicProperties properties)
		{
			base.ParseProperties(properties);
			properties.ParseString(ActionAddPlayerLevel.PropNewLevel, ref this.addedLevelsText);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override BaseAction CloneChildSettings()
		{
			return new ActionAddPlayerLevel
			{
				addedLevelsText = this.addedLevelsText
			};
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public string addedLevelsText;

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropNewLevel = "levels";
	}
}
