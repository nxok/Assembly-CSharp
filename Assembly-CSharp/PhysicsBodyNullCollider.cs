using System;
using UnityEngine;

public class PhysicsBodyNullCollider : PhysicsBodyColliderBase
{
	public PhysicsBodyNullCollider(Transform _transform, PhysicsBodyColliderConfiguration _config)
		: base(_transform, _config)
	{
	}

	public override EnumColliderMode ColliderMode
	{
		set
		{
		}
	}
}
