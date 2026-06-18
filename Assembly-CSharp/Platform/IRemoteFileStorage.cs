using System;

namespace Platform
{
	public interface IRemoteFileStorage
	{
		void Init(IPlatform _owner);

		bool IsReady { get; }

		bool Unavailable { get; }

		void GetFile(string _filename, IRemoteFileStorage.FileDownloadCompleteCallback _callback);

		void GetCachedFile(string _filename, IRemoteFileStorage.FileDownloadCompleteCallback _callback);

		public enum EFileDownloadResult
		{
			Ok,
			EmptyFilename,
			FileNotFound,
			Other
		}

		public delegate void FileDownloadCompleteCallback(IRemoteFileStorage.EFileDownloadResult _result, string _errorName, byte[] _data);
	}
}
