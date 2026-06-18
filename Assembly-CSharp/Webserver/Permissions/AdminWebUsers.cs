using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Xml;

namespace Webserver.Permissions
{
	public class AdminWebUsers : AdminSectionAbs
	{
		public static AdminWebUsers Instance
		{
			get
			{
				return GameManager.Instance.adminTools.WebUsers;
			}
		}

		public AdminWebUsers(AdminTools _parent)
			: base(_parent, "webusers")
		{
		}

		public override void Clear()
		{
			this.users.Clear();
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override void ParseElement(XmlElement _childElement)
		{
			AdminWebUsers.WebUser webUser;
			if (AdminWebUsers.WebUser.TryParse(_childElement, out webUser))
			{
				this.users[webUser.Name] = webUser;
			}
		}

		public override void Save(XmlElement _root)
		{
			XmlElement xmlElement = _root.AddXmlElement(this.SectionTypeName);
			foreach (KeyValuePair<string, AdminWebUsers.WebUser> keyValuePair in this.users)
			{
				string text;
				AdminWebUsers.WebUser webUser;
				keyValuePair.Deconstruct(out text, out webUser);
				AdminWebUsers.WebUser webUser2 = webUser;
				webUser2.ToXml(xmlElement);
			}
		}

		public void AddUser(string _name, string _password, PlatformUserIdentifierAbs _userIdentifier, PlatformUserIdentifierAbs _crossPlatformIdentifier)
		{
			AdminTools parent = this.Parent;
			lock (parent)
			{
				AdminWebUsers.WebUser webUser = new AdminWebUsers.WebUser(_name, _password, _userIdentifier, _crossPlatformIdentifier);
				this.users[_name] = webUser;
				this.Parent.Save();
			}
		}

		public bool RemoveUser(string _name)
		{
			AdminTools parent = this.Parent;
			bool flag3;
			lock (parent)
			{
				bool flag2 = this.users.Remove(_name);
				if (flag2)
				{
					this.Parent.Save();
				}
				flag3 = flag2;
			}
			return flag3;
		}

		public Dictionary<string, AdminWebUsers.WebUser> GetUsers()
		{
			AdminTools parent = this.Parent;
			Dictionary<string, AdminWebUsers.WebUser> dictionary;
			lock (parent)
			{
				dictionary = this.users;
			}
			return dictionary;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static byte[] Hash(string _input)
		{
			return new MD5Cng().ComputeHash(Encoding.UTF8.GetBytes(_input));
		}

		public bool TryGetUser(string _name, string _password, out AdminWebUsers.WebUser _result)
		{
			AdminTools parent = this.Parent;
			bool flag2;
			lock (parent)
			{
				if (this.users.TryGetValue(_name, out _result) && _result.ValidatePassword(_password))
				{
					flag2 = true;
				}
				else
				{
					_result = default(AdminWebUsers.WebUser);
					flag2 = false;
				}
			}
			return flag2;
		}

		public bool HasUser(PlatformUserIdentifierAbs _platformUser, PlatformUserIdentifierAbs _crossPlatformUser, out AdminWebUsers.WebUser _result)
		{
			AdminTools parent = this.Parent;
			bool flag2;
			lock (parent)
			{
				_result = default(AdminWebUsers.WebUser);
				foreach (KeyValuePair<string, AdminWebUsers.WebUser> keyValuePair in this.users)
				{
					string text;
					AdminWebUsers.WebUser webUser;
					keyValuePair.Deconstruct(out text, out webUser);
					AdminWebUsers.WebUser webUser2 = webUser;
					if (object.Equals(webUser2.PlatformUser, _platformUser) && object.Equals(webUser2.CrossPlatformUser, _crossPlatformUser))
					{
						_result = webUser2;
						return true;
					}
				}
				flag2 = false;
			}
			return flag2;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly Dictionary<string, AdminWebUsers.WebUser> users = new CaseInsensitiveStringDictionary<AdminWebUsers.WebUser>();

		public readonly struct WebUser
		{
			public WebUser(string _name, byte[] _passwordHash, PlatformUserIdentifierAbs _platformUser, PlatformUserIdentifierAbs _crossPlatformUser)
			{
				this.Name = _name;
				this.PasswordHash = _passwordHash;
				this.PlatformUser = _platformUser;
				this.CrossPlatformUser = _crossPlatformUser;
			}

			public WebUser(string _name, string _password, PlatformUserIdentifierAbs _platformUser, PlatformUserIdentifierAbs _crossPlatformUser)
			{
				this.Name = _name;
				this.PasswordHash = AdminWebUsers.Hash(_password);
				this.PlatformUser = _platformUser;
				this.CrossPlatformUser = _crossPlatformUser;
			}

			public void ToXml(XmlElement _parent)
			{
				XmlElement xmlElement = _parent.AddXmlElement("user");
				xmlElement.SetAttrib("name", this.Name).SetAttrib("pass", Convert.ToBase64String(this.PasswordHash));
				this.PlatformUser.ToXml(xmlElement, "");
				PlatformUserIdentifierAbs crossPlatformUser = this.CrossPlatformUser;
				if (crossPlatformUser == null)
				{
					return;
				}
				crossPlatformUser.ToXml(xmlElement, "cross");
			}

			public static bool TryParse(XmlElement _element, out AdminWebUsers.WebUser _result)
			{
				_result = default(AdminWebUsers.WebUser);
				string text;
				if (!_element.TryGetAttribute("name", out text))
				{
					Log.Warning("[Web] [Perms] Ignoring user-entry because of missing 'name' attribute: " + _element.OuterXml);
					return false;
				}
				string text2;
				if (!_element.TryGetAttribute("pass", out text2))
				{
					Log.Warning("[Web] [Perms] Ignoring user-entry because of missing 'pass' attribute: " + _element.OuterXml);
					return false;
				}
				PlatformUserIdentifierAbs platformUserIdentifierAbs = PlatformUserIdentifierAbs.FromXml(_element, false, null);
				if (platformUserIdentifierAbs == null)
				{
					Log.Warning("[Web] [Perms] Ignoring user-entry because of missing 'platform' or 'userid' attribute: " + _element.OuterXml);
					return false;
				}
				PlatformUserIdentifierAbs platformUserIdentifierAbs2 = PlatformUserIdentifierAbs.FromXml(_element, false, "cross");
				byte[] array = Convert.FromBase64String(text2);
				_result = new AdminWebUsers.WebUser(text, array, platformUserIdentifierAbs, platformUserIdentifierAbs2);
				return true;
			}

			public bool ValidatePassword(string _password)
			{
				return Utils.ArrayEquals(AdminWebUsers.Hash(_password), this.PasswordHash);
			}

			public readonly string Name;

			public readonly byte[] PasswordHash;

			public readonly PlatformUserIdentifierAbs PlatformUser;

			public readonly PlatformUserIdentifierAbs CrossPlatformUser;
		}
	}
}
