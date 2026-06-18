using System;
using UnityEngine.Scripting;

namespace GameEvent.SequenceActions
{
	[Preserve]
	public class ActionBeltTooltip : ActionBaseClientAction
	{
		public override void OnClientPerform(Entity target)
		{
			EntityPlayerLocal entityPlayerLocal = target as EntityPlayerLocal;
			if (entityPlayerLocal != null)
			{
				if (this.soundName != "")
				{
					GameManager.ShowTooltip(entityPlayerLocal, this.text, false, false, 0f);
					return;
				}
				GameManager.ShowTooltip(entityPlayerLocal, this.text, "", this.soundName, null, false, false, 0f);
			}
		}

		public override void ParseProperties(DynamicProperties properties)
		{
			base.ParseProperties(properties);
			if (properties.Values.ContainsKey(ActionBeltTooltip.PropText))
			{
				this.text = properties.Values[ActionBeltTooltip.PropText];
			}
			if (properties.Values.ContainsKey(ActionBeltTooltip.PropTextKey))
			{
				this.textKey = properties.Values[ActionBeltTooltip.PropTextKey];
				this.text = Localization.Get(this.textKey, false, null);
			}
			properties.ParseString(ActionBeltTooltip.PropSound, ref this.soundName);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override BaseAction CloneChildSettings()
		{
			return new ActionBeltTooltip
			{
				targetGroup = this.targetGroup,
				textKey = this.textKey,
				text = this.text,
				soundName = this.soundName
			};
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public string textKey = "Sequence Complete";

		[PublicizedFrom(EAccessModifier.Protected)]
		public string text = "";

		[PublicizedFrom(EAccessModifier.Protected)]
		public string soundName = "";

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropText = "text";

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropTextKey = "text_key";

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropSound = "sound";
	}
}
