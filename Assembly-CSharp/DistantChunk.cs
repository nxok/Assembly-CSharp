using System;
using System.Collections.Generic;
using UnityEngine;

public class DistantChunk
{
	public DistantChunk(DistantChunkMap _BaseChunkMap, int _ResLevel, Vector2i _CellIdVector, Vector2 _TerrainOriginCoor, float[] _EdgeResFactor, Stack<GameObject>[] _waterPlaneGameObjStack)
	{
		this.ResLevel = _ResLevel;
		this.BaseChunkMap = _BaseChunkMap;
		this.ChunkMapInfo = _BaseChunkMap.ChunkMapInfoArray[_ResLevel];
		if (DistantChunk.SMPool == null)
		{
			DistantChunk.SMPool = new DChunkSquareMeshPool(128, this.BaseChunkMap.NbResLevel);
		}
		this.ChunkObjExist = false;
		this.IsChunkActivated = true;
		this.IsOnActivationProcess = false;
		this.IsFreeToUse = true;
		this.IsOnSeamCorrectionProcess = false;
		this.WasReset = false;
		this.IsMeshUpdated = false;
		this.CellIdVector = _CellIdVector;
		this.TerrainOriginCoor = new Vector3(_TerrainOriginCoor.x, 0f, _TerrainOriginCoor.y);
		this.CellKey = DistantChunk.GetCellKeyIdFromIdVector(_CellIdVector);
		this.LeftDownCoor.Set((float)_CellIdVector.x * this.ChunkMapInfo.ChunkWidth, 0f, (float)_CellIdVector.y * this.ChunkMapInfo.ChunkWidth);
		this.EdgeResFactor = (float[])_EdgeResFactor.Clone();
		this.Size = this.ChunkMapInfo.ChunkWidth;
		this.Resolution = this.ChunkMapInfo.ChunkResolution;
		this.NextResLevelEdgeFactor = this.ChunkMapInfo.NextResLevelEdgeFactor;
		this.WaterPlaneGameObjStack = _waterPlaneGameObjStack[this.ResLevel];
		if (this.ResLevel == 0)
		{
			this.WaterPlaneUnitStep = 16f;
		}
		else if (this.ResLevel == 1)
		{
			this.WaterPlaneUnitStep = 64f;
		}
		this.WaterPlaneHeight = 0f;
		this.WaterPlaneObjExist = false;
		this.WaterPlaneIsActive = false;
		int num = this.ChunkMapInfo.BaseMesh.Vertices.Length;
		this.CMDataVerticesHeight = new float[num];
		this.CMDataEdgeCorHeight = new float[this.Resolution * 4];
		if (DistantChunk.ActivateObject_NTabArray == null)
		{
			DistantChunk.ActivateObject_NTabArray = new float[this.BaseChunkMap.NbResLevel][];
		}
		if (DistantChunk.ActivateObject_NTabArray[this.ChunkMapInfo.ResLevel] == null)
		{
			DistantChunk.ActivateObject_NTabArray[this.ChunkMapInfo.ResLevel] = new float[this.Resolution * this.Resolution * 3];
		}
		this.ActivateObject_NTab = DistantChunk.ActivateObject_NTabArray[this.ChunkMapInfo.ResLevel];
		if (DistantChunk.ActivateObject_ENTabArray == null)
		{
			DistantChunk.ActivateObject_ENTabArray = new float[this.BaseChunkMap.NbResLevel][];
		}
		if (DistantChunk.ActivateObject_ENTabArray[this.ChunkMapInfo.ResLevel] == null)
		{
			DistantChunk.ActivateObject_ENTabArray[this.ChunkMapInfo.ResLevel] = new float[this.Resolution * 4 * 3];
		}
		this.ActivateObject_ENTab = DistantChunk.ActivateObject_ENTabArray[this.ChunkMapInfo.ResLevel];
		if (DistantChunk.CalcMeshTg_tan1Array == null)
		{
			DistantChunk.CalcMeshTg_tan1Array = new Vector3[this.BaseChunkMap.NbResLevel][];
		}
		if (DistantChunk.CalcMeshTg_tan1Array[this.ChunkMapInfo.ResLevel] == null)
		{
			DistantChunk.CalcMeshTg_tan1Array[this.ChunkMapInfo.ResLevel] = new Vector3[num];
		}
		this.CalcMeshTg_tan1 = DistantChunk.CalcMeshTg_tan1Array[this.ChunkMapInfo.ResLevel];
		if (DistantChunk.CalcMeshTg_tan2Array == null)
		{
			DistantChunk.CalcMeshTg_tan2Array = new Vector3[this.BaseChunkMap.NbResLevel][];
		}
		if (DistantChunk.CalcMeshTg_tan2Array[this.ChunkMapInfo.ResLevel] == null)
		{
			DistantChunk.CalcMeshTg_tan2Array[this.ChunkMapInfo.ResLevel] = new Vector3[num];
		}
		this.CalcMeshTg_tan2 = DistantChunk.CalcMeshTg_tan2Array[this.ChunkMapInfo.ResLevel];
		if (DistantChunk.TmpMeshV == null)
		{
			DistantChunk.TmpMeshV = new Vector3[this.BaseChunkMap.NbResLevel][];
		}
		if (DistantChunk.TmpMeshV[this.ResLevel] == null)
		{
			DistantChunk.TmpMeshV[this.ResLevel] = new Vector3[num];
		}
		if (DistantChunk.TmpMeshN == null)
		{
			DistantChunk.TmpMeshN = new Vector3[this.BaseChunkMap.NbResLevel][];
		}
		if (DistantChunk.TmpMeshN[this.ResLevel] == null)
		{
			DistantChunk.TmpMeshN[this.ResLevel] = new Vector3[num];
		}
		if (DistantChunk.StaticWaterSMesh == null)
		{
			DistantChunk.StaticWaterSMesh = new DistantChunk.WaterMesh[this.BaseChunkMap.NbResLevel];
		}
		if (DistantChunk.StaticWaterSMesh[this.ResLevel] == null)
		{
			int num3;
			if (this.ResLevel < 2)
			{
				int num2 = (int)((double)(this.Size / this.WaterPlaneUnitStep) + 0.0001) + 1;
				num = num2 * num2;
				num3 = (num2 - 1) * (num2 - 1) * 2;
			}
			else
			{
				num = 64;
				num3 = 32;
			}
			DistantChunk.StaticWaterSMesh[this.ResLevel] = new DistantChunk.WaterMesh(num, num3, this.Size);
		}
	}

	public void ResetDistantChunkSameResLevel(DistantChunkMap _BaseChunkMap, Vector2i _CellIdVector, Vector2 _TerrainOriginCoor, float[] _EdgeResFactor)
	{
		this.BaseChunkMap = _BaseChunkMap;
		this.ChunkMapInfo = _BaseChunkMap.ChunkMapInfoArray[this.ResLevel];
		if (this.CellMeshData == null)
		{
			this.CellMeshData = DistantChunk.SMPool.GetObject(_BaseChunkMap, this.ResLevel);
		}
		this.CellIdVector = _CellIdVector;
		this.TerrainOriginCoor = new Vector3(_TerrainOriginCoor.x, 0f, _TerrainOriginCoor.y);
		this.CellKey = DistantChunk.GetCellKeyIdFromIdVector(_CellIdVector);
		this.LeftDownCoor.Set((float)_CellIdVector.x * this.ChunkMapInfo.ChunkWidth, 0f, (float)_CellIdVector.y * this.ChunkMapInfo.ChunkWidth);
		this.EdgeResFactor = (float[])_EdgeResFactor.Clone();
		this.IsMeshUpdated = false;
		this.IsOnActivationProcess = false;
		this.IsOnSeamCorrectionProcess = false;
		this.IsChunkActivated = true;
		this.WaterPlaneObjExist = false;
		this.WaterPlaneIsActive = false;
	}

	public void Cleanup()
	{
		DistantChunk.SMPool.ReturnObject(this.CellMeshData, this.ResLevel);
		this.CellMeshData = null;
	}

