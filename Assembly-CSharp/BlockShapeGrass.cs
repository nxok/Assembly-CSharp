using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class BlockShapeGrass : BlockShapeBillboardAbstract
{
	public BlockShapeGrass()
	{
		this.LightOpacity = 0;
		this.IsOmitTerrainSnappingUp = true;
	}

	public override void Init(Block _block)
	{
		base.Init(_block);
		_block.IsDecoration = true;
	}

	public override void renderFull(Vector3i _worldPos, BlockValue _blockValue, Vector3 _drawPos, Vector3[] _vertices, LightingAround _lightingAround, TextureFullArray _textureFullArray, VoxelMesh[] _meshes, BlockShape.MeshPurpose _purpose = BlockShape.MeshPurpose.World)
	{
		Vector3 vector;
		vector.x = _drawPos.x + (float)((int)_drawPos.z & 1) * 0.2f - 0.1f;
		vector.y = _drawPos.y;
		vector.z = _drawPos.z + (float)((int)_drawPos.x & 1) * 0.2f - 0.1f;
		byte meta2and = _blockValue.meta2and1;
		BlockShapeBillboardPlant.RenderData renderData;
		renderData.offsetY = -0.15f;
		renderData.scale = 0.93f + (float)(meta2and >> 6) * 0.07f;
		renderData.height = renderData.scale;
		switch ((meta2and >> 3) & 3)
		{
		case 0:
			renderData.count = 2 + MeshDescription.GrassQualityPlanes;
			renderData.scale *= 0.8f;
			renderData.sideShift = 0.045f;
			break;
		case 1:
			renderData.count = 2 + MeshDescription.GrassQualityPlanes;
			renderData.sideShift = 0.075f;
			break;
		case 2:
			renderData.count = 3 + MeshDescription.GrassQualityPlanes;
			renderData.sideShift = 0.09f;
			break;
		default:
			renderData.count = 3 + MeshDescription.GrassQualityPlanes * 2;
			renderData.sideShift = 0.2f;
			break;
		}
		renderData.count2 = 0;
		renderData.sideShift *= renderData.scale;
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
		byte sun = _lightingAround[LightingAround.Pos.Middle].sun;
		byte block2 = _lightingAround[LightingAround.Pos.Middle].block;
		BlockShapeBillboardPlant.RenderSpinMesh(voxelMesh, vector, _vertices, uvrectFromSideAndMetadata, sun, block2, renderData);
		BlockShapeBillboardPlant.AddCollider(voxelMesh, _drawPos, 0.85f);
	}

	public override bool IsMovementBlocked(BlockValue _blockValue, BlockFace crossingFace)
	{
		return false;
	}

	public override float GetStepHeight(BlockValue _blockValue, BlockFace crossingFace)
	{
		return 0f;
	}
}
