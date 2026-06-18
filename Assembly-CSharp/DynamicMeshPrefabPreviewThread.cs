using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

public class DynamicMeshPrefabPreviewThread
{
	public bool HasStopBeenRequested
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return this.CancelToken.IsCancellationRequested;
		}
	}

	public void StartThread()
	{
		this.StopThread();
		DynamicMeshPrefabPreviewThread.Instance = this;
		this.BuilderManager = DynamicMeshBuilderManager.GetOrCreate();
		this.TokenSource = new CancellationTokenSource();
		this.Wait = new AutoResetEvent(false);
		this.CancelToken = this.TokenSource.Token;
		this.WaitHandles[0] = this.CancelToken.WaitHandle;
		this.WaitHandles[1] = this.Wait;
		this.PreviewCheckThread = new Thread(new ThreadStart(this.ThreadLoop));
		this.PreviewCheckThread.Start();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ThreadLoop()
	{
		while (!this.HasStopBeenRequested)
		{
			this.BuilderManager.CheckPreviews();
			bool flag = this.ProcessList();
			if (!this.ChunksToProcess.IsEmpty)
			{
				if (!flag)
				{
					Thread.Sleep(100);
				}
			}
			else
			{
				WaitHandle.WaitAny(this.WaitHandles);
			}
		}
	}

	public void StopThread()
	{
		if (this.PreviewCheckThread == null)
		{
			return;
		}
		this.TokenSource.Cancel();
		DateTime dateTime = DateTime.Now.AddSeconds(3.0);
		while (this.PreviewCheckThread.IsAlive || dateTime > DateTime.Now)
		{
			Thread.Sleep(10);
		}
		Thread previewCheckThread = this.PreviewCheckThread;
		if (previewCheckThread != null && previewCheckThread.IsAlive)
		{
			try
			{
				Thread previewCheckThread2 = this.PreviewCheckThread;
				if (previewCheckThread2 != null)
				{
					previewCheckThread2.Abort();
				}
			}
			catch
			{
			}
		}
	}

	public void AddChunk(DynamicMeshItem item)
	{
		this.LockGenerationUntil = DateTime.Now.AddMilliseconds(300.0);
		this.ChunksToProcess.TryAdd(item.Key, item);
		this.Wait.Set();
	}

	public void ClearChunks()
	{
		this.ChunksToProcess.Clear();
	}

	public void CleanUp()
	{
		this.BuilderManager.StopThreads(true);
		this.ChunksToProcess.Clear();
	}

	public bool ProcessList()
	{
		if (this.LockGenerationUntil > DateTime.Now)
		{
			return false;
		}
		KeyValuePair<long, DynamicMeshItem> keyValuePair = this.ChunksToProcess.FirstOrDefault<KeyValuePair<long, DynamicMeshItem>>();
		if (keyValuePair.Value == null)
		{
			return false;
		}
		DynamicMeshItem value = keyValuePair.Value;
		DynamicMeshChunkProcessor nextBuilder = this.BuilderManager.GetNextBuilder();
		if (((nextBuilder != null) ? nextBuilder.AddItemForMeshPreview(value, this.PreviewData) : 0) != 1)
		{
			return false;
		}
		DynamicMeshItem dynamicMeshItem;
		this.ChunksToProcess.TryRemove(value.Key, out dynamicMeshItem);
		return true;
	}

	public ConcurrentDictionary<long, DynamicMeshItem> ChunksToProcess = new ConcurrentDictionary<long, DynamicMeshItem>();

	[PublicizedFrom(EAccessModifier.Private)]
	public DynamicMeshBuilderManager BuilderManager;

	public static DynamicMeshPrefabPreviewThread Instance;

	public ChunkPreviewData PreviewData;

	[PublicizedFrom(EAccessModifier.Private)]
	public AutoResetEvent Wait;

	[PublicizedFrom(EAccessModifier.Private)]
	public CancellationTokenSource TokenSource;

	[PublicizedFrom(EAccessModifier.Private)]
	public CancellationToken CancelToken;

	[PublicizedFrom(EAccessModifier.Private)]
	public WaitHandle[] WaitHandles = new WaitHandle[2];

	[PublicizedFrom(EAccessModifier.Private)]
	public Thread PreviewCheckThread;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool chunkAdded;

	[PublicizedFrom(EAccessModifier.Private)]
	public DateTime LockGenerationUntil;
}
