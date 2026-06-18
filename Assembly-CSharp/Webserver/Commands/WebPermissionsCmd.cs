using System;
using System.Collections.Generic;
using UnityEngine.Scripting;
using Webserver.Permissions;

namespace Webserver.Commands
{
	[Preserve]
	public class WebPermissionsCmd : ConsoleCmdAbstract
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		public override string[] getCommands()
		{
			return new string[] { "webpermission" };
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override string getDescription()
		{
			return "Manage web permission levels";
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override string getHelp()
		{
			return "\n\t\t\t\t|Set/get permission levels required to access a given web functionality. Default\n\t\t\t    |level required for functions that are not explicitly specified is 0.\n\t\t\t    |Usage:\n\t\t\t\t|   1. webpermission add <webfunction> <method> <level>\n\t\t\t    |   2. webpermission remove <webfunction>\n\t\t\t    |   3. webpermission list [includedefaults]\n\t\t\t\t|1. Add a new override (or replace the existing one) for the given function. Method must be a HTTP method (like 'GET', 'POST')\n\t\t\t\t\t\tsupported by the function or the keyword 'global' for a per-API permission level. Use the permission level keyword\n\t\t\t\t\t\t'inherit' to use the per-API permission level for the specified method instead of a custom one for just the single method.\n\t\t\t\t|2. Removes any custom overrides for the specified function.\n\t\t\t\t|3. List all permissions. Pass in 'true' for the includedefaults argument to also show functions that do not have a custom override defined.\n\t\t\t\t".Unindent(true);
		}

		public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
		{
			if (_params.Count < 1)
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("No sub command given.");
				return;
			}
			if (_params[0].EqualsCaseInsensitive("add"))
			{
				this.ExecuteAdd(_params);
				return;
			}
			if (_params[0].EqualsCaseInsensitive("remove"))
			{
				this.ExecuteRemove(_params);
				return;
			}
			if (_params[0].EqualsCaseInsensitive("list"))
			{
				this.ExecuteList(_params);
				return;
			}
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Invalid sub command \"" + _params[0] + "\".");
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void ExecuteAdd(List<string> _params)
		{
			if (_params.Count != 4)
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("Wrong number of arguments, expected 4, found {0}.", _params.Count));
				return;
			}
			string text = _params[1];
			string text2 = _params[2];
			string text3 = _params[3];
			ERequestMethod erequestMethod = ERequestMethod.Count;
			bool flag = false;
			bool flag2 = false;
			if (!AdminWebModules.Instance.IsKnownModule(text))
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("\"" + text + "\" is not a valid web function.");
				return;
			}
			AdminWebModules.WebModule webModule = AdminWebModules.Instance.GetModule(text);
			if (text2.EqualsCaseInsensitive("global"))
			{
				flag = true;
			}
			else
			{
				if (!EnumUtils.TryParse<ERequestMethod>(text2, out erequestMethod, true))
				{
					SingletonMonoBehaviour<SdtdConsole>.Instance.Output("\"" + text2 + "\" is neither a valid HTTP method nor the 'global' keyword.");
					return;
				}
				if (webModule.LevelPerMethod == null || webModule.LevelPerMethod[(int)erequestMethod] == -2147483647)
				{
					SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Concat(new string[] { "\"", text2, "\" is not a method supported by the \"", text, "\" function." }));
					return;
				}
			}
			int minValue;
			if (text3.EqualsCaseInsensitive("inherit"))
			{
				if (flag)
				{
					SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Permission level can not use the 'inherit' keyword with the 'global' method keyword.");
					return;
				}
				flag2 = true;
				minValue = int.MinValue;
			}
			else if (!int.TryParse(text3, out minValue))
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("\"" + text3 + "\" is neither a valid integer nor the 'inherit' keyword.");
				return;
			}
			if (flag)
			{
				webModule = webModule.SetLevelGlobal(minValue);
			}
			else
			{
				webModule = webModule.SetLevelForMethod(erequestMethod, minValue);
			}
			AdminWebModules.Instance.AddModule(webModule);
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Concat(new string[]
			{
				text,
				", method ",
				text2,
				" added ",
				flag2 ? ", inheriting the APIs global permission level" : ("with permission level " + minValue.ToString()),
				"."
			}));
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void ExecuteRemove(List<string> _params)
		{
			if (_params.Count != 2)
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("Wrong number of arguments, expected 2, found {0}.", _params.Count));
				return;
			}
			if (!AdminWebModules.Instance.IsKnownModule(_params[1]))
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("\"" + _params[1] + "\" is not a valid web function.");
				return;
			}
			AdminWebModules.Instance.RemoveModule(_params[1]);
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output(_params[1] + " removed from permissions list.");
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void ExecuteList(List<string> _params)
		{
			bool flag = _params.Count > 1 && ConsoleHelper.ParseParamBool(_params[1], true);
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Defined web function permissions:");
			List<AdminWebModules.WebModule> modules = AdminWebModules.Instance.GetModules();
			for (int i = 0; i < modules.Count; i++)
			{
				AdminWebModules.WebModule webModule = modules[i];
				if (flag || !webModule.IsDefault)
				{
					SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("  {0,-25}: {1,4}{2}", webModule.Name, webModule.LevelGlobal, webModule.IsDefault ? " (default permissions)" : ""));
					if (webModule.LevelPerMethod != null)
					{
						for (int j = 0; j < webModule.LevelPerMethod.Length; j++)
						{
							int num = webModule.LevelPerMethod[j];
							ERequestMethod erequestMethod = (ERequestMethod)j;
							if (num != -2147483647)
							{
								if (num == -2147483648)
								{
									SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("  {0,25}: {1,4} (Using API level)", erequestMethod.ToStringCached<ERequestMethod>(), webModule.LevelGlobal));
								}
								else
								{
									SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("  {0,25}: {1,4}", erequestMethod.ToStringCached<ERequestMethod>(), num));
								}
							}
						}
					}
				}
			}
		}
	}
}
