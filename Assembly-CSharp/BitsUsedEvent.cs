using System;
using Newtonsoft.Json;

public class BitsUsedEvent
{
	[JsonProperty("is_anonymous")]
	public bool IsAnonymous { get; set; }

	[JsonProperty("user_id")]
	public string UserId { get; set; } = "";

	[JsonProperty("user_login")]
	public string UserLogin { get; set; } = "";

	[JsonProperty("user_name")]
	public string UserName { get; set; } = "";

	[JsonProperty("broadcaster_user_id")]
	public string BroadcasterUserId { get; set; } = "";

	[JsonProperty("broadcaster_user_login")]
	public string BroadcasterUserLogin { get; set; } = "";

	[JsonProperty("broadcaster_user_name")]
	public string BroadcasterUserName { get; set; } = "";

	[JsonProperty("bits")]
	public int Bits { get; set; }
}
