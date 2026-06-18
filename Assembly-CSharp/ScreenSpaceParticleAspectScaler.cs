using System;
using UnityEngine;

public class ScreenSpaceParticleAspectScaler : MonoBehaviour
{
	[PublicizedFrom(EAccessModifier.Private)]
	public void Update()
	{
		float num = (float)Screen.width / (float)Screen.height / this.designTimeAspectRatio;
		base.transform.localScale = new Vector3(num, 1f, 1f);
	}

	public float designTimeAspectRatio;
}
