using System;
using System.Collections.Generic;
using Audio;
using GameEvent.SequenceActions;
using UnityEngine;

namespace GameEvent.GameEventHelpers
{
	public class HomerunData
	{
		public int Score
		{
			get
			{
				return this.score;
			}
			set
			{
				this.score = value;
				this.currentScoreIndex = this.GetRewardIndex(this.currentScoreIndex, value);
			}
		}

		public HomerunData(EntityPlayer player, float gameTime, string goalEntityNames, List<int> rewardLevels, List<string> rewardEvents, HomerunManager manager, Action completeCallback)
		{
			this.Player = player;
			this.Owner = manager;
			this.rewardLevels = rewardLevels;
			this.rewardEvents = rewardEvents;
			this.CompleteCallback = completeCallback;
			if (player.IsInParty())
			{
				this.BuffedPlayers = new List<EntityPlayer>();
				for (int i = 0; i < player.Party.MemberList.Count; i++)
				{
					EntityPlayer entityPlayer = player.Party.MemberList[i];
					if (!entityPlayer.Buffs.HasBuff("twitch_buffHomeRun"))
					{
						entityPlayer.Buffs.AddBuff("twitch_buffHomeRun", -1, true, false, -1f);
					}
					if (player != entityPlayer)
					{
						this.BuffedPlayers.Add(entityPlayer);
					}
				}
			}
			else if (!player.Buffs.HasBuff("twitch_buffHomeRun"))
			{
				player.Buffs.AddBuff("twitch_buffHomeRun", -1, true, false, -1f);
			}
			this.gr = GameEventManager.Current.Random;
			this.timeRemaining = gameTime;
			this.SetupEntityIDs(goalEntityNames);
			this.world = GameManager.Instance.World;
		}

		public void SetupEntityIDs(string entityNames)
		{
			string[] array = entityNames.Split(',', StringSplitOptions.None);
			this.entityIDs.Clear();
			for (int i = 0; i < array.Length; i++)
			{
				foreach (KeyValuePair<int, EntityClass> keyValuePair in EntityClass.list.Dict)
				{
					if (keyValuePair.Value.entityClassName == array[i])
					{
						this.entityIDs.Add(keyValuePair.Key);
						if (this.entityIDs.Count == array.Length)
						{
							break;
						}
					}
				}
			}
		}

