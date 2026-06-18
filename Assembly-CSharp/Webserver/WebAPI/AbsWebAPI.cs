using System;
using Webserver.Permissions;

namespace Webserver.WebAPI
{
	public abstract class AbsWebAPI
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		public AbsWebAPI(string _name = null)
			: this(null, _name)
		{
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public AbsWebAPI(Web _parentWeb, string _name = null)
		{
			this.Name = _name ?? base.GetType().Name;
			this.ParentWeb = _parentWeb;
			this.CachedApiModuleName = "webapi." + this.Name;
			this.RegisterPermissions();
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public virtual void RegisterPermissions()
		{
			AdminWebModules.Instance.AddKnownModule(new AdminWebModules.WebModule(this.CachedApiModuleName, this.DefaultPermissionLevel(), true));
		}

		public abstract void HandleRequest(RequestContext _context);

		public virtual bool Authorized(RequestContext _context)
		{
			return AdminWebModules.Instance.GetModule(this.CachedApiModuleName).LevelGlobal >= _context.PermissionLevel;
		}

		public virtual int DefaultPermissionLevel()
		{
			return 0;
		}

		public readonly string Name;

		[PublicizedFrom(EAccessModifier.Protected)]
		public readonly Web ParentWeb;

		[PublicizedFrom(EAccessModifier.Protected)]
		public readonly string CachedApiModuleName;
	}
}
