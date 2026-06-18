using System;
using System.IO;
using UnityEngine;

public abstract class PhysicsBodyColliderBase : IBodyColliderInstance
{
	public PhysicsBodyColliderBase(Transform _transform, PhysicsBodyColliderConfiguration _config)
	{
		this.transform = _transform;
		this.config = _config;
		this.rigidBody = _transform.GetComponent<Rigidbody>();
		if (this.rigidBody == null)
		{
			this.rigidBody = _transform.gameObject.AddComponent<Rigidbody>();
		}
		this.transform.tag = this.config.Tag;
		this.transform.gameObject.layer = this.config.CollisionLayer;
		this.enableRigidBody(false);
	}

	public void WriteToXML(TextWriter stream)
	{
		throw new NotImplementedException();
	}

	public Transform Transform
	{
		get
		{
			return this.transform;
		}
	}

	public PhysicsBodyColliderConfiguration Config
	{
		get
		{
			return this.config;
		}
	}

	public abstract EnumColliderMode ColliderMode { set; }

	[PublicizedFrom(EAccessModifier.Protected)]
	public void enableRigidBody(bool _enabled)
	{
		if (this.rigidBody)
		{
			bool isKinematic = this.rigidBody.isKinematic;
			this.rigidBody.isKinematic = !_enabled;
			if (_enabled)
			{
				if (isKinematic)
				{
					this.rigidBody.velocity = Vector3.zero;
					this.rigidBody.angularVelocity = Vector3.zero;
				}
				this.rigidBody.useGravity = true;
				this.rigidBody.interpolation = 1;
				return;
			}
			this.rigidBody.interpolation = 0;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Rigidbody rigidBody;

	[PublicizedFrom(EAccessModifier.Private)]
	public Transform transform;

	[PublicizedFrom(EAccessModifier.Private)]
	public PhysicsBodyColliderConfiguration config;
}
