using System;
using Newtonsoft.Json;

[Serializable]
public class EventSubMetadata
{
	[JsonProperty("message_id")]
	public string MessageId { get; set; } = string.Empty;

	[JsonProperty("message_type")]
	public string MessageType { get; set; } = string.Empty;

	[JsonProperty("message_timestamp")]
	public string MessageTimestamp { get; set; } = string.Empty;
}
