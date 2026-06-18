using System;
using System.Collections.Generic;
using System.Threading;
using Unity.XGamingRuntime;
using Unity.XGamingRuntime.Interop;

namespace Platform.XBL
{
	public class MultiplayerActivityQueryManager
	{
		public MultiplayerActivityQueryManager(XblContextHandle xblContextHandle)
		{
			this.xblContextHandle = xblContextHandle;
		}

		public void GetActivityAsync(ulong[] xuids, MultiplayerActivityQueryManager.OnGetActivityComplete callback)
		{
			int num = (xuids.Length + 30 - 1) / 30;
			MultiplayerActivityQueryManager.Request request = new MultiplayerActivityQueryManager.Request(xuids, num, callback);
			object obj = this.pendingLock;
			lock (obj)
			{
				if (!this.pendingRequests.TryAdd(request.id, request))
				{
					Log.Error("[XBL] could not start GetActivityAsync as request could not be enqueued");
					return;
				}
			}
			if (num == 1)
			{
				this.StartBatch(request, xuids);
				return;
			}
			for (int i = 0; i < num; i++)
			{
				int num2 = i * 30;
				int num3 = Math.Min(xuids.Length - num2, 30);
				ulong[] array = new ulong[num3];
				Array.Copy(xuids, num2, array, 0, num3);
				this.StartBatch(request, array);
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void StartBatch(MultiplayerActivityQueryManager.Request request, ulong[] xuids)
		{
			SDK.XBL.XblMultiplayerActivityGetActivityAsync(this.xblContextHandle, xuids, delegate(int hresult, XblMultiplayerActivityInfo[] results)
			{
				if (HR.SUCCEEDED(hresult))
				{
					this.CompleteBatch(request, results);
					return;
				}
				if (hresult == -2145844819)
				{
					object obj = this.retryLock;
					lock (obj)
					{
						this.retryQueue.Enqueue(new MultiplayerActivityQueryManager.RequestBatch(xuids, request));
						if (this.retryTask == null)
						{
							this.retryTask = ThreadManager.AddSingleTask(new ThreadManager.TaskFunctionDelegate(this.RetryBatchesTask), null, new ThreadManager.ExitCallbackTask(this.RetryExitHandler), true);
						}
					}
					return;
				}
				XblHelpers.LogHR(hresult, "XblMultiplayerActivityGetActivityAsync", false);
				this.CompleteBatch(request, null);
			});
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void CompleteBatch(MultiplayerActivityQueryManager.Request request, XblMultiplayerActivityInfo[] batchResults)
		{
			if (batchResults != null && batchResults.Length != 0)
			{
				List<XblMultiplayerActivityInfo> results = request.results;
				lock (results)
				{
					request.results.AddRange(batchResults);
				}
			}
			if (Interlocked.Decrement(ref request.batchesPending) == 0)
			{
				request.callback(request.xuids, request.results);
				object obj = this.pendingLock;
				lock (obj)
				{
					MultiplayerActivityQueryManager.Request request2;
					this.pendingRequests.Remove(request.id, out request2);
				}
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void RetryBatchesTask(ThreadManager.TaskInfo taskInfo)
		{
			for (;;)
			{
				Log.Warning(string.Format("[XBL] too many multiplayer activity requests, will try again in {0}s", 20));
				Thread.Sleep(20000);
				object obj = this.retryLock;
				lock (obj)
				{
					int num = 0;
					MultiplayerActivityQueryManager.RequestBatch requestBatch;
					while (this.retryQueue.TryDequeue(out requestBatch) && num < 20)
					{
						this.StartBatch(requestBatch.request, requestBatch.xuids);
						num++;
					}
					if (this.retryQueue.Count != 0)
					{
						continue;
					}
				}
				break;
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void RetryExitHandler(ThreadManager.TaskInfo _ti, Exception _e)
		{
			object obj = this.retryLock;
			lock (obj)
			{
				this.retryTask = null;
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public const int batchMax = 30;

		[PublicizedFrom(EAccessModifier.Private)]
		public const int backoffSeconds = 20;

		[PublicizedFrom(EAccessModifier.Private)]
		public const int burstLimit = 20;

		[PublicizedFrom(EAccessModifier.Private)]
		public static int nextRequestId;

		[PublicizedFrom(EAccessModifier.Private)]
		public XblContextHandle xblContextHandle;

		[PublicizedFrom(EAccessModifier.Private)]
		public object pendingLock = new object();

		[PublicizedFrom(EAccessModifier.Private)]
		public Dictionary<int, MultiplayerActivityQueryManager.Request> pendingRequests = new Dictionary<int, MultiplayerActivityQueryManager.Request>();

		[PublicizedFrom(EAccessModifier.Private)]
		public object retryLock = new object();

		[PublicizedFrom(EAccessModifier.Private)]
		public Queue<MultiplayerActivityQueryManager.RequestBatch> retryQueue = new Queue<MultiplayerActivityQueryManager.RequestBatch>();

		[PublicizedFrom(EAccessModifier.Private)]
		public ThreadManager.TaskInfo retryTask;

		[PublicizedFrom(EAccessModifier.Private)]
		public class Request
		{
			public Request(ulong[] xuids, int batchCount, MultiplayerActivityQueryManager.OnGetActivityComplete callback)
			{
				this.id = Interlocked.Increment(ref MultiplayerActivityQueryManager.nextRequestId);
				this.xuids = xuids;
				this.batchesPending = batchCount;
				this.callback = callback;
			}

			public int id;

			public ulong[] xuids;

			public MultiplayerActivityQueryManager.OnGetActivityComplete callback;

			public List<XblMultiplayerActivityInfo> results = new List<XblMultiplayerActivityInfo>();

			public int batchesPending;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public struct RequestBatch
		{
			public RequestBatch(ulong[] xuidBatch, MultiplayerActivityQueryManager.Request request)
			{
				this.xuids = xuidBatch;
				this.request = request;
			}

			public ulong[] xuids;

			public MultiplayerActivityQueryManager.Request request;
		}

		public delegate void OnGetActivityComplete(ulong[] requestedXuids, List<XblMultiplayerActivityInfo> results);
	}
}
