using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageDamageEntity : NetPackage
{
	public NetPackageDamageEntity Setup(int _targetEntityId, DamageResponse _dmResponse)
	{
		this.entityId = _targetEntityId;
		DamageSource source = _dmResponse.Source;
		this.damageSrc = source.GetSource();
		this.damageTyp = source.GetDamageType();
		this.attackingItem = source.AttackingItem;
		int num = _dmResponse.Strength;
		if (num > 65535)
		{
			num = 65535;
		}
		this.strength = (ushort)num;
		this.hitDirection = (int)_dmResponse.HitDirection;
		this.hitBodyPart = (short)_dmResponse.HitBodyPart;
		this.movementState = _dmResponse.MovementState;
		this.bPainHit = _dmResponse.PainHit;
		this.bFatal = _dmResponse.Fatal;
		this.bCritical = _dmResponse.Critical;
		this.attackerEntityId = source.getEntityId();
		this.dirV = source.getDirection();
		this.blockPos = source.BlockPosition;
		this.hitTransformName = source.getHitTransformName() ?? string.Empty;
		this.hitTransformPosition = source.getHitTransformPosition();
		this.uvHit = source.getUVHit();
		this.damageMultiplier = source.DamageMultiplier;
		this.bonusDamageType = (byte)source.BonusDamageType;
		this.random = _dmResponse.Random;
		this.bIgnoreConsecutiveDamages = source.IsIgnoreConsecutiveDamages();
		this.bDismember = _dmResponse.Dismember;
		this.bCrippleLegs = _dmResponse.CrippleLegs;
		this.bTurnIntoCrawler = _dmResponse.TurnIntoCrawler;
		this.StunType = (byte)_dmResponse.Stun;
		this.StunDuration = _dmResponse.StunDuration;
		this.ArmorSlot = _dmResponse.ArmorSlot;
		this.ArmorSlotGroup = _dmResponse.ArmorSlotGroup;
		this.ArmorDamage = _dmResponse.ArmorDamage;
		this.bFromBuff = source.BuffClass != null;
		return this;
	}

	public override void read(PooledBinaryReader _reader)
	{
		this.entityId = _reader.ReadInt32();
		this.damageSrc = (EnumDamageSource)_reader.ReadByte();
		this.damageTyp = (EnumDamageTypes)_reader.ReadByte();
		this.strength = _reader.ReadUInt16();
		this.hitDirection = (int)_reader.ReadByte();
		this.hitBodyPart = _reader.ReadInt16();
		this.movementState = (int)_reader.ReadByte();
		this.bPainHit = _reader.ReadBoolean();
		this.bFatal = _reader.ReadBoolean();
		this.bCritical = _reader.ReadBoolean();
		this.attackerEntityId = _reader.ReadInt32();
		this.dirV.x = _reader.ReadSingle();
		this.dirV.y = _reader.ReadSingle();
		this.dirV.z = _reader.ReadSingle();
		this.blockPos = StreamUtils.ReadVector3i(_reader);
		this.hitTransformName = _reader.ReadString();
		this.hitTransformPosition.x = _reader.ReadSingle();
		this.hitTransformPosition.y = _reader.ReadSingle();
		this.hitTransformPosition.z = _reader.ReadSingle();
		this.uvHit.x = _reader.ReadSingle();
		this.uvHit.y = _reader.ReadSingle();
		this.damageMultiplier = _reader.ReadSingle();
		this.random = _reader.ReadSingle();
		this.bIgnoreConsecutiveDamages = _reader.ReadBoolean();
		this.bIsDamageTransfer = _reader.ReadBoolean();
		this.bDismember = _reader.ReadBoolean();
		this.bCrippleLegs = _reader.ReadBoolean();
		this.bTurnIntoCrawler = _reader.ReadBoolean();
		this.bonusDamageType = _reader.ReadByte();
		this.StunType = _reader.ReadByte();
		this.StunDuration = _reader.ReadSingle();
		this.bFromBuff = _reader.ReadBoolean();
		this.ArmorSlot = (EquipmentSlots)_reader.ReadByte();
		this.ArmorSlotGroup = (EquipmentSlotGroups)_reader.ReadByte();
		this.ArmorDamage = (int)_reader.ReadInt16();
		if (_reader.ReadBoolean())
		{
			this.attackingItem = new ItemValue();
			this.attackingItem.Read(_reader);
		}
	}

	public override void write(PooledBinaryWriter _writer)
	{
		base.write(_writer);
		_writer.Write(this.entityId);
		_writer.Write((byte)this.damageSrc);
		_writer.Write((byte)this.damageTyp);
		_writer.Write(this.strength);
		_writer.Write((byte)this.hitDirection);
		_writer.Write(this.hitBodyPart);
		_writer.Write((byte)this.movementState);
		_writer.Write(this.bPainHit);
		_writer.Write(this.bFatal);
		_writer.Write(this.bCritical);
		_writer.Write(this.attackerEntityId);
		_writer.Write(this.dirV.x);
		_writer.Write(this.dirV.y);
		_writer.Write(this.dirV.z);
		StreamUtils.Write(_writer, this.blockPos);
		_writer.Write(this.hitTransformName);
		_writer.Write(this.hitTransformPosition.x);
		_writer.Write(this.hitTransformPosition.y);
		_writer.Write(this.hitTransformPosition.z);
		_writer.Write(this.uvHit.x);
		_writer.Write(this.uvHit.y);
		_writer.Write(this.damageMultiplier);
		_writer.Write(this.random);
		_writer.Write(this.bIgnoreConsecutiveDamages);
		_writer.Write(this.bIsDamageTransfer);
		_writer.Write(this.bDismember);
		_writer.Write(this.bCrippleLegs);
		_writer.Write(this.bTurnIntoCrawler);
		_writer.Write(this.bonusDamageType);
		_writer.Write(this.StunType);
		_writer.Write(this.StunDuration);
		_writer.Write(this.bFromBuff);
		_writer.Write((byte)this.ArmorSlot);
		_writer.Write((byte)this.ArmorSlotGroup);
		_writer.Write((ushort)this.ArmorDamage);
		_writer.Write(this.attackingItem != null);
		if (this.attackingItem != null)
		{
			this.attackingItem.Write(_writer);
		}
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world == null)
		{
			return;
		}
		if (_world.GetPrimaryPlayer() != null && _world.GetPrimaryPlayer().entityId == this.entityId)
		{
			if (this.damageTyp == EnumDamageTypes.Falling)
			{
				return;
			}
			if (this.damageSrc == EnumDamageSource.External && (this.damageTyp == EnumDamageTypes.Piercing || this.damageTyp == EnumDamageTypes.BarbedWire) && this.attackerEntityId == -1)
			{
				return;
			}
		}
		Entity entity = _world.GetEntity(this.entityId);
		if (entity != null)
		{
			DamageSource damageSource = new DamageSourceEntity(this.damageSrc, this.damageTyp, this.attackerEntityId, this.dirV, this.hitTransformName, this.hitTransformPosition, this.uvHit);
			damageSource.SetIgnoreConsecutiveDamages(this.bIgnoreConsecutiveDamages);
			damageSource.DamageMultiplier = this.damageMultiplier;
			damageSource.BonusDamageType = (EnumDamageBonusType)this.bonusDamageType;
			damageSource.AttackingItem = this.attackingItem;
			damageSource.BlockPosition = this.blockPos;
			DamageResponse damageResponse = default(DamageResponse);
			damageResponse.Strength = (int)this.strength;
			damageResponse.ModStrength = 0;
			damageResponse.MovementState = this.movementState;
			damageResponse.HitDirection = (Utils.EnumHitDirection)this.hitDirection;
			damageResponse.HitBodyPart = (EnumBodyPartHit)this.hitBodyPart;
			damageResponse.PainHit = this.bPainHit;
			damageResponse.Fatal = this.bFatal;
			damageResponse.Critical = this.bCritical;
			damageResponse.Random = this.random;
			damageResponse.Source = damageSource;
			damageResponse.CrippleLegs = this.bCrippleLegs;
			damageResponse.Dismember = this.bDismember;
			damageResponse.TurnIntoCrawler = this.bTurnIntoCrawler;
			damageResponse.Stun = (EnumEntityStunType)this.StunType;
			damageResponse.StunDuration = this.StunDuration;
			damageResponse.ArmorSlot = this.ArmorSlot;
			damageResponse.ArmorSlotGroup = this.ArmorSlotGroup;
			damageResponse.ArmorDamage = this.ArmorDamage;
			if (this.bFromBuff)
			{
				damageResponse.Source.BuffClass = new BuffClass("");
			}
			entity.FireAttackedEvents(damageResponse);
			entity.ProcessDamageResponse(damageResponse);
		}
	}

	public override int GetLength()
	{
		return 50;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int entityId;

	[PublicizedFrom(EAccessModifier.Private)]
	public int attackerEntityId;

	[PublicizedFrom(EAccessModifier.Private)]
	public EnumDamageSource damageSrc;

	[PublicizedFrom(EAccessModifier.Private)]
	public EnumDamageTypes damageTyp;

	[PublicizedFrom(EAccessModifier.Private)]
	public ushort strength;

	[PublicizedFrom(EAccessModifier.Private)]
	public int hitDirection;

	[PublicizedFrom(EAccessModifier.Private)]
	public short hitBodyPart;

	[PublicizedFrom(EAccessModifier.Private)]
	public int movementState;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 dirV;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3i blockPos;

	[PublicizedFrom(EAccessModifier.Private)]
	public string hitTransformName;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 hitTransformPosition;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2 uvHit;

	[PublicizedFrom(EAccessModifier.Private)]
	public float damageMultiplier;

	[PublicizedFrom(EAccessModifier.Private)]
	public float random;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bIgnoreConsecutiveDamages;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bPainHit;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bFatal;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bCritical;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bIsDamageTransfer;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bDismember;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bCrippleLegs;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bTurnIntoCrawler;

	[PublicizedFrom(EAccessModifier.Private)]
	public byte bonusDamageType;

	[PublicizedFrom(EAccessModifier.Private)]
	public byte StunType;

	[PublicizedFrom(EAccessModifier.Private)]
	public float StunDuration;

	[PublicizedFrom(EAccessModifier.Private)]
	public EquipmentSlots ArmorSlot;

	[PublicizedFrom(EAccessModifier.Private)]
	public EquipmentSlotGroups ArmorSlotGroup;

	[PublicizedFrom(EAccessModifier.Private)]
	public int ArmorDamage;

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemValue attackingItem;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bFromBuff;
}
