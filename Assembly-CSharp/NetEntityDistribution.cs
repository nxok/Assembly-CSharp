using System;
using System.Collections.Generic;
using UnityEngine;

public class NetEntityDistribution
{
	public NetEntityDistribution(World _world, int _v)
	{
		this.trackedEntitySet = new HashSetList<NetEntityDistributionEntry>();
		this.trackedEntityHashTable = new IntHashMap();
		this.world = _world;
	}

	public void OnUpdateEntities()
	{
		this.playerList.Clear();
		this.enemyList.Clear();
		for (int i = 0; i < this.trackedEntitySet.list.Count; i++)
		{
			NetEntityDistributionEntry netEntityDistributionEntry = this.trackedEntitySet.list[i];
			EntityEnemy entityEnemy = netEntityDistributionEntry.trackedEntity as EntityEnemy;
			if (entityEnemy != null)
			{
				this.enemyList.Add(entityEnemy);
			}
			else
			{
				EntityPlayer entityPlayer = netEntityDistributionEntry.trackedEntity as EntityPlayer;
				if (entityPlayer != null)
				{
					this.playerList.Add(entityPlayer);
				}
			}
		}
		foreach (EntityEnemy entityEnemy2 in this.enemyList)
		{
			NetEntityDistributionEntry netEntityDistributionEntry2 = (NetEntityDistributionEntry)this.trackedEntityHashTable.lookup(entityEnemy2.entityId);
			bool flag = entityEnemy2.IsAirBorne();
			netEntityDistributionEntry2.priorityLevel = 1;
			if (GameManager.enableNetworkdPrioritization)
			{
				float num = float.MaxValue;
				bool flag2 = false;
				Vector3 position = entityEnemy2.transform.position;
				position.y = 0f;
				for (int j = 0; j < this.world.Players.Count; j++)
				{
					Transform transform = this.world.Players.list[j].transform;
					Vector3 position2 = transform.position;
					position2.y = 0f;
					Vector3 vector = position - position2;
					float num2 = vector.x * vector.x + vector.z + vector.z;
					if (!flag2 && num2 < 16384f && Vector3.Angle(transform.forward, vector.normalized) < NetEntityDistribution.priorityViewAngleLimit)
					{
						flag2 = true;
					}
					if (num2 < num)
					{
						num = num2;
					}
				}
				if (num < 25f)
				{
					netEntityDistributionEntry2.priorityLevel = 0;
				}
				else if (!flag2 && !flag)
				{
					if (num > 625f)
					{
						netEntityDistributionEntry2.priorityLevel = 3;
					}
					else if (num > 324f)
					{
						netEntityDistributionEntry2.priorityLevel = 2;
					}
				}
			}
		}
		if (this.playerList.Count > 1)
		{
			foreach (EntityPlayer entityPlayer2 in this.playerList)
			{
				NetEntityDistributionEntry netEntityDistributionEntry3 = (NetEntityDistributionEntry)this.trackedEntityHashTable.lookup(entityPlayer2.entityId);
				netEntityDistributionEntry3.priorityLevel = 1;
				if (GameManager.enableNetworkdPrioritization)
				{
					Vector3 position3 = entityPlayer2.transform.position;
					foreach (EntityPlayer entityPlayer3 in this.playerList)
					{
						if (!(entityPlayer3 == entityPlayer2))
						{
							Vector3 vector2 = position3 - entityPlayer3.transform.position;
							if (vector2.x * vector2.x + vector2.z * vector2.z < 25f)
							{
								netEntityDistributionEntry3.priorityLevel = 0;
								break;
							}
						}
					}
				}
			}
		}
		for (int k = 0; k < this.trackedEntitySet.list.Count; k++)
		{
			this.trackedEntitySet.list[k].updatePlayerList(this.world.Players.list);
		}
		for (int l = 0; l < this.playerList.Count; l++)
		{
			EntityPlayer entityPlayer4 = this.playerList[l];
			for (int m = 0; m < this.trackedEntitySet.list.Count; m++)
			{
				NetEntityDistributionEntry netEntityDistributionEntry4 = this.trackedEntitySet.list[m];
				if (netEntityDistributionEntry4.trackedEntity != entityPlayer4)
				{
					netEntityDistributionEntry4.updatePlayerEntity(entityPlayer4);
				}
			}
		}
	}

