using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Platform.Steam;
using Utf8Json;
using Webserver.Permissions;

namespace Webserver.UrlHandlers
{
	public class SessionHandler : AbsHandler
	{
		public SessionHandler()
			: base(null, 0)
		{
		}

		public override void HandleRequest(RequestContext _context)
		{
			if (_context.Request.RemoteEndPoint == null)
			{
				WebUtils.WriteText(_context.Response, "NoRemoteEndpoint", HttpStatusCode.BadRequest, null);
				return;
			}
			string text = _context.RequestPath.Remove(0, this.urlBasePath.Length);
			string text2 = _context.Request.RemoteEndPoint.ToString();
			if (text.StartsWith("verifysteamopenid"))
			{
				if (SessionHandler.HandleSteamVerification(this.parent.ConnectionHandler, _context, text2))
				{
					_context.Response.Redirect("/app");
					return;
				}
				_context.Response.Redirect("/app/error/SteamLoginFailed");
				return;
			}
			else
			{
				if (text.StartsWith("logout"))
				{
					SessionHandler.HandleLogout(this.parent.ConnectionHandler, _context, "/app");
					return;
				}
				if (text.StartsWith("loginsteam"))
				{
					SessionHandler.HandleSteamLogin(_context, this.urlBasePath + "verifysteamopenid");
					return;
				}
				if (text.StartsWith("login"))
				{
					SessionHandler.HandleUserPassLogin(this.parent.ConnectionHandler, _context, text2);
					return;
				}
				WebUtils.WriteText(_context.Response, "InvalidSessionsCommand", HttpStatusCode.BadRequest, null);
				return;
			}
		}

		public static bool HandleUserPassLogin(ConnectionHandler _connectionHandler, RequestContext _context, string _remoteEndpointString)
		{
			if (!_context.Request.HasEntityBody)
			{
				WebUtils.WriteText(_context.Response, "NoLoginData", HttpStatusCode.BadRequest, null);
				return false;
			}
			Stream inputStream = _context.Request.InputStream;
			byte[] array = new byte[_context.Request.ContentLength64];
			inputStream.Read(array, 0, (int)_context.Request.ContentLength64);
			IDictionary<string, object> dictionary;
			try
			{
				dictionary = JsonSerializer.Deserialize<IDictionary<string, object>>(array);
			}
			catch (Exception ex)
			{
				Log.Error("Error deserializing JSON from user/password login:");
				Log.Exception(ex);
				WebUtils.WriteText(_context.Response, "InvalidLoginJson", HttpStatusCode.BadRequest, null);
				return false;
			}
			object obj;
			if (dictionary.TryGetValue("username", out obj))
			{
				string text = obj as string;
				if (text != null)
				{
					if (dictionary.TryGetValue("password", out obj))
					{
						string text2 = obj as string;
						if (text2 != null)
						{
							AdminWebUsers.WebUser webUser;
							if (!AdminWebUsers.Instance.TryGetUser(text, text2, out webUser))
							{
								WebUtils.WriteText(_context.Response, "UserPassInvalid", HttpStatusCode.Unauthorized, null);
								Log.Out("[Web] User/pass login failed from " + _remoteEndpointString);
								return false;
							}
							bool flag = SessionHandler.HandleUserIdLogin(_connectionHandler, _context, _remoteEndpointString, "User/pass", webUser.Name, webUser.PlatformUser, webUser.CrossPlatformUser);
							if (flag)
							{
								WebUtils.WriteText(_context.Response, "", HttpStatusCode.OK, null);
								return flag;
							}
							WebUtils.WriteText(_context.Response, "LoginError", HttpStatusCode.InternalServerError, null);
							return flag;
						}
					}
					WebUtils.WriteText(_context.Response, "InvalidLoginJson", HttpStatusCode.BadRequest, null);
					return false;
				}
			}
			WebUtils.WriteText(_context.Response, "InvalidLoginJson", HttpStatusCode.BadRequest, null);
			return false;
		}

