using System;
using UnityEngine;

public class CollisionCallForward : MonoBehaviour
{
	public void OnCollisionEnter(Collision collision)
	{
		if (this.Entity != null)
		{
			this.Entity.OnCollisionForward(base.transform, collision, false);
		}
	}

	public void OnCollisionStay(Collision collision)
	{
		if (this.Entity != null)
		{
			this.Entity.OnCollisionForward(base.transform, collision, true);
		}
	}

	public static Entity FindEntity(Transform _t)
	{
		Entity component = _t.GetComponent<Entity>();
		if (component)
		{
			return component;
		}
		CollisionCallForward componentInParent = _t.GetComponentInParent<CollisionCallForward>();
		if (componentInParent)
		{
			return componentInParent.Entity;
		}
		return null;
	}

	public Entity Entity;
}
