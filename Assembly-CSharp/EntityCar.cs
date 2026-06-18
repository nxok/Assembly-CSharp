using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class EntityCar : EntityAlive
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public override void Awake()
	{
		base.Awake();
		this.stepHeight = 0.2f;
		this.updateLightOnAllMaterials = base.transform.GetComponent<UpdateLightOnAllMaterials>();
	}

	public override void Init(int _entityClass, EntityInstanceAssets _assets, EModelInstanceAssets _eModelAssets)
	{
		base.Init(_entityClass, _assets, _eModelAssets);
		EntityClass entityClass = EntityClass.list[this.entityClass];
		this.curModelIdx = 0;
		this.modelCount = 1;
		bool flag = true;
		int num = 1;
		while (flag)
		{
			flag = entityClass.Properties.Values.ContainsKey(EntityCar.PropDamagedModel + num.ToString());
			if (flag)
			{
				string text = entityClass.Properties.Values[EntityCar.PropDamagedModel + num.ToString()];
				if (DataLoader.IsInResources(text))
				{
					text = "Entities/" + text;
				}
				GameObject gameObject = DataLoader.LoadAsset<GameObject>(text, false);
				if (gameObject == null)
				{
					throw new Exception("Missing car model '" + text + "'");
				}
				GameObject gameObject2 = Object.Instantiate<GameObject>(gameObject);
				gameObject2.transform.parent = this.emodel.GetModelTransformParent();
				gameObject2.transform.localEulerAngles = Vector3.zero;
				gameObject2.transform.localPosition = Vector3.zero;
				gameObject2.transform.gameObject.SetActive(false);
				this.modelCount++;
			}
			num++;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void Update()
	{
		base.Update();
		if (this.bBoundingBoxNeedsUpdate)
		{
			this.bBoundingBoxNeedsUpdate = false;
			BoxCollider component = base.gameObject.GetComponent<BoxCollider>();
			Vector3 vector = base.transform.localRotation * component.size / 2f;
			vector.x = Mathf.Abs(vector.x);
			vector.y = Mathf.Abs(vector.y);
			vector.z = Mathf.Abs(vector.z);
			this.scaledExtent = new Vector3(vector.x * base.transform.localScale.x, vector.y * base.transform.localScale.y, vector.z * base.transform.localScale.z);
			this.SetPosition(this.position, true);
		}
	}

	public override void OnUpdateLive()
	{
		this.updateDamageModel();
		if (this.isEntityRemote)
		{
			return;
		}
		this.entityCollision(this.motion);
		this.motion.y = this.motion.y - 0.08f;
		this.motion.y = this.motion.y * 0.98f;
		this.motion.x = this.motion.x * 0.75f;
		this.motion.z = this.motion.z * 0.75f;
		if (base.transform.position.y + Origin.position.y < 0f)
		{
			this.SetDead();
		}
		if (this.bPrimed)
		{
			int num = this.explosionTimer - 1;
			this.explosionTimer = num;
			if (num <= 0)
			{
				this.SetDead();
				this.bPrimed = false;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool pushOutOfBlocks(float _x, float _y, float _z)
	{
		int num = Utils.Fastfloor(_x);
		int num2 = Utils.Fastfloor(_y);
		int num3 = Utils.Fastfloor(_z);
		float num4 = _x - (float)num;
		float num5 = _y - (float)num2;
		float num6 = _z - (float)num3;
		Block block = this.world.GetBlock(num, num2, num3).Block;
		if (block.blockID > 0 && block.IsCollideMovement)
		{
			bool flag = !this.world.GetBlock(num - 1, num2, num3).Block.shape.IsSolidCube;
			bool flag2 = !this.world.GetBlock(num + 1, num2, num3).Block.shape.IsSolidCube;
			bool flag3 = !this.world.GetBlock(num, num2 - 1, num3).Block.shape.IsSolidCube;
			bool flag4 = !this.world.GetBlock(num, num2 + 1, num3).Block.shape.IsSolidCube;
			bool flag5 = !this.world.GetBlock(num, num2, num3 - 1).Block.shape.IsSolidCube;
			bool flag6 = !this.world.GetBlock(num, num2, num3 + 1).Block.shape.IsSolidCube;
			byte b = byte.MaxValue;
			double num7 = 9999.0;
			if (flag && (double)num4 < num7)
			{
				num7 = (double)num4;
				b = 0;
			}
			if (flag2 && 1.0 - (double)num4 < num7)
			{
				num7 = (double)(1f - num4);
				b = 1;
			}
			if (flag3 && (double)num5 < num7)
			{
				num7 = (double)num5;
				b = 2;
			}
			if (flag4 && (double)(1f - num5) < num7)
			{
				num7 = (double)(1f - num5);
				b = 3;
			}
			if (flag5 && (double)num6 < num7)
			{
				num7 = (double)num6;
				b = 4;
			}
			if (flag6 && (double)(1f - num6) < num7)
			{
				b = 5;
			}
			float num8 = this.rand.RandomFloat * 0.2f + 0.1f;
			if (b == 0)
			{
				this.motion.x = -num8;
			}
			if (b == 1)
			{
				this.motion.x = num8;
			}
			if (b == 2)
			{
				this.motion.y = -num8;
			}
			if (b == 3)
			{
				this.motion.y = num8;
			}
			if (b == 4)
			{
				this.motion.z = -num8;
			}
			if (b == 5)
			{
				this.motion.z = num8;
			}
			return true;
		}
		return false;
	}

	public override void SetRotation(Vector3 _rot)
	{
		if (this.isEntityRemote)
		{
			base.SetRotation(_rot);
		}
		else
		{
			this.rotation.y = _rot.y % 360f;
			if (this.rotation.y < 0f)
			{
				this.rotation.y = this.rotation.y + 360f;
			}
			this.rotation.y = (float)((int)((this.rotation.y + 45f) / 90f) * 90);
		}
		this.bBoundingBoxNeedsUpdate = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateDamageModel()
	{
		if (this.IsDead())
		{
			return;
		}
		float num = (float)this.Health / (float)this.GetMaxHealth();
		int num2 = (int)((1f - Utils.FastMax(num, 0f)) * (float)(this.modelCount - 1));
		if (num2 != this.curModelIdx)
		{
			this.emodel.GetModelTransformParent().GetChild(num2).gameObject.SetActive(true);
			this.emodel.GetModelTransformParent().GetChild(this.curModelIdx).gameObject.SetActive(false);
			this.curModelIdx = num2;
			this.updateLightOnAllMaterials.Reset();
		}
	}

	public override int DamageEntity(DamageSource _damageSource, int _strength, bool _criticalHit, float impuleScale)
	{
		if (_strength < 10)
		{
			return 0;
		}
		_strength = base.DamageEntity(_damageSource, _strength, _criticalHit, impuleScale);
		float num = (float)this.Health / (float)this.GetMaxHealth();
		if (!this.bPrimed && num <= 0.4f)
		{
			this.bPrimed = true;
			this.world.GetGameManager().SpawnParticleEffectClient(new ParticleEffect("smoke", base.GetPosition() + Vector3.up * 0.2f, this.GetLightBrightness(), Color.white, "Ambient_Loops/a_fire_med_lp", base.transform, false, 1f, ""), this.entityId, false, false);
			this.explosionTimer = 100;
		}
		return _strength;
	}

	public override void OnEntityDeath()
	{
		base.OnEntityDeath();
		if (this.isEntityRemote)
		{
			return;
		}
		if (EntityClass.list[this.entityClass].explosionData.ParticleIndex > 0)
		{
			GameManager.Instance.ExplosionServer(base.GetPosition(), World.worldToBlockPos(base.GetPosition()), base.transform.rotation, EntityClass.list[this.entityClass].explosionData, this.entityId, 0f, false, null);
		}
		base.SetDeathTime(int.MaxValue);
	}

	public override bool CanCollideWithBlocks()
	{
		return false;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool isGameMessageOnDeath()
	{
		return false;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool isEntityStatic()
	{
		return true;
	}

	public override bool CanBePushed()
	{
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool bBoundingBoxNeedsUpdate;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static string PropDamagedModel = "Model-Damage-";

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int curModelIdx;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int modelCount;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool bPrimed;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int explosionTimer;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public UpdateLightOnAllMaterials updateLightOnAllMaterials;
}
