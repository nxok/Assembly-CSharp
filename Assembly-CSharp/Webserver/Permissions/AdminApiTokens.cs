using System;
using System.Collections.Generic;
using System.Xml;

namespace Webserver.Permissions
{
	public class AdminApiTokens : AdminSectionAbs
	{
		public static AdminApiTokens Instance
		{
			get
			{
				return GameManager.Instance.adminTools.ApiTokens;
			}
		}

		public AdminApiTokens(AdminTools _parent)
			: base(_parent, "apitokens")
		{
		}

		public override void Clear()
		{
			this.tokens.Clear();
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override void ParseElement(XmlElement _childElement)
		{
			AdminApiTokens.ApiToken apiToken;
			if (AdminApiTokens.ApiToken.TryParse(_childElement, out apiToken))
			{
				this.tokens[apiToken.Name] = apiToken;
			}
		}

		public override void Save(XmlElement _root)
		{
			XmlElement xmlElement = _root.AddXmlElement(this.SectionTypeName);
			xmlElement.AddXmlComment(" <token name=\"adminuser1\" secret=\"supersecrettoken\" permission_level=\"0\" /> ");
			foreach (KeyValuePair<string, AdminApiTokens.ApiToken> keyValuePair in this.tokens)
			{
				string text;
				AdminApiTokens.ApiToken apiToken;
				keyValuePair.Deconstruct(out text, out apiToken);
				AdminApiTokens.ApiToken apiToken2 = apiToken;
				apiToken2.ToXml(xmlElement);
			}
		}

		public void AddToken(string _name, string _secret, int _permissionLevel)
		{
			AdminTools parent = this.Parent;
			lock (parent)
			{
				AdminApiTokens.ApiToken apiToken = new AdminApiTokens.ApiToken(_name, _secret, _permissionLevel);
				this.tokens[_name] = apiToken;
				this.Parent.Save();
			}
		}

		public bool RemoveToken(string _name)
		{
			AdminTools parent = this.Parent;
			bool flag3;
			lock (parent)
			{
				bool flag2 = this.tokens.Remove(_name);
				if (flag2)
				{
					this.Parent.Save();
				}
				flag3 = flag2;
			}
			return flag3;
		}

		public Dictionary<string, AdminApiTokens.ApiToken> GetTokens()
		{
			AdminTools parent = this.Parent;
			Dictionary<string, AdminApiTokens.ApiToken> dictionary;
			lock (parent)
			{
				dictionary = this.tokens;
			}
			return dictionary;
		}

		public int GetPermissionLevel(string _name, string _secret)
		{
			AdminTools parent = this.Parent;
			int num;
			lock (parent)
			{
				AdminApiTokens.ApiToken apiToken;
				if (this.tokens.TryGetValue(_name, out apiToken) && apiToken.Secret == _secret)
				{
					num = apiToken.PermissionLevel;
				}
				else if (this.IsCommandlineToken(_name, _secret))
				{
					num = 0;
				}
				else
				{
					num = int.MaxValue;
				}
			}
			return num;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public bool IsCommandlineToken(string _name, string _secret)
		{
			if (!this.commandlineChecked)
			{
				this.commandlineTokenName = GameUtils.GetLaunchArgument("webapitokenname");
				this.commandlineTokenSecret = GameUtils.GetLaunchArgument("webapitokensecret");
				this.commandlineChecked = true;
			}
			return !string.IsNullOrEmpty(this.commandlineTokenName) && !string.IsNullOrEmpty(this.commandlineTokenSecret) && _name == this.commandlineTokenName && _secret == this.commandlineTokenSecret;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly Dictionary<string, AdminApiTokens.ApiToken> tokens = new CaseInsensitiveStringDictionary<AdminApiTokens.ApiToken>();

		[PublicizedFrom(EAccessModifier.Private)]
		public bool commandlineChecked;

		[PublicizedFrom(EAccessModifier.Private)]
		public string commandlineTokenName;

		[PublicizedFrom(EAccessModifier.Private)]
		public string commandlineTokenSecret;

		public readonly struct ApiToken
		{
			public ApiToken(string _name, string _secret, int _permissionLevel)
			{
				this.Name = _name;
				this.Secret = _secret;
				this.PermissionLevel = _permissionLevel;
			}

			public void ToXml(XmlElement _parent)
			{
				_parent.AddXmlElement("token").SetAttrib("name", this.Name).SetAttrib("secret", this.Secret)
					.SetAttrib("permission_level", this.PermissionLevel.ToString());
			}

			public static bool TryParse(XmlElement _element, out AdminApiTokens.ApiToken _result)
			{
				_result = default(AdminApiTokens.ApiToken);
				string text;
				if (!_element.TryGetAttribute("name", out text))
				{
					Log.Warning("[Web] [Perms] Ignoring apitoken-entry because of missing 'name' attribute: " + _element.OuterXml);
					return false;
				}
				string text2;
				if (!_element.TryGetAttribute("secret", out text2))
				{
					Log.Warning("[Web] [Perms] Ignoring apitoken-entry because of missing 'secret' attribute: " + _element.OuterXml);
					return false;
				}
				string text3;
				if (!_element.TryGetAttribute("permission_level", out text3))
				{
					Log.Warning("[Web] [Perms] Ignoring apitoken-entry because of missing 'permission_level' attribute: " + _element.OuterXml);
					return false;
				}
				int num;
				if (!int.TryParse(text3, out num))
				{
					Log.Warning("[Web] [Perms] Ignoring apitoken-entry because of invalid (non-numeric) value for 'permission_level' attribute: " + _element.OuterXml);
					return false;
				}
				_result = new AdminApiTokens.ApiToken(text, text2, num);
				return true;
			}

			public readonly string Name;

			public readonly string Secret;

			public readonly int PermissionLevel;
		}
	}
}
