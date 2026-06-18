using System;
using System.Collections.Generic;
using System.Net;
using UnityEngine.Scripting;
using Utf8Json;
using Webserver.Permissions;

namespace Webserver.WebAPI.APIs.Permissions
{
	[Preserve]
	public class WebApiTokens : AbsRestApi
	{
		public static AdminApiTokens ApiTokensInstance
		{
			[PublicizedFrom(EAccessModifier.Private)]
			get
			{
				return AdminApiTokens.Instance;
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
				foreach (KeyValuePair<string, AdminApiTokens.ApiToken> keyValuePair in WebApiTokens.ApiTokensInstance.GetTokens())
				{
					string text;
					AdminApiTokens.ApiToken apiToken;
					keyValuePair.Deconstruct(out text, out apiToken);
					AdminApiTokens.ApiToken apiToken2 = apiToken;
					if (!flag)
					{
						jsonWriter.WriteValueSeparator();
					}
					flag = false;
					this.writeTokenJson(ref jsonWriter, apiToken2);
				}
				jsonWriter.WriteEndArray();
				AbsRestApi.SendEnvelopedResult(_context, ref jsonWriter, HttpStatusCode.OK, null, null, null);
				return;
			}
			jsonWriter.WriteRaw(WebUtils.JsonEmptyData);
			AbsRestApi.SendEnvelopedResult(_context, ref jsonWriter, HttpStatusCode.BadRequest, null, null, null);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void writeTokenJson(ref JsonWriter _writer, AdminApiTokens.ApiToken _token)
		{
			_writer.WriteRaw(WebApiTokens.jsonKeyName);
			_writer.WriteString(_token.Name);
			_writer.WriteRaw(WebApiTokens.jsonKeySecret);
			_writer.WriteString(_token.Secret);
			_writer.WriteRaw(WebApiTokens.jsonKeyPermissionLevel);
			_writer.WriteInt32(_token.PermissionLevel);
			_writer.WriteEndObject();
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override void HandleRestPost(RequestContext _context, IDictionary<string, object> _jsonInput, byte[] _jsonInputData)
		{
			string requestPath = _context.RequestPath;
			if (string.IsNullOrEmpty(requestPath))
			{
				AbsRestApi.SendEmptyResponse(_context, HttpStatusCode.BadRequest, _jsonInputData, EApiErrorCode.NO_NAME, null);
				return;
			}
			string text;
			if (!JsonCommons.TryGetJsonField(_jsonInput, "secret", out text))
			{
				AbsRestApi.SendEmptyResponse(_context, HttpStatusCode.BadRequest, _jsonInputData, EApiErrorCode.NO_OR_INVALID_SECRET, null);
				return;
			}
			int num;
			if (!JsonCommons.TryGetJsonField(_jsonInput, "permissionLevel", out num))
			{
				AbsRestApi.SendEmptyResponse(_context, HttpStatusCode.BadRequest, _jsonInputData, EApiErrorCode.NO_OR_INVALID_PERMISSION_LEVEL, null);
				return;
			}
			WebApiTokens.ApiTokensInstance.AddToken(requestPath, text, num);
			AbsRestApi.SendEmptyResponse(_context, HttpStatusCode.Created, null, null, null);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override void HandleRestDelete(RequestContext _context)
		{
			string requestPath = _context.RequestPath;
			AbsRestApi.SendEmptyResponse(_context, WebApiTokens.ApiTokensInstance.RemoveToken(requestPath) ? HttpStatusCode.NoContent : HttpStatusCode.NotFound, null, null, null);
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

		public WebApiTokens()
			: base(null)
		{
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public const string propertyName = "name";

		[PublicizedFrom(EAccessModifier.Private)]
		public const string propertySecret = "secret";

		[PublicizedFrom(EAccessModifier.Private)]
		public const string propertyPermissionLevel = "permissionLevel";

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly byte[] jsonKeyName = JsonWriter.GetEncodedPropertyNameWithBeginObject("name");

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly byte[] jsonKeySecret = JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("secret");

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly byte[] jsonKeyPermissionLevel = JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("permissionLevel");
	}
}
