using System;
using Newtonsoft.Json;

public class SubscriptionEvent : SubscriptionEventBase
{
	[JsonProperty("is_gift")]
	public bool IsGift { get; set; }
}
