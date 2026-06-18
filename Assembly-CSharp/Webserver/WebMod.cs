using System;
using System.IO;
using Webserver.FileCache;
using Webserver.UrlHandlers;

namespace Webserver
{
	public class WebMod
	{
		public WebMod(Web _parentWeb, Mod _parentMod, bool _useStaticCache)
		{
			this.ParentMod = _parentMod;
			string text = _parentMod.Path + "/WebMod";
			this.IsWebMod = Directory.Exists(text);
			if (!this.IsWebMod)
			{
				return;
			}
			this.ModUrl = "/webmods/" + _parentMod.Name + "/";
			this.ReactBundle = text + "/bundle.js";
			this.ReactBundle = (File.Exists(this.ReactBundle) ? (this.ModUrl + "bundle.js") : null);
			this.CssPath = text + "/styling.css";
			this.CssPath = (File.Exists(this.CssPath) ? (this.ModUrl + "styling.css") : null);
			_parentWeb.RegisterPathHandler(this.ModUrl, new StaticHandler(text, _useStaticCache ? new SimpleCache() : new DirectAccess(), false, null));
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public const string modsBaseUrl = "/webmods/";

		[PublicizedFrom(EAccessModifier.Private)]
		public const string reactBundleName = "bundle.js";

		[PublicizedFrom(EAccessModifier.Private)]
		public const string stylingFileName = "styling.css";

		public readonly Mod ParentMod;

		public readonly string ModUrl;

		public readonly string ReactBundle;

		public readonly string CssPath;

		public readonly bool IsWebMod;
	}
}
