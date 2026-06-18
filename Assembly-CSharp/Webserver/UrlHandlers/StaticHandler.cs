using System;
using System.IO;
using Webserver.FileCache;

namespace Webserver.UrlHandlers
{
	public class StaticHandler : AbsHandler
	{
		public StaticHandler(string _filePath, AbstractCache _cache, bool _logMissingFiles, string _moduleName = null)
			: base(_moduleName, 0)
		{
			this.datapath = _filePath + ((_filePath[_filePath.Length - 1] == '/') ? "" : "/");
			this.cache = _cache;
			this.logMissingFiles = _logMissingFiles;
		}

		public override void HandleRequest(RequestContext _context)
		{
			string text = _context.RequestPath.Remove(0, this.urlBasePath.Length);
			byte[] fileContent = this.cache.GetFileContent(this.datapath + text);
			if (fileContent != null)
			{
				_context.Response.ContentType = MimeType.GetMimeType(Path.GetExtension(text));
				_context.Response.ContentLength64 = (long)fileContent.Length;
				_context.Response.OutputStream.Write(fileContent, 0, fileContent.Length);
				return;
			}
			_context.Response.StatusCode = 404;
			if (this.logMissingFiles)
			{
				Log.Warning(string.Concat(new string[] { "[Web] Static: FileNotFound: \"", _context.RequestPath, "\" @ \"", this.datapath, text, "\"" }));
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly AbstractCache cache;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly string datapath;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly bool logMissingFiles;
	}
}
