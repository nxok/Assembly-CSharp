using System;
using System.Collections.Generic;
using UnityEngine;

public class RenderDisplacedCube
{
	public RenderDisplacedCube(Transform _focusCubePrefab)
	{
		this.objectShader = GlobalAssets.FindShader("Game/Glow");
		this.objectShader_TA = GlobalAssets.FindShader("Game/OverlayHighlight_TA");
		this.terrainShader = GlobalAssets.FindShader("Game/TerrainGlow");
		this.transformParent = new GameObject("FocusBox").transform;
		Origin.Add(this.transformParent, 1);
		this.transformWireframeCube = new GameObject("Wireframe").transform;
		this.transformWireframeCube.parent = this.transformParent;
		this.transformWireframeCube.localPosition = Vector3.zero;
		this.transformFocusCubePrefab = _focusCubePrefab;
		if (this.transformFocusCubePrefab != null)
		{
			this.transformFocusCubePrefab.parent = this.transformWireframeCube;
			this.transformFocusCubePrefab.localPosition = new Vector3(0.5f, 0.01f, 0.5f);
			this.bUseFocusCubePrefab = true;
			return;
		}
		this.transformWireframeCube.localScale = new Vector3(1.1f, 1.1f, 1.1f);
		this.vertices.Add(new Vector3[4]);
		this.vertices.Add(new Vector3[4]);
		this.vertices.Add(new Vector3[4]);
		this.vertices.Add(new Vector3[4]);
		this.vertices.Add(new Vector3[4]);
		this.vertices.Add(new Vector3[4]);
		Material material = new Material(Shader.Find("Transparent/Diffuse"));
		material.SetTexture("_MainTex", Resources.Load("Textures/focusbox") as Texture2D);
		for (int i = 0; i < 6; i++)
		{
			this.goSides[i] = new GameObject();
			this.goSides[i].name = "Side_" + i.ToString();
			this.goSides[i].transform.parent = this.transformWireframeCube;
			this.goSides[i].transform.localScale = Vector3.one;
			this.meshFilters[i] = this.goSides[i].AddComponent<MeshFilter>();
			MeshRenderer meshRenderer = this.goSides[i].AddComponent<MeshRenderer>();
			meshRenderer.material = material;
			meshRenderer.shadowCastingMode = 0;
			meshRenderer.receiveShadows = true;
			this.meshFilters[i].mesh.Clear();
			this.calcVerticesForSide(i, this.cube, null);
			this.meshFilters[i].mesh.uv = this.uvs;
			this.meshFilters[i].mesh.triangles = this.indices;
			this.meshFilters[i].mesh.RecalculateNormals();
		}
	}

