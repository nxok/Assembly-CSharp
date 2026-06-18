using System;
using System.Collections.Generic;
using System.Net;
using UnityEngine.Scripting;
using Utf8Json;

namespace Webserver.WebAPI.APIs.Permissions
{
	[Preserve]
	public class Blacklist : AbsRestApi
	{
		public static AdminBlacklist BlacklistInstance
		{
			[PublicizedFrom(EAccessModifier.Private)]
			get
			{
				return GameManager.Instance.adminTools.Blacklist;
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
				foreach (AdminBlacklist.BannedUser bannedUser in Blacklist.BlacklistInstance.GetBanned())
				{
					if (!flag)
					{
						jsonWriter.WriteValueSeparator();
					}
					flag = false;
					this.writeBan(ref jsonWriter, bannedUser);
				}
				jsonWriter.WriteEndArray();
				AbsRestApi.SendEnvelopedResult(_context, ref jsonWriter, HttpStatusCode.OK, null, null, null);
				return;
			}
			jsonWriter.WriteRaw(WebUtils.JsonEmptyData);
			AbsRestApi.SendEnvelopedResult(_context, ref jsonWriter, HttpStatusCode.BadRequest, null, null, null);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void writeBan(ref JsonWriter _writer, AdminBlacklist.BannedUser _ban)
		{
			_writer.WriteRaw(Blacklist.jsonKeyName);
			_writer.WriteString(_ban.Name ?? "");
			_writer.WriteRaw(Blacklist.jsonKeyUserId);
			JsonCommons.WritePlatformUserIdentifier(ref _writer, _ban.UserIdentifier);
			_writer.WriteRaw(Blacklist.jsonKeyBannedUntil);
			JsonCommons.WriteDateTime(ref _writer, _ban.BannedUntil);
			_writer.WriteRaw(Blacklist.jsonKeyBanReason);
			_writer.WriteString(_ban.BanReason);
			_writer.WriteEndObject();
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override void HandleRestPost(RequestContext _context, IDictionary<string, object> _jsonInput, byte[] _jsonInputData)
		{
			PlatformUserIdentifierAbs platformUserIdentifierAbs;
			if (!this.TryParseId(_context, _jsonInputData, out platformUserIdentifierAbs))
			{
				return;
			}
			DateTime dateTime;
			if (!JsonCommons.TryReadDateTime(_jsonInput, "bannedUntil", out dateTime))
			{
				AbsRestApi.SendEmptyResponse(_context, HttpStatusCode.BadRequest, _jsonInputData, EApiErrorCode.NO_OR_INVALID_BANNED_UNTIL, null);
				return;
			}
			string text;
			JsonCommons.TryGetJsonField(_jsonInput, "banReason", out text);
			string text2;
			JsonCommons.TryGetJsonField(_jsonInput, "name", out text2);
			Blacklist.BlacklistInstance.AddBan(text2, platformUserIdentifierAbs, dateTime, text);
			AbsRestApi.SendEmptyResponse(_context, HttpStatusCode.Created, null, null, null);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override void HandleRestDelete(RequestContext _context)
		{
			PlatformUserIdentifierAbs platformUserIdentifierAbs;
			if (!this.TryParseId(_context, null, out platformUserIdentifierAbs))
			{
				return;
			}
			AbsRestApi.SendEmptyResponse(_context, Blacklist.BlacklistInstance.RemoveBan(platformUserIdentifierAbs) ? HttpStatusCode.NoContent : HttpStatusCode.NotFound, null, null, null);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public bool TryParseId(RequestContext _context, byte[] _jsonInputData, out PlatformUserIdentifierAbs _userId)
		{
			string requestPath = _context.RequestPath;
			_userId = null;
			if (string.IsNullOrEmpty(requestPath))
			{
				AbsRestApi.SendEmptyResponse(_context, HttpStatusCode.BadRequest, _jsonInputData, EApiErrorCode.NO_USER, null);
				return false;
			}
			if (!PlatformUserIdentifierAbs.TryFromCombinedString(requestPath, out _userId))
			{
				AbsRestApi.SendEmptyResponse(_context, HttpStatusCode.BadRequest, _jsonInputData, EApiErrorCode.INVALID_USER, null);
				return false;
			}
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

		public Blacklist()
			: base(null)
		{
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public const string propertyName = "name";

		[PublicizedFrom(EAccessModifier.Private)]
		public const string propertyUserId = "userId";

		[PublicizedFrom(EAccessModifier.Private)]
		public const string propertyBannedUntil = "bannedUntil";

		[PublicizedFrom(EAccessModifier.Private)]
		public const string propertyBanReason = "banReason";

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly byte[] jsonKeyName = JsonWriter.GetEncodedPropertyNameWithBeginObject("name");

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly byte[] jsonKeyUserId = JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("userId");

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly byte[] jsonKeyBannedUntil = JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("bannedUntil");

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly byte[] jsonKeyBanReason = JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("banReason");
	}
}