	public void ResetEdgeToOwnResLevel(int _edgeId)
	{
		if (this.CellObj == null)
		{
			return;
		}
		Vector3[] vertices = this.ChunkMapInfo.BaseMesh.Vertices;
		int[][] edgeMap = this.ChunkMapInfo.EdgeMap;
		float[] cmdataVerticesHeight = this.CMDataVerticesHeight;
		for (int i = 0; i < vertices.Length; i++)
		{
			vertices[i].y = this.CMDataVerticesHeight[i];
		}
		for (int j = 0; j < this.Resolution; j++)
		{
			vertices[edgeMap[_edgeId][j]].y = cmdataVerticesHeight[edgeMap[_edgeId][j]];
		}
		this.CellObj.GetComponent<MeshFilter>().mesh.vertices = vertices;
	}

	public void ResetEdgeToNextResLevel(int _edgeId)
	{
		if (this.CellObj == null)
		{
			return;
		}
		int num = _edgeId * this.Resolution;
		Vector3[] vertices = this.ChunkMapInfo.BaseMesh.Vertices;
		int[][] edgeMap = this.ChunkMapInfo.EdgeMap;
		for (int i = 0; i < vertices.Length; i++)
		{
			vertices[i].y = this.CMDataVerticesHeight[i];
		}
		for (int j = 0; j < this.Resolution; j++)
		{
			vertices[edgeMap[_edgeId][j]].y = this.CMDataEdgeCorHeight[num + j];
		}
		this.CellObj.GetComponent<MeshFilter>().mesh.vertices = vertices;
	}

	public void ActivateObject(bool _bHeightAndNormalOnly)
	{
		int num = this.ChunkMapInfo.BaseMesh.Vertices.Length;
		int num2 = this.Resolution * this.Resolution;
		this.WasReset = true;
		float[] array = this.CMDataVerticesHeight;
		this.calculateEdgeInformation(this.LeftDownCoor.x + this.TerrainOriginCoor.x, this.LeftDownCoor.z + this.TerrainOriginCoor.z, this.Size, this.Resolution, 1f, 1f, this.NextResLevelEdgeFactor, this.NextResLevelEdgeFactor, this.NextResLevelEdgeFactor, this.NextResLevelEdgeFactor, array);
		int i = 0;
		int num3 = 0;
		while (i < this.Resolution * 4)
		{
			this.CellMeshData.EdgeCorNormals[i].x = -this.ActivateObject_ENTab[num3];
			this.CellMeshData.EdgeCorNormals[i].y = -this.ActivateObject_ENTab[num3 + 1];
			this.CellMeshData.EdgeCorNormals[i].z = -this.ActivateObject_ENTab[num3 + 2];
			i++;
			num3 += 3;
		}
		float num4 = float.MaxValue;
		float num5 = float.MinValue;
		int j = 0;
		int num6 = 0;
		while (j < num2)
		{
			if (array[j] > num5)
			{
				num5 = array[j];
			}
			if (array[j] < num4)
			{
				num4 = array[j];
			}
			this.CellMeshData.Normals[j].Set(-this.ActivateObject_NTab[num6], -this.ActivateObject_NTab[num6 + 1], -this.ActivateObject_NTab[num6 + 2]);
			j++;
			num6 += 3;
		}
		for (int k = 0; k < 4; k++)
		{
			for (int l = 0; l < this.Resolution; l++)
			{
				int num7 = k * this.Resolution + l;
				this.CellMeshData.Normals[this.ChunkMapInfo.EdgeMap[k][l]].Set(-this.ActivateObject_ENTab[num7 * 3], -this.ActivateObject_ENTab[num7 * 3 + 1], -this.ActivateObject_ENTab[num7 * 3 + 2]);
			}
		}
		bool flag = num > num2;
		if (flag)
		{
			int num8 = num2;
			for (int m = 0; m < 4; m++)
			{
				for (int n = 0; n < this.Resolution; n++)
				{
					this.CMDataVerticesHeight[num8] = this.CMDataVerticesHeight[this.ChunkMapInfo.EdgeMap[m][n]] - 35f;
					this.CellMeshData.Normals[num8].Set(this.CellMeshData.Normals[this.ChunkMapInfo.EdgeMap[m][n]].x, this.CellMeshData.Normals[this.ChunkMapInfo.EdgeMap[m][n]].y, this.CellMeshData.Normals[this.ChunkMapInfo.EdgeMap[m][n]].z);
					num8++;
				}
			}
		}
		this.CellMeshData.ChunkBound.SetMinMax(new Vector3(0f, num4, 0f), new Vector3(this.Size, num5, this.Size));
		this.calculateMeshTangents(this.CellMeshData);
		this.IsWaterActivated = false;
		array = this.CMDataVerticesHeight;
		Vector3[] vertices = this.ChunkMapInfo.BaseMesh.Vertices;
		int num9 = 0;
		int num10 = 0;
		while (num9 < array.Length)
		{
			int num11 = (int)(vertices[num9].x + this.LeftDownCoor.x + this.TerrainOriginCoor.x);
			int num12;
			if (flag && num9 >= num2)
			{
				num12 = (int)(array[num9] + this.LeftDownCoor.y + this.TerrainOriginCoor.y + 35f);
			}
			else
			{
				num12 = (int)(array[num9] + this.LeftDownCoor.y + this.TerrainOriginCoor.y);
			}
			int num13 = (int)(vertices[num9].z + this.LeftDownCoor.z + this.TerrainOriginCoor.z);
			if (this.BaseChunkMap != null && this.BaseChunkMap.wcd != null)
			{
				int num14;
				if (DistantTerrainConstants.SeaLevel > 0f && (float)num12 <= DistantTerrainConstants.SeaLevel)
				{
					num14 = 840;
					bool flag2 = !this.WaterPlaneIsActive && DistantTerrainConstants.SeaLevel >= (float)num12;
					this.IsWaterActivated = this.IsWaterActivated || flag2;
					if (flag2)
					{
						this.CellMeshData.WaterPlaneBlockId = num14;
					}
				}
				num14 = GameManager.Instance.World.m_WorldEnvironment.DistantTerrain_GetBlockIdAt(num11, num12, num13);
				Block block = Block.list[num14];
				this.CellMeshData.IsWater[num9] = block.blockMaterial.IsLiquid;
				int sideTextureId = block.GetSideTextureId(new BlockValue((uint)num14), BlockFace.Top, 0);
				int sideTextureId2 = block.GetSideTextureId(new BlockValue((uint)num14), BlockFace.South, 0);
				if (sideTextureId != -1 && sideTextureId2 != -1)
				{
					this.CellMeshData.TextureId[num9] = VoxelMeshTerrain.EncodeTexIds(sideTextureId, sideTextureId2);
				}
				else
				{
					this.CellMeshData.TextureId[num9] = -1;
				}
			}
			else
			{
				this.CellMeshData.TextureId[num9] = -1;
			}
			num9++;
			num10 += 3;
		}
		Transvoxel.BuildVertex buildVertex = default(Transvoxel.BuildVertex);
		buildVertex.bTopSoil = true;
		this.CellMeshData.VoxelMesh.ClearMesh();
		this.CurTri = this.ChunkMapInfo.BaseMesh.Triangles;
		for (int num15 = 0; num15 < this.CurTri.Length; num15 += 3)
		{
			int num16 = this.CurTri[num15];
			int num17 = this.CurTri[num15 + 1];
			int num18 = this.CurTri[num15 + 2];
			int num19;
			if (this.CellMeshData.IsWater[num16] && this.CellMeshData.IsWater[num17] && this.CellMeshData.IsWater[num18])
			{
				num19 = this.CellMeshData.VoxelMesh.FindOrCreateSubMesh(VoxelMeshTerrain.EncodeTexIds(5000, 5000), VoxelMeshTerrain.EncodeTexIds(5001, 5001), VoxelMeshTerrain.EncodeTexIds(5002, 5002));
			}
			else
			{
				int num20 = this.CellMeshData.TextureId[num16];
				int num21 = this.CellMeshData.TextureId[num17];
				int num22 = this.CellMeshData.TextureId[num18];
				if (this.CellMeshData.IsWater[num16])
				{
					num20 = VoxelMeshTerrain.EncodeTexIds(178, 178);
				}
				if (this.CellMeshData.IsWater[num17])
				{
					num21 = VoxelMeshTerrain.EncodeTexIds(178, 178);
				}
				if (this.CellMeshData.IsWater[num18])
				{
					num22 = VoxelMeshTerrain.EncodeTexIds(178, 178);
				}
				num19 = this.CellMeshData.VoxelMesh.FindOrCreateSubMesh(num20, num21, num22);
			}
			this.CellMeshData.VoxelMesh.AddIndices(num16, num17, num18, num19);
			buildVertex.texture = this.CellMeshData.TextureId[num16];
			this.CellMeshData.VoxelMesh.GetColorForTextureId(num19, ref buildVertex);
			this.CellMeshData.Colors[num16] = buildVertex.color;
			buildVertex.texture = this.CellMeshData.TextureId[num17];
			this.CellMeshData.VoxelMesh.GetColorForTextureId(num19, ref buildVertex);
			this.CellMeshData.Colors[num17] = buildVertex.color;
			buildVertex.texture = this.CellMeshData.TextureId[num18];
			this.CellMeshData.VoxelMesh.GetColorForTextureId(num19, ref buildVertex);
			this.CellMeshData.Colors[num18] = buildVertex.color;
		}
	}

