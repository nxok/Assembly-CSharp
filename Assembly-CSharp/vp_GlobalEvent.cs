using System;
using System.Collections;
using System.Collections.Generic;

public static class vp_GlobalEvent
{
	public static void Register(string name, vp_GlobalCallback callback)
	{
		if (string.IsNullOrEmpty(name))
		{
			throw new ArgumentNullException("name");
		}
		if (callback == null)
		{
			throw new ArgumentNullException("callback");
		}
		List<vp_GlobalCallback> list = (List<vp_GlobalCallback>)vp_GlobalEvent.m_Callbacks[name];
		if (list == null)
		{
			list = new List<vp_GlobalCallback>();
			vp_GlobalEvent.m_Callbacks.Add(name, list);
		}
		list.Add(callback);
	}

	public static void Unregister(string name, vp_GlobalCallback callback)
	{
		if (string.IsNullOrEmpty(name))
		{
			throw new ArgumentNullException("name");
		}
		if (callback == null)
		{
			throw new ArgumentNullException("callback");
		}
		List<vp_GlobalCallback> list = (List<vp_GlobalCallback>)vp_GlobalEvent.m_Callbacks[name];
		if (list != null)
		{
			list.Remove(callback);
			return;
		}
		throw vp_GlobalEventInternal.ShowUnregisterException(name);
	}

	public static void Send(string name)
	{
		vp_GlobalEvent.Send(name, vp_GlobalEventMode.DONT_REQUIRE_LISTENER);
	}

	public static void Send(string name, vp_GlobalEventMode mode)
	{
		if (string.IsNullOrEmpty(name))
		{
			throw new ArgumentNullException("name");
		}
		List<vp_GlobalCallback> list = (List<vp_GlobalCallback>)vp_GlobalEvent.m_Callbacks[name];
		if (list != null)
		{
			using (List<vp_GlobalCallback>.Enumerator enumerator = list.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					vp_GlobalCallback vp_GlobalCallback = enumerator.Current;
					vp_GlobalCallback();
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
