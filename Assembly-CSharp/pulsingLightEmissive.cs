using System;
using UnityEngine;

public class pulsingLightEmissive : LightState
{
	public override float Intensity
	{
		get
		{
			return (Mathf.Sin(6.2831855f * this.lightLOD.StateRate * Time.time) + 1f) / 2f;
		}
	}

	public override float Emissive
	{
		get
		{
			return this.Intensity;
		}
	}
}
