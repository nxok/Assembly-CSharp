using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class BlockShapeGrassShort : BlockShapeGrass
{
	public override void renderFull(Vector3i _worldPos, BlockValue _blockValue, Vector3 _drawPos, Vector3[] _vertices, LightingAround _lightingAround, TextureFullArray _textureFullArray, VoxelMesh[] _meshes, BlockShape.MeshPurpose _purpose = BlockShape.MeshPurpose.World)
	{
		Vector3 vector;
		vector.x = _drawPos.x + (float)((int)_drawPos.z & 1) * 0.2f - 0.1f;
		vector.y = _drawPos.y;
		vector.z = _drawPos.z + (float)((int)_drawPos.x & 1) * 0.2f - 0.1f;
		byte meta2and = _blockValue.meta2and1;
		BlockShapeBillboardPlant.RenderData renderData;
		renderData.offsetY = -0.09f;
		renderData.scale = 1f;
		renderData.height = 0.33f;
		switch ((meta2and >> 3) & 3)
		{
		case 0:
			renderData.count = 1 + MeshDescription.GrassQualityPlanes * 2;
			renderData.count2 = 2;
			renderData.sideShift = 0.22f;
			break;
		case 1:
			renderData.count = 2 + MeshDescription.GrassQualityPlanes * 2;
			renderData.count2 = 2;
			renderData.sideShift = 0.26f;
			break;
		case 2:
			renderData.count = 1 + MeshDescription.GrassQualityPlanes;
			renderData.count2 = 3;
			renderData.sideShift = 0.18f;
			break;
		default:
			renderData.count = 2 + MeshDescription.GrassQualityPlanes;
			renderData.count2 = 3;
			renderData.sideShift = 0.3f;
			break;
		}
		renderData.rotation = 10f + (float)(_blockValue.rotation & 7) * 22.5f;
		Block block = _blockValue.Block;
		VoxelMesh voxelMesh = _meshes[(int)block.MeshIndex];
		int num = (int)(_blockValue.meta & 7);
		if (num >= 6)
		{
			num = 0;
		}
		BlockFace blockFace = (BlockFace)num;
		Rect uvrectFromSideAndMetadata = block.getUVRectFromSideAndMetadata((int)block.MeshIndex, blockFace, Vector3.zero, _blockValue);
		uvrectFromSideAndMetadata.height *= 0.33f;
		byte sun = _lightingAround[LightingAround.Pos.Middle].sun;
		byte block2 = _lightingAround[LightingAround.Pos.Middle].block;
		BlockShapeBillboardPlant.RenderGridMesh(voxelMesh, vector, _vertices, uvrectFromSideAndMetadata, sun, block2, renderData);
		BlockShapeBillboardPlant.AddCollider(voxelMesh, _drawPos, 0.5f);
	}
}
