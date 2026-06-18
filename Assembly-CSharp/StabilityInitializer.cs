using System;
using System.Collections.Generic;

public class StabilityInitializer
{
	public StabilityInitializer(WorldBase _world)
	{
		this.world = _world;
	}

	public void DistributeStability(Chunk _chunk)
	{
		int maxHeight = (int)_chunk.GetMaxHeight();
		for (int i = 0; i < 16; i++)
		{
			for (int j = 0; j < 16; j++)
			{
				for (int k = 0; k <= maxHeight; k++)
				{
					int blockId = _chunk.GetBlockId(j, k, i);
					if (blockId != 0)
					{
						Block block = Block.list[blockId];
						if (!block.blockMaterial.IsLiquid && !block.StabilityIgnore)
						{
							byte stability = _chunk.GetStability(j, k, i);
							if (stability > 1)
							{
								this.spreadHorizontal(_chunk, j, k, i, (int)stability);
							}
						}
					}
				}
			}
		}
		_chunk.StopStabilityCalculation = false;
	}

	public void BlockRemovedAt(int _worldX, int _worldY, int _worldZ)
	{
		Chunk chunk = (Chunk)this.world.GetChunkFromWorldPos(_worldX, _worldY, _worldZ);
		if (chunk != null)
		{
			int num = World.toBlockXZ(_worldX);
			int num2 = World.toBlockXZ(_worldZ);
			byte b = chunk.GetStability(num, _worldY, num2);
			chunk.SetStability(num, _worldY, num2, 0);
			HashSet<Vector3i> hashSet = new HashSet<Vector3i>();
			this.unspreadHorizontal(chunk, num, _worldY, num2, (int)b, hashSet);
			this.unspreadVertical(chunk, num, _worldY, num2, (int)b, hashSet);
			foreach (Vector3i vector3i in hashSet)
			{
				chunk = (Chunk)this.world.GetChunkFromWorldPos(vector3i.x, vector3i.y, vector3i.z);
				if (chunk != null)
				{
					num = World.toBlockXZ(vector3i.x);
					num2 = World.toBlockXZ(vector3i.z);
					b = chunk.GetStability(num, vector3i.y, num2);
					this.spreadHorizontal(chunk, num, vector3i.y, num2, (int)b);
					this.spreadVertical(chunk, num, vector3i.y, num2, (int)b);
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void unspreadVertical(Chunk _chunk, int _blockX, int _y, int _blockZ, int _stab, HashSet<Vector3i> _list)
	{
		for (int i = _y + 1; i < 256; i++)
		{
			BlockValue blockNoDamage = _chunk.GetBlockNoDamage(_blockX, i, _blockZ);
			Block block = blockNoDamage.Block;
			if (blockNoDamage.isair || !block.StabilitySupport || block.blockMaterial.IsLiquid || block.StabilityIgnore)
			{
				break;
			}
			byte stability = _chunk.GetStability(_blockX, i, _blockZ);
			if (stability == 0)
			{
				break;
			}
			if ((int)stability > _stab)
			{
				_list.Add(new Vector3i(_chunk.GetBlockWorldPosX(_blockX), i, _chunk.GetBlockWorldPosZ(_blockZ)));
				break;
			}
			_chunk.SetStability(_blockX, i, _blockZ, 0);
			this.unspreadHorizontal(_chunk, _blockX, i, _blockZ, _stab, _list);
		}
		int num = _stab - 1;
		int num2 = _y - 1;
		while (num2 > 0 && num > 0)
		{
			BlockValue blockNoDamage2 = _chunk.GetBlockNoDamage(_blockX, num2, _blockZ);
			Block block2 = blockNoDamage2.Block;
			if (blockNoDamage2.isair || !block2.StabilitySupport || block2.blockMaterial.IsLiquid || block2.StabilityIgnore)
			{
				break;
			}
			byte stability2 = _chunk.GetStability(_blockX, num2, _blockZ);
			if (stability2 == 0)
			{
				break;
			}
			if ((int)stability2 > num)
			{
				_list.Add(new Vector3i(_chunk.GetBlockWorldPosX(_blockX), num2, _chunk.GetBlockWorldPosZ(_blockZ)));
				return;
			}
			_chunk.SetStability(_blockX, num2, _blockZ, 0);
			this.unspreadHorizontal(_chunk, _blockX, num2, _blockZ, num, _list);
			num--;
			num2--;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void unspreadHorizontal(Chunk _chunk, int _blockX, int _y, int _blockZ, int _stab, HashSet<Vector3i> _list)
	{
		if (_stab <= 0)
		{
			return;
		}
		_stab--;
		for (int i = 0; i < Vector3i.HORIZONTAL_DIRECTIONS.Length; i++)
		{
			int num = _blockX + Vector3i.HORIZONTAL_DIRECTIONS[i].x;
			int num2 = _blockZ + Vector3i.HORIZONTAL_DIRECTIONS[i].z;
			Chunk chunk = _chunk;
			if (num < 0 || num >= 16 || num2 < 0 || num2 >= 16)
			{
				chunk = (Chunk)this.world.GetChunkFromWorldPos(_chunk.GetBlockWorldPosX(num), _y, _chunk.GetBlockWorldPosZ(num2));
				num = World.toBlockXZ(num);
				num2 = World.toBlockXZ(num2);
			}
			if (chunk != null)
			{
				BlockValue blockNoDamage = chunk.GetBlockNoDamage(num, _y, num2);
				Block block = blockNoDamage.Block;
				if (!blockNoDamage.isair && block.StabilitySupport && !block.blockMaterial.IsLiquid && !block.StabilityIgnore)
				{
					byte stability = chunk.GetStability(num, _y, num2);
					if (stability != 0)
					{
						if ((int)stability > _stab)
						{
							_list.Add(new Vector3i(chunk.GetBlockWorldPosX(num), _y, chunk.GetBlockWorldPosZ(num2)));
						}
						else
						{
							chunk.SetStability(num, _y, num2, 0);
							this.unspreadHorizontal(chunk, num, _y, num2, _stab, _list);
							this.unspreadVertical(chunk, num, _y, num2, _stab, _list);
						}
					}
				}
			}
		}
	}

	public void BlockPlacedAt(int _worldX, int _worldY, int _worldZ, BlockValue _blockValue)
	{
		int num = 0;
		for (int i = 0; i < Vector3i.HORIZONTAL_DIRECTIONS.Length; i++)
		{
			num = Math.Max((int)this.world.GetStability(_worldX + Vector3i.HORIZONTAL_DIRECTIONS[i].x, _worldY, _worldZ + Vector3i.HORIZONTAL_DIRECTIONS[i].z), num);
		}
		num--;
		int num2 = (int)this.world.GetStability(_worldX, _worldY - 1, _worldZ);
		int num3 = (int)this.world.GetStability(_worldX, _worldY + 1, _worldZ);
		Chunk chunk = (Chunk)this.world.GetChunkFromWorldPos(_worldX, _worldY, _worldZ);
		if (chunk != null)
		{
			int num4 = World.toBlockXZ(_worldX);
			int num5 = World.toBlockXZ(_worldZ);
			if (15 == num2)
			{
				num2 = 15;
			}
			else
			{
				num2--;
			}
			num3--;
			int num6 = Math.Max(num3, num2);
			int num7 = Math.Max(Math.Min(Math.Max(num, num6), 15), 0);
			chunk.SetStability(num4, _worldY, num5, (byte)num7);
			if (num7 > 0)
			{
				this.spreadHorizontal(chunk, num4, _worldY, num5, num7);
				this.spreadVertical(chunk, num4, _worldY, num5, num7);
				return;
			}
			Log.Out("Unbalanced");
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void spreadVertical(Chunk _chunk, int _blockX, int _y, int _blockZ, int _stab)
	{
		for (int i = _y + 1; i < 256; i++)
		{
			int blockId = _chunk.GetBlockId(_blockX, i, _blockZ);
			if (blockId == 0)
			{
				break;
			}
			Block block = Block.list[blockId];
			if (block.blockMaterial.IsLiquid || block.StabilityIgnore)
			{
				break;
			}
			int num = Math.Min(_stab, 15);
			if (num > 1 && !block.StabilitySupport)
			{
				num = 1;
			}
			byte stability = _chunk.GetStability(_blockX, i, _blockZ);
			if (num <= (int)stability)
			{
				break;
			}
			_chunk.SetStability(_blockX, i, _blockZ, (byte)num);
			if (block.StabilitySupport)
			{
				this.spreadHorizontal(_chunk, _blockX, i, _blockZ, num);
			}
		}
		int num2 = _stab - 1;
		int num3 = _y - 1;
		while (num3 > 0 && num2 > 0)
		{
			int blockId2 = _chunk.GetBlockId(_blockX, num3, _blockZ);
			if (blockId2 == 0)
			{
				break;
			}
			Block block2 = Block.list[blockId2];
			if (block2.blockMaterial.IsLiquid || block2.StabilityIgnore)
			{
				break;
			}
			int num4 = Math.Min(num2, 15);
			if (num4 > 1 && !block2.StabilitySupport)
			{
				num4 = 1;
			}
			byte stability2 = _chunk.GetStability(_blockX, num3, _blockZ);
			if (num4 <= (int)stability2)
			{
				break;
			}
			_chunk.SetStability(_blockX, num3, _blockZ, (byte)num4);
			if (block2.StabilitySupport)
			{
				this.spreadHorizontal(_chunk, _blockX, num3, _blockZ, num4);
			}
			num2--;
			num3--;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void spreadHorizontal(Chunk _chunk, int _blockX, int _y, int _blockZ, int _stab)
	{
		if (_stab <= 1)
		{
			return;
		}
		_stab--;
		int i = Vector3i.HORIZONTAL_DIRECTIONS.Length - 1;
		while (i >= 0)
		{
			int num = _blockX + Vector3i.HORIZONTAL_DIRECTIONS[i].x;
			int num2 = _blockZ + Vector3i.HORIZONTAL_DIRECTIONS[i].z;
			Chunk chunk = _chunk;
			if (num < 16 && num2 < 16)
			{
				goto IL_0082;
			}
			chunk = (Chunk)this.world.GetChunkFromWorldPos(_chunk.GetBlockWorldPosX(num), _y, _chunk.GetBlockWorldPosZ(num2));
			if (chunk != null)
			{
				num = World.toBlockXZ(num);
				num2 = World.toBlockXZ(num2);
				goto IL_0082;
			}
			IL_0105:
			i--;
			continue;
			IL_0082:
			int blockId = chunk.GetBlockId(num, _y, num2);
			if (blockId == 0)
			{
				goto IL_0105;
			}
			Block block = Block.list[blockId];
			if (block.blockMaterial.IsLiquid || block.StabilityIgnore)
			{
				goto IL_0105;
			}
			int num3 = _stab;
			if (num3 > 1 && !block.StabilitySupport)
			{
				num3 = 1;
			}
			byte stability = chunk.GetStability(num, _y, num2);
			if (num3 <= (int)stability)
			{
				goto IL_0105;
			}
			chunk.SetStability(num, _y, num2, (byte)num3);
			if (block.StabilitySupport)
			{
				this.spreadHorizontal(chunk, num, _y, num2, num3);
				this.spreadVertical(chunk, num, _y, num2, num3);
				goto IL_0105;
			}
			goto IL_0105;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void clearDown(Chunk _chunk, int _blockX, int _y, int _blockZ, int _stabStop)
	{
		for (int i = _y - 1; i > 0; i--)
		{
			BlockValue blockNoDamage = _chunk.GetBlockNoDamage(_blockX, i, _blockZ);
			Block block = blockNoDamage.Block;
			if (blockNoDamage.isair || !block.StabilitySupport || block.blockMaterial.IsLiquid || block.StabilityIgnore)
			{
				break;
			}
			byte stability = _chunk.GetStability(_blockX, i, _blockZ);
			if (stability == 0 || (int)stability >= _stabStop)
			{
				break;
			}
			_chunk.SetStability(_blockX, i, _blockZ, 0);
			this.clearHorizontal(_chunk, _blockX, i, _blockZ, _stabStop);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void clearHorizontal(Chunk _chunk, int _blockX, int _y, int _blockZ, int _stabStop)
	{
		for (int i = 0; i < Vector3i.HORIZONTAL_DIRECTIONS.Length; i++)
		{
			int num = _blockX + Vector3i.HORIZONTAL_DIRECTIONS[i].x;
			int num2 = _blockZ + Vector3i.HORIZONTAL_DIRECTIONS[i].z;
			Chunk chunk = _chunk;
			if (num < 0 || num >= 16 || num2 < 0 || num2 >= 16)
			{
				chunk = (Chunk)this.world.GetChunkFromWorldPos(_chunk.GetBlockWorldPosX(num), _y, _chunk.GetBlockWorldPosZ(num2));
				num = World.toBlockXZ(num);
				num2 = World.toBlockXZ(num2);
			}
			if (chunk != null)
			{
				BlockValue blockNoDamage = chunk.GetBlockNoDamage(num, _y, num2);
				Block block = blockNoDamage.Block;
				if (!blockNoDamage.isair && block.StabilitySupport && !block.blockMaterial.IsLiquid && !block.StabilityIgnore)
				{
					byte stability = chunk.GetStability(num, _y, num2);
					if (stability != 0 && (int)stability < _stabStop)
					{
						chunk.SetStability(num, _y, num2, 0);
						this.clearHorizontal(chunk, num, _y, num2, _stabStop);
						this.clearDown(chunk, num, _y, num2, _stabStop);
					}
				}
			}
		}
	}

	public const byte MaxStability = 15;

	[PublicizedFrom(EAccessModifier.Private)]
	public WorldBase world;
}
