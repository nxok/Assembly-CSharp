using System;
using UnityEngine;

public class RandomPositionGenerator
{
	public static Vector3 CalcAround(EntityAlive _entity, int _maxXZ, int _maxY)
	{
		Vector3 vector;
		if (!RandomPositionGenerator.CalcAround(_entity, _maxXZ, _maxY, false, out vector) && _entity.isSwimming)
		{
			RandomPositionGenerator.CalcAround(_entity, _maxXZ, _maxY, true, out vector);
		}
		return vector;
	}

	public static Vector3 CalcTowards(EntityAlive _entity, int _minXZ, int _maxXZ, int _maxY, Vector3 _position)
	{
		Vector3 vector = _position - _entity.position;
		return RandomPositionGenerator.CalcInDir(_entity, _minXZ, _maxXZ, _maxY, vector, 80f);
	}

	public static Vector3 CalcAway(EntityAlive _entity, int _minXZ, int _maxXZ, int _maxY, Vector3 _position)
	{
		Vector3 vector = _entity.position - _position;
		return RandomPositionGenerator.CalcInDir(_entity, _minXZ, _maxXZ, _maxY, vector, 80f);
	}

	public static Vector3 CalcInDir(EntityAlive _entity, int _minXZ, int _maxXZ, int _maxY, Vector3 _dirV, float _angleMax = 80f)
	{
		Vector3 vector;
		if (!RandomPositionGenerator.CalcDir(_entity, _minXZ, _maxXZ, _maxY, _dirV, _angleMax, false, out vector) && _entity.isSwimming)
		{
			RandomPositionGenerator.CalcDir(_entity, _minXZ, _maxXZ, _maxY, _dirV, _angleMax, true, out vector);
		}
		return vector;
	}

	public static Vector3 CalcNear(EntityAlive _entity, Vector3 target, int _xzDist, int _yDist)
	{
		GameRandom rand = _entity.rand;
		int num = rand.RandomRange(2 * _xzDist) - _xzDist;
		int num2 = rand.RandomRange(2 * _yDist) - _yDist;
		int num3 = rand.RandomRange(2 * _xzDist) - _xzDist;
		num += Utils.Fastfloor(target.x);
		num2 += Utils.Fastfloor(target.y);
		num3 += Utils.Fastfloor(target.z);
		return new Vector3((float)num, (float)num2, (float)num3);
	}

