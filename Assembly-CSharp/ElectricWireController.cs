using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public class ElectricWireController : MonoBehaviour
{
	public float HealthRatio
	{
		get
		{
			return this.healthRatio;
		}
		set
		{
			this.lastHealthRatio = this.healthRatio;
			this.healthRatio = value;
			if (this.lastHealthRatio != -1f)
			{
				if (this.lastHealthRatio > this.brokenPercentage && this.healthRatio <= this.brokenPercentage)
				{
					this.setWireDip(true);
					return;
				}
				if (this.lastHealthRatio <= this.brokenPercentage && this.healthRatio > this.brokenPercentage)
				{
					this.setWireDip(false);
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setWireDip(bool dip)
	{
		float wireDip = this.WireNode.GetWireDip();
		if (dip)
		{
			if (Mathf.Approximately(wireDip, 0f))
			{
				this.WireNode.SetWireDip(0.25f);
				this.WireNode.BuildMesh();
				return;
			}
		}
		else if (!Mathf.Approximately(wireDip, 0f))
		{
			this.WireNode.SetWireDip(0f);
			this.WireNode.BuildMesh();
		}
	}

	public void Init(DynamicProperties _properties)
	{
		if (_properties.Values.ContainsKey("Buff"))
		{
			if (this.buffActions == null)
			{
				this.buffActions = new List<string>();
			}
			string[] array = _properties.Values["Buff"].Split(',', StringSplitOptions.None);
			for (int i = 0; i < array.Length; i++)
			{
				this.buffActions.Add(array[i]);
			}
		}
		if (_properties.Values.ContainsKey("BreakingPercentage"))
		{
			this.breakingPercentage = Mathf.Clamp01(StringParsers.ParseFloat(_properties.Values["BreakingPercentage"], 0, -1, NumberStyles.Any));
		}
		else
		{
			this.breakingPercentage = 0.5f;
		}
		if (_properties.Values.ContainsKey("BrokenPercentage"))
		{
			this.brokenPercentage = Mathf.Clamp01(StringParsers.ParseFloat(_properties.Values["BrokenPercentage"], 0, -1, NumberStyles.Any));
		}
		else
		{
			this.brokenPercentage = 0.25f;
		}
		if (_properties.Values.ContainsKey("DamageReceived"))
		{
			StringParsers.TryParseFloat(_properties.Values["DamageReceived"], out this.damageReceived);
		}
		else
		{
			this.damageReceived = 0.1f;
		}
		this.healthRatio = -1f;
		this.BlockPosition = this.TileEntityChild.ToWorldPos();
		this.startPoint = this.WireNode.GetStartPosition() + this.WireNode.GetStartPositionOffset();
		this.endPoint = this.WireNode.GetEndPosition() + this.WireNode.GetEndPositionOffset();
		BlockValue block = GameManager.Instance.World.GetBlock(this.BlockPosition);
		float num = 1f - (float)block.damage / (float)block.Block.MaxDamage;
		if (num <= this.brokenPercentage)
		{
			this.setWireDip(true);
			return;
		}
		if (num > this.brokenPercentage)
		{
			this.setWireDip(false);
		}
	}

	public void DamageSelf(float damage)
	{
		this.totalDamage += damage;
		if (this.totalDamage < 1f)
		{
			return;
		}
		damage = (float)((int)this.totalDamage);
		this.totalDamage = 0f;
		if (this.chunk == null)
		{
			this.chunk = (Chunk)GameManager.Instance.World.GetChunkFromWorldPos(this.BlockPosition);
		}
		BlockValue block = GameManager.Instance.World.GetBlock(this.BlockPosition);
		this.HealthRatio = 1f - (float)block.damage / (float)block.Block.MaxDamage;
		float num = this.HealthRatio;
		float num2 = ((float)block.damage + damage) / (float)block.Block.MaxDamage;
		block.damage = Mathf.Clamp(block.damage + (int)damage, 0, block.Block.MaxDamage);
		GameManager.Instance.World.SetBlockRPC(this.BlockPosition, block);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Update()
	{
		BlockValue block = GameManager.Instance.World.GetBlock(this.BlockPosition);
		this.HealthRatio = 1f - (float)block.damage / (float)block.Block.MaxDamage;
		bool flag = this.HealthRatio < this.brokenPercentage;
		this.HandleParticlesForBroken(flag);
		this.setWireDip(flag);
		if (this.TileEntityParent == null || !this.TileEntityParent.IsPowered)
		{
			if (this.CollidersThisFrame != null && this.CollidersThisFrame.Count > 0)
			{
				this.CollidersThisFrame.Clear();
			}
			return;
		}
		if (this.CollidersThisFrame == null || this.CollidersThisFrame.Count == 0)
		{
			return;
		}
		for (int i = 0; i < this.CollidersThisFrame.Count; i++)
		{
			this.touched(this.CollidersThisFrame[i]);
		}
		this.CollidersThisFrame.Clear();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnTriggerEnter(Collider other)
	{
		if (this.TileEntityParent == null || !this.TileEntityParent.IsPowered)
		{
			return;
		}
		if (this.CollidersThisFrame == null)
		{
			this.CollidersThisFrame = new List<Collider>();
		}
		if (!this.CollidersThisFrame.Contains(other))
		{
			this.CollidersThisFrame.Add(other);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnTriggerStay(Collider other)
	{
		if (this.TileEntityParent == null || !this.TileEntityParent.IsPowered)
		{
			return;
		}
		if (this.CollidersThisFrame == null)
		{
			this.CollidersThisFrame = new List<Collider>();
		}
		if (!this.CollidersThisFrame.Contains(other))
		{
			this.CollidersThisFrame.Add(other);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnTriggerExit(Collider other)
	{
		if (this.TileEntityParent == null || !this.TileEntityParent.IsPowered)
		{
			return;
		}
		if (this.CollidersThisFrame == null)
		{
			this.CollidersThisFrame = new List<Collider>();
		}
		if (!this.CollidersThisFrame.Contains(other))
		{
			this.CollidersThisFrame.Add(other);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void touched(Collider collider)
	{
		if (this.TileEntityParent == null || this.TileEntityChild == null || this.WireNode == null || collider == null)
		{
			return;
		}
		if (this.TileEntityParent.IsPowered && this.TileEntityChild.IsPowered && collider.transform != null)
		{
			EntityAlive entityAlive = collider.transform.GetComponent<EntityAlive>();
			if (entityAlive == null)
			{
				entityAlive = collider.transform.GetComponentInParent<EntityAlive>();
			}
			if (entityAlive == null && collider.transform.parent != null)
			{
				entityAlive = collider.transform.parent.GetComponentInChildren<EntityAlive>();
			}
			if (entityAlive == null)
			{
				entityAlive = collider.transform.GetComponentInChildren<EntityAlive>();
			}
			if (entityAlive != null && entityAlive.IsAlive())
			{
				bool flag = false;
				if (this.HealthRatio < this.brokenPercentage)
				{
					this.HandleParticlesForBroken(true);
					return;
				}
				if (!entityAlive.Electrocuted && entityAlive.Buffs.GetCustomVar("ShockImmunity") == 0f && this.buffActions != null)
				{
					for (int i = 0; i < this.buffActions.Count; i++)
					{
						if (entityAlive.emodel != null && entityAlive.emodel.transform != null)
						{
							Transform transform = entityAlive.emodel.transform;
							if (entityAlive.emodel.GetHitTransform(BodyPrimaryHit.Torso) != null)
							{
								entityAlive.Buffs.SetCustomVar("ETrapHit", 1f, true, CVarOperation.set);
								entityAlive.Buffs.AddBuff(this.buffActions[i], this.TileEntityParent.OwnerEntityID, true, true, -1f);
								entityAlive.Electrocuted = true;
								flag = true;
							}
						}
					}
				}
				if (flag)
				{
					this.DamageSelf(this.damageReceived);
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static string GetGameObjectPath(Entity e, Transform transform)
	{
		string text = transform.name;
		while (transform.parent != null && transform.parent.name != e.transform.name)
		{
			transform = transform.parent;
			text = transform.name + "/" + text;
		}
		return text;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleParticlesForBroken(bool isBroken)
	{
		if (isBroken && this.TileEntityParent.IsPowered)
		{
			if (this.particleDelay > 0f)
			{
				this.particleDelay -= Time.deltaTime;
			}
			if (isBroken && this.particleDelay <= 0f)
			{
				Vector3 vector = this.WireNode.GetEndPosition() + this.WireNode.GetEndPositionOffset();
				float num = GameManager.Instance.World.GetLightBrightness(World.worldToBlockPos(this.BlockPosition.ToVector3())) / 2f;
				ParticleEffect particleEffect = new ParticleEffect("electric_fence_sparks", vector, num, new Color(1f, 1f, 1f, 0.3f), "electric_fence_impact", null, false, 1f, "");
				GameManager.Instance.SpawnParticleEffectServer(particleEffect, -1, true, true);
				this.particleDelay = 1f + Random.value * 4f;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const float cDipValue = 0.25f;

	public TileEntityPoweredMeleeTrap TileEntityParent;

	public TileEntityPoweredMeleeTrap TileEntityChild;

	public IWireNode WireNode;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float healthRatio = -1f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public List<string> buffActions;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public static string PropDamageReceived = "Damage_received";

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float damageReceived;

	public Vector3i BlockPosition;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Chunk chunk;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float totalDamage;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float particleDelay;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float brokenPercentage;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float breakingPercentage;

	public int OwnerEntityID = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3 startPoint;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3 endPoint;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public List<Collider> CollidersThisFrame;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float lastHealthRatio = 1f;
}
