using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine.Scripting;

[Preserve]
public class vp_Message : vp_Event
{
	[Preserve]
	[PublicizedFrom(EAccessModifier.Protected)]
	public static void Empty()
	{
	}

	[Preserve]
	public vp_Message(string name)
		: base(name)
	{
		this.InitFields();
	}

	[Preserve]
	[PublicizedFrom(EAccessModifier.Protected)]
	public override void InitFields()
	{
		this.m_Fields = new FieldInfo[] { base.GetType().GetField("Send") };
		base.StoreInvokerFieldNames();
		this.m_DefaultMethods = new MethodInfo[] { base.GetType().GetMethod("Empty") };
		this.m_DelegateTypes = new Type[] { typeof(vp_Message.Sender) };
		this.Prefixes = new Dictionary<string, int> { { "OnMessage_", 0 } };
		this.Send = new vp_Message.Sender(vp_Message.Empty);
	}

	[Preserve]
	public override void Register(object t, string m, int v)
	{
		this.Send = (vp_Message.Sender)Delegate.Combine(this.Send, (vp_Message.Sender)Delegate.CreateDelegate(this.m_DelegateTypes[v], t, m));
		base.Refresh();
	}

	[Preserve]
	public override void Unregister(object t)
	{
		base.RemoveExternalMethodFromField(t, this.m_Fields[0]);
		base.Refresh();
	}

	[Preserve]
	public vp_Message.Sender Send;

	[Preserve]
	public delegate void Sender();
}
