using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class vp_Attempt<V> : vp_Attempt
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public static bool AlwaysOK<T>(T value)
	{
		return true;
	}

	public vp_Attempt(string name)
		: base(name)
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void InitFields()
	{
		this.m_Fields = new FieldInfo[] { base.GetType().GetField("Try") };
		base.StoreInvokerFieldNames();
		this.m_DefaultMethods = new MethodInfo[] { base.GetStaticGenericMethod(base.GetType(), "AlwaysOK", this.m_ArgumentType, typeof(bool)) };
		this.m_DelegateTypes = new Type[] { typeof(vp_Attempt<>.Tryer<>) };
		this.Prefixes = new Dictionary<string, int> { { "OnAttempt_", 0 } };
		if (this.m_DefaultMethods[0] != null)
		{
			base.SetFieldToLocalMethod(this.m_Fields[0], this.m_DefaultMethods[0], base.MakeGenericType(this.m_DelegateTypes[0]));
		}
	}

	public override void Register(object t, string m, int v)
	{
		if (((Delegate)this.m_Fields[v].GetValue(this)).Method.Name != this.m_DefaultMethods[v].Name)
		{
			Debug.LogWarning("Warning: Event '" + base.EventName + "' of type (vp_Attempt) targets multiple methods. Events of this type must reference a single method (only the last reference will be functional).");
		}
		if (m != null)
		{
			base.SetFieldToExternalMethod(t, this.m_Fields[0], m, base.MakeGenericType(this.m_DelegateTypes[v]));
		}
	}

	public override void Unregister(object t)
	{
		if (this.m_DefaultMethods[0] != null)
		{
			base.SetFieldToLocalMethod(this.m_Fields[0], this.m_DefaultMethods[0], base.MakeGenericType(this.m_DelegateTypes[0]));
		}
	}

	public new vp_Attempt<V>.Tryer<V> Try;

	public delegate bool Tryer<T>(T value);
}
