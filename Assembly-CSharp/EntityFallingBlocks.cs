using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class EntityFallingBlocks : Entity
{
	public override Entity.EnumPositionUpdateMovementType positionUpdateMovementType
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return Entity.EnumPositionUpdateMovementType.Instant;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void Awake()
	{
		base.Awake();
		this.yOffset = 0.15f;
		Transform transform = base.transform;
		this.rigidBody = transform.GetComponent<Rigidbody>();
		if (this.isEntityRemote)
		{
			Object.Destroy(this.rigidBody);
		}
		this.boxCollider = base.transform.GetComponent<BoxCollider>();
	}

	public override void InitLocation(Vector3 _pos, Vector3 _rot)
	{
		base.InitLocation(_pos, _rot);
		Vector2i vector2i = World.toChunkXZ(_pos);
		this.chunkKey = WorldChunkCache.MakeChunkKey(vector2i.x, vector2i.y);
		if (!EntityFallingBlocks.fallingBlocksByChunk.ContainsKey(this.chunkKey))
		{
			EntityFallingBlocks.fallingBlocksByChunk[this.chunkKey] = new List<EntityFallingBlocks>();
		}
		EntityFallingBlocks.fallingBlocksByChunk[this.chunkKey].Add(this);
		if (!this.isEntityRemote)
		{
			this.rigidBody.position = _pos - Origin.position;
			this.rigidBody.rotation = Quaternion.Euler(_rot);
		}
	}

	public BlockValue[] GetBlockValues()
	{
		return this.blockValues;
	}

	public Vector3i[] GetBlockPositions()
	{
		return this.blockPositions;
	}

	public void SetBlockGroupData(Vector3i[] _blockPositions, BlockValue[] _blockValues)
	{
		this.blockPositions = _blockPositions;
		this.blockValues = _blockValues;
	}

	public TextureFullArray[] GetTextureFullArrays()
	{
		return this.textureFullArrays;
	}

	public void SetTextureFullArrays(TextureFullArray[] _textureFullArrays)
	{
		this.textureFullArrays = _textureFullArrays;
	}

	public void SetStartVelocity(Vector3 _vel, float _angularVel)
	{
		this.startVel = _vel;
		this.startAngularVel = _angularVel;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void Update()
	{
		base.Update();
		if (!this.isMeshCreated && !GameManager.IsDedicatedServer)
		{
			this.CreateMesh();
			if (this.meshRenderer)
			{
				this.isMeshCreated = true;
				this.meshRenderer.enabled = true;
			}
		}
		if (this.boxColliders.Count > 0 && !this.boxColliders[0].enabled)
		{
			foreach (BoxCollider boxCollider in this.boxColliders)
			{
				boxCollider.enabled = true;
			}
			this.massKg = 0f;
			foreach (BlockValue blockValue in this.blockValues)
			{
				Block block = blockValue.Block;
				this.massKg += Utils.FastMin(block.blockMaterial.Hardness.Value * (float)block.blockMaterial.Mass.Value, 10f) * 8f;
			}
			if (!this.isEntityRemote)
			{
				this.rigidBody.mass = Utils.FastMax(10f, this.massKg);
				this.rigidBody.velocity = this.startVel;
				this.rigidBody.angularVelocity = this.rand.RandomOnUnitSphere * this.startAngularVel;
			}
		}
	}

	public override void OnUpdateEntity()
	{
		base.OnUpdateEntity();
		if (this.IsDead())
		{
			return;
		}
		this.fallTimeInTicks++;
		Block block = this.blockValues[0].Block;
		if (this.fallTimeInTicks == 1 && Time.time - EntityFallingBlocks.lastTimeStartParticleSpawned > 0.2f)
		{
			EntityFallingBlocks.lastTimeStartParticleSpawned = Time.time;
			if (!GameManager.IsDedicatedServer && this.prefabParticleOnFallT)
			{
				Object.Instantiate<GameObject>(this.prefabParticleOnFallT.gameObject, base.transform.position, Quaternion.identity).GetComponent<ParticleSystem>().Emit(10);
			}
		}
		if (this.isEntityRemote)
		{
			return;
		}
		Vector3 velocity = this.rigidBody.velocity;
		if ((this.fallTimeInTicks & 1) == 0)
		{
			List<Entity> entitiesInBounds = this.world.GetEntitiesInBounds(this, BoundsUtils.ExpandBounds(BoundsUtils.ExpandDirectional(this.boundingBox, this.motion), 0f, 0.2f, 0f));
			for (int i = 0; i < entitiesInBounds.Count; i++)
			{
				Entity entity = entitiesInBounds[i];
				int entityId = entity.entityId;
				int num;
				this.entityHits.TryGetValue(entityId, out num);
				if (num < 3 && entity.CanCollideWith(this) && this.position.y >= entity.getHeadPosition().y && velocity.y < -0.8f)
				{
					float num2 = (float)((int)Utils.FastMin(this.massKg * velocity.y * -0.05f, 40f));
					num2 = EffectManager.GetValue(PassiveEffects.FallingBlockDamage, null, num2, entity as EntityAlive, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false);
					int num3 = (int)num2;
					entity.DamageEntity(DamageSource.fallingBlock, num3, false, 1f);
					if (num3 >= 1)
					{
						num++;
						this.entityHits[entityId] = num;
					}
					Log.Warning("{0} EntityFallingBlocks {1} hit {2}, vel {3}, for {4}", new object[]
					{
						GameManager.frameCount,
						this,
						entity,
						velocity,
						num3
					});
				}
			}
		}
		bool flag = false;
		Transform transform = base.transform;
		if (this.fallTimeInTicks < 60 || velocity.sqrMagnitude > 0.0625f)
		{
			this.notMovingCount = 0;
		}
		else
		{
			int num4 = this.notMovingCount + 1;
			this.notMovingCount = num4;
			if (num4 > 3)
			{
				Vector3i vector3i = World.worldToBlockPos(this.position + Vector3.down);
				if (!this.world.GetBlock(vector3i).isair && this.world.GetStability(vector3i) > 0)
				{
					float time = Time.time;
					if (time - EntityFallingBlocks.lastTimeEndParticleSpawned > 0.15f)
					{
						EntityFallingBlocks.lastTimeEndParticleSpawned = time;
						Block block2 = block;
						string destroyParticle = block2.GetDestroyParticle(this.blockValues[0]);
						if (destroyParticle != null && block2.blockMaterial.SurfaceCategory != null)
						{
							Vector3i vector3i2 = World.worldToBlockPos(transform.position + new Vector3(0f, 0.5f, 0f) + Origin.position);
							this.world.GetGameManager().SpawnParticleEffectServer(new ParticleEffect("blockdestroy_" + destroyParticle, World.blockToTransformPos(vector3i2), this.world.GetLightBrightness(vector3i2), block2.GetColorForSide(this.blockValues[0], BlockFace.Top), block2.blockMaterial.SurfaceCategory + "destroy", null, false, 1f, ""), this.entityId, false, false);
						}
					}
					if (GamePrefs.GetBool(EnumGamePrefs.OptionsStabSpawnBlocksOnGround))
					{
						if (block.HasItemsToDropForEvent(EnumDropEvent.Fall))
						{
							float num5 = 1f;
							List<Block.SItemDropProb> list = block.itemsToDrop[EnumDropEvent.Fall];
							if (list.Count > 0)
							{
								num5 = list[0].prob;
							}
							block.DropItemsOnEvent(this.world, this.blockValues[0], EnumDropEvent.Fall, num5, base.GetPosition(), new Vector3(1.5f, 0f, 1.5f), Constants.cItemExplosionLifetime, -1, false);
						}
						else if (this.fallTimeInTicks < 16)
						{
							block.DropItemsOnEvent(this.world, this.blockValues[0], EnumDropEvent.Destroy, 0.7f, base.GetPosition(), new Vector3(1.5f, 0f, 1.5f), Constants.cItemExplosionLifetime, -1, false);
						}
					}
				}
				flag = true;
			}
		}
		if (this.fallTimeInTicks > 300)
		{
			flag = true;
		}
		if (transform.position.y + Origin.position.y < 2f)
		{
			flag = true;
		}
		if (flag)
		{
			this.SetDead();
		}
	}

	public override void SetDead()
	{
		EntityFallingBlocks.fallingBlocksByChunk[this.chunkKey].Remove(this);
		base.SetDead();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CreateMesh()
	{
		int num = this.blockValues[0].ToItemType();
		ItemClass forId = ItemClass.GetForId(num);
		if (num == 0 || forId == null)
		{
			Log.Error("EntityFallingBlocks failed id {0}, type", new object[] { num });
			this.SetDead();
			return;
		}
		Transform transform = base.transform;
		int meshIndex = (int)this.blockValues[0].Block.MeshIndex;
		Vector3i vector3i = World.worldToBlockPos(this.blockPositions[0]);
		byte b;
		byte b2;
		this.world.GetSunAndBlockColors(vector3i, out b, out b2);
		VoxelMesh voxelMesh = VoxelMesh.Create(meshIndex, MeshDescription.meshes[meshIndex].meshType, 1);
		VoxelMesh[] array = new VoxelMesh[MeshDescription.meshes.Length];
		array[meshIndex] = voxelMesh;
		for (int i = 0; i < this.blockValues.Length; i++)
		{
			BlockValue blockValue = this.blockValues[i];
			Vector3i vector3i2 = World.worldToBlockPos(this.blockPositions[i]);
			if (!(blockValue.Block.shape is BlockShapeTerrain))
			{
				blockValue.Block.shape.renderFull(vector3i2, blockValue, vector3i2 - vector3i + EntityFallingBlocks.renderOffsetV, null, new LightingAround(b, b2, 0), this.textureFullArrays[i], array, BlockShape.MeshPurpose.Local);
				int count = voxelMesh.m_Vertices.Count;
			}
		}
		GameObject gameObject = new GameObject();
		gameObject.transform.SetParent(transform, false);
		gameObject.AddComponent<UpdateLightOnChunkMesh>();
		gameObject.name = "Block_" + this.blockValues[0].type.ToString();
		Transform transform2 = gameObject.transform;
		MeshFilter meshFilter;
		MeshRenderer meshRenderer;
		VoxelMesh.CreateMeshFilter(meshIndex, 0, gameObject, "Item", false, out meshFilter, out meshRenderer);
		if (meshFilter != null)
		{
			voxelMesh.CopyToMesh(meshFilter, meshRenderer, 0, null);
			for (int j = 0; j < this.blockValues.Length; j++)
			{
				BlockValue[] array2 = this.blockValues;
				Vector3i vector3i3 = World.worldToBlockPos(this.blockPositions[j]);
				GameObject gameObject2 = new GameObject(string.Format("blockCollider{0}", j));
				gameObject2.transform.SetParent(transform, false);
				gameObject2.transform.localPosition = vector3i3 - vector3i;
				BoxCollider boxCollider = gameObject2.AddComponent<BoxCollider>();
				boxCollider.size = new Vector3(0.9f, 0.9f, 0.9f);
				boxCollider.material = this.boxCollider.material;
				this.boxColliders.Add(boxCollider);
			}
		}
		if (!transform2)
		{
			Log.Warning("EntityFallingBlock failed id {0}, mesh", new object[] { num });
			this.SetDead();
			return;
		}
		this.meshRenderer = transform2.GetComponentInChildren<Renderer>();
		this.meshRenderer.shadowCastingMode = 0;
		Collider[] componentsInChildren = transform2.GetComponentsInChildren<Collider>();
		for (int k = 0; k < componentsInChildren.Length; k++)
		{
			componentsInChildren[k].enabled = false;
		}
		Animator[] componentsInChildren2 = transform2.GetComponentsInChildren<Animator>();
		for (int k = 0; k < componentsInChildren2.Length; k++)
		{
			componentsInChildren2[k].enabled = false;
		}
		Utils.SetColliderLayerRecursively(transform.gameObject, 13);
	}

	public override void VisiblityCheck(float _distanceSqr, bool _masterIsZooming)
	{
		if (this.meshRenderer != null)
		{
			this.meshRenderer.enabled = _distanceSqr < (float)(_masterIsZooming ? 14400 : 10000);
		}
	}

	public override bool CanCollideWith(Entity _other)
	{
		return _other is EntityAlive;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void updateTransform()
	{
		Transform transform = base.transform;
		if (this.isEntityRemote)
		{
			if (this.targetPos.sqrMagnitude > 0f && (this.targetPos - this.interpEndPos).sqrMagnitude > 0.0001f)
			{
				this.interpStartPos = transform.position + Origin.position;
				this.interpEndPos = this.targetPos;
				this.interpTime = 0.1f;
			}
			Vector3 vector = this.targetPos;
			float deltaTime = Time.deltaTime;
			if (this.interpTime > 0f)
			{
				this.interpTime -= deltaTime;
				float num = 1f - this.interpTime / 0.1f;
				vector = Vector3.Lerp(this.interpStartPos, this.interpEndPos, num);
			}
			Quaternion quaternion = Quaternion.Slerp(transform.rotation, this.qrotation, deltaTime * 20f);
			transform.SetPositionAndRotation(vector - Origin.position, quaternion);
			return;
		}
		this.SetPosition(transform.position + Origin.position, true);
		this.SetRotation(transform.eulerAngles);
		this.qrotation = transform.rotation;
	}

	public void OnContactEvent()
	{
		if (this.isGroundHit || this.isEntityRemote)
		{
			return;
		}
		Vector3i blockPosition = base.GetBlockPosition();
		blockPosition.y--;
		BlockValue block = this.world.GetBlock(blockPosition);
		if (block.isair)
		{
			return;
		}
		this.isGroundHit = true;
		float lightBrightness = this.world.GetLightBrightness(blockPosition);
		Color colorForSide = block.Block.GetColorForSide(block, BlockFace.Top);
		string text = "impact_stone_on_" + block.Block.blockMaterial.SurfaceCategory;
		this.world.GetGameManager().SpawnParticleEffectServer(new ParticleEffect(text, base.GetPosition(), Quaternion.identity, lightBrightness, colorForSide, this.blockValues[0].Block.blockMaterial.SurfaceCategory + "hit" + block.Block.blockMaterial.SurfaceCategory, null, 1f, ""), this.entityId, false, false);
	}

	public override bool IsQRotationUsed()
	{
		return true;
	}

	public override void OnEntityUnload()
	{
		base.OnEntityUnload();
		if (this.meshRenderer != null)
		{
			Object.Destroy(this.meshRenderer.material);
		}
	}

	public static void ClearFallingBlocksForChunks(HashSetLong chunks)
	{
		List<EntityFallingBlocks> list = new List<EntityFallingBlocks>();
		foreach (long num in chunks)
		{
			if (EntityFallingBlocks.fallingBlocksByChunk.ContainsKey(num))
			{
				foreach (EntityFallingBlocks entityFallingBlocks in EntityFallingBlocks.fallingBlocksByChunk[num])
				{
					list.Add(entityFallingBlocks);
				}
			}
		}
		foreach (EntityFallingBlocks entityFallingBlocks2 in list)
		{
			entityFallingBlocks2.SetDead();
		}
	}

	public static bool Enabled = false;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static readonly Vector3 renderOffsetV = new Vector3(-0.5f, -0.5f, -0.5f);

	public static int MaxGroupSize = 3;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const int cMaxHitsPerEntity = 3;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const int cMinDamageToCountHit = 1;

	public Transform prefabParticleOnFallT;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Renderer meshRenderer;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Rigidbody rigidBody;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public BlockValue[] blockValues;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3i[] blockPositions;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float massKg;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public TextureFullArray[] textureFullArrays;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool isMeshCreated;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int fallTimeInTicks;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Dictionary<int, int> entityHits = new Dictionary<int, int>(8);

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static float lastTimeStartParticleSpawned;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static float lastTimeEndParticleSpawned;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int notMovingCount;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool isGroundHit;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public BoxCollider boxCollider;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public List<BoxCollider> boxColliders = new List<BoxCollider>();

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public long chunkKey;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static Dictionary<long, List<EntityFallingBlocks>> fallingBlocksByChunk = new Dictionary<long, List<EntityFallingBlocks>>();

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3 startVel;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float startAngularVel = 0.5f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3 interpStartPos;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3 interpEndPos;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float interpTime;
}
