using System;
using UnityEngine.Scripting;

[Preserve]
public class DialogRequirementDroneState : BaseDialogRequirement
{
	public override BaseDialogRequirement.RequirementTypes RequirementType
	{
		get
		{
			return BaseDialogRequirement.RequirementTypes.DroneState;
		}
	}

	public override bool CheckRequirement(EntityPlayer player, EntityNPC talkingTo)
	{
		EntityDrone entityDrone = talkingTo as EntityDrone;
		if (entityDrone)
		{
			EntityDrone.Orders orders;
			if (Enum.TryParse<EntityDrone.Orders>(base.Value, out orders))
			{
				return entityDrone.OrderState == orders;
			}
			EntityDrone.AllyHealMode allyHealMode;
			if (Enum.TryParse<EntityDrone.AllyHealMode>(base.Value, out allyHealMode) && entityDrone.IsHealModAttached)
			{
				return entityDrone.HealAllyMode == allyHealMode;
			}
			EntityDrone.AttackMode attackMode;
			if (Enum.TryParse<EntityDrone.AttackMode>(base.Value, out attackMode) && entityDrone.IsWeaponAttached && GamePrefs.GetBool(EnumGamePrefs.DebugMenuEnabled))
			{
				return entityDrone.AttackState == attackMode;
			}
			if (entityDrone.IsFlashlightAttached)
			{
				if (base.Value.Equals("LightOff"))
				{
					return !entityDrone.IsFlashlightOn;
				}
				if (base.Value.Equals("LightOn"))
				{
					return entityDrone.IsFlashlightOn;
				}
			}
			bool flag = entityDrone.TargetCanBeHealed(player);
			if (base.Value.Equals("Heal") && flag)
			{
				return flag;
			}
		}
		return false;
	}
}
