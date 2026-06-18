using System;
using UnityEngine.Scripting;

namespace GameEvent.SequenceActions
{
	[Preserve]
	public class ActionAddQuest : ActionBaseClientAction
	{
		public override void OnClientPerform(Entity target)
		{
			EntityPlayer entityPlayer = target as EntityPlayer;
			if (entityPlayer != null)
			{
				Quest quest = QuestClass.CreateQuest(this.QuestID);
				entityPlayer.QuestJournal.AddQuest(quest, this.Notify);
			}
		}

		public override void ParseProperties(DynamicProperties properties)
		{
			base.ParseProperties(properties);
			properties.ParseString(ActionAddQuest.PropQuestID, ref this.QuestID);
			properties.ParseBool(ActionAddQuest.PropNotify, ref this.Notify);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override BaseAction CloneChildSettings()
		{
			return new ActionAddQuest
			{
				targetGroup = this.targetGroup,
				QuestID = this.QuestID,
				Notify = this.Notify
			};
		}

		public string QuestID;

		public bool Notify = true;

		public static string PropQuestID = "quest";

		public static string PropNotify = "notify";
	}
}
