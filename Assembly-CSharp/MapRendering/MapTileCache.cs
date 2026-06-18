using System;
using System.IO;
using UnityEngine;
using Webserver.FileCache;

namespace MapRendering
{
	public class MapTileCache : AbstractCache
	{
		public MapTileCache(int _tileSize)
		{
			Texture2D texture2D = new Texture2D(_tileSize, _tileSize);
			Color color;
			color..ctor(0f, 0f, 0f, 0f);
			for (int i = 0; i < _tileSize; i++)
			{
				for (int j = 0; j < _tileSize; j++)
				{
					texture2D.SetPixel(i, j, color);
				}
			}
			this.transparentTile = ImageConversion.EncodeToPNG(texture2D);
			Object.Destroy(texture2D);
		}

		public void SetZoomCount(int _count)
		{
			this.cache = new MapTileCache.CurrentZoomFile[_count];
			for (int i = 0; i < this.cache.Length; i++)
			{
				this.cache[i] = new MapTileCache.CurrentZoomFile();
			}
		}

		public byte[] LoadTile(int _zoomlevel, string _filename)
		{
			try
			{
				MapTileCache.CurrentZoomFile[] array = this.cache;
				lock (array)
				{
					MapTileCache.CurrentZoomFile currentZoomFile = this.cache[_zoomlevel];
					if (currentZoomFile.filename != null && currentZoomFile.filename.Equals(_filename))
					{
						return currentZoomFile.pngData;
					}
					currentZoomFile.filename = _filename;
					if (!File.Exists(_filename))
					{
						currentZoomFile.pngData = null;
						return null;
					}
					currentZoomFile.pngData = MapTileCache.ReadAllBytes(_filename);
					return currentZoomFile.pngData;
				}
			}
			catch (Exception ex)
			{
				Log.Warning(string.Format("Error in MapTileCache.LoadTile: {0}", ex));
			}
			return null;
		}

		public void SaveTile(int _zoomlevel, byte[] _contentPng)
		{
			try
			{
				MapTileCache.CurrentZoomFile[] array = this.cache;
				lock (array)
				{
					MapTileCache.CurrentZoomFile currentZoomFile = this.cache[_zoomlevel];
					string filename = currentZoomFile.filename;
					if (!string.IsNullOrEmpty(filename))
					{
						currentZoomFile.pngData = _contentPng;
						using (Stream stream = new FileStream(filename, FileMode.Create, FileAccess.ReadWrite, FileShare.None, 4096))
						{
							stream.Write(_contentPng, 0, _contentPng.Length);
						}
					}
				}
			}
			catch (Exception ex)
			{
				Log.Warning(string.Format("Error in MapTileCache.SaveTile: {0}", ex));
			}
		}

		public void ResetTile(int _zoomlevel)
		{
			try
			{
				MapTileCache.CurrentZoomFile[] array = this.cache;
				lock (array)
				{
					this.cache[_zoomlevel].filename = null;
					this.cache[_zoomlevel].pngData = null;
				}
			}
			catch (Exception ex)
			{
				Log.Warning(string.Format("Error in MapTileCache.ResetTile: {0}", ex));
			}
		}

		public override byte[] GetFileContent(string _filename)
		{
			try
			{
				MapTileCache.CurrentZoomFile[] array = this.cache;
				lock (array)
				{
					foreach (MapTileCache.CurrentZoomFile currentZoomFile in this.cache)
					{
						if (currentZoomFile.filename != null && currentZoomFile.filename.Equals(_filename))
						{
							return currentZoomFile.pngData;
						}
					}
					return (!File.Exists(_filename)) ? this.transparentTile : MapTileCache.ReadAllBytes(_filename);
				}
			}
			catch (Exception ex)
			{
				Log.Warning(string.Format("Error in MapTileCache.GetFileContent: {0}", ex));
			}
			return null;
		}

		public override ValueTuple<int, int> Invalidate()
		{
			return new ValueTuple<int, int>(0, 0);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static byte[] ReadAllBytes(string _path)
		{
			byte[] array2;
			using (FileStream fileStream = new FileStream(_path, FileMode.Open, FileAccess.Read, FileShare.Read, 4096))
			{
				int num = 0;
				int i = (int)fileStream.Length;
				byte[] array = new byte[i];
				while (i > 0)
				{
					int num2 = fileStream.Read(array, num, i);
					if (num2 == 0)
					{
						throw new IOException("Unexpected end of stream");
					}
					num += num2;
					i -= num2;
				}
				array2 = array;
			}
			return array2;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly byte[] transparentTile;

		[PublicizedFrom(EAccessModifier.Private)]
		public MapTileCache.CurrentZoomFile[] cache;

		[PublicizedFrom(EAccessModifier.Private)]
		public class CurrentZoomFile
		{
			public string filename;

			public byte[] pngData;
		}
	}
}
