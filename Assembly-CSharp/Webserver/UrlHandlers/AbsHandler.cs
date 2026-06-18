using System;
using Webserver.Permissions;

namespace Webserver.UrlHandlers
{
	public abstract class AbsHandler
	{
		public string UrlBasePath
		{
			get
			{
				return this.urlBasePath;
			}
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public AbsHandler(string _moduleName, int _defaultPermissionLevel = 0)
		{
			this.ModuleName = _moduleName;
			AdminWebModules.Instance.AddKnownModule(new AdminWebModules.WebModule(_moduleName, _defaultPermissionLevel, true));
		}

		public abstract void HandleRequest(RequestContext _context);

		public virtual bool IsAuthorizedForHandler(RequestContext _context)
		{
			return this.ModuleName == null || AdminWebModules.Instance.ModuleAllowedWithLevel(this.ModuleName, _context.PermissionLevel);
		}

		public virtual void Shutdown()
		{
		}

		public virtual void SetBasePathAndParent(Web _parent, string _relativePath)
		{
			this.parent = _parent;
			this.urlBasePath = _relativePath;
			this.parent.OpenApiHelpers.LoadOpenApiSpec(this);
		}

		public readonly string ModuleName;

		[PublicizedFrom(EAccessModifier.Protected)]
		public string urlBasePath;

		[PublicizedFrom(EAccessModifier.Protected)]
		public Web parent;
	}
}
