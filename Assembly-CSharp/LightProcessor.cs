using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Profiling;

public class LightProcessor : ILightProcessor
{
	public LightProcessor(IChunkAccess _world)
	{
		this.m_World = _world;
	}

	public void GenerateSunlight(Chunk chunk, bool _isSpread)
	{
		for (int i = 0; i < 16; i++)
		{
			for (int j = 0; j < 16; j++)
			{
				this.RefreshSunlightAtLocalPos(chunk, j, i, _isSpread);
			}
		}
	}

	public void LightChunk(Chunk c)
	{
		int maxHeight = (int)c.GetMaxHeight();
		for (int i = 0; i < 16; i++)
		{
			for (int j = 0; j < 16; j++)
			{
				for (int k = maxHeight; k >= 0; k--)
				{
					byte light = c.GetLight(j, k, i, Chunk.LIGHT_TYPE.SUN);
					if (light > 0)
					{
						this.SpreadLight(c, j, k, i, light, Chunk.LIGHT_TYPE.SUN, false);
					}
				}
			}
		}
	}

	public void RefreshSunlightAtLocalPos(Chunk c, int x, int z, bool _isSpread)
	{
		bool flag = false;
		int num = 15;
		for (int i = 255; i >= 0; i--)
		{
			int blockId = c.GetBlockId(x, i, z);
			int lightOpacity = Block.list[blockId].lightOpacity;
			if (lightOpacity == 255)
			{
				flag = true;
			}
			byte light = c.GetLight(x, i, z, Chunk.LIGHT_TYPE.SUN);
			byte b;
			if (!flag)
			{
				num = Utils.FastMax(0, num - lightOpacity);
				b = (byte)num;
				if (light != b)
				{
					c.SetLight(x, i, z, b, Chunk.LIGHT_TYPE.SUN);
				}
			}
			else
			{
				if (light != 0)
				{
					c.SetLight(x, i, z, 0, Chunk.LIGHT_TYPE.SUN);
				}
				b = 0;
				if (_isSpread)
				{
					b = this.RefreshLightAtLocalPos(c, x, i, z, Chunk.LIGHT_TYPE.SUN);
				}
			}
			if (_isSpread)
			{
				if (light > b)
				{
					this.UnspreadLight(c, x, i, z, light, Chunk.LIGHT_TYPE.SUN);
				}
				else if (light < b)
				{
					this.SpreadLight(c, x, i, z, b, Chunk.LIGHT_TYPE.SUN, true);
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public byte GetLightAt(int worldX, int worldY, int worldZ, Chunk.LIGHT_TYPE type)
	{
		IChunk chunkFromWorldPos = this.m_World.GetChunkFromWorldPos(worldX, worldY, worldZ);
		if (chunkFromWorldPos != null)
		{
			return chunkFromWorldPos.GetLight(World.toBlockXZ(worldX), World.toBlockY(worldY), World.toBlockXZ(worldZ), type);
		}
		return 0;
	}

	public byte RefreshLightAtLocalPos(Chunk c, int x, int y, int z, Chunk.LIGHT_TYPE type)
	{
		byte b = 0;
		int blockId = c.GetBlockId(x, y, z);
		int lightOpacity = Block.list[blockId].lightOpacity;
		if (lightOpacity == 255)
		{
			c.SetLight(x, y, z, 0, type);
		}
		else
		{
			int blockWorldPosX = c.GetBlockWorldPosX(x);
			int blockWorldPosZ = c.GetBlockWorldPosZ(z);
			byte lightAt = this.GetLightAt(blockWorldPosX, y, blockWorldPosZ, type);
			byte lightAt2 = this.GetLightAt(blockWorldPosX + 1, y, blockWorldPosZ, type);
			byte lightAt3 = this.GetLightAt(blockWorldPosX - 1, y, blockWorldPosZ, type);
			byte lightAt4 = this.GetLightAt(blockWorldPosX, y, blockWorldPosZ + 1, type);
			byte lightAt5 = this.GetLightAt(blockWorldPosX, y, blockWorldPosZ - 1, type);
			byte lightAt6 = this.GetLightAt(blockWorldPosX, y + 1, blockWorldPosZ, type);
			byte lightAt7 = this.GetLightAt(blockWorldPosX, y - 1, blockWorldPosZ, type);
			int num = (int)Utils.FastMax(Utils.FastMax(Utils.FastMax(lightAt2, lightAt3), Utils.FastMax(lightAt4, lightAt5)), Utils.FastMax(lightAt6, lightAt7));
			num = num - 1 - lightOpacity;
			if (num < 0)
			{
				num = 0;
			}
			b = (byte)Utils.FastMax(num, (int)lightAt);
			c.SetLight(x, y, z, b, type);
		}
		return b;
	}

	public void UnspreadLight(Chunk c, int x, int y, int z, byte lightValue, Chunk.LIGHT_TYPE type)
	{
		this.brightSpots.Clear();
		this.UnspreadLight(c, x, y, z, lightValue, 0, type, this.brightSpots);
		foreach (Vector3i vector3i in this.brightSpots)
		{
			Chunk chunk = (Chunk)this.m_World.GetChunkFromWorldPos(vector3i.x, vector3i.y, vector3i.z);
			if (chunk != null)
			{
				byte lightAt = this.GetLightAt(vector3i.x, vector3i.y, vector3i.z, type);
				this.SpreadLight(chunk, World.toBlockXZ(vector3i.x), World.toBlockY(vector3i.y), World.toBlockXZ(vector3i.z), lightAt, 0, type, true);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UnspreadLight(Chunk _chunk, int _blockX, int _blockY, int _blockZ, byte _lightValue, int depth, Chunk.LIGHT_TYPE type, List<Vector3i> brightSpots)
	{
		_chunk.SetLight(_blockX, _blockY, _blockZ, 0, type);
		for (int i = 0; i < Vector3i.AllDirections.Length; i++)
		{
			int num = _blockY + Vector3i.AllDirections[i].y;
			if (num < 256)
			{
				int num2 = _blockX + Vector3i.AllDirections[i].x;
				int num3 = _blockZ + Vector3i.AllDirections[i].z;
				Chunk chunk = _chunk;
				if (num2 >= 16 || num3 >= 16)
				{
					chunk = (Chunk)this.m_World.GetChunkFromWorldPos(_chunk.GetBlockWorldPosX(num2), num, _chunk.GetBlockWorldPosZ(num3));
					if (chunk == null)
					{
						goto IL_0109;
					}
					num2 = World.toBlockXZ(num2);
					num3 = World.toBlockXZ(num3);
				}
				byte light = chunk.GetLight(num2, num, num3, type);
				if (light < _lightValue && light != 0)
				{
					int blockId = chunk.GetBlockId(num2, num, num3);
					int num4 = (int)this.CalcNextLightStep(_lightValue, blockId);
					if (num4 > 0)
					{
						this.UnspreadLight(chunk, num2, num, num3, (byte)num4, depth + 1, type, brightSpots);
					}
				}
				else if (light >= _lightValue)
				{
					brightSpots.Add(new Vector3i(chunk.GetBlockWorldPosX(num2), num, chunk.GetBlockWorldPosZ(num3)));
				}
			}
			IL_0109:;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void SpreadLight(Chunk c, int blockX, int blockY, int blockZ, byte lightValue, Chunk.LIGHT_TYPE type, bool bSetAtStarterPos = true)
	{
		this.SpreadLight(c, blockX, blockY, blockZ, lightValue, 0, type, bSetAtStarterPos);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SpreadLight(Chunk _chunk, int _blockX, int _blockY, int _blockZ, byte _lightValue, int depth, Chunk.LIGHT_TYPE type, bool bSetAtStarterPos = true)
	{
		if (bSetAtStarterPos)
		{
			_chunk.SetLight(_blockX, _blockY, _blockZ, _lightValue, type);
		}
		if (_lightValue == 0)
		{
			return;
		}
		for (int i = Vector3i.AllDirections.Length - 1; i >= 0; i--)
		{
			Vector3i vector3i = Vector3i.AllDirections[i];
			int num = _blockY + vector3i.y;
			if (num <= 255)
			{
				int num2 = _blockX + vector3i.x;
				int num3 = _blockZ + vector3i.z;
				Chunk chunk = _chunk;
				if (num2 >= 16 || num3 >= 16)
				{
					chunk = (Chunk)this.m_World.GetChunkFromWorldPos(_chunk.GetBlockWorldPosX(num2), num, _chunk.GetBlockWorldPosZ(num3));
					if (chunk == null)
					{
						goto IL_00F0;
					}
					num2 = World.toBlockXZ(num2);
					num3 = World.toBlockXZ(num3);
				}
				byte light = chunk.GetLight(num2, num, num3, type);
				if (light < 15)
				{
					int type2 = chunk.GetBlockNoDamage(num2, num, num3).type;
					byte b = this.CalcNextLightStep(_lightValue, type2);
					if (light < b)
					{
						this.SpreadLight(chunk, num2, num, num3, b, depth + 1, type, true);
					}
				}
			}
			IL_00F0:;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public byte CalcNextLightStep(byte _currentLight, int _id)
	{
		int lightOpacity = Block.list[_id].lightOpacity;
		int num = (int)_currentLight - ((lightOpacity != 0) ? lightOpacity : 1);
		return (byte)((num >= 0) ? num : 0);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly IChunkAccess m_World;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<Vector3i> brightSpots = new List<Vector3i>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static ProfilerMarker pmSpreadLight = new ProfilerMarker("LightProcessor SpreadLight");

	[PublicizedFrom(EAccessModifier.Private)]
	public static ProfilerMarker pmUnspreadLight = new ProfilerMarker("LightProcessor UnspreadLight");
}
