using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

public class AdminBlacklist : AdminSectionAbs
{
	public AdminBlacklist(AdminTools _parent)
		: base(_parent, "blacklist")
	{
	}

	public override void Clear()
	{
		this.bannedUsers.Clear();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void ParseElement(XmlElement _childElement)
	{
		AdminBlacklist.BannedUser bannedUser;
		if (AdminBlacklist.BannedUser.TryParse(_childElement, out bannedUser))
		{
			this.bannedUsers[bannedUser.UserIdentifier] = bannedUser;
		}
	}

	public override void Save(XmlElement _root)
	{
		XmlElement xmlElement = _root.AddXmlElement("blacklist");
		xmlElement.AddXmlComment(" <blacklisted platform=\"\" userid=\"\" name=\"\" unbandate=\"\" reason=\"\" /> ");
		foreach (KeyValuePair<PlatformUserIdentifierAbs, AdminBlacklist.BannedUser> keyValuePair in this.bannedUsers)
		{
			keyValuePair.Value.ToXml(xmlElement);
		}
	}

	public void AddBan(string _name, PlatformUserIdentifierAbs _identifier, DateTime _banUntil, string _banReason)
	{
		AdminTools parent = this.Parent;
		lock (parent)
		{
			AdminBlacklist.BannedUser bannedUser = new AdminBlacklist.BannedUser(_name, _identifier, _banUntil, _banReason);
			this.bannedUsers[_identifier] = bannedUser;
			if (_banUntil > DateTime.Now)
			{
				this.Parent.Users.RemoveUser(_identifier, false);
			}
			this.Parent.Save();
		}
	}

	public bool RemoveBan(PlatformUserIdentifierAbs _identifier)
	{
		AdminTools parent = this.Parent;
		bool flag3;
		lock (parent)
		{
			bool flag2 = this.bannedUsers.Remove(_identifier);
			if (flag2)
			{
				this.Parent.Save();
			}
			flag3 = flag2;
		}
		return flag3;
	}

	public bool IsBanned(PlatformUserIdentifierAbs _identifier, out DateTime _bannedUntil, out string _reason)
	{
		AdminTools parent = this.Parent;
		bool flag2;
		lock (parent)
		{
			if (this.bannedUsers.ContainsKey(_identifier))
			{
				AdminBlacklist.BannedUser bannedUser = this.bannedUsers[_identifier];
				if (bannedUser.BannedUntil > DateTime.Now)
				{
					_bannedUntil = bannedUser.BannedUntil;
					_reason = bannedUser.BanReason;
					return true;
				}
			}
			_bannedUntil = DateTime.Now;
			_reason = string.Empty;
			flag2 = false;
		}
		return flag2;
	}

	public List<AdminBlacklist.BannedUser> GetBanned()
	{
		AdminTools parent = this.Parent;
		List<AdminBlacklist.BannedUser> list;
		lock (parent)
		{
			list = this.bannedUsers.Values.Where((AdminBlacklist.BannedUser _b) => _b.BannedUntil > DateTime.Now).ToList<AdminBlacklist.BannedUser>();
		}
		return list;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<PlatformUserIdentifierAbs, AdminBlacklist.BannedUser> bannedUsers = new Dictionary<PlatformUserIdentifierAbs, AdminBlacklist.BannedUser>();

	public readonly struct BannedUser
	{
		public BannedUser(string _name, PlatformUserIdentifierAbs _userIdentifier, DateTime _banUntil, string _banReason)
		{
			this.Name = _name;
			this.UserIdentifier = _userIdentifier;
			this.BannedUntil = _banUntil;
			this.BanReason = _banReason ?? string.Empty;
		}

		public void ToXml(XmlElement _parent)
		{
			XmlElement xmlElement = _parent.AddXmlElement("blacklisted");
			this.UserIdentifier.ToXml(xmlElement, "");
			if (this.Name != null)
			{
				xmlElement.SetAttrib("name", this.Name);
			}
			xmlElement.SetAttrib("unbandate", this.BannedUntil.ToCultureInvariantString());
			xmlElement.SetAttrib("reason", this.BanReason);
		}

		public static bool TryParse(XmlElement _element, out AdminBlacklist.BannedUser _result)
		{
			_result = default(AdminBlacklist.BannedUser);
			string text = _element.GetAttribute("name");
			if (text.Length == 0)
			{
				text = null;
			}
			if (!_element.HasAttribute("unbandate"))
			{
				Log.Warning("Ignoring blacklist-entry because of missing 'unbandate' attribute: " + _element.OuterXml);
				return false;
			}
			DateTime dateTime;
			if (!StringParsers.TryParseDateTime(_element.GetAttribute("unbandate"), out dateTime) && !DateTime.TryParse(_element.GetAttribute("unbandate"), out dateTime))
			{
				Log.Warning("Ignoring blacklist-entry because of invalid value for 'unbandate' attribute: " + _element.OuterXml);
				return false;
			}
			PlatformUserIdentifierAbs platformUserIdentifierAbs = AdminTools.ParseUserIdentifier(_element);
			if (platformUserIdentifierAbs == null)
			{
				return false;
			}
			string attribute = _element.GetAttribute("reason");
			_result = new AdminBlacklist.BannedUser(text, platformUserIdentifierAbs, dateTime, attribute);
			return true;
		}

		public readonly string Name;

		public readonly PlatformUserIdentifierAbs UserIdentifier;

		public readonly DateTime BannedUntil;

		public readonly string BanReason;
	}
}
