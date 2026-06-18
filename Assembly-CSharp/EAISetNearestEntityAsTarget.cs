using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class EAISetNearestEntityAsTarget : EAITarget
{
	public override void Init(EntityAlive _theEntity)
	{
		base.Init(_theEntity, 25f, true);
		this.MutexBits = 1;
		this.sorter = new EAISetNearestEntityAsTargetSorter(_theEntity);
	}

	public override void SetData(Dictionary<string, string> data)
	{
		base.SetData(data);
		this.targetClasses = new List<EAISetNearestEntityAsTarget.TargetClass>();
		string text;
		if (data.TryGetValue("class", out text))
		{
			string[] array = text.Split(',', StringSplitOptions.None);
			for (int i = 0; i < array.Length; i += 3)
			{
				EAISetNearestEntityAsTarget.TargetClass targetClass;
				targetClass.type = EntityFactory.GetEntityType(array[i]);
				targetClass.hearDistMax = 0f;
				if (i + 1 < array.Length)
				{
					targetClass.hearDistMax = StringParsers.ParseFloat(array[i + 1], 0, -1, NumberStyles.Any);
				}
				if (targetClass.hearDistMax == 0f)
				{
					targetClass.hearDistMax = 50f;
				}
				targetClass.seeDistMax = 0f;
				if (i + 2 < array.Length)
				{
					targetClass.seeDistMax = StringParsers.ParseFloat(array[i + 2], 0, -1, NumberStyles.Any);
				}
				if (targetClass.type == typeof(EntityPlayer))
				{
					this.playerTargetClassIndex = this.targetClasses.Count;
				}
				this.targetClasses.Add(targetClass);
			}
		}
	}

	public void SetTargetOnlyPlayers(float _distance)
	{
		this.targetClasses.Clear();
		EAISetNearestEntityAsTarget.TargetClass targetClass = default(EAISetNearestEntityAsTarget.TargetClass);
		targetClass.type = typeof(EntityPlayer);
		targetClass.hearDistMax = _distance;
		targetClass.seeDistMax = -_distance;
		this.targetClasses.Add(targetClass);
		this.playerTargetClassIndex = 0;
	}

	public override bool CanExecute()
	{
		if (this.theEntity.distraction != null)
		{
			return false;
		}
		this.FindTarget();
		if (!this.closeTargetEntity)
		{
			return false;
		}
		this.targetEntity = this.closeTargetEntity;
		this.targetPlayer = this.closeTargetEntity as EntityPlayer;
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void FindTarget()
	{
		this.closeTargetDist = float.MaxValue;
		this.closeTargetEntity = null;
		float seeDistance = this.theEntity.GetSeeDistance();
		for (int i = 0; i < this.targetClasses.Count; i++)
		{
			EAISetNearestEntityAsTarget.TargetClass targetClass = this.targetClasses[i];
			float num = seeDistance;
			if (targetClass.seeDistMax != 0f)
			{
				float num2 = ((targetClass.seeDistMax < 0f) ? (-targetClass.seeDistMax) : (targetClass.seeDistMax * this.theEntity.senseScale));
				num = Utils.FastMin(num, num2);
			}
			if (targetClass.type == typeof(EntityPlayer))
			{
				this.FindTargetPlayer(num);
				if (this.theEntity.noisePlayer && this.theEntity.noisePlayer != this.closeTargetEntity)
				{
					if (this.closeTargetEntity)
					{
						if (this.theEntity.noisePlayerVolume >= this.theEntity.sleeperNoiseToWake)
						{
							Vector3 position = this.theEntity.noisePlayer.position;
							float magnitude = (this.theEntity.position - position).magnitude;
							if (magnitude < this.closeTargetDist)
							{
								this.closeTargetDist = magnitude;
								this.closeTargetEntity = this.theEntity.noisePlayer;
							}
						}
					}
					else if (!this.theEntity.IsSleeping)
					{
						this.SeekNoise(this.theEntity.noisePlayer);
					}
				}
				if (this.closeTargetEntity)
				{
					EntityPlayer entityPlayer = (EntityPlayer)this.closeTargetEntity;
					if (entityPlayer.IsBloodMoonDead && entityPlayer.currentLife >= 0.5f)
					{
						Log.Out("Player {0}, living {1}, lost BM immunity", new object[]
						{
							entityPlayer.GetDebugName(),
							entityPlayer.currentLife * 60f
						});
						entityPlayer.IsBloodMoonDead = false;
					}
				}
				if (this.theEntity.attractPlayer && !this.closeTargetEntity && !this.theEntity.IsSleeping && !this.theEntity.HasInvestigatePosition)
				{
					this.SeekBreadcrumb(this.theEntity.attractPlayer, 15f);
				}
				if (this.theEntity.smellPlayer && !this.closeTargetEntity && !this.theEntity.IsSleeping && !this.theEntity.HasInvestigatePosition && this.theEntity.smellPlayer.currentLife > 1f)
				{
					this.SeekBreadcrumb(this.theEntity.smellPlayer, 24f);
				}
			}
			else if (!this.theEntity.IsSleeping && !this.theEntity.HasInvestigatePosition)
			{
				this.theEntity.world.GetEntitiesInBounds(targetClass.type, BoundsUtils.ExpandBounds(this.theEntity.boundingBox, num, 4f, num), EAISetNearestEntityAsTarget.list);
				EAISetNearestEntityAsTarget.list.Sort(this.sorter);
				int j = 0;
				while (j < EAISetNearestEntityAsTarget.list.Count)
				{
					EntityAlive entityAlive = (EntityAlive)EAISetNearestEntityAsTarget.list[j];
					if (!(entityAlive is EntityDrone) && base.check(entityAlive))
					{
						float distance = this.theEntity.GetDistance(entityAlive);
						if (distance < this.closeTargetDist)
						{
							this.closeTargetDist = distance;
							this.closeTargetEntity = entityAlive;
							this.lastSeenPos = entityAlive.position;
							break;
						}
						break;
					}
					else
					{
						j++;
					}
				}
				EAISetNearestEntityAsTarget.list.Clear();
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SeekNoise(EntityPlayer player)
	{
		float num = (player.position - this.theEntity.position).magnitude;
		if (this.playerTargetClassIndex >= 0)
		{
			float num2 = this.targetClasses[this.playerTargetClassIndex].hearDistMax;
			num2 *= this.theEntity.senseScale;
			num2 *= player.DetectUsScale(this.theEntity);
			if (num > num2)
			{
				return;
			}
		}
		num *= 0.9f;
		if (num > this.manager.noiseSeekDist)
		{
			num = this.manager.noiseSeekDist;
		}
		if (this.theEntity.IsBloodMoon)
		{
			num = this.manager.noiseSeekDist * 0.25f;
		}
		Vector3 breadcrumbPos = player.GetBreadcrumbPos(num * base.RandomFloat);
		int num3 = this.theEntity.CalcInvestigateTicks((int)(30f + base.RandomFloat * 30f) * 20, player);
		this.theEntity.SetInvestigatePosition(breadcrumbPos, num3, true);
		this.PlaySoundSense();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SeekBreadcrumb(EntityPlayer player, float _maxDist)
	{
		float num = Mathf.Pow(base.RandomFloat, 2.1f);
		Vector3 breadcrumbPos = player.GetBreadcrumbPos(1f + _maxDist * num);
		int num2 = this.theEntity.CalcInvestigateTicks((int)(10f + base.RandomFloat * 10f) * 20, player);
		this.theEntity.SetInvestigatePosition(breadcrumbPos, num2, true);
		this.PlaySoundSense();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void PlaySoundSense()
	{
		float time = Time.time;
		if (this.senseSoundTime - time < 0f)
		{
			this.senseSoundTime = time + 10f + base.RandomFloat * 10f;
			this.theEntity.PlayOneShot(this.theEntity.GetSoundSense(), false, false, false, null, 1f);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void FindTargetPlayer(float seeDist)
	{
		if (this.theEntity.IsSleeperPassive)
		{
			return;
		}
		this.theEntity.world.GetPlayersAround(this.theEntity.position, seeDist, EAISetNearestEntityAsTarget.playerList);
		if (this.theEntity.IsSleeping)
		{
			EAISetNearestEntityAsTarget.playerList.Sort(this.sorter);
			EntityPlayer entityPlayer = null;
			float num = float.MaxValue;
			bool flag = false;
			if (this.theEntity.noisePlayer != null)
			{
				if (this.theEntity.noisePlayerVolume >= this.theEntity.sleeperNoiseToWake)
				{
					entityPlayer = this.theEntity.noisePlayer;
					num = this.theEntity.noisePlayerDistance;
				}
				else if (this.theEntity.noisePlayerVolume >= this.theEntity.sleeperNoiseToSense)
				{
					flag = true;
				}
			}
			for (int i = 0; i < EAISetNearestEntityAsTarget.playerList.Count; i++)
			{
				EntityPlayer entityPlayer2 = EAISetNearestEntityAsTarget.playerList[i];
				if (this.theEntity.CanSee(entityPlayer2) && !entityPlayer2.IsIgnoredByAI())
				{
					float distance = this.theEntity.GetDistance(entityPlayer2);
					int sleeperDisturbedLevel = this.theEntity.GetSleeperDisturbedLevel(distance, entityPlayer2.Stealth.lightLevel);
					if (sleeperDisturbedLevel >= 2)
					{
						if (distance < num)
						{
							entityPlayer = entityPlayer2;
							num = distance;
						}
					}
					else if (sleeperDisturbedLevel >= 1)
					{
						flag = true;
					}
				}
			}
			if (entityPlayer != null)
			{
				this.closeTargetDist = num;
				this.closeTargetEntity = entityPlayer;
			}
			else if (flag)
			{
				this.theEntity.Groan();
			}
			else
			{
				this.theEntity.Snore();
			}
		}
		else
		{
			for (int j = 0; j < EAISetNearestEntityAsTarget.playerList.Count; j++)
			{
				EntityPlayer entityPlayer3 = EAISetNearestEntityAsTarget.playerList[j];
				if (entityPlayer3.IsAlive() && !entityPlayer3.IsIgnoredByAI())
				{
					float seeDistance = this.manager.GetSeeDistance(entityPlayer3);
					if (seeDistance < this.closeTargetDist && this.theEntity.CanSee(entityPlayer3) && this.theEntity.CanSeeStealth(seeDistance, entityPlayer3.Stealth.lightLevel))
					{
						this.closeTargetDist = seeDistance;
						this.closeTargetEntity = entityPlayer3;
					}
				}
			}
		}
		EAISetNearestEntityAsTarget.playerList.Clear();
	}

	public override void Start()
	{
		this.theEntity.SetAttackTarget(this.targetEntity, 200);
		this.theEntity.ConditionalTriggerSleeperWakeUp();
		this.PlaySoundSense();
		base.Start();
	}

	public override bool Continue()
	{
		if (this.targetEntity.IsDead() || this.theEntity.distraction != null)
		{
			if (this.theEntity.GetAttackTarget() == this.targetEntity)
			{
				this.theEntity.SetAttackTarget(null, 0);
			}
			return false;
		}
		this.findTime += 0.05f;
		if (this.findTime > 2f)
		{
			this.findTime = 0f;
			this.FindTarget();
			if (this.closeTargetEntity && this.closeTargetEntity != this.targetEntity)
			{
				return false;
			}
		}
		if (this.theEntity.GetAttackTarget() != this.targetEntity)
		{
			return false;
		}
		if (base.check(this.targetEntity) && (this.targetPlayer == null || this.theEntity.CanSeeStealth(this.manager.GetSeeDistance(this.targetEntity), this.targetPlayer.Stealth.lightLevel)))
		{
			this.theEntity.SetAttackTarget(this.targetEntity, 600);
			this.lastSeenPos = this.targetEntity.position;
			return true;
		}
		if (this.theEntity.GetDistanceSq(this.lastSeenPos) < 2.25f)
		{
			this.lastSeenPos = Vector3.zero;
		}
		this.theEntity.SetAttackTarget(null, 0);
		int num = this.theEntity.CalcInvestigateTicks(Constants.cEnemySenseMemory * 20, this.targetEntity);
		if (this.lastSeenPos != Vector3.zero)
		{
			this.theEntity.SetInvestigatePosition(this.lastSeenPos, num, true);
		}
		return false;
	}

	public override void Reset()
	{
		this.targetEntity = null;
		this.targetPlayer = null;
	}

	public override string ToString()
	{
		return string.Format("{0}, {1}", base.ToString(), this.targetEntity ? this.targetEntity.EntityName : "");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cHearDistMax = 50f;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<EAISetNearestEntityAsTarget.TargetClass> targetClasses;

	[PublicizedFrom(EAccessModifier.Private)]
	public int playerTargetClassIndex = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	public float closeTargetDist;

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityAlive closeTargetEntity;

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityAlive targetEntity;

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityPlayer targetPlayer;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 lastSeenPos;

	[PublicizedFrom(EAccessModifier.Private)]
	public float findTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public float senseSoundTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public EAISetNearestEntityAsTargetSorter sorter;

	[PublicizedFrom(EAccessModifier.Private)]
	public static List<Entity> list = new List<Entity>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static List<EntityPlayer> playerList = new List<EntityPlayer>();

	[PublicizedFrom(EAccessModifier.Private)]
	public struct TargetClass
	{
		public Type type;

		public float hearDistMax;

		public float seeDistMax;
	}
}
