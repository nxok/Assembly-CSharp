using System;
using UnityEngine;

public class DamageSource
{
	public ItemClass ItemClass
	{
		get
		{
			if (this.AttackingItem != null)
			{
				return this.AttackingItem.ItemClass;
			}
			return null;
		}
	}

	public DamageSource(EnumDamageSource _dsn, EnumDamageTypes _damageType)
	{
		this.damageSource = _dsn;
		this.damageType = _damageType;
		this.DamageTypeTag = FastTags<TagGroup.Global>.Parse(_damageType.ToStringCached<EnumDamageTypes>());
	}

	public DamageSource(EnumDamageSource _dsn, EnumDamageTypes _damageType, Vector3 _direction)
	{
		this.damageSource = _dsn;
		this.damageType = _damageType;
		this.direction = _direction;
		this.DamageTypeTag = FastTags<TagGroup.Global>.Parse(_damageType.ToStringCached<EnumDamageTypes>());
	}

	public bool AffectedByArmor()
	{
		return this.damageSource == EnumDamageSource.External;
	}

	public EquipmentSlots GetEntityDamageEquipmentSlot(Entity entity)
	{
		if (entity.emodel)
		{
			Transform hitTransform = entity.emodel.GetHitTransform(this);
			if (hitTransform)
			{
				string tag = hitTransform.tag;
				if ("E_BP_Head".Equals(tag))
				{
					return EquipmentSlots.Head;
				}
				if ("E_BP_Body".Equals(tag))
				{
					return EquipmentSlots.Chest;
				}
				if ("E_BP_LLeg".Equals(tag))
				{
					return EquipmentSlots.Chest;
				}
				if ("E_BP_RLeg".Equals(tag))
				{
					return EquipmentSlots.Chest;
				}
				if ("E_BP_LArm".Equals(tag))
				{
					return EquipmentSlots.Hands;
				}
				if ("E_BP_RArm".Equals(tag))
				{
					return EquipmentSlots.Hands;
				}
			}
		}
		return EquipmentSlots.Count;
	}

	public EquipmentSlotGroups GetEntityDamageEquipmentSlotGroup(Entity entity)
	{
		if (entity.emodel)
		{
			Transform hitTransform = entity.emodel.GetHitTransform(this);
			if (hitTransform)
			{
				string tag = hitTransform.tag;
				if ("E_BP_Head".Equals(tag))
				{
					return EquipmentSlotGroups.Head;
				}
				if ("E_BP_Body".Equals(tag))
				{
					return EquipmentSlotGroups.UpperBody;
				}
				if ("E_BP_LLeg".Equals(tag))
				{
					return EquipmentSlotGroups.LowerBody;
				}
				if ("E_BP_RLeg".Equals(tag))
				{
					return EquipmentSlotGroups.LowerBody;
				}
				if ("E_BP_LArm".Equals(tag))
				{
					return EquipmentSlotGroups.UpperBody;
				}
				"E_BP_RArm".Equals(tag);
				return EquipmentSlotGroups.UpperBody;
			}
		}
		return EquipmentSlotGroups.UpperBody;
	}

	public EnumBodyPartHit GetEntityDamageBodyPart(Entity entity)
	{
		if (this.bodyParts != EnumBodyPartHit.None)
		{
			return this.bodyParts;
		}
		if (entity.emodel)
		{
			Transform hitTransform = entity.emodel.GetHitTransform(this);
			if (hitTransform)
			{
				return DamageSource.TagToBodyPart(hitTransform.tag);
			}
		}
		return EnumBodyPartHit.None;
	}

	public static EnumBodyPartHit TagToBodyPart(string _name)
	{
		uint num = <PrivateImplementationDetails>.ComputeStringHash(_name);
		if (num <= 1181961453U)
		{
			if (num <= 719580451U)
			{
				if (num != 265341418U)
				{
					if (num == 719580451U)
					{
						if (_name == "E_BP_Special")
						{
							return EnumBodyPartHit.Special;
						}
					}
				}
				else if (_name == "E_BP_Head")
				{
					return EnumBodyPartHit.Head;
				}
			}
			else if (num != 769391494U)
			{
				if (num != 1103658916U)
				{
					if (num == 1181961453U)
					{
						if (_name == "E_BP_RLowerLeg")
						{
							return EnumBodyPartHit.RightLowerLeg;
						}
					}
				}
				else if (_name == "E_BP_Body")
				{
					return EnumBodyPartHit.Torso;
				}
			}
			else if (_name == "E_BP_LArm")
			{
				return EnumBodyPartHit.LeftUpperArm;
			}
		}
		else if (num <= 2493191411U)
		{
			if (num != 1478707584U)
			{
				if (num != 2129509723U)
				{
					if (num == 2493191411U)
					{
						if (_name == "E_BP_RLowerArm")
						{
							return EnumBodyPartHit.RightLowerArm;
						}
					}
				}
				else if (_name == "E_BP_LLowerLeg")
				{
					return EnumBodyPartHit.LeftLowerLeg;
				}
			}
			else if (_name == "E_BP_RArm")
			{
				return EnumBodyPartHit.RightUpperArm;
			}
		}
		else if (num != 2661128377U)
		{
			if (num != 2886638712U)
			{
				if (num == 3661196970U)
				{
					if (_name == "E_BP_RLeg")
					{
						return EnumBodyPartHit.RightUpperLeg;
					}
				}
			}
			else if (_name == "E_BP_LLeg")
			{
				return EnumBodyPartHit.LeftUpperLeg;
			}
		}
		else if (_name == "E_BP_LLowerArm")
		{
			return EnumBodyPartHit.LeftLowerArm;
		}
		return EnumBodyPartHit.None;
	}

