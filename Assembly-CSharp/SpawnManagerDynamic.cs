using System;
using System.IO;
using System.Xml;
using UnityEngine;

public class SpawnManagerDynamic : SpawnManagerAbstract
{
	public SpawnManagerDynamic(World _world, XmlDocument _spawnXml)
		: base(_world)
	{
		this.lastDaySpawned = -1;
	}

	public void Write(BinaryWriter _bw)
	{
		_bw.Write(1);
		_bw.Write(this.currentSpawner != null);
		if (this.currentSpawner != null)
		{
			_bw.Write(this.lastDaySpawned);
			this.currentSpawner.Write(_bw);
		}
	}

	public void Read(BinaryReader _br)
	{
		_br.ReadByte();
		if (_br.ReadBoolean())
		{
			this.currentSpawner = new EntitySpawner();
			this.lastDaySpawned = _br.ReadInt32();
			this.currentSpawner.Read(_br);
		}
	}

	public override void Update(string _spawnerName, bool _bSpawnEnemyEntities, object _userData)
	{
		if (this.world.IsDaytime())
		{
			return;
		}
		if (this.world.Players.list.Count == 0)
		{
			return;
		}
		int num = GameUtils.WorldTimeToDays(this.world.worldTime);
		if (num != this.lastDaySpawned || this.currentSpawner == null)
		{
			this.lastDaySpawned = num;
			EntitySpawner entitySpawner = this.currentSpawner;
			Log.Out("New ES '" + _spawnerName + "' for day: " + num.ToString());
			this.currentSpawner = new EntitySpawner(_spawnerName, Vector3i.zero, Vector3i.zero, 0, (entitySpawner != null) ? entitySpawner.GetEntityIdsSpaned() : null);
		}
		if (this.currentSpawner != null)
		{
			this.currentSpawner.SpawnManually(this.world, num, _bSpawnEnemyEntities, delegate(EntitySpawner _es, out EntityPlayer _outPlayerToAttack)
			{
				_outPlayerToAttack = null;
				return true;
			}, delegate(EntitySpawner _es, EntityPlayer _inPlayerToAttack, out EntityPlayer _outPlayerToAttack, out Vector3 _pos)
			{
				return this.world.GetRandomSpawnPositionMinMaxToRandomPlayer(64, 96, true, out _outPlayerToAttack, out _pos);
			}, null, null);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const byte CurrentFileVersion = 1;

	public const int cMinRange = 64;

	public const int cMaxRange = 96;

	[PublicizedFrom(EAccessModifier.Private)]
	public EntitySpawner currentSpawner;

	[PublicizedFrom(EAccessModifier.Private)]
	public int lastDaySpawned;
}
