using System;
using System.Collections.Generic;
using System.Net;
using UnityEngine.Scripting;
using Utf8Json;
using Webserver.Permissions;

namespace Webserver.WebAPI.APIs.Permissions
{
	[Preserve]
	public class WebUsers : AbsRestApi
	{
		public static AdminWebUsers WebUsersInstance
		{
			[PublicizedFrom(EAccessModifier.Private)]
			get
			{
				return AdminWebUsers.Instance;
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
				foreach (KeyValuePair<string, AdminWebUsers.WebUser> keyValuePair in WebUsers.WebUsersInstance.GetUsers())
				{
					string text;
					AdminWebUsers.WebUser webUser;
					keyValuePair.Deconstruct(out text, out webUser);
					AdminWebUsers.WebUser webUser2 = webUser;
					if (!flag)
					{
						jsonWriter.WriteValueSeparator();
					}
					flag = false;
					this.writeUserJson(ref jsonWriter, webUser2);
				}
				jsonWriter.WriteEndArray();
				AbsRestApi.SendEnvelopedResult(_context, ref jsonWriter, HttpStatusCode.OK, null, null, null);
				return;
			}
			jsonWriter.WriteRaw(WebUtils.JsonEmptyData);
			AbsRestApi.SendEnvelopedResult(_context, ref jsonWriter, HttpStatusCode.BadRequest, null, null, null);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void writeUserJson(ref JsonWriter _writer, AdminWebUsers.WebUser _user)
		{
			_writer.WriteRaw(WebUsers.jsonKeyName);
			_writer.WriteString(_user.Name ?? "");
			_writer.WriteRaw(WebUsers.jsonKeyPlatformUserId);
			JsonCommons.WritePlatformUserIdentifier(ref _writer, _user.PlatformUser);
			_writer.WriteRaw(WebUsers.jsonKeyCrossplatformUserId);
			JsonCommons.WritePlatformUserIdentifier(ref _writer, _user.CrossPlatformUser);
			_writer.WriteEndObject();
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override void HandleRestPost(RequestContext _context, IDictionary<string, object> _jsonInput, byte[] _jsonInputData)
		{
			string text;
			if (!this.TryParseName(_context, _jsonInputData, out text))
			{
				return;
			}
			string text2;
			if (!JsonCommons.TryGetJsonField(_jsonInput, "password", out text2))
			{
				AbsRestApi.SendEmptyResponse(_context, HttpStatusCode.BadRequest, _jsonInputData, EApiErrorCode.NO_OR_INVALID_PASSWORD, null);
				return;
			}
			IDictionary<string, object> dictionary;
			if (!JsonCommons.TryGetJsonField(_jsonInput, "platformUserId", out dictionary))
			{
				AbsRestApi.SendEmptyResponse(_context, HttpStatusCode.BadRequest, _jsonInputData, EApiErrorCode.NO_PLATFORM_USER_ID, null);
				return;
			}
			PlatformUserIdentifierAbs platformUserIdentifierAbs;
			if (!JsonCommons.TryReadPlatformUserIdentifier(dictionary, out platformUserIdentifierAbs))
			{
				AbsRestApi.SendEmptyResponse(_context, HttpStatusCode.BadRequest, _jsonInputData, EApiErrorCode.INVALID_PLATFORM_USER_ID, null);
				return;
			}
			PlatformUserIdentifierAbs platformUserIdentifierAbs2 = null;
			if (JsonCommons.TryGetJsonField(_jsonInput, "crossplatformUserId", out dictionary) && !JsonCommons.TryReadPlatformUserIdentifier(dictionary, out platformUserIdentifierAbs2))
			{
				AbsRestApi.SendEmptyResponse(_context, HttpStatusCode.BadRequest, _jsonInputData, EApiErrorCode.INVALID_CROSSPLATFORM_USER_ID, null);
				return;
			}
			WebUsers.WebUsersInstance.AddUser(text, text2, platformUserIdentifierAbs, platformUserIdentifierAbs2);
			AbsRestApi.SendEmptyResponse(_context, HttpStatusCode.Created, null, null, null);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override void HandleRestDelete(RequestContext _context)
		{
			string text;
			if (!this.TryParseName(_context, null, out text))
			{
				return;
			}
			AbsRestApi.SendEmptyResponse(_context, WebUsers.WebUsersInstance.RemoveUser(text) ? HttpStatusCode.NoContent : HttpStatusCode.NotFound, null, null, null);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public bool TryParseName(RequestContext _context, byte[] _jsonInputData, out string _userName)
		{
			string requestPath = _context.RequestPath;
			_userName = null;
			if (string.IsNullOrEmpty(requestPath))
			{
				AbsRestApi.SendEmptyResponse(_context, HttpStatusCode.BadRequest, _jsonInputData, EApiErrorCode.NO_USERNAME, null);
				return false;
			}
			_userName = requestPath;
			return true;
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

		public WebUsers()
			: base(null)
		{
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public const string propertyName = "name";

		[PublicizedFrom(EAccessModifier.Private)]
		public const string propertyPassword = "password";

		[PublicizedFrom(EAccessModifier.Private)]
		public const string propertyPlatformUserId = "platformUserId";

		[PublicizedFrom(EAccessModifier.Private)]
		public const string propertyCrossplatformUserId = "crossplatformUserId";

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly byte[] jsonKeyName = JsonWriter.GetEncodedPropertyNameWithBeginObject("name");

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly byte[] jsonKeyPlatformUserId = JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("platformUserId");

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly byte[] jsonKeyCrossplatformUserId = JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("crossplatformUserId");
	}
}
