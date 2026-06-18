using System;
using UnityEngine.Scripting;

[Preserve]
public class AIDirectorZombieState : IMemoryPoolableObject
{
	public AIDirectorZombieState Construct(EntityEnemy zombie)
	{
		this.m_zombie = zombie;
		return this;
	}

	public void Reset()
	{
		this.m_zombie = null;
	}

	public void Cleanup()
	{
	}

	public EntityEnemy Zombie
	{
		get
		{
			return this.m_zombie;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityEnemy m_zombie;
}
