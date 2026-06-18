using System;
using Newtonsoft.Json;

public class SubscriptionGiftEvent : SubscriptionEventBase
{
	[JsonProperty("is_anonymous")]
	public bool IsAnonymous { get; set; }

	[JsonProperty("total")]
	public int Total { get; set; }
}
