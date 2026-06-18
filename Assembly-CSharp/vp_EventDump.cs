using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine.Scripting;

[Preserve]
public class vp_EventDump
{
	public static string Dump(vp_EventHandler handler, string[] eventTypes)
	{
		string text = "";
		foreach (string text2 in eventTypes)
		{
			if (!(text2 == "vp_Message"))
			{
				if (!(text2 == "vp_Attempt"))
				{
					if (!(text2 == "vp_Value"))
					{
						if (text2 == "vp_Activity")
						{
							text += vp_EventDump.DumpEventsOfType("vp_Activity", (eventTypes.Length > 1) ? "ACTIVITIES:\n\n" : "", handler);
						}
					}
					else
					{
						text += vp_EventDump.DumpEventsOfType("vp_Value", (eventTypes.Length > 1) ? "VALUES:\n\n" : "", handler);
					}
				}
				else
				{
					text += vp_EventDump.DumpEventsOfType("vp_Attempt", (eventTypes.Length > 1) ? "ATTEMPTS:\n\n" : "", handler);
				}
			}
			else
			{
				text += vp_EventDump.DumpEventsOfType("vp_Message", (eventTypes.Length > 1) ? "MESSAGES:\n\n" : "", handler);
			}
		}
		return text;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static string DumpEventsOfType(string type, string caption, vp_EventHandler handler)
	{
		string text = caption.ToUpper();
		foreach (FieldInfo fieldInfo in handler.GetFields())
		{
			string text2 = null;
			if (!(type == "vp_Message"))
			{
				if (!(type == "vp_Attempt"))
				{
					if (!(type == "vp_Value"))
					{
						if (type == "vp_Activity")
						{
							if (fieldInfo.FieldType.ToString().Contains("vp_Activity"))
							{
								text2 = vp_EventDump.DumpEventListeners((vp_Event)fieldInfo.GetValue(handler), new string[] { "StartConditions", "StopConditions", "StartCallbacks", "StopCallbacks", "FailStartCallbacks", "FailStopCallbacks" });
							}
						}
					}
					else if (fieldInfo.FieldType.ToString().Contains("vp_Value"))
					{
						text2 = vp_EventDump.DumpEventListeners((vp_Event)fieldInfo.GetValue(handler), new string[] { "Get", "Set" });
					}
				}
				else if (fieldInfo.FieldType.ToString().Contains("vp_Attempt"))
				{
					text2 = vp_EventDump.DumpEventListeners((vp_Event)fieldInfo.GetValue(handler), new string[] { "Try" });
				}
			}
			else if (fieldInfo.FieldType.ToString().Contains("vp_Message"))
			{
				text2 = vp_EventDump.DumpEventListeners((vp_Message)fieldInfo.GetValue(handler), new string[] { "Send" });
			}
			if (!string.IsNullOrEmpty(text2))
			{
				text = string.Concat(new string[] { text, "\t\t", fieldInfo.Name, "\n", text2, "\n" });
			}
		}
		return text;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static string DumpEventListeners(object e, string[] invokers)
	{
		Type type = e.GetType();
		string text = "";
		foreach (string text2 in invokers)
		{
			FieldInfo field = type.GetField(text2);
			if (field == null)
			{
				return "";
			}
			Delegate @delegate = (Delegate)field.GetValue(e);
			string[] array = null;
			if (@delegate != null)
			{
				array = vp_EventDump.GetMethodNames(@delegate.GetInvocationList());
			}
			text += "\t\t\t\t";
			if (type.ToString().Contains("vp_Value"))
			{
				if (!(text2 == "Get"))
				{
					if (!(text2 == "Set"))
					{
						text += "Unsupported listener: ";
					}
					else
					{
						text += "Set";
					}
				}
				else
				{
					text += "Get";
				}
			}
			else if (type.ToString().Contains("vp_Attempt"))
			{
				text += "Try";
			}
			else if (type.ToString().Contains("vp_Message"))
			{
				text += "Send";
			}
			else if (type.ToString().Contains("vp_Activity"))
			{
				if (!(text2 == "StartConditions"))
				{
					if (!(text2 == "StopConditions"))
					{
						if (!(text2 == "StartCallbacks"))
						{
							if (!(text2 == "StopCallbacks"))
							{
								if (!(text2 == "FailStartCallbacks"))
								{
									if (!(text2 == "FailStopCallbacks"))
									{
										text += "Unsupported listener: ";
									}
									else
									{
										text += "FailStop";
									}
								}
								else
								{
									text += "FailStart";
								}
							}
							else
							{
								text += "Stop";
							}
						}
						else
						{
							text += "Start";
						}
					}
					else
					{
						text += "TryStop";
					}
				}
				else
				{
					text += "TryStart";
				}
			}
			else
			{
				text += "Unsupported listener";
			}
			if (array != null)
			{
				if (array.Length > 2)
				{
					text += ":\n";
				}
				else
				{
					text += ": ";
				}
				text += vp_EventDump.DumpDelegateNames(array);
			}
		}
		return text;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static string[] GetMethodNames(Delegate[] list)
	{
		list = vp_EventDump.RemoveDelegatesFromList(list);
		string[] array = new string[list.Length];
		if (list.Length == 1)
		{
			string[] array2 = array;
			int num = 0;
			string text2;
			if (list[0].Target != null)
			{
				string text = "(";
				object target = list[0].Target;
				text2 = text + ((target != null) ? target.ToString() : null) + ") ";
			}
			else
			{
				text2 = "";
			}
			array2[num] = text2 + list[0].Method.Name;
		}
		else
		{
			for (int i = 1; i < list.Length; i++)
			{
				string[] array3 = array;
				int num2 = i;
				string text4;
				if (list[i].Target != null)
				{
					string text3 = "(";
					object target2 = list[i].Target;
					text4 = text3 + ((target2 != null) ? target2.ToString() : null) + ") ";
				}
				else
				{
					text4 = "";
				}
				array3[num2] = text4 + list[i].Method.Name;
			}
		}
		return array;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static Delegate[] RemoveDelegatesFromList(Delegate[] list)
	{
		List<Delegate> list2 = new List<Delegate>(list);
		for (int i = list2.Count - 1; i > -1; i--)
		{
			if (list2[i] != null && list2[i].Method.Name.Contains("m_"))
			{
				list2.RemoveAt(i);
			}
		}
		return list2.ToArray();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static string DumpDelegateNames(string[] array)
	{
		string text = "";
		foreach (string text2 in array)
		{
			if (!string.IsNullOrEmpty(text2))
			{
				text = text + ((array.Length > 2) ? "\t\t\t\t\t\t\t" : "") + text2 + "\n";
			}
		}
		return text;
	}
}
