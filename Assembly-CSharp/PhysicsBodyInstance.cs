using System;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsBodyInstance
{
	public PhysicsBodyInstance(Transform _modelRoot, PhysicsBodyLayout _layout, EnumColliderMode _initialMode)
	{
		this.modelRoot = _modelRoot;
		this.layout = _layout;
		this.Mode = _initialMode;
		this.BindColliders();
		for (int i = 0; i < this.colliders.Count; i++)
		{
			this.colliders[i].ColliderMode = _initialMode;
		}
	}

	public void BindColliders()
	{
		this.colliders.Clear();
		for (int i = 0; i < this.layout.Colliders.Count; i++)
		{
			PhysicsBodyColliderConfiguration physicsBodyColliderConfiguration = this.layout.Colliders[i];
			this.bindCollider(physicsBodyColliderConfiguration);
		}
	}

	public void SetColliderMode(EnumColliderType colliderTypes, EnumColliderMode _mode)
	{
		this.Mode = _mode;
		for (int i = 0; i < this.colliders.Count; i++)
		{
			IBodyColliderInstance bodyColliderInstance = this.colliders[i];
			if ((bodyColliderInstance.Config.Type & colliderTypes) != EnumColliderType.None)
			{
				bodyColliderInstance.ColliderMode = _mode;
			}
		}
	}

	public Transform GetTransformForColliderTag(string tag)
	{
		for (int i = 0; i < this.colliders.Count; i++)
		{
			IBodyColliderInstance bodyColliderInstance = this.colliders[i];
			if (bodyColliderInstance.Transform != null && bodyColliderInstance.Config.Tag == tag)
			{
				return bodyColliderInstance.Transform;
			}
		}
		return null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void bindCollider(PhysicsBodyColliderConfiguration _bodyConfig)
	{
		Transform transform = this.modelRoot.Find(_bodyConfig.Path);
		if (transform)
		{
			BoxCollider component;
			CapsuleCollider component2;
			SphereCollider component3;
			if ((component = transform.GetComponent<BoxCollider>()) != null)
			{
				this.colliders.Add(new PhysicsBodyBoxCollider(component, _bodyConfig));
			}
			else if ((component2 = transform.GetComponent<CapsuleCollider>()) != null)
			{
				this.colliders.Add(new PhysicsBodyCapsuleCollider(component2, _bodyConfig));
			}
			else if ((component3 = transform.GetComponent<SphereCollider>()) != null)
			{
				this.colliders.Add(new PhysicsBodySphereCollider(component3, _bodyConfig));
			}
			else
			{
				this.colliders.Add(new PhysicsBodyNullCollider(transform, _bodyConfig));
			}
			NGUITools.AddMissingComponent<RootTransformRefEntity>(transform.gameObject);
			CharacterJoint component4 = transform.GetComponent<CharacterJoint>();
			if (component4)
			{
				component4.enablePreprocessing = false;
				component4.enableProjection = true;
				return;
			}
		}
		else
		{
			Entity componentInParent = this.modelRoot.GetComponentInParent<Entity>();
			Log.Warning("PhysicsBodies {0}, {1}, path not found {2}", new object[]
			{
				(componentInParent != null) ? componentInParent.GetDebugName() : this.modelRoot.name,
				_bodyConfig.Tag,
				_bodyConfig.Path
			});
		}
	}

	public EnumColliderMode Mode;

	[PublicizedFrom(EAccessModifier.Private)]
	public Transform modelRoot;

	[PublicizedFrom(EAccessModifier.Private)]
	public PhysicsBodyLayout layout;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<IBodyColliderInstance> colliders = new List<IBodyColliderInstance>();
}
