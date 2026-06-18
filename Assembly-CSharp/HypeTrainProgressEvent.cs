using System;
using Newtonsoft.Json;

public class HypeTrainProgressEvent
{
	[JsonProperty("level")]
	public int Level { get; set; }
}
