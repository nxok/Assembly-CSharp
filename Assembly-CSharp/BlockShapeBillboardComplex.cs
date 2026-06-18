using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class BlockShapeBillboardComplex : BlockShapeBillboardAbstract
{
	public override void renderFull(Vector3i _worldPos, BlockValue _blockValue, Vector3 _drawPos, Vector3[] _vertices, LightingAround _lightingAround, TextureFullArray _textureFullArray, VoxelMesh[] _meshes, BlockShape.MeshPurpose _purpose = BlockShape.MeshPurpose.World)
	{
		byte sun = _lightingAround[LightingAround.Pos.Middle].sun;
		byte block = _lightingAround[LightingAround.Pos.Middle].block;
		this.v[0] = new Vector3(_drawPos.x - 0.2f, _drawPos.y - 0.2f, _drawPos.z - 0.2f);
		this.v[1] = new Vector3(_drawPos.x + 1f + 0.2f, _drawPos.y - 0.2f, _drawPos.z + 1f + 0.2f + 0.01f);
		this.v[2] = new Vector3(_drawPos.x + 1f + 0.2f, _drawPos.y - 0.2f, _drawPos.z - 0.2f);
		this.v[3] = new Vector3(_drawPos.x - 0.2f, _drawPos.y - 0.2f, _drawPos.z + 1f + 0.2f + 0.01f);
		this.v[4] = new Vector3(_drawPos.x - 0.2f, _drawPos.y + 1f + 0.2f, _drawPos.z - 0.2f);
		this.v[5] = new Vector3(_drawPos.x + 1f + 0.2f, _drawPos.y + 1f + 0.2f, _drawPos.z + 1f + 0.2f + 0.01f);
		this.v[6] = new Vector3(_drawPos.x + 1f + 0.2f, _drawPos.y + 1f + 0.2f, _drawPos.z - 0.2f + 0.01f);
		this.v[7] = new Vector3(_drawPos.x - 0.2f, _drawPos.y + 1f + 0.2f, _drawPos.z + 1f + 0.2f);
		this.v[8] = new Vector3(_drawPos.x - 0.2f, _drawPos.y + 0.5f, _drawPos.z - 0.2f);
		this.v[9] = new Vector3(_drawPos.x + 1f + 0.2f, _drawPos.y + 0.5f + 0.01f, _drawPos.z - 0.2f);
		this.v[10] = new Vector3(_drawPos.x + 1f + 0.2f, _drawPos.y + 0.5f + 0.01f, _drawPos.z + 1f + 0.2f);
		this.v[11] = new Vector3(_drawPos.x - 0.2f, _drawPos.y + 0.5f, _drawPos.z + 1f + 0.2f);
		Block block2 = _blockValue.Block;
		VoxelMesh voxelMesh = _meshes[(int)block2.MeshIndex];
		voxelMesh.AddBlockSide(this.v[0], this.v[1], this.v[5], this.v[4], _blockValue, VoxelMesh.COLOR_TOP, BlockFace.Top, sun, block, (int)block2.MeshIndex);
		voxelMesh.AddBlockSide(this.v[1], this.v[0], this.v[4], this.v[5], _blockValue, VoxelMesh.COLOR_TOP, BlockFace.Top, sun, block, (int)block2.MeshIndex);
		voxelMesh.AddBlockSide(this.v[3], this.v[2], this.v[6], this.v[7], _blockValue, VoxelMesh.COLOR_TOP, BlockFace.Top, sun, block, (int)block2.MeshIndex);
		voxelMesh.AddBlockSide(this.v[2], this.v[3], this.v[7], this.v[6], _blockValue, VoxelMesh.COLOR_TOP, BlockFace.Top, sun, block, (int)block2.MeshIndex);
		voxelMesh.AddBlockSide(this.v[8], this.v[9], this.v[10], this.v[11], _blockValue, VoxelMesh.COLOR_TOP, BlockFace.Top, sun, block, (int)block2.MeshIndex);
		voxelMesh.AddBlockSide(this.v[11], this.v[10], this.v[9], this.v[8], _blockValue, VoxelMesh.COLOR_TOP, BlockFace.Top, sun, block, (int)block2.MeshIndex);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3[] v = new Vector3[12];
}
