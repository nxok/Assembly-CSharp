using System;
using UnityEngine;

namespace Twitch
{
	public class ViewerEntry
	{
		public float CombinedPoints
		{
			get
			{
				return this.SpecialPoints + this.StandardPoints;
			}
		}

		public void RemovePoints(float usedPoints, TwitchAction.PointTypes pointType, TwitchActionEntry entry)
		{
			if (pointType == TwitchAction.PointTypes.SP)
			{
				this.SpecialPoints -= usedPoints;
				entry.SpecialPointsUsed = (int)usedPoints;
				return;
			}
			if (pointType == TwitchAction.PointTypes.PP)
			{
				float num = Mathf.Min(usedPoints, this.StandardPoints);
				entry.StandardPointsUsed = (int)num;
				this.StandardPoints -= num;
				num = usedPoints - num;
				if (num > 0f)
				{
					this.SpecialPoints -= num;
					entry.SpecialPointsUsed = (int)num;
					return;
				}
			}
			else if (pointType == TwitchAction.PointTypes.Bits)
			{
				int num2 = Utils.FastMin((int)usedPoints, (ExtensionManager.Version == "2.0.1") ? this.BitCredits : TwitchAction.GetAdjustedBitPriceFloor(this.BitCredits));
				this.BitCredits -= num2;
				entry.CreditsUsed = num2;
				entry.BitsUsed = (int)usedPoints;
				TwitchLeaderboardStats leaderboardStats = TwitchManager.LeaderboardStats;
				int num3 = ((ExtensionManager.Version == "2.0.1") ? (entry.BitsUsed - num2) : TwitchAction.GetAdjustedBitPriceCeil(entry.BitsUsed - num2));
				if (num3 > 0)
				{
					leaderboardStats.TotalBits += num3;
					leaderboardStats.CheckMostBitsSpent(leaderboardStats.AddBitsUsed(entry.UserName, this.UserColor, num3));
				}
			}
		}

		public float SpecialPoints;

		public float StandardPoints;

		public int BitCredits;

		public int UserID = -1;

		public string UserColor = "FFFFFF";

		public float LastAction = -1f;

		public float addPointsUntil;

		public bool IsActive;

		public bool IsSub;
	}
}
