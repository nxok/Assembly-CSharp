using System;
using System.Collections.Generic;
using System.Net;
using UnityEngine.Scripting;
using Utf8Json;
using Webserver.Permissions;

namespace Webserver.WebAPI.APIs.Permissions
{
	[Preserve]
	public class WebModules : AbsRestApi
	{
		[PublicizedFrom(EAccessModifier.Private)]
		static WebModules()
		{
			for (int i = 0; i < WebModules.jsonMethodNameKeys.Length; i++)
			{
				ERequestMethod erequestMethod = (ERequestMethod)i;
				WebModules.jsonMethodNameKeys[i] = JsonWriter.GetEncodedPropertyName(erequestMethod.ToStringCached<ERequestMethod>());
			}
		}

		public static AdminWebModules ModulesInstance
		{
			[PublicizedFrom(EAccessModifier.Private)]
			get
			{
				return AdminWebModules.Instance;
			}
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override void HandleRestGet(RequestContext _context)
		{
			string requestPath = _context.RequestPath;
			JsonWriter jsonWriter;
			AbsRestApi.PrepareEnvelopedResult(out jsonWriter);
			if (string.IsNullOrEmpty(requestPath))
			{
				jsonWriter.WriteBeginArray();
				bool flag = true;
				foreach (AdminWebModules.WebModule webModule in WebModules.ModulesInstance.GetModules())
				{
					if (!flag)
					{
						jsonWriter.WriteValueSeparator();
					}
					flag = false;
					this.writeModuleJson(ref jsonWriter, webModule);
				}
				jsonWriter.WriteEndArray();
				AbsRestApi.SendEnvelopedResult(_context, ref jsonWriter, HttpStatusCode.OK, null, null, null);
				return;
			}
			jsonWriter.WriteRaw(WebUtils.JsonEmptyData);
			AbsRestApi.SendEnvelopedResult(_context, ref jsonWriter, HttpStatusCode.BadRequest, null, null, null);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void writeModuleJson(ref JsonWriter _writer, AdminWebModules.WebModule _module)
		{
			_writer.WriteRaw(WebModules.jsonKeyModule);
			_writer.WriteString(_module.Name);
			_writer.WriteRaw(WebModules.jsonKeyPermissionLevelGlobal);
			_writer.WriteInt32(_module.LevelGlobal);
			_writer.WriteRaw(WebModules.jsonKeyPermissionLevelPerMethod);
			_writer.WriteBeginObject();
			if (_module.LevelPerMethod != null)
			{
				bool flag = true;
				for (int i = 0; i < _module.LevelPerMethod.Length; i++)
				{
					int num = _module.LevelPerMethod[i];
					if (num != -2147483647)
					{
						if (!flag)
						{
							_writer.WriteValueSeparator();
						}
						flag = false;
						_writer.WriteRaw(WebModules.jsonMethodNameKeys[i]);
						if (num == -2147483648)
						{
							_writer.WriteString("inherit");
						}
						else
						{
							_writer.WriteInt32(num);
						}
					}
				}
			}
			_writer.WriteEndObject();
			_writer.WriteRaw(WebModules.jsonKeyIsDefault);
			_writer.WriteBoolean(_module.IsDefault);
			_writer.WriteEndObject();
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override void HandleRestPost(RequestContext _context, IDictionary<string, object> _jsonInput, byte[] _jsonInputData)
		{
			string requestPath = _context.RequestPath;
			if (string.IsNullOrEmpty(requestPath))
			{
				AbsRestApi.SendEmptyResponse(_context, HttpStatusCode.BadRequest, _jsonInputData, EApiErrorCode.NO_MODULE, null);
				return;
			}
			if (!AdminWebModules.Instance.IsKnownModule(requestPath))
			{
				AbsRestApi.SendEmptyResponse(_context, HttpStatusCode.BadRequest, _jsonInputData, EApiErrorCode.INVALID_MODULE, null);
				return;
			}
			AdminWebModules.WebModule webModule = AdminWebModules.Instance.GetModule(requestPath);
			if (_jsonInput.ContainsKey("permissionLevelGlobal"))
			{
				int num;
				if (!JsonCommons.TryGetJsonField(_jsonInput, "permissionLevelGlobal", out num))
				{
					AbsRestApi.SendEmptyResponse(_context, HttpStatusCode.BadRequest, _jsonInputData, EApiErrorCode.INVALID_PERMISSION_LEVEL_GLOBAL, null);
					return;
				}
				webModule = webModule.SetLevelGlobal(num);
			}
			object obj;
			if (_jsonInput.TryGetValue("permissionLevelPerMethod", out obj))
			{
				IDictionary<string, object> dictionary = obj as IDictionary<string, object>;
				if (dictionary == null)
				{
					AbsRestApi.SendEmptyResponse(_context, HttpStatusCode.BadRequest, _jsonInputData, EApiErrorCode.INVALID_PERMISSION_LEVEL_PER_METHOD_PROPERTY, null);
					return;
				}
				foreach (KeyValuePair<string, object> keyValuePair in dictionary)
				{
					string text;
					object obj2;
					keyValuePair.Deconstruct(out text, out obj2);
					string text2 = text;
					object obj3 = obj2;
					ERequestMethod erequestMethod;
					if (!EnumUtils.TryParse<ERequestMethod>(text2, out erequestMethod, true))
					{
						AbsRestApi.SendEmptyResponse(_context, HttpStatusCode.BadRequest, _jsonInputData, EApiErrorCode.INVALID_METHOD_NAME, null);
						return;
					}
					if (webModule.LevelPerMethod == null || webModule.LevelPerMethod[(int)erequestMethod] == -2147483647)
					{
						AbsRestApi.SendEmptyResponse(_context, HttpStatusCode.BadRequest, _jsonInputData, EApiErrorCode.UNSUPPORTED_METHOD, null);
						return;
					}
					string text3 = obj3 as string;
					int num3;
					if (text3 == null)
					{
						if (obj3 is double)
						{
							double num2 = (double)obj3;
							try
							{
								num3 = (int)num2;
								goto IL_0191;
							}
							catch (Exception)
							{
								AbsRestApi.SendEmptyResponse(_context, HttpStatusCode.BadRequest, _jsonInputData, EApiErrorCode.INVALID_PERMISSION_VALUE, null);
								return;
							}
						}
						AbsRestApi.SendEmptyResponse(_context, HttpStatusCode.BadRequest, _jsonInputData, EApiErrorCode.INVALID_PERMISSION_VALUE_TYPE, null);
						return;
					}
					if (!text3.EqualsCaseInsensitive("inherit"))
					{
						AbsRestApi.SendEmptyResponse(_context, HttpStatusCode.BadRequest, _jsonInputData, EApiErrorCode.INVALID_PERMISSION_STRING, null);
						return;
					}
					num3 = int.MinValue;
					IL_0191:
					webModule = webModule.SetLevelForMethod(erequestMethod, num3);
				}
			}
			WebModules.ModulesInstance.AddModule(webModule);
			AbsRestApi.SendEmptyResponse(_context, HttpStatusCode.Created, null, null, null);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override void HandleRestDelete(RequestContext _context)
		{
			string requestPath = _context.RequestPath;
			AbsRestApi.SendEmptyResponse(_context, WebModules.ModulesInstance.RemoveModule(requestPath) ? HttpStatusCode.NoContent : HttpStatusCode.NotFound, null, null, null);
		}

		public override bool AllowPostWithId
		{
			[PublicizedFrom(EAccessModifier.Protected)]
			get
			{
				return true;
			}
		}

		public override int[] DefaultMethodPermissionLevels()
		{
			return new int[] { -2147483647, int.MinValue, int.MinValue, -2147483647, int.MinValue };
		}

		public WebModules()
			: base(null)
		{
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public const string propertyModule = "module";

		[PublicizedFrom(EAccessModifier.Private)]
		public const string propertyPermissionLevelGlobal = "permissionLevelGlobal";

		[PublicizedFrom(EAccessModifier.Private)]
		public const string propertyPermissionLevelPerMethod = "permissionLevelPerMethod";

		[PublicizedFrom(EAccessModifier.Private)]
		public const string propertyIsDefault = "isDefault";

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly byte[] jsonKeyModule = JsonWriter.GetEncodedPropertyNameWithBeginObject("module");

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly byte[] jsonKeyPermissionLevelGlobal = JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("permissionLevelGlobal");

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly byte[] jsonKeyPermissionLevelPerMethod = JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("permissionLevelPerMethod");

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly byte[] jsonKeyIsDefault = JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("isDefault");

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly byte[][] jsonMethodNameKeys = new byte[7][];
	}
}