	public void Cleanup()
	{
		if (this.previewMaterial != null)
		{
			Object.Destroy(this.previewMaterial);
			this.previewMaterial = null;
		}
		if (this.transformParent != null)
		{
			Origin.Remove(this.transformParent);
			Object.Destroy(this.transformParent.gameObject);
			this.transformParent = null;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void calcVerticesForSide(int _idx, Vector3[] _cube, Vector3[] _offsets)
	{
		for (int i = 0; i < 4; i++)
		{
			this.vertices[_idx][i] = _cube[this.sides[_idx, i]] + ((_offsets != null) ? _offsets[this.sides[_idx, i]] : Vector3.zero);
		}
		this.meshFilters[_idx].mesh.vertices = this.vertices[_idx];
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void rebuildCubeOnBounds(BlockValue _blockValue)
	{
		BlockShape shape = _blockValue.Block.shape;
		if (!(shape is BlockShapeModelEntity) && !(shape is BlockShapeNew))
		{
			this.multiDim = Vector3.one;
			this.localPos = Vector3.zero;
			return;
		}
		Bounds blockPlacementBounds = GameUtils.GetBlockPlacementBounds(_blockValue.Block);
		this.multiDim = blockPlacementBounds.size;
		this.localPos = blockPlacementBounds.center;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setFocusType(RenderCubeType _focusType)
	{
		if (this.bUseFocusCubePrefab)
		{
			return;
		}
		if (_focusType != this.focusType)
		{
			this.focusType = _focusType;
			GameObject[] array = this.goSides;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].SetActive(_focusType == RenderCubeType.FullBlockBothSides);
			}
			if (_focusType < RenderCubeType.FullBlockBothSides)
			{
				this.goSides[(int)_focusType].SetActive(true);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void GenerateLandClaimPreview(float scale)
	{
		this.cachedLandClaimBoundary = new GameObject("LandClaimBoundary");
		this.cachedLandClaimBoundary.transform.parent = this.transformFocusCubePrefab;
		this.cachedLandClaimBoundary.transform.localPosition = Vector3.zero;
		this.cachedLandClaimBoundary.transform.localRotation = Quaternion.identity;
		this.cachedLandClaimBoundary.transform.localScale = new Vector3(1f, 10000f, 1f) * scale / 1f;
		GameObject gameObject = GameObject.CreatePrimitive(3);
		Object.Destroy(gameObject.GetComponent<BoxCollider>());
		gameObject.transform.parent = this.cachedLandClaimBoundary.transform;
		gameObject.transform.localScale = Vector3.one;
		gameObject.transform.localPosition = Vector3.zero;
		gameObject.transform.localRotation = Quaternion.identity;
		Renderer component = gameObject.GetComponent<Renderer>();
		Material material = Resources.Load("Materials/LandClaimBoundary", typeof(Material)) as Material;
		component.material = material;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool update0(World _world, WorldRayHitInfo _hitInfo, Vector3i _focusBlockPos, EntityAlive _player, PersistentPlayerData _ppLocal, bool _bAlternativeBlockPos, BlockValue _holdingBlockValue, TextureFullArray _texture)
	{
		HitInfoDetails hit = _hitInfo.hit;
		ItemClassBlock.ItemBlockInventoryData itemBlockInventoryData = _player.inventory.holdingItemData as ItemClassBlock.ItemBlockInventoryData;
		BlockPlacement.EnumPlacement enumPlacement;
		BlockPlacement.EnumRotationMode enumRotationMode;
		int num;
		PropTransform propTransform;
		if (itemBlockInventoryData != null)
		{
			enumPlacement = itemBlockInventoryData.Placement;
			enumRotationMode = itemBlockInventoryData.mode;
			num = itemBlockInventoryData.localRot;
			propTransform = itemBlockInventoryData.propTransform;
		}
		else
		{
			enumPlacement = BlockPlacement.EnumPlacement.Voxel;
			enumRotationMode = BlockPlacement.EnumRotationMode.Advanced;
			num = 0;
			propTransform = PropTransform.identity;
		}
		HitInfoDetails hitInfoDetails = _hitInfo.hit.Clone();
		hitInfoDetails.blockPos = _hitInfo.lastBlockPos;
		BlockPlacement.Result result = _holdingBlockValue.Block.BlockPlacementHelper.OnPlaceBlock(enumPlacement, enumRotationMode, num, _world, _holdingBlockValue, propTransform, hitInfoDetails, _player.GetPosition());
		_holdingBlockValue.rotation = result.blockValue.rotation;
		Vector3 zero = Vector3.zero;
		_world.GetBlock(_focusBlockPos);
		this.rebuildCubeOnBounds(result.blockValue);
		if (this.focusTransformBlockValue.Block.IsTerrainDecoration && _world.GetBlock(_focusBlockPos - Vector3i.up).Block.shape.IsTerrain())
		{
			sbyte density = _world.GetDensity(_focusBlockPos);
			sbyte density2 = _world.GetDensity(_focusBlockPos - Vector3i.up);
			zero.y += MarchingCubes.GetDecorationOffsetY(density, density2);
		}
		if (!_bAlternativeBlockPos)
		{
			_focusBlockPos = _holdingBlockValue.Block.GetFreePlacementPosition(_world, _focusBlockPos, _holdingBlockValue, _player);
		}
		BlockPlacement.EnumPlacement placement = result.placement;
		Vector3 vector;
		if (placement != BlockPlacement.EnumPlacement.Voxel)
		{
			if (placement != BlockPlacement.EnumPlacement.Free)
			{
				throw new ArgumentOutOfRangeException();
			}
			vector = result.pos - result.blockPos + result.blockFace.GetNormal() * 0.5f;
		}
		else
		{
			vector = new Vector3(0.5f, 0.5f, 0.5f);
		}
		Vector3 vector2 = vector;
		Vector3 vector3 = _focusBlockPos + vector2;
		Vector3 vector4 = vector3 + zero;
		if ((vector4 - this.transformLastPos).sqrMagnitude > 64f)
		{
			this.transformLastPos = vector4;
		}
		Vector3 vector5 = Vector3.LerpUnclamped(this.transformLastPos, vector4, 0.3f);
		vector5 = Vector3.MoveTowards(vector5, vector4, 5f * Time.deltaTime);
		this.transformLastPos = vector5;
		vector5 -= Origin.position;
		bool flag = false;
		if (!this.focusTransformPosition.Equals(_focusBlockPos))
		{
			this.focusTransformPosition = _focusBlockPos;
			this.lastTimeFocusTransformMoved = Time.time;
			flag = true;
		}
		bool flag2 = false;
		if (!_bAlternativeBlockPos && ItemClass.GetForId(_holdingBlockValue.type) != null && ItemClass.GetForId(_holdingBlockValue.type).IsBlock())
		{
			bool flag3 = (false | (this.transformBlockPreview == null) | !this.focusTransformBlockValue.Equals(_holdingBlockValue) | (this.focusTransformBlockValue.rotation != _holdingBlockValue.rotation) | (this.focusedBlockFace != _hitInfo.hit.blockFace) | (this.focusedTexture != _texture)) || flag;
			flag2 = true;
			if (flag3)
			{
				if (!this.focusTransformBlockValue.Equals(_holdingBlockValue) || this.focusedTexture != _texture)
				{
					this.transparencyFade = 0f;
				}
				this.DestroyPreview();
				this.focusedBlockFace = _hitInfo.hit.blockFace;
				for (int i = 0; i < this.arrFocusBoxDisplacedVertices.Length; i++)
				{
					this.arrFocusBoxDisplacedVertices[i] = Vector3.zero;
				}
				this.focusTransformBlockValue = _holdingBlockValue;
				this.focusedTexture = _texture;
				hitInfoDetails.blockPos = _hitInfo.lastBlockPos;
				this.transformBlockPreview = ItemClassBlock.CreateMesh(null, _world, this.focusTransformBlockValue, this.arrFocusBoxDisplacedVertices, _focusBlockPos.ToVector3(), this.transformParent, BlockShape.MeshPurpose.Preview, _texture);
				if (this.transformBlockPreview != null)
				{
					this.disableAllComponents(this.transformBlockPreview);
					this.transformBlockPreview.gameObject.layer = LayerMask.NameToLayer("NotInReflections");
					Block block = this.focusTransformBlockValue.Block;
					if (block.shape.IsTerrain())
					{
						if (this.previewMaterial != null)
						{
							Object.DestroyImmediate(this.previewMaterial);
						}
						Renderer component = this.transformBlockPreview.GetComponent<Renderer>();
						if (component)
						{
							component.receiveShadows = true;
							this.previewMaterial = component.material;
							if (this.previewMaterial)
							{
								this.previewMaterial.shader = this.terrainShader;
								this.previewMaterial.SetFloat("_TexI", (float)block.TerrainTAIndex);
							}
						}
					}
					else if (block.shape is BlockShapeModelEntity)
					{
						Color color;
						color..ctor(0.7f, 0.7f, 0.7f);
						int num2 = Shader.PropertyToID("_MainTex");
						this.transformBlockPreview.GetComponentsInChildren<Renderer>(this.renderers);
						for (int j = 0; j < this.renderers.Count; j++)
						{
							foreach (Material material in this.renderers[j].materials)
							{
								material.EnableKeyword("_EMISSION");
								if (material.HasProperty(num2))
								{
									Texture texture = material.GetTexture(num2);
									material.SetTexture("_EmissionMap", texture ? texture : Texture2D.whiteTexture);
									material.SetColor("_EmissionColor", color);
								}
							}
							Material[] materials;
							this.blockPreviewMats.AddRange(materials);
						}
						this.transformBlockPreview.GetComponentsInChildren<SignRenderer>(this.signRenderers);
						float previewCanvasRotation = (float)TEFeatureCanvas.GetUprightRotation(this.focusTransformBlockValue.rotation) * 3.1415927f * 0.5f;
						SignDataManager.Instance.TryApplyRenderingData(SignData.defaultSignDataID, delegate(MaterialPropertyBlock mpb)
						{
							mpb.SetFloat(SignShaderIDs._CanvasRotation, previewCanvasRotation);
						}, this.signRenderers, SignCanvas.SignBlendMode.Cutout, null);
						Utils.SetLayerRecursively(this.transformBlockPreview.gameObject, 2);
					}
					else
					{
						if (this.previewMaterial != null)
						{
							Object.DestroyImmediate(this.previewMaterial);
						}
						Renderer component2 = this.transformBlockPreview.GetComponent<Renderer>();
						if (component2)
						{
							component2.receiveShadows = true;
							if (block.MeshIndex == 2)
							{
								component2.materials = new Material[] { component2.materials[1] };
								this.previewMaterial = component2.material;
								if (this.previewMaterial)
								{
									this.previewMaterial.shader = this.objectShader;
									this.previewMaterial.SetFloat("_Cutoff", 0f);
									this.previewMaterial.SetTexture("_MainTex", this.previewMaterial.GetTexture("_Albedo"));
								}
							}
							else
							{
								this.previewMaterial = component2.material;
								if (this.previewMaterial)
								{
									string name = this.previewMaterial.shader.name;
									if (!name.StartsWith("Game/Debug"))
									{
										if (name.EndsWith("_TA"))
										{
											this.previewMaterial.shader = this.objectShader_TA;
										}
										else
										{
											this.previewMaterial.shader = this.objectShader;
											if (!block.blockMaterial.SurfaceCategory.EqualsCaseInsensitive("glass"))
											{
												this.previewMaterial.SetFloat("_Cutoff", 0.038f);
											}
											else
											{
												this.previewMaterial.SetFloat("_Cutoff", 0f);
											}
										}
									}
									if (block.MeshIndex == 3)
									{
										Texture texture2 = this.previewMaterial.GetTexture("_Albedo");
										this.previewMaterial.SetTexture("_MainTex", texture2);
									}
								}
							}
						}
					}
				}
			}
			if (Mathf.Abs(_player.speedForward) + Mathf.Abs(_player.speedStrafe) < 0.45f)
			{
				this.transparencyFade += Time.deltaTime * 5f;
				this.transparencyFade = Mathf.Clamp01(this.transparencyFade);
			}
			else
			{
				this.transparencyFade = 0f;
			}
			float num3 = 1f * this.transparencyFade;
			if (this.transformBlockPreview)
			{
				bool flag4 = this.focusTransformBlockValue.Block.CanPlaceBlockAt(_world, _focusBlockPos, _holdingBlockValue, false);
				bool flag5 = _holdingBlockValue.Block.IndexName == "lpblock";
				bool flag6 = _holdingBlockValue.Block.IndexName == "brBlock";
				if (flag4)
				{
					if (flag5)
					{
						flag4 = _world.CanPlaceLandProtectionBlockAt(_focusBlockPos, _ppLocal);
					}
					else
					{
						flag4 = _world.CanPlaceBlockAt(_focusBlockPos, _ppLocal, false);
					}
				}
				if (flag4)
				{
					flag4 &= _player.IsGodMode.Value || !GameUtils.IsColliderWithinBlock(_focusBlockPos, _holdingBlockValue);
				}
				this.transformWireframeCube.localScale = Vector3.one;
				if (this.cachedLandClaimBoundary != null)
				{
					Object.DestroyImmediate(this.cachedLandClaimBoundary);
				}
				if (this.transformFocusCubePrefab)
				{
					if (flag5)
					{
						float num4 = (float)GameStats.GetInt(EnumGameStats.LandClaimSize);
						this.GenerateLandClaimPreview(num4);
						this.transformFocusCubePrefab.localScale = this.multiDim * 1f;
					}
					else if (flag6)
					{
						float num5 = (float)GamePrefs.GetInt(EnumGamePrefs.BedrollDeadZoneSize) * 1f;
						this.transformFocusCubePrefab.localScale = new Vector3(num5, num5, num5);
					}
					else
					{
						this.transformFocusCubePrefab.localScale = this.multiDim * 1f;
					}
					this.transformFocusCubePrefab.localPosition = this.localPos;
					this.transformFocusCubePrefab.gameObject.SetActive(true);
				}
				float num6 = num3;
				float magnitude = (_player.getHeadPosition() - vector3).magnitude;
				if (magnitude < 1f)
				{
					num6 *= magnitude / 1f;
				}
				Color color2;
				if (flag4)
				{
					color2 = Color.white;
					EnumLandClaimOwner landClaimOwner = _world.GetLandClaimOwner(_focusBlockPos, _ppLocal);
					if (landClaimOwner == EnumLandClaimOwner.Self)
					{
						color2 = Color.green;
					}
					else if (landClaimOwner == EnumLandClaimOwner.Ally)
					{
						color2 = Color.yellow;
					}
					if (this.cachedFocusBlockPos != _focusBlockPos || !this.focusTransformBlockValue.Equals(this.cachedFocusBlockValue))
					{
						this.drawStability = StabilityCalculator.GetBlockStabilityIfPlaced(_focusBlockPos, this.focusTransformBlockValue);
						this.cachedFocusBlockPos = _focusBlockPos;
						this.cachedFocusBlockValue = this.focusTransformBlockValue;
					}
				}
				else
				{
					color2 = Color.red;
					num6 *= 0.5f;
				}
				color2.r *= 0.5f;
				color2.g *= 0.5f;
				color2.b *= 0.5f;
				Renderer[] array;
				if (this.transformFocusCubePrefab)
				{
					Color color3 = color2;
					if (flag4 && this.drawStability <= 0f)
					{
						color3..ctor(1f, 0f, 0.55f);
					}
					array = this.transformFocusCubePrefab.GetComponentsInChildren<Renderer>();
					for (int l = 0; l < array.Length; l++)
					{
						array[l].material.SetColor("_Color", color3);
					}
				}
				if (this.cachedLandClaimBoundary != null)
				{
					Color color4 = (flag4 ? Color.green : Color.red);
					Renderer componentInChildren = this.cachedLandClaimBoundary.GetComponentInChildren<Renderer>();
					Color color5 = componentInChildren.material.GetColor("_BaseColor");
					componentInChildren.material.GetColor("_BoundaryColor");
					componentInChildren.material.SetColor("_BaseColor", new Color(color4.r, color4.g, color4.b, color5.a * 10f));
					componentInChildren.material.SetColor("_BoundaryColor", new Color(color4.r, color4.g, color4.b, color5.a * 10f));
				}
				Block block2 = _holdingBlockValue.Block;
				if (block2.tintColor.a > 0f)
				{
					color2 = block2.tintColor;
					color2.a = 0.5f;
				}
				Renderer[] componentsInChildren = this.transformBlockPreview.GetComponentsInChildren<Renderer>();
				bool flag7 = false;
				array = componentsInChildren;
				for (int l = 0; l < array.Length; l++)
				{
					if (array[l].shadowCastingMode == 3)
					{
						flag7 = true;
						break;
					}
				}
				foreach (Renderer renderer in componentsInChildren)
				{
					Material material2 = renderer.material;
					material2.SetInt("_BlendModeSrc", flag4 ? 1 : 5);
					material2.SetInt("_BlendModeDest", flag4 ? 0 : 1);
					material2.SetFloat("_Alpha", num6);
					material2.SetColor("_Color", color2);
					material2.SetFloat("_Stability", this.drawStability);
					if (!flag7)
					{
						renderer.enabled = num6 > 0f;
						renderer.shadowCastingMode = ((num6 > 0.1f) ? 1 : 0);
					}
					else
					{
						float num7 = 0f;
						if (renderer.shadowCastingMode == 3)
						{
							num7 = 0.1f;
						}
						renderer.enabled = num6 > num7;
					}
				}
				this.transformBlockPreview.position = vector5;
				this.transformBlockPreview.localScale = Vector3.one;
			}
		}
		else if (!_bAlternativeBlockPos && this.transformFocusCubePrefab)
		{
			this.transformFocusCubePrefab.localScale = new Vector3(1f, 1f, 1f);
			this.transformFocusCubePrefab.gameObject.SetActive(Time.time - this.lastTimeFocusTransformMoved > 1f);
		}
		this.setFocusType(this.focusType);
		this.transformWireframeCube.position = vector5;
		this.transformWireframeCube.rotation = this.focusTransformBlockValue.Block.shape.GetRotation(this.focusTransformBlockValue);
		return flag2;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void disableAllComponents(Transform _transform)
	{
		MonoBehaviour[] componentsInChildren = _transform.GetComponentsInChildren<MonoBehaviour>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].enabled = false;
		}
		Animation[] componentsInChildren2 = _transform.GetComponentsInChildren<Animation>();
		for (int j = 0; j < componentsInChildren2.Length; j++)
		{
			componentsInChildren2[j].enabled = false;
		}
		Animator[] componentsInChildren3 = _transform.GetComponentsInChildren<Animator>();
		for (int k = 0; k < componentsInChildren3.Length; k++)
		{
			componentsInChildren3[k].enabled = false;
		}
		SpriteRenderer[] componentsInChildren4 = _transform.GetComponentsInChildren<SpriteRenderer>();
		for (int l = 0; l < componentsInChildren4.Length; l++)
		{
			componentsInChildren4[l].enabled = false;
		}
		Collider[] componentsInChildren5 = _transform.GetComponentsInChildren<Collider>();
		for (int m = 0; m < componentsInChildren5.Length; m++)
		{
			Object.Destroy(componentsInChildren5[m]);
		}
	}

	public void Update(bool _bMeshSelected, World _world, WorldRayHitInfo _hitInfo, Vector3i _focusBlockPos, EntityAlive _player, PersistentPlayerData _ppLocal, bool _bAlternativeBlockPos)
	{
		bool flag = false;
		BlockValue blockValue = _player.inventory.holdingItemItemValue.ToBlockValue(false);
		Block block = blockValue.Block;
		if (block != null)
		{
			TextureFullArray textureFullArray = _player.inventory.holdingItemItemValue.TextureFullArray;
			if (block.SelectAlternates)
			{
				blockValue = block.GetAltBlockValue(_player.inventory.holdingItemItemValue.Meta);
				block = blockValue.Block;
				textureFullArray = ((block.GetAutoShapeType() != EAutoShapeType.None) ? _player.inventory.holdingItemItemValue.TextureFullArray : TextureFullArray.Default);
			}
			if (_player.inventory.holdingItemData is ItemClassBlock.ItemBlockInventoryData)
			{
				blockValue.rotation = ((ItemClassBlock.ItemBlockInventoryData)_player.inventory.holdingItemData).rotation;
			}
			RenderCubeType renderCubeType = _player.inventory.holdingItem.GetFocusType(_player.inventory.holdingItemData);
			int placementDistanceSq = block.GetPlacementDistanceSq();
			if (_hitInfo.hit.distanceSq > (float)placementDistanceSq)
			{
				renderCubeType = RenderCubeType.None;
			}
			if (_bMeshSelected && renderCubeType != RenderCubeType.None)
			{
				flag = this.update0(_world, _hitInfo, _focusBlockPos, _player, _ppLocal, _bAlternativeBlockPos, blockValue, textureFullArray);
			}
			if (this.transformWireframeCube != null && this.transformWireframeCube.gameObject != null)
			{
				this.transformWireframeCube.gameObject.SetActive(_bMeshSelected && renderCubeType != RenderCubeType.None && block.bHasPlacementWireframe);
			}
		}
		if (!flag)
		{
			this.transparencyFade = 0f;
			this.transformLastPos.x = float.MaxValue;
			this.DestroyPreview();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void DestroyPreview()
	{
		if (this.transformBlockPreview)
		{
			if (!(this.focusTransformBlockValue.Block.shape is BlockShapeModelEntity))
			{
				MeshFilter[] componentsInChildren = this.transformBlockPreview.GetComponentsInChildren<MeshFilter>();
				for (int i = 0; i < componentsInChildren.Length; i++)
				{
					VoxelMesh.AddPooledMesh(componentsInChildren[i].sharedMesh);
				}
			}
			Object.Destroy(this.transformBlockPreview.gameObject);
			this.transformBlockPreview = null;
		}
		for (int j = 0; j < this.blockPreviewMats.Count; j++)
		{
			Object.Destroy(this.blockPreviewMats[j]);
		}
		this.blockPreviewMats.Clear();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cWireframeFadeMaxDistance = 1f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cTransparencyMax = 1f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cCubeScale = 1f;

	[PublicizedFrom(EAccessModifier.Private)]
	public RenderCubeType focusType;

	[PublicizedFrom(EAccessModifier.Private)]
	public MeshFilter[] meshFilters = new MeshFilter[6];

	[PublicizedFrom(EAccessModifier.Private)]
	public GameObject[] goSides = new GameObject[6];

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3[] cube = new Vector3[]
	{
		new Vector3(0f, 0f, 0f),
		new Vector3(0f, 1f, 0f),
		new Vector3(1f, 1f, 0f),
		new Vector3(1f, 0f, 0f),
		new Vector3(0f, 0f, 1f),
		new Vector3(0f, 1f, 1f),
		new Vector3(1f, 1f, 1f),
		new Vector3(1f, 0f, 1f)
	};

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3[] cubeCopy = new Vector3[8];

	[PublicizedFrom(EAccessModifier.Private)]
	public int[,] sides = new int[,]
	{
		{ 0, 3, 2, 1 },
		{ 7, 4, 5, 6 },
		{ 4, 0, 1, 5 },
		{ 3, 7, 6, 2 },
		{ 6, 5, 1, 2 },
		{ 4, 7, 3, 0 }
	};

	[PublicizedFrom(EAccessModifier.Private)]
	public List<Vector3[]> vertices = new List<Vector3[]>();

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2[] uvs = new Vector2[]
	{
		new Vector2(0f, 0f),
		new Vector2(1f, 0f),
		new Vector2(1f, 1f),
		new Vector2(0f, 1f)
	};

	[PublicizedFrom(EAccessModifier.Private)]
	public int[] indices = new int[]
	{
		0, 2, 1, 3, 2, 0, 0, 1, 2, 3,
		0, 2
	};

	[PublicizedFrom(EAccessModifier.Private)]
	public Transform transformParent;

	[PublicizedFrom(EAccessModifier.Private)]
	public Transform transformWireframeCube;

	[PublicizedFrom(EAccessModifier.Private)]
	public Transform transformBlockPreview;

	[PublicizedFrom(EAccessModifier.Private)]
	public Transform transformFocusCubePrefab;

	[PublicizedFrom(EAccessModifier.Private)]
	public Shader objectShader;

	[PublicizedFrom(EAccessModifier.Private)]
	public Shader objectShader_TA;

	[PublicizedFrom(EAccessModifier.Private)]
	public Shader terrainShader;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<Material> blockPreviewMats = new List<Material>();

	[PublicizedFrom(EAccessModifier.Private)]
	public float lastTimeFocusTransformMoved;

	[PublicizedFrom(EAccessModifier.Private)]
	public Material previewMaterial;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3[] arrFocusBoxDisplacedVertices = new Vector3[8];

	[PublicizedFrom(EAccessModifier.Private)]
	public BlockValue focusTransformBlockValue;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3i focusTransformPosition;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bUseFocusCubePrefab;

	[PublicizedFrom(EAccessModifier.Private)]
	public TextureFullArray focusedTexture;

	[PublicizedFrom(EAccessModifier.Private)]
	public float transparencyFade;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 transformLastPos;

	[PublicizedFrom(EAccessModifier.Private)]
	public float drawStability;

	[PublicizedFrom(EAccessModifier.Private)]
	public BlockFace focusedBlockFace;

	[PublicizedFrom(EAccessModifier.Private)]
	public BlockValue cachedFocusBlockValue;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3i cachedFocusBlockPos;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 multiDim;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 localPos;

	[PublicizedFrom(EAccessModifier.Private)]
	public GameObject cachedLandClaimBoundary;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<Renderer> renderers = new List<Renderer>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<SignRenderer> signRenderers = new List<SignRenderer>();
}
