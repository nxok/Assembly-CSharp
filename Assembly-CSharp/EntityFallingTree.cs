using System;
using System.Collections;
using System.Collections.Generic;
using Audio;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class EntityFallingTree : Entity
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public override void Awake()
	{
		base.Awake();
		this.treeRB = base.GetComponent<Rigidbody>();
		this.treeRB.useGravity = !this.isEntityRemote;
		this.treeRB.isKinematic = this.isEntityRemote;
	}

	public Vector3i GetBlockPos()
	{
		return this.treeBlockPos;
	}

	public Vector3 GetFallTreeDir()
	{
		return this.fallTreeDir;
	}

	public void SetBlockPos(Vector3i _blockPos, Vector3 _fallTreeDir)
	{
		this.treeBlockPos = _blockPos;
		this.fallTreeDir = _fallTreeDir;
		if (!this.isEntityRemote)
		{
			base.SetAirBorne(true);
		}
		Chunk chunk = (Chunk)this.world.GetChunkFromWorldPos(this.treeBlockPos);
		if (chunk == null)
		{
			return;
		}
		this.treeBV = chunk.GetBlock(World.toBlock(_blockPos));
		if (DecoManager.Instance.IsEnabled && this.treeBV.Block.IsDistantDecoration)
		{
			this.treeTransform = DecoManager.Instance.GetDecorationTransform(this.treeBlockPos, true);
		}
		else
		{
			BlockEntityData blockEntity = chunk.GetBlockEntity(this.treeBlockPos);
			if (blockEntity != null && blockEntity.bHasTransform)
			{
				this.treeTransform = blockEntity.transform;
				blockEntity.transform = null;
				blockEntity.bHasTransform = false;
			}
		}
		this.collHeight = 3f;
		if (this.treeTransform)
		{
			foreach (Collider collider in this.treeTransform.GetComponentsInChildren<Collider>())
			{
				collider.enabled = false;
				CapsuleCollider capsuleCollider = collider as CapsuleCollider;
				if (capsuleCollider != null)
				{
					this.collHeight = Utils.FastMax(this.collHeight, capsuleCollider.height);
				}
			}
		}
		this.collHeight *= 0.9f;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void OnCollisionEnter(Collision collision)
	{
		this.Collide(collision);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void OnCollisionStay(Collision collision)
	{
		this.Collide(collision);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Collide(Collision collision)
	{
		if (this.isEntityRemote)
		{
			return;
		}
		if (collision.contactCount == 0)
		{
			return;
		}
		float magnitude = collision.relativeVelocity.magnitude;
		if (magnitude > 1f)
		{
			this.collidedWith(collision.gameObject.transform);
		}
		if (magnitude > 0.2f && collision.impulse.magnitude / this.treeRB.mass > 1.5f)
		{
			Vector3 vector = base.transform.position;
			float num = -1f;
			for (int i = 0; i < collision.contactCount; i++)
			{
				ContactPoint contact = collision.GetContact(i);
				float magnitude2 = contact.impulse.magnitude;
				if (magnitude2 > num)
				{
					num = magnitude2;
					vector = contact.point;
				}
			}
			Manager.BroadcastPlay(this, "treefallimpact", false, 1f);
			ParticleEffect particleEffect = new ParticleEffect("treefall", vector + Origin.position, base.transform.rotation * Quaternion.AngleAxis(90f, Vector3.forward), 1f, Color.white, null, null, 1f, "");
			GameManager.Instance.SpawnParticleEffectServer(particleEffect, this.entityId, false, false);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void collidedWith(Transform _other)
	{
		if (this.timeToEnableDamage > 0f)
		{
			return;
		}
		Transform transform = _other;
		string text = _other.tag;
		if (text.StartsWith("E_BP_"))
		{
			transform = GameUtils.GetHitRootTransform(text, transform);
			text = transform.tag;
		}
		if (text.StartsWith("E_"))
		{
			Entity component = transform.GetComponent<Entity>();
			if (component && !component.IsDead() && this.treeCanDamageEntity(component))
			{
				this.hitEntities.Add(component.entityId);
				int num = (int)(this.treeRB.mass * 0.35999998f);
				base.StartCoroutine(this.onEntityDamageLater(component, num));
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool treeCanDamageEntity(Entity _entity)
	{
		return !this.hitEntities.Contains(_entity.entityId) && !(_entity is EntityPlayer) && !(_entity is EntitySupplyCrate);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator onEntityDamageLater(Entity _entity, int _damage)
	{
		yield return new WaitForSeconds(0.05f);
		if (!_entity.IsDead() && _damage > 10)
		{
			_entity.DamageEntity(new DamageSource(EnumDamageSource.External, EnumDamageTypes.Crushing), _damage, false, 1f);
		}
		yield break;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CreateMesh()
	{
		if (!this.isEntityRemote && this.world.GetBlock(this.treeBlockPos).type == this.treeBV.type)
		{
			this.world.SetBlockRPC(this.treeBlockPos, BlockValue.Air);
		}
		if (this.treeBV.isair)
		{
			if (this.treeTransform)
			{
				Object.Destroy(this.treeTransform.gameObject);
				this.treeTransform = null;
			}
			return;
		}
		Transform transform = base.transform;
		Vector3 position = this.treeTransform.position;
		this.SetPosition(position + Origin.position, true);
		this.SetRotation(this.treeTransform.eulerAngles);
		transform.SetPositionAndRotation(position, this.treeTransform.rotation);
		this.treeTransform.SetParent(transform, false);
		this.treeTransform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
		Transform transform2 = this.treeTransform.Find("rootBall");
		if (transform2)
		{
			transform2.gameObject.SetActive(true);
			transform2.GetComponent<MeshRenderer>().shadowCastingMode = 0;
		}
		this.treeRB.useGravity = !this.isEntityRemote;
		this.treeRB.isKinematic = this.isEntityRemote;
		this.treeRB.position = position;
		this.treeRB.rotation = this.treeTransform.rotation;
		Block block = this.treeBV.Block;
		BlockShapeModelEntity blockShapeModelEntity = block.shape as BlockShapeModelEntity;
		float num = ((blockShapeModelEntity != null) ? blockShapeModelEntity.modelOffset.y : 0f);
		RaycastHit raycastHit;
		if (Physics.SphereCast(new Ray(transform.position + 3f * Vector3.up, Vector3.down), 0.25f, ref raycastHit, 5f, -538750989))
		{
			num = transform.position.y - raycastHit.point.y;
		}
		transform.gameObject.layer = 23;
		CapsuleCollider component = transform.GetComponent<CapsuleCollider>();
		component.height = this.collHeight;
		component.center = new Vector3(0f, this.collHeight * 0.5f - num, 0f);
		component.enabled = true;
		this.treeRB.mass = (15f + 7f * this.collHeight) * 5f;
		this.treeRB.centerOfMass = new Vector3(0f, this.collHeight * 0.3f - num, 0f);
		if (!this.isEntityRemote)
		{
			this.treeRB.velocity = Vector3.zero;
			this.treeRB.angularVelocity = Vector3.zero;
			this.treeRB.solverIterations = 10;
			this.treeRB.solverVelocityIterations = 3;
			this.treeRB.AddForceAtPosition(this.fallTreeDir * ((80f + this.collHeight * 8f) * 5f), transform.position + Vector3.up * (this.collHeight * 0.65f - num), 1);
			block.SpawnDestroyParticleEffect(this.world, this.treeBV, this.treeBlockPos, this.world.GetLightBrightness(this.treeBlockPos), block.GetColorForSide(this.treeBV, BlockFace.Top), -1);
			this.lifetime = 3f;
			this.timeToEnableDamage = 1.5f;
		}
		this.rendererList.Clear();
		foreach (MeshRenderer meshRenderer in transform.GetComponentsInChildren<MeshRenderer>())
		{
			this.rendererList.Add(meshRenderer);
		}
	}

	public override void OnUpdateEntity()
	{
		base.OnUpdateEntity();
		if (this.isEntityRemote)
		{
			return;
		}
		this.timeToEnableDamage -= 0.05f;
		if (this.lifetime > 0f)
		{
			this.lifetime -= 0.05f;
			return;
		}
		if (this.timeToRemoveTree < 0f)
		{
			if (this.treeRB.angularVelocity.sqrMagnitude < 0.1f && this.treeRB.velocity.sqrMagnitude < 0.1f)
			{
				this.timeToRemoveTree = 1f;
				this.targetFade = 0f;
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<EntityFallingTree.NetPackageTreeFade>().Setup(this), false, -1, -1, -1, null, 192, false);
			}
		}
		else
		{
			this.timeToRemoveTree -= 0.05f;
			if (this.timeToRemoveTree < 0f)
			{
				this.DestroyTree();
			}
		}
		if (this.isMeshCreated && base.transform.position.y + Origin.position.y < 1f)
		{
			this.DestroyTree();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void DestroyTree()
	{
		this.SetDead();
		if (!this.isEntityRemote && this.world.GetBlock(this.treeBlockPos).type == this.treeBV.type)
		{
			this.world.SetBlockRPC(this.treeBlockPos, BlockValue.Air);
		}
		if (this.treeTransform != null)
		{
			Object.Destroy(this.treeTransform.gameObject);
			this.treeTransform = null;
		}
	}

	public override bool CanCollideWith(Entity _other)
	{
		return false;
	}

	public override bool IsQRotationUsed()
	{
		return true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void updateTransform()
	{
		if (!this.isMeshCreated && this.treeTransform)
		{
			this.isMeshCreated = true;
			this.CreateMesh();
		}
		float deltaTime = Time.deltaTime;
		this.fade = Mathf.MoveTowards(this.fade, this.targetFade, deltaTime);
		if (this.fade < 1f)
		{
			float num = this.fade;
			for (int i = 0; i < this.rendererList.Count; i++)
			{
				Renderer renderer = this.rendererList[i];
				if (renderer)
				{
					if (this.fade < 0.5f && renderer.shadowCastingMode == 3)
					{
						renderer.gameObject.SetActive(false);
					}
					renderer.GetMaterials(this.mats);
					for (int j = 0; j < this.mats.Count; j++)
					{
						if (num < 1f)
						{
							this.mats[j].EnableKeyword("ENABLE_FADEOUT");
						}
						else
						{
							this.mats[j].DisableKeyword("ENABLE_FADEOUT");
						}
						this.mats[j].SetFloat("_FadeOut", num);
					}
				}
			}
			this.mats.Clear();
		}
		Transform transform = base.transform;
		Vector3 vector = transform.position;
		if (this.isEntityRemote)
		{
			float num2 = deltaTime * 20f;
			vector = Vector3.Lerp(vector, this.targetPos - Origin.position, num2);
			Quaternion quaternion = Quaternion.Slerp(transform.rotation, this.targetQRot, num2);
			transform.SetPositionAndRotation(vector, quaternion);
			return;
		}
		this.SetPosition(vector + Origin.position, true);
		this.SetRotation(transform.eulerAngles);
	}

	public override bool IsSavedToFile()
	{
		return false;
	}

	public override void OnEntityUnload()
	{
		if (this.treeTransform != null)
		{
			Object.Destroy(this.treeTransform.gameObject);
			this.treeTransform = null;
		}
		base.OnEntityUnload();
	}

	public override void MarkToUnload()
	{
		base.MarkToUnload();
		if (!this.isEntityRemote)
		{
			this.world.SetBlockRPC(this.treeBlockPos, BlockValue.Air);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const float cMassScale = 5f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3i treeBlockPos;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public BlockValue treeBV;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3 fallTreeDir;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Transform treeTransform;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Rigidbody treeRB;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public List<MeshRenderer> rendererList = new List<MeshRenderer>();

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float collHeight;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool isMeshCreated;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float timeToRemoveTree = -1f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float timeToEnableDamage;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public List<int> hitEntities = new List<int>();

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float fade = 1f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float targetFade = 1f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public List<Material> mats = new List<Material>();

	[Preserve]
	public class NetPackageTreeFade : NetPackage
	{
		public override NetPackageDirection PackageDirection
		{
			get
			{
				return NetPackageDirection.ToClient;
			}
		}

		public EntityFallingTree.NetPackageTreeFade Setup(Entity entity)
		{
			this.entityId = entity.entityId;
			return this;
		}

		public override void read(PooledBinaryReader _reader)
		{
			this.entityId = _reader.ReadInt32();
		}

		public override void write(PooledBinaryWriter _writer)
		{
			base.write(_writer);
			_writer.Write(this.entityId);
		}

		public override void ProcessPackage(World _world, GameManager _callbacks)
		{
			if (_world == null)
			{
				return;
			}
			EntityFallingTree entityFallingTree = _world.GetEntity(this.entityId) as EntityFallingTree;
			if (entityFallingTree != null)
			{
				entityFallingTree.targetFade = 0f;
			}
		}

		public override int GetLength()
		{
			return 4;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public int entityId;
	}
}
