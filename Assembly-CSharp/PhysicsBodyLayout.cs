using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;

public class PhysicsBodyLayout
{
	public string Name
	{
		get
		{
			return this.name;
		}
	}

	public List<PhysicsBodyColliderConfiguration> Colliders
	{
		get
		{
			return this.colliders;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static PhysicsBodyLayout New(string _name)
	{
		if (PhysicsBodyLayout.bodyLayouts.ContainsKey(_name))
		{
			throw new Exception("duplicate physics body!");
		}
		PhysicsBodyLayout physicsBodyLayout = new PhysicsBodyLayout();
		physicsBodyLayout.name = _name;
		PhysicsBodyLayout.bodyLayouts[_name] = physicsBodyLayout;
		return physicsBodyLayout;
	}

	public static PhysicsBodyLayout New()
	{
		int num = 0;
		string text;
		for (;;)
		{
			text = string.Format("unnamed{0}", num);
			if (!PhysicsBodyLayout.bodyLayouts.ContainsKey(text))
			{
				break;
			}
			num++;
		}
		return PhysicsBodyLayout.New(text);
	}

	public bool Rename(string newName)
	{
		if (PhysicsBodyLayout.bodyLayouts.ContainsKey(newName))
		{
			return false;
		}
		PhysicsBodyLayout.bodyLayouts.Remove(this.name);
		PhysicsBodyLayout.bodyLayouts[newName] = this;
		this.name = newName;
		return true;
	}

	public static bool Remove(string _name)
	{
		return PhysicsBodyLayout.bodyLayouts.Remove(_name);
	}

	public static PhysicsBodyLayout Find(string _name)
	{
		PhysicsBodyLayout physicsBodyLayout = null;
		PhysicsBodyLayout.bodyLayouts.TryGetValue(_name, out physicsBodyLayout);
		return physicsBodyLayout;
	}

	public static PhysicsBodyLayout[] BodyLayouts
	{
		get
		{
			PhysicsBodyLayout[] array = new PhysicsBodyLayout[PhysicsBodyLayout.bodyLayouts.Count];
			PhysicsBodyLayout.bodyLayouts.CopyValuesTo(array);
			return array;
		}
	}

	public static void Reset()
	{
		PhysicsBodyLayout.bodyLayouts.Clear();
	}

	public static PhysicsBodyLayout Read(XElement _e)
	{
		if (!_e.HasAttribute("name"))
		{
			throw new Exception("Physics body needs a name");
		}
		PhysicsBodyLayout physicsBodyLayout = PhysicsBodyLayout.New(_e.GetAttribute("name"));
		foreach (XElement xelement in _e.Elements("collider"))
		{
			physicsBodyLayout.colliders.Add(PhysicsBodyColliderConfiguration.Read(xelement));
		}
		return physicsBodyLayout;
	}

	public void Write(XmlElement _elem)
	{
		XmlElement xmlElement = _elem.AddXmlElement("body").SetAttrib("name", this.name);
		for (int i = 0; i < this.colliders.Count; i++)
		{
			this.colliders[i].Write(xmlElement);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static Dictionary<string, PhysicsBodyLayout> bodyLayouts = new Dictionary<string, PhysicsBodyLayout>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<PhysicsBodyColliderConfiguration> colliders = new List<PhysicsBodyColliderConfiguration>();

	[PublicizedFrom(EAccessModifier.Private)]
	public string name;
}
