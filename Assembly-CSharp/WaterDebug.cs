using System;
using System.Diagnostics;

public static class WaterDebug
{
	public static WaterDebugManager Manager
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get;
		[PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public static bool IsAvailable
	{
		get
		{
			return WaterDebug.Manager != null;
		}
	}

	public static bool RenderingEnabled
	{
		get
		{
			WaterDebugManager manager = WaterDebug.Manager;
			return manager != null && manager.RenderingEnabled;
		}
		set
		{
			if (WaterDebug.Manager != null)
			{
				WaterDebug.Manager.RenderingEnabled = value;
			}
		}
	}

	[Conditional("UNITY_EDITOR")]
	public static void Init()
	{
		if (!WaterSimulationNative.Instance.ShouldEnable)
		{
			return;
		}
		WaterDebugPools.CreatePools();
		WaterDebug.Manager = new WaterDebugManager();
		WaterDebug.RenderingEnabled = false;
	}

	[Conditional("UNITY_EDITOR")]
	public static void InitializeForChunk(Chunk _chunk)
	{
		WaterDebugManager manager = WaterDebug.Manager;
		if (manager == null)
		{
			return;
		}
		manager.InitializeDebugRender(_chunk);
	}

	[Conditional("UNITY_EDITOR")]
	public static void Draw()
	{
		WaterDebugManager manager = WaterDebug.Manager;
		if (manager == null)
		{
			return;
		}
		manager.DebugDraw();
	}

	[Conditional("UNITY_EDITOR")]
	public static void Cleanup()
	{
		WaterDebugManager manager = WaterDebug.Manager;
		if (manager != null)
		{
			manager.Cleanup();
		}
		WaterDebugPools.Cleanup();
		WaterDebug.Manager = null;
	}
}
