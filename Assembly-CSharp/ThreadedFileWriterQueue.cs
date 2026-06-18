using System;
using System.Collections.Generic;

public class ThreadedFileWriterQueue
{
	public ThreadedFileWriterQueue(string taskName, string filepath)
	{
		this.taskName = taskName;
		this.filepath = filepath;
	}

	public void Write(PooledExpandableMemoryStream stream, bool waitForComplete = false)
	{
		object obj = this.threadLock;
		ThreadManager.ThreadInfo threadInfo;
		lock (obj)
		{
			this.memoryStreams.Enqueue(stream);
			if (this.writerThread == null)
			{
				this.writerThread = ThreadManager.StartThread(this.taskName, new ThreadManager.ThreadFunctionDelegate(this.WriteSaveFileThread), null, null, false, true);
			}
			threadInfo = this.writerThread;
		}
		if (waitForComplete)
		{
			threadInfo.WaitForEnd(30);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void WriteSaveFileThread(ThreadManager.ThreadInfo threadInfo)
	{
		for (;;)
		{
			object obj = this.threadLock;
			PooledExpandableMemoryStream pooledExpandableMemoryStream;
			lock (obj)
			{
				if (!this.memoryStreams.TryDequeue(out pooledExpandableMemoryStream))
				{
					this.writerThread = null;
					break;
				}
			}
			try
			{
				StreamUtils.WriteStreamToFile(pooledExpandableMemoryStream, this.filepath);
			}
			catch (Exception ex)
			{
				Log.Error("[" + this.taskName + "] Error while writing to file " + ex.Message);
				Log.Exception(ex);
			}
			MemoryPools.poolMemoryStream.FreeSync(pooledExpandableMemoryStream);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly string taskName;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly string filepath;

	[PublicizedFrom(EAccessModifier.Private)]
	public object threadLock = new object();

	[PublicizedFrom(EAccessModifier.Private)]
	public ThreadManager.ThreadInfo writerThread;

	[PublicizedFrom(EAccessModifier.Private)]
	public Queue<PooledExpandableMemoryStream> memoryStreams = new Queue<PooledExpandableMemoryStream>();
}
