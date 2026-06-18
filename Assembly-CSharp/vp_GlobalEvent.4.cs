using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Scripting;

public static class vp_GlobalEvent<T, U, V>
{
	[Preserve]
	public static void Register(string name, vp_GlobalCallback<T, U, V> callback)
	{
		if (string.IsNullOrEmpty(name))
		{
			throw new ArgumentNullException("name");
		}
		if (callback == null)
		{
			throw new ArgumentNullException("callback");
		}
		List<vp_GlobalCallback<T, U, V>> list = (List<vp_GlobalCallback<T, U, V>>)vp_GlobalEvent<T, U, V>.m_Callbacks[name];
		if (list == null)
		{
			list = new List<vp_GlobalCallback<T, U, V>>();
			vp_GlobalEvent<T, U, V>.m_Callbacks.Add(name, list);
		}
		list.Add(callback);
	}

	[Preserve]
	public static void Unregister(string name, vp_GlobalCallback<T, U, V> callback)
	{
		if (string.IsNullOrEmpty(name))
		{
			throw new ArgumentNullException("name");
		}
		if (callback == null)
		{
			throw new ArgumentNullException("callback");
		}
		List<vp_GlobalCallback<T, U, V>> list = (List<vp_GlobalCallback<T, U, V>>)vp_GlobalEvent<T, U, V>.m_Callbacks[name];
		if (list != null)
		{
			list.Remove(callback);
			return;
		}
		throw vp_GlobalEventInternal.ShowUnregisterException(name);
	}

	[Preserve]
	public static void Send(string name, T arg1, U arg2, V arg3)
	{
		vp_GlobalEvent<T, U, V>.Send(name, arg1, arg2, arg3, vp_GlobalEventMode.DONT_REQUIRE_LISTENER);
	}

	public static void Send(string name, T arg1, U arg2, V arg3, vp_GlobalEventMode mode)
	{
		if (string.IsNullOrEmpty(name))
		{
			throw new ArgumentNullException("name");
		}
		if (arg1 == null)
		{
			throw new ArgumentNullException("arg1");
		}
		if (arg2 == null)
		{
			throw new ArgumentNullException("arg2");
		}
		if (arg3 == null)
		{
			throw new ArgumentNullException("arg3");
		}
		List<vp_GlobalCallback<T, U, V>> list = (List<vp_GlobalCallback<T, U, V>>)vp_GlobalEvent<T, U, V>.m_Callbacks[name];
		if (list != null)
		{
			using (List<vp_GlobalCallback<T, U, V>>.Enumerator enumerator = list.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					vp_GlobalCallback<T, U, V> vp_GlobalCallback = enumerator.Current;
					vp_GlobalCallback(arg1, arg2, arg3);
				}
				return;
			}
		}
		if (mode == vp_GlobalEventMode.REQUIRE_LISTENER)
		{
			throw vp_GlobalEventInternal.ShowSendException(name);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static Hashtable m_Callbacks = vp_GlobalEventInternal.Callbacks;
}
