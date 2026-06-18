using System;

namespace Twitch.PubSub
{
	public class BasePubSubMessage
	{
		public string type
		{
			get; [PublicizedFrom(EAccessModifier.Protected)]
			set;
		}

		public string nonce { get; set; } = Guid.NewGuid().ToString().Replace("-", "");

		public virtual void ReceiveData(string data)
		{
		}
	}
}
