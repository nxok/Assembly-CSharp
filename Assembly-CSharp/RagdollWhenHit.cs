using System;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class RagdollWhenHit : RootTransformRefEntity
{
	public Entity theEntity
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			if (this._entity == null && this.RootTransform != null)
			{
				this._entity = this.RootTransform.GetComponent<Entity>();
			}
			return this._entity;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void Start()
	{
		base.Start();
		CapsuleCollider component = base.GetComponent<CapsuleCollider>();
		if (component != null)
		{
			this._radius = component.radius;
			this._offset = component.center;
			return;
		}
		SphereCollider component2 = base.GetComponent<SphereCollider>();
		if (component2 != null)
		{
			this._radius = component2.radius;
			this._offset = component2.center;
			return;
		}
		BoxCollider component3 = base.GetComponent<BoxCollider>();
		this._radius = Mathf.Max(new float[]
		{
			component3.size.x,
			component3.size.y,
			component3.size.z
		});
		this._offset = component3.center;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnEnable()
	{
		this._pos = base.transform.position;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void LateUpdate()
	{
		Vector3 vector = base.transform.position - this._pos;
		if (vector.sqrMagnitude > 0.001f)
		{
			float magnitude = vector.magnitude;
			vector.Normalize();
			RaycastHit raycastHit;
			if (Physics.SphereCast(this._pos + this._offset, this._radius, vector, ref raycastHit, magnitude, 65536))
			{
				base.enabled = false;
				if (this.theEntity != null)
				{
					DamageResponse damageResponse = DamageResponse.New(false);
					damageResponse.ImpulseScale = 0f;
					this.theEntity.emodel.DoRagdoll(in damageResponse, EModelBase.RagdollMode.Default, 999999f);
					return;
				}
			}
			else
			{
				this._pos = base.transform.position;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Entity _entity;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public SphereCollider _sphere;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public BoxCollider _box;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public CapsuleCollider _capsule;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool _hasFrame;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3 _pos;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3 _offset;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float _radius;
}