		public static void HandleSteamLogin(RequestContext _context, string _verificationCallbackUrl)
		{
			string text = (WebUtils.IsSslRedirected(_context.Request) ? "https://" : "http://") + _context.Request.UserHostName;
			string openIdLoginUrl = OpenID.GetOpenIdLoginUrl(text, text + _verificationCallbackUrl);
			_context.Response.Redirect(openIdLoginUrl);
		}

		public static bool HandleLogout(ConnectionHandler _connectionHandler, RequestContext _context, string _pageBase)
		{
			Cookie cookie = new Cookie("sid", "", "/")
			{
				Expired = true
			};
			_context.Response.AppendCookie(cookie);
			if (_context.Connection == null)
			{
				_context.Response.Redirect(_pageBase);
				return false;
			}
			_connectionHandler.LogOut(_context.Connection.SessionID);
			_context.Response.Redirect(_pageBase);
			return true;
		}

		public static bool HandleSteamVerification(ConnectionHandler _connectionHandler, RequestContext _context, string _remoteEndpointString)
		{
			ulong num;
			try
			{
				num = OpenID.Validate(_context.Request);
			}
			catch (Exception ex)
			{
				Log.Error("[Web] Error validating Steam login from " + _remoteEndpointString + ":");
				Log.Exception(ex);
				return false;
			}
			if (num <= 0UL)
			{
				Log.Out("[Web] Steam OpenID login failed (invalid ID) from " + _remoteEndpointString);
				return false;
			}
			UserIdentifierSteam userIdentifierSteam = new UserIdentifierSteam(num);
			return SessionHandler.HandleUserIdLogin(_connectionHandler, _context, _remoteEndpointString, "Steam OpenID", userIdentifierSteam.ToString(), userIdentifierSteam, null);
		}

		public static bool HandleUserIdLogin(ConnectionHandler _connectionHandler, RequestContext _context, string _remoteEndpointString, string _loginName, string _username, PlatformUserIdentifierAbs _userId, PlatformUserIdentifierAbs _crossUserId = null)
		{
			try
			{
				WebConnection webConnection = _connectionHandler.LogIn(_context.Request.RemoteEndPoint.Address, _username, _userId, _crossUserId);
				int userPermissionLevel = GameManager.Instance.adminTools.Users.GetUserPermissionLevel(_userId);
				int num = int.MaxValue;
				if (_crossUserId != null)
				{
					num = GameManager.Instance.adminTools.Users.GetUserPermissionLevel(_crossUserId);
				}
				int num2 = Math.Min(userPermissionLevel, num);
				Log.Out(string.Format("[Web] {0} login from {1}, name {2} with ID {3}, CID {4}, permission level {5}", new object[]
				{
					_loginName,
					_remoteEndpointString,
					_username,
					_userId,
					(_crossUserId != null) ? _crossUserId.ToString() : "none",
					num2
				}));
				Cookie cookie = new Cookie("sid", webConnection.SessionID, "/")
				{
					Expired = false,
					Expires = DateTime.MinValue,
					HttpOnly = true,
					Secure = false
				};
				_context.Response.AppendCookie(cookie);
				return true;
			}
			catch (Exception ex)
			{
				Log.Error("[Web] Error during " + _loginName + " login:");
				Log.Exception(ex);
			}
			return false;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public const string pageBasePath = "/app";

		[PublicizedFrom(EAccessModifier.Private)]
		public const string pageErrorPath = "/app/error/";

		[PublicizedFrom(EAccessModifier.Private)]
		public const string steamOpenIdVerifyUrl = "verifysteamopenid";

		[PublicizedFrom(EAccessModifier.Private)]
		public const string steamLoginUrl = "loginsteam";

		[PublicizedFrom(EAccessModifier.Private)]
		public const string steamLoginName = "Steam OpenID";

		[PublicizedFrom(EAccessModifier.Private)]
		public const string steamLoginFailedPage = "SteamLoginFailed";

		[PublicizedFrom(EAccessModifier.Private)]
		public const string userPassLoginUrl = "login";

		public const string userPassLoginName = "User/pass";
	}
}
