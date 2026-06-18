using System;
using UnityEngine.Scripting;

namespace GameEvent.SequenceActions
{
	[Preserve]
	public class ActionEjectFromVehicle : ActionBaseTargetAction
	{
		public override BaseAction.ActionCompleteStates PerformTargetAction(Entity target)
		{
			EntityPlayer entityPlayer = target as EntityPlayer;
			if (entityPlayer != null && entityPlayer.AttachedToEntity != null)
			{
				if (entityPlayer.isEntityRemote)
				{
					SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageCloseAllWindows>().Setup(entityPlayer.entityId), false, entityPlayer.entityId, -1, -1, null, 192, false);
				}
				else
				{
					(entityPlayer as EntityPlayerLocal).PlayerUI.windowManager.CloseAllOpenModalWindows(null, false);
				}
				entityPlayer.SendDetach();
			}
			return BaseAction.ActionCompleteStates.Complete;
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override BaseAction CloneChildSettings()
		{
			return new ActionEjectFromVehicle
			{
				targetGroup = this.targetGroup
			};
		}
	}
}
