using System;
using System.Collections.Generic;
using System.Diagnostics;
using Twitch;
using UnityEngine;

public class Explosion
{
	public Explosion(World _world, Vector3 _worldPos, Vector3i _blockPos, ExplosionData _explosionData, int _entityId)
	{
		this.world = _world;
		this.worldPos = _worldPos;
		this.blockPos = _blockPos;
		this.explosionData = _explosionData;
		this.entityId = _entityId;
		this.damagedBlockPositions = new HashSet<Vector3i>();
		this.ChangedBlockPositions = new Dictionary<Vector3i, BlockChangeInfo>();
	}

	public void AttackBlocks(int _entityThatCausedExplosion, ItemValue _itemValueExplosionSource)
	{
		ChunkCluster chunkCache = this.world.ChunkCache;
		if (chunkCache == null)
		{
			return;
		}
		EntityAlive entityAlive = this.world.GetEntity(_entityThatCausedExplosion) as EntityAlive;
		PersistentPlayerData playerDataFromEntityID = GameManager.Instance.persistentPlayers.GetPlayerDataFromEntityID(_entityThatCausedExplosion);
		FastTags<TagGroup.Global> fastTags = Explosion.explosionTag;
		if (_itemValueExplosionSource != null)
		{
			ItemClass itemClass = _itemValueExplosionSource.ItemClass;
			fastTags |= itemClass.ItemTags;
		}
		float num = EffectManager.GetValue(PassiveEffects.ExplosionRadius, _itemValueExplosionSource, this.explosionData.BlockRadius, entityAlive, null, fastTags, true, true, true, true, true, 1, true, false);
		if (num == 0f)
		{
			num = 0.01f;
		}
		int num2 = Mathf.CeilToInt(num);
		FastTags<TagGroup.Global> fastTags2 = ((this.explosionData.BlockTags.Length > 0) ? FastTags<TagGroup.Global>.Parse(this.explosionData.BlockTags) : FastTags<TagGroup.Global>.none);
		bool flag = !fastTags2.IsEmpty;
		if (chunkCache.GetBlock(this.blockPos).Block.shape.IsTerrain() && !chunkCache.GetBlock(this.blockPos + Vector3i.up).Block.shape.IsTerrain())
		{
			this.blockPos.y = this.blockPos.y + 1;
		}
		Vector3 vector = this.blockPos.ToVector3Center();
		bool flag2 = false;
		for (int i = -num2; i <= num2; i++)
		{
			for (int j = -num2; j <= num2; j++)
			{
				for (int k = -num2; k <= num2; k++)
				{
					Vector3 vector2 = vector;
					Vector3 vector3;
					vector3..ctor((float)i, (float)j, (float)k);
					float num3 = vector3.magnitude + 0.001f;
					vector3 *= 0.51f / num3;
					while (num3 >= -0.1f)
					{
						Vector3i vector3i = World.worldToBlockPos(vector2);
						BlockValue blockValue = chunkCache.GetBlock(vector3i);
						if (!blockValue.isair && !blockValue.isWater)
						{
							if (this.damagedBlockPositions.Contains(vector3i))
							{
								break;
							}
							Block block = blockValue.Block;
							if (!block.StabilityIgnore)
							{
								if (vector3i != this.blockPos)
								{
									this.damagedBlockPositions.Add(vector3i);
								}
								else
								{
									if (flag2)
									{
										goto IL_05F8;
									}
									flag2 = true;
								}
								float num4 = (vector3i.ToVector3Center() - this.worldPos).magnitude - 0.5f;
								if (num4 < 0f)
								{
									num4 = 0f;
								}
								FastTags<TagGroup.Global> fastTags3 = fastTags;
								fastTags3 |= block.Tags;
								float num5 = EffectManager.GetValue(PassiveEffects.ExplosionBlockDamage, _itemValueExplosionSource, this.explosionData.BlockDamage, entityAlive, null, fastTags3, true, true, true, true, true, 1, true, false);
								if (entityAlive)
								{
									num5 = num5 * entityAlive.GetBlockDamageScale(blockValue.isTerrain) + 0.5f;
								}
								if (Utils.FastMax(num5, 1f) / (float)(2 * num2 + 1) > 0f)
								{
									float num6 = (1f - num4 / num) * num5;
									if (flag && block.Tags.Test_AnySet(fastTags2) && this.explosionData.damageMultiplier != null)
									{
										num6 *= this.explosionData.damageMultiplier.Get("tags");
									}
									if (num6 <= 0f)
									{
										break;
									}
									if (blockValue.ischild)
									{
										vector3i = block.multiBlockPos.GetParentPos(vector3i, blockValue);
										blockValue = this.world.GetBlock(vector3i);
										block = blockValue.Block;
									}
									BlockChangeInfo blockChangeInfo;
									if (!this.ChangedBlockPositions.TryGetValue(vector3i, out blockChangeInfo))
									{
										blockChangeInfo = new BlockChangeInfo(vector3i, blockValue);
										blockChangeInfo.bChangeDamage = true;
										if (blockChangeInfo.blockValue.isWater)
										{
											goto IL_05F8;
										}
										this.ChangedBlockPositions[vector3i] = blockChangeInfo;
									}
									if (!blockChangeInfo.blockValue.isair && (World.SandboxUseTraderArea != TraderAreaStates.Default || !this.world.IsWithinTraderArea(vector3i)) && this.world.InBoundsForPlayersPercent(vector3i.ToVector3CenterXZ()) >= 0.5f)
									{
										Block block2 = blockChangeInfo.blockValue.Block;
										float explosionResistance = block2.GetExplosionResistance();
										float hardness = block2.GetHardness();
										float landProtectionHardnessModifier = this.world.GetLandProtectionHardnessModifier(vector3i, entityAlive, playerDataFromEntityID);
										int num7 = 0;
										if (hardness > 0f)
										{
											float num8 = (1f - explosionResistance) * num6 / (hardness * landProtectionHardnessModifier);
											if (this.explosionData.damageMultiplier != null)
											{
												num8 *= this.explosionData.damageMultiplier.Get(block2.blockMaterial.DamageCategory);
											}
											num7 = Mathf.RoundToInt(num8);
										}
										else if (landProtectionHardnessModifier > 0f)
										{
											num7 = Mathf.RoundToInt((float)block.MaxDamage / landProtectionHardnessModifier);
										}
										if (num7 > 0)
										{
											if (num7 + blockChangeInfo.blockValue.damage >= block.MaxDamage)
											{
												num7 = num7 + blockChangeInfo.blockValue.damage - block.MaxDamage;
												blockChangeInfo.bChangeDamage = false;
												Block.DestroyedResult destroyedResult = block2.OnBlockDestroyedByExplosion(this.world, vector3i, blockChangeInfo.blockValue, this.entityId);
												if (!block2.DowngradeBlock.isair && destroyedResult == Block.DestroyedResult.Downgrade)
												{
													do
													{
														BlockValue blockValue2 = blockChangeInfo.blockValue.Block.DowngradeBlock;
														blockValue2 = BlockPlaceholderMap.Instance.Replace(blockValue2, this.world.GetGameRandom(), vector3i.x, vector3i.z, false);
														blockValue2.rotation = blockChangeInfo.blockValue.rotation;
														blockValue2.meta = blockChangeInfo.blockValue.meta;
														blockChangeInfo.blockValue = blockValue2;
														blockChangeInfo.blockValue.damage = num7;
														num7 -= blockChangeInfo.blockValue.Block.MaxDamage;
													}
													while (num7 > 0 && !blockChangeInfo.blockValue.Block.DowngradeBlock.isair);
													if (num7 >= 0)
													{
														blockChangeInfo.blockValue = BlockValue.Air;
													}
												}
												else
												{
													blockChangeInfo.blockValue = BlockValue.Air;
												}
												if (!blockChangeInfo.blockValue.isair)
												{
													break;
												}
												this.damagedBlockPositions.Remove(vector3i);
											}
											else
											{
												blockChangeInfo.blockValue.damage = num7 + blockChangeInfo.blockValue.damage;
											}
											if (!blockChangeInfo.blockValue.isair)
											{
												break;
											}
										}
									}
								}
							}
						}
						IL_05F8:
						num3 -= 0.51f;
						vector2 += vector3;
					}
				}
			}
		}
	}

