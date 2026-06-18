using System;
using System.Security.Cryptography;
using UnityEngine.Scripting;

[Preserve]
public class BlockShapeInvisible : BlockShape
{
	public BlockShapeInvisible()
	{
		this.IsSolidCube = false;
		this.IsSolidSpace = false;
		this.LightOpacity = 0;
	}

	public override void Init(Block _block)
	{
		base.Init(_block);
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
}
