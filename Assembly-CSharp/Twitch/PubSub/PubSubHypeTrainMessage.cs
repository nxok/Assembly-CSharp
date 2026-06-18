using System;
using UnityEngine;

namespace Twitch.PubSub
{
	public class PubSubHypeTrainMessage : BasePubSubMessage
	{
		public static PubSubHypeTrainMessage Deserialize(string message)
		{
			Debug.LogWarning("HypeTrainMessage:\n" + message);
			return new PubSubHypeTrainMessage();
		}

		public PubSubHypeTrainMessage.HypeTrainData data;

		public class HypeTrainData : EventArgs
		{
			public string user_name { get; set; }
		}
	}
}
