using System;
using UnityEngine.Scripting;

namespace GameEvent.SequenceActions
{
	[Preserve]
	public class ActionResetSleepers : BaseAction
	{
		public override BaseAction.ActionCompleteStates OnPerformAction()
		{
			GameManager.Instance.World.ResetSleeperVolumes();
			Log.Out("Reset all sleeper volumes");
			return BaseAction.ActionCompleteStates.Complete;
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override BaseAction CloneChildSettings()
		{
			return new ActionResetSleepers();
		}
	}
}
