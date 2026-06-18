using System;
using UnityEngine.Scripting;

namespace GameEvent.SequenceActions
{
	[Preserve]
	public class ActionBaseClientAction : ActionBaseTargetAction
	{
		public override BaseAction.ActionCompleteStates PerformTargetAction(Entity target)
		{
			EntityPlayer entityPlayer = target as EntityPlayer;
			if (entityPlayer != null)
			{
				this.OnServerPerform(entityPlayer);
				if (entityPlayer is EntityPlayerLocal)
				{
					this.OnClientPerform(entityPlayer);
				}
				else
				{
					SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageGameEventResponse>().Setup(base.Owner.Name, entityPlayer.entityId, base.Owner.ExtraData, base.Owner.Tag, NetPackageGameEventResponse.ResponseTypes.ClientSequenceAction, -1, -1, false, base.GetActionKey()), false, entityPlayer.entityId, -1, -1, null, 192, false);
				}
			}
			return BaseAction.ActionCompleteStates.Complete;
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public virtual void OnServerPerform(Entity target)
		{
		}
	}
}
