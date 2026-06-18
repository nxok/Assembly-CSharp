using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class EntityFallingBlock : Entity
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
		transform.GetComponent<BoxCollider>().enabled = false;
		transform.GetComponent<SphereCollider>().enabled = false;
	}

	public override void InitLocation(Vector3 _pos, Vector3 _rot)
	{
		base.InitLocation(_pos, _rot);
		Vector2i vector2i = World.toChunkXZ(_pos);
		this.chunkKey = WorldChunkCache.MakeChunkKey(vector2i.x, vector2i.y);
		if (!EntityFallingBlock.fallingBlocksByChunk.ContainsKey(this.chunkKey))
		{
			EntityFallingBlock.fallingBlocksByChunk[this.chunkKey] = new List<EntityFallingBlock>();
		}
		EntityFallingBlock.fallingBlocksByChunk[this.chunkKey].Add(this);
		if (!this.isEntityRemote)
		{
			this.rigidBody.position = _pos - Origin.position;
			this.rigidBody.rotation = Quaternion.Euler(_rot);
		}
	}

	public BlockValue GetBlockValue()
	{
		return this.blockValue;
	}

	public void SetBlockValue(BlockValue _blockValue)
	{
		this.blockValue = _blockValue;
		this.isTerrain = this.blockValue.Block.shape.IsTerrain();
		if (this.isTerrain)
		{
			this.terrainScale = this.rand.RandomRange(0.3f, 0.98f);
			this.myCollider = base.transform.GetComponent<SphereCollider>();
			return;
		}
		this.myCollider = base.transform.GetComponent<BoxCollider>();
	}

	public TextureFullArray GetTextureFull()
	{
		return this.textureFull;
	}

	public void SetTextureFull(TextureFullArray _textureFull)
	{
		this.textureFull = _textureFull;
	}

	public void SetCanvasState(SignCanvas.CanvasState _state)
	{
		this.pendingCanvasState = _state;
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
				if (this.isTerrain)
				{
					base.transform.localScale = new Vector3(this.terrainScale, this.terrainScale, this.terrainScale);
					TextureAtlasTerrain textureAtlasTerrain = (TextureAtlasTerrain)MeshDescription.meshes[5].textureAtlas;
					int sideTextureId = this.blockValue.Block.GetSideTextureId(this.blockValue, BlockFace.Top, 0);
					MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();
					materialPropertyBlock.SetTexture("_MainTex", textureAtlasTerrain.diffuse[sideTextureId]);
					materialPropertyBlock.SetTexture("_BumpMap", textureAtlasTerrain.normal[sideTextureId]);
					this.meshRenderer.SetPropertyBlock(materialPropertyBlock);
				}
			}
		}
		if (this.myCollider && !this.myCollider.enabled)
		{
			this.myCollider.enabled = true;
			Block block = this.blockValue.Block;
			this.massKg = Utils.FastMin(block.blockMaterial.Hardness.Value * (float)block.blockMaterial.Mass.Value, 10f) * 8f;
			this.massKg *= ((!this.isTerrain) ? (block.isMultiBlock ? 2.2f : 1f) : (this.terrainScale * this.terrainScale * 1.5f));
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
		Block block = this.blockValue.Block;
		if (this.fallTimeInTicks == 1 && Time.time - EntityFallingBlock.lastTimeStartParticleSpawned > 0.2f)
		{
			EntityFallingBlock.lastTimeStartParticleSpawned = Time.time;
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
					Log.Warning("{0} EntityFallingBlock {1} hit {2}, vel {3}, for {4}", new object[]
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
				BlockValue block2 = this.world.GetBlock(vector3i);
				if (!block2.isair && this.world.GetStability(vector3i) > 0)
				{
					float time = Time.time;
					if (time - EntityFallingBlock.lastTimeEndParticleSpawned > 0.15f)
					{
						EntityFallingBlock.lastTimeEndParticleSpawned = time;
						Block block3 = block;
						string destroyParticle = block3.GetDestroyParticle(this.blockValue);
						if (destroyParticle != null && block3.blockMaterial.SurfaceCategory != null)
						{
							Vector3i vector3i2 = World.worldToBlockPos(transform.position + new Vector3(0f, 0.5f, 0f) + Origin.position);
							this.world.GetGameManager().SpawnParticleEffectServer(new ParticleEffect("blockdestroy_" + destroyParticle, World.blockToTransformPos(vector3i2), this.world.GetLightBrightness(vector3i2), block3.GetColorForSide(this.blockValue, BlockFace.Top), block3.blockMaterial.SurfaceCategory + "destroy", null, false, 1f, ""), this.entityId, false, false);
						}
					}
					if (GamePrefs.GetBool(EnumGamePrefs.OptionsStabSpawnBlocksOnGround) && (!this.isTerrain || block2.Block.shape.IsTerrain()))
					{
						if (block.HasItemsToDropForEvent(EnumDropEvent.Fall))
						{
							float num5 = 1f;
							List<Block.SItemDropProb> list = block.itemsToDrop[EnumDropEvent.Fall];
							if (list.Count > 0)
							{
								num5 = list[0].prob;
							}
							block.DropItemsOnEvent(this.world, this.blockValue, EnumDropEvent.Fall, num5, base.GetPosition(), new Vector3(1.5f, 0f, 1.5f), Constants.cItemExplosionLifetime, -1, false);
						}
						else if (this.fallTimeInTicks < 16)
						{
							block.DropItemsOnEvent(this.world, this.blockValue, EnumDropEvent.Destroy, 0.7f, base.GetPosition(), new Vector3(1.5f, 0f, 1.5f), Constants.cItemExplosionLifetime, -1, false);
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
		SignCanvas signCanvas = this.signCanvas;
		if (signCanvas != null)
		{
			signCanvas.Cleanup();
		}
		this.signCanvas = null;
		EntityFallingBlock.fallingBlocksByChunk[this.chunkKey].Remove(this);
		base.SetDead();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CreateMesh()
	{
		int num = this.blockValue.ToItemType();
		ItemClass forId = ItemClass.GetForId(num);
		if (num == 0 || forId == null)
		{
			Log.Error("EntityFallingBlock failed id {0}, type", new object[] { num });
			this.SetDead();
			return;
		}
		Transform transform = base.transform;
		Transform transform2 = null;
		if (this.isTerrain)
		{
			GameObject gameObject = DataLoader.LoadAsset<GameObject>("@:Entities/Debris/Falling/Terrain1.prefab", false);
			if (gameObject)
			{
				transform2 = Object.Instantiate<GameObject>(gameObject).transform;
				transform2.SetParent(transform, false);
				transform2.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
			}
		}
		else
		{
			transform2 = forId.CloneModel(this.world, this.blockValue.ToItemValue(), this.position, transform, BlockShape.MeshPurpose.Local, this.textureFull);
		}
		if (!transform2)
		{
			Log.Warning("EntityFallingBlock failed id {0}, mesh", new object[] { num });
			this.SetDead();
			return;
		}
		transform2.rotation = this.blockValue.Block.shape.GetRotation(this.blockValue);
		this.meshRenderer = transform2.GetComponentInChildren<Renderer>();
		if (this.isTerrain)
		{
			return;
		}
		if (this.meshRenderer)
		{
			this.meshRenderer.shadowCastingMode = 0;
		}
		Collider[] componentsInChildren = transform2.GetComponentsInChildren<Collider>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].enabled = false;
		}
		Animator[] componentsInChildren2 = transform2.GetComponentsInChildren<Animator>();
		for (int i = 0; i < componentsInChildren2.Length; i++)
		{
			componentsInChildren2[i].enabled = false;
		}
		Utils.SetColliderLayerRecursively(transform.gameObject, 13);
		if (this.pendingCanvasState != null)
		{
			this.signCanvas = transform2.GetComponentInChildren<SignCanvas>(true);
			if (this.signCanvas != null)
			{
				this.signCanvas.State = this.pendingCanvasState;
				this.signCanvas.Initialize(null);
			}
		}
	}

	public override void VisiblityCheck(float _distanceSqr, bool _masterIsZooming)
	{
		if (this.meshRenderer)
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
		this.world.GetGameManager().SpawnParticleEffectServer(new ParticleEffect(text, base.GetPosition(), Quaternion.identity, lightBrightness, colorForSide, this.blockValue.Block.blockMaterial.SurfaceCategory + "hit" + block.Block.blockMaterial.SurfaceCategory, null, 1f, ""), this.entityId, false, false);
	}

	public override bool IsQRotationUsed()
	{
		return true;
	}

	public override void OnEntityUnload()
	{
		base.OnEntityUnload();
		if (!this.isTerrain && this.meshRenderer)
		{
			Object.Destroy(this.meshRenderer.material);
		}
	}

	public static void ClearFallingBlocksForChunks(HashSetLong chunks)
	{
		List<EntityFallingBlock> list = new List<EntityFallingBlock>();
		foreach (long num in chunks)
		{
			if (EntityFallingBlock.fallingBlocksByChunk.ContainsKey(num))
			{
				foreach (EntityFallingBlock entityFallingBlock in EntityFallingBlock.fallingBlocksByChunk[num])
				{
					list.Add(entityFallingBlock);
				}
			}
		}
		foreach (EntityFallingBlock entityFallingBlock2 in list)
		{
			entityFallingBlock2.SetDead();
		}
	}

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
	public BlockValue blockValue;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool isTerrain;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float terrainScale;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float massKg;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public TextureFullArray textureFull;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool isMeshCreated;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public SignCanvas.CanvasState pendingCanvasState;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public SignCanvas signCanvas;

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
	public Collider myCollider;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public long chunkKey;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static Dictionary<long, List<EntityFallingBlock>> fallingBlocksByChunk = new Dictionary<long, List<EntityFallingBlock>>();

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
