using System;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class BlockShapeCube : BlockShape
{
	public BlockShapeCube()
	{
		this.IsSolidCube = true;
		this.IsSolidSpace = true;
		this.IsRotatable = true;
	}

	public override void renderFace(Vector3i _worldPos, BlockValue _blockValue, Vector3 _drawPos, BlockFace _face, Vector3[] _vertices, LightingAround _lightingAround, TextureFullArray _textureFullArray, VoxelMesh[] _meshes, BlockShape.MeshPurpose _purpose = BlockShape.MeshPurpose.World)
	{
		byte meshIndex = _blockValue.Block.MeshIndex;
		Rect uvrectFromSideAndMetadata = this.block.getUVRectFromSideAndMetadata((int)meshIndex, _face, _vertices, _blockValue);
		_meshes[(int)meshIndex].AddQuadWithCracks(_vertices[0], Color.white, _vertices[1], Color.white, _vertices[2], Color.white, _vertices[3], Color.white, uvrectFromSideAndMetadata, WorldConstants.MapDamageToUVRect(_blockValue), false);
		if (_blockValue.hasdecal && _blockValue.decalface == _face)
		{
			UVRectTiling uvrectTiling = MeshDescription.meshes[4].textureAtlas.uvMapping[500 + (int)_blockValue.decaltex];
			Utils.MoveInBlockFaceDirection(_vertices, _face, 0.02f);
			_meshes[4].AddQuadWithCracks(_vertices[0], Color.white, _vertices[1], Color.white, _vertices[2], Color.white, _vertices[3], Color.white, uvrectTiling.uv, WorldConstants.uvRectZero, false);
		}
	}

	public override void renderFull(Vector3i _worldPos, BlockValue _blockValue, Vector3 _drawPos, Vector3[] _vertices, LightingAround _lightingAround, TextureFullArray _textureFullArray, VoxelMesh[] _meshes, BlockShape.MeshPurpose _purpose = BlockShape.MeshPurpose.World)
	{
		byte sun = _lightingAround[LightingAround.Pos.Middle].sun;
		byte block = _lightingAround[LightingAround.Pos.Middle].block;
		byte meshIndex = _blockValue.Block.MeshIndex;
		VoxelMesh voxelMesh = _meshes[(int)meshIndex];
		if (_vertices == null)
		{
			this.v[0] = new Vector3(_drawPos.x, _drawPos.y, _drawPos.z);
			this.v[1] = new Vector3(_drawPos.x, _drawPos.y + 1f, _drawPos.z);
			this.v[2] = new Vector3(_drawPos.x + 1f, _drawPos.y + 1f, _drawPos.z);
			this.v[3] = new Vector3(_drawPos.x + 1f, _drawPos.y, _drawPos.z);
			this.v[4] = new Vector3(_drawPos.x, _drawPos.y, _drawPos.z + 1f);
			this.v[5] = new Vector3(_drawPos.x, _drawPos.y + 1f, _drawPos.z + 1f);
			this.v[6] = new Vector3(_drawPos.x + 1f, _drawPos.y + 1f, _drawPos.z + 1f);
			this.v[7] = new Vector3(_drawPos.x + 1f, _drawPos.y, _drawPos.z + 1f);
		}
		else
		{
			this.v[0] = new Vector3(_drawPos.x, _drawPos.y, _drawPos.z) + _vertices[0];
			this.v[1] = new Vector3(_drawPos.x, _drawPos.y + 1f, _drawPos.z) + _vertices[1];
			this.v[2] = new Vector3(_drawPos.x + 1f, _drawPos.y + 1f, _drawPos.z) + _vertices[2];
			this.v[3] = new Vector3(_drawPos.x + 1f, _drawPos.y, _drawPos.z) + _vertices[3];
			this.v[4] = new Vector3(_drawPos.x, _drawPos.y, _drawPos.z + 1f) + _vertices[4];
			this.v[5] = new Vector3(_drawPos.x, _drawPos.y + 1f, _drawPos.z + 1f) + _vertices[5];
			this.v[6] = new Vector3(_drawPos.x + 1f, _drawPos.y + 1f, _drawPos.z + 1f) + _vertices[6];
			this.v[7] = new Vector3(_drawPos.x + 1f, _drawPos.y, _drawPos.z + 1f) + _vertices[7];
		}
		voxelMesh.AddBlockSide(this.v[0], this.v[3], this.v[2], this.v[1], _blockValue, VoxelMesh.COLOR_SOUTH, BlockFace.South, sun, block, (int)meshIndex);
		voxelMesh.AddBlockSide(this.v[7], this.v[4], this.v[5], this.v[6], _blockValue, VoxelMesh.COLOR_NORTH, BlockFace.North, sun, block, (int)meshIndex);
		voxelMesh.AddBlockSide(this.v[4], this.v[0], this.v[1], this.v[5], _blockValue, VoxelMesh.COLOR_WEST, BlockFace.West, sun, block, (int)meshIndex);
		voxelMesh.AddBlockSide(this.v[3], this.v[7], this.v[6], this.v[2], _blockValue, VoxelMesh.COLOR_EAST, BlockFace.East, sun, block, (int)meshIndex);
		voxelMesh.AddBlockSide(this.v[1], this.v[2], this.v[6], this.v[5], _blockValue, VoxelMesh.COLOR_TOP, BlockFace.Top, sun, block, (int)meshIndex);
		voxelMesh.AddBlockSide(this.v[4], this.v[7], this.v[3], this.v[0], _blockValue, VoxelMesh.COLOR_BOTTOM, BlockFace.Bottom, sun, block, (int)meshIndex);
	}

	public override int MapSideAndRotationToTextureIdx(BlockValue _blockValue, BlockFace _side)
	{
		if (_side == BlockFace.Bottom || _side == BlockFace.Top)
		{
			return (int)_side;
		}
		return (int)(((_side - BlockFace.North + _blockValue.rotation) & 3) + 2);
	}

	public override byte Rotate(bool _bLeft, int _rotation)
	{
		_rotation += (_bLeft ? (-1) : 1);
		if (_rotation > 3)
		{
			_rotation = 0;
		}
		if (_rotation < 0)
		{
			_rotation = 3;
		}
		return (byte)_rotation;
	}

	public override BlockValue RotateY(bool _bLeft, BlockValue _blockValue, int _rotCount)
	{
		for (int i = 0; i < _rotCount; i++)
		{
			byte b = _blockValue.rotation;
			if (b <= 3)
			{
				if (_bLeft)
				{
					b = ((b > 0) ? (b - 1) : 3);
				}
				else
				{
					b = ((b < 3) ? (b + 1) : 0);
				}
			}
			else if (b <= 7)
			{
				if (_bLeft)
				{
					b = ((b > 4) ? (b - 1) : 7);
				}
				else
				{
					b = ((b < 7) ? (b + 1) : 4);
				}
			}
			else if (b <= 11)
			{
				if (_bLeft)
				{
					b = ((b > 8) ? (b - 1) : 11);
				}
				else
				{
					b = ((b < 11) ? (b + 1) : 8);
				}
			}
			_blockValue.rotation = b;
		}
		return _blockValue;
	}

	public override void CalculateCollisionHash(IncrementalHash calcHash)
	{
		int num = 1;
		calcHash.AppendDataNoAlloc(num);
		calcHash.AppendDataNoAlloc(this.IsSolidCube);
	}

	public static readonly Vector3[] Cube = new Vector3[]
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
	public readonly Vector3[] v = new Vector3[8];
}
