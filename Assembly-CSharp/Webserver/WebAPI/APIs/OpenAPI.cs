using System;
using System.Net;
using UnityEngine.Scripting;

namespace Webserver.WebAPI.APIs
{
	[Preserve]
	public class OpenAPI : AbsRestApi
	{
		public OpenAPI(Web _parentWeb)
			: base(_parentWeb, null)
		{
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override void HandleRestGet(RequestContext _context)
		{
			string requestPath = _context.RequestPath;
			string text;
			if (!this.ParentWeb.OpenApiHelpers.TryGetOpenApiSpec(requestPath, out text))
			{
				WebUtils.WriteText(_context.Response, "Spec for " + requestPath + " not found", HttpStatusCode.NotFound, null);
				return;
			}
			WebUtils.WriteText(_context.Response, text, HttpStatusCode.OK, "text/x-yaml");
		}

		public override int DefaultPermissionLevel()
		{
			return 2000;
		}
	}
}
