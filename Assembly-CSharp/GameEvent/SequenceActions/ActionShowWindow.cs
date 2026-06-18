using System;
using UnityEngine.Scripting;

namespace GameEvent.SequenceActions
{
	[Preserve]
	public class ActionShowWindow : ActionBaseClientAction
	{
		public override void OnClientPerform(Entity target)
		{
			EntityPlayerLocal entityPlayerLocal = target as EntityPlayerLocal;
			if (entityPlayerLocal != null)
			{
				entityPlayerLocal.PlayerUI.windowManager.Open(this.window, true);
			}
		}

		public override void ParseProperties(DynamicProperties properties)
		{
			base.ParseProperties(properties);
			properties.ParseString(ActionShowWindow.PropWindow, ref this.window);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override BaseAction CloneChildSettings()
		{
			return new ActionShowWindow
			{
				targetGroup = this.targetGroup,
				window = this.window
			};
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static string PropWindow = "window";

		[PublicizedFrom(EAccessModifier.Private)]
		public string window = "";
	}
}
