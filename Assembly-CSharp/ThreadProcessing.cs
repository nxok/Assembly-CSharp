using System;
using System.Collections.Generic;

public class ThreadProcessing
{
	public ThreadProcessing(List<ThreadInfoParam> _JobList)
	{
		this.JobList = new List<ThreadInfoParam>(100);
		this.Init(_JobList);
	}

	public ThreadProcessing()
	{
		this.JobList = new List<ThreadInfoParam>(100);
		this.IsCancelled = false;
		this.IsFinished = false;
	}

	public void Init(List<ThreadInfoParam> _JobList)
	{
		this.RemoveTreatedElement(_JobList);
		if (this.JobList.Count == 0)
		{
			this.IsFinished = true;
			return;
		}
		this.IsCancelled = false;
		this.IsFinished = false;
		for (int i = 0; i < this.JobList.Count; i++)
		{
			for (int j = 0; j < this.JobList[i].LengthThreadContList; j++)
			{
				DistantChunk dchunk = this.JobList[i].ThreadContListA[j].DChunk;
				dchunk.CellMeshData = DistantChunk.SMPool.GetObject(dchunk.BaseChunkMap, dchunk.ResLevel);
			}
		}
		this.TaskInfo = ThreadManager.AddSingleTask(new ThreadManager.TaskFunctionDelegate(this.ThreadJob), this.JobList, null, true);
	}

	public void RemoveTreatedElement(List<ThreadInfoParam> _JobList)
	{
		this.JobList.Clear();
		for (int i = 0; i < _JobList.Count; i++)
		{
			if (!_JobList[i].IsThreadDone)
			{
				this.JobList.Add(_JobList[i]);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ThreadJob(ThreadManager.TaskInfo _InfoJob)
	{
		List<ThreadInfoParam> list = (List<ThreadInfoParam>)_InfoJob.parameter;
		object obj;
		for (int i = 0; i < list.Count; i++)
		{
			for (int j = 0; j < list[i].LengthThreadContList; j++)
			{
				obj = ThreadProcessing.LockObjectThread;
				lock (obj)
				{
					if (this.IsCancelled)
					{
						break;
					}
					list[i].ThreadContListA[j].ThreadExtraWork();
				}
			}
			obj = ThreadProcessing.LockObjectThread;
			lock (obj)
			{
				if (this.IsCancelled)
				{
					break;
				}
				list[i].IsThreadDone = true;
			}
		}
		obj = ThreadProcessing.LockObjectThread;
		lock (obj)
		{
			this.IsFinished = true;
		}
	}

	public void CancelThread()
	{
		object lockObjectThread = ThreadProcessing.LockObjectThread;
		lock (lockObjectThread)
		{
			this.IsCancelled = true;
		}
	}

	public long CancelThreadAndWaitFinished()
	{
		object lockObjectThread = ThreadProcessing.LockObjectThread;
		lock (lockObjectThread)
		{
			this.IsCancelled = true;
		}
		if (this.TaskInfo != null)
		{
			this.TaskInfo.WaitForEnd();
		}
		return 0L;
	}

	public bool IsThreadFinished()
	{
		return this.IsFinished;
	}

	public bool IsThreadDone(int ThreadInfoParamId)
	{
		object lockObjectThread = ThreadProcessing.LockObjectThread;
		bool isThreadDone;
		lock (lockObjectThread)
		{
			isThreadDone = this.JobList[ThreadInfoParamId].IsThreadDone;
		}
		return isThreadDone;
	}

	public bool IsFinished;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool IsCancelled;

	public ThreadManager.TaskInfo TaskInfo;

	[PublicizedFrom(EAccessModifier.Private)]
	public static object LockObjectThread = new object();

	public List<ThreadInfoParam> JobList;
}
