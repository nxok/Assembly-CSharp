using System;

public interface IBlockAccess
{
	BlockValue GetBlock(int x, int y, int z);

	BlockValue GetBlock(Vector3i pos)
	{
		return IBlockAccess.DefaultGetBlock(this, pos);
	}

	BlockValue GetBlock(BlockValueRef bvRef)
	{
		return IBlockAccess.DefaultGetBlock(this, bvRef);
	}

	PropValue GetProp(int chunkX, int chunkZ, int propId);

	PropValue GetProp(long chunkKey, int propId);

	PropValue GetProp(Vector2i chunkPos, int propId)
	{
		return IBlockAccess.DefaultGetProp(this, chunkPos, propId);
	}

	PropValue GetProp(PropRef propRef)
	{
		return IBlockAccess.DefaultGetProp(this, propRef);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public static BlockValue DefaultGetBlock(IBlockAccess iba, Vector3i pos)
	{
		return iba.GetBlock(pos.x, pos.y, pos.z);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public static BlockValue DefaultGetBlock(IBlockAccess iba, BlockValueRef bvRef)
	{
		BlockValue blockValue;
		switch (bvRef.Type)
		{
		case BlockValueRefType.None:
			blockValue = BlockValue.Air;
			break;
		case BlockValueRefType.Block:
			blockValue = iba.GetBlock(bvRef.BlockPosition);
			break;
		case BlockValueRefType.Prop:
			blockValue = iba.GetProp(bvRef.PropReference).blockValue;
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
		return blockValue;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public static PropValue DefaultGetProp(IBlockAccess iba, Vector2i chunkPos, int propId)
	{
		return iba.GetProp(chunkPos.x, chunkPos.y, propId);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public static PropValue DefaultGetProp(IBlockAccess iba, PropRef propRef)
	{
		return iba.GetProp(propRef.ChunkPos, propRef.PropId);
	}
}
