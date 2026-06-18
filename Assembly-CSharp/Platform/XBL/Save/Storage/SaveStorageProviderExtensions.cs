using System;
using Platform.XBL.Save.Storage.Blobs;
using Platform.XBL.Save.Storage.Files;

namespace Platform.XBL.Save.Storage
{
	public static class SaveStorageProviderExtensions
	{
		public static ISaveStorageProvider Create(this SaveStorageProvider storageProvider)
		{
			ISaveStorageProvider saveStorageProvider;
			if (storageProvider != SaveStorageProvider.Blobs)
			{
				if (storageProvider != SaveStorageProvider.Files)
				{
					throw new ArgumentOutOfRangeException("storageProvider", storageProvider, string.Format("Unknown {0}: {1}", "SaveStorageProvider", storageProvider));
				}
				saveStorageProvider = new SaveStorageStorageProviderFiles();
			}
			else
			{
				saveStorageProvider = new SaveStorageStorageProviderBlobs();
			}
			return saveStorageProvider;
		}
	}
}
