using System;
using UnityEngine.Rendering.PostProcessing;

[PostProcess(typeof(ScreenEffectsProxyRenderer), 3, "Custom/Screen Effects Proxy", false)]
[Serializable]
public sealed class ScreenEffectsProxy : PostProcessEffectSettings
{
	public override bool IsEnabledAndSupported(PostProcessRenderContext context)
	{
		if (base.IsEnabledAndSupported(context))
		{
			ScreenEffects instance = ScreenEffects.Instance;
			if (instance != null && instance.activeEffects.Count > 0)
			{
				return ScreenEffects.Instance.isActiveAndEnabled;
			}
		}
		return false;
	}
}
