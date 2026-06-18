using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

public abstract class SaveDataManagerBase : ISaveDataManager
{
	public virtual bool AppliesSaveSizeLimit
	{
		get
		{
			return false;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public abstract Stream GetStream(SaveDataManagedPath path, FileMode mode, FileAccess access, FileShare share);

	[PublicizedFrom(EAccessModifier.Protected)]
	public abstract void ReturnStream(Stream stream);

	[PublicizedFrom(EAccessModifier.Protected)]
	public void OnCommitStarted()
	{
		object commitProgressLock = this.m_commitProgressLock;
		lock (commitProgressLock)
		{
			this.m_commitInProgress = true;
			this.m_commitInProgressChanged = true;
			this.QueueOnCommitTask();
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void OnCommitFinished()
	{
		object commitProgressLock = this.m_commitProgressLock;
		lock (commitProgressLock)
		{
			this.m_commitInProgress = false;
			this.m_commitInProgressChanged = true;
			this.QueueOnCommitTask();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void QueueOnCommitTask()
	{
		object commitProgressLock = this.m_commitProgressLock;
		lock (commitProgressLock)
		{
			if (!this.m_commitInProgressHasTask)
			{
				this.m_commitInProgressHasTask = true;
				ThreadManager.AddSingleTaskMainThread("SaveDataManager.OnCommitTask", new ThreadManager.MainThreadTaskFunctionDelegate(this.OnCommitTask), null);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnCommitTask(object _)
	{
		object commitProgressLock = this.m_commitProgressLock;
		bool commitInProgress;
		bool commitInProgressChanged;
		bool commitInProgressLast;
		lock (commitProgressLock)
		{
			commitInProgress = this.m_commitInProgress;
			commitInProgressChanged = this.m_commitInProgressChanged;
			commitInProgressLast = this.m_commitInProgressLast;
			this.m_commitInProgressChanged = false;
			this.m_commitInProgressLast = commitInProgress;
			this.m_commitInProgressHasTask = false;
		}
		if (commitInProgressLast != commitInProgress)
		{
			if (commitInProgress)
			{
				this.<OnCommitTask>g__FireCommitStarted|12_0();
				return;
			}
			this.<OnCommitTask>g__FireCommitFinished|12_1();
			return;
		}
		else
		{
			if (!commitInProgressChanged)
			{
				return;
			}
			if (!commitInProgress)
			{
				this.<OnCommitTask>g__FireCommitStarted|12_0();
				this.<OnCommitTask>g__FireCommitFinished|12_1();
				return;
			}
			this.<OnCommitTask>g__FireCommitFinished|12_1();
			this.<OnCommitTask>g__FireCommitStarted|12_0();
			return;
		}
	}

	[Conditional("DEBUG_SAVE_DATA_MANAGER")]
	[PublicizedFrom(EAccessModifier.Private)]
	public static void DebugLog(string text)
	{
		Log.Out("[SaveDataManagerBase] " + text);
	}

	public virtual void Init()
	{
	}

	public virtual SaveDataWriteMode GetWriteMode()
	{
		return SaveDataWriteMode.None;
	}

	public virtual void SetWriteMode(SaveDataWriteMode writeMode)
	{
	}

	public virtual void Cleanup()
	{
	}

	public virtual void RegisterRegionFileManager(RegionFileManager regionFileManager)
	{
	}

	public virtual void DeregisterRegionFileManager(RegionFileManager regionFileManager)
	{
	}

	public virtual int CommitAsync()
	{
		return 0;
	}

	public virtual bool IsCommitPending(int token)
	{
		return false;
	}

	public virtual void CommitSync()
	{
	}

	public virtual IEnumerator CommitCoroutine(Action<double> progressFeedback = null)
	{
		yield break;
	}

	public event Action CommitStarted;

	public event Action CommitFinished;

	public virtual bool ShouldLimitSize()
	{
		return false;
	}

	public virtual void UpdateSizes()
	{
	}

	public virtual SaveDataSizes GetSizes()
	{
		return default(SaveDataSizes);
	}

	public Stream ManagedFileOpen(SaveDataManagedPath path, FileMode mode, FileAccess access, FileShare share)
	{
		return new SaveDataManagerBase.WrappedStream(this, path, mode, access, share);
	}

	public virtual void ManagedFileDelete(SaveDataManagedPath path)
	{
		SaveDataManager_Placeholder.Instance.ManagedFileDelete(path);
	}

	public virtual bool ManagedFileExists(SaveDataManagedPath path)
	{
		return SaveDataManager_Placeholder.Instance.ManagedFileExists(path);
	}

	public virtual DateTime ManagedFileGetLastWriteTimeUtc(SaveDataManagedPath path)
	{
		return SaveDataManager_Placeholder.Instance.ManagedFileGetLastWriteTimeUtc(path);
	}

	public virtual SdDirectoryInfo ManagedDirectoryCreateDirectory(SaveDataManagedPath path)
	{
		return SaveDataManager_Placeholder.Instance.ManagedDirectoryCreateDirectory(path);
	}

	public virtual DateTime ManagedDirectoryGetLastWriteTimeUtc(SaveDataManagedPath path)
	{
		return SaveDataManager_Placeholder.Instance.ManagedDirectoryGetLastWriteTimeUtc(path);
	}

	public virtual bool ManagedDirectoryExists(SaveDataManagedPath path)
	{
		return SaveDataManager_Placeholder.Instance.ManagedDirectoryExists(path);
	}

	public virtual IEnumerable<SaveDataManagedPath> ManagedDirectoryEnumerateDirectories(SaveDataManagedPath path, string searchPattern, SearchOption searchOption)
	{
		return SaveDataManager_Placeholder.Instance.ManagedDirectoryEnumerateDirectories(path, searchPattern, searchOption);
	}

	public virtual IEnumerable<SaveDataManagedPath> ManagedDirectoryEnumerateFiles(SaveDataManagedPath path, string searchPattern, SearchOption searchOption)
	{
		return SaveDataManager_Placeholder.Instance.ManagedDirectoryEnumerateFiles(path, searchPattern, searchOption);
	}

	public virtual IEnumerable<SaveDataManagedPath> ManagedDirectoryEnumerateFileSystemEntries(SaveDataManagedPath path, string searchPattern, SearchOption searchOption)
	{
		return SaveDataManager_Placeholder.Instance.ManagedDirectoryEnumerateFileSystemEntries(path, searchPattern, searchOption);
	}

	public virtual void ManagedDirectoryDelete(SaveDataManagedPath path, bool recursive)
	{
		SaveDataManager_Placeholder.Instance.ManagedDirectoryDelete(path, recursive);
	}

	public virtual long ManagedFileInfoLength(SaveDataManagedPath path)
	{
		return SaveDataManager_Placeholder.Instance.ManagedFileInfoLength(path);
	}

	public virtual IEnumerable<SdDirectoryInfo> ManagedDirectoryInfoEnumerateDirectories(SaveDataManagedPath path, string searchPattern, SearchOption searchOption)
	{
		return SaveDataManager_Placeholder.Instance.ManagedDirectoryInfoEnumerateDirectories(path, searchPattern, searchOption);
	}

	public virtual IEnumerable<SdFileInfo> ManagedDirectoryInfoEnumerateFiles(SaveDataManagedPath path, string searchPattern, SearchOption searchOption)
	{
		return SaveDataManager_Placeholder.Instance.ManagedDirectoryInfoEnumerateFiles(path, searchPattern, searchOption);
	}

	public virtual IEnumerable<SdFileSystemInfo> ManagedDirectoryInfoEnumerateFileSystemInfos(SaveDataManagedPath path, string searchPattern, SearchOption searchOption)
	{
		return SaveDataManager_Placeholder.Instance.ManagedDirectoryInfoEnumerateFileSystemInfos(path, searchPattern, searchOption);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public SaveDataManagerBase()
	{
	}

	[CompilerGenerated]
	[PublicizedFrom(EAccessModifier.Private)]
	public void <OnCommitTask>g__FireCommitStarted|12_0()
	{
		Action commitStarted = this.CommitStarted;
		if (commitStarted == null)
		{
			return;
		}
		commitStarted();
	}

	[CompilerGenerated]
	[PublicizedFrom(EAccessModifier.Private)]
	public void <OnCommitTask>g__FireCommitFinished|12_1()
	{
		Action commitFinished = this.CommitFinished;
		if (commitFinished == null)
		{
			return;
		}
		commitFinished();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly object m_commitProgressLock = new object();

	[PublicizedFrom(EAccessModifier.Private)]
	public bool m_commitInProgress;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool m_commitInProgressChanged;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool m_commitInProgressLast;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool m_commitInProgressHasTask;

	[PublicizedFrom(EAccessModifier.Private)]
	public class WrappedStream : Stream, IDisposable
	{
		public WrappedStream(SaveDataManagerBase sdm, SaveDataManagedPath path, FileMode mode, FileAccess access, FileShare share)
		{
			this.sdm = sdm;
			this.stream = sdm.GetStream(path, mode, access, share);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override void Dispose(bool disposing)
		{
			if (!this.disposedValue)
			{
				if (disposing)
				{
					this.sdm.ReturnStream(this.stream);
				}
				this.disposedValue = true;
			}
			base.Dispose(disposing);
		}

		public override bool CanRead
		{
			get
			{
				return this.stream.CanRead;
			}
		}

		public override bool CanSeek
		{
			get
			{
				return this.stream.CanSeek;
			}
		}

		public override bool CanWrite
		{
			get
			{
				return this.stream.CanWrite;
			}
		}

		public override long Length
		{
			get
			{
				return this.stream.Length;
			}
		}

		public override long Position
		{
			get
			{
				return this.stream.Position;
			}
			set
			{
				this.stream.Position = value;
			}
		}

		public override void Flush()
		{
			this.stream.Flush();
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			return this.stream.Read(buffer, offset, count);
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			return this.stream.Seek(offset, origin);
		}

		public override void SetLength(long value)
		{
			this.stream.SetLength(value);
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			this.stream.Write(buffer, offset, count);
		}

		public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
		{
			return this.stream.BeginRead(buffer, offset, count, callback, state);
		}

		public override int EndRead(IAsyncResult asyncResult)
		{
			return this.stream.EndRead(asyncResult);
		}

		public override int ReadByte()
		{
			return this.stream.ReadByte();
		}

		public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
		{
			return this.stream.BeginWrite(buffer, offset, count, callback, state);
		}

		public override void EndWrite(IAsyncResult asyncResult)
		{
			this.stream.EndWrite(asyncResult);
		}

		public override void WriteByte(byte value)
		{
			this.stream.WriteByte(value);
		}

		public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
		{
			return this.stream.ReadAsync(buffer, offset, count, cancellationToken);
		}

		public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
		{
			return this.stream.WriteAsync(buffer, offset, count, cancellationToken);
		}

		public override Task FlushAsync(CancellationToken cancellationToken)
		{
			return this.stream.FlushAsync(cancellationToken);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public bool disposedValue;

		[PublicizedFrom(EAccessModifier.Private)]
		public Stream stream;

		[PublicizedFrom(EAccessModifier.Private)]
		public SaveDataManagerBase sdm;
	}
}
