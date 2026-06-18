using System;
using Newtonsoft.Json;

public class SubscriptionEventBase
{
	[JsonProperty("user_id")]
	public string UserId { get; set; } = "";

	[JsonProperty("user_login")]
	public string UserLogin { get; set; } = "";

	[JsonProperty("user_name")]
	public string UserName { get; set; } = "";

	[JsonProperty("tier")]
	public string Tier { get; set; } = "";
}
