using System;
using System.Collections.Generic;
using System.Xml;
using JetBrains.Annotations;
using UnityEngine;

namespace Webserver.Permissions
{
	public class AdminWebModules : AdminSectionAbs
	{
		public static AdminWebModules Instance
		{
			get
			{
				return GameManager.Instance.adminTools.WebModules;
			}
		}

		public AdminWebModules(AdminTools _parent)
			: base(_parent, "webmodules")
		{
		}

		public override void Clear()
		{
			this.allModulesList.Clear();
			this.modules.Clear();
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override void ParseElement(XmlElement _childElement)
		{
			this.allModulesList.Clear();
			AdminWebModules.WebModule webModule;
			if (!AdminWebModules.WebModule.TryParse(_childElement, out webModule))
			{
				return;
			}
			AdminWebModules.WebModule webModule2;
			if (this.knownModules.TryGetValue(webModule.Name, out webModule2))
			{
				webModule = webModule.FixPermissionLevelsFromKnownModule(webModule2);
			}
			this.modules[webModule.Name] = webModule;
		}

		public override void Save(XmlElement _root)
		{
			XmlElement xmlElement = _root.AddXmlElement(this.SectionTypeName);
			foreach (KeyValuePair<string, AdminWebModules.WebModule> keyValuePair in this.modules)
			{
				string text;
				AdminWebModules.WebModule webModule;
				keyValuePair.Deconstruct(out text, out webModule);
				AdminWebModules.WebModule webModule2 = webModule;
				webModule2.ToXml(xmlElement);
			}
		}

		public void AddModule(AdminWebModules.WebModule _module)
		{
			AdminTools parent = this.Parent;
			lock (parent)
			{
				this.allModulesList.Clear();
				this.modules[_module.Name] = _module;
				this.Parent.Save();
			}
		}

		public bool RemoveModule(string _module)
		{
			AdminTools parent = this.Parent;
			bool flag3;
			lock (parent)
			{
				this.allModulesList.Clear();
				bool flag2 = this.modules.Remove(_module);
				if (flag2)
				{
					this.Parent.Save();
				}
				flag3 = flag2;
			}
			return flag3;
		}

		public List<AdminWebModules.WebModule> GetModules()
		{
			AdminTools parent = this.Parent;
			List<AdminWebModules.WebModule> list;
			lock (parent)
			{
				if (this.allModulesList.Count != 0)
				{
					list = this.allModulesList;
				}
				else
				{
					foreach (KeyValuePair<string, AdminWebModules.WebModule> keyValuePair in this.knownModules)
					{
						string text;
						AdminWebModules.WebModule webModule;
						keyValuePair.Deconstruct(out text, out webModule);
						string text2 = text;
						AdminWebModules.WebModule webModule2 = webModule;
						AdminWebModules.WebModule webModule3;
						this.allModulesList.Add(this.modules.TryGetValue(text2, out webModule3) ? webModule3 : webModule2);
					}
					list = this.allModulesList;
				}
			}
			return list;
		}

		public void AddKnownModule(AdminWebModules.WebModule _module)
		{
			if (!_module.IsDefault)
			{
				Log.Warning("Call to AddKnownModule with IsDefault==false! From:\n" + StackTraceUtility.ExtractStackTrace());
			}
			if (string.IsNullOrEmpty(_module.Name))
			{
				return;
			}
			AdminTools parent = this.Parent;
			lock (parent)
			{
				this.allModulesList.Clear();
				this.knownModules[_module.Name] = _module;
				AdminWebModules.WebModule webModule;
				if (this.modules.TryGetValue(_module.Name, out webModule))
				{
					webModule = webModule.FixPermissionLevelsFromKnownModule(_module);
					this.modules[_module.Name] = webModule;
				}
			}
		}

		public bool IsKnownModule(string _module)
		{
			if (string.IsNullOrEmpty(_module))
			{
				return false;
			}
			AdminTools parent = this.Parent;
			bool flag2;
			lock (parent)
			{
				flag2 = this.knownModules.ContainsKey(_module);
			}
			return flag2;
		}

		public bool ModuleAllowedWithLevel(string _module, int _level)
		{
			AdminTools parent = this.Parent;
			bool flag2;
			lock (parent)
			{
				flag2 = this.GetModule(_module).LevelGlobal >= _level;
			}
			return flag2;
		}

		public AdminWebModules.WebModule GetModule(string _module)
		{
			AdminTools parent = this.Parent;
			AdminWebModules.WebModule webModule2;
			lock (parent)
			{
				AdminWebModules.WebModule webModule;
				if (this.modules.TryGetValue(_module, out webModule))
				{
					webModule2 = webModule;
				}
				else
				{
					webModule2 = (this.knownModules.TryGetValue(_module, out webModule) ? webModule : this.defaultModulePermission);
				}
			}
			return webModule2;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly Dictionary<string, AdminWebModules.WebModule> modules = new CaseInsensitiveStringDictionary<AdminWebModules.WebModule>();

		public const int MethodLevelInheritGlobal = -2147483648;

		public const string MethodLevelInheritKeyword = "inherit";

		public const int MethodLevelNotSupported = -2147483647;

		public const int PermissionLevelUser = 1000;

		public const int PermissionLevelGuest = 2000;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly Dictionary<string, AdminWebModules.WebModule> knownModules = new CaseInsensitiveStringDictionary<AdminWebModules.WebModule>();

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly List<AdminWebModules.WebModule> allModulesList = new List<AdminWebModules.WebModule>();

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly AdminWebModules.WebModule defaultModulePermission = new AdminWebModules.WebModule("", 0, true);

		public readonly struct WebModule
		{
			public WebModule(string _name, int _level, bool _isDefault)
			{
				this.LevelPerMethod = null;
				this.Name = _name;
				this.LevelGlobal = _level;
				this.IsDefault = _isDefault;
			}

			public WebModule(string _name, int _levelGlobal, int[] _levelPerMethod, bool _isDefault)
			{
				if (_levelPerMethod != null && _levelPerMethod.Length != 7)
				{
					this.LevelPerMethod = AdminWebModules.WebModule.createDefaultPerMethodArray();
					for (int i = 0; i < 7; i++)
					{
						if (_levelPerMethod != null && i < _levelPerMethod.Length)
						{
							this.LevelPerMethod[i] = _levelPerMethod[i];
						}
					}
				}
				else
				{
					this.LevelPerMethod = _levelPerMethod;
				}
				this.Name = _name;
				this.LevelGlobal = _levelGlobal;
				this.IsDefault = _isDefault;
			}

			public void ToXml(XmlElement _parent)
			{
				bool flag = this.LevelPerMethod != null;
				XmlElement xmlElement = _parent.AddXmlElement("module").SetAttrib("name", this.Name).SetAttrib("permission_level", this.LevelGlobal.ToString());
				if (!flag)
				{
					return;
				}
				for (int i = 0; i < this.LevelPerMethod.Length; i++)
				{
					ERequestMethod erequestMethod = (ERequestMethod)i;
					int num = this.LevelPerMethod[i];
					if (num != -2147483647)
					{
						xmlElement.AddXmlElement("method").SetAttrib("name", erequestMethod.ToStringCached<ERequestMethod>()).SetAttrib("permission_level", (num == int.MinValue) ? "inherit" : num.ToString());
					}
				}
			}

			public static bool TryParse(XmlElement _element, out AdminWebModules.WebModule _result)
			{
				_result = default(AdminWebModules.WebModule);
				string text;
				if (!_element.TryGetAttribute("name", out text))
				{
					Log.Warning("[Web] [Perms] Ignoring module-entry because of missing 'name' attribute: " + _element.OuterXml);
					return false;
				}
				string text2;
				if (!_element.TryGetAttribute("permission_level", out text2))
				{
					Log.Warning("[Web] [Perms] Ignoring module-entry because of missing 'permission_level' attribute: " + _element.OuterXml);
					return false;
				}
				int num;
				if (!int.TryParse(text2, out num))
				{
					Log.Warning("[Web] [Perms] Ignoring module-entry because of invalid (non-numeric) value for 'permission_level' attribute: " + _element.OuterXml);
					return false;
				}
				int[] array = null;
				foreach (object obj in _element.ChildNodes)
				{
					XmlNode xmlNode = (XmlNode)obj;
					if (xmlNode.NodeType == XmlNodeType.Element)
					{
						XmlElement xmlElement = (XmlElement)xmlNode;
						string text3;
						ERequestMethod erequestMethod;
						if (xmlElement.Name != "method")
						{
							Log.Warning("[Web] [Perms] Ignoring module child element, invalid element name: " + xmlElement.OuterXml);
						}
						else if (!xmlElement.TryGetAttribute("name", out text3))
						{
							Log.Warning("[Web] [Perms] Ignoring module child element, missing 'name' attribute: " + xmlElement.OuterXml);
						}
						else if (!EnumUtils.TryParse<ERequestMethod>(text3, out erequestMethod, true))
						{
							Log.Warning("[Web] [Perms] Ignoring module child element, unknown method name in 'name' attribute: " + xmlElement.OuterXml);
						}
						else if (erequestMethod >= ERequestMethod.Count)
						{
							Log.Warning("[Web] [Perms] Ignoring module child element, invalid method name in 'name' attribute: " + xmlElement.OuterXml);
						}
						else if (!xmlElement.TryGetAttribute("permission_level", out text2))
						{
							Log.Warning("[Web] [Perms] Ignoring module child element, missing 'permission_level' attribute: " + xmlElement.OuterXml);
						}
						else
						{
							int minValue;
							if (text2.EqualsCaseInsensitive("inherit"))
							{
								minValue = int.MinValue;
							}
							else if (!int.TryParse(text2, out minValue))
							{
								Log.Warning("[Web] [Perms] Ignoring module child element, invalid (non-numeric) value for 'permission_level' attribute: " + xmlElement.OuterXml);
								continue;
							}
							if (array == null)
							{
								array = AdminWebModules.WebModule.createDefaultPerMethodArray();
							}
							array[(int)erequestMethod] = minValue;
						}
					}
				}
				_result = new AdminWebModules.WebModule(text, num, array, false);
				return true;
			}

			[MustUseReturnValue]
			public AdminWebModules.WebModule SetLevelGlobal(int _level)
			{
				int[] array = ((this.LevelPerMethod == null) ? null : new int[this.LevelPerMethod.Length]);
				if (array != null)
				{
					Array.Copy(this.LevelPerMethod, array, array.Length);
				}
				return new AdminWebModules.WebModule(this.Name, _level, array, false);
			}

			[MustUseReturnValue]
			public AdminWebModules.WebModule SetLevelForMethod(ERequestMethod _method, int _level)
			{
				int[] array = AdminWebModules.WebModule.createDefaultPerMethodArray();
				if (this.LevelPerMethod != null)
				{
					Array.Copy(this.LevelPerMethod, array, array.Length);
				}
				array[(int)_method] = _level;
				return new AdminWebModules.WebModule(this.Name, this.LevelGlobal, array, false);
			}

			[PublicizedFrom(EAccessModifier.Private)]
			public static int[] createDefaultPerMethodArray()
			{
				int[] array = new int[7];
				for (int i = 0; i < 7; i++)
				{
					array[i] = -2147483647;
				}
				return array;
			}

			[MustUseReturnValue]
			public AdminWebModules.WebModule FixPermissionLevelsFromKnownModule(AdminWebModules.WebModule _knownModule)
			{
				if (_knownModule.LevelPerMethod != null)
				{
					AdminWebModules.WebModule webModule = this;
					for (int i = 0; i < _knownModule.LevelPerMethod.Length; i++)
					{
						if (webModule.LevelPerMethod == null || webModule.LevelPerMethod[i] == -2147483647)
						{
							webModule = webModule.SetLevelForMethod((ERequestMethod)i, _knownModule.LevelPerMethod[i]);
						}
					}
					return webModule;
				}
				if (this.LevelPerMethod != null)
				{
					return new AdminWebModules.WebModule(this.Name, this.LevelGlobal, false);
				}
				return this;
			}

			public readonly string Name;

			public readonly int LevelGlobal;

			public readonly int[] LevelPerMethod;

			public readonly bool IsDefault;
		}
	}
}
