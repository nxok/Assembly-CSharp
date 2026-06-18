using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Unity.XGamingRuntime;

namespace Platform.XBL.Save.Storage.Files
{
	public sealed class SaveStorageStorageContainerFiles : ISaveStorageContainer, IDisposable
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public static void LogInfo(string text)
		{
			Log.Out("[XBL: SaveStorageStorageContainerFiles] " + text);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static void LogError(string text)
		{
			Log.Error("[XBL: SaveStorageStorageContainerFiles] " + text);
		}

		[Conditional("DEBUG_SAVE_DATA_MANAGER")]
		[PublicizedFrom(EAccessModifier.Private)]
		public static void LogTrace(string text)
		{
			Log.Out("[XBL: SaveStorageStorageContainerFiles] " + text);
		}

		public SaveStorageStorageContainerFiles(string gameSaveFolder, string containerName)
		{
			using (this.m_lastAccessHelper.CreateScope())
			{
				bool flag = false;
				try
				{
					this.m_containerName = containerName;
					this.m_containerPath = Path.GetFullPath(Path.Join(gameSaveFolder, this.m_containerName));
					Directory.CreateDirectory(this.m_containerPath);
					if (!Directory.Exists(this.m_containerPath))
					{
						SaveStorageStorageContainerFiles.LogError("Container '" + containerName + "' failed to create directory at: " + this.m_containerPath);
					}
					else
					{
						SaveStorageStorageContainerFiles.LogInfo("Container '" + containerName + "' exists at: " + this.m_containerPath);
						flag = true;
					}
				}
				finally
				{
					if (!flag)
					{
						this.Dispose();
					}
				}
			}
		}

		public void Dispose()
		{
			this.m_disposed = true;
		}

		public bool IsDisposed
		{
			get
			{
				return this.m_disposed;
			}
		}

		public string Name
		{
			get
			{
				return this.m_containerName;
			}
		}

		public DateTime LastAccessed
		{
			get
			{
				return this.m_lastAccessHelper.Time;
			}
		}

		public void Flush(bool waitForFlush)
		{
		}

		public bool TryEnumerateBlobInfos(out XGameSaveBlobInfo[] blobInfos)
		{
			bool flag;
			using (this.m_lastAccessHelper.CreateScope())
			{
				try
				{
					blobInfos = (from fi in new DirectoryInfo(this.m_containerPath).EnumerateFiles()
						select new XGameSaveBlobInfo
						{
							Name = fi.Name,
							Size = (uint)fi.Length
						}).ToArray<XGameSaveBlobInfo>();
					flag = true;
				}
				catch (IOException ex)
				{
					SaveStorageStorageContainerFiles.LogError(string.Format("{0} failed: {1}", "TryEnumerateBlobInfos", ex));
					blobInfos = null;
					flag = false;
				}
			}
			return flag;
		}

		public XGameSaveBlobInfo GetBlobInfo(string blobName)
		{
			XGameSaveBlobInfo xgameSaveBlobInfo;
			using (this.m_lastAccessHelper.CreateScope())
			{
				FileInfo fileInfo = new FileInfo(Path.Join(this.m_containerPath, blobName));
				if (!fileInfo.Exists)
				{
					xgameSaveBlobInfo = null;
				}
				else
				{
					xgameSaveBlobInfo = new XGameSaveBlobInfo
					{
						Name = blobName,
						Size = (uint)fileInfo.Length
					};
				}
			}
			return xgameSaveBlobInfo;
		}

		public RefCountedBuffer[] GetBlobs(string[] blobNames, StringSpan debugIdentifier)
		{
			RefCountedBuffer[] array2;
			using (this.m_lastAccessHelper.CreateScope())
			{
				bool flag = false;
				RefCountedBuffer[] array = new RefCountedBuffer[blobNames.Length];
				try
				{
					for (int i = 0; i < blobNames.Length; i++)
					{
						string text = blobNames[i];
						using (FileStream fileStream = File.Open(this.GetBlobPath(text), FileMode.Open, FileAccess.Read, FileShare.Read))
						{
							int num = (int)fileStream.Length;
							using (RefCountedBuffer refCountedBuffer = RefCountedBuffer.CreatePooled(num))
							{
								int num2;
								for (int j = 0; j < num; j += num2)
								{
									num2 = fileStream.Read(refCountedBuffer.Span.Slice(j, num - j));
									if (num2 <= 0)
									{
										throw new IOException("Unexpected end of file stream.");
									}
								}
								array[i] = refCountedBuffer.CreateRef();
							}
						}
					}
					flag = true;
					array2 = array;
				}
				finally
				{
					if (!flag)
					{
						foreach (RefCountedBuffer refCountedBuffer2 in array)
						{
							if (refCountedBuffer2 != null)
							{
								refCountedBuffer2.Dispose();
							}
						}
					}
				}
			}
			return array2;
		}

		public void SetBlob(string blobName, RefCountedBuffer blobData)
		{
			using (this.m_lastAccessHelper.CreateScope())
			{
				using (FileStream fileStream = File.Open(this.GetBlobPath(blobName), FileMode.Create, FileAccess.Write))
				{
					fileStream.Write(blobData.Span);
				}
			}
		}

		public void DeleteBlob(string blobName)
		{
			using (this.m_lastAccessHelper.CreateScope())
			{
				File.Delete(this.GetBlobPath(blobName));
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public string GetBlobPath(string blobName)
		{
			return Path.Join(this.m_containerPath, blobName);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly LastAccessHelper m_lastAccessHelper = new LastAccessHelper();

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly string m_containerName;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly string m_containerPath;

		[PublicizedFrom(EAccessModifier.Private)]
		public bool m_disposed;
	}
}