	public void AttackEntites(int _entityThatCausedExplosion, ItemValue _itemValueExplosionSource, EnumDamageTypes damageType)
	{
		EntityAlive entityAlive = this.world.GetEntity(_entityThatCausedExplosion) as EntityAlive;
		FastTags<TagGroup.Global> fastTags = Explosion.explosionTag;
		if (_itemValueExplosionSource != null)
		{
			ItemClass itemClass = _itemValueExplosionSource.ItemClass;
			fastTags |= itemClass.ItemTags;
		}
		float value = EffectManager.GetValue(PassiveEffects.ExplosionEntityDamage, _itemValueExplosionSource, this.explosionData.EntityDamage, entityAlive, null, fastTags, true, true, true, true, true, 1, true, false);
		float value2 = EffectManager.GetValue(PassiveEffects.ExplosionRadius, _itemValueExplosionSource, (float)this.explosionData.EntityRadius, entityAlive, null, fastTags, true, true, true, true, true, 1, true, false);
		Collider[] array = Physics.OverlapSphere(this.worldPos - Origin.position, value2, -538480653);
		bool flag = false;
		Vector3i vector3i = World.worldToBlockPos(this.worldPos);
		foreach (Collider collider in array)
		{
			if (collider)
			{
				Transform transform = collider.transform;
				if (transform.CompareTag("Item"))
				{
					EntityItem entityItem = transform.GetComponent<EntityItem>();
					if (!entityItem)
					{
						RootTransformRefEntity component = transform.GetComponent<RootTransformRefEntity>();
						if (component)
						{
							entityItem = component.RootTransform.GetComponent<EntityItem>();
						}
					}
					if (entityItem && !entityItem.IsDead() && !Explosion.hitEntities.ContainsKey(entityItem.entityId))
					{
						Explosion.hitEntities.Add(entityItem.entityId, default(Explosion.DamageRecord));
						entityItem.OnDamagedByExplosion();
						entityItem.SetDead();
					}
				}
				else
				{
					string tag = transform.tag;
					if (tag.StartsWith("E_BP_"))
					{
						flag = true;
						Transform hitRootTransform = GameUtils.GetHitRootTransform(tag, transform);
						EntityAlive entityAlive2 = (hitRootTransform ? hitRootTransform.GetComponent<EntityAlive>() : null);
						if (entityAlive2)
						{
							entityAlive2.ConditionalTriggerSleeperWakeUp();
							Vector3 vector = transform.position + Origin.position - this.worldPos;
							float magnitude = vector.magnitude;
							vector.Normalize();
							if (!Voxel.Raycast(this.world, new Ray(this.worldPos, vector), magnitude, 65536, 66, 0f))
							{
								EntityClass entityClass = EntityClass.list[entityAlive2.entityClass];
								float num;
								if (transform.CompareTag("E_BP_LArm") || transform.CompareTag("E_BP_RArm"))
								{
									num = entityClass.ArmsExplosionDamageMultiplier;
								}
								else if (transform.CompareTag("E_BP_LLeg") || transform.CompareTag("E_BP_RLeg"))
								{
									num = entityClass.LegsExplosionDamageMultiplier;
								}
								else if (transform.CompareTag("E_BP_Head"))
								{
									num = entityClass.HeadExplosionDamageMultiplier;
								}
								else
								{
									num = entityClass.ChestExplosionDamageMultiplier;
								}
								float num2 = value * num;
								num2 *= 1f - magnitude / value2;
								num2 = (float)((int)EffectManager.GetValue(PassiveEffects.ExplosionIncomingDamage, null, num2, entityAlive2, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false));
								if (num2 >= 3f)
								{
									Explosion.DamageRecord damageRecord;
									if (!Explosion.hitEntities.TryGetValue(entityAlive2.entityId, out damageRecord))
									{
										damageRecord.entity = entityAlive2;
									}
									EnumBodyPartHit enumBodyPartHit = DamageSource.TagToBodyPart(tag);
									if (num2 > damageRecord.damage)
									{
										damageRecord.damage = num2;
										damageRecord.dir = vector;
										damageRecord.hitTransform = transform;
										damageRecord.mainPart = enumBodyPartHit;
									}
									damageRecord.parts |= enumBodyPartHit;
									Explosion.hitEntities[entityAlive2.entityId] = damageRecord;
								}
							}
						}
					}
				}
			}
		}
		int num3 = 0;
		int num4 = 0;
		float num5 = (float)Explosion.hitEntities.Count * 0.8f;
		foreach (KeyValuePair<int, Explosion.DamageRecord> keyValuePair in Explosion.hitEntities)
		{
			EntityAlive entity = keyValuePair.Value.entity;
			if (entity != null)
			{
				bool flag2 = entity.IsDead();
				int health = entity.Health;
				bool flag3 = keyValuePair.Value.damage > (float)entity.GetMaxHealth() * 0.1f;
				EnumBodyPartHit mainPart = keyValuePair.Value.mainPart;
				EnumBodyPartHit enumBodyPartHit2 = keyValuePair.Value.parts;
				enumBodyPartHit2 &= ~mainPart;
				float num6 = (flag2 ? 0.85f : 0.6f);
				for (int j = 0; j < 11; j++)
				{
					int num7 = 1 << j;
					if ((enumBodyPartHit2 & (EnumBodyPartHit)num7) != EnumBodyPartHit.None && entity.rand.RandomFloat <= num6)
					{
						enumBodyPartHit2 &= (EnumBodyPartHit)(~(EnumBodyPartHit)num7);
					}
				}
				if (entity is EntityEnemy && ((float)num3 >= 4f || flag2))
				{
					if ((entity.position - this.worldPos).sqrMagnitude < value2 * 0.67f)
					{
						entity.canDisintegrate = true;
					}
					else if ((float)num4 < num5)
					{
						entity.canDisintegrate = true;
					}
				}
				entity.DamageEntity(new DamageSourceEntity(EnumDamageSource.External, damageType, _entityThatCausedExplosion, keyValuePair.Value.dir, keyValuePair.Value.hitTransform.name, Vector3.zero, Vector2.zero)
				{
					bodyParts = (mainPart | enumBodyPartHit2),
					AttackingItem = _itemValueExplosionSource,
					DismemberChance = (flag3 ? 0.5f : 0f),
					BlockPosition = vector3i
				}, (int)keyValuePair.Value.damage, flag3, 1f);
				if (entity.isDisintegrated)
				{
					num4++;
				}
				if (entityAlive != null)
				{
					MinEventTypes minEventTypes = ((health != entity.Health) ? MinEventTypes.onSelfExplosionDamagedOther : MinEventTypes.onSelfExplosionAttackedOther);
					entityAlive.MinEventContext.Self = entityAlive;
					entityAlive.MinEventContext.Other = entity;
					entityAlive.MinEventContext.ItemValue = _itemValueExplosionSource;
					if (_itemValueExplosionSource != null)
					{
						_itemValueExplosionSource.FireEvent(minEventTypes, entityAlive.MinEventContext);
					}
					entityAlive.FireEvent(minEventTypes, false);
				}
				if (this.explosionData.BuffActions != null)
				{
					for (int k = 0; k < this.explosionData.BuffActions.Count; k++)
					{
						BuffClass buff = BuffManager.GetBuff(this.explosionData.BuffActions[k]);
						if (buff != null)
						{
							float num8 = 1f;
							num8 = EffectManager.GetValue(PassiveEffects.BuffProcChance, null, num8, entityAlive, null, FastTags<TagGroup.Global>.Parse(buff.Name), true, true, true, true, true, 1, true, false);
							if (entity.rand.RandomFloat <= num8)
							{
								entity.Buffs.AddBuff(this.explosionData.BuffActions[k], _entityThatCausedExplosion, true, false, -1f);
							}
						}
					}
				}
				if (!flag2 && entity.IsDead())
				{
					num3++;
					EntityPlayer entityPlayer = entityAlive as EntityPlayer;
					if (entityPlayer)
					{
						entityPlayer.AddKillXP(entity, _itemValueExplosionSource, 1f);
					}
					EntityPlayer entityPlayer2 = entity as EntityPlayer;
					if (entityPlayer2 != null)
					{
						TwitchManager.Current.CheckKiller(entityPlayer2, entityAlive, vector3i);
					}
					if (entityAlive != null)
					{
						if (entityAlive.isEntityRemote)
						{
							SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageMinEventFire>().Setup(entityAlive.entityId, entity.entityId, MinEventTypes.onSelfKilledOther, _itemValueExplosionSource), false, entityAlive.entityId, -1, -1, null, 192, false);
						}
						else
						{
							entityAlive.FireEvent(MinEventTypes.onSelfKilledOther, true);
						}
					}
				}
			}
		}
		if (!flag && entityAlive != null)
		{
			if (entityAlive.isEntityRemote)
			{
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageMinEventFire>().Setup(entityAlive.entityId, -1, MinEventTypes.onSelfExplosionMissEntity, _itemValueExplosionSource), false, entityAlive.entityId, -1, -1, null, 192, false);
			}
			else
			{
				entityAlive.MinEventContext.Self = entityAlive;
				entityAlive.MinEventContext.Other = null;
				entityAlive.MinEventContext.ItemValue = _itemValueExplosionSource;
				entityAlive.FireEvent(MinEventTypes.onSelfExplosionMissEntity, true);
			}
		}
		Explosion.hitEntities.Clear();
	}

	[Conditional("DEBUG_EXPLOSION_LOG")]
	[PublicizedFrom(EAccessModifier.Private)]
	public static void log(string format, params object[] args)
	{
		format = string.Format("{0} {1} Explosion {2}", GameManager.frameTime.ToCultureInvariantString(), GameManager.frameCount, format);
		Log.Warning(format, args);
	}

	public Dictionary<Vector3i, BlockChangeInfo> ChangedBlockPositions;

	[PublicizedFrom(EAccessModifier.Private)]
	public World world;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 worldPos;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3i blockPos;

	[PublicizedFrom(EAccessModifier.Private)]
	public int entityId;

	[PublicizedFrom(EAccessModifier.Private)]
	public ExplosionData explosionData;

	[PublicizedFrom(EAccessModifier.Private)]
	public HashSet<Vector3i> damagedBlockPositions;

	[PublicizedFrom(EAccessModifier.Private)]
	public static FastTags<TagGroup.Global> explosionTag = FastTags<TagGroup.Global>.Parse("explosion");

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cDisintegrationEpicenterPercent = 0.67f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cMinZombiesForDisintegration = 4f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cMinZombiesForDisintegrationPercent = 0.8f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cStepDistance = 0.51f;

	[PublicizedFrom(EAccessModifier.Private)]
	public static Dictionary<int, Explosion.DamageRecord> hitEntities = new Dictionary<int, Explosion.DamageRecord>();

	[PublicizedFrom(EAccessModifier.Private)]
	public struct DamageRecord
	{
		public EntityAlive entity;

		public float damage;

		public Vector3 dir;

		public Transform hitTransform;

		public EnumBodyPartHit mainPart;

		public EnumBodyPartHit parts;
	}
}
