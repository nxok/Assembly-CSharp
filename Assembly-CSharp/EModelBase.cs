using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Assets.DuckType.Jiggle;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public abstract class EModelBase : MonoBehaviour
{
	public bool IsFPV { get; set; }

	public virtual Transform NeckTransform
	{
		get
		{
			return this.neckTransform;
		}
	}

	public virtual void Init(World _world, Entity _entity, EModelInstanceAssets _eModelAssets)
	{
		EntityClass entityClass = EntityClass.list[_entity.entityClass];
		this.assets = _eModelAssets;
		this.visible = false;
		this.entity = _entity;
		this.ragdollChance = entityClass.RagdollOnDeathChance;
		this.bHasRagdoll = entityClass.HasRagdoll;
		this.modelTransformParent = EModelBase.FindModel(base.transform);
		this.createModel(_world, entityClass);
		bool flag = this.entity.RootMotion || this.bHasRagdoll;
		if (GameManager.IsDedicatedServer && !flag)
		{
			if (entityClass.Properties.GetString(EntityClass.PropAvatarController).Length > 0)
			{
				this.avatarController = base.gameObject.AddComponent<AvatarControllerDummy>();
				Animator[] componentsInChildren = base.transform.GetComponentsInChildren<Animator>();
				for (int i = 0; i < componentsInChildren.Length; i++)
				{
					componentsInChildren[i].enabled = false;
				}
			}
		}
		else
		{
			this.createAvatarController(entityClass);
			if (GameManager.IsDedicatedServer && this.avatarController && flag)
			{
				this.avatarController.SetVisible(true);
			}
		}
		this.gazeController = base.transform.GetComponentInChildren<CharacterGazeController>();
		this.InitCommon();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void InitCommon()
	{
		this.LookAtInit();
		if (this.modelTransformParent)
		{
			this.SwitchModelAndView(false, EntityClass.list[this.entity.entityClass].bIsMale);
		}
		else
		{
			this.modelTransformParent = base.transform;
			this.headTransform = base.transform;
		}
		this.InitRigidBodies();
		this.JiggleInit();
	}

	public void InitRigidBodies()
	{
		if (!this.bipedRootTransform)
		{
			return;
		}
		List<Rigidbody> list = new List<Rigidbody>();
		this.bipedRootTransform.GetComponentsInChildren<Rigidbody>(list);
		for (int i = list.Count - 1; i >= 0; i--)
		{
			if (list[i].gameObject.CompareTag("AudioRigidBody"))
			{
				list.RemoveAt(i);
			}
		}
		float num = EntityClass.list[this.entity.entityClass].MassKg / (float)list.Count;
		if (list.Count == 11)
		{
			num *= 1.1224489f;
			int j = 0;
			while (j < list.Count)
			{
				Rigidbody rigidbody = list[j];
				float num2 = 1f;
				bool flag = false;
				string name = rigidbody.name;
				uint num3 = <PrivateImplementationDetails>.ComputeStringHash(name);
				if (num3 <= 2248339218U)
				{
					if (num3 <= 977255260U)
					{
						if (num3 != 819656463U)
						{
							if (num3 != 928972447U)
							{
								if (num3 != 977255260U)
								{
									goto IL_02C9;
								}
								if (!(name == "RightUpLeg"))
								{
									goto IL_02C9;
								}
							}
							else
							{
								if (!(name == "RightForeArm"))
								{
									goto IL_02C9;
								}
								goto IL_0295;
							}
						}
						else if (!(name == "LeftUpLeg"))
						{
							goto IL_02C9;
						}
						num2 = 1f;
					}
					else
					{
						if (num3 != 1413449684U)
						{
							if (num3 != 2231561599U)
							{
								if (num3 != 2248339218U)
								{
									goto IL_02C9;
								}
								if (!(name == "Spine2"))
								{
									goto IL_02C9;
								}
							}
							else if (!(name == "Spine1"))
							{
								goto IL_02C9;
							}
						}
						else if (!(name == "Spine"))
						{
							goto IL_02C9;
						}
						num2 = 2f;
						flag = true;
					}
				}
				else
				{
					if (num3 <= 2996251363U)
					{
						if (num3 != 2449536012U)
						{
							if (num3 != 2925602325U)
							{
								if (num3 != 2996251363U)
								{
									goto IL_02C9;
								}
								if (!(name == "Head"))
								{
									goto IL_02C9;
								}
								num2 = 0.8f;
								goto IL_02C9;
							}
							else if (!(name == "RightArm"))
							{
								goto IL_02C9;
							}
						}
						else
						{
							if (!(name == "LeftLeg"))
							{
								goto IL_02C9;
							}
							goto IL_02BF;
						}
					}
					else if (num3 <= 3119934960U)
					{
						if (num3 != 3001187991U)
						{
							if (num3 != 3119934960U)
							{
								goto IL_02C9;
							}
							if (!(name == "LeftForeArm"))
							{
								goto IL_02C9;
							}
							goto IL_0295;
						}
						else
						{
							if (!(name == "RightLeg"))
							{
								goto IL_02C9;
							}
							goto IL_02BF;
						}
					}
					else if (num3 != 3537553655U)
					{
						if (num3 != 4018002826U)
						{
							goto IL_02C9;
						}
						if (!(name == "LeftArm"))
						{
							goto IL_02C9;
						}
					}
					else
					{
						if (!(name == "Hips"))
						{
							goto IL_02C9;
						}
						num2 = 2f;
						goto IL_02C9;
					}
					num2 = 0.5f;
					goto IL_02C9;
					IL_02BF:
					num2 = 0.5f;
					flag = true;
				}
				IL_02C9:
				rigidbody.mass = num * num2;
				if (flag && !this.entity.isEntityRemote)
				{
					rigidbody.gameObject.GetOrAddComponent<CollisionCallForward>().Entity = this.entity;
				}
				if (rigidbody.drag <= 0f)
				{
					rigidbody.drag = 0.25f;
				}
				j++;
				continue;
				IL_0295:
				num2 = 0.5f;
				flag = true;
				goto IL_02C9;
			}
			return;
		}
		for (int k = 0; k < list.Count; k++)
		{
			list[k].mass = num;
		}
	}

	public virtual void PostInit()
	{
	}

	public static Transform FindModel(Transform _t)
	{
		Transform transform = _t.Find("Graphics/Model");
		if (transform)
		{
			return transform;
		}
		return _t;
	}

	public virtual void OnUnload()
	{
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnDestroy()
	{
		this.assets.Release();
	}

	public void OriginChanged(Vector3 _deltaPos)
	{
		this.ragdollPosePelvisPos += _deltaPos;
	}

	public virtual Vector3 GetHeadPosition()
	{
		if (this.headTransform == null)
		{
			return this.entity.position + Vector3.up * this.entity.GetEyeHeight();
		}
		return this.headTransform.position + Origin.position;
	}

	public virtual Vector3 GetNavObjectPosition()
	{
		if (this.NavObjectTransform == null)
		{
			return this.GetHeadPosition();
		}
		return this.NavObjectTransform.position + Origin.position;
	}

	public virtual Vector3 GetHipPosition()
	{
		if (this.bipedPelvisTransform == null)
		{
			return this.entity.position + Vector3.up * (this.entity.height * 0.5f);
		}
		return this.bipedPelvisTransform.position + Origin.position;
	}

	public virtual Quaternion GetFlatHipRotation()
	{
		Vector3 vector;
		if (this.bipedPelvisTransform == null)
		{
			vector = this.entity.rotation;
		}
		else
		{
			vector = this.bipedPelvisTransform.eulerAngles;
		}
		vector..ctor(0f, vector.y, 0f);
		return Quaternion.Euler(vector);
	}

	public virtual Vector3 GetChestPosition()
	{
		if (this.bipedPelvisTransform == null || this.headTransform == null)
		{
			return Vector3.Lerp(this.GetHipPosition(), this.GetHeadPosition(), 0.4f);
		}
		return Vector3.Lerp(this.bipedPelvisTransform.position, this.headTransform.position, 0.6f) + Origin.position;
	}

	public virtual Vector3 GetBellyPosition()
	{
		if (this.bipedPelvisTransform == null || this.headTransform == null)
		{
			return Vector3.Lerp(this.GetHipPosition(), this.GetHeadPosition(), 0.2f);
		}
		return Vector3.Lerp(this.bipedPelvisTransform.position, this.headTransform.position, 0.2f) + Origin.position;
	}

	public IKController AddIKController()
	{
		IKController ikcontroller = null;
		Transform transform = this.GetModelTransform();
		if (transform)
		{
			Animator componentInChildren = transform.GetComponentInChildren<Animator>();
			if (componentInChildren)
			{
				ikcontroller = componentInChildren.GetComponent<IKController>();
				if (!ikcontroller)
				{
					ikcontroller = componentInChildren.gameObject.AddComponent<IKController>();
				}
			}
		}
		return ikcontroller;
	}

	public void RemoveIKController()
	{
		Transform transform = this.GetModelTransform();
		if (transform)
		{
			IKController componentInChildren = transform.GetComponentInChildren<IKController>();
			if (componentInChildren)
			{
				componentInChildren.Cleanup();
				Object.Destroy(componentInChildren);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CrouchUpdate(EntityAlive _ea)
	{
		Transform transform = this.neckParentTransform;
		Transform parent = transform.parent;
		float crouchBendPer = _ea.crouchBendPer;
		Quaternion quaternion = Quaternion.Euler(28f * crouchBendPer, 0f, 0f);
		transform.localRotation *= quaternion;
		parent.localRotation *= quaternion;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void LookAtInit()
	{
		EntityClass entityClass = EntityClass.list[this.entity.entityClass];
		this.lookAtMaxAngle = entityClass.LookAtAngle;
		this.lookAtEnabled = this.lookAtMaxAngle > 0f;
	}

	public void ClearLookAt()
	{
		this.lookAtBlendPerTarget = 0f;
	}

	public void SetLookAt(Vector3 _pos)
	{
		this.lookAtPos = _pos;
		this.lookAtBlendPerTarget = this.lookAtFullBlendPer;
		this.lookAtIsPos = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ResetLookAt()
	{
		this.lookAtBlendPer = 0f;
		this.lookAtBlendPerTarget = 0f;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void LookAtUpdate(EntityAlive e)
	{
		if (this.gazeController != null)
		{
			return;
		}
		EnumEntityStunType currentStun = e.bodyDamage.CurrentStun;
		float deltaTime = Time.deltaTime;
		if (e.IsDead() || (currentStun != EnumEntityStunType.None && currentStun != EnumEntityStunType.Getup))
		{
			this.lookAtBlendPerTarget = 0f;
		}
		else if (!this.lookAtIsPos)
		{
			this.lookAtBlendPerTarget -= deltaTime;
			Vector3 vector;
			bool flag;
			if (e.GetHeadLookTarget(out vector, out flag) && (!flag || e.CanSee(vector)))
			{
				this.lookAtPos = vector;
				this.lookAtBlendPerTarget = this.lookAtFullBlendPer;
			}
		}
		if (this.lookAtBlendPer <= 0f && this.lookAtBlendPerTarget <= 0f)
		{
			return;
		}
		this.lookAtFullChangeTime -= deltaTime;
		if (this.lookAtFullChangeTime <= 0f)
		{
			this.lookAtFullChangeTime = 1.3f + 2.7f * e.rand.RandomFloat;
			this.lookAtFullBlendPer = 0.2f + 1.5f * e.rand.RandomFloat;
			if (this.lookAtFullBlendPer > 1f)
			{
				this.lookAtFullBlendPer = 1f;
			}
		}
		this.lookAtBlendPer = Mathf.MoveTowards(this.lookAtBlendPer, this.lookAtBlendPerTarget, deltaTime * 1.5f);
		Quaternion rotation = this.neckParentTransform.rotation;
		Transform transform = this.headTransform;
		Vector3 vector2 = rotation * Vector3.up;
		Quaternion quaternion;
		if (this.entity is EntityNPC && !(this.avatarController is AvatarSDCSController))
		{
			quaternion = Quaternion.LookRotation(this.lookAtPos - Origin.position - transform.position);
			quaternion *= Quaternion.AngleAxis(-90f, Vector3.forward);
		}
		else
		{
			quaternion = Quaternion.LookRotation(this.lookAtPos - Origin.position - transform.position, vector2);
			quaternion *= Quaternion.Slerp(Quaternion.identity, transform.localRotation, 0.5f);
		}
		Quaternion quaternion2 = Quaternion.RotateTowards(rotation, quaternion, this.lookAtMaxAngle);
		this.lookAtRot = Quaternion.Slerp(this.lookAtRot, quaternion2, 0.16f);
		float num = this.lookAtBlendPer;
		this.neckTransform.rotation = Quaternion.Slerp(this.neckTransform.rotation, this.lookAtRot, num * 0.4f);
		Quaternion rotation2 = transform.rotation;
		transform.rotation = Quaternion.Slerp(rotation2, this.lookAtRot, num);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void FixedUpdate()
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void Update()
	{
		this.FrameUpdateRagdoll();
		if (this.modelTransformParent != this.headTransform)
		{
			this.UpdateHeadState();
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void LateUpdate()
	{
		if (this.ragdollIsBlending)
		{
			this.BlendRagdoll();
		}
		EntityAlive entityAlive = this.entity as EntityAlive;
		if (entityAlive != null && !this.IsRagdollActive)
		{
			if (entityAlive.crouchType > 0)
			{
				this.CrouchUpdate(entityAlive);
			}
			if (this.lookAtEnabled)
			{
				this.LookAtUpdate(entityAlive);
			}
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Transform GetModelTransform()
	{
		if (!this.modelTransform)
		{
			return this.modelTransformParent;
		}
		return this.modelTransform;
	}

	public Transform GetModelTransformParent()
	{
		return this.modelTransformParent;
	}

	public virtual Transform GetHitTransform(DamageSource _damageSource)
	{
		string hitTransformName = _damageSource.getHitTransformName();
		if (hitTransformName != null && this.bipedRootTransform)
		{
			return this.bipedRootTransform.FindInChilds(hitTransformName, false);
		}
		return null;
	}

	public virtual Transform GetHitTransform(BodyPrimaryHit _primary)
	{
		if (this.physicsBody != null)
		{
			string text;
			switch (_primary)
			{
			case BodyPrimaryHit.Torso:
				text = "E_BP_Body";
				break;
			case BodyPrimaryHit.Head:
				text = "E_BP_Head";
				break;
			case BodyPrimaryHit.LeftUpperArm:
				text = "E_BP_LArm";
				break;
			case BodyPrimaryHit.RightUpperArm:
				text = "E_BP_RArm";
				break;
			case BodyPrimaryHit.LeftUpperLeg:
				text = "E_BP_LLeg";
				break;
			case BodyPrimaryHit.RightUpperLeg:
				text = "E_BP_RLeg";
				break;
			case BodyPrimaryHit.LeftLowerArm:
				text = "E_BP_LLowerArm";
				break;
			case BodyPrimaryHit.RightLowerArm:
				text = "E_BP_RLowerArm";
				break;
			case BodyPrimaryHit.LeftLowerLeg:
				text = "E_BP_LLowerLeg";
				break;
			case BodyPrimaryHit.RightLowerLeg:
				text = "E_BP_RLowerLeg";
				break;
			default:
				return null;
			}
			return this.physicsBody.GetTransformForColliderTag(text);
		}
		return null;
	}

	public virtual Transform GetHeadTransform()
	{
		if (!this.headTransform)
		{
			return base.transform;
		}
		return this.headTransform;
	}

	public virtual Transform GetPelvisTransform()
	{
		return this.bipedPelvisTransform;
	}

	public virtual Transform GetThirdPersonCameraTransform()
	{
		return base.transform;
	}

	public virtual void OnDeath(DamageResponse _dmResponse, ChunkCluster _cc)
	{
		EntityAlive entityAlive = this.entity as EntityAlive;
		bool flag = entityAlive && entityAlive.bodyDamage.CurrentStun > EnumEntityStunType.None;
		bool flag2 = true;
		if (this.HasRagdoll() && (flag2 || !this.entity.HasDeathAnim || entityAlive.IsSleeper || entityAlive.GetWalkType() == 21 || flag || _dmResponse.Random < this.ragdollChance))
		{
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				this.DoRagdoll(in _dmResponse, EModelBase.RagdollMode.Default, 999999f);
				return;
			}
			if (!entityAlive.IsSpawned())
			{
				this.SpawnWithRagdoll();
				return;
			}
		}
		else if (this.avatarController != null)
		{
			this.avatarController.StartDeathAnimation(_dmResponse.HitBodyPart, _dmResponse.MovementState, _dmResponse.Random);
			if (this.entity is EntityPlayer && this.bipedRootTransform)
			{
				Transform transform = this.bipedRootTransform.Find("Spine1");
				if (transform)
				{
					RagdollWhenHit ragdollWhenHit;
					if (transform.TryGetComponent<RagdollWhenHit>(ref ragdollWhenHit))
					{
						ragdollWhenHit.enabled = true;
						return;
					}
					transform.gameObject.AddComponent<RagdollWhenHit>();
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void restoreTPose(PhysicsBodyInstance physicsBody)
	{
	}

	public virtual bool HasRagdoll()
	{
		return this.bHasRagdoll && this.physicsBody != null;
	}

	public bool IsRagdollActive
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return this.ragdollState > EModelBase.ERagdollState.Off;
		}
	}

	public bool IsRagdollMovement
	{
		get
		{
			return this.ragdollState != EModelBase.ERagdollState.Off && this.ragdollState != EModelBase.ERagdollState.StandCollide;
		}
	}

	public bool IsRagdollOn
	{
		get
		{
			return this.ragdollState == EModelBase.ERagdollState.On || this.ragdollState == EModelBase.ERagdollState.Dead;
		}
	}

	public bool IsRagdollDead
	{
		get
		{
			return this.ragdollState == EModelBase.ERagdollState.Dead;
		}
	}

	public void DoRagdoll(EModelBase.RagdollMode _mode, float stunTime, EnumBodyPartHit bodyPart, Vector3 forceVec, Vector3 forceWorldPos, bool isRemote)
	{
		if (this.entity.IsFlyMode.Value || this.entity.AttachedToEntity)
		{
			return;
		}
		if (!isRemote && !SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer && this.entity.isEntityRemote)
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageEntityRagdoll>().Setup(this.entity, (byte)_mode, stunTime, bodyPart, forceVec, forceWorldPos), false);
			return;
		}
		bool flag = SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer && this.entity.isEntityRemote;
		if (stunTime == 0f)
		{
			if (!flag)
			{
				this.entity.PhysicsPush(forceVec, forceWorldPos, false);
			}
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				this.entity.world.entityDistributer.SendPacketToTrackedPlayersAndTrackedEntity(this.entity.entityId, -1, NetPackageManager.GetPackage<NetPackageEntityRagdoll>().Setup(this.entity, (byte)_mode, 0f, EnumBodyPartHit.Torso, forceVec, forceWorldPos), false);
			}
			return;
		}
		if (!this.StartRagdoll(stunTime))
		{
			return;
		}
		if (forceVec.sqrMagnitude > 0f && !this.entity.isEntityRemote)
		{
			Vector3 vector = forceVec;
			if (bodyPart == EnumBodyPartHit.None)
			{
				float num = -10f;
				if (vector.y < num)
				{
					vector.y = num;
				}
				this.SetRagdollVelocity(vector);
			}
			else
			{
				BodyPrimaryHit bodyPrimaryHit = bodyPart.ToPrimary();
				Transform hitTransform = this.GetHitTransform(bodyPrimaryHit);
				if (hitTransform)
				{
					Rigidbody component = hitTransform.GetComponent<Rigidbody>();
					if (component)
					{
						if (_mode != EModelBase.RagdollMode.FullForce)
						{
							this.ClampForceForMass(ref vector);
						}
						if (forceWorldPos.sqrMagnitude > 0f)
						{
							component.AddForceAtPosition(vector, forceWorldPos - Origin.position, 1);
						}
						else
						{
							component.AddForce(vector, 1);
						}
					}
				}
			}
		}
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			this.entity.world.entityDistributer.SendPacketToTrackedPlayersAndTrackedEntity(this.entity.entityId, -1, NetPackageManager.GetPackage<NetPackageEntityRagdoll>().Setup(this.entity, (byte)_mode, stunTime, bodyPart, forceVec, forceWorldPos), false);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ClampForceForMass(ref Vector3 _force)
	{
		float num = EntityClass.list[this.entity.entityClass].MassKg * 8f;
		float num2 = _force.sqrMagnitude + 0.001f;
		if (num2 > num * num)
		{
			_force *= num / Mathf.Sqrt(num2);
		}
	}

	public void DoRagdoll(in DamageResponse dr, EModelBase.RagdollMode _mode = EModelBase.RagdollMode.Default, float stunTime = 999999f)
	{
		EntityAlive entityAlive = this.entity as EntityAlive;
		if (entityAlive && entityAlive.isDisintegrated)
		{
			return;
		}
		float num = (float)dr.Strength;
		DamageSource source = dr.Source;
		if (num > 0f && source != null)
		{
			Vector3 vector = source.getDirection();
			EnumDamageTypes damageType = source.GetDamageType();
			if (damageType != EnumDamageTypes.Falling && damageType != EnumDamageTypes.Crushing)
			{
				float num2;
				if (dr.HitBodyPart == EnumBodyPartHit.None)
				{
					num2 = this.entity.rand.RandomRange(5f, 25f);
				}
				else
				{
					float num3 = -10f;
					if (stunTime == 0f)
					{
						num3 = 5f;
					}
					num2 = this.entity.rand.RandomRange(num3, 40f);
					num *= 0.5f;
					if (source.damageType == EnumDamageTypes.Bashing)
					{
						num *= 2.5f;
					}
					if (dr.Critical)
					{
						num2 += 25f;
						num *= 2f;
					}
					if ((dr.HitBodyPart & EnumBodyPartHit.Head) > EnumBodyPartHit.None)
					{
						num *= 0.45f;
					}
					num = Utils.FastMin(20f + num, 500f);
					vector *= num;
				}
				Vector3 vector2 = Vector3.Cross(vector.normalized, Vector3.up);
				vector = Quaternion.AngleAxis(num2, vector2) * vector;
			}
			this.DoRagdoll(_mode, stunTime, dr.HitBodyPart, vector, source.getHitTransformPosition(), false);
			return;
		}
		this.DoRagdoll(_mode, stunTime, dr.HitBodyPart, Vector3.zero, Vector3.zero, false);
	}

	public void SetRagdollState(int newState)
	{
		if (this.ragdollState == EModelBase.ERagdollState.On && newState == 2)
		{
			this.ragdollTime = 9999f;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SpawnWithRagdoll()
	{
		if (this.ragdollState == EModelBase.ERagdollState.Off)
		{
			this.ragdollState = EModelBase.ERagdollState.SpawnWait;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool StartRagdoll(float stunTime)
	{
		if (!this.HasRagdoll())
		{
			return false;
		}
		if (this.ragdollState == EModelBase.ERagdollState.Dead)
		{
			return true;
		}
		bool flag = this.entity.IsDead();
		if (!flag && this.ragdollState == EModelBase.ERagdollState.On)
		{
			this.ragdollDuration = Utils.FastMax(this.ragdollDuration, stunTime);
			return true;
		}
		if (this.entity.IsMarkedForUnload())
		{
			return false;
		}
		this.ragdollAnimator = this.avatarController.GetAnimator();
		if (!this.ragdollAnimator)
		{
			return false;
		}
		bool flag2 = this.ragdollState > EModelBase.ERagdollState.Off;
		this.ragdollState = EModelBase.ERagdollState.On;
		this.ragdollTime = 0f;
		this.entity.OnRagdoll(true);
		this.ragdollIsPlayer = this.entity is EntityPlayer;
		this.ragdollAnimator.cullingMode = 0;
		this.ragdollAnimator.keepAnimatorStateOnDisable = true;
		this.ragdollAnimator.enabled = false;
		Animation component = this.GetModelTransform().GetComponent<Animation>();
		if (component)
		{
			component.cullingType = 0;
			component.enabled = false;
		}
		this.CaptureRagdollBones();
		this.CaptureRagdollZeroBones();
		if (flag)
		{
			stunTime = 0.3f;
			this.SetRagdollDead();
		}
		if (this.physicsBody != null)
		{
			this.physicsBody.SetColliderMode(EnumColliderType.All, EnumColliderMode.Ragdoll);
		}
		this.entity.PhysicsPause();
		EntityAlive entityAlive = this.entity as EntityAlive;
		entityAlive.SetStun(EnumEntityStunType.Prone);
		entityAlive.bodyDamage.StunDuration = 1f;
		entityAlive.SetCVar("ragdoll", 1f);
		if (!this.ragdollIsPlayer)
		{
			this.entity.PhysicsTransform.gameObject.SetActive(false);
		}
		this.ragdollTime = 0f;
		this.ragdollDuration = stunTime;
		this.ragdollRotY = this.entity.rotation.y;
		if (this.ragdollIsPlayer)
		{
			this.ragdollDuration *= 0.5f;
		}
		if (flag2)
		{
			this.ragdollIsBlending = false;
			this.ragdollAdjustPosDelay = 0f;
		}
		this.ResetLookAt();
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetRagdollDead()
	{
		this.ragdollState = EModelBase.ERagdollState.Dead;
		for (int i = 0; i < this.ragdollPoses.Count; i++)
		{
			Rigidbody rb = this.ragdollPoses[i].rb;
			if (rb)
			{
				rb.maxDepenetrationVelocity = 2f;
				rb.maxAngularVelocity = 1f;
			}
		}
	}

	public void DisableRagdoll(bool isSetAlive)
	{
		if (this.ragdollState == EModelBase.ERagdollState.Off)
		{
			return;
		}
		if (this.physicsBody != null)
		{
			this.physicsBody.SetColliderMode(EnumColliderType.All, EnumColliderMode.Collision);
		}
		if (this.bipedRootTransform)
		{
			RagdollWhenHit[] componentsInChildren = this.bipedRootTransform.GetComponentsInChildren<RagdollWhenHit>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				Object.DestroyImmediate(componentsInChildren[i]);
			}
		}
		EntityAlive entityAlive = this.entity as EntityAlive;
		if (!isSetAlive && !this.entity.IsDead())
		{
			Vector3 vector = this.headTransform.position - this.bipedPelvisTransform.position;
			float num = Mathf.Atan2(vector.x, vector.z) * 57.29578f;
			this.ragdollIsFacingUp = false;
			this.ragdollIsAnimal = EntityClass.list[entityAlive.entityClass].bIsAnimalEntity;
			string text;
			if (this.ragdollIsAnimal)
			{
				text = "Knockdown";
			}
			else
			{
				text = "Knockdown - Chest";
				if (this.bipedPelvisTransform.forward.y > 0f)
				{
					this.ragdollIsFacingUp = true;
					text = "Knockdown - Back";
					num += 180f;
				}
			}
			this.ragdollRotY = num;
			this.CopyRagdollRot();
			Animation component = this.GetModelTransform().GetComponent<Animation>();
			if (component != null)
			{
				component.cullingType = 0;
				component.enabled = true;
			}
			this.avatarController.ResetAnimations();
			this.ragdollAnimator.keepAnimatorStateOnDisable = true;
			int num2 = (this.ragdollIsPlayer ? 5 : 0);
			this.ragdollAnimator.CrossFadeInFixedTime(text, 0.25f, num2, 2f, 0f);
			this.ragdollAdjustPosDelay = 0.05f;
			this.ragdollState = EModelBase.ERagdollState.BlendOutGround;
			this.ragdollTime = 0f;
			this.ragdollIsBlending = true;
			entityAlive.SetStun(EnumEntityStunType.Getup);
			return;
		}
		this.bipedPelvisTransform.localPosition = this.ragdollPosePelvisLocalPos;
		this.RestoreRagdollStartRot();
		this.SetRagdollOff();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CaptureRagdollBones()
	{
		if (this.ragdollPoses.Count > 0)
		{
			return;
		}
		Animator animator = this.avatarController.GetAnimator();
		if (!animator)
		{
			return;
		}
		this.ragdollPosePelvisLocalPos = this.bipedPelvisTransform.localPosition;
		animator.GetComponentsInChildren<Rigidbody>(EModelBase.ragdollTempRBs);
		for (int i = 0; i < EModelBase.ragdollTempRBs.Count; i++)
		{
			Rigidbody rigidbody = EModelBase.ragdollTempRBs[i];
			GameObject gameObject = rigidbody.gameObject;
			if (!gameObject.CompareTag("Item") && !gameObject.CompareTag("AudioRigidBody"))
			{
				Transform transform = rigidbody.transform;
				EModelBase.RagdollPose ragdollPose;
				ragdollPose.t = transform;
				ragdollPose.rb = rigidbody;
				ragdollPose.rot = Quaternion.identity;
				ragdollPose.startRot = transform.localRotation;
				this.ragdollPoses.Add(ragdollPose);
			}
		}
		EModelBase.ragdollTempRBs.Clear();
	}

	public void CaptureRagdollPositions(List<Vector3> positionList)
	{
		this.CaptureRagdollBones();
		positionList.Clear();
		for (int i = 0; i < this.ragdollPoses.Count; i++)
		{
			positionList.Add(this.ragdollPoses[i].t.position);
		}
	}

	public void ApplyRagdollVelocities(List<Vector3> velocities)
	{
		if (velocities.Count == this.ragdollPoses.Count)
		{
			for (int i = 0; i < this.ragdollPoses.Count; i++)
			{
				Rigidbody rb = this.ragdollPoses[i].rb;
				if (rb)
				{
					rb.velocity = velocities[i];
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetRagdollVelocity(Vector3 _vel)
	{
		for (int i = 0; i < this.ragdollPoses.Count; i++)
		{
			this.ragdollPoses[i].rb.velocity = _vel;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CopyRagdollRot()
	{
		this.ragdollPosePelvisPos = this.bipedPelvisTransform.position;
		for (int i = 0; i < this.ragdollPoses.Count; i++)
		{
			EModelBase.RagdollPose ragdollPose = this.ragdollPoses[i];
			if (ragdollPose.t == this.bipedPelvisTransform)
			{
				ragdollPose.rot = ragdollPose.t.rotation;
			}
			else
			{
				ragdollPose.rot = ragdollPose.t.localRotation;
			}
			this.ragdollPoses[i] = ragdollPose;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RestoreRagdollStartRot()
	{
		for (int i = 0; i < this.ragdollPoses.Count; i++)
		{
			this.ragdollPoses[i].t.localRotation = this.ragdollPoses[i].startRot;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CaptureRagdollZeroBones()
	{
		if (this.ragdollZeroBones != null)
		{
			return;
		}
		Transform parent = this.headTransform;
		if (!parent)
		{
			return;
		}
		this.ragdollZeroTime = 0.33f;
		this.ragdollZeroBones = new List<Transform>();
		while ((parent = parent.parent) && !(parent == this.bipedPelvisTransform))
		{
			if (!parent.GetComponent<Rigidbody>())
			{
				GameObject gameObject = parent.gameObject;
				if (!gameObject.CompareTag("Item") && !gameObject.CompareTag("AudioRigidBody"))
				{
					this.ragdollZeroBones.Add(parent);
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BlendRagdollZeroBones()
	{
		if (this.ragdollZeroTime > 0f)
		{
			float deltaTime = Time.deltaTime;
			this.ragdollZeroTime -= deltaTime;
			for (int i = 0; i < this.ragdollZeroBones.Count; i++)
			{
				Transform transform = this.ragdollZeroBones[i];
				transform.localRotation = Quaternion.RotateTowards(transform.localRotation, Quaternion.identity, 120f * deltaTime);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BlendRagdoll()
	{
		if (this.ragdollAdjustPosDelay > 0f)
		{
			this.ragdollAdjustPosDelay -= Time.deltaTime;
			if (this.ragdollAdjustPosDelay <= 0f)
			{
				Vector3 vector = this.bipedPelvisTransform.position - this.modelTransform.position;
				Vector3 vector2 = this.ragdollPosePelvisPos;
				vector2.x -= vector.x;
				vector2.z -= vector.z;
				if (this.entity.isEntityRemote)
				{
					vector2 = Vector3.Lerp(vector2, this.entity.targetPos - Origin.position, Time.fixedDeltaTime * 10f);
				}
				int num = 0;
				RaycastHit raycastHit;
				while (num < 5 && Physics.Raycast(vector2, Vector3.down, ref raycastHit, 3f, -538750989))
				{
					RootTransformRefEntity component = raycastHit.transform.GetComponent<RootTransformRefEntity>();
					if (!component || component.RootTransform != this.entity.transform)
					{
						vector2.y = raycastHit.point.y + 0.02f;
						break;
					}
					vector2.y = raycastHit.point.y - 0.01f;
					num++;
				}
				this.entity.PhysicsResume(vector2 + Origin.position, this.ragdollRotY);
			}
		}
		float num2 = this.ragdollTime / 0.7f;
		float num3 = num2;
		if (!this.ragdollIsAnimal)
		{
			num3 = (num2 - 0.2f) / 0.8f;
			if (num3 < 0f)
			{
				num3 = 0f;
			}
		}
		this.bipedPelvisTransform.position = Vector3.Lerp(this.ragdollPosePelvisPos, this.bipedPelvisTransform.position, num3);
		for (int i = 0; i < this.ragdollPoses.Count; i++)
		{
			Transform t = this.ragdollPoses[i].t;
			if (t == this.bipedPelvisTransform)
			{
				t.rotation = Quaternion.Slerp(this.ragdollPoses[i].rot, t.rotation, num3);
			}
			else
			{
				t.localRotation = Quaternion.Slerp(this.ragdollPoses[i].rot, t.localRotation, num2);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void FrameUpdateRagdoll()
	{
		if (this.ragdollState == EModelBase.ERagdollState.Off)
		{
			return;
		}
		if (this.ragdollState == EModelBase.ERagdollState.SpawnWait)
		{
			Chunk chunk = (Chunk)this.entity.world.GetChunkFromWorldPos(this.entity.GetBlockPosition());
			if (chunk != null && chunk.IsCollisionMeshGenerated && chunk.IsDisplayed)
			{
				this.ragdollState = EModelBase.ERagdollState.Off;
				this.StartRagdoll(float.MaxValue);
			}
			return;
		}
		bool flag = this.entity.IsDead();
		if (this.pelvisRB && this.IsRagdollMovement)
		{
			if (!flag && this.entity.isEntityRemote)
			{
				Vector3 position = this.pelvisRB.position;
				Vector3 vector = this.entity.targetPos - Origin.position;
				this.pelvisRB.AddForce(-EModelBase.serverPosSpringForce * (position - vector) - EModelBase.serverPosSpringDamping * this.pelvisRB.velocity, 5);
			}
			if (!(this.entity is EntityPlayer))
			{
				this.entity.SetPosition(this.pelvisRB.position + Origin.position, false);
			}
			else if (!this.entity.isEntityRemote)
			{
				this.entity.SetPosition(this.pelvisRB.position + Origin.position, false);
			}
		}
		this.entity.SetRotationAndStopTurning(new Vector3(0f, this.ragdollRotY, 0f));
		this.ragdollTime += Time.deltaTime;
		switch (this.ragdollState)
		{
		case EModelBase.ERagdollState.On:
			this.BlendRagdollZeroBones();
			if (this.ragdollTime >= this.ragdollDuration && (this.pelvisRB.velocity.sqrMagnitude <= 0.25f || this.ragdollTime > 10f))
			{
				this.DisableRagdoll(false);
				if (!this.entity.isEntityRemote)
				{
					NetPackageEntityRagdoll netPackageEntityRagdoll = NetPackageManager.GetPackage<NetPackageEntityRagdoll>().Setup(this.entity, (byte)this.ragdollState);
					if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
					{
						this.entity.world.entityDistributer.SendPacketToTrackedPlayersAndTrackedEntity(this.entity.entityId, -1, netPackageEntityRagdoll, false);
						return;
					}
					SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(netPackageEntityRagdoll, false);
					return;
				}
			}
			break;
		case EModelBase.ERagdollState.BlendOutGround:
			this.RestoreRagdollStartRot();
			if (this.ragdollTime >= 0.25f)
			{
				string text = "GetUpChest";
				if (this.ragdollIsFacingUp)
				{
					text = "GetUpBack";
				}
				if ((this.entity as EntityAlive).IsWalkTypeACrawl())
				{
					text = "CrawlerGetUpChest";
				}
				int num = (this.ragdollIsPlayer ? 5 : 0);
				this.ragdollAnimator.CrossFade(text, 0.3f, num);
				this.ragdollState = EModelBase.ERagdollState.BlendOutStand;
				return;
			}
			break;
		case EModelBase.ERagdollState.BlendOutStand:
			this.RestoreRagdollStartRot();
			if (this.ragdollTime >= 0.7f)
			{
				this.ragdollIsBlending = false;
				this.ragdollState = EModelBase.ERagdollState.Stand;
				this.ragdollTime = 0f;
				return;
			}
			break;
		case EModelBase.ERagdollState.Stand:
			if (this.ragdollTime >= 0.8f)
			{
				if (!this.ragdollIsPlayer)
				{
					this.entity.PhysicsTransform.gameObject.SetActive(true);
				}
				this.ragdollState = EModelBase.ERagdollState.StandCollide;
				return;
			}
			break;
		case EModelBase.ERagdollState.StandCollide:
			if (this.ragdollTime >= 1.7f)
			{
				this.SetRagdollOff();
				this.entity.OnRagdoll(false);
				return;
			}
			break;
		case EModelBase.ERagdollState.SpawnWait:
			break;
		case EModelBase.ERagdollState.Dead:
			this.BlendRagdollZeroBones();
			if (this.ragdollTime >= this.ragdollDuration)
			{
				this.ragdollDuration = float.MaxValue;
				if (this.physicsBody != null)
				{
					this.physicsBody.SetColliderMode(EnumColliderType.All, EnumColliderMode.RagdollDead);
				}
			}
			break;
		default:
			return;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetRagdollOff()
	{
		this.ragdollState = EModelBase.ERagdollState.Off;
		EntityAlive entityAlive = this.entity as EntityAlive;
		entityAlive.SetCVar("ragdoll", 0f);
		entityAlive.ClearStun();
		if (!this.ragdollIsPlayer)
		{
			this.entity.PhysicsTransform.gameObject.SetActive(true);
		}
		this.ragdollPoses.Clear();
		this.ragdollZeroBones = null;
		this.CheckAnimFreeze();
	}

	public string GetRagdollDebugInfo()
	{
		return string.Format("{0:0.#}/{1:0.#} {2}", this.ragdollTime.ToCultureInvariantString(), this.ragdollDuration.ToCultureInvariantString(), this.ragdollState.ToStringCached<EModelBase.ERagdollState>());
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void ClothSimInit()
	{
		this.clothSim = base.GetComponentsInChildren<Cloth>();
		if (this.clothSim != null)
		{
			for (int i = this.clothSim.Length - 1; i >= 0; i--)
			{
				this.clothSim[i].gameObject.SetActive(false);
			}
			if (GameManager.IsDedicatedServer)
			{
				this.clothSim = null;
			}
			this.isClothSimOn = false;
		}
	}

	public void ClothSimOn(bool _on, bool _force = false)
	{
		if (this.clothSim == null || (this.isClothSimOn == _on && !_force))
		{
			return;
		}
		bool flag = this.entity is EntityPlayerLocal;
		this.isClothSimOn = _on;
		for (int i = this.clothSim.Length - 1; i >= 0; i--)
		{
			Cloth cloth = this.clothSim[i];
			cloth.gameObject.SetActive(_on);
			if (_on && flag)
			{
				cloth.worldAccelerationScale = 0.3f;
			}
		}
	}

	public void ClearClothMotion()
	{
		for (int i = this.clothSim.Length - 1; i >= 0; i--)
		{
			this.clothSim[i].ClearTransformMotion();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void JiggleInit()
	{
		this.jiggles = base.GetComponentsInChildren<Jiggle>();
		if (GameManager.IsDedicatedServer && this.jiggles != null)
		{
			for (int i = this.jiggles.Length - 1; i >= 0; i--)
			{
				this.jiggles[i].gameObject.SetActive(false);
			}
			this.jiggles = null;
		}
	}

	public void JiggleOn(bool _on)
	{
		if (this.jiggles == null || this.isJiggleOn == _on)
		{
			return;
		}
		this.isJiggleOn = _on;
		for (int i = this.jiggles.Length - 1; i >= 0; i--)
		{
			this.jiggles[i].gameObject.SetActive(_on);
		}
	}

	public void LogJiggles()
	{
		if (this.jiggles == null || this.jiggles.Length == 0)
		{
			return;
		}
		Log.Out(string.Format("{0}_{1} jiggle count: {2}", this.entity.GetDebugName(), this.entity.entityId, this.jiggles.Length));
		for (int i = 0; i < this.jiggles.Length; i++)
		{
			Jiggle jiggle = this.jiggles[i];
			Log.Out(string.Format("\t{0} {1}", i, jiggle.DebugString()));
		}
	}

	public virtual void SwitchModelAndView(bool _bFPV, bool _bMale)
	{
		if (this.GetModelTransform() != null)
		{
			Animator component = this.GetModelTransform().GetComponent<Animator>();
			if (component != null)
			{
				component.enabled = !_bFPV;
			}
		}
		this.IsFPV = _bFPV;
		if (this.modelName != null && this.modelTransformParent != null)
		{
			this.modelTransform = this.modelTransformParent.Find(this.modelName);
			this.meshTransform = GameUtils.FindTagInDirectChilds(this.modelTransform, "E_Mesh");
			if (!this.meshTransform)
			{
				this.meshTransform = this.modelTransform.Find("LOD0");
				if (!this.meshTransform)
				{
					Renderer componentInChildren = this.modelTransform.GetComponentInChildren<Renderer>();
					if (componentInChildren)
					{
						this.meshTransform = componentInChildren.transform;
					}
				}
			}
		}
		Transform transform = this.GetModelTransform();
		if (this.avatarController)
		{
			this.avatarController.SwitchModelAndView(this.modelName, _bFPV, _bMale);
		}
		else if (transform)
		{
			transform.gameObject.SetActive(true);
		}
		this.headTransform = GameUtils.FindTagInChilds(transform, "E_BP_Head");
		if (this.headTransform)
		{
			this.neckTransform = this.headTransform.parent;
			this.neckParentTransform = this.neckTransform.parent;
		}
		this.bipedRootTransform = GameUtils.FindTagInChilds(transform, "E_BP_BipedRoot");
		if (this.bipedRootTransform == null)
		{
			foreach (string text in EModelBase.commonBips)
			{
				if ((this.bipedRootTransform = transform.Find(text)) != null || (this.bipedRootTransform = transform.FindInChilds(text, false)) != null)
				{
					break;
				}
			}
		}
		if (this.bipedRootTransform != null)
		{
			if (this.bipedRootTransform.name != "pelvis" && this.bipedRootTransform.name != "Hips")
			{
				this.bipedPelvisTransform = GameUtils.FindChildWithPartialName(this.bipedRootTransform, new string[] { "pelvis" });
				if (this.bipedPelvisTransform == null)
				{
					this.bipedPelvisTransform = GameUtils.FindChildWithPartialName(this.bipedRootTransform, new string[] { "hips" });
					if (this.bipedPelvisTransform == null)
					{
						this.bipedPelvisTransform = GameUtils.FindChildWithPartialName(this.bipedRootTransform, new string[] { "hip" });
					}
				}
			}
			else
			{
				this.bipedPelvisTransform = this.bipedRootTransform;
			}
		}
		if (this.bipedPelvisTransform)
		{
			this.pelvisRB = this.bipedPelvisTransform.GetComponent<Rigidbody>();
			if (!(this.entity is EntityPlayer))
			{
				foreach (SkinnedMeshRenderer skinnedMeshRenderer in this.modelTransformParent.GetComponentsInChildren<SkinnedMeshRenderer>(true))
				{
					if (skinnedMeshRenderer.quality == null)
					{
						skinnedMeshRenderer.quality = 2;
					}
					Bounds localBounds = skinnedMeshRenderer.localBounds;
					if (skinnedMeshRenderer.CompareTag("E_BP_Eye"))
					{
						if (this.headTransform)
						{
							skinnedMeshRenderer.rootBone = this.headTransform;
							localBounds.center = Vector3.zero;
							localBounds.extents = new Vector3(0.25f, 0.25f, 0.25f);
						}
						skinnedMeshRenderer.shadowCastingMode = 0;
					}
					else if (skinnedMeshRenderer.rootBone != this.bipedPelvisTransform)
					{
						skinnedMeshRenderer.rootBone = this.bipedPelvisTransform;
						localBounds.center += -this.bipedPelvisTransform.localPosition;
					}
					skinnedMeshRenderer.localBounds = localBounds;
				}
			}
		}
		EntityAlive entityAlive = this.entity as EntityAlive;
		if (entityAlive && entityAlive.inventory != null)
		{
			if (this.entity.isEntityRemote)
			{
				entityAlive.inventory.ForceHoldingItemUpdate();
			}
			else if (this.entity is EntityPlayerLocal && (this.entity as EntityPlayerLocal).PlayerUI != null && !(this.entity as EntityPlayerLocal).PlayerUI.windowManager.IsWindowOpen("character"))
			{
				entityAlive.inventory.ForceHoldingItemUpdate();
			}
		}
		if (this.GetRightHandTransform() != null)
		{
			SkinnedMeshRenderer[] componentsInChildren2 = this.GetRightHandTransform().GetComponentsInChildren<SkinnedMeshRenderer>();
			for (int k = 0; k < componentsInChildren2.Length; k++)
			{
				componentsInChildren2[k].updateWhenOffscreen = true;
			}
		}
		this.NavObjectTransform = base.transform.FindInChilds("IconTag", false);
		PhysicsBodyLayout physicsBodyLayout = EntityClass.list[this.entity.entityClass].PhysicsBody;
		if (physicsBodyLayout != null && this.bipedRootTransform != null)
		{
			if (this.physicsBody != null)
			{
				this.physicsBody.SetColliderMode(EnumColliderType.All, EnumColliderMode.Disabled);
			}
			this.physicsBody = new PhysicsBodyInstance(this.bipedRootTransform, physicsBodyLayout, EnumColliderMode.Collision);
			this.physicsBody.SetColliderMode(EnumColliderType.All, EnumColliderMode.Collision);
			RagdollWhenHit[] componentsInChildren3 = this.bipedRootTransform.GetComponentsInChildren<RagdollWhenHit>();
			for (int i = 0; i < componentsInChildren3.Length; i++)
			{
				componentsInChildren3[i].enabled = false;
			}
		}
		else
		{
			this.physicsBody = null;
		}
		if (transform != null)
		{
			Animator component2 = transform.GetComponent<Animator>();
			if (component2 != null)
			{
				component2.enabled = !this.IsFPV;
			}
			this.SetColliderLayers(transform, 0);
		}
		this.CheckAnimFreeze();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void SetColliderLayers(Transform modelT, int layer)
	{
		CapsuleCollider[] componentsInChildren = modelT.GetComponentsInChildren<CapsuleCollider>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			GameObject gameObject = componentsInChildren[i].gameObject;
			if (!gameObject.CompareTag("LargeEntityBlocker") && !gameObject.CompareTag("Physics"))
			{
				gameObject.layer = layer;
			}
		}
		BoxCollider[] componentsInChildren2 = modelT.GetComponentsInChildren<BoxCollider>();
		for (int j = 0; j < componentsInChildren2.Length; j++)
		{
			componentsInChildren2[j].gameObject.layer = layer;
		}
	}

	public virtual void SetInRightHand(Transform _transform)
	{
		if (this.avatarController != null)
		{
			this.avatarController.SetInRightHand(_transform);
		}
	}

	public virtual void SetAlive()
	{
		this.DisableRagdoll(true);
		Transform transform = this.GetModelTransform();
		if (transform)
		{
			Utils.MoveTaggedToLayer(transform.gameObject, "LargeEntityBlocker", 19);
			Animator component = transform.GetComponent<Animator>();
			if (component != null)
			{
				component.enabled = !this.IsFPV;
			}
		}
		if (this.avatarController != null)
		{
			this.avatarController.SetAlive();
		}
	}

	public virtual void SetDead()
	{
		Transform transform = this.GetModelTransform();
		if (transform)
		{
			Utils.MoveTaggedToLayer(transform.gameObject, "LargeEntityBlocker", 17);
		}
		if (this.ragdollState == EModelBase.ERagdollState.On)
		{
			this.SetRagdollDead();
			if (this.physicsBody != null && this.physicsBody.Mode != EnumColliderMode.RagdollDead)
			{
				this.physicsBody.SetColliderMode(EnumColliderType.All, EnumColliderMode.RagdollDead);
			}
		}
	}

	public bool visible
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public virtual void SetVisible(bool _bVisible, bool _isKeepColliders = false)
	{
		this.visible = _bVisible;
		if (_isKeepColliders)
		{
			this.modelTransformParent.GetComponentsInChildren<Renderer>(EModelBase.rendererList);
			for (int i = 0; i < EModelBase.rendererList.Count; i++)
			{
				EModelBase.rendererList[i].enabled = _bVisible;
			}
			EModelBase.rendererList.Clear();
			if (!_bVisible)
			{
				return;
			}
		}
		if (this.avatarController != null)
		{
			this.avatarController.SetVisible(_bVisible);
		}
	}

	public void SetFade(float _fade)
	{
		if (this.matPropBlock == null)
		{
			this.matPropBlock = new MaterialPropertyBlock();
		}
		this.modelTransformParent.GetComponentsInChildren<Renderer>(EModelBase.rendererList);
		for (int i = 0; i < EModelBase.rendererList.Count; i++)
		{
			Renderer renderer = EModelBase.rendererList[i];
			bool flag = false;
			Material material = renderer.material;
			string name = material.shader.name;
			if (material.HasProperty("_Fade") && name.Contains("Game/Character"))
			{
				flag = true;
			}
			if (renderer.gameObject.CompareTag("LOD") || renderer.gameObject.CompareTag("E_Mesh") || flag)
			{
				this.matPropBlock.SetFloat(EModelBase.fadeId, _fade);
				renderer.SetPropertyBlock(this.matPropBlock);
			}
		}
		EModelBase.rendererList.Clear();
	}

	public virtual Transform GetRightHandTransform()
	{
		if (this.avatarController != null)
		{
			return this.avatarController.GetRightHandTransform();
		}
		return null;
	}

	public virtual void SetSkinTexture(string _textureName)
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void createModel(World _world, EntityClass _ec)
	{
		if (this.modelTransformParent == null)
		{
			return;
		}
		Transform transform = null;
		if (_ec.IsPrefabCombined && this.modelTransformParent.childCount > 0)
		{
			transform = this.modelTransformParent.GetChild(0);
		}
		if (this.assets.Mesh)
		{
			transform = Object.Instantiate<Transform>(this.assets.Mesh, this.modelTransformParent, false);
			transform.name = this.assets.Mesh.name;
			Vector3 localPosition = transform.localPosition;
			if ((double)localPosition.z < -0.5 || (double)localPosition.z > 0.5)
			{
				Log.Warning("createModel mesh moved {0} {1} {2}", new object[]
				{
					transform.name,
					localPosition.ToString("f3"),
					transform.localRotation
				});
			}
			localPosition.x = 0f;
			localPosition.y = 0f;
			transform.localPosition = localPosition;
		}
		if (transform)
		{
			transform.gameObject.SetActive(true);
			this.modelName = transform.name;
			if (_ec.particleOnSpawn.fileName != null)
			{
				ParticleSystem particleSystem = DataLoader.LoadAsset<ParticleSystem>(_ec.particleOnSpawn.fileName, false);
				if (particleSystem != null)
				{
					ParticleSystem particleSystem2 = Object.Instantiate<ParticleSystem>(particleSystem);
					particleSystem2.transform.parent = this.modelTransformParent;
					if (_ec.particleOnSpawn.shapeMesh != null && _ec.particleOnSpawn.shapeMesh.Length > 0)
					{
						SkinnedMeshRenderer[] componentsInChildren = base.GetComponentsInChildren<SkinnedMeshRenderer>();
						ParticleSystem.ShapeModule shapeModule = particleSystem2.shape;
						shapeModule.shapeType = 14;
						string text = _ec.particleOnSpawn.shapeMesh.ToLower();
						if (text.Contains("setshapetomesh"))
						{
							text = text.Replace("setshapetomesh", "");
							int num = int.Parse(text);
							if (num >= 0 && num < componentsInChildren.Length)
							{
								shapeModule.skinnedMeshRenderer = componentsInChildren[num];
								ParticleSystem[] componentsInChildren2 = particleSystem2.transform.GetComponentsInChildren<ParticleSystem>();
								if (componentsInChildren2 != null)
								{
									for (int i = 0; i < componentsInChildren2.Length; i++)
									{
										shapeModule = componentsInChildren2[i].shape;
										shapeModule.shapeType = 14;
										shapeModule.skinnedMeshRenderer = componentsInChildren[num];
									}
								}
							}
						}
					}
				}
			}
			bool flag = false;
			this.AltMaterial = this.assets.AltMaterial;
			if (this.AltMaterial != null)
			{
				flag = true;
			}
			Color color = Color.black;
			string @string = _ec.Properties.GetString(EntityClass.PropMatColor);
			if (@string.Length > 0)
			{
				color = EntityClass.sColors[@string];
				flag = true;
			}
			if (flag)
			{
				base.GetComponentsInChildren<SkinnedMeshRenderer>(true, EModelBase.skinnedRendererList);
				Material material = this.AltMaterial;
				if (@string.Length > 0)
				{
					if (!material)
					{
						for (int j = 0; j < EModelBase.skinnedRendererList.Count; j++)
						{
							SkinnedMeshRenderer skinnedMeshRenderer = EModelBase.skinnedRendererList[j];
							if (skinnedMeshRenderer.CompareTag("LOD"))
							{
								material = skinnedMeshRenderer.sharedMaterials[0];
								break;
							}
						}
					}
					if (@string.Length > 0)
					{
						material = Object.Instantiate<Material>(material);
						material.SetColor("_EmissiveColor", color);
					}
					this.AltMaterial = material;
				}
				for (int k = 0; k < EModelBase.skinnedRendererList.Count; k++)
				{
					SkinnedMeshRenderer skinnedMeshRenderer2 = EModelBase.skinnedRendererList[k];
					if (skinnedMeshRenderer2.CompareTag("LOD"))
					{
						Material[] sharedMaterials = skinnedMeshRenderer2.sharedMaterials;
						sharedMaterials[0] = material;
						skinnedMeshRenderer2.materials = sharedMaterials;
					}
				}
				EModelBase.skinnedRendererList.Clear();
			}
			if (_ec.MatSwap != null)
			{
				foreach (Renderer renderer in base.GetComponentsInChildren<Renderer>(true))
				{
					if (renderer.CompareTag("LOD"))
					{
						Material[] sharedMaterials2 = renderer.sharedMaterials;
						int num2 = Utils.FastMin(sharedMaterials2.Length, _ec.MatSwap.Length);
						for (int m = 0; m < num2; m++)
						{
							string text2 = _ec.MatSwap[m];
							if (text2 != null && text2.Length > 0)
							{
								Material material2 = DataLoader.LoadAsset<Material>(text2, false);
								if (material2)
								{
									sharedMaterials2[m] = material2;
								}
							}
						}
						renderer.materials = sharedMaterials2;
					}
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void createAvatarController(EntityClass _ec)
	{
		string text = ((this.entity is EntityPlayerLocal) ? EntityClass.PropLocalAvatarController : EntityClass.PropAvatarController);
		text = _ec.Properties.GetString(text);
		if (text.Length > 0)
		{
			Type type = Type.GetType(text);
			this.avatarController = base.gameObject.GetComponent(type) as AvatarController;
			if (!this.avatarController)
			{
				this.avatarController = base.gameObject.AddComponent(type) as AvatarController;
			}
		}
	}

	public virtual void Detach()
	{
		for (int i = 0; i < this.modelTransformParent.childCount; i++)
		{
			Object.Destroy(this.modelTransformParent.GetChild(i).gameObject);
		}
		Object.Destroy(this.avatarController);
		Object.Destroy(this);
		this.avatarController = null;
	}

	[Conditional("DEBUG_RAGDOLL")]
	public void LogRagdoll(string _format = "", params object[] _args)
	{
		_format = string.Format("{0} Ragdoll {1}, id{2}, {3}, {4}", new object[]
		{
			GameManager.frameCount,
			this.entity.GetDebugName(),
			this.entity.entityId,
			this.ragdollState,
			_format
		});
		Log.Warning(_format, _args);
	}

	[Conditional("DEBUG_RAGDOLLDO")]
	public void LogRagdollDo(string _format = "", params object[] _args)
	{
		_format = string.Format("{0} Ragdoll Do {1}, id{2}, {3}, time {4}, {5}", new object[]
		{
			GameManager.frameCount,
			this.entity.GetDebugName(),
			this.entity.entityId,
			this.ragdollState,
			this.ragdollTime,
			_format
		});
		Log.Warning(_format, _args);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CheckAnimFreeze()
	{
		if (EAIManager.isAnimFreeze)
		{
			EntityAlive entityAlive = this.entity as EntityAlive;
			if (entityAlive && entityAlive.aiManager != null && this.avatarController)
			{
				Animator animator = this.avatarController.GetAnimator();
				if (animator)
				{
					animator.enabled = false;
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateHeadState()
	{
		if (this.headTransform == null)
		{
			return;
		}
		switch (this.HeadState)
		{
		case EModelBase.HeadStates.Standard:
			if (this.headTransform.localScale.x != this.HeadStandardSize)
			{
				this.headTransform.localScale = new Vector3(this.HeadStandardSize, this.HeadStandardSize, this.HeadStandardSize);
				return;
			}
			break;
		case EModelBase.HeadStates.Growing:
		{
			float num = this.headTransform.localScale.y + this.headScaleSpeed * Time.deltaTime;
			if (num >= this.HeadBigSize)
			{
				num = this.HeadBigSize;
				this.HeadState = EModelBase.HeadStates.BigHead;
				EntityAlive entityAlive = this.entity as EntityAlive;
				if (entityAlive != null)
				{
					entityAlive.CurrentHeadState = this.HeadState;
				}
			}
			this.headTransform.localScale = new Vector3(num, num, num);
			return;
		}
		case EModelBase.HeadStates.BigHead:
			if (this.headTransform.localScale.x != this.HeadBigSize)
			{
				this.headTransform.localScale = new Vector3(this.HeadBigSize, this.HeadBigSize, this.HeadBigSize);
			}
			break;
		case EModelBase.HeadStates.Shrinking:
		{
			float num2 = this.headTransform.localScale.y - this.headScaleSpeed * Time.deltaTime;
			if (num2 <= this.HeadStandardSize)
			{
				num2 = this.HeadStandardSize;
				this.HeadState = EModelBase.HeadStates.Standard;
				EntityAlive entityAlive2 = this.entity as EntityAlive;
				if (entityAlive2 != null)
				{
					entityAlive2.CurrentHeadState = this.HeadState;
				}
			}
			this.headTransform.localScale = new Vector3(num2, num2, num2);
			return;
		}
		default:
			return;
		}
	}

	public void ForceHeadState(EModelBase.HeadStates headState)
	{
		if (this.headTransform == null)
		{
			return;
		}
		this.HeadState = headState;
		if (headState != EModelBase.HeadStates.Standard)
		{
			if (headState == EModelBase.HeadStates.BigHead)
			{
				float headBigSize = this.HeadBigSize;
				this.headTransform.localScale = new Vector3(headBigSize, headBigSize, headBigSize);
				return;
			}
		}
		else
		{
			float headStandardSize = this.HeadStandardSize;
			this.headTransform.localScale = new Vector3(headStandardSize, headStandardSize, headStandardSize);
		}
	}

	public void SetHeadScale(float standard)
	{
		this.HeadStandardSize = standard;
		this.HeadBigSize = Mathf.Min(4.5f, standard * 3f);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public EModelBase()
	{
	}

	public const string ExtFirstPerson = "_FP";

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const string cEmissiveColor = "_EmissiveColor";

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public EModelInstanceAssets assets;

	public AvatarController avatarController;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Transform modelTransformParent;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Transform modelTransform;

	public Transform meshTransform;

	public Transform bipedRootTransform;

	public Transform bipedPelvisTransform;

	public Rigidbody pelvisRB;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Transform headTransform;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Transform neckTransform;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Transform neckParentTransform;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public CharacterGazeController gazeController;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public PhysicsBodyInstance physicsBody;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public string modelName;

	public Material AltMaterial;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public float ragdollChance;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public bool bHasRagdoll;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Cloth[] clothSim;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool isClothSimOn = true;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Jiggle[] jiggles;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool isJiggleOn = true;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Entity entity;

	public EModelBase.HeadStates HeadState;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public float headScaleSpeed = 2f;

	public float HeadStandardSize = 1f;

	public float HeadBigSize = 3f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Transform NavObjectTransform;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public const float ragdollAlignmentForce = 25f;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public const float ragdollAlingmentDistance = 0.2f;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public const float ragdollBlendPositionSpeed = 10f;

	public static float serverPosSpringForce = 50f;

	public static float serverPosSpringDamping = 0.1f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static List<Renderer> rendererList = new List<Renderer>();

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static List<SkinnedMeshRenderer> skinnedRendererList = new List<SkinnedMeshRenderer>();

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const float cLookAtSlerpPer = 0.16f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const float cLookAtAnimBlend = 0.5f;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public bool lookAtEnabled;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float lookAtMaxAngle;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool lookAtIsPos;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3 lookAtPos;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Quaternion lookAtRot;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float lookAtBlendPer;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float lookAtBlendPerTarget;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float lookAtFullChangeTime;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float lookAtFullBlendPer = 1f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const float cRadgollBlendOutGroundTime = 0.25f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const float cRadgollBlendOutTime = 0.7f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const float cRagdollMinDisableVel = 0.5f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const float cRagdollDeadMaxDepentrationVel = 2f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const float cRagdollDeadMaxAngularVel = 1f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const int cRadgollPlayerAnimLayer = 5;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public EModelBase.ERagdollState ragdollState;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float ragdollTime;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float ragdollDuration;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Animator ragdollAnimator;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float ragdollRotY;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool ragdollIsBlending;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float ragdollAdjustPosDelay;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool ragdollIsPlayer;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool ragdollIsAnimal;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool ragdollIsFacingUp;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public readonly List<EModelBase.RagdollPose> ragdollPoses = new List<EModelBase.RagdollPose>();

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3 ragdollPosePelvisPos;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3 ragdollPosePelvisLocalPos;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static readonly List<Rigidbody> ragdollTempRBs = new List<Rigidbody>();

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float ragdollZeroTime;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public List<Transform> ragdollZeroBones;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static readonly string[] commonBips = new string[] { "Bip001", "Bip01" };

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public MaterialPropertyBlock matPropBlock;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static int fadeId = Shader.PropertyToID("_Fade");

	public enum HeadStates
	{
		Standard,
		Growing,
		BigHead,
		Shrinking
	}

	public enum RagdollMode
	{
		Default,
		FullForce
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public enum ERagdollState
	{
		Off,
		On,
		BlendOutGround,
		BlendOutStand,
		Stand,
		StandCollide,
		SpawnWait,
		Dead
	}

	public struct RagdollPose
	{
		public Transform t;

		public Rigidbody rb;

		public Quaternion startRot;

		public Quaternion rot;
	}
}
