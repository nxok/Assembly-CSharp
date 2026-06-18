using System;
using UnityEngine.Scripting;

[Preserve]
public class MinEventActionRemoveBuff : MinEventActionBuffModifierBase
{
	public override void Execute(MinEventParams _params)
	{
		base.Remove(_params);
	}
}
