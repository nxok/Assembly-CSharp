using System;
using UnityEngine;

public class ColliderHitCallForward : MonoBehaviour
{
	public void OnControllerColliderHit(ControllerColliderHit hit)
	{
		if (this.Entity != null)
		{
			this.Entity.OnControllerColliderHit(hit);
		}
	}

	public Entity Entity;
}
