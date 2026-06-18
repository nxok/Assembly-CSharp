using System;
using Twitch;
using UnityEngine;
using UnityEngine.Scripting;

namespace GameEvent.SequenceActions
{
	[Preserve]
	public class ActionTwitchAddPoints : ActionBaseClientAction
	{
		public override void OnClientPerform(Entity target)
		{
			EntityPlayerLocal entityPlayerLocal = target as EntityPlayerLocal;
			if (entityPlayerLocal != null && base.Owner.Requester == entityPlayerLocal)
			{
				this.amount = GameEventManager.GetIntValue(entityPlayerLocal, this.amountText, 0);
				if (this.recipientType == ActionTwitchAddPoints.RecipientTypes.All)
				{
					switch (this.pointType)
					{
					case TwitchAction.PointTypes.PP:
						TwitchManager.Current.ViewerData.AddPointsAll(this.amount, 0, true);
						break;
					case TwitchAction.PointTypes.SP:
						TwitchManager.Current.ViewerData.AddPointsAll(0, this.amount, true);
						break;
					case TwitchAction.PointTypes.Bits:
						Debug.LogWarning("TwitchAddPoints: Cannot add Bit Credit to all.");
						break;
					}
					TwitchManager.Current.SendChannelMessage(Localization.Get(this.awardText, false, null), true);
					return;
				}
				this.viewer = ((this.recipientType == ActionTwitchAddPoints.RecipientTypes.Requester) ? base.Owner.ExtraData : TwitchManager.Current.ViewerData.GetRandomActiveViewer());
				if (this.viewer == "")
				{
					return;
				}
				switch (this.pointType)
				{
				case TwitchAction.PointTypes.PP:
					TwitchManager.Current.ViewerData.AddPoints(this.viewer, this.amount, false, false);
					break;
				case TwitchAction.PointTypes.SP:
					TwitchManager.Current.ViewerData.AddPoints(this.viewer, this.amount, true, false);
					break;
				case TwitchAction.PointTypes.Bits:
					TwitchManager.Current.ViewerData.AddCredit(this.viewer, this.amount, false);
					break;
				}
				if (this.awardText != "")
				{
					TwitchManager.Current.SendChannelMessage(base.GetTextWithElements(Localization.Get(this.awardText, false, null)), true);
				}
			}
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override string ParseTextElement(string element)
		{
			if (element == "amount")
			{
				return this.amount.ToString();
			}
			if (!(element == "viewer"))
			{
				return base.ParseTextElement(element);
			}
			return this.viewer;
		}

		public override void ParseProperties(DynamicProperties properties)
		{
			base.ParseProperties(properties);
			properties.ParseString(ActionTwitchAddPoints.PropAmount, ref this.amountText);
			properties.ParseEnum<TwitchAction.PointTypes>(ActionTwitchAddPoints.PropPointType, ref this.pointType);
			properties.ParseEnum<ActionTwitchAddPoints.RecipientTypes>(ActionTwitchAddPoints.PropRecipientType, ref this.recipientType);
			properties.ParseBool(ActionTwitchAddPoints.PropRequesterOnly, ref this.requesterOnly);
			properties.ParseString(ActionTwitchAddPoints.PropAwardText, ref this.awardText);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override BaseAction CloneChildSettings()
		{
			return new ActionTwitchAddPoints
			{
				amountText = this.amountText,
				pointType = this.pointType,
				recipientType = this.recipientType,
				requesterOnly = this.requesterOnly,
				awardText = this.awardText
			};
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public string amountText;

		[PublicizedFrom(EAccessModifier.Protected)]
		public string viewer;

		[PublicizedFrom(EAccessModifier.Protected)]
		public int amount;

		[PublicizedFrom(EAccessModifier.Protected)]
		public TwitchAction.PointTypes pointType;

		[PublicizedFrom(EAccessModifier.Protected)]
		public bool requesterOnly = true;

		[PublicizedFrom(EAccessModifier.Protected)]
		public string awardText = "";

		[PublicizedFrom(EAccessModifier.Protected)]
		public ActionTwitchAddPoints.RecipientTypes recipientType;

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropAmount = "amount";

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropPointType = "point_type";

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropRecipientType = "recipient_type";

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropRequesterOnly = "requester_only";

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropAwardText = "award_text";

		[PublicizedFrom(EAccessModifier.Protected)]
		public enum RecipientTypes
		{
			Requester,
			All,
			Random
		}
	}
}
