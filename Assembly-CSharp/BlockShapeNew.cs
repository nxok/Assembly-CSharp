using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class BlockShapeNew : BlockShape
{
	[PublicizedFrom(EAccessModifier.Private)]
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void ThreadDataInit()
	{
		if (BlockShapeNew.threadData.trisXPos == null)
		{
			BlockShapeNew.threadData.Init();
		}
	}

	public BlockShapeNew()
	{
		this.IsSolidCube = false;
		this.IsSolidSpace = false;
		this.IsRotatable = true;
	}

	public override void Init(Block _block)
	{
		this.ShapeName = _block.Properties.GetValue("Model");
		if (this.ShapeName == null)
		{
			throw new Exception("No model specified on block with name " + _block.GetBlockName());
		}
		Vector3 vector;
		vector..ctor(1f, 0f, 1f);
		_block.Properties.ParseVec("ModelOffset", ref vector);
		BlockShapeNew.MeshData meshData;
		if (!BlockShapeNew.meshData.TryGetValue(this.ShapeName, out meshData))
		{
			meshData = new BlockShapeNew.MeshData();
			BlockShapeNew.meshData.Add(this.ShapeName, meshData);
		}
		BlockShapeNew.MeshData.Arrays arrays;
		if (!meshData.posArrays.TryGetValue(vector, out arrays))
		{
			if (!meshData.obj)
			{
				GameObject gameObject = DataLoader.LoadAsset<GameObject>(this.ShapeName ?? "", false);
				if (!gameObject)
				{
					throw new Exception("Model with name " + this.ShapeName + " not found");
				}
				meshData.obj = gameObject;
			}
			arrays = new BlockShapeNew.MeshData.Arrays();
			meshData.posArrays.Add(vector, arrays);
			this.ParseModel(meshData, arrays, vector);
		}
		this.visualMeshes = arrays.meshes;
		this.colliderMeshes = arrays.colliderMeshes;
		this.faceInfo = arrays.faceInfo;
		this.boundsRotations = arrays.boundsRotations;
		if (meshData.symTypeOverride != -1)
		{
			this.SymmetryType = meshData.symTypeOverride;
		}
		this.IsSolidCube = meshData.IsSolidCube;
		if (_block.PathType < 0)
		{
			this.boundsPathOffsetRotations = new Vector2[32];
			for (int i = 0; i < 28; i++)
			{
				Bounds bounds = this.boundsRotations[i];
				Vector3 min = bounds.min;
				Vector3 max = bounds.max;
				Vector2 vector2;
				vector2.x = 0f;
				float num = min.x + BlockShapeNew.centerOffsetV.x;
				if (num >= -0.01f)
				{
					vector2.x = (-0.5f + num) * 0.5f;
				}
				num = max.x + BlockShapeNew.centerOffsetV.x;
				if (num <= 0.01f)
				{
					vector2.x = (0.5f + num) * 0.5f;
				}
				vector2.y = 0f;
				float num2 = min.z + BlockShapeNew.centerOffsetV.z;
				if (num2 >= -0.01f)
				{
					vector2.y = (-0.5f + num2) * 0.5f;
				}
				num2 = max.z + BlockShapeNew.centerOffsetV.z;
				if (num2 <= 0.01f)
				{
					vector2.y = (0.5f + num2) * 0.5f;
				}
				if (vector2.x != 0f || vector2.y != 0f)
				{
					_block.PathType = -1;
					this.boundsPathOffsetRotations[i] = vector2;
				}
			}
		}
		base.Init(_block);
		if (meshData.obj)
		{
			foreach (MeshRenderer meshRenderer in meshData.obj.GetComponentsInChildren<MeshRenderer>())
			{
				Material sharedMaterial = meshRenderer.sharedMaterial;
				if (sharedMaterial != null)
				{
					meshRenderer.sharedMaterial = null;
					Resources.UnloadAsset(sharedMaterial);
				}
			}
			MeshFilter[] componentsInChildren2 = meshData.obj.GetComponentsInChildren<MeshFilter>();
			for (int j = 0; j < componentsInChildren2.Length; j++)
			{
				Mesh sharedMesh = componentsInChildren2[j].sharedMesh;
				if (sharedMesh != null)
				{
					meshData.obj = null;
					Resources.UnloadAsset(sharedMesh);
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ParseModel(BlockShapeNew.MeshData _data, BlockShapeNew.MeshData.Arrays _arrays, Vector3 _modelOffset)
	{
		if (BlockShapeNew.convertRotationCached == null)
		{
			BlockShapeNew.convertRotationCached = new int[32, 7];
			for (int i = 0; i < 32; i++)
			{
				for (int j = 0; j < 7; j++)
				{
					BlockShapeNew.convertRotationCached[i, j] = BlockShapeNew.convertRotation((BlockFace)j, i);
				}
			}
		}
		Transform transform = _data.obj.transform;
		for (int k = 0; k < transform.childCount; k++)
		{
			Transform child = transform.GetChild(k);
			string name = child.name;
			if (name == "Solid")
			{
				_data.IsSolidCube = true;
			}
			else if (name == "LOD0")
			{
				for (int l = 0; l < child.childCount; l++)
				{
					Transform child2 = child.GetChild(l);
					string name2 = child2.name;
					int num = this.CharToFaceIndex(name2[0]);
					if (num != -1)
					{
						_arrays.meshes[num] = this.CreateMeshFromMeshFilter(child2, _modelOffset);
						if (child2.childCount > 0)
						{
							Log.Error(string.Format("{0} face \"{1}\" has 1 base mesh and {2} submeshes, exceeding the total limit of {3} meshes per face.", new object[]
							{
								_data.obj.name,
								name2,
								child2.childCount,
								1
							}));
						}
						int num2 = 0;
						while (num2 < child2.childCount && num2 < 0)
						{
							Transform child3 = child2.GetChild(num2);
							_arrays.meshes[num + (num2 + 1) * 7] = this.CreateMeshFromMeshFilter(child3, _modelOffset);
							num2++;
						}
						if (name2.Length > 2)
						{
							for (int m = 2; m < name2.Length; m++)
							{
								char c = name2[m];
								if (c == 'F')
								{
									_arrays.faceInfo[num] = BlockShapeNew.EnumFaceOcclusionInfo.Full;
								}
								else if (c == 'P')
								{
									_arrays.faceInfo[num] = BlockShapeNew.EnumFaceOcclusionInfo.Part;
								}
								else if (c == 'A')
								{
									_arrays.faceInfo[num] = BlockShapeNew.EnumFaceOcclusionInfo.Remove;
								}
								else if (c == 'C')
								{
									_arrays.faceInfo[num] = BlockShapeNew.EnumFaceOcclusionInfo.Continuous;
								}
								else if (c == 'Y')
								{
									_arrays.faceInfo[num] = BlockShapeNew.EnumFaceOcclusionInfo.RemoveIfAny;
								}
								else if (c == 'O')
								{
									_arrays.faceInfo[num] = BlockShapeNew.EnumFaceOcclusionInfo.OwnFaces;
								}
								else if (c == 'H')
								{
									_arrays.faceInfo[num] = BlockShapeNew.EnumFaceOcclusionInfo.HideIfSame;
								}
								else if (c == 'T')
								{
									_arrays.faceInfo[num] = BlockShapeNew.EnumFaceOcclusionInfo.Transparent;
								}
							}
						}
					}
				}
			}
			else if (name == "Collider")
			{
				for (int n = 0; n < child.childCount; n++)
				{
					Transform child4 = child.GetChild(n);
					string name3 = child4.name;
					int num3 = this.CharToFaceIndex(name3[0]);
					if (num3 != -1)
					{
						_arrays.colliderMeshes[num3] = this.CreateMeshFromMeshFilter(child4, _modelOffset);
					}
				}
			}
			else if (name == "SymType_0")
			{
				_data.symTypeOverride = 0;
			}
			else if (name == "SymType_2")
			{
				_data.symTypeOverride = 2;
			}
			else if (name == "SymType_3")
			{
				_data.symTypeOverride = 3;
			}
			else if (name == "SymType_4")
			{
				_data.symTypeOverride = 4;
			}
		}
		this.CalcBounds(_arrays);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CalcBounds(BlockShapeNew.MeshData.Arrays _arrays)
	{
		Bounds bounds = default(Bounds);
		for (int i = 0; i < 28; i++)
		{
			Quaternion quaternion = BlockShapeNew.rotationsToQuats[i];
			Vector3 vector = Vector3.positiveInfinity;
			Vector3 vector2 = Vector3.negativeInfinity;
			for (int j = 0; j < 6; j++)
			{
				BlockShapeNew.MySimpleMesh mySimpleMesh = _arrays.meshes[j];
				if (mySimpleMesh != null)
				{
					List<Vector3> vertices = mySimpleMesh.Vertices;
					int count = vertices.Count;
					for (int k = 0; k < count; k++)
					{
						Vector3 vector3 = quaternion * (vertices[k] + BlockShapeNew.centerOffsetV);
						if (vector3.x < vector.x)
						{
							vector.x = vector3.x;
						}
						if (vector3.x > vector2.x)
						{
							vector2.x = vector3.x;
						}
						if (vector3.y < vector.y)
						{
							vector.y = vector3.y;
						}
						if (vector3.y > vector2.y)
						{
							vector2.y = vector3.y;
						}
						if (vector3.z < vector.z)
						{
							vector.z = vector3.z;
						}
						if (vector3.z > vector2.z)
						{
							vector2.z = vector3.z;
						}
					}
				}
			}
			vector -= BlockShapeNew.centerOffsetV;
			vector2 -= BlockShapeNew.centerOffsetV;
			bounds.SetMinMax(vector, vector2);
			bounds.extents = new Vector3(Utils.FastMax(bounds.extents.x, 0.1f), Utils.FastMax(bounds.extents.y, 0.1f), Utils.FastMax(bounds.extents.z, 0.1f));
			_arrays.boundsRotations[i] = bounds;
		}
	}

	public static void Cleanup()
	{
		BlockShapeNew.meshData.Clear();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int CharToFaceIndex(char _c)
	{
		if (_c <= 'E')
		{
			if (_c == 'B')
			{
				return 1;
			}
			if (_c == 'E')
			{
				return 5;
			}
		}
		else
		{
			if (_c == 'M')
			{
				return 6;
			}
			if (_c == 'N')
			{
				return 2;
			}
			switch (_c)
			{
			case 'S':
				return 4;
			case 'T':
				return 0;
			case 'W':
				return 3;
			}
		}
		return -1;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static int convertRotation(BlockFace _face, int _rotation)
	{
		Vector3 vector = Vector3.zero;
		Vector3 vector2;
		switch (_face)
		{
		case BlockFace.Top:
			vector2 = Vector3.up;
			break;
		case BlockFace.Bottom:
			vector2 = Vector3.down;
			break;
		case BlockFace.North:
			vector2 = Vector3.forward;
			break;
		case BlockFace.West:
			vector2 = Vector3.left;
			break;
		case BlockFace.South:
			vector2 = Vector3.back;
			break;
		case BlockFace.East:
			vector2 = Vector3.right;
			break;
		default:
			vector2 = Vector3.zero;
			break;
		}
		Quaternion quaternion = Quaternion.Inverse(BlockShapeNew.rotationsToQuats[_rotation]);
		vector = quaternion * vector;
		vector2 = quaternion * vector2;
		Vector3 vector3 = vector2 - vector;
		int num;
		if (vector3.x > 0.9f)
		{
			num = 5;
		}
		else if (vector3.x < -0.9f)
		{
			num = 3;
		}
		else if (vector3.y > 0.9f)
		{
			num = 0;
		}
		else if (vector3.y < -0.9f)
		{
			num = 1;
		}
		else if (vector3.z > 0.9f)
		{
			num = 2;
		}
		else if (vector3.z < -0.9f)
		{
			num = 4;
		}
		else
		{
			num = (int)_face;
		}
		return num;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void convertRotationUVDirs(BlockFace _face, int _rotation, out Vector3 _dX, out Vector3 _dY)
	{
		Vector3 vector;
		Vector3 vector2;
		switch (_face)
		{
		case BlockFace.Top:
			vector = Vector3.right;
			vector2 = Vector3.forward;
			break;
		case BlockFace.Bottom:
			vector = Vector3.left;
			vector2 = Vector3.forward;
			break;
		case BlockFace.North:
			vector = Vector3.left;
			vector2 = Vector3.up;
			break;
		case BlockFace.West:
			vector = Vector3.back;
			vector2 = Vector3.up;
			break;
		case BlockFace.South:
			vector = Vector3.right;
			vector2 = Vector3.up;
			break;
		case BlockFace.East:
			vector = Vector3.forward;
			vector2 = Vector3.up;
			break;
		case BlockFace.Middle:
			vector = Vector3.left;
			vector2 = Vector3.up;
			break;
		default:
			vector = Vector3.zero;
			vector2 = Vector3.zero;
			break;
		}
		Quaternion quaternion = BlockShapeNew.rotationsToQuats[_rotation];
		_dX = quaternion * vector;
		_dY = quaternion * vector2;
	}

	public static int ConvertRotationFree(int _rotation, Quaternion _q, bool _bApplyRotFirst = false)
	{
		Vector3 vector;
		vector..ctor(-0.5f, -0.5f, -0.5f);
		Vector3 vector2 = Vector3.up;
		Quaternion rotationStatic = BlockShapeNew.GetRotationStatic(_rotation);
		if (_bApplyRotFirst)
		{
			vector = _q * vector;
			vector2 = _q * vector2;
			vector = rotationStatic * vector;
			vector2 = rotationStatic * vector2;
		}
		else
		{
			vector = rotationStatic * vector;
			vector2 = rotationStatic * vector2;
			vector = _q * vector;
			vector2 = _q * vector2;
		}
		vector.x += 0.5f;
		vector.y += 0.5f;
		vector.z += 0.5f;
		Vector3i vector3i = new Vector3i(Mathf.RoundToInt(vector.x), Mathf.RoundToInt(vector.y), Mathf.RoundToInt(vector.z));
		Vector3i vector3i2 = new Vector3i(Mathf.RoundToInt(vector2.x), Mathf.RoundToInt(vector2.y), Mathf.RoundToInt(vector2.z));
		if (vector3i2.x == 0)
		{
			if (vector3i2.z == 0)
			{
				if (vector3i2.y == 1)
				{
					if (vector3i.y == 0)
					{
						int num = BlockShapeNew.BlockFaceToRot(BlockFace.Top);
						if (vector3i.x == 0)
						{
							if (vector3i.z == 0)
							{
								return num;
							}
							return num + 1;
						}
						else
						{
							if (vector3i.z == 1)
							{
								return num + 2;
							}
							return num + 3;
						}
					}
				}
				else if (vector3i.y == 1)
				{
					int num2 = BlockShapeNew.BlockFaceToRot(BlockFace.Bottom);
					if (vector3i.x == 1)
					{
						if (vector3i.z == 0)
						{
							return num2;
						}
						return num2 + 1;
					}
					else
					{
						if (vector3i.z == 1)
						{
							return num2 + 2;
						}
						return num2 + 3;
					}
				}
			}
			if (vector3i2.y == 0)
			{
				if (vector3i2.z == 1)
				{
					if (vector3i.z == 0)
					{
						int num3 = BlockShapeNew.BlockFaceToRot(BlockFace.North);
						if (vector3i.x == 1)
						{
							if (vector3i.y == 0)
							{
								return num3;
							}
							return num3 + 1;
						}
						else
						{
							if (vector3i.y == 1)
							{
								return num3 + 2;
							}
							return num3 + 3;
						}
					}
				}
				else if (vector3i.z == 1)
				{
					int num4 = BlockShapeNew.BlockFaceToRot(BlockFace.South);
					if (vector3i.x == 0)
					{
						if (vector3i.y == 0)
						{
							return num4;
						}
						return num4 + 1;
					}
					else
					{
						if (vector3i.y == 1)
						{
							return num4 + 2;
						}
						return num4 + 3;
					}
				}
			}
		}
		else if (vector3i2.y == 0 && vector3i2.z == 0)
		{
			if (vector3i2.x == -1)
			{
				if (vector3i.x == 1)
				{
					int num5 = BlockShapeNew.BlockFaceToRot(BlockFace.West);
					if (vector3i.y == 0)
					{
						if (vector3i.z == 0)
						{
							return num5;
						}
						return num5 + 1;
					}
					else
					{
						if (vector3i.z == 1)
						{
							return num5 + 2;
						}
						return num5 + 3;
					}
				}
			}
			else if (vector3i.x == 0)
			{
				int num6 = BlockShapeNew.BlockFaceToRot(BlockFace.East);
				if (vector3i.y == 1)
				{
					if (vector3i.z == 0)
					{
						return num6;
					}
					return num6 + 1;
				}
				else
				{
					if (vector3i.z == 1)
					{
						return num6 + 2;
					}
					return num6 + 3;
				}
			}
		}
		return 0;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public BlockShapeNew.MySimpleMesh CreateMeshFromMeshFilter(Transform _transform, Vector3 _modelOffset)
	{
		MeshFilter component = _transform.GetComponent<MeshFilter>();
		if (component == null)
		{
			return null;
		}
		Mesh sharedMesh = component.sharedMesh;
		if (!sharedMesh.isReadable)
		{
			Log.Error("Mesh '" + sharedMesh.name + "' not readable in shape with Model=" + this.ShapeName);
			return null;
		}
		BlockShapeNew.MySimpleMesh mySimpleMesh = new BlockShapeNew.MySimpleMesh();
		Matrix4x4 localToWorldMatrix = _transform.localToWorldMatrix;
		localToWorldMatrix.m03 += _modelOffset.x;
		localToWorldMatrix.m13 += _modelOffset.y;
		localToWorldMatrix.m23 += _modelOffset.z;
		sharedMesh.GetVertices(mySimpleMesh.Vertices);
		int count = mySimpleMesh.Vertices.Count;
		for (int i = 0; i < count; i++)
		{
			Vector3 vector = mySimpleMesh.Vertices[i];
			vector = localToWorldMatrix.MultiplyPoint3x4(vector);
			mySimpleMesh.Vertices[i] = vector;
		}
		if (sharedMesh.subMeshCount == 1)
		{
			sharedMesh.GetTriangles(mySimpleMesh.Indices, 0, true);
		}
		else
		{
			int[] triangles = sharedMesh.triangles;
			for (int j = 0; j < triangles.Length; j++)
			{
				mySimpleMesh.Indices.Add((ushort)triangles[j]);
			}
		}
		sharedMesh.GetNormals(mySimpleMesh.Normals);
		int count2 = mySimpleMesh.Normals.Count;
		for (int k = 0; k < count2; k++)
		{
			Vector3 vector2 = mySimpleMesh.Normals[k];
			vector2 = localToWorldMatrix.MultiplyVector(vector2);
			mySimpleMesh.Normals[k] = vector2.normalized;
		}
		sharedMesh.GetUVs(0, mySimpleMesh.Uvs);
		return mySimpleMesh;
	}

	public override int getFacesDrawnFullBitfield(BlockValue _blockValue)
	{
		int num = 0;
		for (int i = 0; i < this.faceInfo.Length; i++)
		{
			int num2 = BlockShapeNew.convertRotationCached[(int)_blockValue.rotation, i];
			if (this.faceInfo[num2] == BlockShapeNew.EnumFaceOcclusionInfo.Full)
			{
				num |= 1 << i;
			}
		}
		return num;
	}

	public override bool isRenderFace(BlockValue _blockValue, BlockFace _face, BlockValue _adjBlockValue)
	{
		if (_blockValue.ischild)
		{
			return false;
		}
		int num = BlockShapeNew.convertRotationCached[(int)_blockValue.rotation, (int)_face];
		if (this.visualMeshes[num] == null)
		{
			return false;
		}
		Block block = _blockValue.Block;
		switch (this.faceInfo[num])
		{
		case BlockShapeNew.EnumFaceOcclusionInfo.None:
			return false;
		case BlockShapeNew.EnumFaceOcclusionInfo.Part:
		{
			BlockShapeNew blockShapeNew;
			if (!_adjBlockValue.ischild && block.MeshIndex == _adjBlockValue.Block.MeshIndex && (blockShapeNew = _adjBlockValue.Block.shape as BlockShapeNew) != null)
			{
				int num2 = BlockShapeNew.convertRotationCached[(int)_adjBlockValue.rotation, (int)BlockFaceFlags.OppositeFace(_face)];
				if (_adjBlockValue.rotation == _blockValue.rotation && blockShapeNew.ShapeName == this.ShapeName && blockShapeNew.faceInfo[num2] == BlockShapeNew.EnumFaceOcclusionInfo.Part)
				{
					return false;
				}
				if (blockShapeNew.faceInfo[num2] == BlockShapeNew.EnumFaceOcclusionInfo.Full)
				{
					return false;
				}
			}
			break;
		}
		case BlockShapeNew.EnumFaceOcclusionInfo.Remove:
		{
			BlockShapeNew blockShapeNew2 = _adjBlockValue.Block.shape as BlockShapeNew;
			if (blockShapeNew2 != null)
			{
				int num3 = BlockShapeNew.convertRotationCached[(int)_adjBlockValue.rotation, (int)BlockFaceFlags.OppositeFace(_face)];
				if (blockShapeNew2.faceInfo[num3] == BlockShapeNew.EnumFaceOcclusionInfo.Remove)
				{
					return false;
				}
			}
			return true;
		}
		case BlockShapeNew.EnumFaceOcclusionInfo.Continuous:
		{
			BlockShapeNew blockShapeNew3;
			if (!_adjBlockValue.ischild && block.MeshIndex == _adjBlockValue.Block.MeshIndex && (blockShapeNew3 = _adjBlockValue.Block.shape as BlockShapeNew) != null)
			{
				int num4 = BlockShapeNew.convertRotationCached[(int)_adjBlockValue.rotation, (int)BlockFaceFlags.OppositeFace(_face)];
				if (blockShapeNew3.faceInfo[num4] == BlockShapeNew.EnumFaceOcclusionInfo.Full)
				{
					return false;
				}
			}
			break;
		}
		case BlockShapeNew.EnumFaceOcclusionInfo.RemoveIfAny:
			if (!_adjBlockValue.isair)
			{
				return false;
			}
			break;
		case BlockShapeNew.EnumFaceOcclusionInfo.OwnFaces:
			if (_adjBlockValue.type == _blockValue.type)
			{
				return false;
			}
			break;
		case BlockShapeNew.EnumFaceOcclusionInfo.HideIfSame:
			if (_adjBlockValue.type == _blockValue.type && _adjBlockValue.rotation == _blockValue.rotation)
			{
				return false;
			}
			break;
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public BlockShapeNew.MySimpleMesh getVisualMesh(int _idx)
	{
		return this.visualMeshes[_idx];
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool AbsEpsilonEqual(float A, float B, float epsilon = 0.0001f)
	{
		return Mathf.Abs(A) <= Mathf.Abs(B) + epsilon && Mathf.Abs(A) >= Mathf.Abs(B) - epsilon;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static int FloorModInt(int a, int m)
	{
		if (m <= 0)
		{
			return 0;
		}
		int num = a % m;
		if (num < 0)
		{
			num += m;
		}
		return num;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static int FloorModFloat(float x, int m)
	{
		return BlockShapeNew.FloorModInt(Mathf.FloorToInt(x), m);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static BlockShapeNew.PlanePick ClassifyTriPlane(BlockShapeNew.MySimpleMesh mesh, int triStart, Quaternion q)
	{
		int num = (int)mesh.Indices[triStart];
		int num2 = (int)mesh.Indices[triStart + 1];
		int num3 = (int)mesh.Indices[triStart + 2];
		Vector3 vector = q * mesh.Vertices[num];
		Vector3 vector2 = q * mesh.Vertices[num2];
		Vector3 vector3 = q * mesh.Vertices[num3];
		Vector3 vector4 = Vector3.Cross(vector2 - vector, vector3 - vector);
		float magnitude = vector4.magnitude;
		if (magnitude < 1E-09f)
		{
			return new BlockShapeNew.PlanePick
			{
				axis = BlockShapeNew.PlaneAxis.Y,
				neg = false
			};
		}
		Vector3 vector5 = vector4 / magnitude;
		float num4 = Mathf.Abs(vector5.x);
		Mathf.Abs(vector5.y);
		float num5 = Mathf.Abs(vector5.z);
		if (num4 > 0.25f && num4 - num5 - 0.001f > 0f)
		{
			return new BlockShapeNew.PlanePick
			{
				axis = BlockShapeNew.PlaneAxis.X,
				neg = (vector5.x < 0f)
			};
		}
		if (num5 > 0.25f)
		{
			return new BlockShapeNew.PlanePick
			{
				axis = BlockShapeNew.PlaneAxis.Z,
				neg = (vector5.z < 0f)
			};
		}
		return new BlockShapeNew.PlanePick
		{
			axis = BlockShapeNew.PlaneAxis.Y,
			neg = (vector5.y < 0f)
		};
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void AppendSubsetTriPlanar(VoxelMesh targetMesh, BlockShapeNew.MySimpleMesh src, List<int> triIdx, Quaternion q, Vector3 sum1, Vector3 sum2, int texArrayIndex, int blockW, int blockH, int xTiling, int yTiling, Vector3 drawPos, LightingAround lighting, Rect cracksRect, bool imposterActive)
	{
		if (triIdx == null || triIdx.Count == 0)
		{
			return;
		}
		BlockShapeNew.threadData.subsetUsed.Clear();
		BlockShapeNew.threadData.subsetUsedList.Clear();
		for (int i = 0; i < triIdx.Count; i++)
		{
			int num = triIdx[i];
			int num2 = (int)src.Indices[num];
			if (!BlockShapeNew.threadData.subsetUsed.ContainsKey(num2))
			{
				BlockShapeNew.threadData.subsetUsed[num2] = BlockShapeNew.threadData.subsetUsedList.Count;
				BlockShapeNew.threadData.subsetUsedList.Add(num2);
			}
		}
		int count = BlockShapeNew.threadData.subsetUsedList.Count;
		targetMesh.CheckVertexLimit(count);
		if (count + targetMesh.m_Vertices.Count > 786432)
		{
			return;
		}
		int num3 = targetMesh.m_Vertices.Alloc(count);
		targetMesh.m_Normals.Alloc(count);
		targetMesh.m_ColorVertices.Alloc(count);
		Color color;
		color.g = (float)texArrayIndex;
		color.a = 1f;
		color.b = (float)lighting[LightingAround.Pos.Middle].stability / 15f;
		for (int j = 0; j < count; j++)
		{
			int num4 = BlockShapeNew.threadData.subsetUsedList[j];
			int num5 = num3 + j;
			Vector3 vector = q * (src.Vertices[num4] + sum1) + sum2;
			Vector3 vector2 = q * src.Normals[num4];
			targetMesh.m_Vertices[num5] = vector;
			targetMesh.m_Normals[num5] = vector2;
			Vector3 vector3 = vector - drawPos;
			float num6 = (float)lighting[LightingAround.Pos.X0Y0Z0].sun * (1f - vector3.x) * (1f - vector3.y) * (1f - vector3.z) + (float)lighting[LightingAround.Pos.X1Y0Z0].sun * vector3.x * (1f - vector3.y) * (1f - vector3.z) + (float)lighting[LightingAround.Pos.X0Y0Z1].sun * (1f - vector3.x) * (1f - vector3.y) * vector3.z + (float)lighting[LightingAround.Pos.X1Y0Z1].sun * vector3.x * (1f - vector3.y) * vector3.z + (float)lighting[LightingAround.Pos.X0Y1Z0].sun * (1f - vector3.x) * vector3.y * (1f - vector3.z) + (float)lighting[LightingAround.Pos.X0Y1Z1].sun * (1f - vector3.x) * vector3.y * vector3.z + (float)lighting[LightingAround.Pos.X1Y1Z0].sun * vector3.x * vector3.y * (1f - vector3.z) + (float)lighting[LightingAround.Pos.X1Y1Z1].sun * vector3.x * vector3.y * vector3.z;
			color.r = num6 / 15f;
			targetMesh.m_ColorVertices[num5] = color;
		}
		int curTriangleIndex = targetMesh.CurTriangleIndex;
		int count2 = triIdx.Count;
		int num7 = targetMesh.m_Indices.Alloc(count2);
		for (int k = 0; k < count2; k++)
		{
			int num8 = triIdx[k];
			int num9 = (int)src.Indices[num8];
			int num10 = BlockShapeNew.threadData.subsetUsed[num9];
			targetMesh.m_Indices[num7 + k] = curTriangleIndex + num10;
		}
		targetMesh.CurTriangleIndex += count;
		int num11 = targetMesh.m_Uvs.Alloc(count);
		Vector2 vector4;
		vector4..ctor((float)blockW / (float)Mathf.Max(1, xTiling), (float)blockH / (float)Mathf.Max(1, yTiling));
		for (int l = 0; l < count; l++)
		{
			targetMesh.m_Uvs[num11 + l] = vector4;
		}
		if (targetMesh.UvsCrack != null)
		{
			int num12 = targetMesh.UvsCrack.Alloc(count);
			for (int m = 0; m < count; m++)
			{
				int num13 = BlockShapeNew.threadData.subsetUsedList[m];
				Vector2 vector5;
				if (!BlockShapeNew.bImposterGenerationActive)
				{
					vector5.x = cracksRect.x + src.Uvs[num13].x * cracksRect.width;
					vector5.y = cracksRect.y + src.Uvs[num13].y * cracksRect.height;
				}
				else
				{
					vector5.x = (float)texArrayIndex;
					vector5.y = 1f;
				}
				targetMesh.UvsCrack[num12 + m] = vector5;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int ComputeTaIndexOffsFor(BlockShapeNew.PlaneAxis plane, UVRectTiling uvTexRect, Vector3 uvPos, bool flipU)
	{
		int num = Mathf.Max(1, (int)uvTexRect.uv.width);
		int num2 = Mathf.Max(1, (int)uvTexRect.uv.height);
		float num3;
		float num4;
		if (plane != BlockShapeNew.PlaneAxis.X)
		{
			if (plane == BlockShapeNew.PlaneAxis.Y)
			{
				num3 = uvPos.x;
				num4 = uvPos.z;
			}
			else
			{
				num3 = uvPos.x;
				num4 = uvPos.y;
			}
		}
		else
		{
			num3 = uvPos.z;
			num4 = uvPos.y;
		}
		int num5 = BlockShapeNew.FloorModFloat(num3, num);
		int num6 = BlockShapeNew.FloorModFloat(num4, num2);
		if (flipU)
		{
			num5 = num - 1 - num5;
		}
		num6 = num2 - 1 - num6;
		return num5 + num6 * num;
	}

	public override void renderFace(Vector3i _chunkPos, BlockValue _blockValue, Vector3 _drawPos, BlockFace _face, Vector3[] _vertices, LightingAround _lightingAround, TextureFullArray _textureFullArray, VoxelMesh[] _meshes, BlockShape.MeshPurpose _purpose = BlockShape.MeshPurpose.World)
	{
		if (_blockValue.ischild)
		{
			return;
		}
		this.ThreadDataInit();
		int num = BlockShapeNew.convertRotationCached[(int)_blockValue.rotation, (int)_face];
		Block block = _blockValue.Block;
		int num2;
		if (this.faceInfo[num] == BlockShapeNew.EnumFaceOcclusionInfo.Transparent)
		{
			num2 = 2;
			_face = BlockFace.North;
		}
		else
		{
			num2 = (int)block.MeshIndex;
		}
		VoxelMesh voxelMesh = _meshes[num2];
		if (voxelMesh == null)
		{
			return;
		}
		MeshDescription meshDescription = MeshDescription.meshes[num2];
		bool bTextureArray = meshDescription.bTextureArray;
		Vector3 vector = _drawPos;
		if (_purpose == BlockShape.MeshPurpose.Preview)
		{
			vector.x += (float)(_chunkPos.x + 1);
			vector.y += (float)(_chunkPos.y + 1);
			vector.z += (float)(_chunkPos.z + 1);
		}
		Quaternion quaternion = BlockShapeNew.rotationsToQuats[(int)_blockValue.rotation];
		Vector3 vector2 = BlockShapeNew.centerOffsetV;
		Vector3 vector3 = _drawPos - BlockShapeNew.centerOffsetV + this.GetRotationOffset(_blockValue);
		if (voxelMesh.m_CollVertices != null)
		{
			if (this.colliderMeshes[num] != null)
			{
				BlockShapeNew.<renderFace>g__CopyColliderMesh|38_0(voxelMesh, quaternion, vector2, vector3, this.colliderMeshes[num]);
			}
			else
			{
				for (int i = 0; i < 1; i++)
				{
					int num3 = i * 7;
					BlockShapeNew.MySimpleMesh mySimpleMesh = this.visualMeshes[num3 + num];
					if (mySimpleMesh != null)
					{
						BlockShapeNew.<renderFace>g__CopyColliderMesh|38_0(voxelMesh, quaternion, vector2, vector3, mySimpleMesh);
					}
				}
			}
		}
		if (_purpose == BlockShape.MeshPurpose.SimplifiedCollisionOnly)
		{
			return;
		}
		for (int j = 0; j < 1; j++)
		{
			int num4 = j * 7;
			BlockShapeNew.MySimpleMesh visualMesh = this.getVisualMesh(num + num4);
			if (visualMesh != null)
			{
				int count = visualMesh.Vertices.Count;
				voxelMesh.CheckVertexLimit(count);
				if (count + voxelMesh.m_Vertices.Count <= 786432)
				{
					int num5 = 0;
					if (num2 == 0)
					{
						int num6 = Chunk.Value64FullToIndex(_textureFullArray[j], (BlockFace)num);
						BlockTextureData blockTextureData = BlockTextureData.list[num6];
						if (blockTextureData == null)
						{
							if (!DynamicMeshBlockSwap.InvalidPaintIds.Contains(num6))
							{
								DynamicMeshBlockSwap.InvalidPaintIds.Add(num6);
								Log.Out(string.Concat(new string[]
								{
									"Missing paint ID XML entry: ",
									num6.ToString(),
									" for block '",
									block.GetBlockName(),
									"'"
								}));
							}
						}
						else
						{
							num5 = (int)blockTextureData.TextureID;
						}
					}
					int num7 = ((num5 == 0) ? block.GetSideTextureId(_blockValue, (BlockFace)num, j) : num5);
					if ((ulong)num7 < (ulong)((long)meshDescription.textureAtlas.uvMapping.Length))
					{
						UVRectTiling uvrectTiling = meshDescription.textureAtlas.uvMapping[num7];
						if (uvrectTiling.blockW == 0 || uvrectTiling.blockH == 0)
						{
							Log.Error("Block with name '{0}' uses a texture id {1} that is not in the atlas!", new object[]
							{
								block.GetBlockName(),
								num7
							});
						}
						else
						{
							int num8 = uvrectTiling.blockW;
							int num9 = uvrectTiling.blockH;
							bool flag = uvrectTiling.bGlobalUV;
							Block.UVMode uvmode = block.GetUVMode(num, j);
							if (uvmode != Block.UVMode.Global)
							{
								if (uvmode == Block.UVMode.Local)
								{
									flag = false;
								}
							}
							else
							{
								flag = true;
							}
							flag &= _purpose != BlockShape.MeshPurpose.Local;
							Vector3 vector4;
							Vector3 vector5;
							this.convertRotationUVDirs((BlockFace)num, (int)_blockValue.rotation, out vector4, out vector5);
							float num10 = 0f;
							float num11 = 0f;
							float num12 = 0f;
							float num13 = 0f;
							if (!flag || !bTextureArray)
							{
								switch (_face)
								{
								case BlockFace.Top:
								case BlockFace.Bottom:
									if (vector4.x < -0.9f || vector4.x > 0.9f)
									{
										num10 = vector4.x;
										num12 = vector.x;
									}
									else
									{
										num10 = vector4.z;
										num12 = vector.z;
									}
									if (vector5.z < -0.9f || vector5.z > 0.9f)
									{
										num11 = vector5.z;
										num13 = vector.z;
									}
									else
									{
										num11 = vector5.x;
										num13 = vector.x;
									}
									break;
								case BlockFace.North:
								case BlockFace.South:
									if (vector4.x < -0.9f || vector4.x > 0.9f)
									{
										num10 = vector4.x;
										num12 = vector.x;
									}
									else
									{
										num10 = vector4.y;
										num12 = vector.y;
									}
									if (vector5.y < -0.9f || vector5.y > 0.9f)
									{
										num11 = vector5.y;
										num13 = vector.y;
									}
									else
									{
										num11 = vector5.x;
										num13 = vector.x;
									}
									break;
								case BlockFace.West:
								case BlockFace.East:
									if (vector4.z < -0.9f || vector4.z > 0.9f)
									{
										num10 = vector4.z;
										num12 = vector.z;
									}
									else
									{
										num10 = vector4.y;
										num12 = vector.y;
									}
									if (vector5.y < -0.9f || vector5.y > 0.9f)
									{
										num11 = vector5.y;
										num13 = vector.y;
									}
									else
									{
										num11 = vector5.z;
										num13 = vector.z;
									}
									break;
								}
							}
							Vector2 vector6 = Vector3.zero;
							int num14 = (int)uvrectTiling.uv.width;
							int num15 = (int)uvrectTiling.uv.height;
							int num16 = 0;
							if (!bTextureArray)
							{
								vector6.x = ((num10 > 0f) ? (uvrectTiling.uv.x + (float)BlockShapeNew.FloorModFloat(num12, num8) * uvrectTiling.uv.width) : (uvrectTiling.uv.x + uvrectTiling.uv.width * (float)(num8 - 1) - (float)BlockShapeNew.FloorModFloat(num12, num8) * uvrectTiling.uv.width));
								vector6.y = ((num11 > 0f) ? (uvrectTiling.uv.y + (float)BlockShapeNew.FloorModFloat(num13, num9) * uvrectTiling.uv.height) : (uvrectTiling.uv.y + uvrectTiling.uv.height * (float)(num9 - 1) - (float)BlockShapeNew.FloorModFloat(num13, num9) * uvrectTiling.uv.height));
							}
							else if (!flag)
							{
								num8 = Utils.FastMax(num8, num14);
								num9 = Utils.FastMax(num9, num15);
								vector6.x = (float)((num10 > 0f) ? BlockShapeNew.FloorModFloat(num12, num8) : (num8 - 1 - BlockShapeNew.FloorModFloat(num12, num8)));
								vector6.y = (float)((num11 > 0f) ? BlockShapeNew.FloorModFloat(num13, num9) : (num9 - 1 - BlockShapeNew.FloorModFloat(num13, num9)));
								if (num14 > 1)
								{
									num16 += BlockShapeNew.FloorModInt((int)vector6.x, num14);
								}
								if (num15 > 1)
								{
									num16 += BlockShapeNew.FloorModInt((int)vector6.y, num15) * num14;
								}
								vector6.x -= (float)((int)vector6.x);
								vector6.y -= (float)((int)vector6.y);
							}
							int num17 = uvrectTiling.index + num16;
							Color color;
							if (bTextureArray)
							{
								color.g = (float)num17;
							}
							else
							{
								color.g = (float)(block.Properties.Contains("Frame") ? 1 : 0);
							}
							color.b = (float)_lightingAround[LightingAround.Pos.Middle].stability / 15f;
							color.a = (float)(flag ? 1 : 0);
							if (bTextureArray && flag)
							{
								for (int k = 0; k < visualMesh.Indices.Count; k += 3)
								{
									BlockShapeNew.PlanePick planePick = BlockShapeNew.ClassifyTriPlane(visualMesh, k, quaternion);
									BlockShapeNew.PlaneAxis axis = planePick.axis;
									List<int> list;
									if (axis != BlockShapeNew.PlaneAxis.X)
									{
										if (axis != BlockShapeNew.PlaneAxis.Y)
										{
											list = (planePick.neg ? BlockShapeNew.threadData.trisZNeg : BlockShapeNew.threadData.trisZPos);
										}
										else
										{
											list = (planePick.neg ? BlockShapeNew.threadData.trisYNeg : BlockShapeNew.threadData.trisYPos);
										}
									}
									else
									{
										list = (planePick.neg ? BlockShapeNew.threadData.trisXNeg : BlockShapeNew.threadData.trisXPos);
									}
									List<int> list2 = list;
									list2.Add(k);
									list2.Add(k + 1);
									list2.Add(k + 2);
								}
								int num18 = Mathf.Max(uvrectTiling.blockW, num14);
								int num19 = Mathf.Max(uvrectTiling.blockH, num15);
								Rect rect = WorldConstants.MapDamageToUVRect(_blockValue);
								Vector3 vector7 = _drawPos + _chunkPos + new Vector3(0.5f, 0.5f, 0.5f);
								if (BlockShapeNew.threadData.trisXPos.Count > 0)
								{
									int num20 = uvrectTiling.index + this.ComputeTaIndexOffsFor(BlockShapeNew.PlaneAxis.X, uvrectTiling, vector7, false);
									this.AppendSubsetTriPlanar(voxelMesh, visualMesh, BlockShapeNew.threadData.trisXPos, quaternion, vector2, vector3, num20, num18, num19, num14, num15, _drawPos, _lightingAround, rect, BlockShapeNew.bImposterGenerationActive);
									BlockShapeNew.threadData.trisXPos.Clear();
								}
								if (BlockShapeNew.threadData.trisXNeg.Count > 0)
								{
									int num21 = uvrectTiling.index + this.ComputeTaIndexOffsFor(BlockShapeNew.PlaneAxis.X, uvrectTiling, vector7, true);
									this.AppendSubsetTriPlanar(voxelMesh, visualMesh, BlockShapeNew.threadData.trisXNeg, quaternion, vector2, vector3, num21, num18, num19, num14, num15, _drawPos, _lightingAround, rect, BlockShapeNew.bImposterGenerationActive);
									BlockShapeNew.threadData.trisXNeg.Clear();
								}
								if (BlockShapeNew.threadData.trisYPos.Count > 0)
								{
									int num22 = uvrectTiling.index + this.ComputeTaIndexOffsFor(BlockShapeNew.PlaneAxis.Y, uvrectTiling, vector7, false);
									this.AppendSubsetTriPlanar(voxelMesh, visualMesh, BlockShapeNew.threadData.trisYPos, quaternion, vector2, vector3, num22, num18, num19, num14, num15, _drawPos, _lightingAround, rect, BlockShapeNew.bImposterGenerationActive);
									BlockShapeNew.threadData.trisYPos.Clear();
								}
								if (BlockShapeNew.threadData.trisYNeg.Count > 0)
								{
									int num23 = uvrectTiling.index + this.ComputeTaIndexOffsFor(BlockShapeNew.PlaneAxis.Y, uvrectTiling, vector7, false);
									this.AppendSubsetTriPlanar(voxelMesh, visualMesh, BlockShapeNew.threadData.trisYNeg, quaternion, vector2, vector3, num23, num18, num19, num14, num15, _drawPos, _lightingAround, rect, BlockShapeNew.bImposterGenerationActive);
									BlockShapeNew.threadData.trisYNeg.Clear();
								}
								if (BlockShapeNew.threadData.trisZPos.Count > 0)
								{
									int num24 = uvrectTiling.index + this.ComputeTaIndexOffsFor(BlockShapeNew.PlaneAxis.Z, uvrectTiling, vector7, true);
									this.AppendSubsetTriPlanar(voxelMesh, visualMesh, BlockShapeNew.threadData.trisZPos, quaternion, vector2, vector3, num24, num18, num19, num14, num15, _drawPos, _lightingAround, rect, BlockShapeNew.bImposterGenerationActive);
									BlockShapeNew.threadData.trisZPos.Clear();
								}
								if (BlockShapeNew.threadData.trisZNeg.Count > 0)
								{
									int num25 = uvrectTiling.index + this.ComputeTaIndexOffsFor(BlockShapeNew.PlaneAxis.Z, uvrectTiling, vector7, false);
									this.AppendSubsetTriPlanar(voxelMesh, visualMesh, BlockShapeNew.threadData.trisZNeg, quaternion, vector2, vector3, num25, num18, num19, num14, num15, _drawPos, _lightingAround, rect, BlockShapeNew.bImposterGenerationActive);
									BlockShapeNew.threadData.trisZNeg.Clear();
								}
							}
							else
							{
								int count2 = voxelMesh.m_Vertices.Count;
								ArrayListMP<Vector3> collVertices = voxelMesh.m_CollVertices;
								if (collVertices != null)
								{
									int count3 = collVertices.Count;
								}
								int num26 = voxelMesh.m_Vertices.Alloc(count);
								voxelMesh.m_Normals.Alloc(count);
								voxelMesh.m_ColorVertices.Alloc(count);
								for (int l = 0; l < count; l++)
								{
									int num27 = num26 + l;
									Vector3 vector8 = quaternion * (visualMesh.Vertices[l] + vector2) + vector3;
									voxelMesh.m_Vertices[num27] = vector8;
									Vector3 vector9 = quaternion * visualMesh.Normals[l];
									voxelMesh.m_Normals[num27] = vector9;
									vector8 -= _drawPos;
									float num28 = (float)_lightingAround[LightingAround.Pos.X0Y0Z0].sun * (1f - vector8.x) * (1f - vector8.y) * (1f - vector8.z) + (float)_lightingAround[LightingAround.Pos.X1Y0Z0].sun * vector8.x * (1f - vector8.y) * (1f - vector8.z) + (float)_lightingAround[LightingAround.Pos.X0Y0Z1].sun * (1f - vector8.x) * (1f - vector8.y) * vector8.z + (float)_lightingAround[LightingAround.Pos.X1Y0Z1].sun * vector8.x * (1f - vector8.y) * vector8.z + (float)_lightingAround[LightingAround.Pos.X0Y1Z0].sun * (1f - vector8.x) * vector8.y * (1f - vector8.z) + (float)_lightingAround[LightingAround.Pos.X0Y1Z1].sun * (1f - vector8.x) * vector8.y * vector8.z + (float)_lightingAround[LightingAround.Pos.X1Y1Z0].sun * vector8.x * vector8.y * (1f - vector8.z) + (float)_lightingAround[LightingAround.Pos.X1Y1Z1].sun * vector8.x * vector8.y * vector8.z;
									color.r = num28 / 15f;
									voxelMesh.m_ColorVertices[num27] = color;
								}
								int num29 = voxelMesh.m_Indices.Alloc(visualMesh.Indices.Count);
								for (int m = 0; m < visualMesh.Indices.Count; m++)
								{
									voxelMesh.m_Indices[num29 + m] = voxelMesh.CurTriangleIndex + (int)visualMesh.Indices[m];
								}
								voxelMesh.CurTriangleIndex += visualMesh.Vertices.Count;
								int count4 = visualMesh.Uvs.Count;
								Rect rect = WorldConstants.MapDamageToUVRect(_blockValue);
								if (bTextureArray)
								{
									num29 = voxelMesh.m_Uvs.Alloc(count4);
									Vector2 vector10;
									for (int n = 0; n < count4; n++)
									{
										int num30 = num29 + n;
										if (flag)
										{
											vector10.x = (float)(num8 / num14);
											vector10.y = (float)(num9 / num15);
										}
										else
										{
											vector10.x = vector6.x + visualMesh.Uvs[n].x;
											vector10.y = vector6.y + visualMesh.Uvs[n].y;
										}
										voxelMesh.m_Uvs[num30] = vector10;
									}
									if (voxelMesh.UvsCrack != null)
									{
										voxelMesh.UvsCrack.Alloc(count4);
										for (int num31 = 0; num31 < count4; num31++)
										{
											int num32 = num29 + num31;
											if (!BlockShapeNew.bImposterGenerationActive)
											{
												vector10.x = rect.x + visualMesh.Uvs[num31].x * rect.width;
												vector10.y = rect.y + visualMesh.Uvs[num31].y * rect.height;
											}
											else
											{
												vector10.x = color.g;
												vector10.y = color.a;
											}
											voxelMesh.UvsCrack[num32] = vector10;
										}
									}
								}
								else if (!flag)
								{
									for (int num33 = 0; num33 < count4; num33++)
									{
										Vector2 vector10;
										vector10.x = vector6.x + visualMesh.Uvs[num33].x * uvrectTiling.uv.width;
										vector10.y = vector6.y + visualMesh.Uvs[num33].y * uvrectTiling.uv.height;
										voxelMesh.m_Uvs.Add(vector10);
										if (voxelMesh.UvsCrack != null)
										{
											vector10.x = rect.x + visualMesh.Uvs[num33].x * rect.width;
											vector10.y = rect.y + visualMesh.Uvs[num33].y * rect.height;
											voxelMesh.UvsCrack.Add(vector10);
										}
									}
								}
								else
								{
									Vector3i vector3i;
									vector3i.x = num8;
									vector3i.y = num9;
									vector3i.z = num8;
									Vector3i vector3i2;
									vector3i2.x = Mathf.Abs(_chunkPos.x + (int)_drawPos.x);
									vector3i2.y = Mathf.Abs(_chunkPos.y + (int)_drawPos.y);
									vector3i2.z = Mathf.Abs(_chunkPos.z + (int)_drawPos.z);
									if (_chunkPos.x < 0)
									{
										vector3i2.x = Mathf.Abs(_chunkPos.x + 16 - (int)_drawPos.x);
									}
									if (_chunkPos.y < 0)
									{
										vector3i2.y = Mathf.Abs(_chunkPos.y + 16 - (int)_drawPos.y);
									}
									if (_chunkPos.z < 0)
									{
										vector3i2.z = Mathf.Abs(_chunkPos.z + 16 - (int)_drawPos.z);
									}
									Vector3 vector11;
									vector11.x = (float)(vector3i2.x % vector3i.x);
									vector11.y = (float)(vector3i2.y % vector3i.y);
									vector11.z = (float)(vector3i2.z % vector3i.z);
									bool flag2 = false;
									for (int num34 = 0; num34 < 6; num34++)
									{
										BlockShapeNew.MySimpleMesh visualMesh2 = this.getVisualMesh(num34);
										if (visualMesh2 != null && visualMesh2.Uvs.Count > 6)
										{
											flag2 = true;
											break;
										}
									}
									for (int num35 = 0; num35 < count4; num35++)
									{
										Vector3 vector12 = voxelMesh.m_Normals[count2];
										object obj = Mathf.Abs(vector12.y) > Mathf.Abs(vector12.x) && Mathf.Abs(vector12.y) > Mathf.Abs(vector12.z);
										bool flag3 = Mathf.Abs(vector12.x) > Mathf.Abs(vector12.y) && Mathf.Abs(vector12.x) > Mathf.Abs(vector12.z);
										object obj2 = obj;
										if (obj2 == null)
										{
											bool flag4 = !flag3;
										}
										if (obj2 != null)
										{
											vector3i.z = uvrectTiling.blockH;
											vector11.z = (float)(vector3i2.z % vector3i.z);
										}
										Vector2 zero = Vector2.zero;
										vector12 = voxelMesh.m_Normals[count2 + num35];
										if (!flag2)
										{
											BlockFace blockFace = BlockFace.Top;
											if (vector12.z > 0.95f)
											{
												blockFace = BlockFace.North;
											}
											else if (vector12.z < -0.95f)
											{
												blockFace = BlockFace.South;
											}
											else if (vector12.x > 0.95f)
											{
												blockFace = BlockFace.East;
											}
											else if (vector12.x < -0.95f)
											{
												blockFace = BlockFace.West;
											}
											else if (vector12.y < 0f)
											{
												blockFace = BlockFace.Bottom;
											}
											bool flag5 = Mathf.Abs(vector12.z) > 0.0001f;
											bool flag6 = vector12.y < 0f;
											bool flag7 = Mathf.Abs(vector12.y) > 0.99f;
											bool flag8 = Mathf.Abs(vector12.x) > 0.0001f;
											bool flag9 = vector12.x > 0.0001f;
											bool flag10 = vector12.z > 0.0001f;
											bool flag11 = Mathf.Abs(vector12.x) > 0.1f && Mathf.Abs(vector12.y) > 0.1f && Mathf.Abs(vector12.z) > 0.1f;
											if ((blockFace == BlockFace.Top || blockFace == BlockFace.Bottom) && flag8 && !flag5)
											{
												vector3i.x = uvrectTiling.blockH;
												vector3i.z = uvrectTiling.blockW;
											}
											Vector3 vector13 = voxelMesh.m_Vertices[count2 + num35] - _drawPos;
											vector13.x = Mathf.Abs(vector13.x);
											vector13.y = Mathf.Abs(vector13.y);
											vector13.z = Mathf.Abs(vector13.z);
											vector11.x = Mathf.Abs(vector11.x);
											vector11.y = Mathf.Abs(vector11.y);
											vector11.z = Mathf.Abs(vector11.z);
											Vector3 vector14 = vector11 + vector13;
											vector14.x = (((int)Mathf.Abs(vector14.x) != vector3i.x) ? (Mathf.Abs(vector14.x) % (float)vector3i.x) : Mathf.Abs(vector14.x));
											vector14.y = (((int)Mathf.Abs(vector14.y) != vector3i.y) ? (Mathf.Abs(vector14.y) % (float)vector3i.y) : Mathf.Abs(vector14.y));
											vector14.z = (((int)Mathf.Abs(vector14.z) != vector3i.z) ? (Mathf.Abs(vector14.z) % (float)vector3i.z) : Mathf.Abs(vector14.z));
											if ((int)vector14.x > vector3i.x)
											{
												vector14.x %= (float)vector3i.x;
											}
											if ((int)vector14.y > vector3i.y)
											{
												vector14.y %= (float)vector3i.y;
											}
											if ((int)vector14.z > vector3i.z)
											{
												vector14.z %= (float)vector3i.z;
											}
											Vector3 vector15 = vector3i.ToVector3() - vector14;
											float num36 = (((int)Mathf.Abs(vector14.x) != vector3i.x) ? (Mathf.Abs(vector14.x) % (float)vector3i.x) : Mathf.Abs(vector14.x));
											float num37 = (((int)Mathf.Abs(vector14.z) != vector3i.z) ? (Mathf.Abs(vector14.z) % (float)vector3i.z) : Mathf.Abs(vector14.z));
											switch (blockFace)
											{
											case BlockFace.Top:
											case BlockFace.Bottom:
												if (!flag7)
												{
													if (flag8)
													{
														if (flag9)
														{
															if (flag5)
															{
																if (flag10)
																{
																	if (flag11)
																	{
																		zero.x = (vector14.x + vector15.z) * 0.5f;
																	}
																	else
																	{
																		zero.x = (float)vector3i.x - num36;
																	}
																}
																else if (flag11)
																{
																	zero.x = (vector15.x + vector15.z) * 0.5f;
																}
																else
																{
																	zero.x = num36;
																}
																zero.y = vector14.y;
															}
															else
															{
																zero.x = vector14.z;
																if (flag6)
																{
																	zero.y = vector14.x;
																}
																else
																{
																	zero.y = vector15.x;
																}
															}
														}
														else if (flag5)
														{
															if (flag10)
															{
																if (flag11)
																{
																	zero.x = (vector15.x + vector15.z) * 0.5f;
																}
																else
																{
																	zero.x = (float)vector3i.x - num36;
																}
															}
															else if (flag11)
															{
																zero.x = (vector14.x + vector15.z) * 0.5f;
															}
															else
															{
																zero.x = num36;
															}
															zero.y = vector14.y;
														}
														else
														{
															zero.x = vector15.z;
															if (flag6)
															{
																zero.y = vector15.x;
															}
															else
															{
																zero.y = vector14.x;
															}
														}
													}
													else if (flag10)
													{
														zero.x = vector15.x;
														if (flag6)
														{
															zero.y = vector14.z;
														}
														else
														{
															zero.y = vector15.z;
														}
													}
													else
													{
														zero.x = vector14.x;
														if (flag6)
														{
															zero.y = vector15.z;
														}
														else
														{
															zero.y = vector14.z;
														}
													}
												}
												else
												{
													zero.x = num36;
													zero.y = num37;
												}
												break;
											case BlockFace.North:
												zero.x = vector15.x;
												zero.y = vector14.y;
												break;
											case BlockFace.West:
												zero.x = vector15.z;
												zero.y = vector14.y;
												break;
											case BlockFace.South:
												goto IL_1828;
											case BlockFace.East:
												zero.x = vector14.z;
												zero.y = vector14.y;
												break;
											default:
												goto IL_1828;
											}
											IL_1844:
											voxelMesh.m_Uvs.Add(new Vector2(uvrectTiling.uv.x + zero.x * uvrectTiling.uv.width, uvrectTiling.uv.y + zero.y * uvrectTiling.uv.height));
											goto IL_191D;
											IL_1828:
											zero.x = vector14.x;
											zero.y = vector14.y;
											goto IL_1844;
										}
										zero.x = Mathf.Floor(1f / uvrectTiling.uv.width * 1000f);
										zero.y = (float)uvrectTiling.blockW + Mathf.Floor((float)(uvrectTiling.blockH * 1000));
										zero.x += uvrectTiling.uv.x;
										zero.y += uvrectTiling.uv.y;
										voxelMesh.m_Uvs.Add(zero);
										IL_191D:
										if (voxelMesh.UvsCrack != null)
										{
											voxelMesh.UvsCrack.Add(new Vector2(rect.x + visualMesh.Uvs[num35].x * rect.width, rect.y + visualMesh.Uvs[num35].y * rect.height));
										}
									}
								}
							}
						}
					}
				}
			}
		}
	}

	public override bool IsRenderDecoration()
	{
		return true;
	}

	public override void renderDecorations(Vector3i _worldPos, BlockValue _blockValue, Vector3 _drawPos, Vector3[] _vertices, LightingAround _lightingAround, TextureFullArray _textureFullArray, VoxelMesh[] _meshes, INeighborBlockCache _nBlocks)
	{
		if (_blockValue.ischild)
		{
			return;
		}
		for (int i = 0; i < this.faceInfo.Length; i++)
		{
			int num = BlockShapeNew.convertRotationCached[(int)_blockValue.rotation, i];
			if (this.visualMeshes[num] != null)
			{
				BlockShapeNew.EnumFaceOcclusionInfo enumFaceOcclusionInfo = this.faceInfo[num];
				if (enumFaceOcclusionInfo != BlockShapeNew.EnumFaceOcclusionInfo.None)
				{
					if (enumFaceOcclusionInfo != BlockShapeNew.EnumFaceOcclusionInfo.OwnFaces)
					{
						if (enumFaceOcclusionInfo != BlockShapeNew.EnumFaceOcclusionInfo.Transparent)
						{
							goto IL_014F;
						}
					}
					else
					{
						BlockFace blockFace = (BlockFace)i;
						Vector3i vector3i = BlockFaceFlags.OffsetIForFace(blockFace);
						BlockValue blockValue = _nBlocks.Get(vector3i.x, vector3i.y + (int)_drawPos.y, vector3i.z);
						Block block = blockValue.Block;
						bool flag = !blockValue.ischild && block.shape.IsSolidCube && !block.shape.IsTerrain();
						if (!flag)
						{
							int num2 = 0;
							switch (blockFace)
							{
							case BlockFace.Top:
								num2 = 2;
								break;
							case BlockFace.Bottom:
								num2 = 1;
								break;
							case BlockFace.North:
								num2 = 16;
								break;
							case BlockFace.West:
								num2 = 32;
								break;
							case BlockFace.South:
								num2 = 4;
								break;
							case BlockFace.East:
								num2 = 8;
								break;
							}
							int facesDrawnFullBitfield = block.shape.getFacesDrawnFullBitfield(blockValue);
							flag = flag || (facesDrawnFullBitfield & num2) != 0;
						}
						if (flag && this.isRenderFace(_blockValue, blockFace, blockValue))
						{
							this.renderFace(_worldPos, _blockValue, _drawPos, blockFace, _vertices, _lightingAround, _textureFullArray, _meshes, BlockShape.MeshPurpose.World);
							goto IL_014F;
						}
						goto IL_014F;
					}
				}
				this.renderFace(_worldPos, _blockValue, _drawPos, (BlockFace)i, _vertices, _lightingAround, _textureFullArray, _meshes, BlockShape.MeshPurpose.World);
			}
			IL_014F:;
		}
	}

	public override void renderFull(Vector3i _worldPos, BlockValue _blockValue, Vector3 _drawPos, Vector3[] _vertices, LightingAround _lightingAround, TextureFullArray _textureFullArray, VoxelMesh[] _meshes, BlockShape.MeshPurpose _purpose = BlockShape.MeshPurpose.World)
	{
		if (_blockValue.ischild)
		{
			return;
		}
		int num = 7;
		for (int i = 0; i < num; i++)
		{
			this.renderFace(_worldPos, _blockValue, _drawPos, (BlockFace)i, _vertices, _lightingAround, _textureFullArray, _meshes, _purpose);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static int BlockFaceToRot(BlockFace _blockFace)
	{
		return (int)((int)_blockFace << 2);
	}

	public static BlockFace RotToBlockFace(int _rotation)
	{
		int num = (_rotation >> 2) & 7;
		if (num <= 5)
		{
			return (BlockFace)num;
		}
		return BlockFace.Top;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static int RotToLocalRot(int _rotation)
	{
		return _rotation & 3;
	}

	public static Quaternion GetRotationStatic(int _rotation)
	{
		return BlockShapeNew.rotationsToQuats[_rotation];
	}

	public override Quaternion GetRotation(BlockValue _blockValue)
	{
		return BlockShapeNew.rotationsToQuats[(int)_blockValue.rotation];
	}

	public override byte Rotate(bool _bLeft, int _rotation)
	{
		_rotation += (_bLeft ? (-1) : 1);
		if (_rotation > 23)
		{
			_rotation = 0;
		}
		if (_rotation < 0)
		{
			_rotation = 23;
		}
		return (byte)_rotation;
	}

	public override Quaternion GetPreviewRotation()
	{
		return Quaternion.AngleAxis(180f, Vector3.up);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	static BlockShapeNew()
	{
		for (int i = 1; i < 4; i++)
		{
			for (byte b = 0; b < 28; b += 1)
			{
				BlockShapeNew.rotations[i - 1, (int)b] = BlockShapeNew.CalcRotation(b, i);
			}
		}
	}

	public override BlockValue RotateY(bool _bLeft, BlockValue _blockValue, int _rotCount)
	{
		if (_rotCount == 0)
		{
			return _blockValue;
		}
		if (_bLeft)
		{
			_rotCount = 4 - _rotCount;
		}
		_blockValue.rotation = BlockShapeNew.rotations[_rotCount - 1, (int)_blockValue.rotation];
		return _blockValue;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static byte CalcRotation(byte _rotation, int _rotCount)
	{
		if (_rotation >= 24)
		{
			for (int i = 0; i < _rotCount; i++)
			{
				_rotation += 1;
				if (_rotation > 27)
				{
					_rotation = 24;
				}
				else if (_rotation < 24)
				{
					_rotation = 27;
				}
			}
		}
		else
		{
			int num = 90 * _rotCount;
			_rotation = (byte)BlockShapeNew.ConvertRotationFree((int)_rotation, Quaternion.AngleAxis((float)num, Vector3.up), false);
		}
		return _rotation;
	}

	public static int MirrorStatic(EnumMirrorAlong _axis, int _rotation, int _symType = 1)
	{
		switch (_symType)
		{
		case 0:
			return _rotation;
		default:
		{
			Quaternion quaternion = BlockShapeNew.GetRotationStatic(_rotation);
			switch (_axis)
			{
			case EnumMirrorAlong.XAxis:
				quaternion.x = -quaternion.x;
				quaternion.w = -quaternion.w;
				break;
			case EnumMirrorAlong.YAxis:
				quaternion.y = -quaternion.y;
				quaternion.w = -quaternion.w;
				quaternion *= Quaternion.AngleAxis(180f, Vector3.forward);
				break;
			case EnumMirrorAlong.ZAxis:
				quaternion.z = -quaternion.z;
				quaternion.w = -quaternion.w;
				quaternion *= Quaternion.AngleAxis(180f, Vector3.up);
				break;
			}
			return BlockShapeNew.ConvertRotationFree(0, quaternion, false);
		}
		case 2:
		{
			BlockFace blockFace = BlockShapeNew.RotToBlockFace(_rotation);
			int num = 0;
			switch (_axis)
			{
			case EnumMirrorAlong.XAxis:
				if (blockFace != BlockFace.West)
				{
					if (blockFace == BlockFace.East)
					{
						blockFace = BlockFace.West;
					}
				}
				else
				{
					blockFace = BlockFace.East;
				}
				break;
			case EnumMirrorAlong.YAxis:
				switch (blockFace)
				{
				case BlockFace.Top:
					blockFace = BlockFace.Bottom;
					break;
				case BlockFace.Bottom:
					blockFace = BlockFace.Top;
					break;
				case BlockFace.North:
				case BlockFace.South:
					num = 1;
					break;
				}
				break;
			case EnumMirrorAlong.ZAxis:
				if (blockFace != BlockFace.North)
				{
					if (blockFace != BlockFace.South)
					{
						num = 1;
					}
					else
					{
						blockFace = BlockFace.North;
					}
				}
				else
				{
					blockFace = BlockFace.South;
				}
				break;
			}
			int num2 = BlockShapeNew.RotToLocalRot(_rotation);
			if (num != 0)
			{
				if (num == 1)
				{
					switch (num2)
					{
					case 0:
						num2 = 1;
						break;
					case 1:
						num2 = 0;
						break;
					case 2:
						num2 = 3;
						break;
					case 3:
						num2 = 2;
						break;
					}
				}
			}
			else
			{
				switch (num2)
				{
				case 0:
					num2 = 3;
					break;
				case 1:
					num2 = 2;
					break;
				case 2:
					num2 = 1;
					break;
				case 3:
					num2 = 0;
					break;
				}
			}
			return BlockShapeNew.BlockFaceToRot(blockFace) | num2;
		}
		case 3:
		{
			BlockFace blockFace2 = BlockShapeNew.RotToBlockFace(_rotation);
			int num3 = 1;
			switch (_axis)
			{
			case EnumMirrorAlong.XAxis:
				if (blockFace2 != BlockFace.West)
				{
					if (blockFace2 == BlockFace.East)
					{
						blockFace2 = BlockFace.West;
					}
				}
				else
				{
					blockFace2 = BlockFace.East;
				}
				break;
			case EnumMirrorAlong.YAxis:
				switch (blockFace2)
				{
				case BlockFace.Top:
					blockFace2 = BlockFace.Bottom;
					break;
				case BlockFace.Bottom:
					blockFace2 = BlockFace.Top;
					break;
				case BlockFace.North:
				case BlockFace.South:
					num3 = 0;
					break;
				}
				break;
			case EnumMirrorAlong.ZAxis:
				if (blockFace2 != BlockFace.North)
				{
					if (blockFace2 != BlockFace.South)
					{
						num3 = 0;
					}
					else
					{
						blockFace2 = BlockFace.North;
					}
				}
				else
				{
					blockFace2 = BlockFace.South;
				}
				break;
			}
			int num4 = BlockShapeNew.RotToLocalRot(_rotation);
			if (num3 != 0)
			{
				if (num3 == 1)
				{
					switch (num4)
					{
					case 0:
						num4 = 1;
						break;
					case 1:
						num4 = 0;
						break;
					case 2:
						num4 = 3;
						break;
					case 3:
						num4 = 2;
						break;
					}
				}
			}
			else
			{
				switch (num4)
				{
				case 0:
					num4 = 3;
					break;
				case 1:
					num4 = 2;
					break;
				case 2:
					num4 = 1;
					break;
				case 3:
					num4 = 0;
					break;
				}
			}
			return BlockShapeNew.BlockFaceToRot(blockFace2) | num4;
		}
		case 4:
			BlockShapeNew.GetRotationStatic(_rotation);
			switch (_axis)
			{
			case EnumMirrorAlong.XAxis:
				_rotation = BlockShapeNew.ConvertRotationFree(_rotation, Quaternion.AngleAxis(180f, Vector3.up), true);
				return BlockShapeNew.ConvertRotationFree(_rotation, Quaternion.AngleAxis(180f, Vector3.right), false);
			case EnumMirrorAlong.YAxis:
				_rotation = BlockShapeNew.ConvertRotationFree(_rotation, Quaternion.AngleAxis(180f, Vector3.up), true);
				return BlockShapeNew.ConvertRotationFree(_rotation, Quaternion.AngleAxis(180f, Vector3.up), false);
			case EnumMirrorAlong.ZAxis:
				_rotation = BlockShapeNew.ConvertRotationFree(_rotation, Quaternion.AngleAxis(180f, Vector3.up), true);
				return BlockShapeNew.ConvertRotationFree(_rotation, Quaternion.AngleAxis(180f, Vector3.forward), false);
			default:
				return _rotation;
			}
			break;
		}
	}

	public BlockFace GetBlockFaceFromColliderTriangle(BlockValue _blockValue, Vector3 _v1, Vector3 _v2, Vector3 _v3)
	{
		for (int i = 0; i < this.colliderMeshes.Length; i++)
		{
			if (BlockShapeNew.<GetBlockFaceFromColliderTriangle>g__CheckMeshForTri|55_0(this.colliderMeshes[i], _v1, _v2, _v3))
			{
				return (BlockFace)i;
			}
		}
		for (int j = 0; j < this.visualMeshes.Length; j++)
		{
			if (BlockShapeNew.<GetBlockFaceFromColliderTriangle>g__CheckMeshForTri|55_0(this.visualMeshes[j], _v1, _v2, _v3))
			{
				return (BlockFace)(j % 7);
			}
		}
		return BlockFace.None;
	}

	public int GetVisualMeshChannelFromHitInfo(Vector3i blockPos, BlockValue bv, BlockFace blockFace, WorldRayHitInfo hitInfo)
	{
		if (this.visualMeshes.Length <= 7)
		{
			return 0;
		}
		int num = 0;
		int num2 = -1;
		for (int i = 0; i < 1; i++)
		{
			int num3 = i * 7;
			if (this.getVisualMesh((int)(blockFace + (byte)num3)) != null)
			{
				num++;
				num2 = i;
			}
		}
		if (num == 1)
		{
			return num2;
		}
		Vector3 vector = Vector3.one * 0.5f;
		Quaternion quaternion = Quaternion.Inverse(this.GetRotation(bv));
		Ray ray = hitInfo.ray;
		ray.origin = quaternion * (ray.origin - blockPos - vector) + vector;
		ray.direction = quaternion * ray.direction;
		float num4 = float.MaxValue;
		int num5 = -1;
		for (int j = 0; j < 1; j++)
		{
			int num6 = j * 7;
			BlockShapeNew.MySimpleMesh visualMesh = this.getVisualMesh((int)(blockFace + (byte)num6));
			if (visualMesh != null)
			{
				for (int k = 0; k < visualMesh.Indices.Count; k += 3)
				{
					GeometryUtils.Triangle triangle = new GeometryUtils.Triangle(visualMesh.Vertices[(int)visualMesh.Indices[k]], visualMesh.Vertices[(int)visualMesh.Indices[k + 1]], visualMesh.Vertices[(int)visualMesh.Indices[k + 2]]);
					Vector3 vector2;
					float num7;
					if (GeometryUtils.IntersectRayTriangle(ray, triangle, out vector2, out num7) && num7 < num4)
					{
						num4 = num7;
						num5 = j;
					}
				}
			}
		}
		return num5;
	}

	public override Vector2 GetPathOffset(int _rotation)
	{
		return this.boundsPathOffsetRotations[_rotation];
	}

	public override float GetStepHeight(BlockValue blockDef, BlockFace crossingFace)
	{
		return (float)(blockDef.Block.IsCollideMovement ? 1 : 0);
	}

	public override Bounds[] GetBounds(BlockValue _blockValue)
	{
		this.boundsArr[0] = this.boundsRotations[(int)_blockValue.rotation];
		return this.boundsArr;
	}

	public override BlockFace GetRotatedBlockFace(BlockValue _blockValue, BlockFace _face)
	{
		return (BlockFace)BlockShapeNew.convertRotationCached[(int)_blockValue.rotation, (int)_face];
	}

	public override void MirrorFace(EnumMirrorAlong _axis, int _sourceRot, int _targetRot, BlockFace _face, out BlockFace _sourceFace, out BlockFace _targetFace)
	{
		_sourceFace = (BlockFace)BlockShapeNew.convertRotationCached[_sourceRot, (int)_face];
		switch (_axis)
		{
		case EnumMirrorAlong.XAxis:
			switch (_face)
			{
			case BlockFace.Top:
				_face = BlockFace.Top;
				break;
			case BlockFace.Bottom:
				_face = BlockFace.Bottom;
				break;
			case BlockFace.North:
				_face = BlockFace.North;
				break;
			case BlockFace.West:
				_face = BlockFace.East;
				break;
			case BlockFace.South:
				_face = BlockFace.South;
				break;
			case BlockFace.East:
				_face = BlockFace.West;
				break;
			}
			break;
		case EnumMirrorAlong.YAxis:
			switch (_face)
			{
			case BlockFace.Top:
				_face = BlockFace.Bottom;
				break;
			case BlockFace.Bottom:
				_face = BlockFace.Top;
				break;
			case BlockFace.North:
				_face = BlockFace.North;
				break;
			case BlockFace.West:
				_face = BlockFace.West;
				break;
			case BlockFace.South:
				_face = BlockFace.South;
				break;
			case BlockFace.East:
				_face = BlockFace.East;
				break;
			}
			break;
		case EnumMirrorAlong.ZAxis:
			switch (_face)
			{
			case BlockFace.Top:
				_face = BlockFace.Top;
				break;
			case BlockFace.Bottom:
				_face = BlockFace.Bottom;
				break;
			case BlockFace.North:
				_face = BlockFace.South;
				break;
			case BlockFace.West:
				_face = BlockFace.West;
				break;
			case BlockFace.South:
				_face = BlockFace.North;
				break;
			case BlockFace.East:
				_face = BlockFace.East;
				break;
			}
			break;
		}
		_targetFace = (BlockFace)BlockShapeNew.convertRotationCached[_targetRot, (int)_face];
	}

	public override void CalculateCollisionHash(IncrementalHash calcHash)
	{
		Action<IncrementalHash, BlockShapeNew.MySimpleMesh> action = delegate(IncrementalHash hash, BlockShapeNew.MySimpleMesh mesh)
		{
			foreach (ushort num2 in mesh.Indices)
			{
				hash.AppendDataNoAlloc(num2);
			}
			foreach (Vector2 vector in mesh.Uvs)
			{
				hash.AppendDataNoAlloc(vector.x);
				hash.AppendDataNoAlloc(vector.y);
			}
			foreach (Vector3 vector2 in mesh.Vertices)
			{
				hash.AppendDataNoAlloc(vector2.x);
				hash.AppendDataNoAlloc(vector2.y);
				hash.AppendDataNoAlloc(vector2.z);
			}
			foreach (Vector3 vector3 in mesh.Normals)
			{
				hash.AppendDataNoAlloc(vector3.x);
				hash.AppendDataNoAlloc(vector3.y);
				hash.AppendDataNoAlloc(vector3.z);
			}
		};
		int num = 1;
		calcHash.AppendDataNoAlloc(num);
		if (this.colliderMeshes != null)
		{
			for (int i = 0; i < this.colliderMeshes.Length; i++)
			{
				if (this.colliderMeshes[i] != null)
				{
					action(calcHash, this.colliderMeshes[i]);
				}
			}
		}
		if (this.visualMeshes != null)
		{
			for (int j = 0; j < this.visualMeshes.Length; j++)
			{
				if (this.visualMeshes[j] != null)
				{
					action(calcHash, this.visualMeshes[j]);
				}
			}
		}
	}

	public override int GetVertexCount()
	{
		int num = 0;
		for (int i = 0; i < this.visualMeshes.Length; i++)
		{
			num += ((this.visualMeshes[i] != null) ? this.visualMeshes[i].Vertices.Count : 0);
		}
		return num;
	}

	public override int GetTriangleCount()
	{
		int num = 0;
		for (int i = 0; i < this.visualMeshes.Length; i++)
		{
			num += ((this.visualMeshes[i] != null) ? (this.visualMeshes[i].Indices.Count / 3) : 0);
		}
		return num;
	}

	public override string GetName()
	{
		return this.ShapeName;
	}

	public BlockShapeNew.EnumFaceOcclusionInfo GetFaceInfo(BlockValue _blockValue, BlockFace _face)
	{
		_face = this.GetRotatedBlockFace(_blockValue, _face);
		return this.faceInfo[(int)_face];
	}

	[CompilerGenerated]
	[PublicizedFrom(EAccessModifier.Internal)]
	public static void <renderFace>g__CopyColliderMesh|38_0(VoxelMesh targetMesh, Quaternion q, Vector3 sum1, Vector3 sum2, BlockShapeNew.MySimpleMesh colliderMesh)
	{
		int count = targetMesh.m_CollVertices.Count;
		int num = targetMesh.m_CollVertices.Alloc(colliderMesh.Vertices.Count);
		for (int i = 0; i < colliderMesh.Vertices.Count; i++)
		{
			Vector3 vector = q * (colliderMesh.Vertices[i] + sum1) + sum2;
			targetMesh.m_CollVertices[num + i] = vector;
		}
		num = targetMesh.m_CollIndices.Alloc(colliderMesh.Indices.Count);
		for (int j = 0; j < colliderMesh.Indices.Count; j++)
		{
			targetMesh.m_CollIndices[num + j] = count + (int)colliderMesh.Indices[j];
		}
	}

	[CompilerGenerated]
	[PublicizedFrom(EAccessModifier.Internal)]
	public static bool <GetBlockFaceFromColliderTriangle>g__CheckMeshForTri|55_0(BlockShapeNew.MySimpleMesh mesh, Vector3 _v1, Vector3 _v2, Vector3 _v3)
	{
		if (mesh == null)
		{
			return false;
		}
		for (int i = 0; i < mesh.Indices.Count; i += 3)
		{
			if ((_v1 - mesh.Vertices[(int)mesh.Indices[i]]).sqrMagnitude < 0.001f && (_v2 - mesh.Vertices[(int)mesh.Indices[i + 1]]).sqrMagnitude < 0.001f && (_v3 - mesh.Vertices[(int)mesh.Indices[i + 2]]).sqrMagnitude < 0.001f)
			{
				return true;
			}
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cRotationsUsed = 28;

	public static bool bImposterGenerationActive;

	public string ShapeName;

	[PublicizedFrom(EAccessModifier.Private)]
	public BlockShapeNew.MySimpleMesh[] visualMeshes;

	[PublicizedFrom(EAccessModifier.Private)]
	public BlockShapeNew.MySimpleMesh[] colliderMeshes;

	[PublicizedFrom(EAccessModifier.Private)]
	public BlockShapeNew.EnumFaceOcclusionInfo[] faceInfo;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Vector3 centerOffsetV = new Vector3(-0.5f, -0.5f, -0.5f);

	[PublicizedFrom(EAccessModifier.Private)]
	public Bounds[] boundsRotations;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2[] boundsPathOffsetRotations;

	[PublicizedFrom(EAccessModifier.Private)]
	public static int[,] convertRotationCached;

	[PublicizedFrom(EAccessModifier.Private)]
	public static Dictionary<string, BlockShapeNew.MeshData> meshData = new Dictionary<string, BlockShapeNew.MeshData>();

	[ThreadStatic]
	[PublicizedFrom(EAccessModifier.Private)]
	public static BlockShapeNew.ThreadData threadData;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Quaternion[] rotationsToQuats = new Quaternion[]
	{
		Quaternion.AngleAxis(0f, Vector3.up),
		Quaternion.AngleAxis(90f, Vector3.up),
		Quaternion.AngleAxis(180f, Vector3.up),
		Quaternion.AngleAxis(270f, Vector3.up),
		Quaternion.AngleAxis(180f, Vector3.right) * Quaternion.AngleAxis(180f, Vector3.up),
		Quaternion.AngleAxis(180f, Vector3.right) * Quaternion.AngleAxis(270f, Vector3.up),
		Quaternion.AngleAxis(180f, Vector3.right) * Quaternion.AngleAxis(360f, Vector3.up),
		Quaternion.AngleAxis(180f, Vector3.right) * Quaternion.AngleAxis(450f, Vector3.up),
		Quaternion.AngleAxis(90f, Vector3.right) * Quaternion.AngleAxis(180f, Vector3.up),
		Quaternion.AngleAxis(90f, Vector3.right) * Quaternion.AngleAxis(270f, Vector3.up),
		Quaternion.AngleAxis(90f, Vector3.right) * Quaternion.AngleAxis(360f, Vector3.up),
		Quaternion.AngleAxis(90f, Vector3.right) * Quaternion.AngleAxis(450f, Vector3.up),
		Quaternion.AngleAxis(90f, Vector3.forward) * Quaternion.AngleAxis(0f, Vector3.up),
		Quaternion.AngleAxis(90f, Vector3.forward) * Quaternion.AngleAxis(90f, Vector3.up),
		Quaternion.AngleAxis(90f, Vector3.forward) * Quaternion.AngleAxis(180f, Vector3.up),
		Quaternion.AngleAxis(90f, Vector3.forward) * Quaternion.AngleAxis(270f, Vector3.up),
		Quaternion.AngleAxis(270f, Vector3.right) * Quaternion.AngleAxis(0f, Vector3.up),
		Quaternion.AngleAxis(270f, Vector3.right) * Quaternion.AngleAxis(90f, Vector3.up),
		Quaternion.AngleAxis(270f, Vector3.right) * Quaternion.AngleAxis(180f, Vector3.up),
		Quaternion.AngleAxis(270f, Vector3.right) * Quaternion.AngleAxis(270f, Vector3.up),
		Quaternion.AngleAxis(270f, Vector3.forward) * Quaternion.AngleAxis(0f, Vector3.up),
		Quaternion.AngleAxis(270f, Vector3.forward) * Quaternion.AngleAxis(90f, Vector3.up),
		Quaternion.AngleAxis(270f, Vector3.forward) * Quaternion.AngleAxis(180f, Vector3.up),
		Quaternion.AngleAxis(270f, Vector3.forward) * Quaternion.AngleAxis(270f, Vector3.up),
		Quaternion.AngleAxis(45f, Vector3.up),
		Quaternion.AngleAxis(135f, Vector3.up),
		Quaternion.AngleAxis(225f, Vector3.up),
		Quaternion.AngleAxis(315f, Vector3.up),
		Quaternion.identity,
		Quaternion.identity,
		Quaternion.identity,
		Quaternion.identity
	};

	[PublicizedFrom(EAccessModifier.Private)]
	public static byte[,] rotations = new byte[3, 28];

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cDataVersion = 1;

	public enum EnumFaceOcclusionInfo
	{
		None,
		Part,
		Full,
		Remove,
		Continuous,
		RemoveIfAny,
		OwnFaces,
		HideIfSame,
		Transparent
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public class MySimpleMesh
	{
		public override int GetHashCode()
		{
			int num = 0;
			foreach (ushort num2 in this.Indices)
			{
				num = HashCode.Combine<int, ushort>(num, num2);
			}
			foreach (Vector2 vector in this.Uvs)
			{
				num = HashCode.Combine<int, Vector2>(num, vector);
			}
			foreach (Vector3 vector2 in this.Vertices)
			{
				num = HashCode.Combine<int, Vector3>(num, vector2);
			}
			foreach (Vector3 vector3 in this.Normals)
			{
				num = HashCode.Combine<int, Vector3>(num, vector3);
			}
			return num;
		}

		public readonly List<ushort> Indices = new List<ushort>();

		public readonly List<Vector2> Uvs = new List<Vector2>();

		public readonly List<Vector3> Vertices = new List<Vector3>();

		public readonly List<Vector3> Normals = new List<Vector3>();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public class MeshData
	{
		public GameObject obj;

		public int symTypeOverride = -1;

		public bool IsSolidCube;

		public Dictionary<Vector3, BlockShapeNew.MeshData.Arrays> posArrays = new Dictionary<Vector3, BlockShapeNew.MeshData.Arrays>();

		public class Arrays
		{
			public BlockShapeNew.MySimpleMesh[] meshes = new BlockShapeNew.MySimpleMesh[7];

			public BlockShapeNew.MySimpleMesh[] colliderMeshes = new BlockShapeNew.MySimpleMesh[7];

			public BlockShapeNew.EnumFaceOcclusionInfo[] faceInfo = new BlockShapeNew.EnumFaceOcclusionInfo[7];

			public Bounds[] boundsRotations = new Bounds[32];
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public struct ThreadData
	{
		public void Init()
		{
			this.trisXPos = new List<int>(128);
			this.trisXNeg = new List<int>(128);
			this.trisYPos = new List<int>(128);
			this.trisYNeg = new List<int>(128);
			this.trisZPos = new List<int>(128);
			this.trisZNeg = new List<int>(128);
			this.subsetUsed = new Dictionary<int, int>(256);
			this.subsetUsedList = new List<int>(256);
		}

		public List<int> trisXPos;

		public List<int> trisXNeg;

		public List<int> trisYPos;

		public List<int> trisYNeg;

		public List<int> trisZPos;

		public List<int> trisZNeg;

		public Dictionary<int, int> subsetUsed;

		public List<int> subsetUsedList;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public enum PlaneAxis
	{
		X,
		Y,
		Z
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public struct PlanePick
	{
		public BlockShapeNew.PlaneAxis axis;

		public bool neg;
	}
}
