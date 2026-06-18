using System;
using Newtonsoft.Json;

namespace Twitch.PubSub
{
	public class PubSubBitRedemptionMessage : BasePubSubMessage
	{
		public static PubSubBitRedemptionMessage Deserialize(string message)
		{
			return JsonConvert.DeserializeObject<PubSubBitRedemptionMessage>(message);
		}

		public PubSubBitRedemptionMessage.BitRedemptionData data;

		public class BitRedemptionData : EventArgs
		{
			public string user_name { get; set; }

			public string channel_name { get; set; }

			public string user_id { get; set; }

			public string channel_id { get; set; }

			public string chat_message { get; set; }

			public int bits_used { get; set; }

			public int total_bits_used { get; set; }

			public bool is_anonymous { get; set; }
		}
	}
}
