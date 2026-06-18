using System;
using Audio;
using UnityEngine.Scripting;

[Preserve]
public class MinEventActionFadeOutSound : MinEventActionSoundBase
{
	public override void Execute(MinEventParams _params)
	{
		if ((this.localPlayerOnly && this.targets[0] as EntityPlayerLocal != null) || (!this.localPlayerOnly && this.targets[0] != null))
		{
			string soundGroupForTarget = base.GetSoundGroupForTarget();
			Manager.FadeOut(this.targets[0].entityId, soundGroupForTarget);
		}
	}
}