	public void GetEntityDamageBodyPartAndEquipmentSlot(Entity entity, out EnumBodyPartHit bodyPartHit, out EquipmentSlots damageSlot)
	{
		damageSlot = EquipmentSlots.Count;
		bodyPartHit = EnumBodyPartHit.None;
		if (entity.emodel)
		{
			Transform hitTransform = entity.emodel.GetHitTransform(this);
			if (hitTransform)
			{
				string tag = hitTransform.tag;
				if ("E_BP_Head".Equals(tag))
				{
					damageSlot = EquipmentSlots.Head;
					bodyPartHit = EnumBodyPartHit.Head;
					return;
				}
				if ("E_BP_Body".Equals(tag))
				{
					damageSlot = EquipmentSlots.Chest;
					bodyPartHit = EnumBodyPartHit.Torso;
					return;
				}
				if ("E_BP_LLeg".Equals(tag))
				{
					damageSlot = EquipmentSlots.Chest;
					bodyPartHit = EnumBodyPartHit.LeftUpperLeg;
					return;
				}
				if ("E_BP_LLowerLeg".Equals(tag))
				{
					damageSlot = EquipmentSlots.Feet;
					bodyPartHit = EnumBodyPartHit.LeftLowerLeg;
					return;
				}
				if ("E_BP_RLeg".Equals(tag))
				{
					damageSlot = EquipmentSlots.Chest;
					bodyPartHit = EnumBodyPartHit.RightUpperLeg;
					return;
				}
				if ("E_BP_RLowerLeg".Equals(tag))
				{
					damageSlot = EquipmentSlots.Feet;
					bodyPartHit = EnumBodyPartHit.RightLowerLeg;
					return;
				}
				if ("E_BP_LArm".Equals(tag))
				{
					damageSlot = EquipmentSlots.Hands;
					bodyPartHit = EnumBodyPartHit.LeftUpperArm;
					return;
				}
				if ("E_BP_LLowerArm".Equals(tag))
				{
					damageSlot = EquipmentSlots.Hands;
					bodyPartHit = EnumBodyPartHit.LeftLowerArm;
					return;
				}
				if ("E_BP_RArm".Equals(tag))
				{
					damageSlot = EquipmentSlots.Hands;
					bodyPartHit = EnumBodyPartHit.RightUpperArm;
					return;
				}
				if ("E_BP_RLowerArm".Equals(tag))
				{
					damageSlot = EquipmentSlots.Hands;
					bodyPartHit = EnumBodyPartHit.RightLowerArm;
					return;
				}
			}
		}
		else
		{
			if (this.damageType == EnumDamageTypes.Falling)
			{
				bodyPartHit = EnumBodyPartHit.RightLowerLeg;
				damageSlot = EquipmentSlots.Feet;
				return;
			}
			bodyPartHit = EnumBodyPartHit.Torso;
			damageSlot = EquipmentSlots.Chest;
		}
	}

