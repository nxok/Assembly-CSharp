using System;
using System.Security.Cryptography;
using UnityEngine.Scripting;

[Preserve]
public class BlockShapeBillboardAbstract : BlockShape
{
	public BlockShapeBillboardAbstract()
	{
		this.IsSolidCube = false;
		this.IsSolidSpace = false;
		this.LightOpacity = 0;
		this.IsOmitTerrainSnappingUp = true;
	}

	public override bool IsRenderDecoration()
	{
		return true;
	}

	public override bool isRenderFace(BlockValue _blockValue, BlockFace _face, BlockValue _adjBlockValue)
	{
		return false;
	}

	public override int getFacesDrawnFullBitfield(BlockValue _blockValue)
	{
		return 0;
	}

	public override void CalculateCollisionHash(IncrementalHash calcHash)
	{
	}

	public float yPosSubtract;
}
