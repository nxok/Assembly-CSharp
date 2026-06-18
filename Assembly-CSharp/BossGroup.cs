using System;
using System.Collections.Generic;
using Audio;
using GameEvent.SequenceActions;
using UnityEngine;

public class BossGroup
{
	public string GetBossNavClass
	{
		get
		{
			BossGroup.BossGroupTypes currentGroupType = this.CurrentGroupType;
			if (currentGroupType == BossGroup.BossGroupTypes.ImmortalBoss)
			{
				return "twitch_vote_boss_shield";
			}
			if (currentGroupType != BossGroup.BossGroupTypes.Specialized)
			{
				return "twitch_vote_boss";
			}
			return "";
		}
	}

	public string GetMinionNavClass
	{
		get
		{
			BossGroup.BossGroupTypes currentGroupType = this.CurrentGroupType;
			if (currentGroupType == BossGroup.BossGroupTypes.ImmortalMinions)
			{
				return "twitch_vote_minion_shield";
			}
			if (currentGroupType != BossGroup.BossGroupTypes.Specialized)
			{
				return "twitch_vote_minion";
			}
			return "";
		}
	}

	public int MinionCount
	{
		get
		{
			if (this.MinionEntityIDs != null)
			{
				return this.MinionEntityIDs.Count;
			}
			return 0;
		}
	}

	public BossGroup(EntityPlayer target, EntityAlive boss, List<EntityAlive> minions, BossGroup.BossGroupTypes bossGroupType, string bossIcon)
	{
		this.CurrentGroupType = bossGroupType;
		this.TargetPlayer = target;
		this.BossEntity = boss;
		this.MinionEntities = minions;
		this.BossName = Localization.Get(EntityClass.list[this.BossEntity.entityClass].entityClassName, false, null);
		this.BossEntityID = boss.entityId;
		this.MinionEntityIDs = new List<int>();
		for (int i = 0; i < minions.Count; i++)
		{
			this.MinionEntityIDs.Add(minions[i].entityId);
		}
		this.BossIcon = bossIcon;
		this.BossGroupID = ++BossGroup.nextID;
		this.serverBounds.size = this.LeavingSize;
	}

	public BossGroup(int bossGroupID, BossGroup.BossGroupTypes bossGroupType, int bossEntityID, List<int> minionIDs, string bossIcon)
	{
		this.CurrentGroupType = bossGroupType;
		this.BossEntityID = bossEntityID;
		this.MinionEntityIDs = minionIDs;
		this.BossEntity = null;
		this.MinionEntities = null;
		this.BossIcon = bossIcon;
		this.BossGroupID = bossGroupID;
	}

