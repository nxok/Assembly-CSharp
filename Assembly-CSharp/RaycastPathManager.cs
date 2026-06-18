using System;
using System.Collections.Generic;
using RaycastPathing;
using UnityEngine.Scripting;

[Preserve]
public class RaycastPathManager
{
	public static RaycastPathManager Instance
	{
		get
		{
			return RaycastPathManager.instance;
		}
	}

	public static void Init()
	{
		RaycastPathManager.instance = new RaycastPathManager();
		RaycastPathManager.instance._Init();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void _Init()
	{
	}

	public void Add(RaycastPath path)
	{
		if (!this.paths.Contains(path))
		{
			this.paths.Add(path);
		}
	}

	public void Remove(RaycastPath path)
	{
		if (this.paths.Contains(path))
		{
			this.paths.Remove(path);
		}
	}

	public void Update()
	{
		if (RaycastPathManager.DebugModeEnabled)
		{
			this._DebugDraw();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void _DebugDraw()
	{
		for (int i = 0; i < this.paths.Count; i++)
		{
			this.paths[i].DebugDraw();
		}
	}

	public static implicit operator bool(RaycastPathManager exists)
	{
		return exists != null;
	}

	public static bool DebugModeEnabled;

	[PublicizedFrom(EAccessModifier.Protected)]
	public List<RaycastPath> paths = new List<RaycastPath>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static RaycastPathManager instance;
}