		public bool Update(float deltaTime)
		{
			for (int i = this.ScoreDisplays.Count - 1; i >= 0; i--)
			{
				if (!this.ScoreDisplays[i].Update(deltaTime))
				{
					this.ScoreDisplays.RemoveAt(i);
				}
			}
			if (this.Player.IsDead())
			{
				return false;
			}
			if (this.BuffedPlayers != null)
			{
				for (int j = this.BuffedPlayers.Count - 1; j >= 0; j--)
				{
					if (this.BuffedPlayers[j].IsDead())
					{
						this.BuffedPlayers.RemoveAt(j);
					}
				}
			}
			if (this.timeRemaining > 10f && this.timeRemaining - deltaTime < 10f)
			{
				if (!this.Player.Buffs.HasBuff("twitch_buffHomeRunEnding"))
				{
					this.Player.Buffs.AddBuff("twitch_buffHomeRunEnding", -1, true, false, -1f);
				}
				if (this.BuffedPlayers != null)
				{
					for (int k = 0; k < this.BuffedPlayers.Count; k++)
					{
						if (!this.BuffedPlayers[k].Buffs.HasBuff("twitch_buffHomeRunEnding"))
						{
							this.BuffedPlayers[k].Buffs.AddBuff("twitch_buffHomeRunEnding", -1, true, false, -1f);
						}
					}
				}
			}
			this.timeRemaining -= deltaTime;
			if (this.timeRemaining > 0f)
			{
				if (this.GoalControllers.Count < this.ExpectedCount)
				{
					this.createTime -= deltaTime;
					if (this.createTime <= 0f)
					{
						Vector3 zero = Vector3.zero;
						if (ActionBaseSpawn.FindValidPosition(out zero, this.Player, 6f, 12f, true, 1f, true))
						{
							EntityHomerunGoal entityHomerunGoal = EntityFactory.CreateEntity(this.entityIDs[this.gr.RandomRange(this.entityIDs.Count)], zero, Vector3.zero, this.Player.entityId, "") as EntityHomerunGoal;
							entityHomerunGoal.SetSpawnerSource(EnumSpawnerSource.Dynamic);
							GameManager.Instance.World.SpawnEntityInWorld(entityHomerunGoal);
							entityHomerunGoal.StartPosition = zero;
							entityHomerunGoal.position = zero;
							entityHomerunGoal.direction = (EntityHomerunGoal.Direction)this.gr.RandomRange(5);
							Manager.BroadcastPlayByLocalPlayer(entityHomerunGoal.position, "twitch_balloon_spawn");
							entityHomerunGoal.Owner = this;
							this.GoalControllers.Add(entityHomerunGoal);
							this.createTime = 1f;
						}
					}
				}
				for (int l = this.GoalControllers.Count - 1; l >= 0; l--)
				{
					EntityHomerunGoal entityHomerunGoal2 = this.GoalControllers[l];
					if (this.GoalControllers[l].ReadyForDelete)
					{
						Manager.BroadcastPlayByLocalPlayer(entityHomerunGoal2.position, "twitch_balloon_despawn");
						this.world.RemoveEntity(entityHomerunGoal2.entityId, EnumRemoveEntityReason.Killed);
						this.GoalControllers.RemoveAt(l);
					}
				}
				return true;
			}
			int num = -1;
			for (int m = this.rewardLevels.Count - 1; m >= 0; m--)
			{
				if (this.Score > this.rewardLevels[m])
				{
					num = m;
					break;
				}
			}
			if (num >= 0)
			{
				string text = string.Format(Localization.Get("ttTwitchHomerunScore", false, null), Utils.ColorToHex(QualityInfo.GetTierColor(this.currentScoreIndex)), this.Score);
				GameManager.ShowTooltipMP(this.Player, text, "");
				GameEventManager.Current.HandleAction(this.rewardEvents[num], this.Player, this.Player, false, "", "", false, true, "", null);
				if (this.BuffedPlayers != null)
				{
					for (int n = 0; n < this.BuffedPlayers.Count; n++)
					{
						GameManager.ShowTooltipMP(this.BuffedPlayers[n], text, "");
						GameEventManager.Current.HandleAction(this.rewardEvents[num], this.Player, this.BuffedPlayers[n], false, "", "", false, true, "", null);
					}
				}
			}
			else
			{
				string text2 = Localization.Get("ttTwitchHomerunFailed", false, null);
				GameManager.ShowTooltipMP(this.Player, text2, "");
				if (this.BuffedPlayers != null)
				{
					for (int num2 = 0; num2 < this.BuffedPlayers.Count; num2++)
					{
						GameManager.ShowTooltipMP(this.BuffedPlayers[num2], text2, "");
					}
				}
			}
			return false;
		}

		public void Cleanup()
		{
			for (int i = this.ScoreDisplays.Count - 1; i >= 0; i--)
			{
				this.ScoreDisplays[i].Cleanup();
			}
			this.ScoreDisplays.Clear();
			for (int j = 0; j < this.GoalControllers.Count; j++)
			{
				if (this.GoalControllers[j] != null)
				{
					this.world.RemoveEntity(this.GoalControllers[j].entityId, EnumRemoveEntityReason.Killed);
				}
			}
			if (this.Player != null)
			{
				this.Player.Buffs.RemoveBuff("twitch_buffHomeRun", -1, true);
			}
			if (this.BuffedPlayers != null)
			{
				for (int k = 0; k < this.BuffedPlayers.Count; k++)
				{
					this.BuffedPlayers[k].Buffs.RemoveBuff("twitch_buffHomeRun", -1, true);
				}
			}
			this.GoalControllers.Clear();
		}

