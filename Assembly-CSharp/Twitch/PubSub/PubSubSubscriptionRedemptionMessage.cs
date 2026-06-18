using System;
using Newtonsoft.Json;

namespace Twitch.PubSub
{
	public class PubSubSubscriptionRedemptionMessage : BasePubSubMessage
	{
		public int benefit_end_month { get; set; }

		public string user_name { get; set; }

		public string channel_name { get; set; }

		public string user_id { get; set; }

		public string channel_id { get; set; }

		public string sub_plan { get; set; }

		public string sub_plan_name { get; set; }

		public int months { get; set; }

		public int cumulative_months { get; set; }

		public string context { get; set; }

		public bool is_gift { get; set; }

		public int multi_month_duration { get; set; }

		public static PubSubSubscriptionRedemptionMessage Deserialize(string message)
		{
			return JsonConvert.DeserializeObject<PubSubSubscriptionRedemptionMessage>(message);
		}
	}
}
