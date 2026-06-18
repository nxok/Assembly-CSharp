using System;
using UnityEngine;

[ExecuteInEditMode]
public class DangerRoomEnvironmentSim : MonoBehaviour
{
	[PublicizedFrom(EAccessModifier.Private)]
	public void Update()
	{
		if (this.simulateWind)
		{
			Shader.SetGlobalVector("_Wind", new Vector4(this.wind, 0f, 0f, 0f));
			return;
		}
		Shader.SetGlobalVector("_Wind", new Vector4(0f, 0f, 0f, 0f));
	}

	public bool simulateWind = true;

	[Range(0f, 100f)]
	public float wind = 100f;
}
