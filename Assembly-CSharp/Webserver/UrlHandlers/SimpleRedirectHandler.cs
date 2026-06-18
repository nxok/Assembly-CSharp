using System;

namespace Webserver.UrlHandlers
{
	public class SimpleRedirectHandler : AbsHandler
	{
		public SimpleRedirectHandler(string _target)
			: base(null, 0)
		{
			this.target = _target;
		}

		public override void HandleRequest(RequestContext _context)
		{
			_context.Response.Redirect(this.target);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly string target;
	}
}
