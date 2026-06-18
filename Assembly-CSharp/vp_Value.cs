using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine.Scripting;

[Preserve]
public class vp_Value<V> : vp_Event
{
	[Preserve]
	[PublicizedFrom(EAccessModifier.Protected)]
	public static T Empty<T>()
	{
		return default(T);
	}

	[Preserve]
	[PublicizedFrom(EAccessModifier.Protected)]
	public static void Empty<T>(T value)
	{
	}

	[Preserve]
	public FieldInfo[] Fields
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return this.m_Fields;
		}
	}

	[Preserve]
	public vp_Value(string name)
		: base(name)
	{
		this.InitFields();
	}

	public void DoNotCallAOTCompileFix()
	{
		vp_Value<V>.Empty<V>();
		vp_Value<V>.Empty<V>(default(V));
		throw new InvalidOperationException("This method is used for AOT code generation only. Do not call it at runtime.");
	}

	[Preserve]
	[PublicizedFrom(EAccessModifier.Protected)]
	public override void InitFields()
	{
		this.m_Fields = new FieldInfo[]
		{
			base.GetType().GetField("Get"),
			base.GetType().GetField("Set")
		};
		base.StoreInvokerFieldNames();
		this.m_DelegateTypes = new Type[]
		{
			typeof(vp_Value<>.Getter<>),
			typeof(vp_Value<>.Setter<>)
		};
		this.m_DefaultMethods = new MethodInfo[]
		{
			base.GetStaticGenericMethod(base.GetType(), "Empty", typeof(void), this.m_ArgumentType),
			base.GetStaticGenericMethod(base.GetType(), "Empty", this.m_ArgumentType, typeof(void))
		};
		this.Prefixes = new Dictionary<string, int>
		{
			{ "get_OnValue_", 0 },
			{ "set_OnValue_", 1 }
		};
		if (this.m_DefaultMethods[0] != null)
		{
			base.SetFieldToLocalMethod(this.m_Fields[0], this.m_DefaultMethods[0], base.MakeGenericType(this.m_DelegateTypes[0]));
		}
		if (this.m_DefaultMethods[1] != null)
		{
			base.SetFieldToLocalMethod(this.m_Fields[1], this.m_DefaultMethods[1], base.MakeGenericType(this.m_DelegateTypes[1]));
		}
	}

	[Preserve]
	public override void Register(object t, string m, int v)
	{
		if (m == null)
		{
			return;
		}
		base.SetFieldToExternalMethod(t, this.m_Fields[v], m, base.MakeGenericType(this.m_DelegateTypes[v]));
		base.Refresh();
	}

	[Preserve]
	public override void Unregister(object t)
	{
		if (this.m_DefaultMethods[0] != null)
		{
			base.SetFieldToLocalMethod(this.m_Fields[0], this.m_DefaultMethods[0], base.MakeGenericType(this.m_DelegateTypes[0]));
		}
		if (this.m_DefaultMethods[1] != null)
		{
			base.SetFieldToLocalMethod(this.m_Fields[1], this.m_DefaultMethods[1], base.MakeGenericType(this.m_DelegateTypes[1]));
		}
		base.Refresh();
	}

	[Preserve]
	public vp_Value<V>.Getter<V> Get;

	[Preserve]
	public vp_Value<V>.Setter<V> Set;

	[Preserve]
	public delegate T Getter<T>();

	[Preserve]
	public delegate void Setter<T>(T o);
}
