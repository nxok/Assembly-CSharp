using System;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine.Scripting;

[Preserve]
public class MinEventActionBase : IMinEventAction
{
	public virtual void GetInfoStrings(ref List<string> list)
	{
		list.Add(this.EventType.ToStringCached<MinEventTypes>() + ": " + this.ToString());
		RequirementGroup requirements = this.Requirements;
		if (requirements == null)
		{
			return;
		}
		requirements.GetInfoStrings(ref list);
	}

	public virtual void Execute(MinEventParams _params)
	{
	}

	public virtual bool CanExecute(MinEventTypes _eventType, MinEventParams _params)
	{
		return this.Requirements == null || this.Requirements.IsValid(_params);
	}

	public virtual bool ParseXmlAttribute(XAttribute _attribute)
	{
		string localName = _attribute.Name.LocalName;
		if (localName == "trigger")
		{
			this.EventType = EnumUtils.Parse<MinEventTypes>(_attribute.Value, false);
			return true;
		}
		if (!(localName == "delay"))
		{
			return false;
		}
		this.Delay = float.Parse(_attribute.Value);
		return true;
	}

	public static MinEventActionBase ParseAction(XElement _element)
	{
		if (!_element.HasAttribute("action"))
		{
			return null;
		}
		Type typeWithPrefix = ReflectionHelpers.GetTypeWithPrefix("MinEventAction", _element.GetAttribute("action"));
		if (typeWithPrefix == null)
		{
			Log.Out("Unable to find class: MinEventAction{0}", new object[] { _element.GetAttribute("action") });
			return null;
		}
		MinEventActionBase minEventActionBase = (MinEventActionBase)Activator.CreateInstance(typeWithPrefix);
		foreach (XAttribute xattribute in _element.Attributes())
		{
			minEventActionBase.ParseXmlAttribute(xattribute);
		}
		minEventActionBase.Requirements = RequirementBase.ParseRequirementGroup(_element);
		minEventActionBase.ParseXMLPostProcess();
		return minEventActionBase;
	}

	public virtual void ParseXMLPostProcess()
	{
	}

	public MinEventTypes EventType;

	public float Delay;

	[PublicizedFrom(EAccessModifier.Protected)]
	public RequirementGroup Requirements;
}
