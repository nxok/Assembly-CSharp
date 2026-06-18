using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine.Scripting;

[Preserve]
public class vp_Attempt : vp_Event
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public static bool AlwaysOK()
	{
		return true;
	}

	public vp_Attempt(string name)
		: base(name)
	{
		this.InitFields();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void InitFields()
	{
		this.m_Fields = new FieldInfo[] { base.GetType().GetField("Try") };
		base.StoreInvokerFieldNames();
		this.m_DefaultMethods = new MethodInfo[] { base.GetType().GetMethod("AlwaysOK") };
		this.m_DelegateTypes = new Type[] { typeof(vp_Attempt.Tryer) };
		this.Prefixes = new Dictionary<string, int> { { "OnAttempt_", 0 } };
		this.Try = new vp_Attempt.Tryer(vp_Attempt.AlwaysOK);
	}

	public override void Register(object t, string m, int v)
	{
		this.Try = (vp_Attempt.Tryer)Delegate.CreateDelegate(this.m_DelegateTypes[v], t, m);
		base.Refresh();
	}

	public override void Unregister(object t)
	{
		this.Try = new vp_Attempt.Tryer(vp_Attempt.AlwaysOK);
		base.Refresh();
	}

	public vp_Attempt.Tryer Try;

	public delegate bool Tryer();
}
