using System;
using UnityEngine.Scripting;

namespace GameEvent.SequenceActions
{
	[Preserve]
	public class ActionClearGroup : BaseAction
	{
		public override BaseAction.ActionCompleteStates OnPerformAction()
		{
			base.Owner.ClearEntityGroup(this.groupName);
			return BaseAction.ActionCompleteStates.Complete;
		}

		public override void ParseProperties(DynamicProperties properties)
		{
			base.ParseProperties(properties);
			properties.ParseString(ActionClearGroup.PropGroupName, ref this.groupName);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override BaseAction CloneChildSettings()
		{
			return new ActionClearGroup
			{
				groupName = this.groupName
			};
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public string groupName = "";

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropGroupName = "group_name";
	}
}
