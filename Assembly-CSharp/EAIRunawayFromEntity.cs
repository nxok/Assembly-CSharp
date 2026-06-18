using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class EAIRunawayFromEntity : EAIRunAway
{
	public override void Init(EntityAlive _theEntity)
	{
		base.Init(_theEntity);
		this.MutexBits = 1;
	}

	public override void SetData(Dictionary<string, string> data)
	{
		base.SetData(data);
		this.targetClasses = new List<Type>();
		string text;
		if (data.TryGetValue("class", out text))
		{
			string[] array = text.Split(',', StringSplitOptions.None);
			for (int i = 0; i < array.Length; i += 2)
			{
				Type entityType = EntityFactory.GetEntityType(array[i]);
				this.targetClasses.Add(entityType);
			}
		}
		base.GetData(data, "safeDistance", ref this.safeDistance);
		base.GetData(data, "minSneakDistance", ref this.minSneakDistance);
	}

	public override bool CanExecute()
	{
		this.FindEnemy();
		return !(this.enemy == null) && base.CanExecute();
	}

	public void FindEnemy()
	{
		this.enemy = null;
		if (this.theEntity.noisePlayer && this.theEntity.noisePlayerVolume >= 8f)
		{
			this.enemy = this.theEntity.noisePlayer;
			return;
		}
		float seeDistance = this.theEntity.GetSeeDistance();
		Bounds bounds = BoundsUtils.ExpandBounds(this.theEntity.boundingBox, seeDistance, seeDistance, seeDistance);
		for (int i = 0; i < this.targetClasses.Count; i++)
		{
			Type type = this.targetClasses[i];
			this.theEntity.world.GetEntitiesInBounds(type, bounds, EAIRunawayFromEntity.list);
			if (type == typeof(EntityPlayer))
			{
				float num = float.MaxValue;
				for (int j = 0; j < EAIRunawayFromEntity.list.Count; j++)
				{
					EntityPlayer entityPlayer = EAIRunawayFromEntity.list[j] as EntityPlayer;
					float seeDistance2 = this.manager.GetSeeDistance(entityPlayer);
					if (seeDistance2 < num && this.theEntity.CanSee(entityPlayer) && this.theEntity.CanSeeStealth(seeDistance2, entityPlayer.Stealth.lightLevel) && !entityPlayer.IsIgnoredByAI())
					{
						num = seeDistance2;
						this.enemy = entityPlayer;
					}
				}
			}
			else
			{
				float num2 = float.MaxValue;
				for (int k = 0; k < EAIRunawayFromEntity.list.Count; k++)
				{
					EntityAlive entityAlive = EAIRunawayFromEntity.list[k] as EntityAlive;
					float distanceSq = this.theEntity.GetDistanceSq(entityAlive);
					if (distanceSq <= this.minSneakDistance * this.minSneakDistance)
					{
						this.enemy = entityAlive;
						break;
					}
					if (distanceSq < num2 && this.theEntity.CanSee(entityAlive) && !entityAlive.IsIgnoredByAI())
					{
						num2 = distanceSq;
						this.enemy = entityAlive;
					}
				}
			}
			EAIRunawayFromEntity.list.Clear();
			if (this.enemy)
			{
				break;
			}
		}
	}

	public override bool Continue()
	{
		return this.theEntity.GetDistanceSq(this.enemy) < this.safeDistance * this.safeDistance && base.Continue();
	}

	public override void Update()
	{
		base.Update();
		this.theEntity.navigator.setMoveSpeed(this.theEntity.IsSwimming() ? this.theEntity.GetMoveSpeed() : this.theEntity.GetMoveSpeedPanic());
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override Vector3 GetFleeFromPos()
	{
		return this.enemy.position;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public List<Type> targetClasses;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cRunNoiseVolume = 8;

	[PublicizedFrom(EAccessModifier.Private)]
	public float safeDistance = 38f;

	[PublicizedFrom(EAccessModifier.Private)]
	public float minSneakDistance = 3.5f;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly List<Entity> list = new List<Entity>();
}
