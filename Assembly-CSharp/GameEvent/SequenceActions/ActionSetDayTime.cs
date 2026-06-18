using System;
using UnityEngine.Scripting;

namespace GameEvent.SequenceActions
{
	[Preserve]
	public class ActionSetDayTime : BaseAction
	{
		public override BaseAction.ActionCompleteStates OnPerformAction()
		{
			World world = GameManager.Instance.World;
			ulong worldTime = world.worldTime;
			int num = GameUtils.WorldTimeToDays(worldTime);
			int num2 = GameUtils.WorldTimeToHours(worldTime);
			int num3 = GameUtils.WorldTimeToMinutes(worldTime);
			ulong num4 = GameUtils.DayTimeToWorldTime((this.day < 1) ? num : this.day, (this.hours < 0) ? num2 : this.hours, (this.minutes < 0) ? num3 : this.minutes);
			world.SetTimeJump(num4, true);
			return BaseAction.ActionCompleteStates.Complete;
		}

		public override void ParseProperties(DynamicProperties properties)
		{
			base.ParseProperties(properties);
			properties.ParseInt(ActionSetDayTime.PropDay, ref this.day);
			properties.ParseInt(ActionSetDayTime.PropHours, ref this.hours);
			properties.ParseInt(ActionSetDayTime.PropMinutes, ref this.minutes);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override BaseAction CloneChildSettings()
		{
			return new ActionSetDayTime
			{
				day = this.day,
				hours = this.hours,
				minutes = this.minutes
			};
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public int day = -1;

		[PublicizedFrom(EAccessModifier.Protected)]
		public int hours = -1;

		[PublicizedFrom(EAccessModifier.Protected)]
		public int minutes = -1;

		public static string PropDay = "day";

		public static string PropHours = "hours";

		public static string PropMinutes = "minutes";
	}
}
