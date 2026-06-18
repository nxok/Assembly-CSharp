using System;

public readonly struct AIFocus<T> where T : struct, IFocusTarget
{
	public AIFocus(bool thisIsDumb)
	{
		this.FocusTargets = new T[4];
	}

	public void SetFocus(FocusPriority priority, T newTarget)
	{
		this.FocusTargets[(int)priority] = newTarget;
	}

	public void ClearFocus(FocusPriority priority)
	{
		this.FocusTargets[(int)priority] = new T();
	}

	public readonly T[] FocusTargets;
}
