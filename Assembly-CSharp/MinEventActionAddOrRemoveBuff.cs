using System;
using UnityEngine.Scripting;

[Preserve]
public class MinEventActionAddOrRemoveBuff : MinEventActionAddBuff
{
	public override bool CanExecute(MinEventTypes _eventType, MinEventParams _params)
	{
		this.isAdd = base.CanExecute(_eventType, _params);
		return true;
	}

	public override void Execute(MinEventParams _params)
	{
		if (this.isAdd)
		{
			base.Execute(_params);
			return;
		}
		base.Remove(_params);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isAdd;
}
