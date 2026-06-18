using System;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class BlockShapeBillboardPlant : BlockShapeBillboardRotatedAbstract
{
	public BlockShapeBillboardPlant()
	{
		this.boundsArr = new Bounds[] { BoundsUtils.BoundsForMinMax(0.3f, 0f, 0.3f, 0.7f, 1f, 0.7f) };
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void createVertices()
	{
		this.vertices = new Vector3[]
		{
			new Vector3(-0.35f, -0.15f, -0.35f),
			new Vector3(-0.34f, 1.15f, -0.35f),
			new Vector3(1.36f, 1.15f, -0.35f),
			new Vector3(1.35f, -0.15f, -0.35f),
			new Vector3(-0.35f, -0.15f, 1.35f),
			new Vector3(-0.34f, 1.15f, 1.35f),
			new Vector3(1.36f, 1.15f, 1.35f),
			new Vector3(1.35f, -0.15f, 1.35f)
		};
	}

	public override Quaternion GetRotation(BlockValue _blockValue)
	{
		return Quaternion.AngleAxis((float)(20 * _blockValue.rotation), Vector3.up);
	}

	public override void renderFull(Vector3i _worldPos, BlockValue _blockValue, Vector3 _drawPos, Vector3[] _vertices, LightingAround _lightingAround, TextureFullArray _textureFullArray, VoxelMesh[] _meshes, BlockShape.MeshPurpose _purpose = BlockShape.MeshPurpose.World)
	{
		byte sun = _lightingAround[LightingAround.Pos.Middle].sun;
		byte block = _lightingAround[LightingAround.Pos.Middle].block;
		Block block2 = _blockValue.Block;
		int meshIndex = (int)block2.MeshIndex;
		VoxelMesh voxelMesh = _meshes[meshIndex];
		if (meshIndex == 3)
		{
			Rect uvrectFromSideAndMetadata = block2.getUVRectFromSideAndMetadata(meshIndex, BlockFace.Top, Vector3.zero, _blockValue);
			BlockShapeBillboardPlant.RenderData renderData;
			renderData.count = 2 + MeshDescription.GrassQualityPlanes;
			renderData.count2 = 0;
			renderData.offsetY = -0.05f;
			renderData.scale = 1.25f;
			renderData.height = renderData.scale;
			renderData.sideShift = 0.04f;
			renderData.rotation = (float)(20 * _blockValue.rotation);
			BlockShapeBillboardPlant.RenderSpinMesh(voxelMesh, _drawPos, _vertices, uvrectFromSideAndMetadata, sun, block, renderData);
			BlockShapeBillboardPlant.AddCollider(voxelMesh, _drawPos, 0.85f);
			return;
		}
		Vector3[] array = base.rotateVertices(this.vertices, _drawPos, _blockValue);
		voxelMesh.AddBlockSide(array[7], array[0], array[1], array[6], _blockValue, VoxelMesh.COLOR_TOP, BlockFace.Top, sun, block, meshIndex);
		voxelMesh.AddBlockSide(array[0], array[7], array[6], array[1], _blockValue, VoxelMesh.COLOR_TOP, BlockFace.Top, sun, block, meshIndex);
		voxelMesh.AddBlockSide(array[3], array[4], array[5], array[2], _blockValue, VoxelMesh.COLOR_TOP, BlockFace.Top, sun, block, meshIndex);
		voxelMesh.AddBlockSide(array[4], array[3], array[2], array[5], _blockValue, VoxelMesh.COLOR_TOP, BlockFace.Top, sun, block, meshIndex);
		MemoryPools.poolVector3.Free(array);
	}

	public override Bounds[] GetBounds(BlockValue _blockValue)
	{
		return this.boundsArr;
	}

	public override byte Rotate(bool _isLeft, int _rotation)
	{
		_rotation = (_rotation + (_isLeft ? (-1) : 1)) & 3;
		return (byte)_rotation;
	}

	public override BlockValue RotateY(bool _bLeft, BlockValue _blockValue, int _rotCount)
	{
		_blockValue.rotation = (byte)(((int)_blockValue.rotation + _rotCount) & 3);
		return _blockValue;
	}

	public static void RenderSpinMesh(VoxelMesh _mesh, Vector3 _drawPos, Vector3[] _vertices, Rect uvTex, byte _sunlight, byte _blocklight, BlockShapeBillboardPlant.RenderData _data)
	{
		float num = _drawPos.y;
		if (_vertices != null)
		{
			num += _data.offsetY;
		}
		float num2 = num + _data.height;
		float num3 = _drawPos.x + 0.5f;
		float num4 = _drawPos.z + 0.5f;
		float num5 = 180f / (float)_data.count;
		num5 += 180f;
		float num6 = 0.5f * _data.scale;
		for (int i = 0; i < _data.count; i++)
		{
			float num7 = ((float)i * num5 + _data.rotation) * 0.017453292f;
			float num8 = Mathf.Sin(num7);
			float num9 = Mathf.Cos(num7);
			float num10 = -num6;
			float sideShift = _data.sideShift;
			Vector3 vector;
			vector.x = num3 + num10 * num9 - sideShift * num8;
			vector.y = num;
			vector.z = num4 + num10 * num8 + sideShift * num9;
			num10 = num6;
			Vector3 vector2;
			vector2.x = num3 + num10 * num9 - sideShift * num8;
			vector2.y = num;
			vector2.z = num4 + num10 * num8 + sideShift * num9;
			float num11 = num2;
			num11 += (float)(i % 3) * -0.15f;
			Vector3 vector3;
			vector3.x = vector2.x;
			vector3.y = num11;
			vector3.z = vector2.z;
			Vector3 vector4;
			vector4.x = vector.x;
			vector4.y = num11;
			vector4.z = vector.z;
			Vector3 vector5;
			vector5.x = num8;
			vector5.y = 0f;
			vector5.z = -num9;
			Vector4 vector6 = (vector2 - vector).normalized;
			vector6.w = -1f;
			Vector3 vector7 = -vector5;
			_mesh.AddRectangle(vector, BlockShape.uvZero, vector2, BlockShape.uvRightBot, vector3, BlockShape.uvOne, vector4, BlockShape.uvLeftTop, vector5, vector5, vector6, uvTex, _sunlight, _blocklight);
			vector6.w = 1f;
			_mesh.AddRectangle(vector, BlockShape.uvZero, vector4, BlockShape.uvLeftTop, vector3, BlockShape.uvOne, vector2, BlockShape.uvRightBot, vector7, vector7, vector6, uvTex, _sunlight, _blocklight);
		}
	}

	public static void RenderGridMesh(VoxelMesh _mesh, Vector3 _drawPos, Vector3[] _vertices, Rect uvTex, byte _sunlight, byte _blocklight, BlockShapeBillboardPlant.RenderData _data)
	{
		float num = _drawPos.y;
		if (_vertices != null)
		{
			num += _data.offsetY;
		}
		float num2 = num + _data.height - 0.12f;
		float num3 = _drawPos.x + 0.5f;
		float num4 = _drawPos.z + 0.5f;
		float num5 = _data.rotation;
		float num6 = 165f / (float)_data.count2;
		float num7 = _data.sideShift * 2f / ((float)_data.count - 0.99f);
		float num8 = _data.sideShift * 1.85f;
		Vector3 vector;
		vector.y = 0f;
		Vector3 vector2;
		vector2.y = 0f;
		for (int i = 0; i < _data.count2; i++)
		{
			float num9 = -_data.sideShift;
			float num10 = num5 * 0.017453292f;
			float num11 = Mathf.Sin(num10);
			float num12 = Mathf.Cos(num10);
			vector.x = -num12;
			vector.z = -num11;
			for (int j = 0; j < _data.count; j++)
			{
				float num13 = num3 - num8 * num11;
				float num14 = num4 + num8 * num12;
				Vector3 vector3;
				vector3.x = num13 + num9 * num12;
				vector3.y = num;
				vector3.z = num14 + num9 * num11;
				float num15 = num2;
				num15 += (float)(j % 3) * 0.06f;
				float num16 = num9 * 0.7f;
				Vector3 vector4;
				vector4.x = num13 + num16 * num12;
				vector4.y = num15;
				vector4.z = num14 + num16 * num11;
				num13 = num3 + num8 * num11;
				num14 = num4 - num8 * num12;
				Vector3 vector5;
				vector5.x = num13 + num9 * num12;
				vector5.y = num;
				vector5.z = num14 + num9 * num11;
				Vector3 vector6;
				vector6.x = num13 + num16 * num12;
				vector6.y = num15;
				vector6.z = num14 + num16 * num11;
				Vector3 vector7;
				vector7.x = vector.x;
				vector7.y = (num16 - num9) * 2.2f;
				vector7.z = vector.z;
				vector7 *= 1f / vector7.magnitude;
				vector2.x = -vector.x;
				vector2.z = -vector.z;
				Vector4 vector8 = (vector5 - vector3).normalized;
				vector8.w = -1f;
				if ((j & 1) == 0)
				{
					_mesh.AddRectangle(vector3, BlockShape.uvZero, vector5, BlockShape.uvRightBot, vector6, BlockShape.uvOne, vector4, BlockShape.uvLeftTop, vector, vector7, vector8, uvTex, _sunlight, _blocklight);
					vector8.w = 1f;
					_mesh.AddRectangle(vector5, BlockShape.uvRightBot, vector3, BlockShape.uvZero, vector4, BlockShape.uvLeftTop, vector6, BlockShape.uvOne, vector2, -vector7, vector8, uvTex, _sunlight, _blocklight);
				}
				else
				{
					_mesh.AddRectangle(vector3, BlockShape.uvRightBot, vector5, BlockShape.uvZero, vector6, BlockShape.uvLeftTop, vector4, BlockShape.uvOne, vector, vector7, vector8, uvTex, _sunlight, _blocklight);
					vector8.w = 1f;
					_mesh.AddRectangle(vector5, BlockShape.uvZero, vector3, BlockShape.uvRightBot, vector4, BlockShape.uvOne, vector6, BlockShape.uvLeftTop, vector2, -vector7, vector8, uvTex, _sunlight, _blocklight);
				}
				num9 += num7;
			}
			num5 += num6;
		}
	}

	public static void AddCollider(VoxelMesh _mesh, Vector3 _drawPos, float _height)
	{
		float num = _drawPos.x + 0.5f;
		float num2 = _drawPos.z + 0.5f;
		float y = _drawPos.y;
		float num3 = y + _height;
		Vector3 vector;
		vector.x = num - 0.45f;
		vector.y = y;
		vector.z = num2;
		Vector3 vector2;
		vector2.x = num + 0.45f;
		vector2.y = y;
		vector2.z = num2;
		Vector3 vector3;
		vector3.x = vector2.x;
		vector3.y = num3;
		vector3.z = num2;
		Vector3 vector4;
		vector4.x = vector.x;
		vector4.y = num3;
		vector4.z = num2;
		_mesh.AddRectangleColliderPair(vector, vector2, vector3, vector4);
		vector.x = num;
		vector.z = num2 - 0.45f;
		vector2.x = num;
		vector2.z = num2 + 0.45f;
		vector3.x = num;
		vector3.z = vector2.z;
		vector4.x = num;
		vector4.z = vector.z;
		_mesh.AddRectangleColliderPair(vector, vector2, vector3, vector4);
	}

	public override void CalculateCollisionHash(IncrementalHash calcHash)
	{
		int num = 1;
		calcHash.AppendDataNoAlloc(num);
		if (this.vertices != null)
		{
			foreach (Vector3 vector in this.vertices)
			{
				calcHash.AppendDataNoAlloc(vector.x);
				calcHash.AppendDataNoAlloc(vector.y);
				calcHash.AppendDataNoAlloc(vector.z);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const float xzAdd = 0.35f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float yAdd = 0.15f;

	public struct RenderData
	{
		public int count;

		public int count2;

		public float height;

		public float rotation;

		public float scale;

		public float sideShift;

		public float offsetY;
	}
}
