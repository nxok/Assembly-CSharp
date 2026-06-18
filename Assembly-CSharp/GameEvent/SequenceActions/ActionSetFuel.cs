using System;
using UnityEngine.Scripting;

namespace GameEvent.SequenceActions
{
	[Preserve]
	public class ActionSetFuel : ActionBaseTargetAction
	{
		public override BaseAction.ActionCompleteStates PerformTargetAction(Entity target)
		{
			EntityVehicle entityVehicle = target as EntityVehicle;
			if (entityVehicle != null)
			{
				ActionSetFuel.FuelSettingTypes settingType = this.SettingType;
				if (settingType != ActionSetFuel.FuelSettingTypes.Remove)
				{
					if (settingType == ActionSetFuel.FuelSettingTypes.Fill)
					{
						if (entityVehicle.vehicle.GetMaxFuelLevel() > 0f)
						{
							entityVehicle.vehicle.SetFuelLevel(entityVehicle.vehicle.GetMaxFuelLevel());
							entityVehicle.StopUIInteraction();
							return BaseAction.ActionCompleteStates.Complete;
						}
					}
				}
				else if (entityVehicle.vehicle.GetMaxFuelLevel() > 0f)
				{
					entityVehicle.vehicle.SetFuelLevel(0f);
					entityVehicle.StopUIInteraction();
					return BaseAction.ActionCompleteStates.Complete;
				}
			}
			return BaseAction.ActionCompleteStates.InComplete;
		}

		public override void ParseProperties(DynamicProperties properties)
		{
			base.ParseProperties(properties);
			properties.ParseEnum<ActionSetFuel.FuelSettingTypes>(ActionSetFuel.PropFuelSettingType, ref this.SettingType);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override BaseAction CloneChildSettings()
		{
			return new ActionSetFuel
			{
				targetGroup = this.targetGroup,
				SettingType = this.SettingType
			};
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public ActionSetFuel.FuelSettingTypes SettingType;

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropFuelSettingType = "setting_type";

		[PublicizedFrom(EAccessModifier.Protected)]
		public enum FuelSettingTypes
		{
			Remove,
			Fill
		}
	}
}
