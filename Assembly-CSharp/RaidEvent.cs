using System;
using Newtonsoft.Json;

public class RaidEvent
{
	[JsonProperty("from_broadcaster_user_id")]
	public string RaiderID { get; set; }

	[JsonProperty("from_broadcaster_user_login")]
	public string RaiderUserName { get; set; }

	[JsonProperty("viewers")]
	public int viewerCount { get; set; }
}
