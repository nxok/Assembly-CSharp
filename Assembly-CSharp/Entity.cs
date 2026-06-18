using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using Audio;
using Twitch;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class Entity : MonoBehaviour, ILockTarget
{
	public FastTags<TagGroup.Global> EntityTags
	{
		get
		{
			return this.cachedTags;
		}
	}

	public EntityClass EntityClass
	{
		get
		{
			EntityClass entityClass;
			EntityClass.list.TryGetValue(this.entityClass, out entityClass);
			return entityClass;
		}
	}

	public virtual string LocalizedEntityName
	{
		get
		{
			return Localization.Get(EntityClass.list[this.entityClass].entityClassName, false, null);
		}
	}

	public virtual Entity.EnumPositionUpdateMovementType positionUpdateMovementType
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return Entity.EnumPositionUpdateMovementType.Lerp;
		}
	}

	public static bool CheckDistance(int entityID_A, int entityID_B)
	{
		if (GameManager.Instance == null)
		{
			return false;
		}
		if (GameManager.Instance.World == null)
		{
			return false;
		}
		Entity entity = GameManager.Instance.World.GetEntity(entityID_A);
		if (entity == null)
		{
			return false;
		}
		Entity entity2 = GameManager.Instance.World.GetEntity(entityID_B);
		return !(entity2 == null) && Entity.CheckDistance(entity, entity2);
	}

	public static bool CheckDistance(Entity entityB, int entityID_A)
	{
		return Entity.CheckDistance(entityID_A, entityB);
	}

	public static bool CheckDistance(int entityID_A, Entity entityB)
	{
		if (GameManager.Instance == null)
		{
			return false;
		}
		if (GameManager.Instance.World == null)
		{
			return false;
		}
		if (entityB == null)
		{
			return false;
		}
		Entity entity = GameManager.Instance.World.GetEntity(entityID_A);
		return !(entity == null) && Entity.CheckDistance(entity, entityB);
	}

	public static bool CheckDistance(Entity A, Vector3 B)
	{
		return !(A == null) && Entity.CheckDistance(A.transform.position, B);
	}

	public static bool CheckDistance(Vector3 A, Entity B)
	{
		return !(B == null) && Entity.CheckDistance(A, B.transform.position);
	}

	public static bool CheckDistance(Vector3 A, int entityID_B)
	{
		if (GameManager.Instance == null)
		{
			return false;
		}
		if (GameManager.Instance.World == null)
		{
			return false;
		}
		Entity entity = GameManager.Instance.World.GetEntity(entityID_B);
		return !(entity == null) && Entity.CheckDistance(A - Origin.position, entity.transform.position);
	}

	public static bool CheckDistance(Vector3 A, Vector3 B)
	{
		return (A - B).magnitude < 256f;
	}

	public static bool CheckDistance(Entity listenerEntity, Entity sourceEntity)
	{
		return Entity.CheckDistance(sourceEntity.transform.position, listenerEntity.transform.position);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void Awake()
	{
		Entity.InstanceCount++;
		this.world = GameManager.Instance.World;
		this.isEntityRemote = !SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer;
		this.WorldTimeBorn = this.world.worldTime;
		this.rand = this.world.GetGameRandom();
		this.SetupBounds();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void Start()
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public ~Entity()
	{
		Entity.InstanceCount--;
	}

	public virtual void OnXMLChanged()
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void SetupBounds()
	{
		BoxCollider boxCollider;
		if (base.TryGetComponent<BoxCollider>(ref boxCollider))
		{
			this.nativeCollider = boxCollider;
			Vector3 localScale = base.transform.localScale;
			this.scaledExtent = Vector3.Scale(boxCollider.size, localScale) * 0.5f;
			Vector3 vector = Vector3.Scale(boxCollider.center, localScale);
			this.boundingBox = BoundsUtils.BoundsForMinMax(-this.scaledExtent, this.scaledExtent);
			this.boundingBox.center = this.boundingBox.center + vector;
			if (this.isDetailedHeadBodyColliders())
			{
				boxCollider.enabled = false;
				return;
			}
		}
		else
		{
			CharacterController characterController;
			if (base.TryGetComponent<CharacterController>(ref characterController))
			{
				Vector3 localScale2 = base.transform.localScale;
				float radius = characterController.radius;
				this.scaledExtent = new Vector3(radius * localScale2.x, characterController.height * localScale2.y * 0.5f, radius * localScale2.z);
				this.boundingBox = BoundsUtils.BoundsForMinMax(-this.scaledExtent, this.scaledExtent);
				return;
			}
			this.boundingBox = BoundsUtils.BoundsForMinMax(Vector3.zero, Vector3.one);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void Update()
	{
		this.bWasDead = this.IsDead();
		this.animateYaw();
		if (this.physicsMasterTargetTime > 0f)
		{
			this.PhysicsMasterTargetFrameUpdate();
		}
		else
		{
			this.updateTransform();
		}
		if (this.bIsChunkObserver && !this.isEntityRemote)
		{
			if (this.movableChunkObserver == null)
			{
				this.movableChunkObserver = new MovableSharedChunkObserver(this.world.m_SharedChunkObserverCache);
			}
			this.movableChunkObserver.SetPosition(this.position);
		}
		else if (!this.bIsChunkObserver && this.movableChunkObserver != null)
		{
			this.movableChunkObserver.Dispose();
			this.movableChunkObserver = null;
		}
		if (this.animatorAudioMonitoringDictionary.Count > 0)
		{
			List<Entity.StopAnimatorAudioType> list = new List<Entity.StopAnimatorAudioType>();
			foreach (KeyValuePair<Entity.StopAnimatorAudioType, Handle> keyValuePair in this.animatorAudioMonitoringDictionary)
			{
				if (!keyValuePair.Value.IsPlaying())
				{
					keyValuePair.Value.Stop(this.entityId);
					list.Add(keyValuePair.Key);
				}
			}
			foreach (Entity.StopAnimatorAudioType stopAnimatorAudioType in list)
			{
				this.animatorAudioMonitoringDictionary.Remove(stopAnimatorAudioType);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void updateTransform()
	{
		if (this.AttachedToEntity != null)
		{
			return;
		}
		this.ApplyFixedUpdate();
		if (!this.emodel || !this.emodel.IsRagdollOn)
		{
			float num;
			if (this.physicsRB)
			{
				Vector3 vector = this.physicsRBT.position - this.physicsBasePos;
				Vector3 vector2 = Vector3.Lerp(base.transform.position, vector, this.physicsPosMoveDistance * Time.deltaTime / Time.fixedDeltaTime);
				base.transform.position = vector2;
				num = this.physicsRBT.eulerAngles.y;
			}
			else
			{
				Vector3 vector3 = this.position - Origin.position;
				base.transform.position = Vector3.Lerp(base.transform.position, vector3, Time.deltaTime * Entity.updatePositionLerpTimeScale);
				num = this.rotation.y;
			}
			if (this.isRotateToGround)
			{
				Vector3 vector4 = this.groundSurface.normal;
				float num2 = Vector3.Dot(vector4, Vector3.up);
				if (this.IsRotateToGroundFlat)
				{
					num2 = 1f;
				}
				if (num2 > 0.99f || num2 < 0.7f)
				{
					vector4 = Vector3.up;
				}
				Vector3 vector5 = Quaternion.AngleAxis(-num, Vector3.up) * vector4;
				float num3 = 90f - Mathf.Atan2(vector5.y, vector5.z) * 57.29578f;
				this.rotateToGroundPitchVel *= 0.86f;
				this.rotateToGroundPitchVel += Mathf.DeltaAngle(this.rotateToGroundPitch, num3) * 0.8f * Time.deltaTime;
				this.rotateToGroundPitch += this.rotateToGroundPitchVel;
				base.transform.eulerAngles = new Vector3(this.rotateToGroundPitch, num, 0f);
			}
			else
			{
				base.transform.eulerAngles = new Vector3(0f, Mathf.LerpAngle(base.transform.eulerAngles.y, num, Time.deltaTime * Entity.updateRotationLerpTimeScale), 0f);
			}
		}
		if (this.isEntityRemote && this.PhysicsTransform != null)
		{
			this.PhysicsTransform.position = Vector3.Lerp(this.PhysicsTransform.position, this.position - Origin.position, Time.deltaTime * Entity.updateRotationLerpTimeScale);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void FixedUpdate()
	{
		this.ApplyFixedUpdate();
		this.wasFixedUpdate = true;
		if (this.physicsRB)
		{
			this.physicsRB.velocity *= 0.9f;
			this.physicsRB.angularVelocity *= 0.9f;
			Transform transform = this.physicsRBT;
			Vector3 vector = this.physicsTargetPos + this.physicsBasePos;
			Vector3 vector2 = Vector3.Lerp(transform.position, vector, 0.4f);
			this.physicsPos = vector2;
			this.physicsRot = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0f, this.rotation.y, 0f), 0.3f);
			transform.SetPositionAndRotation(vector2, this.physicsRot);
			if (this.physicsCapsuleCollider)
			{
				EntityAlive entityAlive = this as EntityAlive;
				if (entityAlive)
				{
					entityAlive.CrouchHeightFixedUpdate();
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ApplyFixedUpdate()
	{
		if (!this.wasFixedUpdate)
		{
			return;
		}
		this.wasFixedUpdate = false;
		if (this.physicsRB)
		{
			Transform transform = this.physicsRBT;
			Vector3 vector = transform.position;
			if ((vector - this.physicsPos).sqrMagnitude > 0.0001f)
			{
				Vector3 vector2 = this.position;
				Vector3 vector3 = vector - this.physicsBasePos;
				this.physicsPos = vector;
				this.SetPosition(vector3 + Origin.position, false);
				this.PhysicsTransform.position = vector3;
			}
			this.physicsPosMoveDistance = Vector3.Distance(this.physicsPos, base.transform.position);
			if (Mathf.Abs(Quaternion.Angle(transform.rotation, this.physicsRot)) > 0.1f)
			{
				Quaternion quaternion = transform.rotation;
				this.physicsRot = quaternion;
				this.rotation = quaternion.eulerAngles;
				this.qrotation = quaternion;
			}
		}
	}

	public virtual void OriginChanged(Vector3 _deltaPos)
	{
		this.physicsPos += _deltaPos;
		this.physicsTargetPos += _deltaPos;
		if (this.emodel)
		{
			this.emodel.OriginChanged(_deltaPos);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void AddCharacterController()
	{
		if (!this.PhysicsTransform)
		{
			return;
		}
		float num = 0.08f;
		Vector3 vector = Vector3.zero;
		bool flag = false;
		GameObject gameObject = this.PhysicsTransform.gameObject;
		float num2;
		float num3;
		if (this is EntityPlayer)
		{
			CharacterController component = gameObject.GetComponent<CharacterController>();
			if (!component)
			{
				Log.Error("Player !cc");
				return;
			}
			vector = component.center;
			num2 = component.height;
			num3 = component.radius;
			this.m_characterController = new CharacterControllerUnity(component);
			if (!this.isEntityRemote)
			{
				gameObject.AddComponent<ColliderHitCallForward>().Entity = this;
			}
			BoxCollider boxCollider = this.nativeCollider as BoxCollider;
			if (boxCollider)
			{
				num2 = Utils.FastMax(boxCollider.size.y - num, this.stepHeight);
				vector = boxCollider.center;
				vector.y = num2 * 0.5f;
				if (boxCollider.size.x > boxCollider.size.y)
				{
					vector.y += (boxCollider.size.x - boxCollider.size.y) * 0.5f;
				}
				num3 = boxCollider.size.x * 0.5f - num;
				flag = true;
			}
		}
		else
		{
			flag = true;
			CapsuleCollider component2 = gameObject.GetComponent<CapsuleCollider>();
			if (component2)
			{
				vector = component2.center;
				num2 = component2.height;
				num3 = component2.radius;
			}
			else
			{
				gameObject.AddComponent<CapsuleCollider>();
				vector.y = 0.9f;
				num2 = 1.8f;
				num3 = 0.3f;
			}
			if (this.physicsCapsuleCollider)
			{
				num2 = this.physicsBaseHeight;
				vector.y = num2 * 0.5f;
			}
			CharacterController characterController;
			if (gameObject.TryGetComponent<CharacterController>(ref characterController))
			{
				vector = characterController.center;
				num2 = characterController.height;
				num3 = characterController.radius;
				Object.Destroy(characterController);
				Log.Warning("{0} has old CC", new object[] { this.ToString() });
			}
			this.m_characterController = new CharacterControllerKinematic(this);
		}
		if (num2 <= 0f)
		{
			return;
		}
		if (flag)
		{
			vector.y /= this.physicsHeightScale;
			this.m_characterController.SetSize(vector, num2 / this.physicsHeightScale, num3);
			this.physicsBaseHeight = num2;
			this.physicsHeight = num2;
			if (this.physicsCapsuleCollider)
			{
				this.PhysicsSetHeight(num2);
			}
		}
		this.m_characterController.SetStepOffset(this.stepHeight);
		Vector3 localScale = base.transform.localScale;
		this.scaledExtent = new Vector3(num3 * localScale.x, num2 * localScale.y * 0.5f, num3 * localScale.z);
		this.boundingBox = BoundsUtils.BoundsForMinMax(-this.scaledExtent, this.scaledExtent);
		if (this.nativeCollider)
		{
			this.nativeCollider.enabled = false;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetCCScale(float scale)
	{
		CharacterControllerAbstract characterController = this.m_characterController;
		if (characterController == null)
		{
			return;
		}
		this.PhysicsTransform.localScale = Vector3.one;
		Vector3 vector = characterController.GetCenter() * scale;
		float num = characterController.GetHeight() * scale;
		if (num < 2.2f && num > 1.89f)
		{
			num = 1.89f;
			vector.y = num * 0.5f;
		}
		float num2 = Utils.FastMax(scale, 1f);
		characterController.SetSize(vector, num, characterController.GetRadius() * num2);
	}

	public virtual void Init(int _entityClass, EntityInstanceAssets _assets, EModelInstanceAssets _eModelAssets)
	{
		this.entityClass = _entityClass;
		this.assets = _assets;
		this.InitCommon();
		this.InitEModel(_eModelAssets);
		this.PhysicsInit();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void InitCommon()
	{
		EntityClass entityClass = EntityClass.list[this.entityClass];
		this.cachedTags = entityClass.Tags;
		this.bIsChunkObserver = entityClass.bIsChunkObserver;
		this.CopyPropertiesFromEntityClass();
		this.InitializeBagFromLootList();
		if (this.PhysicsTransform)
		{
			this.PhysicsTransform.gameObject.tag = "Physics";
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void InitializeBagFromLootList()
	{
		if (this.bag != null)
		{
			return;
		}
		string text = this.GetLootList();
		if (string.IsNullOrEmpty(text))
		{
			return;
		}
		LootContainer lootContainer = LootContainer.GetLootContainer(text, false);
		if (lootContainer == null)
		{
			return;
		}
		Vector2i size = lootContainer.size;
		if (size.x <= 0 || size.y <= 0)
		{
			return;
		}
		this.bag = new Bag(size.x * size.y);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void InitEModel(EModelInstanceAssets _assets)
	{
		Type modelType = EntityClass.list[this.entityClass].modelType;
		this.emodel = base.gameObject.AddComponent(modelType) as EModelBase;
		this.emodel.Init(this.world, this, _assets);
	}

	public virtual void PostInit()
	{
		if (this.emodel != null)
		{
			this.emodel.PostInit();
			this.HandleNavObject();
		}
		if (this.physicsRB != null)
		{
			this.fallLastY = this.physicsRB.position.y + Origin.position.y;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void PhysicsInit()
	{
		Transform transform = GameUtils.FindTagInChilds(this.ModelTransform, "Physics");
		if (transform)
		{
			this.PhysicsTransform = this.RootTransform.Find("Physics");
			if (this.PhysicsTransform)
			{
				Object.Destroy(this.PhysicsTransform.gameObject);
				Log.Warning("{0} has old Physics", new object[] { this.ToString() });
			}
			this.PhysicsTransform = transform;
			transform.SetParent(this.RootTransform, false);
		}
		else if (!this.PhysicsTransform)
		{
			this.PhysicsTransform = this.RootTransform.Find("Physics");
		}
		this.physicsRBT = GameUtils.FindTagInChilds(this.RootTransform, "LargeEntityBlocker");
		if (this.physicsRBT)
		{
			Transform transform2 = base.transform;
			Transform parent = this.physicsRBT.parent;
			this.physicsPos = this.physicsRBT.position;
			this.physicsRot = transform2.rotation;
			if (parent != transform2.parent)
			{
				Vector3 vector = this.physicsRBT.localPosition;
				float x = parent.lossyScale.x;
				vector += parent.localPosition * (1f / x);
				Collider[] componentsInChildren = this.physicsRBT.GetComponentsInChildren<Collider>();
				for (int i = componentsInChildren.Length - 1; i >= 0; i--)
				{
					Collider collider = componentsInChildren[i];
					CapsuleCollider capsuleCollider;
					BoxCollider boxCollider;
					SphereCollider sphereCollider;
					if (capsuleCollider = collider as CapsuleCollider)
					{
						capsuleCollider.center = (capsuleCollider.center + vector) * x;
						capsuleCollider.height *= x;
						capsuleCollider.radius *= x;
					}
					else if (boxCollider = collider as BoxCollider)
					{
						boxCollider.center = (boxCollider.center + vector) * x;
						boxCollider.size *= x;
					}
					else if (sphereCollider = collider as SphereCollider)
					{
						sphereCollider.center = (sphereCollider.center + vector) * x;
						sphereCollider.radius *= x;
					}
				}
				this.physicsBasePos = Vector3.zero;
				this.physicsRBT.SetParent(transform2.parent, true);
				this.physicsRBT.localScale = Vector3.one;
			}
			else
			{
				this.physicsBasePos = Vector3.Scale(this.physicsRBT.localPosition, parent.lossyScale);
			}
			this.physicsRB = this.physicsRBT.gameObject.AddComponent<Rigidbody>();
			this.physicsRB.useGravity = false;
			float num = EntityClass.list[this.entityClass].MassKg * 0.6f;
			this.physicsRB.mass = Utils.FastMax(30f, num);
			this.physicsRB.constraints = 80;
			this.physicsRB.collisionDetectionMode = 2;
			this.physicsTargetPos = this.physicsPos;
			CapsuleCollider component = this.physicsRBT.GetComponent<CapsuleCollider>();
			if (component && component.direction == 1)
			{
				this.physicsCapsuleCollider = component;
				this.physicsColliderRadius = component.radius;
				this.physicsHeightScale = 1.09f;
				float height = component.height;
				float y = component.center.y;
				float num2 = y + height * 0.5f;
				this.physicsBaseHeight = num2 * this.physicsHeightScale;
				this.physicsColliderLowerY = y - height * 0.5f;
				if ((double)this.physicsBaseHeight > 1.95)
				{
					this.physicsBaseHeight = 1.95f;
				}
			}
		}
	}

	public void PhysicsSetRB(Rigidbody rb)
	{
		this.physicsRB = rb;
	}

	public void PhysicsPause()
	{
		if (this.physicsRBT)
		{
			this.physicsRBT.gameObject.SetActive(false);
		}
	}

	public virtual void PhysicsResume(Vector3 pos, float rotY)
	{
		this.rotation = new Vector3(0f, rotY, 0f);
		if (this.physicsRBT)
		{
			this.physicsRBT.gameObject.SetActive(true);
			this.physicsRBT.eulerAngles = this.rotation;
			this.physicsPosMoveDistance = 0f;
		}
		this.SetPosition(pos, true);
		base.transform.SetPositionAndRotation(pos - Origin.position, Quaternion.Euler(this.rotation));
	}

	public virtual void PhysicsPush(Vector3 forceVec, Vector3 forceWorldPos, bool affectLocalPlayerController = false)
	{
		if (forceVec.sqrMagnitude > 0f)
		{
			Rigidbody rigidbody = this.physicsRB;
			if (rigidbody)
			{
				if (!this.emodel.IsRagdollActive)
				{
					forceVec *= 5f;
				}
				if (forceWorldPos.sqrMagnitude > 0f)
				{
					rigidbody.AddForceAtPosition(forceVec, forceWorldPos - Origin.position, 1);
					return;
				}
				rigidbody.AddForce(forceVec, 1);
			}
		}
	}

	public void PhysicsSetHeight(float _height)
	{
		this.physicsHeight = _height;
		float num = this.physicsColliderLowerY;
		if (_height - num < this.physicsColliderRadius)
		{
			num = _height - this.physicsColliderRadius;
			if (num < 0f)
			{
				num = 0f;
			}
		}
		this.physicsCapsuleCollider.height = _height - num;
		Vector3 center = this.physicsCapsuleCollider.center;
		center.y = (_height + num) * 0.5f;
		if (center.y < this.physicsColliderRadius)
		{
			center.y = this.physicsColliderRadius;
		}
		this.physicsCapsuleCollider.center = center;
	}

	public virtual void PhysicsMasterBecome()
	{
		this.isPhysicsMaster = true;
		this.physicsMasterTargetTime = 0f;
		this.SetPosition(this.physicsMasterTargetPos, false);
		this.qrotation = this.physicsMasterTargetRot;
		if (this.physicsRB)
		{
			this.physicsRB.position = this.position - Origin.position;
			this.physicsRB.rotation = this.qrotation;
			this.physicsRB.velocity = this.physicsVel;
			this.physicsRB.angularVelocity = this.physicsAngVel;
		}
	}

	public NetPackageEntityPhysics PhysicsMasterSetupBroadcast()
	{
		if ((this.position - this.physicsMasterSendPos).sqrMagnitude < 0.0025000002f && Quaternion.Angle(this.qrotation, this.physicsMasterSendRot) < 1f)
		{
			return null;
		}
		this.physicsMasterSendPos = this.position;
		this.physicsMasterSendRot = this.qrotation;
		return NetPackageManager.GetPackage<NetPackageEntityPhysics>().Setup(this);
	}

	public void PhysicsMasterSendToServer(Transform t)
	{
		if (this.clientEntityId != 0)
		{
			return;
		}
		this.position = t.position + Origin.position;
		this.qrotation = t.rotation;
		if (this.GetVelocityPerSecond().sqrMagnitude < 0.16000001f)
		{
			this.isPhysicsMaster = false;
		}
		NetPackageEntityPhysics netPackageEntityPhysics = NetPackageManager.GetPackage<NetPackageEntityPhysics>().Setup(this);
		SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(netPackageEntityPhysics, false);
	}

	public Vector3 PhysicsMasterGetFinalPosition()
	{
		if (this.physicsMasterTargetTime > 0f)
		{
			return this.physicsMasterTargetPos;
		}
		return this.position;
	}

	public void PhysicsMasterSetTargetOrientation(Vector3 pos, Quaternion rot)
	{
		this.physicsMasterFromPos = this.position;
		this.physicsMasterFromRot = this.qrotation;
		this.physicsMasterTargetElapsed = 0f;
		this.physicsMasterTargetTime = 0.1f;
		this.physicsMasterTargetPos = pos;
		this.physicsMasterTargetRot = rot;
	}

	public void PhysicsMasterTargetFrameUpdate()
	{
		this.physicsMasterTargetElapsed += Time.deltaTime;
		float num = this.physicsMasterTargetElapsed / this.physicsMasterTargetTime;
		Vector3 vector = Vector3.Lerp(this.physicsMasterFromPos, this.physicsMasterTargetPos, num);
		this.SetPosition(vector, true);
		Quaternion quaternion = Quaternion.Lerp(this.physicsMasterFromRot, this.physicsMasterTargetRot, num);
		this.qrotation = quaternion;
		this.physicsRB.position = vector - Origin.position;
		this.physicsRB.rotation = quaternion;
		if (this.physicsMasterTargetElapsed >= this.physicsMasterTargetTime)
		{
			this.physicsMasterTargetTime = 0f;
		}
	}

	public void SetHeight(float _height)
	{
		this.m_characterController.SetHeight(_height / this.physicsHeightScale);
		this.PhysicsSetHeight(_height);
	}

	public void SetMaxHeight(float _maxHeight)
	{
		this.physicsBaseHeight = _maxHeight;
		if (this.m_characterController != null)
		{
			this.m_characterController.SetHeight(_maxHeight / this.physicsHeightScale);
		}
		if (this.physicsCapsuleCollider)
		{
			this.PhysicsSetHeight(_maxHeight);
			float y = this.physicsCapsuleCollider.center.y;
			float num = this.physicsCapsuleCollider.height * 0.5f;
			this.physicsBaseHeight = y + num;
			this.physicsColliderLowerY = y - num;
		}
	}

	public void SetScale(float scale)
	{
		Vector3 vector;
		vector..ctor(scale, scale, scale);
		this.ModelTransform.localScale = vector;
		foreach (CharacterJoint characterJoint in this.ModelTransform.GetComponentsInChildren<CharacterJoint>())
		{
			if (characterJoint.autoConfigureConnectedAnchor)
			{
				characterJoint.autoConfigureConnectedAnchor = false;
				characterJoint.autoConfigureConnectedAnchor = true;
			}
		}
		if (this.physicsRBT)
		{
			this.physicsBaseHeight *= scale;
			this.physicsHeight *= scale;
			this.physicsColliderLowerY *= scale;
			Collider[] componentsInChildren2 = this.physicsRBT.GetComponentsInChildren<Collider>();
			for (int j = componentsInChildren2.Length - 1; j >= 0; j--)
			{
				Collider collider = componentsInChildren2[j];
				CapsuleCollider capsuleCollider = collider as CapsuleCollider;
				if (capsuleCollider != null)
				{
					capsuleCollider.center *= scale;
					capsuleCollider.height *= scale;
					capsuleCollider.radius *= scale;
				}
				else
				{
					BoxCollider boxCollider = collider as BoxCollider;
					if (boxCollider != null)
					{
						boxCollider.center *= scale;
						boxCollider.size *= scale;
					}
					else
					{
						SphereCollider sphereCollider = collider as SphereCollider;
						if (sphereCollider != null)
						{
							sphereCollider.center *= scale;
							sphereCollider.radius *= scale;
						}
					}
				}
			}
		}
		this.SetCCScale(scale);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void ReplicateSpeeds()
	{
		int num = this.speedSentTicks - 1;
		this.speedSentTicks = num;
		if (num > 0)
		{
			return;
		}
		float num2 = this.speedForward - this.speedForwardSent;
		float num3 = this.speedStrafe - this.speedStrafeSent;
		if (num2 * num2 + num3 * num3 >= 4.0000004E-06f)
		{
			this.speedSentTicks = 3;
			this.speedForwardSent = this.speedForward;
			this.speedStrafeSent = this.speedStrafe;
			if (this.world.IsRemote())
			{
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageEntitySpeeds>().Setup(this), false);
				return;
			}
			this.world.entityDistributer.SendPacketToTrackedPlayers(this.entityId, this.entityId, NetPackageManager.GetPackage<NetPackageEntitySpeeds>().Setup(this), false);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void SetMovementState()
	{
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void animateYaw()
	{
		if (this.yawSeekTimeMax <= 1E-45f)
		{
			if (this.yawSeekTimeMax > 0f)
			{
				this.rotation.y = this.yawSeekAngleEnd;
				this.yawSeekTimeMax = 0f;
			}
			return;
		}
		this.yawSeekTime += Time.deltaTime;
		float num = Mathf.Clamp01(this.yawSeekTime / this.yawSeekTimeMax);
		if (this.yawSeekTime < this.yawSeekTimeMax)
		{
			this.rotation.y = Mathf.Lerp(this.yawSeekAngle, this.yawSeekAngleEnd, num);
			return;
		}
		this.yawSeekTimeMax = 0f;
		this.rotation.y = this.yawSeekAngleEnd;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool IsSeekYaw()
	{
		return this.yawSeekTimeMax > 0f;
	}

	public void SeekYawToPos(Vector3 _pos, float _yawSlowAt)
	{
		float num = _pos.x - this.position.x;
		float num2 = _pos.z - this.position.z;
		if (num * num + num2 * num2 > 0.0001f)
		{
			float num3 = Mathf.Atan2(num, num2) * 57.29578f;
			this.SeekYaw(num3, 0f, _yawSlowAt);
		}
	}

	public float SeekYaw(float yaw, float _, float yawSlowAt)
	{
		if (yaw < 0f)
		{
			yaw += 360f;
		}
		if (yaw > 360f)
		{
			yaw -= 360f;
		}
		if (this.rotation.y < 0f)
		{
			this.rotation.y = this.rotation.y + 360f;
		}
		if (this.rotation.y > 360f)
		{
			this.rotation.y = this.rotation.y - 360f;
		}
		float num = EntityClass.list[this.entityClass].MaxTurnSpeed;
		if (this.inWaterPercent > 0.3f)
		{
			num *= 1f - this.inWaterPercent * 0.5f;
		}
		if (num > 0f)
		{
			float num2 = yaw - this.rotation.y;
			if (num2 != 0f)
			{
				if (num2 < -180f)
				{
					num2 += 360f;
				}
				if (num2 > 180f)
				{
					num2 -= 360f;
				}
				float num3 = Utils.FastAbs(num2);
				if (num3 < yawSlowAt)
				{
					float num4 = num3 / yawSlowAt;
					num = num * num4 * num4;
					num = Utils.FastMax(num, 20f);
				}
				this.yawSeekTime = 0f;
				this.yawSeekTimeMax = num3 / num;
				this.yawSeekAngle = this.rotation.y;
				this.yawSeekAngleEnd = this.rotation.y + num2;
				return num2;
			}
		}
		this.rotation.y = yaw;
		this.yawSeekTimeMax = 0f;
		return 0f;
	}

	public virtual void KillLootContainer()
	{
		this.Kill(DamageResponse.New(true));
	}

	public virtual void Kill(DamageResponse _dmResponse)
	{
		this.SetDead();
		if (this.attachedEntities != null)
		{
			for (int i = 0; i < this.attachedEntities.Length; i++)
			{
				Entity entity = this.attachedEntities[i];
				if (entity != null)
				{
					entity.Kill(_dmResponse);
					entity.Detach();
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void TickInWater()
	{
		this.inWaterLevel = this.CalcWaterLevel();
		this.inWaterPercent = this.inWaterLevel / (this.GetHeight() * 1.1f);
		this.isInWater = this.inWaterPercent >= 0.25f;
		bool flag = this.isSwimming;
		this.isSwimming = this.CalcIfSwimming();
		if (this.isSwimming != flag)
		{
			this.SwimChanged();
		}
		bool flag2 = this.isHeadUnderwater;
		this.isHeadUnderwater = this.IsHeadUnderwater();
		if (this.isHeadUnderwater != flag2)
		{
			this.OnHeadUnderwaterStateChanged(this.isHeadUnderwater);
		}
	}

	public float CalcWaterLevel()
	{
		float num = this.GetHeight() * 1.1f;
		int num2 = Utils.Fastfloor(this.position.y + num);
		int num3 = Utils.Fastfloor(this.position.y);
		int num4 = num2 - num3 + 1;
		int num5 = Utils.Fastfloor(this.position.x);
		int num6 = Utils.Fastfloor(this.position.z);
		int i = -2;
		while (i < 6)
		{
			Vector3i vector3i;
			if (i < 0)
			{
				vector3i.x = num5;
				vector3i.z = num6;
				goto IL_00E4;
			}
			vector3i.x = Utils.Fastfloor(this.position.x + Entity.waterLevelDirOffsets[i] * 0.28f);
			vector3i.z = Utils.Fastfloor(this.position.z + Entity.waterLevelDirOffsets[i + 1] * 0.28f);
			if (vector3i.x != num5 || vector3i.z != num6)
			{
				goto IL_00E4;
			}
			IL_0184:
			i += 2;
			continue;
			IL_00E4:
			vector3i.y = num2;
			int num7 = num4;
			float num8;
			for (;;)
			{
				num8 = this.world.GetWaterPercent(vector3i);
				if (num8 > 0f)
				{
					break;
				}
				vector3i.y--;
				if (--num7 <= 0)
				{
					goto IL_0184;
				}
			}
			if (num7 == num4)
			{
				vector3i.y++;
				if (this.world.GetWaterPercent(vector3i) == 0f)
				{
					num8 = 0.6f;
				}
				vector3i.y--;
			}
			else
			{
				num8 = 0.6f;
			}
			return Mathf.Clamp((float)vector3i.y + num8 - this.position.y, 0f, num);
		}
		return 0f;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual bool CalcIfSwimming()
	{
		return this.inWaterPercent >= 0.5f;
	}

	public virtual void SwimChanged()
	{
	}

	public virtual bool IsHeadUnderwater()
	{
		return this.inWaterPercent >= 0.9f;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnHeadUnderwaterStateChanged(bool _bUnderwater)
	{
		if (!_bUnderwater)
		{
			Manager.Play(this, "water_emerge", 1f, false);
		}
	}

	public virtual void OnCollisionForward(Transform t, Collision collision, bool isStay)
	{
	}

	public void OnControllerColliderHit(ControllerColliderHit hit)
	{
		if (hit.normal.y > 0.707f && hit.normal.y > this.groundSurface.normal.y && hit.moveDirection.y < 0f)
		{
			if ((double)(hit.point - this.groundSurface.lastHitPoint).sqrMagnitude > 0.001 || this.groundSurface.lastNormal == Vector3.zero)
			{
				this.groundSurface.normal = hit.normal;
			}
			else
			{
				this.groundSurface.normal = this.groundSurface.lastNormal;
			}
			this.groundSurface.hitPoint = hit.point;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ccEntityCollision(Vector3 _vel)
	{
		this.canCCMove = true;
		this.ccEntityCollisionStart(_vel);
		if (!this.isCCDelayed)
		{
			this.ccEntityCollisionResults();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ccEntityCollisionStart(Vector3 _vel)
	{
		this.groundSurface.lastHitPoint = this.groundSurface.hitPoint;
		this.groundSurface.lastNormal = this.groundSurface.normal;
		this.groundSurface.normal = Vector3.up;
		this.ySize *= this.ConditionalScalePhysicsMulConstant(0.4f);
		if (this.isMotionSlowedDown)
		{
			this.isMotionSlowedDown = false;
			_vel.x *= this.motionMultiplier;
			if (!this.isCollidedVertically)
			{
				_vel.y *= this.motionMultiplier;
			}
			_vel.z *= this.motionMultiplier;
		}
		this.hitMove = _vel;
		this.collisionFlags = 0;
		if (this.IsStuck)
		{
			this.PhysicsTransform.position += this.hitMove;
			return;
		}
		this.collisionFlags = this.m_characterController.Move(this.hitMove);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ccEntityCollisionResults()
	{
		Vector3 vector = this.PhysicsTransform.position;
		this.physicsTargetPos = vector;
		vector += Origin.position;
		Vector3 vector2 = vector - this.position;
		this.position = vector;
		this.boundingBox.center = this.boundingBox.center + vector2;
		Vector2 vector3;
		vector3..ctor(this.motion.x, this.motion.z);
		this.projectedMove = 0f;
		float sqrMagnitude = vector3.sqrMagnitude;
		if (sqrMagnitude > 1E-15f)
		{
			Vector2 vector4;
			vector4..ctor(vector2.x, vector2.z);
			this.projectedMove = Utils.FastClamp01(Vector2.Dot(vector4, vector3) / sqrMagnitude);
			vector3 *= this.projectedMove;
		}
		this.motion.x = vector3.x;
		this.motion.z = vector3.y;
		if (this.motion.y > 0f)
		{
			if (vector2.y >= 0f && vector2.y < this.motion.y * 0.95f)
			{
				this.motion.y = 0f;
			}
			else
			{
				this.motion.y = Utils.FastClamp(vector2.y, 0f, this.motion.y);
			}
		}
		else
		{
			this.motion.y = Utils.FastClamp(vector2.y, this.motion.y, 0f);
		}
		this.isCollidedHorizontally = (this.collisionFlags & 1) > 0;
		this.isCollidedVertically = (this.collisionFlags & 6) > 0;
		this.onGround = this.m_characterController.IsGrounded();
		if (!this.onGround && this.wasOnGround && this.physicsRB)
		{
			this.fallLastY = this.physicsRB.position.y + Origin.position.y;
		}
		if (this.onGround)
		{
			this.groundSurface.normal = this.m_characterController.GroundNormal;
		}
		this.world.CheckEntityCollisionWithBlocks(this);
		this.UpdateFall(this.hitMove.y);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void aabbEntityCollision(Vector3 _vel)
	{
		this.ySize *= 0.4f;
		if (this.isMotionSlowedDown)
		{
			this.isMotionSlowedDown = false;
			_vel.x *= this.motionMultiplier;
			if (!this.isCollidedVertically)
			{
				_vel.y *= this.motionMultiplier;
			}
			_vel.z *= this.motionMultiplier;
			this.motion = Vector3.zero;
		}
		Vector3 vector = _vel;
		Bounds bounds = this.boundingBox;
		if (Math.Abs(_vel.x) <= 0.0001f)
		{
			Math.Abs(_vel.z);
		}
		this.collAABB.Clear();
		Bounds bounds2 = BoundsUtils.ExpandDirectional(this.boundingBox, vector);
		this.world.GetCollidingBounds(this, bounds2, this.collAABB);
		Vector3 vector2 = BoundsUtils.ClipBoundsMove(this.boundingBox, vector, this.collAABB, this.collAABB.Count);
		this.boundingBox.center = this.boundingBox.center + vector2;
		bool flag = this.onGround || (vector.y != vector2.y && vector.y < 0f);
		if (this.stepHeight > 0f && flag && this.ySize < 0.05f && (vector.x != vector2.x || vector.z != vector2.z))
		{
			Vector3 vector3 = vector2;
			vector2 = vector;
			vector2.y = this.stepHeight;
			Bounds bounds3 = this.boundingBox;
			this.boundingBox = bounds;
			this.collAABB.Clear();
			bounds2 = BoundsUtils.ExpandDirectional(this.boundingBox, new Vector3(vector2.x, 0f, vector2.z));
			this.world.GetCollidingBounds(this, bounds2, this.collAABB);
			vector2 = BoundsUtils.ClipBoundsMove(this.boundingBox, vector2, this.collAABB, this.collAABB.Count);
			this.boundingBox.center = this.boundingBox.center + vector2;
			float num = BoundsUtils.ClipBoundsMoveY(this.boundingBox.min, this.boundingBox.max, -this.stepHeight, this.collAABB, this.collAABB.Count);
			this.boundingBox.center = this.boundingBox.center + new Vector3(0f, num, 0f);
			vector2.y = num;
			if (vector3.x * vector3.x + vector3.z * vector3.z >= vector2.x * vector2.x + vector2.z * vector2.z)
			{
				vector2 = vector3;
				this.boundingBox = bounds3;
			}
			else if (this.boundingBox.min.y - (float)((int)this.boundingBox.min.y) > 0f)
			{
				this.ySize += this.boundingBox.min.y - bounds3.min.y;
			}
		}
		Vector3 center = this.boundingBox.center;
		this.position.x = center.x;
		this.position.y = this.boundingBox.min.y + this.yOffset - this.ySize;
		this.position.z = center.z;
		if (this.PhysicsTransform != null && (this.PhysicsTransform.position - (this.position - Origin.position)).sqrMagnitude > 0.0001f)
		{
			this.PhysicsTransform.position = this.position - Origin.position;
		}
		this.isCollidedHorizontally = vector.x != vector2.x || vector.z != vector2.z;
		this.isCollidedVertically = vector.y != vector2.y;
		this.onGround = vector.y != vector2.y && vector.y < 0f;
		this.world.CheckEntityCollisionWithBlocks(this);
		this.UpdateFall(vector2.y);
		if (vector.x != vector2.x)
		{
			this.motion.x = 0f;
		}
		if (vector.y != vector2.y)
		{
			this.motion.y = 0f;
		}
		if (vector.z != vector2.z)
		{
			this.motion.z = 0f;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void CalcFixedUpdateTimeScaleConstants()
	{
		this.kAddFixedUpdateTimeScale = Time.deltaTime / 0.05f;
	}

	public float ScalePhysicsMulConstant(float tickMulDelta)
	{
		return Mathf.Pow(tickMulDelta, this.kAddFixedUpdateTimeScale);
	}

	public float ScalePhysicsAddConstant(float tickAddDelta)
	{
		return this.kAddFixedUpdateTimeScale * tickAddDelta;
	}

	public float ConditionalScalePhysicsMulConstant(float tickMulDelta)
	{
		return tickMulDelta;
	}

	public float ConditionalScalePhysicsAddConstant(float tickAddDelta)
	{
		return tickAddDelta;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void entityCollision(Vector3 _motion)
	{
		if (this.emodel.IsRagdollMovement)
		{
			if (this.emodel.pelvisRB)
			{
				float num = this.emodel.bipedPelvisTransform.position.y + Origin.position.y;
				Vector3 velocity = this.emodel.pelvisRB.velocity;
				if (velocity.y < -1f)
				{
					this.fallVelY = Utils.FastMin(this.fallVelY, velocity.y);
					float num2 = this.fallLastY - num;
					if (num2 > 0f)
					{
						this.fallDistance += num2;
					}
				}
				else if (this.fallDistance > 0f)
				{
					this.fallLastMotion.y = this.fallVelY * 0.05f;
					this.onGround = true;
					this.UpdateFall(0f);
				}
				this.fallLastY = num;
			}
			return;
		}
		this.ApplyFixedUpdate();
		if (this.m_characterController != null)
		{
			this.ccEntityCollision(_motion);
			return;
		}
		this.aabbEntityCollision(_motion);
	}

	public virtual void SetMotionMultiplier(float _motionMultiplier)
	{
		this.isMotionSlowedDown = true;
		this.motionMultiplier = _motionMultiplier;
		if (this.motionMultiplier < 0.5f)
		{
			this.fallDistance = 0f;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public float GetDistance(Entity _other)
	{
		return (this.position - _other.position).magnitude;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public float GetDistanceSq(Entity _other)
	{
		return (this.position - _other.position).sqrMagnitude;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public float GetDistanceSq(Vector3 _pos)
	{
		return (this.position - _pos).sqrMagnitude;
	}

	public float GetSoundTravelTime(Vector3 _otherPos)
	{
		return (this.position - _otherPos).magnitude / 343f;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool IsInWater()
	{
		return this.isInWater;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool IsSwimming()
	{
		return this.isSwimming;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool IsInElevator()
	{
		return this.bInElevator;
	}

	public void SetInElevator(bool _b)
	{
		this.bInElevator = _b;
	}

	public virtual bool IsAirBorne()
	{
		return this.bAirBorne || !this.onGround;
	}

	public void SetAirBorne(bool _b)
	{
		this.bAirBorne = _b;
	}

	public float width
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return this.scaledExtent.x * 2f;
		}
	}

	public float depth
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return this.scaledExtent.z * 2f;
		}
	}

	public float height
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return this.scaledExtent.y * 2f;
		}
	}

	public virtual float GetEyeHeight()
	{
		return 0f;
	}

	public virtual float GetHeight()
	{
		if (this.m_characterController != null)
		{
			return this.m_characterController.GetHeight();
		}
		return this.height;
	}

	public float radius
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return this.scaledExtent.x;
		}
	}

	public virtual void Move(Vector3 _direction, bool _isDirAbsolute, float _velocity, float _maxVelocity)
	{
		if (!this.IsClientControlled() && (GamePrefs.GetBool(EnumGamePrefs.DebugStopEnemiesMoving) || GameStats.GetInt(EnumGameStats.GameState) == 2))
		{
			return;
		}
		float y = _direction.y;
		_direction.y = 0f;
		_direction.Normalize();
		if (_isDirAbsolute)
		{
			float num = Mathf.Clamp(_maxVelocity - Mathf.Max(0f, Vector3.Dot(this.motion, _direction)), 0f, _velocity);
			this.motion.x = this.motion.x + this.ConditionalScalePhysicsAddConstant(_direction.x * num);
			this.motion.y = this.motion.y + this.ConditionalScalePhysicsAddConstant(_direction.y * _velocity);
			this.motion.z = this.motion.z + this.ConditionalScalePhysicsAddConstant(_direction.z * num);
			return;
		}
		Vector3 vector = base.transform.forward * _direction.z + base.transform.right * _direction.x;
		vector.Normalize();
		float num2 = Mathf.Clamp(_maxVelocity - Mathf.Max(0f, Vector3.Dot(this.motion, vector)), 0f, _velocity);
		this.motion += base.transform.forward * this.ConditionalScalePhysicsAddConstant(_direction.z * num2) + base.transform.right * this.ConditionalScalePhysicsAddConstant(_direction.x * num2) + base.transform.up * this.ConditionalScalePhysicsAddConstant(y * _velocity);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool IsAlive()
	{
		return !this.IsDead();
	}

	public bool WasAlive()
	{
		return !this.WasDead();
	}

	public virtual bool IsDead()
	{
		return this.bDead;
	}

	public bool WasDead()
	{
		return this.bWasDead;
	}

	public virtual void SetDead()
	{
		this.bDead = true;
		Manager.DestroySoundsForEntity(this.entityId);
		if (this.m_marker != null)
		{
			this.m_marker.Release();
			this.m_marker = null;
		}
		if (this.PhysicsTransform != null)
		{
			if (this.emodel.HasRagdoll())
			{
				this.PhysicsTransform.gameObject.layer = 17;
			}
			else
			{
				this.PhysicsTransform.gameObject.layer = 14;
			}
		}
		if (this.physicsRBT)
		{
			this.physicsRBT.gameObject.SetActive(false);
		}
		if (this.emodel != null)
		{
			this.emodel.SetDead();
		}
	}

	public virtual void SetAlive()
	{
		this.bDead = false;
		if (this.PhysicsTransform != null)
		{
			if (this is EntityPlayerLocal)
			{
				this.PhysicsTransform.gameObject.layer = 20;
				return;
			}
			if (ConsoleCmdCCPhysics.EnableCCPhysicsChanges && this is EntityPlayer)
			{
				this.PhysicsTransform.gameObject.layer = 3;
				return;
			}
			this.PhysicsTransform.gameObject.layer = 15;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateFall(float mY)
	{
		if (this.onGround)
		{
			if (this.fallDistance > 0f)
			{
				this.fallHitGround(this.fallDistance, this.fallLastMotion);
				this.fallDistance = 0f;
				return;
			}
		}
		else if (mY < 0f && this.physicsRB != null)
		{
			float num = this.physicsRB.position.y + Origin.position.y;
			float num2 = this.fallLastY - num;
			this.fallLastY = num;
			this.fallLastMotion = this.motion;
			this.fallDistance += num2;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void fallHitGround(float _v, Vector3 _fallMotion)
	{
	}

	public virtual void OnRagdoll(bool isActive)
	{
		if (isActive && this.emodel.bipedPelvisTransform)
		{
			this.fallLastY = this.emodel.bipedPelvisTransform.position.y + Origin.position.y;
			this.fallVelY = 0f;
		}
	}

	public virtual bool CanDamageEntity(int _sourceEntityId)
	{
		return true;
	}

	public virtual int DamageEntity(DamageSource _damageSource, int _strength, bool _criticalHit, float impulseScale = 1f)
	{
		this.setBeenAttacked();
		return 0;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void setBeenAttacked()
	{
	}

	public virtual void FireAttackedEvents(DamageResponse dmResponse)
	{
	}

	public virtual void ProcessDamageResponse(DamageResponse _dmResponse)
	{
	}

	public Bounds getBoundingBox()
	{
		return this.boundingBox;
	}

	public virtual void OnDamagedByExplosion()
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnPushEntity(Entity _entity)
	{
		Vector3 vector = _entity.position - this.position;
		float num = Utils.FastMax(Mathf.Abs(vector.x), Mathf.Abs(vector.z));
		if (num >= 0.01f)
		{
			num = Mathf.Sqrt(num);
			float num2 = 1f / num;
			vector.x *= num2;
			vector.z *= num2;
			if (num2 < 1f)
			{
				vector.x *= num2;
				vector.z *= num2;
			}
			float num3 = 0.05f * (1f - this.entityCollisionReduction);
			num3 *= Utils.FastMin(_entity.GetWeight(), this.GetWeight()) / Utils.FastMax(_entity.GetWeight(), this.GetWeight());
			vector.x *= num3;
			vector.z *= num3;
			this.AddVelocity(new Vector3(-vector.x, 0f, -vector.z));
			if (_entity.CanBePushed())
			{
				_entity.AddVelocity(new Vector3(vector.x, 0f, vector.z));
			}
		}
	}

	public virtual void AddVelocity(Vector3 _vel)
	{
		this.motion += _vel;
		this.SetAirBorne(true);
	}

	public virtual Vector3 GetVelocityPerSecond()
	{
		if (this.AttachedToEntity)
		{
			return this.AttachedToEntity.GetVelocityPerSecond();
		}
		if (this.physicsRB)
		{
			return this.physicsRB.velocity;
		}
		return this.motion * 20f;
	}

	public virtual Vector3 GetAngularVelocityPerSecond()
	{
		if (this.AttachedToEntity)
		{
			return this.AttachedToEntity.GetAngularVelocityPerSecond();
		}
		if (this.physicsRB)
		{
			return this.physicsRB.angularVelocity;
		}
		return Vector3.zero;
	}

	public virtual void SetVelocityPerSecond(Vector3 vel, Vector3 angularVel)
	{
		if (this.AttachedToEntity)
		{
			this.AttachedToEntity.SetVelocityPerSecond(vel, angularVel);
			return;
		}
		this.physicsVel = vel;
		this.physicsAngVel = angularVel;
		if (this.isPhysicsMaster && this.physicsRB)
		{
			this.physicsRB.velocity = vel;
			this.physicsRB.angularVelocity = angularVel;
		}
		this.motion = vel * 0.05f;
	}

	public virtual bool CanBePushed()
	{
		return false;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual float GetPushBoundsVertical()
	{
		return 0f;
	}

	public virtual bool CanCollideWith(Entity _other)
	{
		return true;
	}

	public virtual void OnUpdatePosition(float _partialTicks)
	{
		this.ticksExisted++;
		this.prevPos = this.position;
		this.prevRotation = this.rotation;
		if (this.isUpdatePosition)
		{
			if (this.AttachedToEntity || (this.emodel && this.emodel.IsRagdollOn))
			{
				this.isUpdatePosition = false;
			}
			else
			{
				switch (this.positionUpdateMovementType)
				{
				case Entity.EnumPositionUpdateMovementType.Lerp:
					this.SetPosition(Vector3.Lerp(this.position, this.targetPos, Time.deltaTime / Time.fixedDeltaTime * Entity.tickPositionLerpMultiplier), false);
					goto IL_00D6;
				case Entity.EnumPositionUpdateMovementType.MoveTowards:
					this.SetPosition(Vector3.MoveTowards(this.position, this.targetPos, Entity.tickPositionMoveTowardsMaxDistance), false);
					goto IL_00D6;
				}
				this.SetPosition(this.targetPos, false);
				IL_00D6:
				if (this.position == this.targetPos)
				{
					this.isUpdatePosition = false;
				}
				if (this.PhysicsTransform != null)
				{
					this.physicsTargetPos = this.position - Origin.position;
					this.PhysicsTransform.position = this.physicsTargetPos;
				}
			}
		}
		if (this.interpolateTargetQRot > 0)
		{
			this.qrotation = Quaternion.Lerp(this.qrotation, this.targetQRot, 1f / (float)this.interpolateTargetQRot);
			this.interpolateTargetQRot--;
		}
		if (this.interpolateTargetRot > 0)
		{
			float num = 1f / (float)this.interpolateTargetRot;
			this.SetRotation(new Vector3(Mathf.LerpAngle(this.rotation.x, this.targetRot.x, num), Mathf.LerpAngle(this.rotation.y, this.targetRot.y, num), Mathf.LerpAngle(this.rotation.z, this.targetRot.z, num)));
			this.interpolateTargetRot--;
		}
		if (!this.isEntityRemote && !this.IsDead() && !this.IsClientControlled() && this.position.y < 0f && this.IsDeadIfOutOfWorld())
		{
			EntityDrone entityDrone = this as EntityDrone;
			if (entityDrone)
			{
				entityDrone.NotifyOffTheWorld();
				return;
			}
			Log.Warning(string.Concat(new string[]
			{
				"Entity ",
				(this != null) ? this.ToString() : null,
				" fell off the world, id=",
				this.entityId.ToString(),
				" pos=",
				this.position.ToCultureInvariantString()
			}));
			this.MarkToUnload();
		}
	}

	public virtual void CheckPosition()
	{
		if (float.IsNaN(this.position.x) || float.IsInfinity(this.position.x))
		{
			this.position.x = this.lastTickPos[0].x;
		}
		if (float.IsNaN(this.position.y) || float.IsInfinity(this.position.y))
		{
			this.position.y = this.lastTickPos[0].y;
		}
		if (float.IsNaN(this.position.z) || float.IsInfinity(this.position.z))
		{
			this.position.z = this.lastTickPos[0].z;
		}
		if (float.IsNaN(this.rotation.x) || float.IsInfinity(this.rotation.x))
		{
			this.rotation.x = this.prevRotation.x;
		}
		if (float.IsNaN(this.rotation.y) || float.IsInfinity(this.rotation.y))
		{
			this.rotation.y = this.prevRotation.y;
		}
		if (float.IsNaN(this.rotation.z) || float.IsInfinity(this.rotation.z))
		{
			this.rotation.z = this.prevRotation.z;
		}
	}

	public virtual void OnUpdateEntity()
	{
		bool flag = this.isInWater;
		if (!this.isEntityStatic())
		{
			this.TickInWater();
		}
		if (this.isEntityRemote)
		{
			return;
		}
		if (this.isInWater)
		{
			if (!flag && !this.firstUpdate && this.fallDistance > 1f)
			{
				this.PlayOneShot("waterfallinginto", false, false, false, null, 1f);
			}
			this.fallDistance = 0f;
		}
		if (!this.RootMotion && !this.IsDead() && this.CanBePushed())
		{
			List<Entity> entitiesInBounds = this.world.GetEntitiesInBounds(this, BoundsUtils.ExpandBounds(this.boundingBox, 0.2f, this.GetPushBoundsVertical(), 0.2f));
			if (entitiesInBounds != null && entitiesInBounds.Count > 0)
			{
				for (int i = 0; i < entitiesInBounds.Count; i++)
				{
					Entity entity = entitiesInBounds[i];
					this.OnPushEntity(entity);
				}
			}
		}
		this.firstUpdate = false;
	}

	public virtual void OnAddedToWorld()
	{
	}

	public virtual void OnEntityUnload()
	{
		if (this.isUnloaded)
		{
			Log.Warning("OnEntityUnload already unloaded {0} ", new object[] { this.GetDebugName() });
			return;
		}
		this.isUnloaded = true;
		Manager.DestroySoundsForEntity(this.entityId);
		if (this.movableChunkObserver != null)
		{
			this.movableChunkObserver.Dispose();
			this.movableChunkObserver = null;
		}
		if (this.attachedEntities != null)
		{
			for (int i = 0; i < this.attachedEntities.Length; i++)
			{
				Entity entity = this.attachedEntities[i];
				if (entity != null)
				{
					entity.Detach();
				}
			}
		}
		if (this.AttachedToEntity != null)
		{
			this.Detach();
		}
		if (this.emodel != null)
		{
			this.emodel.OnUnload();
		}
		try
		{
			Object.Destroy(this.RootTransform.gameObject);
		}
		catch (Exception ex)
		{
			Log.Error("OnEntityUnload: {0}", new object[] { this.GetDebugName() });
			Log.Exception(ex);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnDestroy()
	{
		this.assets.Release();
	}

	public virtual float GetLightBrightness()
	{
		Vector3i blockPosition = this.GetBlockPosition();
		Vector3i vector3i = blockPosition;
		vector3i.y += Mathf.RoundToInt(this.height + 0.5f);
		return Utils.FastMax(this.world.GetLightBrightness(blockPosition), this.world.GetLightBrightness(vector3i));
	}

	public Vector3i GetBlockPosition()
	{
		return World.worldToBlockPos(this.position);
	}

	public virtual void InitLocation(Vector3 _pos, Vector3 _rot)
	{
		this.serverPos = NetEntityDistributionEntry.EncodePos(_pos);
		this.SetPosition(_pos, true);
		this.SetRotation(_rot);
		this.ResetLastTickPos(_pos);
		base.transform.SetPositionAndRotation(this.position - Origin.position, Quaternion.Euler(this.rotation));
	}

	public Vector3 GetPosition()
	{
		return this.position;
	}

	public virtual void SetPosition(Vector3 _pos, bool _bUpdatePhysics = true)
	{
		this.position = _pos;
		float num = this.width * 0.5f;
		float num2 = this.depth * 0.5f;
		float num3 = _pos.y - this.yOffset + this.ySize;
		this.boundingBox = BoundsUtils.BoundsForMinMax(_pos.x - num, num3, _pos.z - num2, _pos.x + num, num3 + this.height, _pos.z + num2);
		if (this.attachedEntities != null)
		{
			for (int i = 0; i < this.attachedEntities.Length; i++)
			{
				Entity entity = this.attachedEntities[i];
				if (entity != null)
				{
					entity.SetPosition(_pos, false);
				}
			}
		}
		if (_bUpdatePhysics && this.PhysicsTransform != null)
		{
			this.PhysicsTransform.position = _pos - Origin.position;
			if (this.physicsRBT)
			{
				this.physicsPos = _pos - Origin.position + this.physicsBasePos;
				this.physicsRBT.position = this.physicsPos;
				this.physicsTargetPos = this.PhysicsTransform.position;
			}
		}
	}

	public void SetRotationAndStopTurning(Vector3 _rot)
	{
		this.SetRotation(_rot);
		this.yawSeekTimeMax = 0f;
		this.interpolateTargetQRot = 0;
		this.interpolateTargetRot = 0;
	}

	public virtual void SetRotation(Vector3 _rot)
	{
		this.rotation = _rot;
		this.qrotation = Quaternion.Euler(_rot);
	}

	public void SetPosAndRotFromNetwork(Vector3 _pos, Vector3 _rot, int _steps)
	{
		this.targetPos = _pos;
		this.targetRot = _rot;
		this.isUpdatePosition = true;
		this.interpolateTargetRot = _steps;
	}

	public void SetPosAndQRotFromNetwork(Vector3 _pos, Quaternion _rot, int _steps)
	{
		this.targetPos = _pos;
		this.targetQRot = _rot;
		this.isUpdatePosition = true;
		this.interpolateTargetQRot = _steps;
	}

	public void SetRotFromNetwork(Vector3 _rot, int _steps)
	{
		this.targetRot = _rot;
		this.interpolateTargetRot = _steps;
	}

	public void SetQRotFromNetwork(Quaternion _qrot, int _steps)
	{
		this.targetQRot = _qrot;
		this.interpolateTargetQRot = _steps;
	}

	public float GetBrightness(float _t)
	{
		int num = Utils.Fastfloor(this.position.x);
		int num2 = Utils.Fastfloor(this.position.z);
		if (this.world.GetChunkSync(World.toChunkXZ(num), World.toChunkXZ(num2)) != null)
		{
			float num3 = (this.boundingBox.max.y - this.boundingBox.min.y) * 0.66f;
			int num4 = Utils.Fastfloor((double)this.position.y - (double)this.yOffset + (double)num3);
			return this.world.GetLightBrightness(new Vector3i(num, num4, num2));
		}
		return 0f;
	}

	public virtual void VisiblityCheck(float _distanceSqr, bool _masterIsZooming)
	{
	}

	public void SetIgnoredByAI(bool ignore)
	{
		this.isIgnoredByAI = ignore;
	}

	public virtual bool IsIgnoredByAI()
	{
		return this.isIgnoredByAI;
	}

	public virtual Vector3 getHeadPosition()
	{
		if (this.emodel == null)
		{
			return this.position + new Vector3(0f, this.GetEyeHeight(), 0f);
		}
		return this.emodel.GetHeadPosition();
	}

	public virtual Vector3 getNavObjectPosition()
	{
		if (this.emodel == null)
		{
			return this.position + new Vector3(0f, this.GetEyeHeight(), 0f);
		}
		return this.emodel.GetNavObjectPosition();
	}

	public virtual Vector3 getBellyPosition()
	{
		if (this.emodel == null)
		{
			return this.position + new Vector3(0f, this.GetEyeHeight() / 2f, 0f);
		}
		return this.emodel.GetBellyPosition();
	}

	public virtual Vector3 getHipPosition()
	{
		if (this.emodel == null)
		{
			return this.position + new Vector3(0f, this.GetEyeHeight() / 2f, 0f);
		}
		return this.emodel.GetHipPosition();
	}

	public virtual Vector3 getChestPosition()
	{
		if (this.emodel == null)
		{
			return this.position + new Vector3(0f, this.GetEyeHeight() / 2.4f, 0f);
		}
		return this.emodel.GetChestPosition();
	}

	public void SetVelocity(Vector3 _vel)
	{
		this.motion = _vel;
	}

	public virtual float GetWeight()
	{
		return 1f;
	}

	public virtual float GetPushFactor()
	{
		return 1f;
	}

	public virtual float GetSightDetectionScale()
	{
		return 1f;
	}

	public virtual void OnLoadedFromEntityCache(EntityCreationData _ed)
	{
		if (this.bIsChunkObserver && !this.isEntityRemote)
		{
			this.movableChunkObserver = new MovableSharedChunkObserver(this.world.m_SharedChunkObserverCache);
			this.movableChunkObserver.SetPosition(this.position);
		}
	}

	public virtual bool IsSavedToNetwork()
	{
		return true;
	}

	public virtual bool IsSavedToFile()
	{
		return !this.world.IsEditor() || !GameManager.Instance.GetDynamicPrefabDecorator().IsEntityInPrefab(this.entityId);
	}

	public virtual void SetEntityName(string _name)
	{
	}

	public virtual void CopyPropertiesFromEntityClass()
	{
		EntityClass entityClass = EntityClass.list[this.entityClass];
		this.RootMotion = entityClass.RootMotion;
		this.HasDeathAnim = entityClass.HasDeathAnim;
		this.entityFlags = entityClass.entityFlags;
		this.entityType = EntityType.Unknown;
		entityClass.Properties.ParseEnum<EntityType>(EntityClass.PropEntityType, ref this.entityType);
		entityClass.Properties.ParseFloat(EntityClass.PropLootDropProb, ref this.lootDropProb);
		entityClass.Properties.ParseString(EntityClass.PropLootList, ref this.lootList);
		entityClass.Properties.ParseString(EntityClass.PropMapIcon, ref this.mapIcon);
		entityClass.Properties.ParseString(EntityClass.PropCompassIcon, ref this.compassIcon);
		entityClass.Properties.ParseString(EntityClass.PropCompassUpIcon, ref this.compassUpIcon);
		entityClass.Properties.ParseString(EntityClass.PropCompassDownIcon, ref this.compassDownIcon);
		entityClass.Properties.ParseString(EntityClass.PropTrackerIcon, ref this.trackerIcon);
		entityClass.Properties.ParseBool(EntityClass.PropRotateToGround, ref this.isRotateToGround);
		int num = 0;
		int num2 = 1;
		while (num2 <= 10 && entityClass.Properties.Values.ContainsKey(string.Format("{0}{1}", EntityClass.PropCustomCommandName, num2)))
		{
			num++;
			num2++;
		}
		this.customCmds = new EntityActivationCommand[num];
		if (num > 0)
		{
			for (int i = 1; i <= num; i++)
			{
				if (entityClass.Properties.Values.ContainsKey(string.Format("{0}{1}", EntityClass.PropCustomCommandName, i)))
				{
					EntityActivationCommand entityActivationCommand = default(EntityActivationCommand);
					entityActivationCommand.commandId = entityClass.Properties.Values[string.Format("{0}{1}", EntityClass.PropCustomCommandName, i)];
					entityActivationCommand.icon = entityClass.Properties.Values[string.Format("{0}{1}", EntityClass.PropCustomCommandIcon, i)];
					entityActivationCommand.eventName = entityClass.Properties.Values[string.Format("{0}{1}", EntityClass.PropCustomCommandEvent, i)];
					string text = string.Format("{0}{1}", EntityClass.PropCustomCommandIconColor, i);
					if (this.EntityClass.Properties.Values.ContainsKey(text))
					{
						entityActivationCommand.iconColor = StringParsers.ParseHexColor(this.EntityClass.Properties.Values[text]);
					}
					else
					{
						entityActivationCommand.iconColor = Color.white;
					}
					text = string.Format("{0}{1}", EntityClass.PropCustomCommandActivateTime, i);
					if (entityClass.Properties.Values.ContainsKey(text))
					{
						entityActivationCommand.activateTime = StringParsers.ParseFloat(entityClass.Properties.Values[text], 0, -1, NumberStyles.Any);
					}
					else
					{
						entityActivationCommand.activateTime = -1f;
					}
					entityActivationCommand.enabled = true;
					this.customCmds[i - 1] = entityActivationCommand;
				}
			}
		}
		this.activationCommands = null;
		this.lastUpdateFrameOfActivationCommands = -1;
		this.lastUpdateActivationCommandsPlayerId = -1;
		this.lastUpdateHadEnabledActivationCommands = false;
	}

	public virtual string GetLootList()
	{
		return this.lootList;
	}

	public virtual void MarkToUnload()
	{
		this.markedForUnload = true;
	}

	public virtual bool IsMarkedForUnload()
	{
		return this.markedForUnload || this.IsDead();
	}

	public virtual bool IsSpawned()
	{
		return true;
	}

	public void ResetLastTickPos(Vector3 _pos)
	{
		for (int i = 0; i < this.lastTickPos.Length; i++)
		{
			this.lastTickPos[i] = _pos;
		}
	}

	public void SetLastTickPos(Vector3 _pos)
	{
		for (int i = this.lastTickPos.Length - 1; i > 0; i--)
		{
			this.lastTickPos[i] = this.lastTickPos[i - 1];
		}
		this.lastTickPos[0] = _pos;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual bool isDetailedHeadBodyColliders()
	{
		return false;
	}

	public virtual Transform GetModelTransform()
	{
		return null;
	}

	public virtual Vector3 GetMapIconScale()
	{
		return new Vector3(1f, 1f, 1f);
	}

	public virtual string GetMapIcon()
	{
		return this.mapIcon;
	}

	public virtual string GetCompassIcon()
	{
		if (this.compassIcon == null)
		{
			return this.mapIcon;
		}
		return this.compassIcon;
	}

	public virtual string GetCompassUpIcon()
	{
		return this.compassUpIcon;
	}

	public virtual string GetCompassDownIcon()
	{
		return this.compassDownIcon;
	}

	public virtual string GetTrackerIcon()
	{
		return this.trackerIcon;
	}

	public virtual bool HasUIIcon()
	{
		return this.mapIcon != null || this.trackerIcon != null || this.compassIcon != null;
	}

	public virtual EnumMapObjectType GetMapObjectType()
	{
		return EnumMapObjectType.Entity;
	}

	public virtual bool IsMapIconBlinking()
	{
		return false;
	}

	public virtual bool IsDrawMapIcon()
	{
		return this.IsSpawned();
	}

	public virtual Color GetMapIconColor()
	{
		return Color.white;
	}

	public virtual bool CanMapIconBeSelected()
	{
		return false;
	}

	public virtual int GetLayerForMapIcon()
	{
		return 2;
	}

	public virtual bool IsClientControlled()
	{
		return this.attachedEntities != null && this.attachedEntities.Length != 0 && this.attachedEntities[0] != null;
	}

	public virtual bool IsDeadIfOutOfWorld()
	{
		return true;
	}

	public virtual bool CanCollideWithBlocks()
	{
		return true;
	}

	public void SetSpawnerSource(EnumSpawnerSource _spawnerSource)
	{
		this.SetSpawnerSource(_spawnerSource, 0L, 0);
	}

	public void SetSpawnerSource(EnumSpawnerSource _spawnerSource, long _chunkKey, int _biomeIdHash)
	{
		this.spawnerSource = _spawnerSource;
		this.spawnerSourceChunkKey = _chunkKey;
		this.spawnerSourceBiomeIdHash = _biomeIdHash;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public EnumSpawnerSource GetSpawnerSource()
	{
		return this.spawnerSource;
	}

	public long GetSpawnerSourceChunkKey()
	{
		return this.spawnerSourceChunkKey;
	}

	public int GetSpawnerSourceBiomeIdHash()
	{
		return this.spawnerSourceBiomeIdHash;
	}

	public float CalculateAudioOcclusion()
	{
		return 0f;
	}

	public virtual void PlayOneShot(string clipName, bool sound_in_head = false, bool serverSignalOnly = false, bool isUnique = false, AnimationEvent _animEvent = null, float volumeScale = 1f)
	{
		if (!sound_in_head)
		{
			if (!serverSignalOnly)
			{
				Manager.BroadcastPlay(this, clipName, serverSignalOnly, volumeScale);
				return;
			}
			Handle handle = Manager.Play(this, clipName, volumeScale, true);
			if (_animEvent != null && handle != null)
			{
				int intParameter = _animEvent.intParameter;
				if (intParameter > 0)
				{
					this.addAnimatorAudioToMonitor((Entity.StopAnimatorAudioType)intParameter, handle);
					return;
				}
			}
		}
		else
		{
			Manager.PlayInsidePlayerHead(clipName, -1, 0f, false, isUnique);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void addAnimatorAudioToMonitor(Entity.StopAnimatorAudioType _sat, Handle _handle)
	{
		Handle handle;
		if (this.animatorAudioMonitoringDictionary.TryGetValue(_sat, out handle))
		{
			handle.Stop(this.entityId);
		}
		this.animatorAudioMonitoringDictionary[_sat] = _handle;
	}

	public void StopAnimatorAudio(Entity.StopAnimatorAudioType _sat)
	{
		Handle handle;
		if (this.animatorAudioMonitoringDictionary.TryGetValue(_sat, out handle))
		{
			handle.Stop(this.entityId);
			this.animatorAudioMonitoringDictionary.Remove(_sat);
		}
	}

	public void StopOneShot(string clipName)
	{
		Manager.BroadcastStop(this.entityId, clipName);
	}

	public bool HasEnabledActivationCommands(EntityPlayerLocal _playerFocusing)
	{
		return this.UpdateActivationCommands(_playerFocusing);
	}

	public EntityActivationCommand[] GetActivationCommands()
	{
		if (this.activationCommands != null)
		{
			return this.activationCommands;
		}
		List<EntityActivationCommand> commands = new List<EntityActivationCommand>();
		this.InitLocalActivationCommands(delegate(EntityActivationCommand command)
		{
			commands.Add(command);
		});
		if (this.customCmds != null)
		{
			for (int i = 0; i < this.customCmds.Length; i++)
			{
				commands.Add(this.customCmds[i]);
			}
		}
		this.ReorderActivationCommands(commands);
		this.activationCommands = commands.ToArray();
		return this.activationCommands;
	}

	public bool UpdateActivationCommands(EntityPlayerLocal _playerFocusing)
	{
		int num = ((_playerFocusing != null) ? _playerFocusing.entityId : (-1));
		int frameCount = Time.frameCount;
		if (frameCount == this.lastUpdateFrameOfActivationCommands && num == this.lastUpdateActivationCommandsPlayerId)
		{
			return this.lastUpdateHadEnabledActivationCommands;
		}
		EntityActivationCommand[] array = this.GetActivationCommands();
		if (array == null || array.Length == 0)
		{
			this.lastUpdateFrameOfActivationCommands = frameCount;
			this.lastUpdateActivationCommandsPlayerId = num;
			this.lastUpdateHadEnabledActivationCommands = false;
			return false;
		}
		if (_playerFocusing == null)
		{
			for (int i = 0; i < array.Length; i++)
			{
				EntityActivationCommand entityActivationCommand = array[i];
				entityActivationCommand.enabled = true;
				array[i] = entityActivationCommand;
			}
			this.lastUpdateFrameOfActivationCommands = frameCount;
			this.lastUpdateActivationCommandsPlayerId = num;
			this.lastUpdateHadEnabledActivationCommands = true;
			return true;
		}
		bool flag = false;
		for (int j = 0; j < array.Length; j++)
		{
			EntityActivationCommand entityActivationCommand2 = array[j];
			entityActivationCommand2.enabled = this.AllowActivationCommand(entityActivationCommand2.commandId, _playerFocusing);
			array[j] = entityActivationCommand2;
			flag |= entityActivationCommand2.enabled;
		}
		this.lastUpdateFrameOfActivationCommands = frameCount;
		this.lastUpdateActivationCommandsPlayerId = num;
		this.lastUpdateHadEnabledActivationCommands = flag;
		return flag;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void InitLocalActivationCommands(Action<EntityActivationCommand> _addCallback)
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void ReorderActivationCommands(List<EntityActivationCommand> _commands)
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public static void MoveActivationCommandAfter(List<EntityActivationCommand> _commands, string _commandToMove, string _afterCommand)
	{
		int num = -1;
		int num2 = -1;
		for (int i = 0; i < _commands.Count; i++)
		{
			if (_commands[i].commandId == _commandToMove)
			{
				num = i;
			}
			if (_commands[i].commandId == _afterCommand)
			{
				num2 = i;
			}
		}
		if (num < 0 || num2 < 0 || num == num2)
		{
			return;
		}
		EntityActivationCommand entityActivationCommand = _commands[num];
		_commands.RemoveAt(num);
		int num3 = ((num < num2) ? num2 : (num2 + 1));
		_commands.Insert(num3, entityActivationCommand);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public static void MoveActivationCommandBefore(List<EntityActivationCommand> _commands, string _commandToMove, string _beforeCommand)
	{
		int num = -1;
		int num2 = -1;
		for (int i = 0; i < _commands.Count; i++)
		{
			if (_commands[i].commandId == _commandToMove)
			{
				num = i;
			}
			if (_commands[i].commandId == _beforeCommand)
			{
				num2 = i;
			}
		}
		if (num < 0 || num2 < 0 || num == num2)
		{
			return;
		}
		EntityActivationCommand entityActivationCommand = _commands[num];
		_commands.RemoveAt(num);
		int num3 = ((num < num2) ? (num2 - 1) : num2);
		_commands.Insert(num3, entityActivationCommand);
	}

	public virtual string GetActivationText()
	{
		return string.Empty;
	}

	public virtual bool AllowActivationCommand(ReadOnlySpan<char> _commandName, EntityPlayerLocal _playerFocusing)
	{
		return true;
	}

	public void ActivateEntityCommand(EntityActivationCommand _command, EntityPlayerLocal _playerFocusing)
	{
		this.OnEntityActivated(_command, _playerFocusing);
		if (!string.IsNullOrEmpty(_command.eventName))
		{
			EntityClass ec = EntityClass.list[this.entityClass];
			if (_command.activateTime > 0f)
			{
				TimerEventData timerEventData2 = new TimerEventData();
				timerEventData2.Data = null;
				timerEventData2.CloseOnHit = true;
				timerEventData2.FullTimeFinishEvent += delegate(TimerEventData timerEventData)
				{
					GameEventManager.Current.HandleAction(ec.onActivateEvent, _playerFocusing, this, false, "", "", false, true, "", null);
				};
				XUiC_Timer.OpenTimer(_playerFocusing.PlayerUI.xui, _command.activateTime, timerEventData2, -1f, "", true);
				return;
			}
			if (ec.onActivateEvent != "")
			{
				GameEventManager.Current.HandleAction(ec.onActivateEvent, _playerFocusing, this, false, "", "", false, true, "", null);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnEntityActivated(EntityActivationCommand _command, EntityPlayerLocal _playerFocusing)
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool CommandIs(ReadOnlySpan<char> _givenCommand, string _compareCommand)
	{
		return _givenCommand.Equals(_compareCommand, StringComparison.Ordinal);
	}

	public void Collect(int _playerId)
	{
		if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageEntityCollect>().Setup(this.entityId, _playerId), false);
			return;
		}
		this.OnCollectLocal(_playerId);
		this.OnCollectServer(_playerId);
	}

	public virtual void OnCollectServer(int _playerId)
	{
	}

	public virtual void OnCollectLocal(int _playerId)
	{
	}

	public LockTargetType LockTargetType
	{
		get
		{
			return LockTargetType.Entity;
		}
	}

	public virtual bool IsSharedLock(ushort _channel)
	{
		return false;
	}

	public virtual bool CanLockOnServer(int _lockingPlayerID, ILockContext _context, ushort _channel)
	{
		if (this.bag == null)
		{
			return false;
		}
		if (this.spawnById > 0 && this.spawnById != _lockingPlayerID)
		{
			if (TwitchManager.Current.StealingCrateEvent != "")
			{
				EntityPlayer entityPlayer = GameManager.Instance.World.GetEntity(_lockingPlayerID) as EntityPlayer;
				if (entityPlayer != null)
				{
					GameEventManager.Current.HandleAction(TwitchManager.Current.StealingCrateEvent, entityPlayer, entityPlayer, false, "", "", false, true, "", null);
				}
			}
			if (!this.spawnByAllowShare)
			{
				if (TwitchManager.Current.DeniedCrateEvent != "")
				{
					EntityPlayer entityPlayer2 = GameManager.Instance.World.GetEntity(_lockingPlayerID) as EntityPlayer;
					if (entityPlayer2 != null)
					{
						GameEventManager.Current.HandleAction(TwitchManager.Current.DeniedCrateEvent, entityPlayer2, entityPlayer2, false, "", "", false, true, "", null);
					}
				}
				return false;
			}
		}
		return true;
	}

	public virtual bool CanLockLocally(ILockContext _context, ushort _channel)
	{
		bool flag;
		if (this.bag == null)
		{
			Entity.EntityLockContext entityLockContext = _context as Entity.EntityLockContext;
			flag = entityLockContext != null && entityLockContext.Bag != null;
		}
		else
		{
			flag = true;
		}
		return flag && LocalPlayerUI.GetUIForPrimaryPlayer() != null;
	}

	public virtual void OnLockedServer(bool _success, int _lockingPlayerID, ILockContext _context, ushort _channel)
	{
		if (!_success || this.bag == null)
		{
			return;
		}
		Entity.EntityLockContext entityLockContext = _context as Entity.EntityLockContext;
		if (entityLockContext != null)
		{
			entityLockContext.FirstTimeTouched = !this.bag.Touched;
			entityLockContext.Bag = this.bag;
		}
		if (!this.bag.Touched)
		{
			GameManager.Instance.lootManager.LootBagOpened(this.bag, this, _lockingPlayerID);
		}
		this.bag.Touched = true;
	}

	public virtual void OnLockedLocal(bool _success, ILockContext _context, ushort _channel)
	{
		Entity.EntityLockContext entityLockContext = _context as Entity.EntityLockContext;
		if (entityLockContext != null && entityLockContext.Bag != null)
		{
			this.bag = entityLockContext.Bag.Clone();
		}
		LocalPlayerUI playerUI = LocalPlayerUI.GetUIForPrimaryPlayer();
		if (this.bag == null || !_success || playerUI == null)
		{
			if (playerUI != null)
			{
				GameManager.ShowTooltip(playerUI.entityPlayer, Localization.Get("ttNoInteractItem", false, null), string.Empty, "ui_denied", null, false, false, 0f);
			}
			return;
		}
		LootContainer lootContainer = LootContainer.GetLootContainer(this.GetLootList(), true);
		XUi xui = playerUI.xui;
		Bag bag = this.bag;
		LootContainer lootContainer2 = lootContainer;
		string text = Localization.Get(this.LocalizedEntityName, false, null);
		Action action = new Action(this.OnBagModified);
		Action action2 = delegate
		{
			LockManager.Instance.UnlockRequestLocal();
		};
		Func<bool> func = () => this.GetDistanceSq(playerUI.entityPlayer) <= Constants.cPlayerInteractDistance * Constants.cPlayerInteractDistance;
		Entity.EntityLockContext entityLockContext2 = _context as Entity.EntityLockContext;
		XUiC_BagStorageWindowGroup.Open(xui, this, bag, lootContainer2, text, action, action2, func, entityLockContext2 != null && entityLockContext2.FirstTimeTouched);
		if (lootContainer != null && playerUI.entityPlayer != null)
		{
			lootContainer.ExecuteBuffActions(this.entityId, playerUI.entityPlayer);
		}
		this.bag.Touched = true;
	}

	public virtual void OnUnlockedServer(int _unlockingPlayerId, ushort _channel)
	{
		if (this.bag == null)
		{
			return;
		}
		LootContainer lootContainer = LootContainer.GetLootContainer(this.GetLootList(), true);
		if (lootContainer == null)
		{
			return;
		}
		LootContainer.DestroyOnClose destroyOnClose = lootContainer.destroyOnClose;
		if (destroyOnClose != LootContainer.DestroyOnClose.True && (destroyOnClose != LootContainer.DestroyOnClose.Empty || !this.bag.IsEmpty()))
		{
			return;
		}
		if (!this.bag.IsEmpty())
		{
			this.DropBagServer();
			this.bag.Clear();
		}
		this.KillLootContainer();
	}

	public void DropBagServer()
	{
		if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			return;
		}
		if (this is EntityLootContainer)
		{
			return;
		}
		Vector3 vector = this.GetPosition();
		vector.y += 0.9f;
		if (this.lootDropProb != 0f)
		{
			EntityClass entityClass = EntityClass.list[this.entityClass];
			if (entityClass.lootDrops != null)
			{
				EntityLootContainer entityLootContainer = EntityFactory.CreateEntity(entityClass.LootDropPick(this.rand), vector, Vector3.zero) as EntityLootContainer;
				GameManager.Instance.World.SpawnEntityInWorld(entityLootContainer);
				entityLootContainer.transform.localScale = new Vector3(1.25f, 1.25f, 1.25f);
				Manager.BroadcastPlay(vector, "zpack_spawn", 0f);
				return;
			}
		}
		if (this.bag == null || this.bag.IsEmpty())
		{
			return;
		}
		EntityLootContainer entityLootContainer2 = EntityFactory.CreateEntity("DroppedLootContainer".GetHashCode(), vector, Vector3.zero) as EntityLootContainer;
		if (entityLootContainer2 == null)
		{
			return;
		}
		entityLootContainer2.OverrideLootList = this.GetLootList();
		entityLootContainer2.SetContent(ItemStack.Clone(this.bag.GetSlots()), this.bag.SlotCount);
		entityLootContainer2.bag.Touched = this.bag.Touched;
		GameManager.Instance.World.SpawnEntityInWorld(entityLootContainer2);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnBagModified()
	{
		if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			NetPackageBag netPackageBag = NetPackageManager.GetPackage<NetPackageBag>().Setup(this.entityId, this.bag);
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(netPackageBag, false);
		}
	}

	public void SetAttachMaxCount(int maxCount)
	{
		if (this.attachedEntities != null)
		{
			if (this.attachedEntities.Length == maxCount)
			{
				return;
			}
			for (int i = maxCount; i < this.attachedEntities.Length; i++)
			{
				Entity entity = this.attachedEntities[i];
				if (entity)
				{
					entity.Detach();
				}
			}
		}
		Entity[] array = this.attachedEntities;
		this.attachedEntities = null;
		if (maxCount > 0)
		{
			this.attachedEntities = new Entity[maxCount];
			if (array != null)
			{
				int num = Utils.FastMin(array.Length, maxCount);
				for (int j = 0; j < num; j++)
				{
					this.attachedEntities[j] = array[j];
				}
			}
		}
	}

	public int GetAttachMaxCount()
	{
		if (this.attachedEntities != null)
		{
			return (int)((byte)this.attachedEntities.Length);
		}
		return 0;
	}

	public int GetAttachFreeCount()
	{
		int num = 0;
		if (this.attachedEntities != null)
		{
			for (int i = 0; i < this.attachedEntities.Length; i++)
			{
				if (this.attachedEntities[i] == null)
				{
					num++;
				}
			}
		}
		return num;
	}

	public Entity GetAttached(int slot)
	{
		if (this.attachedEntities != null && slot < this.attachedEntities.Length)
		{
			return this.attachedEntities[slot];
		}
		return null;
	}

	public Entity AttachedMainEntity
	{
		get
		{
			if (this.attachedEntities == null)
			{
				return null;
			}
			return this.attachedEntities[0];
		}
	}

	public Entity GetFirstAttached()
	{
		if (this.attachedEntities != null)
		{
			for (int i = 0; i < this.attachedEntities.Length; i++)
			{
				Entity entity = this.attachedEntities[i];
				if (entity)
				{
					return entity;
				}
			}
		}
		return null;
	}

	public EntityPlayerLocal GetAttachedPlayerLocal()
	{
		if (this.attachedEntities != null)
		{
			for (int i = 0; i < this.attachedEntities.Length; i++)
			{
				EntityPlayerLocal entityPlayerLocal = this.attachedEntities[i] as EntityPlayerLocal;
				if (entityPlayerLocal)
				{
					return entityPlayerLocal;
				}
			}
		}
		return null;
	}

	public bool CanAttach(Entity _entity)
	{
		return this.FindAttachSlot(_entity) < 0 && this.FindAttachSlot(null) >= 0;
	}

	public int FindAttachSlot(Entity _entity)
	{
		if (this.attachedEntities != null)
		{
			for (int i = 0; i < this.attachedEntities.Length; i++)
			{
				if (this.attachedEntities[i] == _entity)
				{
					return i;
				}
			}
		}
		return -1;
	}

	public bool IsAttached(Entity _entity)
	{
		return this.FindAttachSlot(_entity) >= 0;
	}

	public bool IsDriven()
	{
		return this.attachedEntities != null && this.attachedEntities[0];
	}

	public virtual int AttachEntityToSelf(Entity _other, int slot)
	{
		int num = this.FindAttachSlot(_other);
		if (num >= 0)
		{
			if (slot < 0 || slot == num)
			{
				return num;
			}
			this.DetachEntity(_other);
		}
		if (slot < 0)
		{
			slot = this.FindAttachSlot(null);
			if (slot < 0)
			{
				return -1;
			}
		}
		if (slot >= this.attachedEntities.Length)
		{
			return -1;
		}
		if (slot == 0)
		{
			this.serverPos = NetEntityDistributionEntry.EncodePos(this.position);
			this.isEntityRemote = _other.isEntityRemote;
		}
		this.attachedEntities[slot] = _other;
		return slot;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void DetachEntity(Entity _other)
	{
		int num = this.FindAttachSlot(_other);
		if (num < 0)
		{
			return;
		}
		if (num == 0)
		{
			this.isEntityRemote = this.world.IsRemote();
		}
		this.attachedEntities[num] = null;
	}

	public virtual void StartAttachToEntity(Entity _other, int slot = -1)
	{
		if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageEntityAttach>().Setup(NetPackageEntityAttach.AttachType.AttachServer, this.entityId, _other.entityId, slot), false);
			return;
		}
		slot = this.AttachToEntity(_other, slot);
		if (slot >= 0)
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageEntityAttach>().Setup(NetPackageEntityAttach.AttachType.AttachClient, this.entityId, _other.entityId, slot), false, -1, -1, -1, null, 192, false);
		}
	}

	public virtual int AttachToEntity(Entity _other, int slot = -1)
	{
		if (_other.IsAttached(this))
		{
			return -1;
		}
		slot = _other.AttachEntityToSelf(this, slot);
		if (slot < 0)
		{
			return slot;
		}
		AttachedToEntitySlotInfo attachedToInfo = _other.GetAttachedToInfo(slot);
		this.RootTransform.SetParent(attachedToInfo.enterParentTransform, false);
		this.RootTransform.localPosition = Vector3.zero;
		this.RootTransform.localEulerAngles = Vector3.zero;
		this.ModelTransform.localPosition = attachedToInfo.enterPosition;
		this.ModelTransform.localEulerAngles = attachedToInfo.enterRotation;
		this.rotation = attachedToInfo.enterRotation;
		if (this.isEntityRemote && !attachedToInfo.bKeep3rdPersonModelVisible)
		{
			this.emodel.SetVisible(false, false);
		}
		this.AttachedToEntity = _other;
		return slot;
	}

	public void SendDetach()
	{
		if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageEntityAttach>().Setup(NetPackageEntityAttach.AttachType.DetachServer, this.entityId, -1, -1), false);
		}
		else
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageEntityAttach>().Setup(NetPackageEntityAttach.AttachType.DetachClient, this.entityId, -1, -1), false, -1, -1, -1, null, 192, false);
		}
		this.Detach();
	}

	public virtual void Detach()
	{
		this.RootTransform.parent = EntityFactory.ParentNameToTransform[EntityClass.list[this.entityClass].parentGameObjectName];
		if (this.AttachedToEntity == null)
		{
			return;
		}
		int num = this.AttachedToEntity.FindAttachSlot(this);
		if (num < 0)
		{
			num = 0;
		}
		AttachedToEntitySlotInfo attachedToInfo = this.AttachedToEntity.GetAttachedToInfo(num);
		AttachedToEntitySlotExit attachedToEntitySlotExit = this.FindValidExitPosition(attachedToInfo.exits);
		Entity attachedToEntity = this.AttachedToEntity;
		this.AttachedToEntity = null;
		this.isUpdatePosition = false;
		if (attachedToEntitySlotExit.position != Vector3.zero)
		{
			this.SetPosition(attachedToEntitySlotExit.position, true);
			this.SetRotation(attachedToEntitySlotExit.rotation);
		}
		this.ResetLastTickPos(base.transform.position + Origin.position);
		attachedToEntity.DetachEntity(this);
		if (this.isEntityRemote && !attachedToInfo.bKeep3rdPersonModelVisible)
		{
			this.emodel.SetVisible(true, false);
		}
	}

	public virtual void MoveByAttachedEntity(EntityPlayerLocal _player)
	{
	}

	public virtual AttachedToEntitySlotExit FindValidExitPosition(List<AttachedToEntitySlotExit> candidatePositions)
	{
		AttachedToEntitySlotExit attachedToEntitySlotExit;
		attachedToEntitySlotExit.position = Vector3.zero;
		attachedToEntitySlotExit.rotation = Vector3.zero;
		if (this.m_characterController == null)
		{
			return attachedToEntitySlotExit;
		}
		this.AttachedToEntity.SetPhysicsCollidersLayer(14);
		float radius = this.m_characterController.GetRadius();
		float num = this.m_characterController.GetHeight() - radius * 2f;
		Vector3 vector = base.transform.position + this.m_characterController.GetCenter();
		vector.y -= num * 0.5f;
		for (int i = 0; i < candidatePositions.Count; i++)
		{
			for (float num2 = 0f; num2 < 0.75f; num2 += 0.24f)
			{
				Vector3 vector2 = vector;
				vector2.y += num2;
				attachedToEntitySlotExit = candidatePositions[i];
				attachedToEntitySlotExit.position.y = attachedToEntitySlotExit.position.y + num2;
				Vector3 vector3 = attachedToEntitySlotExit.position - Origin.position - vector2;
				vector3.y += radius;
				Vector3 normalized = vector3.normalized;
				float num3 = vector3.magnitude;
				if (normalized.y < 0f)
				{
					float num4 = normalized.y;
					if (num4 < -0.707f)
					{
						break;
					}
					num4 *= -1.6f;
					num3 += num4;
					attachedToEntitySlotExit.position += normalized * num4;
				}
				bool flag = false;
				Vector3 vector4 = vector2;
				for (float num5 = -radius * 0.5f; num5 < num; num5 += 0.2f)
				{
					vector4.y = vector2.y + num5;
					flag = Physics.Raycast(vector4, normalized, num3, 1084817408);
					if (flag)
					{
						break;
					}
				}
				Vector3 vector5 = vector2 - normalized * 0.1f;
				Vector3 vector6 = vector5;
				vector6.y += num;
				if (!flag && !Physics.CapsuleCast(vector5, vector6, radius, normalized, num3, 1084817408))
				{
					this.AttachedToEntity.SetPhysicsCollidersLayer(21);
					return attachedToEntitySlotExit;
				}
			}
		}
		this.AttachedToEntity.SetPhysicsCollidersLayer(21);
		attachedToEntitySlotExit.position = Vector3.zero;
		return attachedToEntitySlotExit;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetPhysicsCollidersLayer(int layer)
	{
		Collider[] componentsInChildren = this.PhysicsTransform.GetComponentsInChildren<Collider>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].gameObject.layer = layer;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void DebugCapsuleCast()
	{
	}

	public virtual AttachedToEntitySlotInfo GetAttachedToInfo(int _slotIdx)
	{
		return null;
	}

	public virtual bool CanUpdateEntity()
	{
		Vector3i vector3i = World.worldToBlockPos(this.position);
		IChunk chunkFromWorldPos = this.world.GetChunkFromWorldPos(vector3i.x, vector3i.y, vector3i.z);
		if (chunkFromWorldPos == null || !chunkFromWorldPos.GetAvailable())
		{
			return false;
		}
		for (int i = 0; i < this.adjacentPositions.Length; i++)
		{
			int num = World.toChunkXZ(vector3i.x + this.adjacentPositions[i].x);
			int num2 = World.toChunkXZ(vector3i.z + this.adjacentPositions[i].z);
			if (num != chunkFromWorldPos.X || num2 != chunkFromWorldPos.Z)
			{
				IChunk chunkSync = this.world.GetChunkSync(num, num2);
				if (chunkSync == null || !chunkSync.GetAvailable())
				{
					return false;
				}
			}
		}
		return true;
	}

	public virtual Transform GetThirdPersonCameraTransform()
	{
		return this.emodel.GetThirdPersonCameraTransform();
	}

	public virtual void Write(BinaryWriter _bw, bool _bNetworkWrite)
	{
		_bw.Write((byte)this.spawnerSource);
		if (this.spawnerSource == EnumSpawnerSource.Biome)
		{
			_bw.Write(this.spawnerSourceBiomeIdHash);
			_bw.Write(this.spawnerSourceChunkKey);
		}
		_bw.Write(this.WorldTimeBorn);
	}

	public virtual void Read(byte _version, BinaryReader _br)
	{
		if (_version >= 11)
		{
			this.spawnerSource = (EnumSpawnerSource)_br.ReadByte();
			if (this.spawnerSource == EnumSpawnerSource.Biome)
			{
				if (_version >= 28)
				{
					this.spawnerSourceBiomeIdHash = _br.ReadInt32();
				}
				else
				{
					_br.ReadString();
					this.spawnerSource = EnumSpawnerSource.Delete;
				}
				this.spawnerSourceChunkKey = _br.ReadInt64();
			}
		}
		if (_version >= 15)
		{
			this.WorldTimeBorn = _br.ReadUInt64();
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual bool isEntityStatic()
	{
		return false;
	}

	public virtual void AddUIHarvestingItem(ItemStack _is, bool _bAddOnlyIfNotExisting = false)
	{
	}

	public virtual bool IsQRotationUsed()
	{
		return false;
	}

	public bool HasAnyTags(FastTags<TagGroup.Global> tags)
	{
		return this.cachedTags.Test_AnySet(tags);
	}

	public bool HasAllTags(FastTags<TagGroup.Global> tags)
	{
		return this.cachedTags.Test_AllSet(tags);
	}

	public void SetTransformActive(string partName, bool active)
	{
		Transform transform = base.transform.FindInChilds(partName, false);
		if (transform != null)
		{
			transform.gameObject.SetActive(active);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void HandleNavObject()
	{
		if (EntityClass.list[this.entityClass].NavObject != "")
		{
			this.NavObject = NavObjectManager.Instance.RegisterNavObject(EntityClass.list[this.entityClass].NavObject, this, "", false);
		}
	}

	public void AddNavObject(string navObjectName, string overrideSprite, string overrideText)
	{
		if (this.NavObject == null)
		{
			NavObjectManager.Instance.RegisterNavObject(navObjectName, this, overrideSprite, false).name = overrideText;
			return;
		}
		NavObjectClass navObjectClass = NavObjectClass.GetNavObjectClass(navObjectName);
		this.NavObject.name = overrideText;
		this.NavObject.AddNavObjectClass(navObjectClass);
	}

	public void RemoveNavObject(string navObjectName)
	{
		NavObjectClass navObjectClass = NavObjectClass.GetNavObjectClass(navObjectName);
		if (this.NavObject != null && this.NavObject.RemoveNavObjectClass(navObjectClass))
		{
			this.NavObject = null;
		}
	}

	public string GetDebugName()
	{
		EntityAlive entityAlive = this as EntityAlive;
		if (entityAlive != null)
		{
			return entityAlive.EntityName;
		}
		return base.GetType().ToString();
	}

	public const int EntityIdInvalid = -1;

	public const int cIdCreatorIsServer = -2;

	public const int cClientIdStart = -2;

	public const int cClientIdCreate = -1;

	public const int cClientIdNone = 0;

	public const int cKillAnythingDamage = 99999;

	public const int cIgnoreDamage = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public FastTags<TagGroup.Global> cachedTags;

	public bool RootMotion;

	public bool HasDeathAnim;

	public World world;

	public EntityInstanceAssets assets;

	public Transform PhysicsTransform;

	public Transform RootTransform;

	public Transform ModelTransform;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Vector3 scaledExtent;

	public Bounds boundingBox;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Collider nativeCollider;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public int interpolateTargetRot;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public int interpolateTargetQRot;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool isUpdatePosition;

	public int entityId;

	public int clientEntityId;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public float yOffset;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public bool wasOnGround = true;

	public bool onGround;

	public bool isCollided;

	public bool isCollidedHorizontally;

	public bool isCollidedVertically;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public bool isMotionSlowedDown;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public float motionMultiplier;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool firstUpdate = true;

	public Vector3 prevRotation;

	public Vector3 rotation;

	public Quaternion qrotation = Quaternion.identity;

	public Vector3 position;

	public Vector3 prevPos;

	public Vector3 targetPos;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Vector3 targetRot;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Quaternion targetQRot = Quaternion.identity;

	public Vector3i chunkPosAddedEntityTo;

	public Vector3i serverPos;

	public Vector3i serverRot;

	public Vector3[] lastTickPos = new Vector3[5];

	public Vector3 motion;

	public bool IsMovementReplicated = true;

	public bool IsStuck;

	public bool addedToChunk;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool isInWater;

	public bool isSwimming;

	public float inWaterLevel;

	public float inWaterPercent;

	public bool isHeadUnderwater;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public bool bInElevator;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public bool bAirBorne;

	public float stepHeight;

	public float ySize;

	public float distanceWalked;

	public float distanceSwam;

	public float distanceClimbed;

	public float fallDistance;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float fallLastY;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float fallVelY;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3 fallLastMotion;

	public float entityCollisionReduction = 0.9f;

	public bool isEntityRemote;

	public GameRandom rand;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public int ticksExisted;

	public static float updatePositionLerpTimeScale = 8f;

	public static float updateRotationLerpTimeScale = 8f;

	public static float tickPositionMoveTowardsMaxDistance = 3f;

	public static float tickPositionLerpMultiplier = 0.5f;

	public int entityClass;

	public float lifetime;

	public int count;

	public int belongsPlayerId;

	public bool bWillRespawn;

	public ulong WorldTimeBorn;

	public DataItem<bool> IsFlyMode = new DataItem<bool>();

	public DataItem<bool> IsGodMode = new DataItem<bool>();

	public DataItem<bool> IsNoCollisionMode = new DataItem<bool>();

	public EntityFlags entityFlags;

	public EntityType entityType;

	public float lootDropProb;

	public string lootList;

	public Bag bag;

	public float speedForward;

	public float speedStrafe;

	public float speedVertical;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int speedSentTicks;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float speedForwardSent = float.MaxValue;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float speedStrafeSent = float.MaxValue;

	public int MovementState;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float yawSeekTime;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float yawSeekTimeMax;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float yawSeekAngle;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float yawSeekAngleEnd;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public IAIDirectorMarker m_marker;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public string mapIcon;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public string compassIcon;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public string compassUpIcon;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public string compassDownIcon;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public string trackerIcon;

	public bool bDead;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool bWasDead;

	[Preserve]
	public EModelBase emodel;

	public CharacterControllerAbstract m_characterController;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool isCCDelayed;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool canCCMove;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public CollisionFlags collisionFlags;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Entity.MoveHitSurface groundSurface;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3 hitMove;

	public float projectedMove;

	public bool IsRotateToGroundFlat;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool isRotateToGround;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float rotateToGroundPitch;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float rotateToGroundPitchVel;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public EnumSpawnerSource spawnerSource;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int spawnerSourceBiomeIdHash;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public long spawnerSourceChunkKey;

	public static int InstanceCount;

	public bool IsDespawned;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public bool markedForUnload;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public MovableSharedChunkObserver movableChunkObserver;

	public bool bIsChunkObserver;

	public NavObject NavObject;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public bool isIgnoredByAI;

	public Entity AttachedToEntity;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Entity[] attachedEntities;

	public const int cPhysicsMasterTickRate = 2;

	public bool usePhysicsMaster;

	public bool isPhysicsMaster;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3 physicsMasterFromPos;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Quaternion physicsMasterFromRot;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float physicsMasterTargetElapsed;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float physicsMasterTargetTime;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3 physicsMasterTargetPos;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Quaternion physicsMasterTargetRot;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3 physicsMasterSendPos;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Quaternion physicsMasterSendRot;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float physicsHeightScale = 1f;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Transform physicsRBT;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public CapsuleCollider physicsCapsuleCollider;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public float physicsColliderRadius;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public float physicsColliderLowerY;

	public float physicsBaseHeight;

	public float physicsHeight;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Rigidbody physicsRB;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3 physicsPos;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3 physicsBasePos;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3 physicsTargetPos;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float physicsPosMoveDistance;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Quaternion physicsRot;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool wasFixedUpdate;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3 physicsVel;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3 physicsAngVel;

	public bool spawnByAllowShare;

	public int spawnById = -1;

	public string spawnByName;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public EntityActivationCommand[] activationCommands;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int lastUpdateFrameOfActivationCommands = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int lastUpdateActivationCommandsPlayerId = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool lastUpdateHadEnabledActivationCommands;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public EntityActivationCommand[] customCmds;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const float cWaterHeightScale = 1.1f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static float[] waterLevelDirOffsets = new float[]
	{
		Mathf.Cos(0f),
		Mathf.Sin(0f),
		Mathf.Cos(2.0943952f),
		Mathf.Sin(2.0943952f),
		Mathf.Cos(4.1887903f),
		Mathf.Sin(4.1887903f)
	};

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public List<Bounds> collAABB = new List<Bounds>();

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public float kAddFixedUpdateTimeScale = 1f;

	public EnumRemoveEntityReason unloadReason;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool isUnloaded;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public const int cAttachSlotNone = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3i[] adjacentPositions = new Vector3i[]
	{
		Vector3i.forward,
		Vector3i.back,
		Vector3i.left,
		Vector3i.right
	};

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Dictionary<Entity.StopAnimatorAudioType, Handle> animatorAudioMonitoringDictionary = new Dictionary<Entity.StopAnimatorAudioType, Handle>();

	public struct MoveHitSurface
	{
		public Vector3 hitPoint;

		public Vector3 lastHitPoint;

		public Vector3 normal;

		public Vector3 lastNormal;
	}

	public enum EnumPositionUpdateMovementType
	{
		Lerp,
		MoveTowards,
		Instant
	}

	public enum StopAnimatorAudioType
	{
		StopOnReloadCancel = 1,
		StopOnStopHolding
	}

	[Preserve]
	public class EntityLockContext : ILockContext
	{
		public string Command
		{
			get; [PublicizedFrom(EAccessModifier.Private)]
			set;
		} = string.Empty;

		public Bag Bag { get; set; }

		public bool FirstTimeTouched { get; set; }

		public EntityLockContext()
		{
		}

		public EntityLockContext(string _command, Bag _bag)
		{
			this.Command = _command ?? string.Empty;
			this.Bag = _bag;
		}

		public void Write(PooledBinaryWriter _bw)
		{
			_bw.Write(this.Command ?? string.Empty);
			_bw.Write(this.FirstTimeTouched);
			bool flag = this.Bag != null;
			_bw.Write(flag);
			if (flag)
			{
				this.Bag.Write(_bw);
			}
		}

		public void Read(PooledBinaryReader _br)
		{
			this.Command = _br.ReadString();
			this.FirstTimeTouched = _br.ReadBoolean();
			if (_br.ReadBoolean())
			{
				this.Bag = Bag.Read(_br);
				return;
			}
			this.Bag = null;
		}
	}
}
