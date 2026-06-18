using System;
using System.Globalization;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class EntityZombieCop : EntityZombie
{
	public override void Init(int _entityClass, EntityInstanceAssets _assets, EModelInstanceAssets _eModelAssets)
	{
		base.Init(_entityClass, _assets, _eModelAssets);
	}

	public override void PostInit()
	{
		GameEventManager.Current.HandleSpawnModifier(this);
		this.inventory.SetItem(0, this.inventory.GetBareHandItemValue(), 1, true);
		this.HandleNavObject();
	}

	public override void CopyPropertiesFromEntityClass()
	{
		DynamicProperties properties = EntityClass.list[this.entityClass].Properties;
		properties.ParseFloat(EntityClass.PropExplodeDelay, ref this.explodeDelay);
		properties.ParseFloat(EntityClass.PropExplodeHealthThreshold, ref this.explodeHealthThreshold);
		properties.ParseString(EntityClass.PropSoundExplodeWarn, ref this.warnSoundName);
		properties.ParseString(EntityClass.PropSoundTick, ref this.tickSoundName);
		if (this.tickSoundName != null)
		{
			string[] array = this.tickSoundName.Split(',', StringSplitOptions.None);
			this.tickSoundName = array[0];
			if (array.Length >= 2)
			{
				this.tickSoundDelayStart = StringParsers.ParseFloat(array[1], 0, -1, NumberStyles.Any);
				if (array.Length >= 3)
				{
					this.tickSoundDelayScale = StringParsers.ParseFloat(array[2], 0, -1, NumberStyles.Any);
				}
			}
		}
		base.CopyPropertiesFromEntityClass();
	}

	public override void OnUpdateEntity()
	{
		base.OnUpdateEntity();
		if (this.isEntityRemote)
		{
			return;
		}
		if (!this.isPrimed && !this.IsSleeping && !this.Buffs.HasBuff("buffShocked"))
		{
			float num = (float)this.Health;
			if (num > 0f && num < (float)this.GetMaxHealth() * this.explodeHealthThreshold)
			{
				this.isPrimed = true;
				this.ticksToStartToExplode = (int)(this.explodeDelay * 20f);
				this.PlayOneShot(this.warnSoundName, false, false, false, null, 1f);
			}
		}
		if (this.isPrimed && !this.IsDead())
		{
			if (this.ticksToStartToExplode > 0)
			{
				this.ticksToStartToExplode--;
				if (this.ticksToStartToExplode == 0)
				{
					this.SpecialAttack2 = true;
					this.ticksToExplode = (int)(this.explodeDelay / 5f * 1.5f * 20f);
				}
			}
			if (this.ticksToExplode > 0)
			{
				this.ticksToExplode--;
				if (this.ticksToExplode == 0)
				{
					base.NotifySleeperDeath();
					this.SetModelLayer(2, false, null);
					this.ticksToExplode = -1;
					GameManager.Instance.ExplosionServer(base.GetPosition(), World.worldToBlockPos(base.GetPosition()), base.transform.rotation, EntityClass.list[this.entityClass].explosionData, this.entityId, 0f, false, null);
					this.timeStayAfterDeath = 0;
					this.SetDead();
				}
			}
			this.tickSoundDelay -= 0.05f;
			if (this.tickSoundDelay <= 0f)
			{
				this.tickSoundDelayStart *= this.tickSoundDelayScale;
				this.tickSoundDelay = this.tickSoundDelayStart;
				if (this.tickSoundDelay < 0.2f)
				{
					this.tickSoundDelay = 0.2f;
				}
				this.PlayOneShot(this.tickSoundName, false, false, false, null, 1f);
			}
		}
		if (this.ticksToExplode < 0)
		{
			this.motion.x = this.motion.x * 0.7f;
			this.motion.z = this.motion.z * 0.7f;
		}
	}

	public override float GetMoveSpeed()
	{
		if (this.ticksToExplode != 0)
		{
			return 0f;
		}
		return base.GetMoveSpeed();
	}

	public override float GetMoveSpeedAggro()
	{
		if (this.ticksToExplode != 0)
		{
			return 0f;
		}
		if (this.isPrimed)
		{
			return this.moveSpeedAggroMax;
		}
		return base.GetMoveSpeedAggro();
	}

	public override bool IsAttackValid()
	{
		return !this.isPrimed && base.IsAttackValid();
	}

	public override void ProcessDamageResponseLocal(DamageResponse _dmResponse)
	{
		if (!this.isEntityRemote && !this.isPrimed && (_dmResponse.HitBodyPart & EnumBodyPartHit.Special) > EnumBodyPartHit.None && _dmResponse.Source.canHitSpecialBodyParts)
		{
			this.HandlePrimingDetonator(-1f);
		}
		base.ProcessDamageResponseLocal(_dmResponse);
	}

	public void PrimeDetonator()
	{
		Detonator componentInChildren = base.gameObject.GetComponentInChildren<Detonator>(true);
		if (componentInChildren != null)
		{
			componentInChildren.PulseRateScale = 1f;
			componentInChildren.gameObject.GetComponent<Light>().color = Color.red;
			componentInChildren.StartCountdown();
			return;
		}
		Log.Out("PrimeDetonator found no Detonator component");
	}

	public void HandlePrimingDetonator(float overrideDelay = -1f)
	{
		this.PlayOneShot(this.warnSoundName, false, false, false, null, 1f);
		this.isPrimed = true;
		this.ticksToStartToExplode = (int)(((overrideDelay > 0f) ? overrideDelay : this.explodeDelay) * 20f);
		this.PrimeDetonator();
		SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageEntityPrimeDetonator>().Setup(this), false, -1, -1, -1, null, 192, false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int ticksToStartToExplode;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int ticksToExplode;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float explodeDelay = 5f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float explodeHealthThreshold = 0.4f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool isPrimed;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public string warnSoundName;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public string tickSoundName;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float tickSoundDelayStart = 1f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float tickSoundDelayScale = 1f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float tickSoundDelay;
}
