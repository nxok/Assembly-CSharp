using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public abstract class vp_EventHandler : MonoBehaviour
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void Awake()
	{
		this.StoreHandlerEvents();
		this.m_Initialized = true;
		for (int i = this.m_PendingRegistrants.Count - 1; i > -1; i--)
		{
			this.Register(this.m_PendingRegistrants[i]);
			this.m_PendingRegistrants.Remove(this.m_PendingRegistrants[i]);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void StoreHandlerEvents()
	{
		object obj = null;
		List<FieldInfo> fields = this.GetFields();
		if (fields == null || fields.Count == 0)
		{
			return;
		}
		foreach (FieldInfo fieldInfo in fields)
		{
			try
			{
				obj = Activator.CreateInstance(fieldInfo.FieldType, new object[] { fieldInfo.Name });
			}
			catch (Exception ex)
			{
				string[] array = new string[8];
				array[0] = "Error: (";
				array[1] = ((this != null) ? this.ToString() : null);
				array[2] = ") does not support the type of '";
				array[3] = fieldInfo.Name;
				array[4] = "' in '";
				int num = 5;
				Type declaringType = fieldInfo.DeclaringType;
				array[num] = ((declaringType != null) ? declaringType.ToString() : null);
				array[6] = "'. Exception: ";
				array[7] = ex.Message;
				Debug.LogError(string.Concat(array));
				continue;
			}
			if (obj != null)
			{
				fieldInfo.SetValue(this, obj);
				foreach (string text in ((vp_Event)obj).Prefixes.Keys)
				{
					this.m_HandlerEvents.Add(text + fieldInfo.Name, (vp_Event)obj);
				}
			}
		}
	}

	public List<FieldInfo> GetFields()
	{
		List<FieldInfo> list = new List<FieldInfo>();
		Type type = base.GetType();
		Type type2 = null;
		do
		{
			if (type2 != null)
			{
				type = type2;
			}
			list.AddRange(type.GetFields(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic));
			if (type.BaseType != typeof(vp_StateEventHandler) && type.BaseType != typeof(vp_EventHandler))
			{
				type2 = type.BaseType;
			}
		}
		while (type.BaseType != typeof(vp_StateEventHandler) && type.BaseType != typeof(vp_EventHandler) && type != null);
		if (list == null || list.Count == 0)
		{
			Debug.LogWarning("Warning: (" + ((this != null) ? this.ToString() : null) + ") Found no fields to store as events.");
		}
		return list;
	}

	public void Register(object target)
	{
		if (target == null)
		{
			Debug.LogError("Error: (" + ((this != null) ? this.ToString() : null) + ") Target object was null.");
			return;
		}
		if (!this.m_Initialized)
		{
			this.m_PendingRegistrants.Add(target);
			return;
		}
		vp_EventHandler.ScriptMethods scriptMethods = this.GetScriptMethods(target);
		if (scriptMethods == null)
		{
			Debug.LogError(string.Concat(new string[]
			{
				"Error: (",
				(this != null) ? this.ToString() : null,
				") could not get script methods for '",
				(target != null) ? target.ToString() : null,
				"'."
			}));
			return;
		}
		foreach (MethodInfo methodInfo in scriptMethods.Events)
		{
			vp_Event vp_Event;
			if (this.m_HandlerEvents.TryGetValue(methodInfo.Name, out vp_Event))
			{
				int num;
				vp_Event.Prefixes.TryGetValue(methodInfo.Name.Substring(0, methodInfo.Name.IndexOf('_', 4) + 1), out num);
				if (this.CompareMethodSignatures(methodInfo, vp_Event.GetParameterType(num), vp_Event.GetReturnType(num)))
				{
					vp_Event.Register(target, methodInfo.Name, num);
				}
			}
		}
	}

	public void Unregister(object target)
	{
		if (target == null)
		{
			Debug.LogError("Error: (" + ((this != null) ? this.ToString() : null) + ") Target object was null.");
			return;
		}
		foreach (vp_Event vp_Event in this.m_HandlerEvents.Values)
		{
			if (vp_Event != null)
			{
				foreach (string text in vp_Event.InvokerFieldNames)
				{
					FieldInfo field = vp_Event.GetType().GetField(text);
					if (!(field == null))
					{
						object value = field.GetValue(vp_Event);
						if (value != null)
						{
							Delegate @delegate = (Delegate)value;
							if (@delegate != null)
							{
								Delegate[] invocationList = @delegate.GetInvocationList();
								for (int j = 0; j < invocationList.Length; j++)
								{
									if (invocationList[j].Target == target)
									{
										vp_Event.Unregister(target);
									}
								}
							}
						}
					}
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool CompareMethodSignatures(MethodInfo scriptMethod, Type handlerParameterType, Type handlerReturnType)
	{
		if (scriptMethod.ReturnType != handlerReturnType)
		{
			string[] array = new string[9];
			array[0] = "Error: (";
			int num = 1;
			Type declaringType = scriptMethod.DeclaringType;
			array[num] = ((declaringType != null) ? declaringType.ToString() : null);
			array[2] = ") Return type (";
			array[3] = vp_Utility.GetTypeAlias(scriptMethod.ReturnType);
			array[4] = ") is not valid for '";
			array[5] = scriptMethod.Name;
			array[6] = "'. Return type declared in event handler was: (";
			array[7] = vp_Utility.GetTypeAlias(handlerReturnType);
			array[8] = ").";
			Debug.LogError(string.Concat(array));
			return false;
		}
		if (scriptMethod.GetParameters().Length == 1)
		{
			if (((ParameterInfo)scriptMethod.GetParameters().GetValue(0)).ParameterType != handlerParameterType)
			{
				string[] array2 = new string[9];
				array2[0] = "Error: (";
				int num2 = 1;
				Type declaringType2 = scriptMethod.DeclaringType;
				array2[num2] = ((declaringType2 != null) ? declaringType2.ToString() : null);
				array2[2] = ") Parameter type (";
				array2[3] = vp_Utility.GetTypeAlias(((ParameterInfo)scriptMethod.GetParameters().GetValue(0)).ParameterType);
				array2[4] = ") is not valid for '";
				array2[5] = scriptMethod.Name;
				array2[6] = "'. Parameter type declared in event handler was: (";
				array2[7] = vp_Utility.GetTypeAlias(handlerParameterType);
				array2[8] = ").";
				Debug.LogError(string.Concat(array2));
				return false;
			}
		}
		else if (scriptMethod.GetParameters().Length == 0)
		{
			if (handlerParameterType != typeof(void))
			{
				string[] array3 = new string[7];
				array3[0] = "Error: (";
				int num3 = 1;
				Type declaringType3 = scriptMethod.DeclaringType;
				array3[num3] = ((declaringType3 != null) ? declaringType3.ToString() : null);
				array3[2] = ") Can't register method '";
				array3[3] = scriptMethod.Name;
				array3[4] = "' with 0 parameters. Expected: 1 parameter of type (";
				array3[5] = vp_Utility.GetTypeAlias(handlerParameterType);
				array3[6] = ").";
				Debug.LogError(string.Concat(array3));
				return false;
			}
		}
		else if (scriptMethod.GetParameters().Length > 1)
		{
			string[] array4 = new string[9];
			array4[0] = "Error: (";
			int num4 = 1;
			Type declaringType4 = scriptMethod.DeclaringType;
			array4[num4] = ((declaringType4 != null) ? declaringType4.ToString() : null);
			array4[2] = ") Can't register method '";
			array4[3] = scriptMethod.Name;
			array4[4] = "' with ";
			array4[5] = scriptMethod.GetParameters().Length.ToString();
			array4[6] = " parameters. Max parameter count: 1 of type (";
			array4[7] = vp_Utility.GetTypeAlias(handlerParameterType);
			array4[8] = ").";
			Debug.LogError(string.Concat(array4));
			return false;
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public vp_EventHandler.ScriptMethods GetScriptMethods(object target)
	{
		vp_EventHandler.ScriptMethods scriptMethods;
		if (!vp_EventHandler.m_StoredScriptTypes.TryGetValue(target.GetType(), out scriptMethods))
		{
			scriptMethods = new vp_EventHandler.ScriptMethods(target.GetType());
			vp_EventHandler.m_StoredScriptTypes.Add(target.GetType(), scriptMethods);
		}
		return scriptMethods;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public vp_EventHandler()
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public bool m_Initialized;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Dictionary<string, vp_Event> m_HandlerEvents = new Dictionary<string, vp_Event>();

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public List<object> m_PendingRegistrants = new List<object>();

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public static Dictionary<Type, vp_EventHandler.ScriptMethods> m_StoredScriptTypes = new Dictionary<Type, vp_EventHandler.ScriptMethods>();

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public static string[] m_SupportedPrefixes = new string[] { "OnMessage_", "CanStart_", "CanStop_", "OnStart_", "OnStop_", "OnAttempt_", "get_OnValue_", "set_OnValue_", "OnFailStart_", "OnFailStop_" };

	[PublicizedFrom(EAccessModifier.Protected)]
	public class ScriptMethods
	{
		public ScriptMethods(Type type)
		{
			this.Events = vp_EventHandler.ScriptMethods.GetMethods(type);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public static List<MethodInfo> GetMethods(Type type)
		{
			List<MethodInfo> list = new List<MethodInfo>();
			List<string> list2 = new List<string>();
			while (type != null)
			{
				foreach (MethodInfo methodInfo in type.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
				{
					if (!methodInfo.Name.Contains(">m__") && !list2.Contains(methodInfo.Name))
					{
						foreach (string text in vp_EventHandler.m_SupportedPrefixes)
						{
							if (methodInfo.Name.Contains(text))
							{
								list.Add(methodInfo);
								list2.Add(methodInfo.Name);
								break;
							}
						}
					}
				}
				type = type.BaseType;
			}
			return list;
		}

		public List<MethodInfo> Events = new List<MethodInfo>();
	}
}
