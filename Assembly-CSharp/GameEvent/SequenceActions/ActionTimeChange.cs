using System;
using UnityEngine.Scripting;

namespace GameEvent.SequenceActions
{
	[Preserve]
	public class ActionTimeChange : BaseAction
	{
		public override BaseAction.ActionCompleteStates OnPerformAction()
		{
			World world = GameManager.Instance.World;
			ulong num = world.worldTime;
			ulong num2 = world.worldTime;
			switch (this.timePreset)
			{
			case ActionTimeChange.TimePresets.Current:
				num = world.worldTime;
				break;
			case ActionTimeChange.TimePresets.Morning:
				num = GameUtils.DayTimeToWorldTime(GameUtils.WorldTimeToDays(world.worldTime), (int)SkyManager.GetDuskTime(), 0);
				break;
			case ActionTimeChange.TimePresets.Noon:
				num = GameUtils.DayTimeToWorldTime(GameUtils.WorldTimeToDays(world.worldTime), 12, 0);
				break;
			case ActionTimeChange.TimePresets.Night:
				num = GameUtils.DayTimeToWorldTime(GameUtils.WorldTimeToDays(world.worldTime), 21, 45);
				break;
			case ActionTimeChange.TimePresets.NextMorning:
			{
				ulong worldTime = world.worldTime;
				int num3 = GameUtils.WorldTimeToDays(worldTime);
				int num4 = GameUtils.WorldTimeToHours(worldTime);
				int num5 = (int)SkyManager.GetDawnTime();
				if (num4 < num5)
				{
					num = GameUtils.DayTimeToWorldTime(num3, num5, 0);
				}
				else
				{
					num = GameUtils.DayTimeToWorldTime(num3 + 1, num5, 0);
				}
				break;
			}
			case ActionTimeChange.TimePresets.NextNoon:
			{
				ulong worldTime2 = world.worldTime;
				int num6 = GameUtils.WorldTimeToDays(worldTime2);
				if (GameUtils.WorldTimeToHours(worldTime2) < 12)
				{
					num = GameUtils.DayTimeToWorldTime(num6, 12, 0);
				}
				else
				{
					num = GameUtils.DayTimeToWorldTime(num6 + 1, 12, 0);
				}
				break;
			}
			case ActionTimeChange.TimePresets.NextNight:
			{
				ulong worldTime3 = world.worldTime;
				int num7 = GameUtils.WorldTimeToDays(worldTime3);
				if (GameUtils.WorldTimeToHours(worldTime3) < 22)
				{
					num = GameUtils.DayTimeToWorldTime(num7, 22, 0);
				}
				else
				{
					num = GameUtils.DayTimeToWorldTime(num7 + 1, 22, 0);
				}
				break;
			}
			case ActionTimeChange.TimePresets.HordeNight:
				num = GameUtils.DayTimeToWorldTime(GameStats.GetInt(EnumGameStats.BloodMoonDay), 21, 45);
				break;
			}
			int num8 = GameEventManager.GetIntValue(base.Owner.Target as EntityAlive, this.timeText, 60) * 1000 / 60;
			if (num8 < 0)
			{
				num2 = num + (ulong)((long)num8);
				if (num2 > world.worldTime)
				{
					num2 = 0UL;
				}
			}
			else if (num8 > 0)
			{
				num2 = num + (ulong)((long)num8);
				if (num2 < num)
				{
					num2 = num;
				}
			}
			else
			{
				num2 = num;
			}
			if (num2 != world.worldTime)
			{
				world.SetTimeJump(num2, true);
			}
			return BaseAction.ActionCompleteStates.Complete;
		}

		public override void ParseProperties(DynamicProperties properties)
		{
			base.ParseProperties(properties);
			properties.ParseString(ActionTimeChange.PropTime, ref this.timeText);
			properties.ParseEnum<ActionTimeChange.TimePresets>(ActionTimeChange.PropTimePreset, ref this.timePreset);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override BaseAction CloneChildSettings()
		{
			return new ActionTimeChange
			{
				timeText = this.timeText,
				timePreset = this.timePreset
			};
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public string timeText;

		[PublicizedFrom(EAccessModifier.Protected)]
		public ActionTimeChange.TimePresets timePreset;

		public static string PropTimePreset = "time_preset";

		public static string PropTime = "time";

		[PublicizedFrom(EAccessModifier.Protected)]
		public enum TimePresets
		{
			Current,
			Morning,
			Noon,
			Night,
			NextMorning,
			NextNoon,
			NextNight,
			HordeNight
		}
	}
}
