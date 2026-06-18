using System;
using System.Collections.Generic;
using System.Xml;

public class AdminWhitelist : AdminSectionAbs
{
	public AdminWhitelist(AdminTools _parent)
		: base(_parent, "whitelist")
	{
	}

	public override void Clear()
	{
		this.whitelistedUsers.Clear();
		this.whitelistedGroups.Clear();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void ParseElement(XmlElement _childElement)
	{
		AdminWhitelist.WhitelistUser whitelistUser;
		if (_childElement.Name == "group")
		{
			AdminWhitelist.WhitelistGroup whitelistGroup;
			if (AdminWhitelist.WhitelistGroup.TryParse(_childElement, out whitelistGroup))
			{
				this.whitelistedGroups[whitelistGroup.SteamIdGroup] = whitelistGroup;
				return;
			}
		}
		else if (AdminWhitelist.WhitelistUser.TryParse(_childElement, out whitelistUser))
		{
			this.whitelistedUsers[whitelistUser.UserIdentifier] = whitelistUser;
		}
	}

	public override void Save(XmlElement _root)
	{
		XmlElement xmlElement = _root.AddXmlElement(this.SectionTypeName);
		xmlElement.AddXmlComment(" ONLY PUT ITEMS IN WHITELIST IF YOU WANT WHITELIST ONLY ENABLED!!! ");
		xmlElement.AddXmlComment(" If there are any items in the whitelist, the whitelist only mode is enabled ");
		xmlElement.AddXmlComment(" Nobody can join that ISN'T in the whitelist or admins once whitelist only mode is enabled ");
		xmlElement.AddXmlComment(" Name is optional for display purposes only ");
		xmlElement.AddXmlComment(" <user platform=\"\" userid=\"\" name=\"\" /> ");
		xmlElement.AddXmlComment(" <group steamID=\"\" name=\"\" /> ");
		foreach (KeyValuePair<PlatformUserIdentifierAbs, AdminWhitelist.WhitelistUser> keyValuePair in this.whitelistedUsers)
		{
			keyValuePair.Value.ToXml(xmlElement);
		}
		foreach (KeyValuePair<string, AdminWhitelist.WhitelistGroup> keyValuePair2 in this.whitelistedGroups)
		{
			keyValuePair2.Value.ToXml(xmlElement);
		}
	}

	public void AddUser(string _name, PlatformUserIdentifierAbs _identifier)
	{
		AdminTools parent = this.Parent;
		lock (parent)
		{
			AdminWhitelist.WhitelistUser whitelistUser = new AdminWhitelist.WhitelistUser(_name, _identifier);
			this.whitelistedUsers[_identifier] = whitelistUser;
			this.Parent.Save();
		}
	}

	public bool RemoveUser(PlatformUserIdentifierAbs _identifier)
	{
		AdminTools parent = this.Parent;
		bool flag3;
		lock (parent)
		{
			bool flag2 = this.whitelistedUsers.Remove(_identifier);
			if (flag2)
			{
				this.Parent.Save();
			}
			flag3 = flag2;
		}
		return flag3;
	}

	public Dictionary<PlatformUserIdentifierAbs, AdminWhitelist.WhitelistUser> GetUsers()
	{
		AdminTools parent = this.Parent;
		Dictionary<PlatformUserIdentifierAbs, AdminWhitelist.WhitelistUser> dictionary;
		lock (parent)
		{
			dictionary = this.whitelistedUsers;
		}
		return dictionary;
	}

	public void AddGroup(string _name, string _steamId)
	{
		AdminTools parent = this.Parent;
		lock (parent)
		{
			AdminWhitelist.WhitelistGroup whitelistGroup = new AdminWhitelist.WhitelistGroup(_name, _steamId);
			this.whitelistedGroups[_steamId] = whitelistGroup;
			this.Parent.Save();
		}
	}

	public bool RemoveGroup(string _steamId)
	{
		AdminTools parent = this.Parent;
		bool flag3;
		lock (parent)
		{
			bool flag2 = this.whitelistedGroups.Remove(_steamId);
			if (flag2)
			{
				this.Parent.Save();
			}
			flag3 = flag2;
		}
		return flag3;
	}

	public Dictionary<string, AdminWhitelist.WhitelistGroup> GetGroups()
	{
		AdminTools parent = this.Parent;
		Dictionary<string, AdminWhitelist.WhitelistGroup> dictionary;
		lock (parent)
		{
			dictionary = this.whitelistedGroups;
		}
		return dictionary;
	}

	public bool IsWhitelisted(ClientInfo _clientInfo)
	{
		AdminTools parent = this.Parent;
		bool flag2;
		lock (parent)
		{
			if (this.whitelistedUsers.ContainsKey(_clientInfo.PlatformId) || this.whitelistedUsers.ContainsKey(_clientInfo.CrossplatformId))
			{
				flag2 = true;
			}
			else
			{
				foreach (KeyValuePair<string, int> keyValuePair in _clientInfo.groupMemberships)
				{
					if (this.whitelistedGroups.ContainsKey(keyValuePair.Key))
					{
						return true;
					}
				}
				flag2 = false;
			}
		}
		return flag2;
	}

	public bool IsWhiteListEnabled()
	{
		AdminTools parent = this.Parent;
		bool flag2;
		lock (parent)
		{
			flag2 = this.whitelistedUsers.Count > 0 || this.whitelistedGroups.Count > 0;
		}
		return flag2;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<PlatformUserIdentifierAbs, AdminWhitelist.WhitelistUser> whitelistedUsers = new Dictionary<PlatformUserIdentifierAbs, AdminWhitelist.WhitelistUser>();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<string, AdminWhitelist.WhitelistGroup> whitelistedGroups = new Dictionary<string, AdminWhitelist.WhitelistGroup>();

	public readonly struct WhitelistUser
	{
		public WhitelistUser(string _name, PlatformUserIdentifierAbs _userIdentifier)
		{
			this.Name = _name;
			this.UserIdentifier = _userIdentifier;
		}

		public void ToXml(XmlElement _parent)
		{
			XmlElement xmlElement = _parent.AddXmlElement("user");
			this.UserIdentifier.ToXml(xmlElement, "");
			if (this.Name != null)
			{
				xmlElement.SetAttrib("name", this.Name);
			}
		}

		public static bool TryParse(XmlElement _element, out AdminWhitelist.WhitelistUser _result)
		{
			_result = default(AdminWhitelist.WhitelistUser);
			string text = _element.GetAttribute("name");
			if (text.Length == 0)
			{
				text = null;
			}
			PlatformUserIdentifierAbs platformUserIdentifierAbs = AdminTools.ParseUserIdentifier(_element);
			if (platformUserIdentifierAbs == null)
			{
				return false;
			}
			_result = new AdminWhitelist.WhitelistUser(text, platformUserIdentifierAbs);
			return true;
		}

		public readonly string Name;

		public readonly PlatformUserIdentifierAbs UserIdentifier;
	}

	public readonly struct WhitelistGroup
	{
		public WhitelistGroup(string _name, string _steamIdGroup)
		{
			this.Name = _name;
			this.SteamIdGroup = _steamIdGroup;
		}

		public void ToXml(XmlElement _parent)
		{
			XmlElement xmlElement = _parent.AddXmlElement("group");
			xmlElement.SetAttrib("steamID", this.SteamIdGroup);
			if (this.Name != null)
			{
				xmlElement.SetAttrib("name", this.Name);
			}
		}

		public static bool TryParse(XmlElement _element, out AdminWhitelist.WhitelistGroup _result)
		{
			_result = default(AdminWhitelist.WhitelistGroup);
			string text = _element.GetAttribute("name");
			if (text.Length == 0)
			{
				text = null;
			}
			if (!_element.HasAttribute("steamID"))
			{
				Log.Warning("Ignoring whitelist-entry because of missing 'steamID' attribute: " + _element.OuterXml);
				return false;
			}
			string attribute = _element.GetAttribute("steamID");
			_result = new AdminWhitelist.WhitelistGroup(text, attribute);
			return true;
		}

		public readonly string Name;

		public readonly string SteamIdGroup;
	}
}
