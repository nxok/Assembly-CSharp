using System;
using UnityEngine.Scripting;

namespace GameEvent.SequenceActions
{
	[Preserve]
	public class ActionSetScreenEffect : ActionBaseClientAction
	{
		public override void OnClientPerform(Entity target)
		{
			EntityPlayerLocal entityPlayerLocal = target as EntityPlayerLocal;
			if (entityPlayerLocal != null)
			{
				entityPlayerLocal.ScreenEffectManager.SetScreenEffect(this.screenEffect, GameEventManager.GetFloatValue(entityPlayerLocal, this.intensityText, 0f), GameEventManager.GetFloatValue(entityPlayerLocal, this.fadeTimeText, 0f));
			}
		}

		public override void ParseProperties(DynamicProperties properties)
		{
			base.ParseProperties(properties);
			properties.ParseString(ActionSetScreenEffect.PropScreenEffect, ref this.screenEffect);
			properties.ParseString(ActionSetScreenEffect.PropIntensity, ref this.intensityText);
			properties.ParseString(ActionSetScreenEffect.PropFadeTime, ref this.fadeTimeText);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override BaseAction CloneChildSettings()
		{
			return new ActionSetScreenEffect
			{
				screenEffect = this.screenEffect,
				intensityText = this.intensityText,
				fadeTimeText = this.fadeTimeText,
				targetGroup = this.targetGroup
			};
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public string screenEffect = "";

		[PublicizedFrom(EAccessModifier.Protected)]
		public string intensityText;

		[PublicizedFrom(EAccessModifier.Protected)]
		public string fadeTimeText;

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropScreenEffect = "screen_effect";

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropIntensity = "intensity";

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropFadeTime = "fade_time";
	}
}
