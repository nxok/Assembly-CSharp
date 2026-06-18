using System;
using System.ComponentModel;
using Newtonsoft.Json;

namespace Services.Analytics.Events
{
	public class LoginEventData : BaseEventData
	{
		public override string EventType
		{
			get
			{
				return "login";
			}
		}

		[JsonProperty(PropertyName = "session_start_ts")]
		public string SessionStartTimeStamp { get; set; }

		[JsonProperty(PropertyName = "platform")]
		[Description("The platform that the game session was initiated on.")]
		public string Platform { get; set; }

		[JsonProperty(PropertyName = "provider")]
		[Description("Provider Used to launch the game (Steam, XBL, PSN)")]
		public string Provider { get; set; }

		[JsonProperty(PropertyName = "country_code")]
		[Description("The country that the game session was initiated from. Converted From IP Address")]
		public string CountryCode { get; }

		[JsonProperty(PropertyName = "ip")]
		public string IP { get; set; }

		[JsonProperty(PropertyName = "crossplay_enabled")]
		[Description("Was crossplay enabled when the player launched the game")]
		public bool CrossplayEnabled { get; set; }

		[JsonProperty(PropertyName = "is_first_launch_eos")]
		[Description("A flag to identify first session based on EOS")]
		public bool? IsFirstLaunchEos { get; set; }
	}
}