	public static Vector3 CalcPositionInDirection(Entity _entity, Vector3 _startPos, Vector3 _dirV, float _dist, float _angleMax)
	{
		World world = _entity.world;
		_dirV.y = 0f;
		Vector3 vector = _dirV.normalized;
		Quaternion quaternion = Quaternion.Euler(0f, _angleMax * (_entity.rand.RandomFloat - 0.5f), 0f);
		vector = Matrix4x4.TRS(Vector3.zero, quaternion, Vector3.one).MultiplyVector(vector);
		Vector3i vector3i = World.worldToBlockPos(_startPos);
		Chunk chunk;
		do
		{
			vector3i.x = (int)(_startPos.x + vector.x * _dist);
			vector3i.z = (int)(_startPos.z + vector.z * _dist);
			chunk = (Chunk)world.GetChunkFromWorldPos(vector3i.x, 0, vector3i.z);
			if (chunk != null)
			{
				break;
			}
			_dist -= 8f;
		}
		while (_dist > 0f);
		if (_dist <= 0f || chunk == null)
		{
			return Vector3.zero;
		}
		int num = World.toBlockXZ(vector3i.x);
		int num2 = World.toBlockXZ(vector3i.z);
		BlockValue blockValue = chunk.GetBlockNoDamage(num, vector3i.y, num2);
		if (!blockValue.Block.IsMovementBlocked(world, vector3i, blockValue, BlockFaceFlag.None))
		{
			do
			{
				int num3 = vector3i.y - 1;
				vector3i.y = num3;
				if (num3 < 0)
				{
					goto IL_01B7;
				}
				blockValue = chunk.GetBlockNoDamage(num, vector3i.y, num2);
			}
			while (!blockValue.Block.IsMovementBlocked(world, vector3i, blockValue, BlockFaceFlag.None));
			vector3i.y++;
			return vector3i;
		}
		do
		{
			int num3 = vector3i.y + 1;
			vector3i.y = num3;
			if (num3 >= 256)
			{
				goto Block_5;
			}
			blockValue = chunk.GetBlockNoDamage(num, vector3i.y, num2);
		}
		while (blockValue.Block.IsMovementBlocked(world, vector3i, blockValue, BlockFaceFlag.None));
		return vector3i;
		Block_5:
		IL_01B7:
		return Vector3.zero;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool CalcAround(EntityAlive _entity, int _xzDist, int _yDist, bool _canSwim, out Vector3 _destPos)
	{
		GameRandom rand = _entity.rand;
		World world = _entity.world;
		ChunkCluster chunkCache = world.ChunkCache;
		Vector3 vector = _entity.position;
		if (_entity.IsSleeper)
		{
			vector = _entity.SleeperSpawnPosition;
		}
		Vector3i vector3i = World.worldToBlockPos(vector);
		bool flag = false;
		if (_entity.hasHome())
		{
			flag = _entity.getHomePosition().getDistance(vector3i.x, vector3i.y, vector3i.z) + 4f < (float)(_entity.getMaximumHomeDistance() + _xzDist);
		}
		for (int i = 0; i < 30; i++)
		{
			Vector3i vector3i2;
			vector3i2.x = rand.RandomRange(2 * _xzDist) - _xzDist;
			vector3i2.z = rand.RandomRange(2 * _xzDist) - _xzDist;
			vector3i2.y = rand.RandomRange(2 * _yDist) - _yDist;
			vector3i2.x += vector3i.x;
			vector3i2.y += vector3i.y;
			vector3i2.z += vector3i.z;
			if (chunkCache.GetBlock(vector3i2).isair && (_canSwim || !world.IsWater(vector3i2)) && (!flag || _entity.isWithinHomeDistance(vector3i2.x, vector3i2.y, vector3i2.z)) && vector3i2.y >= 0)
			{
				if (!_canSwim)
				{
					bool flag2 = false;
					Vector3i vector3i3 = vector3i2;
					for (int j = 0; j < 10; j++)
					{
						vector3i3.y--;
						BlockValue block = chunkCache.GetBlock(vector3i3);
						if (world.IsWater(vector3i3))
						{
							flag2 = true;
							break;
						}
						if (block.Block.IsMovementBlocked(world, vector3i3, block, BlockFaceFlag.None))
						{
							break;
						}
					}
					if (flag2)
					{
						goto IL_01C6;
					}
				}
				_destPos = new Vector3((float)vector3i2.x + 0.5f, (float)vector3i2.y, (float)vector3i2.z + 0.5f);
				return true;
			}
			IL_01C6:;
		}
		_destPos = Vector3.zero;
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool CalcDir(EntityAlive _entity, int _distMinXZ, int _distMaxXZ, int _distMaxY, Vector3 _directionVec, float _angleMax, bool _canSwim, out Vector3 _destPos)
	{
		if (_directionVec == Vector3.zero)
		{
			return RandomPositionGenerator.CalcAround(_entity, _distMaxXZ, _distMaxY, _canSwim, out _destPos);
		}
		GameRandom rand = _entity.rand;
		ChunkCluster chunkCache = _entity.world.ChunkCache;
		Vector3i vector3i = World.worldToBlockPos(_entity.position);
		if (_distMaxXZ < _distMinXZ)
		{
			_distMaxXZ = _distMinXZ;
		}
		bool flag = false;
		if (_entity.hasHome())
		{
			float num = _entity.getHomePosition().getDistance(vector3i.x, vector3i.y, vector3i.z) + 1f;
			if ((float)_distMinXZ > num)
			{
				_distMinXZ = (int)num;
			}
			if ((float)_distMaxXZ > num)
			{
				_distMaxXZ = (int)num;
			}
			flag = (float)(_entity.getMaximumHomeDistance() + _distMaxXZ) - num >= 2f;
		}
		float num2 = (float)(_distMaxXZ - _distMinXZ);
		Vector2 vector;
		vector.x = _directionVec.x;
		vector.y = _directionVec.z;
		vector.Normalize();
		for (int i = 0; i < 30; i++)
		{
			float num3 = _angleMax * (rand.RandomFloat - 0.5f) * 0.017453292f;
			float num4 = (float)_distMinXZ + rand.RandomRange(num2);
			float num5 = Mathf.Sin(num3);
			float num6 = Mathf.Cos(num3);
			Vector2 vector2;
			vector2.x = vector.x * num6 - vector.y * num5;
			vector2.y = vector.x * num5 + vector.y * num6;
			vector2.x *= num4;
			vector2.y *= num4;
			Vector3i vector3i2;
			vector3i2.x = Utils.Fastfloor(vector2.x);
			vector3i2.z = Utils.Fastfloor(vector2.y);
			vector3i2.y = rand.RandomRange(2 * _distMaxY) - _distMaxY;
			vector3i2.x += vector3i.x;
			vector3i2.y += vector3i.y;
			vector3i2.z += vector3i.z;
			if (chunkCache.GetBlock(vector3i2).isair && (_canSwim || !_entity.world.IsWater(vector3i2)) && (!flag || _entity.isWithinHomeDistance(vector3i2.x, vector3i2.y, vector3i2.z)))
			{
				_destPos = new Vector3((float)vector3i2.x + 0.5f, (float)vector3i2.y, (float)vector3i2.z + 0.5f);
				return true;
			}
		}
		_destPos = Vector3.zero;
		return false;
	}
}
