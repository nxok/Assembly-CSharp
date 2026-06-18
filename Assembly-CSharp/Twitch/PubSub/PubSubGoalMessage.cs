using System;
using Newtonsoft.Json;

namespace Twitch.PubSub
{
	public class PubSubGoalMessage : BasePubSubMessage
	{
		public new string type { get; set; }

		public string TheType
		{
			get
			{
				return this.type;
			}
		}

		public PubSubGoalMessage.GoalData data { get; set; }

		public static PubSubGoalMessage Deserialize(string message)
		{
			return JsonConvert.DeserializeObject<PubSubGoalMessage>(message);
		}

		public class GoalData
		{
			public PubSubGoalMessage.Goal goal { get; set; }
		}

		public class Goal
		{
			public string contributionType { get; set; }

			public string state { get; set; }

			public int currentContributions { get; set; }

			public int targetContributions { get; set; }
		}
	}
}
