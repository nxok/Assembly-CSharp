using System;
using UnityEngine;

public class DamageSourceEntity : DamageSource
{
	public DamageSourceEntity(EnumDamageSource _damageSource, EnumDamageTypes _damageType, int _damageSourceEntityId)
		: base(_damageSource, _damageType)
	{
		this.ownerEntityId = _damageSourceEntityId;
	}

	public DamageSourceEntity(EnumDamageSource _damageSource, EnumDamageTypes _damageType, int _damageSourceEntityId, Vector3 _direction)
		: base(_damageSource, _damageType, _direction)
	{
		this.ownerEntityId = _damageSourceEntityId;
	}

	public DamageSourceEntity(EnumDamageSource _damageSource, EnumDamageTypes _damageType, int _damageSourceEntityId, Vector3 _direction, string _hitTransformName, Vector3 _hitTransformPosition, Vector2 _uvHit)
		: this(_damageSource, _damageType, _damageSourceEntityId, _direction)
	{
		this.hitTransformName = _hitTransformName;
		this.hitTransformPosition = _hitTransformPosition;
		this.uvHit = _uvHit;
	}

	public override Vector3 getHitTransformPosition()
	{
		return this.hitTransformPosition;
	}

	public override string getHitTransformName()
	{
		return this.hitTransformName;
	}

	public override Vector2 getUVHit()
	{
		return this.uvHit;
	}

	public Vector2 uvHit;

	public string hitTransformName;

	public Vector3 hitTransformPosition;
}