	public void SendPacketToTrackedPlayers(int _entityId, int _excludePlayer, NetPackage _package, bool _inRangeOnly = false)
	{
		NetEntityDistributionEntry netEntityDistributionEntry = (NetEntityDistributionEntry)this.trackedEntityHashTable.lookup(_entityId);
		if (netEntityDistributionEntry != null)
		{
			netEntityDistributionEntry.SendToPlayers(_package, _excludePlayer, _inRangeOnly, 192);
		}
	}

	public void SendPacketToTrackedPlayersAndTrackedEntity(int _entityId, int _excludePlayer, NetPackage _package, bool _inRangeOnly = false)
	{
		NetEntityDistributionEntry netEntityDistributionEntry = (NetEntityDistributionEntry)this.trackedEntityHashTable.lookup(_entityId);
		if (netEntityDistributionEntry != null)
		{
			netEntityDistributionEntry.sendPacketToTrackedPlayersAndTrackedEntity(_package, _excludePlayer, _inRangeOnly);
		}
	}

	public void Add(Entity _e)
	{
		for (int i = 0; i < this.config.Count; i++)
		{
			NetEntityDistribution.SEnts sents = this.config[i];
			if (sents.eType.IsAssignableFrom(_e.GetType()))
			{
				this.Add(_e, sents.distance, sents.update, sents.motion);
			}
			if (_e is EntityPlayer)
			{
				EntityPlayer entityPlayer = (EntityPlayer)_e;
				for (int j = 0; j < this.trackedEntitySet.list.Count; j++)
				{
					NetEntityDistributionEntry netEntityDistributionEntry = this.trackedEntitySet.list[j];
					if (netEntityDistributionEntry.trackedEntity != entityPlayer)
					{
						netEntityDistributionEntry.updatePlayerEntity(entityPlayer);
					}
				}
			}
		}
	}

	public void Add(Entity _e, int _d, int _t)
	{
		this.Add(_e, _d, _t, false);
	}

	public void Add(Entity _e, int _distance, int _t, bool _upd)
	{
		if (!this.trackedEntityHashTable.containsItem(_e.entityId))
		{
			NetEntityDistributionEntry netEntityDistributionEntry = new NetEntityDistributionEntry(_e, _distance, _t, _upd);
			this.trackedEntitySet.Add(netEntityDistributionEntry);
			this.trackedEntityHashTable.addKey(_e.entityId, netEntityDistributionEntry);
			netEntityDistributionEntry.updatePlayerEntities(this.world.Players.list);
		}
	}

	public void Remove(Entity _e, EnumRemoveEntityReason _reason)
	{
		if (_e is EntityPlayer)
		{
			EntityPlayer entityPlayer = (EntityPlayer)_e;
			for (int i = 0; i < this.trackedEntitySet.list.Count; i++)
			{
				this.trackedEntitySet.list[i].Remove(entityPlayer);
			}
		}
		NetEntityDistributionEntry netEntityDistributionEntry = (NetEntityDistributionEntry)this.trackedEntityHashTable.removeObject(_e.entityId);
		if (netEntityDistributionEntry != null)
		{
			this.trackedEntitySet.Remove(netEntityDistributionEntry);
			if (_reason == EnumRemoveEntityReason.Unloaded)
			{
				netEntityDistributionEntry.SendUnloadEntityToPlayers();
				return;
			}
			netEntityDistributionEntry.SendDestroyEntityToPlayers();
		}
	}

