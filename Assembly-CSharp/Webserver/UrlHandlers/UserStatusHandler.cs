using System;
using System.Collections.Generic;
using System.Net;
using Utf8Json;
using Webserver.Permissions;

namespace Webserver.UrlHandlers
{
	public class UserStatusHandler : AbsHandler
	{
		public UserStatusHandler(string _moduleName = null)
			: base(_moduleName, 0)
		{
		}

		[PublicizedFrom(EAccessModifier.Private)]
		static UserStatusHandler()
		{
			for (int i = 0; i < UserStatusHandler.jsonMethodNameKeys.Length; i++)
			{
				ERequestMethod erequestMethod = (ERequestMethod)i;
				UserStatusHandler.jsonMethodNameKeys[i] = JsonWriter.GetEncodedPropertyName(erequestMethod.ToStringCached<ERequestMethod>());
			}
		}

		public override void HandleRequest(RequestContext _context)
		{
			JsonWriter jsonWriter;
			WebUtils.PrepareEnvelopedResult(out jsonWriter);
			jsonWriter.WriteRaw(UserStatusHandler.jsonLoggedInKey);
			jsonWriter.WriteBoolean(_context.Connection != null);
			jsonWriter.WriteRaw(UserStatusHandler.jsonUsernameKey);
			jsonWriter.WriteString((_context.Connection != null) ? _context.Connection.Username : string.Empty);
			jsonWriter.WriteRaw(UserStatusHandler.jsonPermissionLevelKey);
			jsonWriter.WriteInt32(_context.PermissionLevel);
			jsonWriter.WriteRaw(UserStatusHandler.jsonPermissionsKey);
			jsonWriter.WriteBeginArray();
			List<AdminWebModules.WebModule> modules = AdminWebModules.Instance.GetModules();
			for (int i = 0; i < modules.Count; i++)
			{
				AdminWebModules.WebModule webModule = modules[i];
				if (i > 0)
				{
					jsonWriter.WriteValueSeparator();
				}
				jsonWriter.WriteRaw(UserStatusHandler.jsonModuleKey);
				jsonWriter.WriteString(webModule.Name);
				jsonWriter.WriteRaw(UserStatusHandler.jsonAllowedKey);
				jsonWriter.WriteBeginObject();
				if (webModule.LevelPerMethod == null)
				{
					jsonWriter.WriteRaw(UserStatusHandler.jsonMethodNameKeys[1]);
					jsonWriter.WriteBoolean(webModule.LevelGlobal >= _context.PermissionLevel);
				}
				else
				{
					bool flag = true;
					for (int j = 0; j < webModule.LevelPerMethod.Length; j++)
					{
						int num = webModule.LevelPerMethod[j];
						if (num != -2147483647)
						{
							if (num == -2147483648)
							{
								num = webModule.LevelGlobal;
							}
							if (!flag)
							{
								jsonWriter.WriteValueSeparator();
							}
							flag = false;
							jsonWriter.WriteRaw(UserStatusHandler.jsonMethodNameKeys[j]);
							jsonWriter.WriteBoolean(num >= _context.PermissionLevel);
						}
					}
				}
				jsonWriter.WriteEndObject();
				jsonWriter.WriteEndObject();
			}
			jsonWriter.WriteEndArray();
			jsonWriter.WriteEndObject();
			WebUtils.SendEnvelopedResult(_context, ref jsonWriter, HttpStatusCode.OK, null, null, null);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly byte[] jsonLoggedInKey = JsonWriter.GetEncodedPropertyNameWithBeginObject("loggedIn");

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly byte[] jsonUsernameKey = JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("username");

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly byte[] jsonPermissionLevelKey = JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("permissionLevel");

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly byte[] jsonPermissionsKey = JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("permissions");

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly byte[] jsonModuleKey = JsonWriter.GetEncodedPropertyNameWithBeginObject("module");

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly byte[] jsonAllowedKey = JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("allowed");

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly byte[][] jsonMethodNameKeys = new byte[7][];
	}
}
