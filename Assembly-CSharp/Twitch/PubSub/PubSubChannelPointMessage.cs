using System;
using Newtonsoft.Json;

namespace Twitch.PubSub
{
	public class PubSubChannelPointMessage : BasePubSubMessage
	{
		public static PubSubChannelPointMessage Deserialize(string message)
		{
			return JsonConvert.DeserializeObject<PubSubChannelPointMessage>(message);
		}

		public PubSubChannelPointMessage.ChannelRedemptionData data;

		public class ChannelRedemptionData : EventArgs
		{
			public PubSubChannelPointMessage.Redemption redemption { get; set; }
		}

		public class Redemption
		{
			public PubSubChannelPointMessage.User user { get; set; }

			public PubSubChannelPointMessage.Reward reward { get; set; }
		}

		public class User
		{
			public string login { get; set; }

			public string display_name { get; set; }
		}

		public class Reward
		{
			public string title { get; set; }
		}
	}
}
