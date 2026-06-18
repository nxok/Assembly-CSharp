using System;
using System.IO;
using System.Reflection;

namespace Webserver
{
	public static class ResourceHelpers
	{
		public static Stream OpenManifestResource(Assembly _assembly, string _name, bool _ignoreCase = false)
		{
			foreach (string text in _assembly.GetManifestResourceNames())
			{
				if (text.EndsWith(_name, _ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal))
				{
					return _assembly.GetManifestResourceStream(text);
				}
			}
			return null;
		}

		public static string GetManifestResourceText(Assembly _assembly, string _name, bool _ignoreCase = false)
		{
			string text;
			using (Stream stream = ResourceHelpers.OpenManifestResource(_assembly, _name, _ignoreCase))
			{
				if (stream == null)
				{
					text = null;
				}
				else
				{
					using (TextReader textReader = new StreamReader(stream))
					{
						text = textReader.ReadToEnd();
					}
				}
			}
			return text;
		}
	}
}
