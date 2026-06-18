using System;
using Challenges;
using UnityEngine.Scripting;

namespace GameEvent.SequenceActions
{
	[Preserve]
	public class ActionCompleteChallenge : ActionBaseClientAction
	{
		public override void OnClientPerform(Entity target)
		{
			EntityPlayerLocal entityPlayerLocal = target as EntityPlayerLocal;
			if (entityPlayerLocal != null)
			{
				string[] array = this.ChallengeID.ToLower().Split(',', StringSplitOptions.None);
				for (int i = 0; i < array.Length; i++)
				{
					Challenge challenge = entityPlayerLocal.challengeJournal.ChallengeDictionary[array[i]];
					if (challenge != null)
					{
						challenge.CompleteChallenge(this.ForceRedeem, true);
					}
				}
				entityPlayerLocal.PlayerUI.xui.QuestTracker.TrackedChallenge = null;
			}
		}

		public override void ParseProperties(DynamicProperties properties)
		{
			base.ParseProperties(properties);
			properties.ParseString(ActionCompleteChallenge.PropChallengeID, ref this.ChallengeID);
			properties.ParseBool(ActionCompleteChallenge.PropForceRedeem, ref this.ForceRedeem);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override BaseAction CloneChildSettings()
		{
			return new ActionCompleteChallenge
			{
				ChallengeID = this.ChallengeID,
				ForceRedeem = this.ForceRedeem
			};
		}

		public string ChallengeID = "";

		public bool ForceRedeem;

		public static string PropChallengeID = "challenges";

		public static string PropForceRedeem = "force_redeem";
	}
}
