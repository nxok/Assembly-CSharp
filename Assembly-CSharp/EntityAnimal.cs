using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public abstract class EntityAnimal : EntityAlive
{
	public void SetDistressed(bool _isDistressed, float _minTime, float _maxTime, int _playerId)
	{
		this.isDistressed = _isDistressed;
		this.minStressTime = _minTime;
		this.maxStressTime = _maxTime;
		this.playerId = _playerId;
		this.timer = 0f;
	}

	public void ClearDistressed()
	{
		this.isDistressed = false;
		EntityPlayerLocal entityPlayerLocal = this.getEntityPlayerLocal();
		if (entityPlayerLocal)
		{
			entityPlayerLocal.Waypoints.TryRemoveLastKnownPositionWaypoint(this.entityId);
		}
	}

	public override void OnUpdateLive()
	{
		base.GetEntitySenses().Clear();
		base.OnUpdateLive();
		if (this.isDistressed && base.IsAlive())
		{
			this.timer -= Time.deltaTime;
			if (this.timer <= 0f)
			{
				this.timer += this.rand.RandomRange(this.minStressTime, this.maxStressTime);
				GameManager.Instance.PlaySoundAtPositionServer(this.position, base.GetSoundHurt(), 1, 1, this.entityId, 1f);
			}
			EntityPlayerLocal entityPlayerLocal = this.getEntityPlayerLocal();
			if (entityPlayerLocal != null)
			{
				entityPlayerLocal.Waypoints.UpdateEntityAnimalWayPoint(this, true);
			}
		}
	}

	public override bool IsDrawMapIcon()
	{
		return false;
	}

	public override Color GetMapIconColor()
	{
		return new Color(1f, 0.8235294f, 0.34117648f);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override float getNextStepSoundDistance()
	{
		return 0.8f;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool isGameMessageOnDeath()
	{
		return false;
	}

	public override int DamageEntity(DamageSource _damageSource, int _strength, bool _criticalHit, float impulseScale)
	{
		return base.DamageEntity(_damageSource, _strength, _criticalHit, impulseScale);
	}

	public override void OnEntityDeath()
	{
		if (this.PhysicsTransform)
		{
			this.PhysicsTransform.gameObject.SetActive(false);
		}
		base.OnEntityDeath();
		EntityPlayerLocal entityPlayerLocal = this.getEntityPlayerLocal();
		if (entityPlayerLocal)
		{
			entityPlayerLocal.Waypoints.TryRemoveLastKnownPositionWaypoint(this.entityId);
		}
	}

	public override void OnEntityUnload()
	{
		base.OnEntityUnload();
		EntityPlayerLocal entityPlayerLocal = this.getEntityPlayerLocal();
		if (entityPlayerLocal)
		{
			entityPlayerLocal.Waypoints.TryRemoveLastKnownPositionWaypoint(this.entityId);
		}
	}

	public override void OnCollectServer(int _playerId)
	{
		GameManager.Instance.World.RemoveEntity(this.entityId, EnumRemoveEntityReason.Captured);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityPlayerLocal getEntityPlayerLocal()
	{
		EntityPlayerLocal entityPlayerLocal = GameManager.Instance.World.GetPrimaryPlayer();
		if (entityPlayerLocal && entityPlayerLocal.entityId != this.playerId)
		{
			entityPlayerLocal = null;
		}
		return entityPlayerLocal;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public EntityAnimal()
	{
	}

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float minStressTime = 2.5f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float maxStressTime = 1f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool isDistressed;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int playerId = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float timer;
}