	public void ActivateUnityMeshGameObject(DistantChunkBasicMesh _BasicMesh)
	{
		Vector3[] array = DistantChunk.TmpMeshV[this.ResLevel];
		Vector3[] array2 = DistantChunk.TmpMeshN[this.ResLevel];
		float[] cmdataVerticesHeight = this.CMDataVerticesHeight;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Set(this.ChunkMapInfo.BaseMesh.Vertices[i].x, cmdataVerticesHeight[i], this.ChunkMapInfo.BaseMesh.Vertices[i].z);
			array2[i].Set(this.CellMeshData.Normals[i].x, this.CellMeshData.Normals[i].y, this.CellMeshData.Normals[i].z);
		}
		for (int j = 0; j < 4; j++)
		{
			if (this.EdgeResFactor[j] > 1f)
			{
				int num = j * this.Resolution;
				for (int k = 0; k < this.Resolution; k++)
				{
					array[this.ChunkMapInfo.EdgeMap[j][k]].y = this.CMDataEdgeCorHeight[num + k];
					array2[this.ChunkMapInfo.EdgeMap[j][k]] = this.CellMeshData.EdgeCorNormals[num + k];
				}
			}
		}
		if (!this.ChunkObjExist)
		{
			this.CellObj = new GameObject("DC", new Type[]
			{
				typeof(MeshRenderer),
				typeof(MeshFilter)
			});
			this.CellObj.GetComponent<Renderer>().shadowCastingMode = 1;
			this.CellObj.GetComponent<Renderer>().receiveShadows = false;
			this.CellObj.transform.parent = this.BaseChunkMap.ParentGameObject.transform;
			if (OcclusionManager.Instance.cullDistantChunks)
			{
				Occludee.Add(this.CellObj);
			}
		}
		this.AcUMeshGO_CellMesh = this.CellObj.GetComponent<MeshFilter>().mesh;
		this.AcUMeshGO_CellMesh.vertices = DistantChunk.TmpMeshV[this.ResLevel];
		this.AcUMeshGO_CellMesh.normals = DistantChunk.TmpMeshN[this.ResLevel];
		this.AcUMeshGO_CellMesh.tangents = this.CellMeshData.Tangents;
		this.AcUMeshGO_CellMesh.bounds = this.CellMeshData.ChunkBound;
		this.AcUMeshGO_CellMesh.RecalculateBounds();
		if (!this.ChunkObjExist)
		{
			this.AcUMeshGO_CellMesh.triangles = this.ChunkMapInfo.BaseMesh.Triangles;
			this.AcUMeshGO_CellMesh.subMeshCount = 1;
		}
		this.CellObj.layer = this.ChunkMapInfo.LayerId;
		this.CellObj.SetActive(this.IsChunkActivated);
		this.ActivateWaterPlane(0f, false);
		if (this.IsWaterActivated)
		{
			this.ActivateWaterPlane(DistantTerrainConstants.SeaLevel, true);
		}
		TextureAtlasTerrain textureAtlasTerrain = (TextureAtlasTerrain)MeshDescription.meshes[5].textureAtlas;
		MeshRenderer meshRenderer = this.CellObj.GetComponent<MeshRenderer>();
		this.CellMeshData.VoxelMesh.ApplyMaterials(meshRenderer, textureAtlasTerrain, this.TilingFacFromResLevel[this.ResLevel], true);
		if (this.WaterPlaneIsActive)
		{
			MeshDescription meshDescription = MeshDescription.meshes[(int)DistantTerrainConstants.MeshIndexWater];
			meshRenderer = this.WaterPlaneObj.GetComponent<MeshRenderer>();
			Object.Destroy(meshRenderer.sharedMaterial);
			Material material = new Material(meshDescription.materialDistant);
			material.SetTextureScale("_MainTex", this.ScaleTexVec);
			material.SetTextureScale("_BumpMap", this.ScaleTexVec);
			meshRenderer.sharedMaterial = material;
			this.FlPosVec.Set(-0.5f, this.WaterPlaneHeight, -0.5f);
			this.WaterPlaneObj.transform.localPosition = this.FlPosVec;
		}
		this.AcUMeshGO_CellMesh.colors = this.CellMeshData.Colors;
		this.AcUMeshGO_CellMesh.subMeshCount = this.CellMeshData.VoxelMesh.submeshes.Count;
		for (int l = 0; l < this.CellMeshData.VoxelMesh.submeshes.Count; l++)
		{
			MeshUnsafeCopyHelper.CopyTriangles(this.CellMeshData.VoxelMesh.submeshes[l].triangles, this.AcUMeshGO_CellMesh, l);
		}
		this.CellObj.transform.localPosition = this.LeftDownCoor + this.TerrainOriginCoor + DistantChunkMap.cShiftTerrainVector + this.ChunkMapInfo.ChunkExtraShiftVector - Origin.position;
		this.ChunkObjExist = true;
		this.Cleanup();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CreateWaterPlaneMesh(float MeshWidth, GameObject _WaterPlaneObj)
	{
		if (this.ResLevel == 2)
		{
			this.createWaterPlaneMeshLowRes(_WaterPlaneObj);
			return;
		}
		DistantChunk.WaterMesh waterMesh = DistantChunk.StaticWaterSMesh[this.ResLevel];
		if (waterMesh.Normals[0].sqrMagnitude == 0f)
		{
			int num = 4;
			int num2 = (int)((double)(this.Size / MeshWidth) + 0.0001) + 1;
			Color color = Lighting.ToColor(15, 0, 1f);
			float num3 = (this.Size + this.WaterPlaneOverlapSize * 2f) / (float)(num2 - 1);
			float num4 = 1f / (float)(num2 - 1);
			float num5 = (float)num * this.BaseChunkMap.ChunkMapInfoArray[0].ChunkWidth;
			int num6 = 0;
			for (int i = 0; i < num2; i++)
			{
				for (int j = 0; j < num2; j++)
				{
					waterMesh.Vertices[num6].Set((float)i * num3, 0f, (float)j * num3);
					waterMesh.Normals[num6].Set(0f, 1f, 0f);
					waterMesh.Tangents[num6].Set(1f, 0f, 0f, -1f);
					if (this.ResLevel == 0)
					{
						int num7 = ((this.CellIdVector.x > 0) ? (this.CellIdVector.x % num) : ((num + this.CellIdVector.x % num) % num));
						int num8 = ((this.CellIdVector.y > 0) ? (this.CellIdVector.y % num) : ((num + this.CellIdVector.y % num) % num));
						waterMesh.UVVectors[num6].Set(((float)num7 + (float)i * num4) * 0.25f, ((float)num8 + (float)j * num4) * 0.25f);
					}
					else
					{
						waterMesh.UVVectors[num6].Set((float)i * MeshWidth / num5 % 1.000001f, (float)j * MeshWidth / num5 % 1.000001f);
					}
					waterMesh.Colors[num6].r = color.r;
					waterMesh.Colors[num6].g = color.g;
					waterMesh.Colors[num6].b = color.b;
					waterMesh.Colors[num6].a = color.a;
					num6++;
				}
			}
			num6 = 0;
			for (int k = 0; k < num2 - 1; k++)
			{
				for (int l = 0; l < num2 - 1; l++)
				{
					int num9 = k * num2 + l;
					waterMesh.Triangles[num6++] = num9 + 1;
					waterMesh.Triangles[num6++] = num9 + num2;
					waterMesh.Triangles[num6++] = num9;
					waterMesh.Triangles[num6++] = num9 + num2 + 1;
					waterMesh.Triangles[num6++] = num9 + num2;
					waterMesh.Triangles[num6++] = num9 + 1;
				}
			}
		}
		Mesh mesh = new Mesh();
		mesh.vertices = waterMesh.Vertices;
		mesh.normals = waterMesh.Normals;
		mesh.uv = waterMesh.UVVectors;
		mesh.triangles = waterMesh.Triangles;
		mesh.colors = waterMesh.Colors;
		mesh.subMeshCount = 1;
		mesh.tangents = waterMesh.Tangents;
		mesh.bounds = waterMesh.Bound;
		mesh.name = "WP";
		MeshFilter component = _WaterPlaneObj.GetComponent<MeshFilter>();
		if (component.sharedMesh != null)
		{
			Object.Destroy(component.sharedMesh);
		}
		component.mesh = mesh;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void createWaterPlaneMeshLowRes(GameObject _WaterPlaneObj)
	{
		DistantChunk.WaterMesh waterMesh = DistantChunk.StaticWaterSMesh[this.ResLevel];
		if (waterMesh.Normals[0].sqrMagnitude == 0f)
		{
			Color color = Lighting.ToColor(15, 0, 1f);
			float num = this.Size / 4f;
			int num2 = 0;
			for (int i = 0; i < 4; i++)
			{
				for (int j = 0; j < 4; j++)
				{
					waterMesh.UVVectors[num2].Set(0f, 0f);
					waterMesh.Vertices[num2++].Set((float)i * num, 0f, (float)j * num);
					waterMesh.UVVectors[num2].Set(1f, 0f);
					waterMesh.Vertices[num2++].Set((float)(i + 1) * num, 0f, (float)j * num);
					waterMesh.UVVectors[num2].Set(1f, 1f);
					waterMesh.Vertices[num2++].Set((float)(i + 1) * num, 0f, (float)(j + 1) * num);
					waterMesh.UVVectors[num2].Set(0f, 1f);
					waterMesh.Vertices[num2++].Set((float)i * num, 0f, (float)(j + 1) * num);
				}
			}
			for (int k = 0; k < 64; k++)
			{
				waterMesh.Normals[k].Set(0f, 1f, 0f);
				waterMesh.Tangents[k].Set(1f, 0f, 0f, -1f);
				waterMesh.Colors[k].r = color.r;
				waterMesh.Colors[k].g = color.g;
				waterMesh.Colors[k].b = color.b;
				waterMesh.Colors[k].a = color.a;
			}
			num2 = 0;
			for (int l = 0; l < 4; l++)
			{
				for (int m = 0; m < 4; m++)
				{
					int num3 = (l * 4 + m) * 4;
					waterMesh.Triangles[num2++] = num3 + 3;
					waterMesh.Triangles[num2++] = num3 + 1;
					waterMesh.Triangles[num2++] = num3;
					waterMesh.Triangles[num2++] = num3 + 2;
					waterMesh.Triangles[num2++] = num3 + 1;
					waterMesh.Triangles[num2++] = num3 + 3;
				}
			}
		}
		Mesh mesh = new Mesh();
		mesh.vertices = waterMesh.Vertices;
		mesh.normals = waterMesh.Normals;
		mesh.uv = waterMesh.UVVectors;
		mesh.triangles = waterMesh.Triangles;
		mesh.colors = waterMesh.Colors;
		mesh.subMeshCount = 1;
		mesh.tangents = waterMesh.Tangents;
		mesh.bounds = waterMesh.Bound;
		mesh.name = "WP";
		MeshFilter component = _WaterPlaneObj.GetComponent<MeshFilter>();
		if (component.sharedMesh != null)
		{
			Object.Destroy(component.sharedMesh);
		}
		component.mesh = mesh;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void createWaterPlaneObject()
	{
		if (this.WaterPlaneObjExist)
		{
			return;
		}
		if (this.WaterPlaneGameObjStack.Count > 0)
		{
			this.WaterPlaneObj = this.WaterPlaneGameObjStack.Pop();
			this.WaterPlaneObj.name = "WP";
		}
		else
		{
			this.WaterPlaneObj = new GameObject("WP", new Type[]
			{
				typeof(MeshRenderer),
				typeof(MeshFilter)
			});
			this.WaterPlaneObj.GetComponent<Renderer>().shadowCastingMode = 0;
			this.CreateWaterPlaneMesh(this.WaterPlaneUnitStep, this.WaterPlaneObj);
			this.WaterPlaneObj.layer = this.ChunkMapInfo.LayerId;
		}
		this.WaterPlaneObj.transform.parent = this.CellObj.transform;
		this.WaterPlaneObj.transform.localPosition = new Vector3(-this.WaterPlaneOverlapSize, this.WaterPlaneHeight, -this.WaterPlaneOverlapSize);
		TextureAtlasTerrain textureAtlasTerrain = (TextureAtlasTerrain)MeshDescription.meshes[5].textureAtlas;
		MeshRenderer meshRenderer = this.CellObj.GetComponent<MeshRenderer>();
		this.CellMeshData.VoxelMesh.ApplyMaterials(meshRenderer, textureAtlasTerrain, this.TilingFacFromResLevel[this.ResLevel], true);
		if (this.WaterPlaneIsActive)
		{
			MeshDescription meshDescription = MeshDescription.meshes[(int)DistantTerrainConstants.MeshIndexWater];
			meshRenderer = this.WaterPlaneObj.GetComponent<MeshRenderer>();
			Object.Destroy(meshRenderer.sharedMaterial);
			Material material = new Material(meshDescription.materialDistant);
			material.SetTextureScale("_MainTex", this.ScaleTexVec);
			material.SetTextureScale("_BumpMap", this.ScaleTexVec);
			meshRenderer.sharedMaterial = material;
			this.FlPosVec.Set(-0.5f, this.WaterPlaneHeight, -0.5f);
			this.WaterPlaneObj.transform.localPosition = this.FlPosVec;
		}
		this.WaterPlaneObj.SetActive(true);
		this.WaterPlaneObjExist = true;
	}

	public void ActivateWaterPlane(float PlaneHeight, bool IsActive)
	{
		this.WaterPlaneHeight = PlaneHeight;
		this.WaterPlaneIsActive = IsActive;
		if (!IsActive)
		{
			if (this.WaterPlaneObjExist)
			{
				this.WaterPlaneObj.SetActive(false);
				this.WaterPlaneGameObjStack.Push(this.WaterPlaneObj);
				this.WaterPlaneObj = null;
				this.WaterPlaneObjExist = false;
			}
			return;
		}
		if (!this.WaterPlaneObjExist)
		{
			this.createWaterPlaneObject();
			return;
		}
		if (this.WaterPlaneHeight != PlaneHeight)
		{
			this.WaterPlaneObj.transform.localPosition = new Vector3(-this.WaterPlaneOverlapSize, this.WaterPlaneHeight, -this.WaterPlaneOverlapSize);
		}
		this.WaterPlaneObj.SetActive(true);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void calculateMeshTangents(DChunkSquareMesh DataMesh)
	{
		this.CurTri = this.ChunkMapInfo.BaseMesh.Triangles;
		Vector3[] vertices = this.ChunkMapInfo.BaseMesh.Vertices;
		float[] cmdataVerticesHeight = this.CMDataVerticesHeight;
		int num = this.CurTri.Length;
		int num2 = vertices.Length;
		for (int i = 0; i < num2; i++)
		{
			this.CalcMeshTg_tan1[i].Set(0f, 0f, 0f);
			this.CalcMeshTg_tan2[i].Set(0f, 0f, 0f);
		}
		for (long num3 = 0L; num3 < (long)num; num3 += 3L)
		{
			long num4 = (long)this.CurTri[(int)(checked((IntPtr)num3))];
			long num5 = (long)this.CurTri[(int)(checked((IntPtr)(unchecked(num3 + 1L))))];
			long num6 = (long)this.CurTri[(int)(checked((IntPtr)(unchecked(num3 + 2L))))];
			checked
			{
				this.v1 = vertices[(int)((IntPtr)num4)];
				this.v1.y = cmdataVerticesHeight[(int)((IntPtr)num4)];
				this.v2 = vertices[(int)((IntPtr)num5)];
				this.v2.y = cmdataVerticesHeight[(int)((IntPtr)num5)];
				this.v3 = vertices[(int)((IntPtr)num6)];
				this.v3.y = cmdataVerticesHeight[(int)((IntPtr)num6)];
				this.w1.Set(0f, 0f);
				this.w2.Set(0f, 0f);
				this.w3.Set(0f, 0f);
			}
			float num7 = this.v2.x - this.v1.x;
			float num8 = this.v3.x - this.v1.x;
			float num9 = this.v2.y - this.v1.y;
			float num10 = this.v3.y - this.v1.y;
			float num11 = this.v2.z - this.v1.z;
			float num12 = this.v3.z - this.v1.z;
			float num13 = this.w2.x - this.w1.x;
			float num14 = this.w3.x - this.w1.x;
			float num15 = this.w2.y - this.w1.y;
			float num16 = this.w3.y - this.w1.y;
			float num17 = num13 * num16 - num14 * num15;
			float num18 = ((num17 == 0f) ? 0f : (1f / num17));
			this.sdir.Set((num16 * num7 - num15 * num8) * num18, (num16 * num9 - num15 * num10) * num18, (num16 * num11 - num15 * num12) * num18);
			this.tdir.Set((num13 * num8 - num14 * num7) * num18, (num13 * num10 - num14 * num9) * num18, (num13 * num12 - num14 * num11) * num18);
			this.CalcMeshTg_tan1[(int)(checked((IntPtr)num4))].Set(this.CalcMeshTg_tan1[(int)(checked((IntPtr)num4))].x + this.sdir.x, this.CalcMeshTg_tan1[(int)(checked((IntPtr)num4))].y + this.sdir.y, this.CalcMeshTg_tan1[(int)(checked((IntPtr)num4))].z + this.sdir.z);
			this.CalcMeshTg_tan1[(int)(checked((IntPtr)num5))].Set(this.CalcMeshTg_tan1[(int)(checked((IntPtr)num5))].x + this.sdir.x, this.CalcMeshTg_tan1[(int)(checked((IntPtr)num5))].y + this.sdir.y, this.CalcMeshTg_tan1[(int)(checked((IntPtr)num5))].z + this.sdir.z);
			this.CalcMeshTg_tan1[(int)(checked((IntPtr)num6))].Set(this.CalcMeshTg_tan1[(int)(checked((IntPtr)num6))].x + this.sdir.x, this.CalcMeshTg_tan1[(int)(checked((IntPtr)num6))].y + this.sdir.y, this.CalcMeshTg_tan1[(int)(checked((IntPtr)num6))].z + this.sdir.z);
			this.CalcMeshTg_tan2[(int)(checked((IntPtr)num4))].Set(this.CalcMeshTg_tan2[(int)(checked((IntPtr)num4))].x + this.tdir.x, this.CalcMeshTg_tan2[(int)(checked((IntPtr)num4))].y + this.tdir.y, this.CalcMeshTg_tan2[(int)(checked((IntPtr)num4))].z + this.tdir.z);
			this.CalcMeshTg_tan2[(int)(checked((IntPtr)num5))].Set(this.CalcMeshTg_tan2[(int)(checked((IntPtr)num5))].x + this.tdir.x, this.CalcMeshTg_tan2[(int)(checked((IntPtr)num5))].y + this.tdir.y, this.CalcMeshTg_tan2[(int)(checked((IntPtr)num5))].z + this.tdir.z);
			this.CalcMeshTg_tan2[(int)(checked((IntPtr)num6))].Set(this.CalcMeshTg_tan2[(int)(checked((IntPtr)num6))].x + this.tdir.x, this.CalcMeshTg_tan2[(int)(checked((IntPtr)num6))].y + this.tdir.y, this.CalcMeshTg_tan2[(int)(checked((IntPtr)num6))].z + this.tdir.z);
		}
		for (long num19 = 0L; num19 < (long)num2; num19 += 1L)
		{
			checked
			{
				this.n = DataMesh.Normals[(int)((IntPtr)num19)];
				this.t = this.CalcMeshTg_tan1[(int)((IntPtr)num19)];
				Vector3.OrthoNormalize(ref this.n, ref this.t);
				DataMesh.Tangents[(int)((IntPtr)num19)].x = this.t.x;
				DataMesh.Tangents[(int)((IntPtr)num19)].y = this.t.y;
				DataMesh.Tangents[(int)((IntPtr)num19)].z = this.t.z;
				DataMesh.Tangents[(int)((IntPtr)num19)].w = unchecked((this.n.y * this.t.z - this.n.z * this.t.y) * this.CalcMeshTg_tan2[(int)(checked((IntPtr)num19))].x + (this.n.z * this.t.x - this.n.x * this.t.z) * this.CalcMeshTg_tan2[(int)(checked((IntPtr)num19))].y + (this.n.x * this.t.y - this.n.y * this.t.x) * this.CalcMeshTg_tan2[(int)(checked((IntPtr)num19))].z);
				DataMesh.Tangents[(int)((IntPtr)num19)].w = ((DataMesh.Tangents[(int)((IntPtr)num19)].w < 0f) ? (-1f) : 1f);
			}
		}
	}

	public static long GetCellKeyIdFromIdVector(int[] CellIdVector)
	{
		return (((long)CellIdVector[1] & 16777215L) << 24) | ((long)CellIdVector[0] & 1048575L);
	}

	public static long GetCellKeyIdFromIdVector(Vector2i CellIdVector)
	{
		return (((long)CellIdVector.y & 16777215L) << 24) | ((long)CellIdVector.x & 1048575L);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void calculateEdgeInformation(float _LowLeftCornerX, float _LowLeftCornerZ, float _Width, int _Resolution, float _XZScale, float _YScale, float _NeighbFactorSouth, float _NeighbFactorEast, float _NeighbFactorNorth, float _NeighbFactorWest, float[] _CurVHeights)
	{
		int[] array = null;
		int[] array2 = null;
		this.EdgeFactorTab[0] = _NeighbFactorSouth;
		this.EdgeFactorTab[1] = _NeighbFactorEast;
		this.EdgeFactorTab[2] = _NeighbFactorNorth;
		this.EdgeFactorTab[3] = _NeighbFactorWest;
		_LowLeftCornerX *= _XZScale;
		_LowLeftCornerZ *= _XZScale;
		_Width *= _XZScale;
		float num = _Width / (float)(_Resolution - 1);
		int num2 = _Resolution * _Resolution;
		int num3 = 3 * _Resolution;
		int i = 0;
		int num4 = (_Resolution - 1) * _Resolution;
		int j = 0;
		while (j < _Resolution)
		{
			this.SouthTab[j] = i;
			this.EastTab[j] = num4;
			this.NorthTab[j] = num2 - 1 - i;
			this.WestTab[j] = _Resolution - 1 - j;
			this.NSouthTab[j] = i * 3;
			this.NEastTab[j] = num4 * 3;
			this.NNorthTab[j] = (num2 - 1 - i) * 3;
			this.NWestTab[j] = (_Resolution - 1 - j) * 3;
			j++;
			i += _Resolution;
			num4++;
		}
		float[] activateObject_NTab = this.ActivateObject_NTab;
		float[] cmdataEdgeCorHeight = this.CMDataEdgeCorHeight;
		float[] activateObject_ENTab = this.ActivateObject_ENTab;
		float num5;
		if (_NeighbFactorSouth < _NeighbFactorEast && _NeighbFactorSouth < _NeighbFactorNorth && _NeighbFactorSouth < _NeighbFactorWest)
		{
			num5 = _NeighbFactorSouth;
		}
		else if (_NeighbFactorEast < _NeighbFactorNorth && _NeighbFactorEast < _NeighbFactorWest)
		{
			num5 = _NeighbFactorEast;
		}
		else if (_NeighbFactorNorth < _NeighbFactorWest)
		{
			num5 = _NeighbFactorNorth;
		}
		else
		{
			num5 = _NeighbFactorWest;
		}
		int num6;
		if (num5 < 1f)
		{
			num6 = (int)((float)_Resolution / num5 + 1f);
		}
		else
		{
			num6 = _Resolution;
		}
		float[] array3 = new float[num6 * 2];
		float[] array4 = new float[num6 * 6];
		for (j = 0; j < _Resolution; j++)
		{
			for (i = 0; i < _Resolution; i++)
			{
				_CurVHeights[i + j * _Resolution] = DistantChunkMap.TGHeightFunc((float)j * num + _LowLeftCornerX, 0f, (float)i * num + _LowLeftCornerZ) * _YScale;
			}
		}
		for (j = 0; j < num2 * 3; j++)
		{
			activateObject_NTab[j] = 0f;
		}
		float num7 = -1f;
		int num8 = 0;
		for (j = 0; j < _Resolution - 1; j++)
		{
			for (i = 0; i < _Resolution - 1; i++)
			{
				int num9 = i + num8;
				int num10 = num9 * 3;
				float num11 = (_CurVHeights[num9 + _Resolution] - _CurVHeights[num9]) / num;
				float num12 = (_CurVHeights[num9 + 1 + _Resolution] - _CurVHeights[num9 + _Resolution]) / num;
				activateObject_NTab[num10] += num11;
				activateObject_NTab[num10 + 1] += num7;
				activateObject_NTab[num10 + 2] += num12;
				activateObject_NTab[num10 + num3] += num11;
				activateObject_NTab[num10 + num3 + 1] += num7;
				activateObject_NTab[num10 + num3 + 2] += num12;
				activateObject_NTab[num10 + 3 + num3] += num11;
				activateObject_NTab[num10 + 3 + num3 + 1] += num7;
				activateObject_NTab[num10 + 3 + num3 + 2] += num12;
				num11 = (_CurVHeights[num9 + 1 + _Resolution] - _CurVHeights[num9 + 1]) / num;
				num12 = (_CurVHeights[num9 + 1] - _CurVHeights[num9]) / num;
				activateObject_NTab[num10] += num11;
				activateObject_NTab[num10 + 1] += num7;
				activateObject_NTab[num10 + 2] += num12;
				activateObject_NTab[num10 + 3] += num11;
				activateObject_NTab[num10 + 3 + 1] += num7;
				activateObject_NTab[num10 + 3 + 2] += num12;
				activateObject_NTab[num10 + 3 + num3] += num11;
				activateObject_NTab[num10 + 3 + num3 + 1] += num7;
				activateObject_NTab[num10 + 3 + num3 + 2] += num12;
			}
			num8 += _Resolution;
		}
		num8 = 0;
		for (j = 0; j < _Resolution; j++)
		{
			for (i = 0; i < _Resolution; i++)
			{
				int num10 = (i + num8) * 3;
				float num13 = 1f / Mathf.Sqrt(activateObject_NTab[num10] * activateObject_NTab[num10] + activateObject_NTab[num10 + 1] * activateObject_NTab[num10 + 1] + activateObject_NTab[num10 + 2] * activateObject_NTab[num10 + 2]);
				activateObject_NTab[num10] *= num13;
				activateObject_NTab[num10 + 1] *= num13;
				activateObject_NTab[num10 + 2] *= num13;
			}
			num8 += _Resolution;
		}
		for (int k = 0; k < 4; k++)
		{
			switch (k)
			{
			case 0:
				array = this.SouthTab;
				array2 = this.NSouthTab;
				break;
			case 1:
				array = this.EastTab;
				array2 = this.NEastTab;
				break;
			case 2:
				array = this.NorthTab;
				array2 = this.NNorthTab;
				break;
			case 3:
				array = this.WestTab;
				array2 = this.NWestTab;
				break;
			}
			float num14 = this.EdgeFactorTab[k];
			int num15 = ((num14 > 1f) ? ((int)((double)num14 + 1E-05)) : ((int)(1.0 / (double)num14 + 1E-05)));
			int num16 = k * _Resolution;
			int num17 = k * 3 * _Resolution;
			if (num14 > 1f)
			{
				float num18 = num * (float)num15;
				int num19 = (_Resolution - 1) / num15 + 1;
				switch (k)
				{
				case 0:
				{
					float num20 = _LowLeftCornerX;
					float num21 = _LowLeftCornerZ - num18;
					j = 0;
					num4 = 0;
					int num22 = num19;
					while (j < num19)
					{
						array3[j] = DistantChunkMap.TGHeightFunc(num20, 0f, num21) * _YScale;
						array3[num22] = _CurVHeights[array[num4]];
						j++;
						num4 += num15;
						num22++;
						num20 += num18;
					}
					break;
				}
				case 1:
				{
					float num20 = _LowLeftCornerX + _Width + num18;
					float num21 = _LowLeftCornerZ;
					j = 0;
					num4 = 0;
					int num22 = num19;
					while (j < num19)
					{
						array3[j] = DistantChunkMap.TGHeightFunc(num20, 0f, num21) * _YScale;
						array3[num22] = _CurVHeights[array[num4]];
						j++;
						num4 += num15;
						num22++;
						num21 += num18;
					}
					break;
				}
				case 2:
				{
					float num20 = _LowLeftCornerX + _Width;
					float num21 = _LowLeftCornerZ + _Width + num18;
					j = 0;
					num4 = 0;
					int num22 = num19;
					while (j < num19)
					{
						array3[j] = DistantChunkMap.TGHeightFunc(num20, 0f, num21) * _YScale;
						array3[num22] = _CurVHeights[array[num4]];
						j++;
						num4 += num15;
						num22++;
						num20 -= num18;
					}
					break;
				}
				case 3:
				{
					float num20 = _LowLeftCornerX - num18;
					float num21 = _LowLeftCornerZ + _Width;
					j = 0;
					num4 = 0;
					int num22 = num19;
					while (j < num19)
					{
						array3[j] = DistantChunkMap.TGHeightFunc(num20, 0f, num21) * _YScale;
						array3[num22] = _CurVHeights[array[num4]];
						j++;
						num4 += num15;
						num22++;
						num21 -= num18;
					}
					break;
				}
				}
				j = 0;
				i = 0;
				while (j < num19)
				{
					array4[i] = 0f;
					array4[i + 1] = 0f;
					array4[i + 2] = 0f;
					j++;
					i += 3;
				}
				this.computeEdgeNormals(num19, k, _Width, array3, array4);
				int num9 = 0;
				j = 0;
				float num13;
				while (j < _Resolution - 1)
				{
					float num23 = _CurVHeights[array[j]];
					float num24 = (_CurVHeights[array[j + num15]] - num23) / (float)num15;
					this.DeltaNormal[0] = (array4[num9 + 3] - array4[num9]) / (float)num15;
					this.DeltaNormal[1] = (array4[num9 + 4] - array4[num9 + 1]) / (float)num15;
					this.DeltaNormal[2] = (array4[num9 + 5] - array4[num9 + 2]) / (float)num15;
					this.NewNormal[0] = array4[num9];
					this.NewNormal[1] = array4[num9 + 1];
					this.NewNormal[2] = array4[num9 + 2];
					cmdataEdgeCorHeight[j + num16] = _CurVHeights[array[j]];
					num4 = 3 * j;
					this.TmpVec[0] = activateObject_NTab[array2[j]] + this.NewNormal[0];
					this.TmpVec[1] = activateObject_NTab[array2[j] + 1] + this.NewNormal[1];
					this.TmpVec[2] = activateObject_NTab[array2[j] + 2] + this.NewNormal[2];
					num13 = 1f / Mathf.Sqrt(this.TmpVec[0] * this.TmpVec[0] + this.TmpVec[1] * this.TmpVec[1] + this.TmpVec[2] * this.TmpVec[2]);
					this.TmpVec[0] *= num13;
					this.TmpVec[1] *= num13;
					this.TmpVec[2] *= num13;
					activateObject_ENTab[num4 + num17] = this.TmpVec[0];
					activateObject_ENTab[num4 + num17 + 1] = this.TmpVec[1];
					activateObject_ENTab[num4 + num17 + 2] = this.TmpVec[2];
					i = j + 1;
					num4 += 3;
					num23 += num24;
					while (i < j + num15)
					{
						cmdataEdgeCorHeight[i + num16] = num23;
						this.NewNormal[0] += this.DeltaNormal[0];
						this.NewNormal[1] += this.DeltaNormal[1];
						this.NewNormal[2] += this.DeltaNormal[2];
						this.TmpVec[0] = activateObject_NTab[array2[i]] + this.NewNormal[0];
						this.TmpVec[1] = activateObject_NTab[array2[i] + 1] + this.NewNormal[1];
						this.TmpVec[2] = activateObject_NTab[array2[i] + 2] + this.NewNormal[2];
						num13 = 1f / Mathf.Sqrt(this.TmpVec[0] * this.TmpVec[0] + this.TmpVec[1] * this.TmpVec[1] + this.TmpVec[2] * this.TmpVec[2]);
						this.TmpVec[0] *= num13;
						this.TmpVec[1] *= num13;
						this.TmpVec[2] *= num13;
						activateObject_ENTab[num4 + num17] = this.TmpVec[0];
						activateObject_ENTab[num4 + num17 + 1] = this.TmpVec[1];
						activateObject_ENTab[num4 + num17 + 2] = this.TmpVec[2];
						i++;
						num4 += 3;
						num23 += num24;
					}
					j += num15;
					num9 += 3;
				}
				cmdataEdgeCorHeight[num16 + _Resolution - 1] = _CurVHeights[array[_Resolution - 1]];
				i = (num19 - 1) * 3;
				num4 = (_Resolution - 1) * 3;
				this.TmpVec[0] = activateObject_NTab[array2[_Resolution - 1]] + array4[i];
				this.TmpVec[1] = activateObject_NTab[array2[_Resolution - 1] + 1] + array4[i + 1];
				this.TmpVec[2] = activateObject_NTab[array2[_Resolution - 1] + 2] + array4[i + 2];
				num13 = 1f / Mathf.Sqrt(this.TmpVec[0] * this.TmpVec[0] + this.TmpVec[1] * this.TmpVec[1] + this.TmpVec[2] * this.TmpVec[2]);
				this.TmpVec[0] *= num13;
				this.TmpVec[1] *= num13;
				this.TmpVec[2] *= num13;
				activateObject_ENTab[num4 + num17] = this.TmpVec[0];
				activateObject_ENTab[num4 + num17 + 1] = this.TmpVec[1];
				activateObject_ENTab[num4 + num17 + 2] = this.TmpVec[2];
			}
			else
			{
				for (j = 0; j < _Resolution; j++)
				{
					cmdataEdgeCorHeight[j + num16] = _CurVHeights[array[j]];
				}
				float num18 = num / (float)num15;
				int num19 = (_Resolution - 1) * num15 + 1;
				switch (k)
				{
				case 0:
				{
					float num20 = _LowLeftCornerX;
					float num21 = _LowLeftCornerZ - num18;
					j = 0;
					num4 = 0;
					int num22 = num19;
					while (j < num19)
					{
						array3[j] = DistantChunkMap.TGHeightFunc(num20, 0f, num21) * _YScale;
						if (j % num15 == 0)
						{
							array3[num22] = _CurVHeights[array[num4]];
							num4++;
						}
						else
						{
							array3[num22] = DistantChunkMap.TGHeightFunc(num20, 0f, num21 + num18) * _YScale;
						}
						j++;
						num22++;
						num20 += num18;
					}
					break;
				}
				case 1:
				{
					float num20 = _LowLeftCornerX + _Width + num18;
					float num21 = _LowLeftCornerZ;
					j = 0;
					num4 = 0;
					int num22 = num19;
					while (j < num19)
					{
						array3[j] = DistantChunkMap.TGHeightFunc(num20, 0f, num21) * _YScale;
						if (j % num15 == 0)
						{
							array3[num22] = _CurVHeights[array[num4]];
							num4++;
						}
						else
						{
							array3[num22] = DistantChunkMap.TGHeightFunc(num20 - num18, 0f, num21) * _YScale;
						}
						j++;
						num22++;
						num21 += num18;
					}
					break;
				}
				case 2:
				{
					float num20 = _LowLeftCornerX + _Width;
					float num21 = _LowLeftCornerZ + _Width + num18;
					j = 0;
					num4 = 0;
					int num22 = num19;
					while (j < num19)
					{
						array3[j] = DistantChunkMap.TGHeightFunc(num20, 0f, num21) * _YScale;
						if (j % num15 == 0)
						{
							array3[num22] = _CurVHeights[array[num4]];
							num4++;
						}
						else
						{
							array3[num22] = DistantChunkMap.TGHeightFunc(num20, 0f, num21 - num18) * _YScale;
						}
						j++;
						num22++;
						num20 -= num18;
					}
					break;
				}
				case 3:
				{
					float num20 = _LowLeftCornerX - num18;
					float num21 = _LowLeftCornerZ + _Width;
					j = 0;
					num4 = 0;
					int num22 = num19;
					while (j < num19)
					{
						array3[j] = DistantChunkMap.TGHeightFunc(num20, 0f, num21) * _YScale;
						if (j % num15 == 0)
						{
							array3[num22] = _CurVHeights[array[num4]];
							num4++;
						}
						else
						{
							array3[num22] = DistantChunkMap.TGHeightFunc(num20 + num18, 0f, num21) * _YScale;
						}
						j++;
						num22++;
						num21 -= num18;
					}
					break;
				}
				}
				j = 0;
				i = 0;
				while (j < num19)
				{
					array4[i] = 0f;
					array4[i + 1] = 0f;
					array4[i + 2] = 0f;
					j++;
					i += 3;
				}
				this.computeEdgeNormals(num19, k, _Width, array3, array4);
				int num9 = 0;
				num4 = 3 * num15;
				j = 0;
				i = 0;
				while (j < _Resolution)
				{
					this.TmpVec[0] = activateObject_NTab[array2[j]] + array4[i];
					this.TmpVec[1] = activateObject_NTab[array2[j] + 1] + array4[i + 1];
					this.TmpVec[2] = activateObject_NTab[array2[j] + 2] + array4[i + 2];
					activateObject_ENTab[num9 + num17] = this.TmpVec[0] / 2f;
					activateObject_ENTab[num9 + num17 + 1] = this.TmpVec[1] / 2f;
					activateObject_ENTab[num9 + num17 + 2] = this.TmpVec[2] / 2f;
					j++;
					i += num4;
					num9 += 3;
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void computeEdgeNormals(int _Resolution, int _EdgeId, float _ChunkWidth, float[] _HeightTab, float[] _NormalTab)
	{
		float num = _ChunkWidth / (float)(_Resolution - 1);
		int num2 = _Resolution;
		int num3 = 0;
		int i = 0;
		while (i < _Resolution - 1)
		{
			float num4 = (_HeightTab[i + 1] - _HeightTab[i]) / num;
			float num5 = (_HeightTab[num2 + 1] - _HeightTab[i + 1]) / num;
			_NormalTab[num3 + 3] += num4;
			_NormalTab[num3 + 4] += -1f;
			_NormalTab[num3 + 5] += num5;
			num4 = (_HeightTab[num2 + 1] - _HeightTab[num2]) / num;
			num5 = (_HeightTab[num2] - _HeightTab[i]) / num;
			_NormalTab[num3] += num4;
			_NormalTab[num3 + 1] += -1f;
			_NormalTab[num3 + 2] += num5;
			_NormalTab[num3 + 3] += num4;
			_NormalTab[num3 + 4] += -1f;
			_NormalTab[num3 + 5] += num5;
			i++;
			num2++;
			num3 += 3;
		}
		for (int j = 0; j < _Resolution * 3; j += 3)
		{
			float num6 = 1f / Mathf.Sqrt(_NormalTab[j] * _NormalTab[j] + _NormalTab[j + 1] * _NormalTab[j + 1] + _NormalTab[j + 2] * _NormalTab[j + 2]);
			switch (_EdgeId)
			{
			case 0:
				_NormalTab[j] *= num6;
				_NormalTab[j + 1] *= num6;
				_NormalTab[j + 2] *= num6;
				break;
			case 1:
			{
				float num7 = _NormalTab[j + 2] * num6;
				_NormalTab[j + 1] *= num6;
				_NormalTab[j + 2] = _NormalTab[j] * num6;
				_NormalTab[j] = -num7;
				break;
			}
			case 2:
				_NormalTab[j] *= -num6;
				_NormalTab[j + 1] *= num6;
				_NormalTab[j + 2] *= -num6;
				break;
			case 3:
			{
				float num7 = _NormalTab[j + 2] * num6;
				_NormalTab[j + 1] *= num6;
				_NormalTab[j + 2] = -_NormalTab[j] * num6;
				_NormalTab[j] = num7;
				break;
			}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public float[] EdgeResFactor;

	[PublicizedFrom(EAccessModifier.Private)]
	public float NextResLevelEdgeFactor;

	[PublicizedFrom(EAccessModifier.Private)]
	public float Size;

	[PublicizedFrom(EAccessModifier.Private)]
	public int Resolution;

	public long CellKey;

	public bool WasReset;

	public int ResLevel;

	public bool IsMeshUpdated;

	public bool IsFreeToUse;

	public Vector3 LeftDownCoor;

	public Vector3 TerrainOriginCoor;

	public Vector2i CellIdVector;

	public DChunkSquareMesh CellMeshData;

	public static DChunkSquareMeshPool SMPool;

	[PublicizedFrom(EAccessModifier.Private)]
	public static float[][] ActivateObject_NTabArray;

	[PublicizedFrom(EAccessModifier.Private)]
	public float[] ActivateObject_NTab;

	[PublicizedFrom(EAccessModifier.Private)]
	public static float[][] ActivateObject_ENTabArray;

	[PublicizedFrom(EAccessModifier.Private)]
	public float[] ActivateObject_ENTab;

	[PublicizedFrom(EAccessModifier.Private)]
	public static Vector3[][] CalcMeshTg_tan1Array;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3[] CalcMeshTg_tan1;

	[PublicizedFrom(EAccessModifier.Private)]
	public static Vector3[][] CalcMeshTg_tan2Array;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3[] CalcMeshTg_tan2;

	[PublicizedFrom(EAccessModifier.Private)]
	public static DistantChunk.WaterMesh[] StaticWaterSMesh;

	[PublicizedFrom(EAccessModifier.Private)]
	public Mesh AcUMeshGO_CellMesh;

	[PublicizedFrom(EAccessModifier.Private)]
	public Mesh AcUMeshGO_ColMesh;

	[PublicizedFrom(EAccessModifier.Private)]
	public static Vector3[][] TmpMeshV;

	[PublicizedFrom(EAccessModifier.Private)]
	public static Vector3[][] TmpMeshN;

	public float[] CMDataVerticesHeight;

	[PublicizedFrom(EAccessModifier.Private)]
	public float[] CMDataEdgeCorHeight;

	public bool ChunkObjExist;

	public bool IsChunkActivated;

	public bool IsOnActivationProcess;

	public bool IsOnSeamCorrectionProcess;

	public GameObject CellObj;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float FoldHeight = 35f;

	[PublicizedFrom(EAccessModifier.Private)]
	public GameObject WaterPlaneObj;

	[PublicizedFrom(EAccessModifier.Private)]
	public float WaterPlaneHeight;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool WaterPlaneIsActive;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool WaterPlaneObjExist;

	[PublicizedFrom(EAccessModifier.Private)]
	public float WaterPlaneUnitStep;

	[PublicizedFrom(EAccessModifier.Private)]
	public float WaterPlaneOverlapSize = 0.01f;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool IsWaterActivated;

	[PublicizedFrom(EAccessModifier.Private)]
	public int[] CurTri;

	public DistantChunkMapInfo ChunkMapInfo;

	public DistantChunkMap BaseChunkMap;

	[PublicizedFrom(EAccessModifier.Private)]
	public Stack<GameObject> WaterPlaneGameObjStack;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2 ScaleTexVec = new Vector2(3.75f, 3.75f);

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 FlPosVec = Vector3.zero;

	[PublicizedFrom(EAccessModifier.Private)]
	public float[] TilingFacFromResLevel = new float[] { 1f, 1f, 1f, 32f };

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 v1;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 v2;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 v3;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2 w1;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2 w2;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2 w3;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 sdir;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 tdir;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 n;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 t;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int MAX_RESOLUTION = 20;

	[PublicizedFrom(EAccessModifier.Private)]
	public int[] SouthTab = new int[20];

	[PublicizedFrom(EAccessModifier.Private)]
	public int[] EastTab = new int[20];

	[PublicizedFrom(EAccessModifier.Private)]
	public int[] NorthTab = new int[20];

	[PublicizedFrom(EAccessModifier.Private)]
	public int[] WestTab = new int[20];

	[PublicizedFrom(EAccessModifier.Private)]
	public int[] NSouthTab = new int[20];

	[PublicizedFrom(EAccessModifier.Private)]
	public int[] NEastTab = new int[20];

	[PublicizedFrom(EAccessModifier.Private)]
	public int[] NNorthTab = new int[20];

	[PublicizedFrom(EAccessModifier.Private)]
	public int[] NWestTab = new int[20];

	[PublicizedFrom(EAccessModifier.Private)]
	public float[] DeltaNormal = new float[3];

	[PublicizedFrom(EAccessModifier.Private)]
	public float[] NewNormal = new float[3];

	[PublicizedFrom(EAccessModifier.Private)]
	public float[] TmpVec = new float[3];

	[PublicizedFrom(EAccessModifier.Private)]
	public float[] EdgeFactorTab = new float[4];

	[PublicizedFrom(EAccessModifier.Private)]
	public class WaterMesh
	{
		public WaterMesh(int NbVertices, int NbTriangles, float Size)
		{
			this.Init(NbVertices, NbTriangles, Size);
		}

		public void Init(int NbVertices, int NbTriangles, float Size)
		{
			this.Vertices = new Vector3[NbVertices];
			this.Normals = new Vector3[NbVertices];
			this.Tangents = new Vector4[NbVertices];
			this.UVVectors = new Vector2[NbVertices];
			this.Triangles = new int[NbTriangles * 3];
			this.Colors = new Color[NbVertices];
			this.Bound = new Bounds(new Vector3(Size / 2f, 0f, Size / 2f), new Vector3(Size, 0f, Size));
		}

		public Vector3[] Vertices;

		public Vector3[] Normals;

		public Vector4[] Tangents;

		public Vector2[] UVVectors;

		public int[] Triangles;

		public Color[] Colors;

		public Bounds Bound;
	}
}
