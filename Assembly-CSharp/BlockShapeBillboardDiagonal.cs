using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class BlockShapeBillboardDiagonal : BlockShapeBillboardAbstract
{
	public override void renderFull(Vector3i _worldPos, BlockValue _blockValue, Vector3 _drawPos, Vector3[] _vertices, LightingAround _lightingAround, TextureFullArray _textureFullArray, VoxelMesh[] _meshes, BlockShape.MeshPurpose _purpose = BlockShape.MeshPurpose.World)
	{
		byte sun = _lightingAround[LightingAround.Pos.Middle].sun;
		byte block = _lightingAround[LightingAround.Pos.Middle].block;
		float num = 1f;
		float num2 = num;
		if (_vertices == null)
		{
			float y = _drawPos.y;
			float y2 = _drawPos.y;
			float y3 = _drawPos.y;
			float y4 = _drawPos.y;
			this.v[0] = new Vector3(_drawPos.x, y, _drawPos.z);
			this.v[1] = new Vector3(_drawPos.x + num, y2, _drawPos.z + num);
			this.v[2] = new Vector3(_drawPos.x + num, y3, _drawPos.z);
			this.v[3] = new Vector3(_drawPos.x, y4, _drawPos.z + num);
			this.v[4] = new Vector3(_drawPos.x, y + num2, _drawPos.z);
			this.v[5] = new Vector3(_drawPos.x + num, y2 + num2, _drawPos.z + num);
			this.v[6] = new Vector3(_drawPos.x + num, y3 + num2, _drawPos.z);
			this.v[7] = new Vector3(_drawPos.x, y4 + num2, _drawPos.z + num);
		}
		else
		{
			float num3 = _drawPos.y - _vertices[0].y;
			this.v[0] = _vertices[0] + new Vector3(0f, num3, 0f);
			this.v[1] = _vertices[7] + new Vector3(0f, num3, 0f);
			this.v[2] = _vertices[3] + new Vector3(0f, num3, 0f);
			this.v[3] = _vertices[4] + new Vector3(0f, num3, 0f);
			this.v[4] = new Vector3(_vertices[0].x, _vertices[0].y + num2 + num3, _vertices[0].z);
			this.v[5] = new Vector3(_vertices[7].x, _vertices[7].y + num2 + num3, _vertices[7].z);
			this.v[6] = new Vector3(_vertices[3].x, _vertices[3].y + num2 + num3, _vertices[3].z);
			this.v[7] = new Vector3(_vertices[4].x, _vertices[4].y + num2 + num3, _vertices[4].z);
		}
		Block block2 = _blockValue.Block;
		VoxelMesh voxelMesh = _meshes[(int)block2.MeshIndex];
		voxelMesh.AddBlockSide(this.v[0], this.v[1], this.v[5], this.v[4], _blockValue, VoxelMesh.COLOR_TOP, BlockFace.Top, sun, block, (int)block2.MeshIndex);
		voxelMesh.AddBlockSide(this.v[1], this.v[0], this.v[4], this.v[5], _blockValue, VoxelMesh.COLOR_TOP, BlockFace.Top, sun, block, (int)block2.MeshIndex);
		voxelMesh.AddBlockSide(this.v[3], this.v[2], this.v[6], this.v[7], _blockValue, VoxelMesh.COLOR_TOP, BlockFace.Top, sun, block, (int)block2.MeshIndex);
		voxelMesh.AddBlockSide(this.v[2], this.v[3], this.v[7], this.v[6], _blockValue, VoxelMesh.COLOR_TOP, BlockFace.Top, sun, block, (int)block2.MeshIndex);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3[] v = new Vector3[8];
}
