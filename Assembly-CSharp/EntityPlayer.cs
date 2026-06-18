using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Platform;
using SandboxOptions;
using Twitch;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class EntityPlayer : EntityAlive
{
	public PersistentPlayerData PersistentPlayerData
	{
		get
		{
			PersistentPlayerList persistentPlayers = GameManager.Instance.persistentPlayers;
			if (persistentPlayers == null)
			{
				return null;
			}
			return persistentPlayers.GetPlayerDataFromEntityID(this.entityId);
		}
	}

	public string PlayerDisplayName
	{
		get
		{
			if (this.cachedPlayerName != null)
			{
				return this.cachedPlayerName.DisplayName;
			}
			if (this.PersistentPlayerData == null)
			{
				return null;
			}
			this.cachedPlayerName = this.PersistentPlayerData.PlayerName;
			return this.cachedPlayerName.DisplayName;
		}
	}

	public event EntityPlayer.OnPlayerTeleportDelegate PlayerTeleportedDelegates;

	public int CarryCapacity
	{
		get; [PublicizedFrom(EAccessModifier.Protected)]
		set;
	}

	public bool TwitchEnabled
	{
		get
		{
			return this.twitchEnabled;
		}
		set
		{
			if (value != this.twitchEnabled)
			{
				this.twitchEnabled = value;
				this.bPlayerTwitchChanged |= !this.isEntityRemote;
				if (TwitchManager.HasInstance && TwitchManager.Current.extensionManager != null)
				{
					TwitchManager.Current.extensionManager.TwitchEnabledChanged(this);
				}
			}
		}
	}

	public bool TwitchSafe
	{
		get
		{
			return this.twitchSafe;
		}
		set
		{
			if (value != this.twitchSafe)
			{
				this.twitchSafe = value;
				this.bPlayerTwitchChanged |= !this.isEntityRemote;
				if (this.twitchSafe)
				{
					this.Buffs.AddBuff("twitch_safe", -1, true, false, -1f);
					return;
				}
				this.Buffs.RemoveBuff("twitch_safe", -1, true);
			}
		}
	}

	public TwitchVoteLockTypes TwitchVoteLock
	{
		get
		{
			return this.twitchVoteLock;
		}
		set
		{
			if (value != this.twitchVoteLock)
			{
				this.twitchVoteLock = value;
				this.bPlayerTwitchChanged |= !this.isEntityRemote;
			}
		}
	}

	public bool TwitchVisionDisabled
	{
		get
		{
			return this.twitchVisionDisabled;
		}
		set
		{
			if (value != this.twitchVisionDisabled)
			{
				this.twitchVisionDisabled = value;
				this.bPlayerTwitchChanged |= !this.isEntityRemote;
			}
		}
	}

	public EntityPlayer.TwitchActionsStates TwitchActionsEnabled
	{
		get
		{
			return this.twitchActionsEnabled;
		}
		set
		{
			if (value != this.twitchActionsEnabled)
			{
				this.twitchActionsEnabled = value;
				this.bPlayerTwitchChanged |= !this.isEntityRemote;
			}
		}
	}

	public bool IsSpectator
	{
		get
		{
			return this.isSpectator;
		}
		set
		{
			this.isSpectator = value;
			this.isIgnoredByAI = this.isSpectator;
			this.SetVisible(this.bModelVisible);
			this.bPlayerStatsChanged |= !this.isEntityRemote;
		}
	}

	public bool IsFriendOfLocalPlayer
	{
		get
		{
			PersistentPlayerData persistentLocalPlayer = GameManager.Instance.persistentLocalPlayer;
			if (persistentLocalPlayer == null)
			{
				return false;
			}
			PersistentPlayerData playerDataFromEntityID = GameManager.Instance.persistentPlayers.GetPlayerDataFromEntityID(this.entityId);
			return playerDataFromEntityID != null && persistentLocalPlayer.IsAlly(playerDataFromEntityID);
		}
	}

	public Vector3i markerPosition
	{
		get
		{
			return this.m_MarkerPosition;
		}
		set
		{
			if (!this.isEntityRemote)
			{
				if (value.Equals(Vector3i.zero))
				{
					if (this.navMarker != null)
					{
						NavObjectManager.Instance.UnRegisterNavObject(this.navMarker);
						this.navMarker = null;
					}
				}
				else if (this.navMarker == null)
				{
					this.navMarker = NavObjectManager.Instance.RegisterNavObject("quick_waypoint", value.ToVector3(), "", this.navMarkerHidden, -1, null);
				}
				else
				{
					this.navMarker.TrackedPosition = value.ToVector3();
					this.navMarker.hiddenOnCompass = this.navMarkerHidden;
				}
				this.m_MarkerPosition = value;
			}
		}
	}

	public event QuestJournal_QuestEvent QuestAccepted;

	public event QuestJournal_QuestEvent QuestChanged;

	public event QuestJournal_QuestEvent QuestRemoved;

	public event QuestJournal_QuestSharedEvent SharedQuestAdded;

	public event QuestJournal_QuestSharedEvent SharedQuestRemoved;

	public void TriggerQuestAddedEvent(Quest _q)
	{
		QuestJournal_QuestEvent questAccepted = this.QuestAccepted;
		if (questAccepted == null)
		{
			return;
		}
		questAccepted(_q);
	}

	public void TriggerQuestChangedEvent(Quest _q)
	{
		QuestJournal_QuestEvent questChanged = this.QuestChanged;
		if (questChanged == null)
		{
			return;
		}
		questChanged(_q);
	}

	public void TriggerQuestRemovedEvent(Quest _q)
	{
		QuestJournal_QuestEvent questRemoved = this.QuestRemoved;
		if (questRemoved == null)
		{
			return;
		}
		questRemoved(_q);
	}

	public void TriggerSharedQuestAddedEvent(SharedQuestEntry _entry)
	{
		if (this.SharedQuestAdded != null)
		{
			this.SharedQuestAdded(_entry);
			return;
		}
		Log.Warning(string.Format("No SharedQuestAdded listeners! Player: {0}", this));
	}

	public void TriggerSharedQuestRemovedEvent(SharedQuestEntry _entry)
	{
		QuestJournal_QuestSharedEvent sharedQuestRemoved = this.SharedQuestRemoved;
		if (sharedQuestRemoved == null)
		{
			return;
		}
		sharedQuestRemoved(_entry);
	}

	public Vector3i RentedVMPosition
	{
		get
		{
			return this.m_rentedVMPosition;
		}
		set
		{
			if (!this.isEntityRemote)
			{
				if (value.Equals(Vector3i.zero))
				{
					if (this.navVending != null)
					{
						NavObjectManager.Instance.UnRegisterNavObject(this.navVending);
						this.navVending = null;
					}
				}
				else if (this.navVending == null)
				{
					this.navVending = NavObjectManager.Instance.RegisterNavObject("vending_machine", value.ToVector3(), "", false, -1, null);
				}
				else
				{
					this.navVending.TrackedPosition = value.ToVector3();
				}
				this.m_rentedVMPosition = value;
			}
		}
	}

	public bool IsAdmin
	{
		get
		{
			if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				return this.isAdmin;
			}
			if (!this.isEntityRemote)
			{
				return true;
			}
			ClientInfo clientInfo = SingletonMonoBehaviour<ConnectionManager>.Instance.Clients.ForEntityId(this.entityId);
			AdminTools adminTools = GameManager.Instance.adminTools;
			return ((adminTools != null) ? adminTools.Users.GetUserPermissionLevel(clientInfo) : 1000) == 0;
		}
		set
		{
			if (value != this.isAdmin)
			{
				this.isAdmin = value;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void Awake()
	{
		base.Awake();
		this.CarryCapacity = (int)EffectManager.GetValue(PassiveEffects.BagSize, null, 45f, this, null, default(FastTags<TagGroup.Global>), false, false, true, true, true, 1, true, false);
		this.bag = new Bag(this.CarryCapacity);
		this.Progression = new Progression(this);
		this.bWillRespawn = true;
	}

	public override void Init(int _entityClass, EntityInstanceAssets _assets, EModelInstanceAssets _eModelAssets)
	{
		this.gameStageBornAtWorldTime = ulong.MaxValue;
		if (this.playerProfile == null)
		{
			this.playerProfile = PlayerProfile.LoadLocalProfile();
		}
		this.Stealth.Init(this);
		this.alertEnabled = false;
		base.Init(_entityClass, _assets, _eModelAssets);
		if (ConsoleCmdCCPhysics.EnableCCPhysicsChanges && !(this is EntityPlayerLocal))
		{
			this.PhysicsTransform.gameObject.layer = 3;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void InitStats()
	{
		this.entityStats = new PlayerEntityStats(this);
		this.startOfFrameStats = new PlayerEntityStats(this);
	}

	public PlayerEntityStats PlayerStats
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return (PlayerEntityStats)this.entityStats;
		}
	}

	public override void CopyPropertiesFromEntityClass()
	{
		base.CopyPropertiesFromEntityClass();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void Start()
	{
		base.Start();
		this.SetVisible(this.Spawned);
	}

	public override void SetAlive()
	{
		bool flag = this.IsDead();
		base.SetAlive();
		if (flag)
		{
			long num = GameStageDefinition.DaysAliveChangeWhenKilled * 24000L;
			if (this.world.worldTime - this.gameStageBornAtWorldTime < (ulong)num)
			{
				this.gameStageBornAtWorldTime = this.world.worldTime;
				return;
			}
			this.gameStageBornAtWorldTime += (ulong)num;
		}
	}

	public override void SetDead()
	{
		base.SetDead();
		if (this.world.aiDirector != null)
		{
			this.IsBloodMoonDead = this.world.aiDirector.BloodMoonComponent.BloodMoonActive;
		}
	}

	public int unModifiedGameStage
	{
		get
		{
			float num = Mathf.Clamp((float)((this.world.worldTime - this.gameStageBornAtWorldTime) / 24000UL), 0f, (float)this.Progression.Level);
			float difficultyBonus = GameStageDefinition.DifficultyBonus;
			return Mathf.FloorToInt(EffectManager.GetValue(PassiveEffects.GameStage, null, ((float)this.Progression.Level + num) * difficultyBonus, this, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false));
		}
	}

	public int gameStage
	{
		get
		{
			float num = Mathf.Clamp((float)((this.world.worldTime - this.gameStageBornAtWorldTime) / 24000UL), 0f, (float)this.Progression.Level);
			float difficultyBonus = GameStageDefinition.DifficultyBonus;
			if (this.biomeStandingOn != null)
			{
				float num2 = 0f;
				float num3 = 0f;
				if (this.QuestJournal.ActiveQuest != null)
				{
					num2 = this.QuestJournal.ActiveQuest.QuestClass.GameStageMod;
					num3 = this.QuestJournal.ActiveQuest.QuestClass.GameStageBonus;
				}
				float num4 = this.biomeStandingOn.GameStageMod * EntityPlayer.BiomeGameStageModifier;
				float num5 = this.biomeStandingOn.GameStageBonus * EntityPlayer.BiomeGameStageModifier;
				return Mathf.FloorToInt(EffectManager.GetValue(PassiveEffects.GameStage, null, ((float)this.Progression.Level * (1f + num4 + num2) + num + num5 + num3) * difficultyBonus, this, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false) * EntityPlayer.GlobalGameStageModifier);
			}
			return Mathf.FloorToInt(EffectManager.GetValue(PassiveEffects.GameStage, null, ((float)this.Progression.Level + num) * difficultyBonus, this, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false) * EntityPlayer.GlobalGameStageModifier);
		}
	}

	public int GetTraderStage(int tier)
	{
		int num = Mathf.Max(0, tier - 1);
		float num2 = TraderManager.QuestTierMod[Mathf.Min(num, TraderManager.QuestTierMod.Length - 1)];
		return Mathf.FloorToInt(EffectManager.GetValue(PassiveEffects.TraderStage, null, (float)this.Progression.Level * (1f + num2), this, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false) * EntityPlayer.GlobalTraderStageModifier);
	}

	public int GetLootStage(float containerMod, float containerBonus)
	{
		float num = 0f;
		float num2 = 0f;
		if (this.prefab != null && this.prefab.prefab.DifficultyTier > 0)
		{
			int num3 = Mathf.Max(0, (int)(this.prefab.prefab.DifficultyTier - 1));
			num = LootManager.POITierMod[Mathf.Min(num3, LootManager.POITierMod.Length - 1)] * EntityPlayer.POITierLootStageModifier;
			num2 = LootManager.POITierBonus[Mathf.Min(num3, LootManager.POITierBonus.Length - 1)] * EntityPlayer.POITierLootStageModifier;
		}
		if (this.biomeStandingOn != null)
		{
			float num4 = this.biomeStandingOn.LootStageMod * EntityPlayer.BiomeLootStageModifier;
			float num5 = this.biomeStandingOn.LootStageBonus * EntityPlayer.BiomeLootStageModifier;
			int num6 = Mathf.FloorToInt(EffectManager.GetValue(PassiveEffects.LootStage, null, (float)this.Progression.Level * (1f + num + num4 + containerMod) + (num2 + num5 + containerBonus), this, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false));
			if (this.biomeStandingOn.LootStageMin != -1)
			{
				num6 = Utils.FastMax(num6, this.biomeStandingOn.LootStageMin);
			}
			if (GameStats.GetBool(EnumGameStats.BiomeProgression) && this.biomeStandingOn.LootStageMax != -1)
			{
				num6 = Utils.FastMin(num6, Mathf.FloorToInt(EffectManager.GetValue(PassiveEffects.LootStageMax, null, (float)this.biomeStandingOn.LootStageMax, this, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false)));
			}
			return num6;
		}
		return Mathf.FloorToInt(EffectManager.GetValue(PassiveEffects.LootStage, null, (float)this.Progression.Level * (1f + num + containerMod) + (num2 + containerBonus), this, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false));
	}

	public int GetHighestPartyLootStage(float containerMod, float containerBonus)
	{
		if (this.Party != null)
		{
			return this.Party.GetHighestLootStage(containerMod, containerBonus);
		}
		return this.GetLootStage(containerMod, containerBonus);
	}

	public int HighestPartyGameStage
	{
		get
		{
			if (this.Party != null)
			{
				return this.Party.HighestGameStage;
			}
			return this.gameStage;
		}
	}

	public int PartyGameStage
	{
		get
		{
			if (this.Party != null)
			{
				return this.Party.GameStage;
			}
			return this.gameStage;
		}
	}

	public void TurnOffLightFlares()
	{
		this.inventory.TurnOffLightFlares();
	}

	public override float GetSeeDistance()
	{
		return 80f;
	}

	public float DetectUsScale(EntityAlive _entity)
	{
		if (this.prefab != null && this.prefab.prefab.DifficultyTier >= 1 && Time.time - this.prefabTimeIn > 60f && _entity.GetSpawnerSource() == EnumSpawnerSource.Biome && _entity is EntityEnemy)
		{
			return 0.3f;
		}
		return 1f;
	}

	public override Vector3 getHeadPosition()
	{
		if (!(this.emodel != null) || !(this.emodel.GetHeadTransform() != null))
		{
			return base.transform.position + new Vector3(0f, base.height - 0.15f, 0f) + Origin.position;
		}
		return this.emodel.GetHeadTransform().position + Origin.position;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void Update()
	{
		base.Update();
		this.generalTags = this.MinEventContext.Tags;
		if (!GameManager.Instance.gameStateManager.IsGameStarted())
		{
			return;
		}
		float num = this.totalTimePlayed + Time.unscaledDeltaTime / 60f;
		if (this is EntityPlayerLocal)
		{
			int num2 = (int)this.totalTimePlayed;
			int num3 = (int)num;
			if (num2 != num3 && num3 % 60 == 0)
			{
				int num4 = num3 / 60;
				if (num4 < 301)
				{
					GameSparksCollector.SetValue(GameSparksCollector.GSDataKey.PlayerLevelAtHour, num4.ToString(), this.Progression.Level, true, GameSparksCollector.GSDataCollection.SessionUpdates);
				}
			}
		}
		this.totalTimePlayed = num;
		if (this.ChunkObserver != null)
		{
			this.ChunkObserver.SetPosition(base.GetPosition());
			if (this.ChunkObserver.mapDatabase != null && this.IsSpawned() && this.chunkPosAddedEntityTo != this.lastChunkPos)
			{
				this.lastChunkPos = this.chunkPosAddedEntityTo;
				this.ChunkObserver.mapDatabase.Add(this.chunkPosAddedEntityTo, this.world);
			}
		}
		if (this.emodel.avatarController != null)
		{
			this.emodel.avatarController.SetHeadAngles(this.rotation.x, 0f);
			if (this.inventory.holdingItem != null && this.inventory.holdingItem.CanHold())
			{
				this.emodel.avatarController.SetArmsAngles(this.rotation.x + 90f, 0f);
			}
			else
			{
				this.emodel.avatarController.SetArmsAngles(0f, 0f);
			}
		}
		if (!this.IsDead())
		{
			this.currentLife += Time.deltaTime / 60f;
			if (this.currentLife > this.longestLife)
			{
				this.longestLife = this.currentLife;
				if ((int)this.longestLife > this.longestLifeLived)
				{
					this.longestLifeLived = (int)this.longestLife;
					if (this is EntityPlayerLocal)
					{
						QuestEventManager.Current.TimeSurvived((float)this.longestLifeLived);
						IAchievementManager achievementManager = PlatformManager.NativePlatform.AchievementManager;
						if (achievementManager != null)
						{
							achievementManager.SetAchievementStat(EnumAchievementDataStat.LongestLifeLived, this.longestLifeLived);
						}
					}
				}
			}
		}
		this.HasUpdated = true;
	}

	public override float GetSpeedModifier()
	{
		float num;
		float num2;
		if (base.IsCrouching)
		{
			if (this.MovementRunning)
			{
				num = Constants.cPlayerSpeedModifierWalking;
				num2 = EffectManager.GetValue(PassiveEffects.WalkSpeed, null, num, this, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false);
				num2 *= SandboxOptionManager.GetFloat(global::SandboxOptions.SandboxOptions.WalkSpeed);
			}
			else
			{
				num = Constants.cPlayerSpeedModifierCrouching;
				num2 = EffectManager.GetValue(PassiveEffects.CrouchSpeed, null, num, this, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false);
				num2 *= SandboxOptionManager.GetFloat(global::SandboxOptions.SandboxOptions.CrouchSpeed);
			}
		}
		else if (this.MovementRunning)
		{
			num = Constants.cPlayerSpeedModifierRunning;
			num2 = EffectManager.GetValue(PassiveEffects.RunSpeed, null, num, this, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false);
			num2 *= SandboxOptionManager.GetFloat(global::SandboxOptions.SandboxOptions.RunSpeed);
		}
		else
		{
			num = Constants.cPlayerSpeedModifierWalking;
			num2 = EffectManager.GetValue(PassiveEffects.WalkSpeed, null, num, this, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false);
			num2 *= SandboxOptionManager.GetFloat(global::SandboxOptions.SandboxOptions.WalkSpeed);
		}
		num *= 0.35f;
		if (num2 < num)
		{
			num2 = num;
		}
		return num2;
	}

	public override float MaxVelocity
	{
		get
		{
			if (this.MovementRunning)
			{
				return 0.35f;
			}
			return 0.17999f;
		}
	}

	public override Vector3 GetVelocityPerSecond()
	{
		if (this.AttachedToEntity)
		{
			return this.AttachedToEntity.GetVelocityPerSecond();
		}
		return this.averageVel * 20f;
	}

	public Color GetTeamColor()
	{
		return Constants.cTeamColors[this.TeamNumber];
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void StartJumpMotion()
	{
		base.StartJumpMotion();
		this.motion.y = EffectManager.GetValue(PassiveEffects.JumpStrength, null, this.jumpStrength, this, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false) * base.Stats.Stamina.ValuePercent * SandboxOptionManager.GetFloat(global::SandboxOptions.SandboxOptions.JumpStrength);
	}

	public override void OnUpdateLive()
	{
		base.Stats.Stamina.RegenerationAmount = 0f;
		base.OnUpdateLive();
		base.GetEntitySenses().Clear();
		this.CheckSleeperTriggers();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void CheckSleeperTriggers()
	{
		if (!this.world.IsRemote() && base.IsAlive())
		{
			this.world.CheckSleeperVolumeTouching(this);
			this.world.CheckTriggerVolumeTrigger(this);
		}
	}

	public override int DamageEntity(DamageSource _damageSource, int _strength, bool _criticalHit, float _impulseScale = 1f)
	{
		if (GameStats.GetBool(EnumGameStats.IsPlayerDamageEnabled) && ItemActionAttack.IncomingDamageModifier > 0f)
		{
			return base.DamageEntity(_damageSource, _strength, _criticalHit, _impulseScale);
		}
		return 0;
	}

	public override void CheckDismember(ref DamageResponse _dmResponse, float damagePer)
	{
	}

	public override void PlayOneShot(string clipName, bool sound_in_head = false, bool serverSignalOnly = false, bool isUnique = false, AnimationEvent _animEvent = null, float volumeScale = 1f)
	{
		if (!this.isSpectator || sound_in_head)
		{
			base.PlayOneShot(clipName, sound_in_head, serverSignalOnly, isUnique, _animEvent, volumeScale);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string GetSoundHurt(DamageSource _damageSource, int _damageStrength)
	{
		string soundDrownPain;
		if (_damageSource.GetDamageType() == EnumDamageTypes.Suffocation && (soundDrownPain = base.GetSoundDrownPain()) != null)
		{
			return soundDrownPain;
		}
		if (_damageStrength > 15 || base.GetSoundHurtSmall() == null)
		{
			return base.GetSoundHurt();
		}
		return base.GetSoundHurtSmall();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string GetSoundDeath(DamageSource _damageSource)
	{
		if (this.soundDrownDeath == null || _damageSource.GetDamageType() != EnumDamageTypes.Suffocation)
		{
			return base.GetSoundDeath(_damageSource);
		}
		return this.soundDrownDeath;
	}

	public bool CanHeal()
	{
		return this.Health > 0 && this.Health < this.GetMaxHealth();
	}

	public override bool IsSavedToFile()
	{
		return false;
	}

	public override bool IsSavedToNetwork()
	{
		return false;
	}

	public virtual void EnableCamera(bool _b)
	{
	}

	public virtual void Respawn(RespawnType _reason)
	{
		this.lastRespawnReason = _reason;
		this.emodel.DisableRagdoll(true);
		this.InitBreadcrumbs();
	}

	public virtual void Teleport(Vector3 _pos, float _dir = -3.4028235E+38f)
	{
		if (this.AttachedToEntity)
		{
			this.AttachedToEntity.SetPosition(_pos, true);
		}
		else
		{
			this.SetPosition(_pos, true);
			if (_dir > -999999f)
			{
				this.SetRotation(new Vector3(0f, _dir, 0f));
			}
		}
		GameEventManager.Current.HandleForceBossDespawn(this);
		this.Respawn(RespawnType.Teleport);
	}

	public void InvokeTeleportDelegates()
	{
		EntityPlayer.OnPlayerTeleportDelegate playerTeleportedDelegates = this.PlayerTeleportedDelegates;
		if (playerTeleportedDelegates == null)
		{
			return;
		}
		playerTeleportedDelegates();
	}

	public virtual void BeforePlayerRespawn(RespawnType _type)
	{
	}

	public virtual void AfterPlayerRespawn(RespawnType _type)
	{
		this.InvokeTeleportDelegates();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void onSpawnStateChanged()
	{
		base.onSpawnStateChanged();
		this.SetVisible(this.Spawned);
		if (this.Spawned)
		{
			this.SpawnedTicks = 0;
			switch (this.lastRespawnReason)
			{
			case RespawnType.NewGame:
			case RespawnType.Died:
			case RespawnType.EnterMultiplayer:
				if (!this.world.IsRemote() && !this.world.IsEditor() && this.IsSafeZoneActive())
				{
					this.world.LockAreaMasterChunksAround(World.worldToBlockPos(base.GetPosition()), this.world.worldTime + (ulong)((long)(GamePrefs.GetInt(EnumGamePrefs.PlayerSafeZoneHours) * 1000)));
				}
				break;
			}
			if (this.lastRespawnReason != RespawnType.Teleport)
			{
				this.lastRespawnReason = RespawnType.Unknown;
			}
		}
	}

	public override int AttachToEntity(Entity _other, int slot = -1)
	{
		slot = base.AttachToEntity(_other, slot);
		if (slot >= 0)
		{
			Transform modelTransformParent = this.emodel.GetModelTransformParent();
			this.attachedModelPos = modelTransformParent.localPosition;
			modelTransformParent.localPosition = Vector3.zero;
		}
		return slot;
	}

	public override void Detach()
	{
		base.Detach();
		this.emodel.GetModelTransformParent().localPosition = this.attachedModelPos;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void onNewPrefabEntered(PrefabInstance _prefabInstance)
	{
		if (_prefabInstance == null)
		{
			return;
		}
		if (_prefabInstance.prefab.bTraderArea)
		{
			EntityPlayerLocal entityPlayerLocal = this as EntityPlayerLocal;
			if (entityPlayerLocal != null)
			{
				Waypoint waypoint = new Waypoint();
				waypoint.pos = World.worldToBlockPos(_prefabInstance.boundingBoxPosition + _prefabInstance.boundingBoxSize / 2);
				waypoint.icon = "ui_game_symbol_map_trader";
				waypoint.name.Update(_prefabInstance.prefab.PrefabName, PlatformManager.MultiPlatform.User.PlatformUserId);
				waypoint.ownerId = null;
				waypoint.lastKnownPositionEntityId = -1;
				waypoint.bIsAutoWaypoint = true;
				waypoint.bUsingLocalizationId = true;
				if (!entityPlayerLocal.Waypoints.ContainsWaypoint(waypoint))
				{
					NavObject navObject = NavObjectManager.Instance.RegisterNavObject("waypoint", waypoint.pos, waypoint.icon, true, -1, null);
					navObject.UseOverrideColor = true;
					navObject.OverrideColor = Color.white;
					navObject.IsActive = false;
					navObject.name = waypoint.name.Text;
					navObject.usingLocalizationId = true;
					waypoint.navObject = navObject;
					entityPlayerLocal.Waypoints.Collection.Add(waypoint);
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void StartJumpSwimMotion()
	{
		this.motion.y = this.motion.y + 0.04f;
	}

	public override bool IsImmuneToLegDamage
	{
		get
		{
			return false;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool isDetailedHeadBodyColliders()
	{
		return true;
	}

	public override int GetLayerForMapIcon()
	{
		return 19;
	}

	public override bool CanMapIconBeSelected()
	{
		return GameStats.GetBool(EnumGameStats.IsSpawnNearOtherPlayer);
	}

	public override bool IsDrawMapIcon()
	{
		return base.IsSpawned() && ((this.IsFriendOfLocalPlayer && GameStats.GetBool(EnumGameStats.ShowFriendPlayerOnMap)) || GameStats.GetBool(EnumGameStats.ShowAllPlayersOnMap) || this.IsInPartyOfLocalPlayer);
	}

	public override Color GetMapIconColor()
	{
		return Color.white;
	}

	public override Vector3 GetMapIconScale()
	{
		return new Vector3(1.5f, 1.5f, 1.5f);
	}

	public override bool IsAlert
	{
		get
		{
			return true;
		}
	}

	public override bool IsClientControlled()
	{
		return true;
	}

	public bool IsFriendsWith(EntityPlayer _other)
	{
		PersistentPlayerData playerDataFromEntityID = GameManager.Instance.persistentPlayers.GetPlayerDataFromEntityID(this.entityId);
		PersistentPlayerData playerDataFromEntityID2 = GameManager.Instance.persistentPlayers.GetPlayerDataFromEntityID(_other.entityId);
		return playerDataFromEntityID2 != null && playerDataFromEntityID2.IsAlly(playerDataFromEntityID);
	}

	public bool IsSafeZoneActive()
	{
		return this.Progression.Level <= GamePrefs.GetInt(EnumGamePrefs.PlayerSafeZoneLevel) && this.spawnPoints.Count == 0;
	}

	public override void OnEntityUnload()
	{
		if (!this.world.IsEditor() && this.prefab != null)
		{
			this.world.triggerManager.RemovePlayer(this.prefab, this.entityId);
		}
		base.OnEntityUnload();
		this.ChunkObserver = null;
	}

	public override void OnUpdateEntity()
	{
		base.OnUpdateEntity();
		this.SpawnedTicks++;
		Vector3 vector = this.position - this.averagVelLastPos;
		this.averagVelLastPos = this.position;
		if (vector.sqrMagnitude < 25f)
		{
			this.averageVel = this.averageVel * 0.7f + vector * 0.3f;
		}
		if (this.Health <= 0)
		{
			this.lastRespawnReason = RespawnType.Died;
			List<Transform> list = new List<Transform>();
			GameUtils.FindDeepChildWithPartialName(base.transform, "temp_Projectile", ref list);
			for (int i = 0; i < list.Count; i++)
			{
				Object.Destroy(list[i].gameObject);
			}
		}
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			Vector3 position = this.position;
			if ((position - this.breadcrumbLastPos).sqrMagnitude >= 0.9025f)
			{
				this.breadcrumbLastPos = position;
				this.breadcrumbIndex = (this.breadcrumbIndex + 1) & 31;
				this.breadcrumbs[this.breadcrumbIndex] = position;
			}
			this.Stealth.TickServer();
		}
		else if (!this.isEntityRemote)
		{
			this.Stealth.TickLocalClient();
		}
		if (this.bag != null)
		{
			this.CarryCapacity = (int)EffectManager.GetValue(PassiveEffects.CarryCapacity, null, 0f, this, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false);
			int num = this.bag.GetUsedSlotCount() - this.CarryCapacity;
			this.Buffs.SetCustomVar("_carrycapacity", (float)this.CarryCapacity, true, CVarOperation.set);
			this.Buffs.SetCustomVar("_encumbrance", Mathf.Max((float)num, 0f) / (float)(this.bag.GetSlots().Length - this.CarryCapacity), true, CVarOperation.set);
			this.Buffs.SetCustomVar("_encumberedslots", Mathf.Max((float)num, 0f), true, CVarOperation.set);
		}
		this.PrefabTick();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void PrefabTick()
	{
		if (Time.time - this.lastTimePrefabChecked > 1f)
		{
			this.lastTimePrefabChecked = Time.time;
			PrefabInstance poiatPosition = this.world.GetPOIAtPosition(this.position, null, null);
			if (poiatPosition != this.prefab)
			{
				if (!this.world.IsEditor())
				{
					if (this.prefab != null)
					{
						this.world.triggerManager.RemovePlayer(this.prefab, this.entityId);
					}
					if (poiatPosition != null)
					{
						this.world.triggerManager.AddPrefabData(poiatPosition, this.entityId);
					}
				}
				this.prefab = poiatPosition;
				this.prefabTimeIn = Time.time;
				this.prefabInfoEntered = false;
				this.onNewPrefabEntered(this.prefab);
			}
			if (this.prefab != null && !this.prefabInfoEntered && !this.world.IsEditor())
			{
				if (this is EntityPlayerLocal)
				{
					if (this.prefab.IsWithinInfoArea(this.position))
					{
						if (this.prefab.prefab.InfoVolumeList.Count > 0 || this.prefab.prefab.DifficultyTier >= 0)
						{
							this.enteredPrefab = this.prefab;
						}
						this.prefabInfoEntered = true;
					}
				}
				else
				{
					this.prefabInfoEntered = true;
				}
			}
			Vector3i blockPosition = base.GetBlockPosition();
			this.IsInTrader = this.world.GetTraderAreaAt(blockPosition) != null;
			if (this.TwitchEnabled || this.HasTwitchMember())
			{
				this.TwitchSafe = !this.world.CanPlaceBlockAt(blockPosition, null, false) || this.IsInTrader;
				return;
			}
			if (this.twitchSafe)
			{
				this.TwitchSafe = false;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void InitBreadcrumbs()
	{
		this.breadcrumbs.Fill(this.position);
	}

	public Vector3 GetBreadcrumbPos(float distance)
	{
		int num = (int)(distance + 0.5f);
		int num2 = this.breadcrumbIndex;
		if (num >= 31)
		{
			num2++;
		}
		else
		{
			num2 -= num;
		}
		return this.breadcrumbs[num2 & 31];
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void updateStepSound(float _distX, float _distZ, float _rotYDelta)
	{
		if (this is EntityPlayerLocal && !this.isSpectator)
		{
			base.updateStepSound(_distX, _distZ, _rotYDelta);
		}
	}

	public override float GetBlockDamageScale(bool isTerrain)
	{
		if (!isTerrain)
		{
			return ItemActionAttack.BlockDamagePercent;
		}
		return ItemActionAttack.TerrainDamagePercent;
	}

	public override void SetDamagedTarget(EntityAlive _attackTarget)
	{
		base.SetDamagedTarget(_attackTarget);
		if (_attackTarget is EntityEnemy)
		{
			this.LastZombieAttackTime = this.world.worldTime;
		}
		this.IsBloodMoonDead = false;
	}

	public override void VisiblityCheck(float _distanceSqr, bool _masterIsZooming)
	{
		if (!this.Spawned)
		{
			return;
		}
		int num = this.visiblityCheckTicks - 1;
		this.visiblityCheckTicks = num;
		if (num > 0)
		{
			return;
		}
		this.visiblityCheckTicks = 5;
		int num2 = Utils.FastMin(12, GameUtils.GetViewDistance()) * 16;
		num2--;
		this.bModelVisible = _distanceSqr < (float)(num2 * num2);
		if (!this.IsDead() && base.GetDeathTime() == 0)
		{
			this.SetVisible(this.bModelVisible);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetVisible(bool _isVisible)
	{
		if (this.isSpectator)
		{
			this.emodel.SetVisible(false, false);
			return;
		}
		this.emodel.SetVisible(_isVisible, !this.world.IsRemote());
	}

	public override void Kill(DamageResponse _dmResponse)
	{
		base.Kill(_dmResponse);
		this.currentLife = 0f;
	}

	public virtual void OnHUD()
	{
	}

	public void ServerNetSendRangeCheckedDamage(Vector3 _origin, float _maxRange, DamageSourceEntity _damageSource, int _strength, bool _isCritical, List<string> _buffActions, string _buffActionsContext, ParticleEffect particleEffect)
	{
		NetPackageRangeCheckDamageEntity netPackageRangeCheckDamageEntity = NetPackageManager.GetPackage<NetPackageRangeCheckDamageEntity>().Setup(this.entityId, _origin, _maxRange, _damageSource, _strength, _isCritical, _buffActions, _buffActionsContext, particleEffect);
		SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(netPackageRangeCheckDamageEntity, false, this.entityId, -1, -1, null, 192, false);
	}

	public override AttachedToEntitySlotExit FindValidExitPosition(List<AttachedToEntitySlotExit> candidatePositions)
	{
		this.lastVehiclePositionOnDismount = this.position;
		this.timeOfVehicleDismount = Time.time;
		this.forcedDetach = false;
		return base.FindValidExitPosition(candidatePositions);
	}

	public override void CheckPosition()
	{
		base.CheckPosition();
		if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			return;
		}
		if (this.IsFlyMode.Value || !this.Spawned)
		{
			return;
		}
		if (this.position.y >= 0f)
		{
			return;
		}
		if (this.AttachedToEntity != null)
		{
			this.Detach();
			this.forcedDetach = true;
			return;
		}
		Log.Out(string.Format("[FELLTHROUGHWORLD] Player is under the world, starting teleport respawn from {0}", this.position));
		Vector3 fallingSavePosition = this.GetFallingSavePosition();
		if (this.isEntityRemote)
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(fallingSavePosition, null, true), false, this.entityId, -1, -1, null, 192, false);
			return;
		}
		Log.Out(string.Format("[FELLTHROUGHWORLD] Attempting teleport to {0}", fallingSavePosition));
		this.Teleport(fallingSavePosition, float.MinValue);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 GetFallingSavePosition()
	{
		if (!this.forcedDetach && Time.time - this.timeOfVehicleDismount < this.vehicleTeleportThresholdSeconds)
		{
			return this.lastVehiclePositionOnDismount;
		}
		Vector3 position = this.position;
		IChunk chunkFromWorldPos = this.world.GetChunkFromWorldPos((int)position.x, (int)position.z);
		if (chunkFromWorldPos == null || chunkFromWorldPos.IsEmpty())
		{
			Log.Out(string.Format("[FELLTHROUGHWORLD] GetFallingSavePosition - CurrentChunk {0}", chunkFromWorldPos));
			IChunk chunk = null;
			Vector2 vector;
			vector..ctor(position.x, position.z);
			float num = float.PositiveInfinity;
			foreach (long num2 in this.ChunkObserver.chunksAround.list)
			{
				IChunk chunkSync = this.world.GetChunkSync(num2);
				if (chunkSync != null && !chunkSync.IsEmpty())
				{
					Vector3i worldPos = chunkSync.GetWorldPos();
					float sqrMagnitude = (new Vector2((float)worldPos.x + 8f, (float)worldPos.z + 8f) - vector).sqrMagnitude;
					if (chunk == null || sqrMagnitude < num)
					{
						chunk = chunkSync;
						num = sqrMagnitude;
					}
				}
			}
			Log.Out(string.Format("[FELLTHROUGHWORLD] GetFallingSavePosition - closestChunk {0}", chunk));
			if (chunk != null)
			{
				Vector3i worldPos2 = chunk.GetWorldPos();
				position.x = Math.Clamp(position.x, (float)worldPos2.x + 0.5f, (float)(worldPos2.x + 16) - 1f);
				position.z = Math.Clamp(position.z, (float)worldPos2.z + 0.5f, (float)(worldPos2.z + 16) - 1f);
			}
		}
		position.y = (float)GameManager.Instance.World.GetTerrainHeight((int)position.x, (int)position.z) + 0.5f;
		return position;
	}

	public override bool FriendlyFireCheck(EntityAlive other)
	{
		bool flag = true;
		try
		{
			EntityPlayer entityPlayer = other as EntityPlayer;
			if (entityPlayer != null)
			{
				if (this.entityId == entityPlayer.entityId)
				{
					return true;
				}
				int @int = GameStats.GetInt(EnumGameStats.PlayerKillingMode);
				if (@int != 0)
				{
					if (@int - 1 <= 1)
					{
						PersistentPlayerData playerDataFromEntityID = GameManager.Instance.persistentPlayers.GetPlayerDataFromEntityID(this.entityId);
						PersistentPlayerData playerDataFromEntityID2 = GameManager.Instance.persistentPlayers.GetPlayerDataFromEntityID(entityPlayer.entityId);
						if (playerDataFromEntityID != null && playerDataFromEntityID2 != null)
						{
							bool flag2 = playerDataFromEntityID2.IsAlly(playerDataFromEntityID);
							bool flag3 = this.Party != null && this.Party.MemberList.Contains(entityPlayer);
							flag = (flag2 || flag3) ^ (@int == 2);
						}
					}
				}
				else
				{
					flag = false;
				}
			}
		}
		catch
		{
			flag = true;
		}
		return flag;
	}

	public Party Party
	{
		get
		{
			return this.party;
		}
		set
		{
			if (this.party != null && value == null && this is EntityPlayerLocal)
			{
				this.party.ClearAllNavObjectColors();
			}
			this.party = value;
			if (this.party == null && this is EntityPlayerLocal)
			{
				this.QuestJournal.RemoveAllSharedQuests();
			}
		}
	}

	public CompanionGroup Companions
	{
		get
		{
			if (this.companions == null)
			{
				this.companions = new CompanionGroup();
			}
			return this.companions;
		}
	}

	public bool IsInParty()
	{
		return this.Party != null;
	}

	public bool IsPartyLead()
	{
		return this.Party != null && this.Party.Leader == this;
	}

	public bool HasTwitchMember()
	{
		return this.Party != null && this.Party.HasTwitchMember;
	}

	public TwitchVoteLockTypes HasTwitchVoteLockMember()
	{
		if (this.Party != null)
		{
			return this.Party.HasTwitchVoteLock;
		}
		return TwitchVoteLockTypes.None;
	}

	public void CreateParty()
	{
		this.Party = new Party();
		this.Party.AddPlayer(this);
		this.Party.LeaderIndex = 0;
		this.HandleOnPartyJoined();
	}

	public void LeaveParty()
	{
		Party party = this.Party;
		if (this.Party != null)
		{
			this.Party.MemberList.Remove(this);
			if (this is EntityPlayerLocal)
			{
				for (int i = 0; i < this.Party.MemberList.Count; i++)
				{
					if (this.Party.MemberList[i].NavObject != null)
					{
						this.Party.MemberList[i].NavObject.UseOverrideColor = false;
					}
				}
			}
		}
		this.Party = null;
		this.HandleOnPartyLeave(party);
	}

	public event OnPartyChanged PartyJoined;

	public event OnPartyChanged PartyChanged;

	public event OnPartyChanged PartyLeave;

	public event OnPartyChanged InvitedToParty;

	public void RemovePartyInvite(int playerEntityID)
	{
		EntityPlayer entityPlayer = GameManager.Instance.World.GetEntity(playerEntityID) as EntityPlayer;
		if (this.partyInvites.Contains(entityPlayer))
		{
			this.partyInvites.Remove(entityPlayer);
		}
	}

	public void RemoveAllPartyInvites()
	{
		this.partyInvites.Clear();
	}

	public void AddPartyInvite(int playerEntityID)
	{
		EntityPlayer entityPlayer = GameManager.Instance.World.GetEntity(playerEntityID) as EntityPlayer;
		if (!this.partyInvites.Contains(entityPlayer))
		{
			this.partyInvites.Add(entityPlayer);
			if (this.InvitedToParty != null)
			{
				this.InvitedToParty(null, this);
			}
		}
	}

	public bool HasPendingPartyInvite(int playerEntityID)
	{
		EntityPlayer entityPlayer = GameManager.Instance.World.GetEntity(playerEntityID) as EntityPlayer;
		return this.partyInvites.Contains(entityPlayer);
	}

	public void HandleOnPartyJoined()
	{
		OnPartyChanged partyJoined = this.PartyJoined;
		if (partyJoined == null)
		{
			return;
		}
		partyJoined(this.party, this);
	}

	public void HandleOnPartyChanged()
	{
		OnPartyChanged partyChanged = this.PartyChanged;
		if (partyChanged == null)
		{
			return;
		}
		partyChanged(this.party, this);
	}

	public void HandleOnPartyLeave(Party _oldParty)
	{
		OnPartyChanged partyLeave = this.PartyLeave;
		if (partyLeave == null)
		{
			return;
		}
		partyLeave(_oldParty, this);
	}

	public void PartyDisconnect()
	{
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			Party.ServerHandleDisconnectParty(this);
		}
	}

	public void SetPrefabsAroundNear(Dictionary<int, PrefabInstance> _prefabsAround)
	{
		this.prefabsAroundNear.Clear();
		foreach (KeyValuePair<int, PrefabInstance> keyValuePair in _prefabsAround)
		{
			this.prefabsAroundNear.Add(keyValuePair.Key, keyValuePair.Value);
		}
	}

	public Dictionary<int, PrefabInstance> GetPrefabsAroundNear()
	{
		return this.prefabsAroundNear;
	}

	public void AddKillXP(EntityAlive killedEntity, ItemValue itemUsed, float xpModifier = 1f)
	{
		int num = EntityClass.list[killedEntity.entityClass].ExperienceValue;
		num = (int)EffectManager.GetValue(PassiveEffects.ExperienceGain, killedEntity.inventory.holdingItemItemValue, (float)num, killedEntity, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false);
		if (xpModifier != 1f)
		{
			num = (int)((float)num * xpModifier + 0.5f);
		}
		if (this.IsInParty())
		{
			num = this.Party.GetPartyXP(this, num);
		}
		if (!this.isEntityRemote)
		{
			this.Progression.AddLevelExp(num, "_xpFromKill", Progression.XPTypes.Kill, true, true, this.entityId, itemUsed);
			this.bPlayerStatsChanged = true;
		}
		else
		{
			NetPackageEntityAddExpClient netPackageEntityAddExpClient = NetPackageManager.GetPackage<NetPackageEntityAddExpClient>().Setup(this.entityId, num, Progression.XPTypes.Kill, itemUsed);
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(netPackageEntityAddExpClient, false, this.entityId, -1, -1, null, 192, false);
		}
		if (xpModifier == 1f)
		{
			GameManager.Instance.SharedKillServer(killedEntity.entityId, this.entityId, xpModifier);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void HandleClientDeath(Vector3i attackPos)
	{
		base.HandleClientDeath(attackPos);
		TwitchManager.Current.CheckKiller(this, this.entityThatKilledMe, attackPos);
		switch (GameStats.GetInt(EnumGameStats.DeathPenalty))
		{
		case 0:
			GameEventManager.Current.HandleAction("game_on_death_none", this, this, false, "", "", false, true, "", null);
			return;
		case 1:
			GameEventManager.Current.HandleAction("game_on_death_default", this, this, false, "", "", false, true, "", null);
			return;
		case 2:
			GameEventManager.Current.HandleAction("game_on_death_injured", this, this, false, "", "", false, true, "", null);
			return;
		case 3:
			GameEventManager.Current.HandleAction("game_on_death_permanent", this, this, false, "", "", false, true, "", null);
			return;
		default:
			return;
		}
	}

	public void HandleTwitchActionsTempEnabled(EntityPlayer.TwitchActionsStates newState)
	{
		if (this.twitchActionsEnabled == EntityPlayer.TwitchActionsStates.Disabled)
		{
			return;
		}
		this.TwitchActionsEnabled = newState;
	}

	public bool IsReloadCancelled()
	{
		if (this.inventory.holdingItemData.actionData != null)
		{
			foreach (ItemActionData itemActionData in this.inventory.holdingItemData.actionData)
			{
				ItemActionRanged.ItemActionDataRanged itemActionDataRanged = itemActionData as ItemActionRanged.ItemActionDataRanged;
				if (itemActionDataRanged != null && itemActionDataRanged.isReloadCancelled)
				{
					return true;
				}
			}
			return false;
		}
		return false;
	}

	public void SetLaserSightData(bool _laserSightActive, Vector3 _laserSightPosition)
	{
		if (_laserSightActive == this.laserSightActive && (this.laserSightPosition - _laserSightPosition).sqrMagnitude < 0.0016f)
		{
			return;
		}
		this.laserSightPosition = _laserSightPosition;
		this.laserSightActive = _laserSightActive;
		if (this.world.entityDistributer != null)
		{
			this.world.entityDistributer.SendPacketToTrackedPlayers(this.entityId, (this.world.GetPrimaryPlayer() != null) ? this.world.GetPrimaryPlayer().entityId : (-1), NetPackageManager.GetPackage<NetPackagePlayerLaserSight>().Setup(this.entityId, _laserSightActive, _laserSightPosition), true);
			return;
		}
		SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackagePlayerLaserSight>().Setup(this.entityId, _laserSightActive, _laserSightPosition), false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static readonly FastTags<TagGroup.Global> STAMINA_LOSS_TAGS = FastTags<TagGroup.Global>.GetTag("Athletics");

	public float jumpStrength = 0.451f;

	public SpawnPosition lastSpawnPosition = SpawnPosition.Undef;

	public PlayerProfile playerProfile;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public PersistentPlayerName cachedPlayerName;

	public static float EncumbranceModifier = 1f;

	public static float GlobalGameStageModifier = 1f;

	public static float BiomeGameStageModifier = 1f;

	public static float GlobalLootStageModifier = 1f;

	public static float BiomeLootStageModifier = 1f;

	public static float POITierLootStageModifier = 1f;

	public static float GlobalTraderStageModifier = 1f;

	public static float FallDamageModifier = 1f;

	public List<EntityAlive> aiClosest = new List<EntityAlive>();

	public AIDirectorBloodMoonParty bloodMoonParty;

	public bool IsBloodMoonDead;

	public PlayerStealth Stealth;

	public const long cSpawnPointKeyInvalid = -1L;

	public long selectedSpawnPointKey = -1L;

	public ulong LastZombieAttackTime;

	public bool IsInPartyOfLocalPlayer;

	public uint totalItemsCrafted;

	public float longestLife;

	public float currentLife;

	public float totalTimePlayed;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int longestLifeLived;

	public ChunkManager.ChunkObserver ChunkObserver;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public RespawnType lastRespawnReason = RespawnType.Unknown;

	public int SpawnedTicks;

	public ulong gameStageBornAtWorldTime;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3i m_MarkerPosition = Vector3i.zero;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public NavObject navMarker;

	public bool navMarkerHidden;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public NavObject navVending;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public float lastTimePrefabChecked;

	public PrefabInstance prefab;

	public PrefabInstance enteredPrefab;

	public bool prefabInfoEntered;

	public float prefabTimeIn;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool bModelVisible = true;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public bool twitchEnabled;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public bool twitchSafe;

	public bool IsInTrader;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public TwitchVoteLockTypes twitchVoteLock;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public bool twitchVisionDisabled;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public EntityPlayer.TwitchActionsStates twitchActionsEnabled = EntityPlayer.TwitchActionsStates.Enabled;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public bool isSpectator;

	public WaypointCollection Waypoints = new WaypointCollection();

	public List<Waypoint> WaypointInvites = new List<Waypoint>();

	public QuestJournal QuestJournal = new QuestJournal();

	public List<ushort> favoriteCreativeStacks = new List<ushort>();

	public List<string> favoriteShapes = new List<string>();

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3i m_rentedVMPosition = Vector3i.zero;

	public ulong RentalEndTime;

	public int RentalEndDay;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3 averageVel;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3 averagVelLastPos;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const int cBreadcrumbMask = 31;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3[] breadcrumbs = new Vector3[32];

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3 breadcrumbLastPos;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int breadcrumbIndex;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public bool isAdmin;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3i lastChunkPos = new Vector3i(int.MinValue, int.MinValue, int.MinValue);

	public bool HasUpdated;

	public FastTags<TagGroup.Global> generalTags = FastTags<TagGroup.Global>.none;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3 attachedModelPos;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int visiblityCheckTicks;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3 lastVehiclePositionOnDismount = Vector3.zero;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float timeOfVehicleDismount;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float vehicleTeleportThresholdSeconds = 10f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool forcedDetach;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Party party;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public CompanionGroup companions;

	public List<EntityPlayer> partyInvites = new List<EntityPlayer>();

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Dictionary<int, PrefabInstance> prefabsAroundNear = new Dictionary<int, PrefabInstance>();

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool laserSightActive;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3 laserSightPosition;

	public delegate void OnPlayerTeleportDelegate();

	public enum TwitchActionsStates
	{
		Disabled,
		Enabled,
		TempDisabled,
		TempDisabledEnding
	}
}
