using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

[PublicizedFrom(EAccessModifier.Internal)]
public static class vp_TargetEventHandler
{
	public static List<Dictionary<object, Dictionary<string, Delegate>>> TargetDict
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			if (vp_TargetEventHandler.m_TargetDict == null)
			{
				vp_TargetEventHandler.m_TargetDict = new List<Dictionary<object, Dictionary<string, Delegate>>>(100);
				for (int i = 0; i < 8; i++)
				{
					vp_TargetEventHandler.m_TargetDict.Add(new Dictionary<object, Dictionary<string, Delegate>>(100));
				}
			}
			return vp_TargetEventHandler.m_TargetDict;
		}
	}

	public static void Register(object target, string eventName, Delegate callback, int dictionary)
	{
		if (target == null)
		{
			Debug.LogWarning("Warning: (" + vp_Utility.GetErrorLocation(2, false) + " -> vp_TargetEvent.Register) Target object is null.");
			return;
		}
		if (string.IsNullOrEmpty(eventName))
		{
			Debug.LogWarning("Warning: (" + vp_Utility.GetErrorLocation(2, false) + " -> vp_TargetEvent.Register) Event name is null or empty.");
			return;
		}
		if (callback == null)
		{
			Debug.LogWarning("Warning: (" + vp_Utility.GetErrorLocation(2, false) + " -> vp_TargetEvent.Register) Callback is null.");
			return;
		}
		if (callback.Method.Name.StartsWith("<"))
		{
			Debug.LogWarning("Warning: (" + vp_Utility.GetErrorLocation(2, false) + " -> vp_TargetEvent.Register) Target events can only be registered to declared methods.");
			return;
		}
		if (!vp_TargetEventHandler.TargetDict[dictionary].ContainsKey(target))
		{
			vp_TargetEventHandler.TargetDict[dictionary].Add(target, new Dictionary<string, Delegate>(100));
		}
		Dictionary<string, Delegate> dictionary2;
		vp_TargetEventHandler.TargetDict[dictionary].TryGetValue(target, out dictionary2);
		Delegate @delegate;
		for (;;)
		{
			dictionary2.TryGetValue(eventName, out @delegate);
			if (@delegate == null)
			{
				break;
			}
			if (!(@delegate.GetType() != callback.GetType()))
			{
				goto IL_010C;
			}
			eventName += "_";
		}
		dictionary2.Add(eventName, callback);
		return;
		IL_010C:
		callback = Delegate.Combine(@delegate, callback);
		if (callback != null)
		{
			dictionary2.Remove(eventName);
			dictionary2.Add(eventName, callback);
		}
	}

	public static void Unregister(object target, string eventName = null, Delegate callback = null)
	{
		if (eventName == null && callback != null)
		{
			return;
		}
		if (callback == null && eventName != null)
		{
			return;
		}
		for (int i = 0; i < 8; i++)
		{
			vp_TargetEventHandler.Unregister(target, i, eventName, callback);
		}
	}

	public static void Unregister(Component component)
	{
		if (component == null)
		{
			return;
		}
		for (int i = 0; i < 8; i++)
		{
			vp_TargetEventHandler.Unregister(i, component);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void Unregister(int dictionary, Component component)
	{
		if (component == null)
		{
			return;
		}
		Dictionary<string, Delegate> dictionary2;
		if (vp_TargetEventHandler.TargetDict[dictionary].TryGetValue(component, out dictionary2))
		{
			vp_TargetEventHandler.TargetDict[dictionary].Remove(component);
		}
		object transform = component.transform;
		if (transform == null)
		{
			return;
		}
		if (!vp_TargetEventHandler.TargetDict[dictionary].TryGetValue(transform, out dictionary2))
		{
			return;
		}
		foreach (string text in new List<string>(dictionary2.Keys))
		{
			Delegate @delegate;
			if (text != null && dictionary2.TryGetValue(text, out @delegate) && @delegate != null)
			{
				Delegate[] invocationList = @delegate.GetInvocationList();
				if (invocationList != null && invocationList.Length >= 1)
				{
					for (int i = invocationList.Length - 1; i > -1; i--)
					{
						if (invocationList[i].Target as Component == component)
						{
							dictionary2.Remove(text);
							Delegate delegate2 = Delegate.Remove(@delegate, invocationList[i]);
							if (delegate2.GetInvocationList().Length != 0)
							{
								dictionary2.Add(text, delegate2);
							}
						}
					}
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void Unregister(object target, int dictionary, string eventName, Delegate callback)
	{
		if (target == null)
		{
			return;
		}
		Dictionary<string, Delegate> dictionary2;
		if (!vp_TargetEventHandler.TargetDict[dictionary].TryGetValue(target, out dictionary2) || dictionary2 == null || dictionary2.Count == 0)
		{
			return;
		}
		if (eventName == null && callback == null)
		{
			vp_TargetEventHandler.TargetDict[dictionary].Remove(dictionary2);
			return;
		}
		Delegate @delegate;
		if (!dictionary2.TryGetValue(eventName, out @delegate))
		{
			return;
		}
		if (@delegate != null)
		{
			dictionary2.Remove(eventName);
			@delegate = Delegate.Remove(@delegate, callback);
			if (@delegate != null && @delegate.GetInvocationList() != null)
			{
				dictionary2.Add(eventName, @delegate);
			}
		}
		else
		{
			dictionary2.Remove(eventName);
		}
		if (dictionary2.Count > 0)
		{
			return;
		}
		vp_TargetEventHandler.TargetDict[dictionary].Remove(target);
	}

	public static void UnregisterAll()
	{
		vp_TargetEventHandler.m_TargetDict = null;
	}

	public static Delegate GetCallback(object target, string eventName, bool upwards, int d, vp_TargetEventOptions options)
	{
		if (target == null)
		{
			return null;
		}
		if (string.IsNullOrEmpty(eventName))
		{
			return null;
		}
		for (;;)
		{
			Delegate @delegate = null;
			if ((options & vp_TargetEventOptions.IncludeInactive) == vp_TargetEventOptions.IncludeInactive)
			{
				goto IL_005F;
			}
			GameObject gameObject = target as GameObject;
			if (gameObject != null)
			{
				if (vp_Utility.IsActive(gameObject))
				{
					goto IL_005F;
				}
				if (!upwards)
				{
					break;
				}
			}
			else
			{
				Behaviour behaviour = target as Behaviour;
				if (!(behaviour != null) || (behaviour.enabled && vp_Utility.IsActive(behaviour.gameObject)))
				{
					goto IL_005F;
				}
				if (!upwards)
				{
					goto Block_9;
				}
			}
			IL_008B:
			if (@delegate != null || !upwards)
			{
				return @delegate;
			}
			target = vp_Utility.GetParent(target as Component);
			if (target == null)
			{
				return @delegate;
			}
			continue;
			IL_005F:
			Dictionary<string, Delegate> dictionary = null;
			if (!vp_TargetEventHandler.TargetDict[d].TryGetValue(target, out dictionary))
			{
				if (!upwards)
				{
					goto Block_11;
				}
				goto IL_008B;
			}
			else
			{
				if (!dictionary.TryGetValue(eventName, out @delegate) && !upwards)
				{
					goto Block_13;
				}
				goto IL_008B;
			}
		}
		return null;
		Block_9:
		return null;
		Block_11:
		return null;
		Block_13:
		return null;
	}

	public static void OnNoReceiver(string eventName, vp_TargetEventOptions options)
	{
		if ((options & vp_TargetEventOptions.RequireReceiver) != vp_TargetEventOptions.RequireReceiver)
		{
			return;
		}
		Debug.LogError(string.Concat(new string[]
		{
			"Error: (",
			vp_Utility.GetErrorLocation(2, false),
			") vp_TargetEvent '",
			eventName,
			"' has no receiver!"
		}));
	}

	public static string Dump()
	{
		Dictionary<object, string> dictionary = new Dictionary<object, string>();
		foreach (Dictionary<object, Dictionary<string, Delegate>> dictionary2 in vp_TargetEventHandler.TargetDict)
		{
			foreach (object obj in dictionary2.Keys)
			{
				string text = "";
				if (obj != null)
				{
					Dictionary<string, Delegate> dictionary3;
					if (dictionary2.TryGetValue(obj, out dictionary3))
					{
						foreach (string text2 in dictionary3.Keys)
						{
							text = text + "        \"" + text2 + "\" -> ";
							bool flag = false;
							Delegate @delegate;
							if (!string.IsNullOrEmpty(text2) && dictionary3.TryGetValue(text2, out @delegate))
							{
								if (@delegate.GetInvocationList().Length > 1)
								{
									flag = true;
									text += "\n";
								}
								foreach (Delegate delegate2 in @delegate.GetInvocationList())
								{
									string text3 = text;
									string text4 = (flag ? "                        " : "");
									Type reflectedType = delegate2.Method.ReflectedType;
									text = text3 + text4 + ((reflectedType != null) ? reflectedType.ToString() : null) + ".cs -> ";
									string text5 = "";
									foreach (ParameterInfo parameterInfo in delegate2.Method.GetParameters())
									{
										text5 = string.Concat(new string[]
										{
											text5,
											vp_Utility.GetTypeAlias(parameterInfo.ParameterType),
											" ",
											parameterInfo.Name,
											", "
										});
									}
									if (text5.Length > 0)
									{
										text5 = text5.Remove(text5.LastIndexOf(", "));
									}
									text = text + vp_Utility.GetTypeAlias(delegate2.Method.ReturnType) + " ";
									if (delegate2.Method.Name.Contains("m_"))
									{
										string text6 = delegate2.Method.Name.TrimStart('<');
										text6 = text6.Remove(text6.IndexOf('>'));
										text = text + text6 + " -> delegate";
									}
									else
									{
										text += delegate2.Method.Name;
									}
									text = text + "(" + text5 + ")\n";
								}
							}
						}
					}
					string text7;
					if (!dictionary.TryGetValue(obj, out text7))
					{
						dictionary.Add(obj, text);
					}
					else
					{
						dictionary.Remove(obj);
						dictionary.Add(obj, text7 + text);
					}
				}
			}
		}
		string text8 = "--- TARGET EVENT DUMP ---\n\n";
		foreach (object obj2 in dictionary.Keys)
		{
			if (obj2 != null)
			{
				text8 = text8 + obj2.ToString() + ":\n";
				string text9;
				if (dictionary.TryGetValue(obj2, out text9))
				{
					text8 += text9;
				}
			}
		}
		return text8;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static List<Dictionary<object, Dictionary<string, Delegate>>> m_TargetDict;
}
