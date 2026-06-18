using System;
using UnityEngine.Scripting;

namespace GameEvent.SequenceActions
{
	[Preserve]
	public class ActionCloseWindow : ActionBaseClientAction
	{
		public override void OnClientPerform(Entity target)
		{
			EntityPlayerLocal entityPlayerLocal = target as EntityPlayerLocal;
			if (entityPlayerLocal != null)
			{
				if (this.windowName == "")
				{
					entityPlayerLocal.PlayerUI.windowManager.CloseAllOpenModalWindows(null, false);
					return;
				}
				entityPlayerLocal.PlayerUI.windowManager.Close(this.windowName);
			}
		}

		public override void ParseProperties(DynamicProperties properties)
		{
			base.ParseProperties(properties);
			properties.ParseString(ActionCloseWindow.PropWindow, ref this.windowName);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override BaseAction CloneChildSettings()
		{
			return new ActionCloseWindow
			{
				targetGroup = this.targetGroup,
				windowName = this.windowName
			};
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public string windowName = "";

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropWindow = "window";
	}
}
