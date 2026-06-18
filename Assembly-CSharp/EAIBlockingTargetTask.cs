using System;
using UnityEngine.Scripting;

[Preserve]
public class EAIBlockingTargetTask : EAIBase
{
	public override void Init(EntityAlive _theEntity)
	{
		base.Init(_theEntity);
		this.MutexBits = 1;
	}

	public override bool CanExecute()
	{
		return this.canExecute;
	}

	public override bool Continue()
	{
		return this.canExecute;
	}

	public bool canExecute;
}
