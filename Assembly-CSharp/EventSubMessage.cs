using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

[Serializable]
public class EventSubMessage
{
	[JsonProperty("metadata")]
	public EventSubMetadata Metadata { get; set; } = new EventSubMetadata();

	[JsonProperty("payload")]
	public JObject Payload { get; set; }
}
