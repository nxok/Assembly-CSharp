using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public abstract class vp_Event
{
	public string EventName
	{
		get
		{
			return this.m_Name;
		}
	}

	[Preserve]
	public Type ArgumentType
	{
		get
		{
			return this.m_ArgumentType;
		}
	}

	[Preserve]
	public Type ReturnType
	{
		get
		{
			return this.m_ReturnType;
		}
	}

	[Preserve]
	public abstract void Register(object target, string method, int variant);

	[Preserve]
	public abstract void Unregister(object target);

	[Preserve]
	[PublicizedFrom(EAccessModifier.Protected)]
	public abstract void InitFields();

	[Preserve]
	public vp_Event(string name = "")
	{
		this.m_ArgumentType = this.GetArgumentType;
		this.m_ReturnType = this.GetGenericReturnType;
		this.m_Name = name;
	}

	[Preserve]
	[PublicizedFrom(EAccessModifier.Protected)]
	public void StoreInvokerFieldNames()
	{
		this.InvokerFieldNames = new string[this.m_Fields.Length];
		for (int i = 0; i < this.m_Fields.Length; i++)
		{
			this.InvokerFieldNames[i] = this.m_Fields[i].Name;
		}
	}

	[Preserve]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Type MakeGenericType(Type type)
	{
		if (this.m_ReturnType == typeof(void))
		{
			return type.MakeGenericType(new Type[] { this.m_ArgumentType, this.m_ArgumentType });
		}
		return type.MakeGenericType(new Type[] { this.m_ArgumentType, this.m_ReturnType, this.m_ArgumentType, this.m_ReturnType });
	}

	[Preserve]
	[PublicizedFrom(EAccessModifier.Protected)]
	public void SetFieldToExternalMethod(object target, FieldInfo field, string method, Type type)
	{
		Delegate @delegate = Delegate.CreateDelegate(type, target, method, false, false);
		if (@delegate == null)
		{
			Debug.LogError(string.Concat(new string[]
			{
				"Error (",
				(this != null) ? this.ToString() : null,
				") Failed to bind: ",
				(target != null) ? target.ToString() : null,
				" -> ",
				method,
				"."
			}));
			return;
		}
		field.SetValue(this, @delegate);
	}

	[Preserve]
	[PublicizedFrom(EAccessModifier.Protected)]
	public void AddExternalMethodToField(object target, FieldInfo field, string method, Type type)
	{
		Delegate @delegate = Delegate.Combine((Delegate)field.GetValue(this), Delegate.CreateDelegate(type, target, method, false, false));
		if (@delegate == null)
		{
			Debug.LogError(string.Concat(new string[]
			{
				"Error (",
				(this != null) ? this.ToString() : null,
				") Failed to bind: ",
				(target != null) ? target.ToString() : null,
				" -> ",
				method,
				"."
			}));
			return;
		}
		field.SetValue(this, @delegate);
	}

	[Preserve]
	[PublicizedFrom(EAccessModifier.Protected)]
	public void SetFieldToLocalMethod(FieldInfo field, MethodInfo method, Type type)
	{
		if (method == null)
		{
			return;
		}
		Delegate @delegate = Delegate.CreateDelegate(type, method);
		if (@delegate == null)
		{
			Debug.LogError(string.Concat(new string[]
			{
				"Error (",
				(this != null) ? this.ToString() : null,
				") Failed to bind: ",
				(method != null) ? method.ToString() : null,
				"."
			}));
			return;
		}
		field.SetValue(this, @delegate);
	}

	[Preserve]
	[PublicizedFrom(EAccessModifier.Protected)]
	public void RemoveExternalMethodFromField(object target, FieldInfo field)
	{
		List<Delegate> list = new List<Delegate>(((Delegate)field.GetValue(this)).GetInvocationList());
		if (list == null)
		{
			Debug.LogError(string.Concat(new string[]
			{
				"Error (",
				(this != null) ? this.ToString() : null,
				") Failed to remove: ",
				(target != null) ? target.ToString() : null,
				" -> ",
				field.Name,
				"."
			}));
			return;
		}
		for (int i = list.Count - 1; i > -1; i--)
		{
			if (list[i].Target == target)
			{
				list.Remove(list[i]);
			}
		}
		if (list != null)
		{
			field.SetValue(this, Delegate.Combine(list.ToArray()));
		}
	}

	[Preserve]
	[PublicizedFrom(EAccessModifier.Protected)]
	public MethodInfo GetStaticGenericMethod(Type e, string name, Type parameterType, Type returnType)
	{
		foreach (MethodInfo methodInfo in e.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy))
		{
			if (!(methodInfo == null) && !(methodInfo.Name != name))
			{
				MethodInfo methodInfo2;
				if (this.GetGenericReturnType == typeof(void))
				{
					methodInfo2 = methodInfo.MakeGenericMethod(new Type[] { this.m_ArgumentType });
				}
				else
				{
					methodInfo2 = methodInfo.MakeGenericMethod(new Type[] { this.m_ArgumentType, this.m_ReturnType });
				}
				if (methodInfo2.GetParameters().Length <= 1 && (methodInfo2.GetParameters().Length != 1 || !(parameterType == typeof(void))) && (methodInfo2.GetParameters().Length != 0 || !(parameterType != typeof(void))) && (methodInfo2.GetParameters().Length != 1 || !(methodInfo2.GetParameters()[0].ParameterType != parameterType)) && !(returnType != methodInfo2.ReturnType))
				{
					return methodInfo2;
				}
			}
		}
		return null;
	}

	[Preserve]
	public Type GetArgumentType
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			if (!base.GetType().IsGenericType)
			{
				return typeof(void);
			}
			return base.GetType().GetGenericArguments()[0];
		}
	}

	[Preserve]
	public Type GetGenericReturnType
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			if (!base.GetType().IsGenericType)
			{
				return typeof(void);
			}
			if (base.GetType().GetGenericArguments().Length != 2)
			{
				return typeof(void);
			}
			return base.GetType().GetGenericArguments()[1];
		}
	}

	[Preserve]
	public Type GetParameterType(int index)
	{
		if (!base.GetType().IsGenericType)
		{
			return typeof(void);
		}
		if (index > this.m_Fields.Length - 1)
		{
			Debug.LogError(string.Concat(new string[]
			{
				"Error: (",
				(this != null) ? this.ToString() : null,
				") Event '",
				this.EventName,
				"' only supports ",
				this.m_Fields.Length.ToString(),
				" indices. 'GetParameterType' referenced index ",
				index.ToString(),
				"."
			}));
		}
		if (this.m_DelegateTypes[index].GetMethod("Invoke").GetParameters().Length == 0)
		{
			return typeof(void);
		}
		return this.m_ArgumentType;
	}

	[Preserve]
	public Type GetReturnType(int index)
	{
		if (index > this.m_Fields.Length - 1)
		{
			Debug.LogError(string.Concat(new string[]
			{
				"Error: (",
				(this != null) ? this.ToString() : null,
				") Event '",
				this.EventName,
				"' only supports ",
				this.m_Fields.Length.ToString(),
				" indices. 'GetReturnType' referenced index ",
				index.ToString(),
				"."
			}));
			return null;
		}
		if (base.GetType().GetGenericArguments().Length > 1)
		{
			return this.GetGenericReturnType;
		}
		Type returnType = this.m_DelegateTypes[index].GetMethod("Invoke").ReturnType;
		if (returnType.IsGenericParameter)
		{
			return this.m_ArgumentType;
		}
		return returnType;
	}

	[Preserve]
	[PublicizedFrom(EAccessModifier.Protected)]
	public void Refresh()
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public string m_Name;

	[Preserve]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Type m_ArgumentType;

	[Preserve]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Type m_ReturnType;

	[Preserve]
	[PublicizedFrom(EAccessModifier.Protected)]
	public FieldInfo[] m_Fields;

	[Preserve]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Type[] m_DelegateTypes;

	[Preserve]
	[PublicizedFrom(EAccessModifier.Protected)]
	public MethodInfo[] m_DefaultMethods;

	[Preserve]
	public string[] InvokerFieldNames;

	[Preserve]
	public Dictionary<string, int> Prefixes;
}
