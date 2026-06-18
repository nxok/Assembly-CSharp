using System;
using UnityEngine.Scripting;

namespace GameEvent.SequenceActions
{
	[Preserve]
	public class ActionShowMessageWindow : ActionBaseClientAction
	{
		public override void OnClientPerform(Entity target)
		{
			EntityPlayerLocal entityPlayerLocal = target as EntityPlayerLocal;
			if (entityPlayerLocal != null)
			{
				XUiC_TipWindow.ShowTip(this.message, this.title, entityPlayerLocal, null);
			}
		}

		public override void ParseProperties(DynamicProperties properties)
		{
			base.ParseProperties(properties);
			properties.ParseString(ActionShowMessageWindow.PropMessage, ref this.message);
			properties.ParseString(ActionShowMessageWindow.PropTitle, ref this.title);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override BaseAction CloneChildSettings()
		{
			return new ActionShowMessageWindow
			{
				targetGroup = this.targetGroup,
				message = this.message,
				title = this.title
			};
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static string PropMessage = "message";

		[PublicizedFrom(EAccessModifier.Private)]
		public static string PropTitle = "title";

		[PublicizedFrom(EAccessModifier.Private)]
		public string message = "";

		[PublicizedFrom(EAccessModifier.Private)]
		public string title = "";
	}
}
