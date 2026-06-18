using System;
using UnityEngine.Scripting;

[Preserve]
public class MinEventActionAddPartTPV : MinEventActionAddPart
{
	public override bool CanExecute(MinEventTypes _eventType, MinEventParams _params)
	{
		EntityPlayerLocal entityPlayerLocal = _params.Self as EntityPlayerLocal;
		if (entityPlayerLocal == null || entityPlayerLocal.emodel.IsFPV)
		{
			EntityPlayer entityPlayer = _params.Self as EntityPlayer;
			if (entityPlayer == null || !entityPlayer.isEntityRemote)
			{
				return false;
			}
		}
		return base.CanExecute(_eventType, _params);
	}
}