	public void Update(EntityPlayerLocal player)
	{
		float num = -1f;
		EntityAlive entityAlive = null;
		if (this.BossEntity == null)
		{
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsClient)
			{
				this.BossEntity = GameManager.Instance.World.GetEntity(this.BossEntityID) as EntityAlive;
				if (this.BossEntity != null)
				{
					if (this.BossName == "")
					{
						this.BossName = Localization.Get(EntityClass.list[this.BossEntity.entityClass].entityClassName, false, null);
					}
					if (this.BossEntity.IsAlive())
					{
						entityAlive = this.BossEntity;
					}
				}
			}
		}
		else if (this.BossEntity.IsAlive())
		{
			entityAlive = this.BossEntity;
		}
		if (entityAlive != null)
		{
			num = entityAlive.GetDistance(player);
		}
		if (this.MinionEntities != null)
		{
			for (int i = 0; i < this.MinionEntities.Count; i++)
			{
				if (this.MinionEntities[i] != null && this.MinionEntities[i].IsAlive())
				{
					float distance = this.MinionEntities[i].GetDistance(player);
					if (num == -1f || distance < num)
					{
						entityAlive = this.MinionEntities[i];
						num = distance;
					}
				}
			}
		}
		else
		{
			for (int j = 0; j < this.MinionEntityIDs.Count; j++)
			{
				EntityAlive entityAlive2 = GameManager.Instance.World.GetEntity(this.MinionEntityIDs[j]) as EntityAlive;
				if (entityAlive2 != null && entityAlive2.IsAlive())
				{
					float distance2 = entityAlive2.GetDistance(player);
					if (num == -1f || distance2 < num)
					{
						entityAlive = entityAlive2;
						num = distance2;
					}
				}
			}
		}
		if (entityAlive == null)
		{
			this.ReadyForRemove = true;
			return;
		}
		this.ReadyForRemove = false;
		this.bounds.center = entityAlive.position;
		this.bounds.size = (this.IsCurrent ? this.LeavingSize : this.EnteringSize);
	}

	public bool IsPlayerWithinRange(EntityPlayer player)
	{
		return this.bounds.Contains(player.position);
	}

	public bool IsPlayerWithinServerRange(EntityPlayer player)
	{
		return this.serverBounds.Contains(player.position);
	}

	public void RemoveMinion(int entityID)
	{
		if (this.MinionEntityIDs != null)
		{
			this.MinionEntityIDs.Remove(entityID);
		}
		if (this.MinionEntities != null)
		{
			for (int i = this.MinionEntities.Count - 1; i >= 0; i--)
			{
				if (this.MinionEntities[i] != null && this.MinionEntities[i].entityId == entityID)
				{
					this.MinionEntities.RemoveAt(i);
				}
			}
		}
	}

	public void AddNavObjects()
	{
		if (this.MinionEntities == null)
		{
			this.MinionEntities = new List<EntityAlive>();
			for (int i = 0; i < this.MinionEntityIDs.Count; i++)
			{
				this.MinionEntities.Add(GameManager.Instance.World.GetEntity(this.MinionEntityIDs[i]) as EntityAlive);
			}
		}
		if (this.BossEntity != null)
		{
			this.BossEntity.Buffs.AddBuff("twitch_give_navobject", -1, true, false, -1f);
		}
		if (this.MinionEntities != null)
		{
			for (int j = 0; j < this.MinionEntities.Count; j++)
			{
				if (this.MinionEntities[j] != null)
				{
					this.MinionEntities[j].Buffs.AddBuff("twitch_give_navobject", -1, true, false, -1f);
				}
			}
		}
	}

	public void RequestStatRefresh()
	{
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsClient)
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageBossEvent>().Setup(NetPackageBossEvent.BossEventTypes.RequestStats, this.BossGroupID), false);
		}
	}

	public void RefreshStats(int playerID)
	{
		if (this.MinionEntities == null)
		{
			this.MinionEntities = new List<EntityAlive>();
			for (int i = 0; i < this.MinionEntityIDs.Count; i++)
			{
				this.MinionEntities.Add(GameManager.Instance.World.GetEntity(this.MinionEntityIDs[i]) as EntityAlive);
			}
		}
		if (this.BossEntity != null)
		{
			this.BossEntity.bPlayerStatsChanged = true;
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageEntityStatChanged>().Setup(this.BossEntity, playerID, NetPackageEntityStatChanged.EnumStat.Health), false, -1, -1, -1, null, 192, false);
		}
		if (this.MinionEntities != null)
		{
			for (int j = 0; j < this.MinionEntities.Count; j++)
			{
				if (this.MinionEntities[j] != null)
				{
					SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageEntityStatChanged>().Setup(this.MinionEntities[j], playerID, NetPackageEntityStatChanged.EnumStat.Health), false, -1, -1, -1, null, 192, false);
				}
			}
		}
	}

	public void RemoveNavObjects()
	{
		if (this.BossEntity != null)
		{
			this.BossEntity.RemoveNavObject("twitch_vote_boss");
			this.BossEntity.RemoveNavObject("twitch_vote_boss_shield");
		}
		if (this.MinionEntities != null)
		{
			for (int i = 0; i < this.MinionEntities.Count; i++)
			{
				if (this.MinionEntities[i] != null)
				{
					this.MinionEntities[i].RemoveNavObject("twitch_vote_minion");
					this.MinionEntities[i].RemoveNavObject("twitch_vote_minion_shield");
				}
			}
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsClient)
			{
				this.MinionEntities = null;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetupTeleportList()
	{
		this.ClosestEnemy = this.GetClosestEntity(this.TargetPlayer);
		if (this.ClosestEnemy == null)
		{
			return;
		}
		this.serverBounds.center = this.ClosestEnemy.position;
		for (int i = 0; i < this.MinionEntities.Count; i++)
		{
			if (!(this.MinionEntities[i] == null) && this.MinionEntities[i].IsAlive() && !(this.MinionEntities[i] == this.ClosestEnemy))
			{
				EntityAlive entityAlive = this.MinionEntities[i];
				if (Vector3.Distance(this.ClosestEnemy.position, this.MinionEntities[i].position) > BossGroup.autoPullDistance)
				{
					if (!this.TeleportList.Contains(this.MinionEntities[i]))
					{
						this.TeleportList.Add(this.MinionEntities[i]);
					}
				}
				else if (this.TeleportList.Contains(this.MinionEntities[i]))
				{
					this.TeleportList.Remove(this.MinionEntities[i]);
				}
			}
		}
		if (this.ClosestEnemy == this.BossEntity || this.BossEntity == null || !this.BossEntity.IsAlive())
		{
			return;
		}
		if (Vector3.Distance(this.ClosestEnemy.position, this.BossEntity.position) > BossGroup.autoPullDistance)
		{
			if (!this.TeleportList.Contains(this.BossEntity))
			{
				this.TeleportList.Add(this.BossEntity);
				return;
			}
		}
		else if (this.TeleportList.Contains(this.BossEntity))
		{
			this.TeleportList.Remove(this.BossEntity);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityAlive GetClosestEntity(EntityPlayer player)
	{
		EntityAlive entityAlive = null;
		float num = -1f;
		for (int i = this.MinionEntities.Count - 1; i >= 0; i--)
		{
			if (this.MinionEntities[i] != null && this.MinionEntities[i].IsAlive())
			{
				float num2 = Vector3.Distance(this.TargetPlayer.position, this.MinionEntities[i].position);
				if (num > num2 || num == -1f)
				{
					num = num2;
					entityAlive = this.MinionEntities[i];
				}
			}
		}
		if (this.BossEntity != null && this.BossEntity.IsAlive())
		{
			float num2 = Vector3.Distance(this.TargetPlayer.position, this.BossEntity.position);
			if (num > num2 || num == -1f)
			{
				entityAlive = this.BossEntity;
			}
		}
		return entityAlive;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleTeleportList()
	{
		for (int i = this.TeleportList.Count - 1; i >= 0; i--)
		{
			EntityAlive entityAlive = this.TeleportList[i];
			Vector3 zero = Vector3.zero;
			if (ActionBaseSpawn.FindValidPosition(out zero, this.ClosestEnemy.position, 3f, 6f, true, 0f, false, 0f))
			{
				if (this.pullSound != "")
				{
					Manager.BroadcastPlayByLocalPlayer(entityAlive.position, this.pullSound);
				}
				entityAlive.SetPosition(zero, true);
				entityAlive.SetAttackTarget(this.TargetPlayer, 12000);
				this.TeleportList.RemoveAt(i);
				if (this.pullSound != "")
				{
					Manager.BroadcastPlayByLocalPlayer(zero, this.pullSound);
				}
			}
		}
	}

	public void HandleAutoPull()
	{
		if (this.TeleportList.Count > 0)
		{
			this.HandleTeleportList();
		}
	}

	public void HandleLiveHandling()
	{
		this.liveTime += Time.deltaTime;
		this.attackTime -= Time.deltaTime;
		if (this.liveTime > 5f && !this.IsPlayerWithinServerRange(this.TargetPlayer))
		{
			this.RemoveNavObjects();
			this.DespawnAll();
		}
		if (this.attackTime <= 0f)
		{
			this.HandleAttackTrigger();
			this.attackTime = 5f;
		}
	}

	public bool ServerUpdate()
	{
		bool flag = false;
		this.SetupTeleportList();
		for (int i = this.MinionEntities.Count - 1; i >= 0; i--)
		{
			if (this.MinionEntities[i] == null || !this.MinionEntities[i].IsAlive())
			{
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageBossEvent>().Setup(NetPackageBossEvent.BossEventTypes.RemoveMinion, this.BossGroupID, this.MinionEntities[i].entityId), false, -1, -1, -1, null, 192, false);
				this.MinionEntityIDs.Remove(this.MinionEntities[i].entityId);
				this.MinionEntities.RemoveAt(i);
			}
			else
			{
				flag = true;
			}
		}
		if (this.BossEntity != null && this.BossEntity.IsAlive())
		{
			flag = true;
		}
		if (!flag)
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageBossEvent>().Setup(NetPackageBossEvent.BossEventTypes.RemoveGroup, this.BossGroupID), false, -1, -1, -1, null, 192, false);
		}
		return !flag;
	}

	public void DespawnAll()
	{
		for (int i = this.MinionEntities.Count - 1; i >= 0; i--)
		{
			if (this.MinionEntities[i] != null && this.MinionEntities[i].IsAlive())
			{
				this.MinionEntities[i].ForceDespawn();
			}
		}
		if (this.BossEntity != null && this.BossEntity.IsAlive())
		{
			this.BossEntity.ForceDespawn();
		}
	}

	public void HandleAttackTrigger()
	{
		for (int i = this.MinionEntities.Count - 1; i >= 0; i--)
		{
			if (this.MinionEntities[i] != null && this.MinionEntities[i].IsAlive() && this.MinionEntities[i].GetAttackTarget() == null)
			{
				this.MinionEntities[i].SetAttackTarget(this.TargetPlayer, 60000);
			}
		}
		if (this.BossEntity != null && this.BossEntity.IsAlive() && this.BossEntity.GetAttackTarget() == null)
		{
			this.BossEntity.SetAttackTarget(this.TargetPlayer, 60000);
		}
	}

	public int BossGroupID = -1;

	public int BossEntityID = -1;

	public EntityAlive BossEntity;

	public List<int> MinionEntityIDs;

	public List<EntityAlive> MinionEntities;

	public EntityPlayer TargetPlayer;

	public BossGroup.BossGroupTypes CurrentGroupType;

	public string BossIcon = "";

	public string BossName = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public Bounds serverBounds;

	[PublicizedFrom(EAccessModifier.Private)]
	public Bounds bounds;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 EnteringSize = new Vector3(32f, 32f, 32f);

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 LeavingSize = new Vector3(200f, 200f, 200f);

	[PublicizedFrom(EAccessModifier.Private)]
	public static float autoPullDistance = 32f;

	[PublicizedFrom(EAccessModifier.Private)]
	public static int nextID = -1;

	public bool IsCurrent;

	public bool ReadyForRemove;

	public string pullSound = "twitch_pull";

	[PublicizedFrom(EAccessModifier.Private)]
	public float liveTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public float attackTime = 5f;

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityAlive ClosestEnemy;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<EntityAlive> TeleportList = new List<EntityAlive>();

	public enum BossGroupTypes
	{
		Standard,
		ImmortalBoss,
		ImmortalMinions,
		Specialized
	}
}
