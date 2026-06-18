using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class DroneRunningLightMan
{
	public DroneRunningLightMan()
	{
		DroneRunningLightMan.instance = this;
		this.runningLights = new List<DroneRunningLight>();
	}

	public void AddLight(DroneRunningLight _light)
	{
		this.runningLights.Add(_light);
	}

	public void QueueLight(DroneRunningLight _light)
	{
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public List<DroneRunningLight> runningLights;

	public static DroneRunningLightMan instance;
}