		public void AddScoreDisplay(Vector3 position)
		{
			Color tierColor = QualityInfo.GetTierColor(this.currentScoreIndex);
			if (this.Player.isEntityRemote)
			{
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageNavObject>().Setup("twitch_score", this.Score.ToString(), position, true, tierColor, false), false, this.Player.entityId, -1, -1, null, 192, false);
			}
			if (this.BuffedPlayers != null)
			{
				for (int i = 0; i < this.BuffedPlayers.Count; i++)
				{
					SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageNavObject>().Setup("twitch_score", this.Score.ToString(), position, true, tierColor, false), false, this.BuffedPlayers[i].entityId, -1, -1, null, 192, false);
				}
			}
			this.ScoreDisplays.Add(new HomerunData.ScoreDisplay(this.Score, position, tierColor)
			{
				Owner = this
			});
		}

		public void RemoveScoreDisplay(Vector3 position)
		{
			if (this.Player.isEntityRemote)
			{
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageNavObject>().Setup("twitch_score", "", position, false, false, -1), false, this.Player.entityId, -1, -1, null, 192, false);
			}
			if (this.BuffedPlayers != null)
			{
				for (int i = 0; i < this.BuffedPlayers.Count; i++)
				{
					SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageNavObject>().Setup("twitch_score", "", position, false, false, -1), false, this.BuffedPlayers[i].entityId, -1, -1, null, 192, false);
				}
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public int GetRewardIndex(int currentIndex, int newScore)
		{
			int num = currentIndex + 1;
			while (num < this.rewardLevels.Count && newScore >= this.rewardLevels[num - 1])
			{
				currentIndex = num;
				num++;
			}
			return currentIndex;
		}

		public List<EntityHomerunGoal> GoalControllers = new List<EntityHomerunGoal>();

		public EntityPlayer Player;

		public List<EntityPlayer> BuffedPlayers;

		public HomerunManager Owner;

		[PublicizedFrom(EAccessModifier.Private)]
		public List<int> rewardLevels;

		[PublicizedFrom(EAccessModifier.Private)]
		public List<string> rewardEvents;

		[PublicizedFrom(EAccessModifier.Private)]
		public List<int> entityIDs = new List<int>();

		public float timeRemaining = 120f;

		public int ExpectedCount = 3;

		public Action CompleteCallback;

		[PublicizedFrom(EAccessModifier.Private)]
		public int currentScoreIndex;

		[PublicizedFrom(EAccessModifier.Private)]
		public int score;

		public List<HomerunData.ScoreDisplay> ScoreDisplays = new List<HomerunData.ScoreDisplay>();

		[PublicizedFrom(EAccessModifier.Private)]
		public World world;

		[PublicizedFrom(EAccessModifier.Private)]
		public float createTime = 1f;

		[PublicizedFrom(EAccessModifier.Private)]
		public GameRandom gr;

		public class ScoreDisplay
		{
			public ScoreDisplay(int score, Vector3 position, Color color)
			{
				this.NavObject = NavObjectManager.Instance.RegisterNavObject("twitch_score", position, "", false, -1, null);
				this.NavObject.IsActive = true;
				this.NavObject.name = score.ToString();
				this.NavObject.UseOverrideFontColor = true;
				this.NavObject.OverrideColor = color;
			}

			public bool Update(float deltaTime)
			{
				this.TimeRemaining -= deltaTime;
				if (this.TimeRemaining <= 0f)
				{
					this.RemoveNavObject();
					return false;
				}
				return true;
			}

			public void Cleanup()
			{
				if (this.NavObject != null)
				{
					this.RemoveNavObject();
				}
			}

			[PublicizedFrom(EAccessModifier.Private)]
			public void RemoveNavObject()
			{
				this.Owner.RemoveScoreDisplay(this.NavObject.TrackedPosition);
				NavObjectManager.Instance.UnRegisterNavObject(this.NavObject);
				this.NavObject = null;
			}

			public int Score;

			public NavObject NavObject;

			public float TimeRemaining = 3f;

			public HomerunData Owner;
		}
	}
}
