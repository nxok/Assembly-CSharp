using System;
using System.Collections;

[PublicizedFrom(EAccessModifier.Internal)]
public static class vp_GlobalEventInternal
{
	public static vp_GlobalEventInternal.UnregisterException ShowUnregisterException(string name)
	{
		return new vp_GlobalEventInternal.UnregisterException(string.Format("Attempting to Unregister the event {0} but vp_GlobalEvent has not registered this event.", name));
	}

	public static vp_GlobalEventInternal.SendException ShowSendException(string name)
	{
		return new vp_GlobalEventInternal.SendException(string.Format("Attempting to Send the event {0} but vp_GlobalEvent has not registered this event.", name));
	}

	public static Hashtable Callbacks = new Hashtable();

	public class UnregisterException : Exception
	{
		public UnregisterException(string msg)
			: base(msg)
		{
		}
	}

	public class SendException : Exception
	{
		public SendException(string msg)
			: base(msg)
		{
		}
	}
}
