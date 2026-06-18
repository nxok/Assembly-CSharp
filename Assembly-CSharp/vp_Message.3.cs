using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine.Scripting;

public class vp_Message<V, VResult> : vp_Message
{
	[Preserve]
	[PublicizedFrom(EAccessModifier.Protected)]
	public static TResult Empty<T, TResult>(T value)
	{
		return default(TResult);
	}

	[Preserve]
	public vp_Message(string name)
		: base(name)
	{
	}

	[Preserve]
	[PublicizedFrom(EAccessModifier.Protected)]
	public override void InitFields()
	{
		this.m_Fields = new FieldInfo[] { base.GetType().GetField("Send") };
		base.StoreInvokerFieldNames();
		this.m_DefaultMethods = new MethodInfo[] { base.GetStaticGenericMethod(base.GetType(), "Empty", this.m_ArgumentType, this.m_ReturnType) };
		this.m_DelegateTypes = new Type[] { typeof(vp_Message<, >.Sender<, >) };
		this.Prefixes = new Dictionary<string, int> { { "OnMessage_", 0 } };
		if (this.m_DefaultMethods[0] != null)
		{
			base.SetFieldToLocalMethod(this.m_Fields[0], this.m_DefaultMethods[0], base.MakeGenericType(this.m_DelegateTypes[0]));
		}
	}

	[Preserve]
	public override void Register(object t, string m, int v)
	{
		if (m == null)
		{
			return;
		}
		base.AddExternalMethodToField(t, this.m_Fields[0], m, base.MakeGenericType(this.m_DelegateTypes[0]));
		base.Refresh();
	}

	[Preserve]
	public override void Unregister(object t)
	{
		base.RemoveExternalMethodFromField(t, this.m_Fields[0]);
		base.Refresh();
	}

	[Preserve]
	public new vp_Message<V, VResult>.Sender<V, VResult> Send;

	[Preserve]
	public delegate TResult Sender<T, TResult>(T value);
}
