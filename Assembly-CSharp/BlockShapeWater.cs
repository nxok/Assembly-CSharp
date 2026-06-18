using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class BlockShapeWater : BlockShapeCube
{
	public BlockShapeWater()
	{
		this.IsSolidCube = false;
		this.IsSolidSpace = false;
		this.LightOpacity = 0;
	}

	public override void renderFace(Vector3[] _vertices, LightingAround _lightingAround, long _textureFull, VoxelMesh[] _meshes, Vector2 UVdata, BlockShape.MeshPurpose _purpose = BlockShape.MeshPurpose.World)
	{
		_meshes[1].AddBasicQuad(_vertices, Color.white, UVdata, true, false);
	}

	public override int getFacesDrawnFullBitfield(BlockValue _blockValue)
	{
		return 0;
	}

	public override bool isRenderFace(BlockValue _blockValue, BlockFace _face, BlockValue _adjBlockValue)
	{
		return false;
	}

	public override float GetStepHeight(BlockValue _blockValue, BlockFace crossingFace)
	{
		return 0f;
	}

	public override bool IsMovementBlocked(BlockValue _blockValue, BlockFace crossingFace)
	{
		return false;
	}
}