	public EquipmentSlots GetDamagedEquipmentSlot(Entity entity)
	{
		if (entity.emodel)
		{
			Transform hitTransform = entity.emodel.GetHitTransform(this);
			if (hitTransform)
			{
				string tag = hitTransform.tag;
				uint num = <PrivateImplementationDetails>.ComputeStringHash(tag);
				if (num <= 1478707584U)
				{
					if (num <= 769391494U)
					{
						if (num != 265341418U)
						{
							if (num == 769391494U)
							{
								if (tag == "E_BP_LArm")
								{
									return EquipmentSlots.Chest;
								}
							}
						}
						else if (tag == "E_BP_Head")
						{
							return EquipmentSlots.Head;
						}
					}
					else if (num != 1103658916U)
					{
						if (num != 1181961453U)
						{
							if (num == 1478707584U)
							{
								if (tag == "E_BP_RArm")
								{
									return EquipmentSlots.Chest;
								}
							}
						}
						else if (tag == "E_BP_RLowerLeg")
						{
							return EquipmentSlots.Feet;
						}
					}
					else if (tag == "E_BP_Body")
					{
						return EquipmentSlots.Chest;
					}
				}
				else if (num <= 2493191411U)
				{
					if (num != 2129509723U)
					{
						if (num == 2493191411U)
						{
							if (tag == "E_BP_RLowerArm")
							{
								return EquipmentSlots.Hands;
							}
						}
					}
					else if (tag == "E_BP_LLowerLeg")
					{
						return EquipmentSlots.Feet;
					}
				}
				else if (num != 2661128377U)
				{
					if (num != 2886638712U)
					{
						if (num == 3661196970U)
						{
							if (tag == "E_BP_RLeg")
							{
								return EquipmentSlots.Chest;
							}
						}
					}
					else if (tag == "E_BP_LLeg")
					{
						return EquipmentSlots.Chest;
					}
				}
				else if (tag == "E_BP_LLowerArm")
				{
					return EquipmentSlots.Hands;
				}
				return EquipmentSlots.Chest;
			}
		}
		else if (this.damageType == EnumDamageTypes.Falling)
		{
			return EquipmentSlots.Feet;
		}
		return EquipmentSlots.Chest;
	}

	public virtual Vector3 getDirection()
	{
		return this.direction;
	}

	public virtual int getEntityId()
	{
		return this.ownerEntityId;
	}

	public virtual string getHitTransformName()
	{
		return null;
	}

	public virtual Vector3 getHitTransformPosition()
	{
		return Vector3.zero;
	}

	public virtual Vector2 getUVHit()
	{
		return Vector2.zero;
	}

	public virtual EnumDamageSource GetSource()
	{
		return this.damageSource;
	}

	public virtual EnumDamageTypes GetDamageType()
	{
		return this.damageType;
	}

	public bool CanStun
	{
		get
		{
			return this.damageType == EnumDamageTypes.Bashing || this.damageType == EnumDamageTypes.Heat || this.damageType == EnumDamageTypes.Piercing || this.damageType == EnumDamageTypes.Crushing || this.damageType == EnumDamageTypes.Falling;
		}
	}

	public void SetIgnoreConsecutiveDamages(bool _b)
	{
		this.bIgnoreConsecutiveDamages = _b;
	}

	public virtual bool IsIgnoreConsecutiveDamages()
	{
		return this.bIgnoreConsecutiveDamages;
	}

	public float DamageMultiplier
	{
		get
		{
			return this.damageMultiplier;
		}
		set
		{
			this.damageMultiplier = value;
		}
	}

	public static readonly DamageSource eat = new DamageSource(EnumDamageSource.External, EnumDamageTypes.Slashing);

	public static readonly DamageSource fallingBlock = new DamageSource(EnumDamageSource.External, EnumDamageTypes.Crushing);

	public static readonly DamageSource radiation = new DamageSource(EnumDamageSource.External, EnumDamageTypes.Radiation);

	public static readonly DamageSource fall = new DamageSource(EnumDamageSource.Internal, EnumDamageTypes.Falling);

	public static readonly DamageSource starve = new DamageSource(EnumDamageSource.Internal, EnumDamageTypes.Starvation);

	public static readonly DamageSource dehydrate = new DamageSource(EnumDamageSource.Internal, EnumDamageTypes.Dehydration);

	public static readonly DamageSource radiationSickness = new DamageSource(EnumDamageSource.Internal, EnumDamageTypes.Radiation);

	public static readonly DamageSource disease = new DamageSource(EnumDamageSource.Internal, EnumDamageTypes.Disease);

	public static readonly DamageSource suffocating = new DamageSource(EnumDamageSource.Internal, EnumDamageTypes.Suffocation);

	public BuffClass BuffClass;

	public ItemValue AttackingItem;

	public EnumDamageSource damageSource;

	public readonly EnumDamageTypes damageType;

	public EnumBodyPartHit bodyParts;

	public float DismemberChance;

	public EnumDamageBonusType BonusDamageType;

	public bool canHitSpecialBodyParts;

	public FastTags<TagGroup.Global> DamageTypeTag;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bIgnoreConsecutiveDamages;

	[PublicizedFrom(EAccessModifier.Private)]
	public float damageMultiplier = 1f;

	[PublicizedFrom(EAccessModifier.Protected)]
	public int ownerEntityId = -1;

	public int CreatorEntityId = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 direction;

	public Vector3i BlockPosition;
}
