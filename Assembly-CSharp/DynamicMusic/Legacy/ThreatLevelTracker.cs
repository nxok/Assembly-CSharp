using System;
using System.Collections.Generic;
using MusicUtils.Enums;
using UnityEngine;

namespace DynamicMusic.Legacy
{
	public class ThreatLevelTracker
	{
		public int DeadEnemies
		{
			get; [PublicizedFrom(EAccessModifier.Private)]
			set;
		}

		public int InactiveEnemies
		{
			get; [PublicizedFrom(EAccessModifier.Private)]
			set;
		}

		public int SleepingEnemies
		{
			get; [PublicizedFrom(EAccessModifier.Private)]
			set;
		}

		public int ActiveEnemies
		{
			get
			{
				if (this.enemies == null)
				{
					return 0;
				}
				return this.enemies.Count - this.DeadEnemies - this.InactiveEnemies - this.SleepingEnemies;
			}
		}

		public float NumericalThreatLevel
		{
			get; [PublicizedFrom(EAccessModifier.Private)]
			set;
		}

		public ThreatLevelLegacyType ThreatLevel
		{
			get
			{
				if (this.NumericalThreatLevel < 0.25f)
				{
					return ThreatLevelLegacyType.Exploration;
				}
				return ThreatLevelLegacyType.Suspense;
			}
		}

		public bool IsMusicPlayingThisTick
		{
			[PublicizedFrom(EAccessModifier.Private)]
			get
			{
				return this.dynamicMusicManager.IsMusicPlayingThisTick;
			}
		}

		public bool IsThreatLevelInExploration
		{
			[PublicizedFrom(EAccessModifier.Private)]
			get
			{
				return this.NumericalThreatLevel < 0.25f;
			}
		}

		public bool IsTargetInExploration
		{
			[PublicizedFrom(EAccessModifier.Private)]
			get
			{
				return this.threatLevelTarget <= 0.25f;
			}
		}

		public bool IsTargetAboveThreatLevel
		{
			[PublicizedFrom(EAccessModifier.Private)]
			get
			{
				return this.threatLevelTarget > this.NumericalThreatLevel;
			}
		}

		public static void Init(DynamicMusicManager _dmManager)
		{
			_dmManager.ThreatLevelTracker = new ThreatLevelTracker();
			_dmManager.ThreatLevelTracker.dynamicMusicManager = _dmManager;
			_dmManager.ThreatLevelTracker.somePlayer = GameManager.Instance.World.GetPrimaryPlayer();
			_dmManager.ThreatLevelTracker.epLocal = _dmManager.PrimaryLocalPlayer;
			_dmManager.ThreatLevelTracker.NumericalThreatLevel = 0f;
			_dmManager.ThreatLevelTracker.enemies = new List<Entity>();
		}

		public void Tick()
		{
			if (GameTimer.Instance.ticks % 20UL == 0UL)
			{
				this.TickTrackThreatLevel();
			}
			this.TickMoveThreatLevel();
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void TickMoveThreatLevel()
		{
			if (!this.IsMusicPlayingThisTick || (this.IsTargetInExploration && this.IsThreatLevelInExploration))
			{
				this.NumericalThreatLevel = this.threatLevelTarget;
				return;
			}
			if (!this.IsTargetAboveThreatLevel)
			{
				this.NumericalThreatLevel = Utils.FastClamp(this.NumericalThreatLevel - 0.003f, this.threatLevelTarget, this.NumericalThreatLevel);
				return;
			}
			if (this.IsThreatLevelInExploration)
			{
				this.NumericalThreatLevel = 0.25f;
				return;
			}
			this.NumericalThreatLevel = Utils.FastClamp(this.NumericalThreatLevel + 0.0015f, 0f, this.threatLevelTarget);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void TickTrackThreatLevel()
		{
			GameManager.Instance.World.GetEntitiesInBounds(typeof(EntityEnemy), new Bounds(this.epLocal.position, ThreatLevelTracker.boundingBoxRange), this.enemies);
			this.DeadEnemies = (this.SleepingEnemies = (this.InactiveEnemies = 0));
			this.threatLevelTarget = 0f;
			for (int i = 0; i < this.enemies.Count; i++)
			{
				EntityEnemy entityEnemy = this.enemies[i] as EntityEnemy;
				if (entityEnemy.IsDead())
				{
					int num = this.DeadEnemies;
					this.DeadEnemies = num + 1;
				}
				else if (entityEnemy.IsSleeping)
				{
					this.threatLevelTarget += 0.03125f;
					int num = this.SleepingEnemies;
					this.SleepingEnemies = num + 1;
				}
				else if (this.EnemyIsTargetingPlayer(entityEnemy))
				{
					this.threatLevelTarget += 0.25f;
				}
				else if (entityEnemy.IsAlert)
				{
					this.threatLevelTarget += 0.125f;
				}
				else
				{
					this.threatLevelTarget += 0.0625f;
					int num = this.InactiveEnemies;
					this.InactiveEnemies = num + 1;
				}
			}
			this.threatLevelTarget = Utils.FastClamp(this.threatLevelTarget, 0f, 0.5f);
			this.enemies.Clear();
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public bool EnemyIsTargetingPlayer(EntityEnemy _enemy)
		{
			EntityAlive attackTarget = _enemy.GetAttackTarget();
			return attackTarget != null && attackTarget.Equals(this.epLocal);
		}

		public void Event(MinEventTypes _eventType, MinEventParams _eventParms)
		{
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public DynamicMusicManager dynamicMusicManager;

		[PublicizedFrom(EAccessModifier.Private)]
		public const float cSleeperIncrement = 0.03125f;

		[PublicizedFrom(EAccessModifier.Private)]
		public const float cTargetIncrement = 0.25f;

		[PublicizedFrom(EAccessModifier.Private)]
		public const float cAlertIncrement = 0.125f;

		[PublicizedFrom(EAccessModifier.Private)]
		public const float cInactiveIncrement = 0.0625f;

		[PublicizedFrom(EAccessModifier.Private)]
		public const float cBaseIncrement = 0.0015f;

		[PublicizedFrom(EAccessModifier.Private)]
		public EntityPlayerLocal epLocal;

		[PublicizedFrom(EAccessModifier.Private)]
		public EntityPlayerLocal somePlayer;

		[PublicizedFrom(EAccessModifier.Private)]
		public List<Entity> enemies;

		[PublicizedFrom(EAccessModifier.Private)]
		public float threatLevelTarget;

		[PublicizedFrom(EAccessModifier.Private)]
		public static Vector3 boundingBoxRange = new Vector3(50f, 50f, 50f);
	}
}
