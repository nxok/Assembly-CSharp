using System;
using System.ComponentModel;
using Newtonsoft.Json;

namespace Services.Analytics.Events
{
	public class HeartbeatEventData : BaseEventData
	{
		public override string EventType
		{
			get
			{
				return "heartbeat";
			}
		}

		[JsonProperty(PropertyName = "heartbeat_ts")]
		[Description("Timestamp of the heartbeat. Sent every 5 minutes after login")]
		public string HeartbeatTimestamp { get; set; }

		[JsonProperty(PropertyName = "server_id")]
		[Description("Id of the connected server, null if not in one")]
		public string ServerId { get; set; }
	}
}
