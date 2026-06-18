using System;
using System.Net;

namespace Webserver.WebAPI.APIs.Permissions
{
	public static class PermissionsApiHelpers
	{
		public static bool TryParseId(RequestContext _context, byte[] _jsonInputData, out PlatformUserIdentifierAbs _userId, out string _groupId)
		{
			string requestPath = _context.RequestPath;
			_userId = null;
			_groupId = null;
			if (string.IsNullOrEmpty(requestPath))
			{
				WebUtils.SendEmptyResponse(_context, HttpStatusCode.BadRequest, _jsonInputData, "NO_USER_OR_GROUP", null);
				return false;
			}
			if (requestPath.StartsWith("user/", StringComparison.Ordinal))
			{
				bool flag = PlatformUserIdentifierAbs.TryFromCombinedString(requestPath.Substring("user/".Length), out _userId);
				if (!flag)
				{
					WebUtils.SendEmptyResponse(_context, HttpStatusCode.BadRequest, _jsonInputData, "INVALID_USER", null);
				}
				return flag;
			}
			if (requestPath.StartsWith("group/", StringComparison.Ordinal))
			{
				_groupId = requestPath.Substring("group/".Length);
				bool flag2 = _groupId.Length > 0;
				if (!flag2)
				{
					WebUtils.SendEmptyResponse(_context, HttpStatusCode.BadRequest, _jsonInputData, "INVALID_GROUP", null);
				}
				return flag2;
			}
			WebUtils.SendEmptyResponse(_context, HttpStatusCode.BadRequest, _jsonInputData, "INVALID_KIND", null);
			return false;
		}
	}
}
