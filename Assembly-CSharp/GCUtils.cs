using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public static class GCUtils
{
	[PublicizedFrom(EAccessModifier.Private)]
	public static void PreUnload()
	{
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void PostUnload()
	{
		GC.WaitForPendingFinalizers();
		GC.Collect();
	}

	public static void UnloadAndCollectStart()
	{
		ThreadManager.StartCoroutine(GCUtils.UnloadAndCollectCo());
	}

	public static IEnumerator UnloadAndCollectCo()
	{
		Interlocked.Increment(ref GCUtils.m_working);
		try
		{
			Task preUnload = Task.Run(new Action(GCUtils.PreUnload));
			yield return Resources.UnloadUnusedAssets();
			while (!preUnload.IsCompleted)
			{
				yield return null;
			}
			Task postUnload = Task.Run(new Action(GCUtils.PostUnload));
			while (!postUnload.IsCompleted)
			{
				yield return null;
			}
			preUnload = null;
			postUnload = null;
		}
		finally
		{
			Interlocked.Decrement(ref GCUtils.m_working);
		}
		yield break;
		yield break;
	}

	public static IEnumerator WaitForIdle()
	{
		while (GCUtils.m_working > 0)
		{
			yield return null;
		}
		yield break;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static int m_working;
}
