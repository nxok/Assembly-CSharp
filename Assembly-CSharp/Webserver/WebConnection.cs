using System;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

namespace Webserver
{
	public class WebConnection : ConsoleConnectionAbstract
	{
		public string SessionID { get; }

		public IPAddress Endpoint { get; }

		public string Username { get; }

		public PlatformUserIdentifierAbs UserId { get; }

		public PlatformUserIdentifierAbs CrossplatformUserId { get; }

		public TimeSpan Age
		{
			get
			{
				return DateTime.Now - this.lastAction;
			}
		}

		public WebConnection(string _sessionId, IPAddress _endpoint, string _username, PlatformUserIdentifierAbs _userId, PlatformUserIdentifierAbs _crossUserId = null)
		{
			this.SessionID = _sessionId;
			this.Endpoint = _endpoint;
			this.Username = _username;
			this.UserId = _userId;
			this.CrossplatformUserId = _crossUserId;
			this.login = DateTime.Now;
			this.lastAction = this.login;
			this.conDescription = string.Format("WebPanel from {0}", this.Endpoint);
		}

		public void UpdateUsage()
		{
			this.lastAction = DateTime.Now;
		}

		public override string GetDescription()
		{
			return this.conDescription;
		}

		public override void SendLine(string _text)
		{
		}

		public override void SendLines(List<string> _output)
		{
		}

		public override void SendLog(string _formattedMsg, string _plainMsg, string _trace, LogType _type, DateTime _timestamp, long _uptime)
		{
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly DateTime login;

		[PublicizedFrom(EAccessModifier.Private)]
		public DateTime lastAction;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly string conDescription;
	}
}
