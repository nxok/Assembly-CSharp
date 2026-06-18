using System;

namespace Twitch.PubSub
{
	public class TwitchTopic
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public TwitchTopic()
		{
		}

		public string TopicString { get; set; }

		public static TwitchTopic ChannelPoints(string channelId)
		{
			return new TwitchTopic
			{
				TopicString = string.Format("channel-points-channel-v1.{0}", channelId)
			};
		}

		public static TwitchTopic Bits(string channelId)
		{
			return new TwitchTopic
			{
				TopicString = string.Format("channel-bits-events-v2.{0}", channelId)
			};
		}

		public static TwitchTopic Subscription(string channelId)
		{
			return new TwitchTopic
			{
				TopicString = string.Format("channel-subscribe-events-v1.{0}", channelId)
			};
		}

		public static TwitchTopic HypeTrain(string channelId)
		{
			return new TwitchTopic
			{
				TopicString = string.Format("hype-train-events-v1.{0}", channelId)
			};
		}

		public static TwitchTopic CreatorGoal(string channelId)
		{
			return new TwitchTopic
			{
				TopicString = string.Format("creator-goals-events-v1.{0}", channelId)
			};
		}
	}
}
