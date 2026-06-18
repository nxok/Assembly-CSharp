using System;
using UnityEngine;

public class TimeRotateObject : MonoBehaviour
{
	[PublicizedFrom(EAccessModifier.Private)]
	public void Update()
	{
		if (GameManager.Instance)
		{
			World world = GameManager.Instance.World;
			if (world != null)
			{
				uint num = GameUtils.WorldTimeToTotalMinutes(world.worldTime) % 60U;
				if (this.hourTransform)
				{
					int num2 = GameUtils.WorldTimeToTotalHours(world.worldTime);
					this.hourTransform.localEulerAngles = new Vector3(0f, 0f, (float)(num2 % 12) * 30f + num * 0.5f);
				}
				if (this.minuteTransform)
				{
					this.minuteTransform.localEulerAngles = new Vector3(0f, 0f, num * 6U);
				}
			}
		}
	}

	public Transform hourTransform;

	public Transform minuteTransform;
}
