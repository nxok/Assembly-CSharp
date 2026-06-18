using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using GamePath;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public abstract class EAIBase
{
	public virtual void Init(EntityAlive _theEntity)
	{
		this.executeDelay = 0.5f;
		this.manager = _theEntity.aiManager;
		this.theEntity = _theEntity;
	}

	public virtual void SetData(Dictionary<string, string> data)
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void GetData(Dictionary<string, string> data, string name, ref float value)
	{
		string text;
		float num;
		if (data.TryGetValue(name, out text) && StringParsers.TryParseFloat(text, out num))
		{
			value = num;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void GetData(Dictionary<string, string> data, string name, ref int value)
	{
		string text;
		int num;
		if (data.TryGetValue(name, out text) && StringParsers.TryParseSInt32(text, out num))
		{
			value = num;
		}
	}

	public abstract bool CanExecute();

	public virtual bool Continue()
	{
		return this.CanExecute();
	}

	public virtual bool IsContinuous()
	{
		return true;
	}

	public virtual void Start()
	{
	}

	public virtual void Reset()
	{
	}

	public virtual void Update()
	{
	}

	public virtual bool IsPathUsageBlocked(PathEntity _path)
	{
		return false;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public static Vector3 GetTargetPos(EntityAlive theEntity)
	{
		if (theEntity.GetAttackTarget() != null)
		{
			return theEntity.GetAttackTarget().position;
		}
		return theEntity.InvestigatePosition;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public static bool EntityHasTarget(EntityAlive theEntity)
	{
		return theEntity.GetAttackTarget() != null || theEntity.HasInvestigatePosition;
	}

	public GameRandom Random
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return this.manager.random;
		}
	}

	public float RandomFloat
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return this.manager.random.RandomFloat;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int GetRandom(int maxExclusive)
	{
		return this.manager.random.RandomRange(maxExclusive);
	}

	public override string ToString()
	{
		if (this.shortedTypeName == null)
		{
			this.shortedTypeName = this.GetTypeName().Substring(3);
		}
		return this.shortedTypeName;
	}

	public string GetTypeName()
	{
		if (this.cachedTypeName == null)
		{
			this.cachedTypeName = base.GetType().Name;
		}
		return this.cachedTypeName;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public EAIBase()
	{
	}

	public EAIManager manager;

	public EntityAlive theEntity;

	public float executeWaitTime;

	public float executeDelay;

	public int MutexBits;

	[PublicizedFrom(EAccessModifier.Private)]
	public string cachedTypeName;

	[PublicizedFrom(EAccessModifier.Private)]
	public string shortedTypeName;
}
