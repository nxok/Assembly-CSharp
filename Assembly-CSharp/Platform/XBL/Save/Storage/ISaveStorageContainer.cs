using System;
using Unity.XGamingRuntime;

namespace Platform.XBL.Save.Storage
{
	public interface ISaveStorageContainer : IDisposable
	{
		bool IsDisposed { get; }

		string Name { get; }

		DateTime LastAccessed { get; }

		void Flush(bool waitForFlush);

		bool TryEnumerateBlobInfos(out XGameSaveBlobInfo[] blobInfos);

		XGameSaveBlobInfo GetBlobInfo(string blobName);

		RefCountedBuffer GetBlob(string blobName, StringSpan debugIdentifier)
		{
			return this.GetBlobs(new string[] { blobName }, debugIdentifier)[0];
		}

		RefCountedBuffer[] GetBlobs(string[] blobNames, StringSpan debugIdentifier);

		void SetBlob(string blobName, RefCountedBuffer blobData);

		void DeleteBlob(string blobName);

		public const string RootSaveContainerName = "root";
	}
}
