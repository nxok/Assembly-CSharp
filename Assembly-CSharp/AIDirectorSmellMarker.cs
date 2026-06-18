using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public sealed class AIDirectorSmellMarker : IAIDirectorMarker, IMemoryPoolableObject
{
	public void Reference()
	{
		this.m_refCount++;
	}

	public bool Release()
	{
		int num = this.m_refCount - 1;
		this.m_refCount = num;
		if (num == 0)
		{
			this.Reset();
			AIDirectorSmellMarker.s_pool.Free(this);
			return true;
		}
		return false;
	}

	public void Reset()
	{
		this.m_playerState = null;
	}

	public void Cleanup()
	{
	}

	public void Tick(double dt)
	{
		this.m_ttl -= dt;
		if (this.m_ttl < 0.0)
		{
			this.m_ttl = 0.0;
		}
		this.m_validTime -= dt;
		if (this.m_validTime < 0.0)
		{
			this.m_validTime = 0.0;
		}
		this.m_time += dt;
		if (this.m_time > this.m_lifetime)
		{
			this.m_time = this.m_lifetime;
		}
		this.m_effectiveRadius = ((this.m_speed > 0.0) ? Math.Min(this.m_radius, this.m_speed * this.m_time) : this.m_radius);
		this.m_effectiveStrength = this.m_strength * (1.0 - this.m_time / this.m_lifetime);
	}

	public EntityPlayer Player
	{
		get
		{
			if (this.m_playerState != null)
			{
				return this.m_playerState.Player;
			}
			return null;
		}
	}

	public double IntensityForPosition(Vector3 position)
	{
		double num = (double)(this.m_pos - position).magnitude;
		if (num > this.m_effectiveRadius)
		{
			return 0.0;
		}
		double num2 = 1.0;
		if (num > 0.0)
		{
			num2 /= num * num;
		}
		return this.m_effectiveStrength * num2;
	}

	public Vector3 Position
	{
		get
		{
			return this.m_pos;
		}
	}

	public Vector3 TargetPosition
	{
		get
		{
			return this.m_targetPos;
		}
	}

	public bool Valid
	{
		get
		{
			return this.m_validTime > 0.0 && (this.Player == null || !this.Player.IsDead());
		}
	}

	public float MaxRadius
	{
		get
		{
			return (float)this.m_radius;
		}
	}

	public float Radius
	{
		get
		{
			return (float)this.m_effectiveRadius;
		}
	}

	public float TimeToLive
	{
		get
		{
			return (float)this.m_ttl;
		}
	}

	public float ValidTime
	{
		get
		{
			return (float)this.m_validTime;
		}
	}

	public float Speed
	{
		get
		{
			return (float)this.m_speed;
		}
	}

	public int Priority
	{
		get
		{
			return this.m_priority;
		}
	}

	public bool InterruptsNonPlayerAttack
	{
		get
		{
			return this.m_interruptsNonPlayerAttack;
		}
	}

	public bool IsDistraction
	{
		get
		{
			return this.m_isDistraction;
		}
	}

	public static AIDirectorSmellMarker Allocate(AIDirectorPlayerState ps, Vector3 position, Vector3 targetPosition, double radius, double strength, double speed, int priority, double ttl, bool interruptsNonPlayerAttack, bool isDistraction)
	{
		return null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public AIDirectorSmellMarker Construct(AIDirectorPlayerState ps, Vector3 position, Vector3 targetPosition, double radius, double strength, double speed, int priority, double ttl, bool interruptsNonPlayerAttack, bool isDistraction)
	{
		this.m_refCount = 1;
		this.m_playerState = ps;
		this.m_pos = position;
		this.m_targetPos = targetPosition;
		this.m_radius = radius;
		this.m_strength = strength;
		this.m_speed = speed;
		this.m_priority = priority;
		this.m_validTime = ttl;
		this.m_lifetime = ttl;
		this.m_time = 0.0;
		this.m_effectiveRadius = 0.0;
		this.m_effectiveStrength = strength;
		this.m_interruptsNonPlayerAttack = interruptsNonPlayerAttack;
		this.m_isDistraction = isDistraction;
		if (isDistraction)
		{
			this.m_ttl = (double)Mathf.Max((float)ttl, 20f);
		}
		else
		{
			this.m_ttl = (double)Constants.cEnemySenseMemory;
		}
		return this;
	}

	public const int kMax = 256;

	[PublicizedFrom(EAccessModifier.Private)]
	public static MemoryPooledObject<AIDirectorSmellMarker> s_pool = new MemoryPooledObject<AIDirectorSmellMarker>(256);

	[PublicizedFrom(EAccessModifier.Private)]
	public double m_radius;

	[PublicizedFrom(EAccessModifier.Private)]
	public double m_strength;

	[PublicizedFrom(EAccessModifier.Private)]
	public double m_speed;

	[PublicizedFrom(EAccessModifier.Private)]
	public double m_ttl;

	[PublicizedFrom(EAccessModifier.Private)]
	public double m_validTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public double m_time;

	[PublicizedFrom(EAccessModifier.Private)]
	public double m_lifetime;

	[PublicizedFrom(EAccessModifier.Private)]
	public double m_effectiveRadius;

	[PublicizedFrom(EAccessModifier.Private)]
	public double m_effectiveStrength;

	[PublicizedFrom(EAccessModifier.Private)]
	public int m_priority;

	[PublicizedFrom(EAccessModifier.Private)]
	public int m_refCount;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 m_pos;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 m_targetPos;

	[PublicizedFrom(EAccessModifier.Private)]
	public AIDirectorPlayerState m_playerState;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool m_interruptsNonPlayerAttack;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool m_isDistraction;
}
