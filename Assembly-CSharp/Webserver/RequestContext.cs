using System;
using System.Collections.Specialized;
using SpaceWizards.HttpListener;

namespace Webserver
{
	public class RequestContext
	{
		public NameValueCollection QueryParameters
		{
			get
			{
				NameValueCollection nameValueCollection;
				if ((nameValueCollection = this.queryParameters) == null)
				{
					nameValueCollection = (this.queryParameters = this.Request.QueryString);
				}
				return nameValueCollection;
			}
		}

		public RequestContext(string _requestPath, HttpListenerRequest _request, HttpListenerResponse _response, WebConnection _connection, int _permissionLevel)
		{
			this.RequestPath = _requestPath;
			this.Request = _request;
			this.Response = _response;
			this.Connection = _connection;
			this.PermissionLevel = _permissionLevel;
			string httpMethod = _request.HttpMethod;
			ERequestMethod erequestMethod;
			if (!(httpMethod == "GET"))
			{
				if (!(httpMethod == "PUT"))
				{
					if (!(httpMethod == "POST"))
					{
						if (!(httpMethod == "DELETE"))
						{
							if (!(httpMethod == "HEAD"))
							{
								if (!(httpMethod == "OPTIONS"))
								{
									erequestMethod = ERequestMethod.Other;
								}
								else
								{
									erequestMethod = ERequestMethod.OPTIONS;
								}
							}
							else
							{
								erequestMethod = ERequestMethod.HEAD;
							}
						}
						else
						{
							erequestMethod = ERequestMethod.DELETE;
						}
					}
					else
					{
						erequestMethod = ERequestMethod.POST;
					}
				}
				else
				{
					erequestMethod = ERequestMethod.PUT;
				}
			}
			else
			{
				erequestMethod = ERequestMethod.GET;
			}
			this.Method = erequestMethod;
		}

		public string RequestPath;

		public readonly ERequestMethod Method;

		public readonly HttpListenerRequest Request;

		[PublicizedFrom(EAccessModifier.Private)]
		public NameValueCollection queryParameters;

		public readonly HttpListenerResponse Response;

		public readonly WebConnection Connection;

		public readonly int PermissionLevel;
	}
}
