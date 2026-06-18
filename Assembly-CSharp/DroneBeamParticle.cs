using System;
using UnityEngine;

public class DroneBeamParticle : MonoBehaviour
{
	[PublicizedFrom(EAccessModifier.Private)]
	public void Start()
	{
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Update()
	{
		if (!this.drone)
		{
			this.drone = base.GetComponentInParent<EntityDrone>();
			Transform transform = base.transform.parent.FindInChilds("WristLeft", false);
			if (transform)
			{
				base.transform.SetParent(transform, false);
			}
			return;
		}
		if (this.displayTime > 0f)
		{
			this.displayTime -= Time.deltaTime;
			EntityAlive attackTargetLocal = this.drone.GetAttackTargetLocal();
			if (attackTargetLocal)
			{
				base.transform.rotation = Quaternion.LookRotation(attackTargetLocal.getChestPosition() - this.drone.GetHealArmPosition());
				if (attackTargetLocal.IsDead())
				{
					this.displayTime = 0f;
				}
			}
			if (this.displayTime <= 0f)
			{
				Object.Destroy(base.gameObject);
			}
		}
	}

	public void SetDisplayTime(float time)
	{
		this.displayTime = time;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public EntityDrone drone;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float displayTime;
}
