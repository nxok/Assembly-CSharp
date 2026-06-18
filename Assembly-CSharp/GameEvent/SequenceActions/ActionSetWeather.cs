using System;
using UnityEngine.Scripting;

namespace GameEvent.SequenceActions
{
	[Preserve]
	public class ActionSetWeather : BaseAction
	{
		public override BaseAction.ActionCompleteStates OnPerformAction()
		{
			float floatValue = GameEventManager.GetFloatValue(base.Owner.Target as EntityAlive, this.timeText, 60f);
			WeatherManager.Instance.ForceWeather(this.weatherGroup, floatValue);
			return BaseAction.ActionCompleteStates.Complete;
		}

		public override void ParseProperties(DynamicProperties properties)
		{
			base.ParseProperties(properties);
			properties.ParseString(ActionSetWeather.PropTime, ref this.timeText);
			properties.ParseString(ActionSetWeather.PropWeatherGroup, ref this.weatherGroup);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override BaseAction CloneChildSettings()
		{
			return new ActionSetWeather
			{
				weatherGroup = this.weatherGroup,
				timeText = this.timeText
			};
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public string timeText;

		[PublicizedFrom(EAccessModifier.Protected)]
		public string weatherGroup = "default";

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropTime = "time";

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropWeatherGroup = "weather_group";
	}
}
