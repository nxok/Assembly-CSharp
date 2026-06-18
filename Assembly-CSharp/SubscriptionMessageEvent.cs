using System;
using Newtonsoft.Json;

public class SubscriptionMessageEvent : SubscriptionEventBase
{
	[JsonProperty("cumulative_months")]
	public int CumulativeMonths { get; set; }

	[JsonProperty("streak_months")]
	public string StreakMonths { get; set; }

	[JsonProperty("duration_months")]
	public string DurationMonths { get; set; } = "";
}
