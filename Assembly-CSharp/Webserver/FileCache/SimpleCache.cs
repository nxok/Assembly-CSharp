using System;
using System.Collections.Generic;
using System.IO;

namespace Webserver.FileCache
{
	public class SimpleCache : AbstractCache
	{
		public override byte[] GetFileContent(string _filename)
		{
			try
			{
				Dictionary<string, byte[]> dictionary = this.fileCache;
				lock (dictionary)
				{
					byte[] array;
					if (this.fileCache.TryGetValue(_filename, out array))
					{
						return array;
					}
					if (!File.Exists(_filename))
					{
						return null;
					}
					byte[] array2 = File.ReadAllBytes(_filename);
					this.fileCache.Add(_filename, array2);
					return array2;
				}
			}
			catch (Exception ex)
			{
				Log.Out(string.Format("Error in SimpleCache.GetFileContent: {0}", ex));
			}
			return null;
		}

		public override ValueTuple<int, int> Invalidate()
		{
			ValueTuple<int, int> valueTuple = new ValueTuple<int, int>(0, 0);
			Dictionary<string, byte[]> dictionary = this.fileCache;
			lock (dictionary)
			{
				valueTuple.Item1 = this.fileCache.Count;
				foreach (KeyValuePair<string, byte[]> keyValuePair in this.fileCache)
				{
					string text;
					byte[] array;
					keyValuePair.Deconstruct(out text, out array);
					byte[] array2 = array;
					valueTuple.Item2 += array2.Length;
				}
				this.fileCache.Clear();
			}
			return valueTuple;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly Dictionary<string, byte[]> fileCache = new Dictionary<string, byte[]>();
	}
}
