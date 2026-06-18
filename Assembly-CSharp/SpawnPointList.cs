using System;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPointList : List<SpawnPoint>
{
	public SpawnPoint Find(Vector3i _blockPos)
	{
		for (int i = 0; i < base.Count; i++)
		{
			SpawnPoint spawnPoint = base[i];
			if (spawnPoint.spawnPosition.ToBlockPos().Equals(_blockPos))
			{
				return spawnPoint;
			}
		}
		return null;
	}

	public virtual SpawnPosition GetRandomSpawnPosition(World _world, Vector3? _refPosition = null, int _minDistance = 0, int _maxDistance = 0)
	{
		if (base.Count > 0)
		{
			GameRandom gameRandom = _world.GetGameRandom();
			if (_refPosition == null)
			{
				return base[gameRandom.RandomRange(base.Count)].spawnPosition;
			}
			Vector3 value = _refPosition.Value;
			for (int i = 0; i < 100; i++)
			{
				int num = gameRandom.RandomRange(base.Count);
				SpawnPosition spawnPosition = base[num].spawnPosition;
				float magnitude = (spawnPosition.position - value).magnitude;
				if (magnitude >= (float)_minDistance && magnitude <= (float)_maxDistance)
				{
					return spawnPosition;
				}
			}
			float num2 = float.MaxValue;
			int num3 = -1;
			for (int j = 0; j < base.Count; j++)
			{
				float sqrMagnitude = (base[j].spawnPosition.position - value).sqrMagnitude;
				if (sqrMagnitude < num2)
				{
					num2 = sqrMagnitude;
					num3 = j;
				}
			}
			if (num3 != -1)
			{
				return base[num3].spawnPosition;
			}
		}
		return SpawnPosition.Undef;
	}

	public void Read(IBinaryReaderOrWriter _readerOrWriter)
	{
		if (_readerOrWriter == null)
		{
			return;
		}
		base.Clear();
		uint num = (uint)_readerOrWriter.ReadWrite(0);
		int num2 = _readerOrWriter.ReadWrite(0);
		for (int i = 0; i < num2; i++)
		{
			SpawnPoint spawnPoint = new SpawnPoint();
			spawnPoint.Read(_readerOrWriter, num);
			base.Add(spawnPoint);
		}
	}

	public void Read(PooledBinaryReader _br)
	{
		if (_br == null)
		{
			return;
		}
		base.Clear();
		uint num = (uint)_br.ReadByte();
		int num2 = _br.ReadInt32();
		for (int i = 0; i < num2; i++)
		{
			SpawnPoint spawnPoint = new SpawnPoint();
			spawnPoint.Read(_br, num);
			base.Add(spawnPoint);
		}
	}

	public void Write(PooledBinaryWriter _bw)
	{
		_bw.Write((byte)SpawnPointList.CurrentSaveVersion);
		_bw.Write(base.Count);
		for (int i = 0; i < base.Count; i++)
		{
			base[i].Write(_bw);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static uint CurrentSaveVersion = 2U;
}
