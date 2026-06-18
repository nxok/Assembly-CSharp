using System;
using System.IO;

namespace Platform.Shared
{
	public class SaveGameProvider : SaveGameIOProviderFixedRoot, IPlatformSaveGameProvider, IPlatformSaveGameIOProvider
	{
		public SaveGameProvider()
			: base(null)
		{
		}

		public SaveGameProvider(string rootPath, long maxStorageSizeBytes = 0L)
			: base(rootPath)
		{
			this.m_maxStorageSizeBytes = maxStorageSizeBytes;
			this.UpdateSizes();
		}

		public ESaveGameProviderStatus Status
		{
			get
			{
				return ESaveGameProviderStatus.Ok;
			}
		}

		public IPlatformSaveGameIOProvider Cache
		{
			get
			{
				return null;
			}
		}

		public event Action Initialized
		{
			add
			{
				value();
			}
			remove
			{
			}
		}

		public void Init(IPlatform _owner)
		{
		}

		public void Destroy()
		{
		}

		public bool ShouldBackup()
		{
			return true;
		}

		public bool ShouldCommit()
		{
			return true;
		}

		public double GetCommitProgress()
		{
			return 1.0;
		}

		public void Flush(bool waitForFlush)
		{
		}

		public bool ShouldLimitSize()
		{
			return !string.IsNullOrEmpty(this.m_rootPath) && this.m_maxStorageSizeBytes > 0L;
		}

		public void UpdateSizes()
		{
			if (this.ShouldLimitSize())
			{
				long num = 0L;
				foreach (string text in Directory.EnumerateFiles(this.m_rootPath, "*", SearchOption.AllDirectories))
				{
					FileInfo fileInfo = new FileInfo(text);
					num += fileInfo.Length;
				}
				this.m_currentSize = new SaveDataSizes(this.m_maxStorageSizeBytes, this.m_maxStorageSizeBytes - num);
			}
		}

		public SaveDataSizes GetSizes()
		{
			return this.m_currentSize;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public long m_maxStorageSizeBytes;

		[PublicizedFrom(EAccessModifier.Private)]
		public SaveDataSizes m_currentSize;
	}
}
