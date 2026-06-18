using System;
using UnityEngine;

public class LocalPlayerFinalCamera : MonoBehaviour
{
	[PublicizedFrom(EAccessModifier.Private)]
	public void OnPreCull()
	{
		this.entityPlayerLocal.finalCamera.fieldOfView = this.entityPlayerLocal.playerCamera.fieldOfView;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnPreRender()
	{
		this.entityPlayerLocal.renderManager.DynamicResolutionRender();
	}

	public EntityPlayerLocal entityPlayerLocal;
}
