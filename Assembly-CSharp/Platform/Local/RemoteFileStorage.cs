using System;
using System.IO;

namespace Platform.Local
{
	public class RemoteFileStorage : IRemoteFileStorage
	{
		public void Init(IPlatform _owner)
		{
		}

		public bool IsReady
		{
			get
			{
				return true;
			}
		}

		public bool Unavailable
		{
			get
			{
				return false;
			}
		}

		public void GetFile(string _filename, IRemoteFileStorage.FileDownloadCompleteCallback _callback)
		{
			if (_callback == null)
			{
				return;
			}
			if (string.IsNullOrEmpty(_filename))
			{
				_callback(IRemoteFileStorage.EFileDownloadResult.EmptyFilename, null, null);
				return;
			}
			string text = GameIO.GetApplicationPath() + "/fakeRemoteFileStorage/" + _filename;
			if (!File.Exists(text))
			{
				_callback(IRemoteFileStorage.EFileDownloadResult.FileNotFound, "", null);
				return;
			}
			try
			{
				byte[] array = File.ReadAllBytes(text);
				_callback(IRemoteFileStorage.EFileDownloadResult.Ok, null, array);
			}
			catch (Exception ex)
			{
				Log.Error("[Local] ReadFile (" + _filename + ") failed:");
				Log.Exception(ex);
				_callback(IRemoteFileStorage.EFileDownloadResult.Other, ex.Message, null);
			}
		}

		public void GetCachedFile(string _filename, IRemoteFileStorage.FileDownloadCompleteCallback _callback)
		{
			throw new NotImplementedException();
		}
	}
}
