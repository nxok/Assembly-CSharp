using System;

namespace Webserver.UrlHandlers
{
	public class RewriteHandler : AbsHandler
	{
		public RewriteHandler(string _target, bool _fixedTarget = false)
			: base(null, 0)
		{
			this.target = _target;
			this.fixedTarget = _fixedTarget;
		}

		public override void HandleRequest(RequestContext _context)
		{
			_context.RequestPath = (this.fixedTarget ? this.target : (this.target + _context.RequestPath.Remove(0, this.urlBasePath.Length)));
			this.parent.ApplyPathHandler(_context);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly string target;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly bool fixedTarget;
	}
}