	public void SyncEntity(Entity _e, Vector3 _pos, Vector3 _rot)
	{
		NetEntityDistributionEntry netEntityDistributionEntry = (NetEntityDistributionEntry)this.trackedEntityHashTable.lookup(_e.entityId);
		if (netEntityDistributionEntry != null)
		{
			netEntityDistributionEntry.encodedPos = NetEntityDistributionEntry.EncodePos(_pos);
			netEntityDistributionEntry.encodedRot = NetEntityDistributionEntry.EncodePos(_rot);
		}
	}

	public void SendFullUpdateNextTick(Entity _e)
	{
		NetEntityDistributionEntry netEntityDistributionEntry = (NetEntityDistributionEntry)this.trackedEntityHashTable.lookup(_e.entityId);
		if (netEntityDistributionEntry != null)
		{
			netEntityDistributionEntry.SendFullUpdateNextTick();
		}
	}

	public void Cleanup()
	{
		this.trackedEntitySet.Clear();
		this.trackedEntityHashTable.clearMap();
	}

	public NetEntityDistributionEntry FindEntry(Entity entity)
	{
		return this.trackedEntityHashTable.lookup(entity.entityId) as NetEntityDistributionEntry;
	}

	public const float cHighPriorityRange = 5f;

	public const float cLowPriorityRange = 18f;

	public const float cLowestPriorityRange = 25f;

	public const int MobsUpdateTicks = 3;

	public const int lowPriorityTick = 6;

	public const int lowestPriorityTick = 10;

	public static float priorityViewAngleLimit = 60f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float priorityViewAngleMinDistance = 128f;

	[PublicizedFrom(EAccessModifier.Private)]
	public HashSetList<NetEntityDistributionEntry> trackedEntitySet;

	[PublicizedFrom(EAccessModifier.Private)]
	public IntHashMap trackedEntityHashTable;

	[PublicizedFrom(EAccessModifier.Private)]
	public World world;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<NetEntityDistribution.SEnts> config = new List<NetEntityDistribution.SEnts>
	{
		new NetEntityDistribution.SEnts(typeof(EntityPlayer), int.MaxValue, 3, false),
		new NetEntityDistribution.SEnts(typeof(EntityVehicle), int.MaxValue, 3, false),
		new NetEntityDistribution.SEnts(typeof(EntityEnemy), 80, 3, false),
		new NetEntityDistribution.SEnts(typeof(EntityNPC), 80, 3, false),
		new NetEntityDistribution.SEnts(typeof(EntityItem), 64, 3, false),
		new NetEntityDistribution.SEnts(typeof(EntityFallingBlock), 120, 3, false),
		new NetEntityDistribution.SEnts(typeof(EntityFallingBlocks), 120, 3, false),
		new NetEntityDistribution.SEnts(typeof(EntityFallingTree), 120, 1, false),
		new NetEntityDistribution.SEnts(typeof(EntityAnimalStag), 80, 3, false),
		new NetEntityDistribution.SEnts(typeof(EntityAnimalRabbit), 64, 3, false),
		new NetEntityDistribution.SEnts(typeof(EntityCar), 100, 3, false),
		new NetEntityDistribution.SEnts(typeof(EntitySupplyCrate), 1200, 3, false),
		new NetEntityDistribution.SEnts(typeof(EntitySupplyPlane), 1200, 3, true),
		new NetEntityDistribution.SEnts(typeof(EntityTurret), 60, 3, false),
		new NetEntityDistribution.SEnts(typeof(EntityHomerunGoal), 80, 3, false)
	};

	[PublicizedFrom(EAccessModifier.Private)]
	public List<EntityPlayer> playerList = new List<EntityPlayer>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<EntityEnemy> enemyList = new List<EntityEnemy>();

	[PublicizedFrom(EAccessModifier.Private)]
	public struct SEnts
	{
		public SEnts(Type _eType, int _distance, int _update, bool _motion)
		{
			this.eType = _eType;
			this.distance = _distance;
			this.update = _update;
			this.motion = _motion;
		}

		public Type eType;

		public int distance;

		public int update;

		public bool motion;
	}
}
