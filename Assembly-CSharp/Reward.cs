using System;
using Newtonsoft.Json;

public class Reward
{
	[JsonProperty("id")]
	public string Id { get; set; } = "";

	[JsonProperty("title")]
	public string Title { get; set; } = "";

	[JsonProperty("cost")]
	public int Cost { get; set; }
}
