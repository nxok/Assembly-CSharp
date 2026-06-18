using System;
using UnityEngine.Scripting;

[Preserve]
public class EAITaskEntry
{
	public EAITaskEntry(int _priority, EAIBase _action)
	{
		this.priority = _priority;
		this.action = _action;
	}

	public EAIBase action;

	public int priority;

	public bool isExecuting;

	public float executeTime;
}
