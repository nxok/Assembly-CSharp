using System;

namespace Twitch.PubSub
{
	public class PubSubListenMessage : BasePubSubMessage
	{
		public PubSubListenMessage()
		{
			base.type = "LISTEN";
		}

		public PubSubListenMessage.PubSubListenData data;

		public class PubSubListenData
		{
			public string[] topics { get; set; }

			public string auth_token { get; set; }
		}
	}
}
