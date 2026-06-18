using System;
using System.IO;

namespace Webserver.FileCache
{
	public class DirectAccess : AbstractCache
	{
		public override byte[] GetFileContent(string _filename)
		{
			try
			{
				return File.Exists(_filename) ? File.ReadAllBytes(_filename) : null;
			}
			catch (Exception ex)
			{
				Log.Out(string.Format("Error in DirectAccess.GetFileContent: {0}", ex));
			}
			return null;
		}

		public override ValueTuple<int, int> Invalidate()
		{
			return new ValueTuple<int, int>(0, 0);
		}
	}
}
