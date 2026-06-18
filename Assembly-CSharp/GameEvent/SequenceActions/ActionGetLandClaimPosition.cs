using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

namespace GameEvent.SequenceActions
{
	[Preserve]
	public class ActionGetLandClaimPosition : BaseAction
	{
		public override BaseAction.ActionCompleteStates OnPerformAction()
		{
			World world = GameManager.Instance.World;
			List<Vector3i> lpblocks = world.GetGameManager().GetPersistentPlayerList().GetPlayerDataFromEntityID(base.Owner.Target.entityId)
				.LPBlocks;
			if (lpblocks == null || lpblocks.Count == 0)
			{
				return BaseAction.ActionCompleteStates.InCompleteRefund;
			}
			int num = (GameStats.GetInt(EnumGameStats.LandClaimSize) - 1) / 2;
			int num2 = num * num;
			bool flag = false;
			for (int i = 0; i < lpblocks.Count; i++)
			{
				Vector3i vector3i = lpblocks[i];
				TEFeatureLandClaim selfOrFeature = world.GetTileEntity(World.toBlock(vector3i)).GetSelfOrFeature<TEFeatureLandClaim>();
				if (selfOrFeature != null && selfOrFeature.IsPrimary() && base.Owner.Target.GetDistanceSq(vector3i) < (float)num2)
				{
					flag = true;
					base.Owner.TargetPosition = vector3i;
				}
			}
			if (!flag)
			{
				return BaseAction.ActionCompleteStates.InCompleteRefund;
			}
			return BaseAction.ActionCompleteStates.Complete;
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override BaseAction CloneChildSettings()
		{
			return new ActionGetLandClaimPosition();
		}
	}
}
