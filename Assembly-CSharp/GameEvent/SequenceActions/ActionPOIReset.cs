using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

namespace GameEvent.SequenceActions
{
	[Preserve]
	public class ActionPOIReset : BaseAction
	{
		public override BaseAction.ActionCompleteStates OnPerformAction()
		{
			if (this._state == ActionPOIReset.State.Start)
			{
				this._state = ActionPOIReset.State.Wait;
				this._retVal = BaseAction.ActionCompleteStates.InComplete;
				GameManager.Instance.StartCoroutine(this.onPerformAction());
			}
			return this._retVal;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public IEnumerator onPerformAction()
		{
			Vector3i poiposition = base.Owner.POIPosition;
			World world = GameManager.Instance.World;
			if (base.Owner.POIInstance == null)
			{
				this._retVal = BaseAction.ActionCompleteStates.InCompleteRefund;
				yield break;
			}
			List<PrefabInstance> prefabsIntersecting = GameManager.Instance.GetDynamicPrefabDecorator().GetPrefabsIntersecting(base.Owner.POIInstance);
			int num = -1;
			if (!GameManager.Instance.IsEditMode() && !GameUtils.IsPlaytesting())
			{
				num = ((base.Owner.Requester != null) ? base.Owner.Requester.entityId : (-1));
			}
			yield return world.ResetPOIS(prefabsIntersecting, QuestEventManager.manualResetTag, num, null, null);
			this._retVal = BaseAction.ActionCompleteStates.Complete;
			yield break;
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public IEnumerator UpdateBlocks(List<BlockChangeInfo> blockChanges)
		{
			yield return new WaitForSeconds(1f);
			GameManager.Instance.World.SetBlocksRPC(blockChanges);
			yield break;
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override BaseAction CloneChildSettings()
		{
			return new ActionPOIReset();
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override void OnReset()
		{
			this._state = ActionPOIReset.State.Start;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public BaseAction.ActionCompleteStates _retVal;

		[PublicizedFrom(EAccessModifier.Private)]
		public ActionPOIReset.State _state;

		[PublicizedFrom(EAccessModifier.Private)]
		public enum State
		{
			Start,
			Wait
		}
	}
}
