using System;
using UnityEngine;

public static class vp_TargetEvent<T, U, V>
{
	public static void Register(object target, string eventName, Action<T, U, V> callback)
	{
		vp_TargetEventHandler.Register(target, eventName, callback, 3);
	}

	public static void Unregister(object target, string eventName, Action<T, U, V> callback)
	{
		vp_TargetEventHandler.Unregister(target, eventName, callback);
	}

	public static void Unregister(object target)
	{
		vp_TargetEventHandler.Unregister(target, null, null);
	}

	public static void Send(object target, string eventName, T arg1, U arg2, V arg3, vp_TargetEventOptions options = vp_TargetEventOptions.DontRequireReceiver)
	{
		for (;;)
		{
			Delegate callback = vp_TargetEventHandler.GetCallback(target, eventName, false, 3, options);
			if (callback == null)
			{
				break;
			}
			try
			{
				((Action<T, U, V>)callback)(arg1, arg2, arg3);
			}
			catch
			{
				eventName += "_";
				continue;
			}
			return;
		}
		vp_TargetEventHandler.OnNoReceiver(eventName, options);
	}

	public static void SendUpwards(Component target, string eventName, T arg1, U arg2, V arg3, vp_TargetEventOptions options = vp_TargetEventOptions.DontRequireReceiver)
	{
		for (;;)
		{
			Delegate callback = vp_TargetEventHandler.GetCallback(target, eventName, true, 3, options);
			if (callback == null)
			{
				break;
			}
			try
			{
				((Action<T, U, V>)callback)(arg1, arg2, arg3);
			}
			catch
			{
				eventName += "_";
				continue;
			}
			return;
		}
		vp_TargetEventHandler.OnNoReceiver(eventName, options);
	}
}
