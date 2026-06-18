using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

public static class LoadManager
{
	public static void Init()
	{
		LoadManager.forceLoadSync = false;
		LoadManager.rootGroup = new LoadManager.LoadGroup(null);
		LoadManager.loadRequests = new WorkBatch<LoadManager.LoadTask>();
		LoadManager.deferedLoadRequests = new List<LoadManager.LoadTask>();
		LoadManager.updateRequestAction = new Action<LoadManager.LoadTask>(LoadManager.UpdateRequest);
		Addressables.InitializeAsync().WaitForCompletion();
		LoadManager.addressablesCaseMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
		foreach (IResourceLocator resourceLocator in Addressables.ResourceLocators)
		{
			foreach (object obj in resourceLocator.Keys)
			{
				string text = obj as string;
				if (text != null && !LoadManager.addressablesCaseMap.TryAdd(text, text))
				{
					Log.Error("Error adding " + text + " to Addressables dictionary, case-insensitive duplicate found.");
				}
			}
		}
	}

	public static void InitSync()
	{
		LoadManager.Init();
		LoadManager.forceLoadSync = true;
	}

	public static void Update()
	{
		List<LoadManager.LoadTask> list = LoadManager.deferedLoadRequests;
		lock (list)
		{
			int num = LoadManager.loadRequests.Count();
			if (LoadManager.deferedLoadRequests.Count > 0 && num < 64)
			{
				int num2 = Mathf.Min(64 - num, LoadManager.deferedLoadRequests.Count);
				for (int i = 0; i < num2; i++)
				{
					LoadManager.AddTask(LoadManager.deferedLoadRequests[i].Group, LoadManager.deferedLoadRequests[i]);
				}
				LoadManager.deferedLoadRequests.RemoveRange(0, num2);
			}
		}
		LoadManager.loadRequests.DoWork(LoadManager.updateRequestAction);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void UpdateRequest(LoadManager.LoadTask _request)
	{
		if (_request.INTERNAL_IsPending)
		{
			_request.Update();
			LoadManager.loadRequests.Add(_request);
			return;
		}
		_request.Complete();
		_request.Group.DecrementPending();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void RemoveRequest(LoadManager.LoadTask _request)
	{
		if (LoadManager.loadRequests.Remove(_request))
		{
			_request.Group.DecrementPending();
		}
	}

	public static void Destroy()
	{
		LoadManager.loadRequests.Clear();
	}

	public static LoadManager.LoadGroup CreateGroup()
	{
		return new LoadManager.LoadGroup(LoadManager.rootGroup);
	}

	public static LoadManager.LoadGroup CreateGroup(LoadManager.LoadGroup _parent)
	{
		return new LoadManager.LoadGroup(_parent);
	}

	public static LoadManager.CoroutineTask AddTask(IEnumerator _task, LoadManager.CompletionCallback _callback = null, LoadManager.LoadGroup _lg = null)
	{
		if (_lg == null)
		{
			_lg = LoadManager.rootGroup;
		}
		_lg.IncrementPending();
		LoadManager.CoroutineTask coroutineTask = new LoadManager.CoroutineTask(_lg, _task, _callback);
		LoadManager.loadRequests.Add(coroutineTask);
		return coroutineTask;
	}

	public static LoadManager.AssetRequestTask<T> LoadAsset<T>(DataLoader.DataPathIdentifier _identifier, Action<T> _callback = null, LoadManager.LoadGroup _lg = null, bool _deferLoading = false, bool _loadSync = false, bool _ignoreDlcEntitlements = false) where T : Object
	{
		if (ThreadManager.IsInSyncCoroutine)
		{
			_loadSync = true;
		}
		if (_identifier.IsBundle)
		{
			AssetBundleManager.Instance.LoadAssetBundle(_identifier.BundlePath, _identifier.FromMod);
			return LoadManager.LoadAssetFromBundle<T>(_identifier, _callback, _lg, _deferLoading, _loadSync);
		}
		if (_identifier.Location == DataLoader.DataPathIdentifier.AssetLocation.Addressable)
		{
			return LoadManager.LoadAssetFromAddressables<T>(_identifier.AssetName, _callback, _lg, _deferLoading, _loadSync, _ignoreDlcEntitlements);
		}
		return LoadManager.LoadAssetFromResources<T>(_identifier.AssetName, _callback, _lg, _deferLoading, _loadSync);
	}

	public static LoadManager.AssetRequestTask<T> LoadAsset<T>(string _uri, Action<T> _callback = null, LoadManager.LoadGroup _lg = null, bool _deferLoading = false, bool _loadSync = false, bool _ignoreDlcEntitlements = false) where T : Object
	{
		return LoadManager.LoadAsset<T>(DataLoader.ParseDataPathIdentifier(_uri), _callback, _lg, _deferLoading, _loadSync, _ignoreDlcEntitlements);
	}

	public static LoadManager.AssetRequestTask<T> LoadAsset<T>(string _bundlePath, string _assetName, Action<T> _callback = null, LoadManager.LoadGroup _lg = null, bool _deferLoading = false, bool _loadSync = false) where T : Object
	{
		return LoadManager.LoadAsset<T>(new DataLoader.DataPathIdentifier(_assetName, _bundlePath, false), _callback, _lg, _deferLoading, _loadSync, false);
	}

	public static LoadManager.ResourceRequestTask<T> LoadAssetFromResources<T>(string _resourcePath, Action<T> _callback = null, LoadManager.LoadGroup _lg = null, bool _deferLoading = false, bool _loadSync = false) where T : Object
	{
		if (_lg == null)
		{
			_lg = LoadManager.rootGroup;
		}
		if (ThreadManager.IsInSyncCoroutine && ThreadManager.IsMainThread())
		{
			_loadSync = true;
		}
		LoadManager.ResourceRequestTask<T> resourceRequestTask = new LoadManager.ResourceRequestTask<T>(_lg, !_loadSync, _resourcePath, _callback);
		LoadManager.addOrExecLoadTask(_lg, resourceRequestTask, _deferLoading, _loadSync);
		return resourceRequestTask;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static LoadManager.AssetBundleRequestTask<T> LoadAssetFromBundle<T>(DataLoader.DataPathIdentifier _identifier, Action<T> _callback = null, LoadManager.LoadGroup _lg = null, bool _deferLoading = false, bool _loadSync = false) where T : Object
	{
		if (_lg == null)
		{
			_lg = LoadManager.rootGroup;
		}
		LoadManager.AssetBundleRequestTask<T> assetBundleRequestTask = new LoadManager.AssetBundleRequestTask<T>(_lg, !_loadSync, _identifier, _callback);
		LoadManager.addOrExecLoadTask(_lg, assetBundleRequestTask, _deferLoading, _loadSync);
		return assetBundleRequestTask;
	}

	public static LoadManager.AddressableRequestTask<T> LoadAssetFromAddressables<T>(object _key, Action<T> _callback = null, LoadManager.LoadGroup _lg = null, bool _deferLoading = false, bool _loadSync = false, bool _ignoreDlcEntitlements = false) where T : Object
	{
		if (_lg == null)
		{
			_lg = LoadManager.rootGroup;
		}
		if (!_ignoreDlcEntitlements && !EntitlementManager.Instance.HasEntitlement(_key))
		{
			Log.Error(string.Format("Tried to load asset without proper DLC entitlement. Missing {0}", EntitlementManager.Instance.GetSetForAsset(_key)));
			return LoadManager.AddressableRequestTask<T>.Empty();
		}
		LoadManager.LoadGroup loadGroup = _lg;
		bool flag = !_loadSync;
		string text = _key as string;
		LoadManager.AddressableRequestTask<T> addressableRequestTask = new LoadManager.AddressableRequestTask<T>(loadGroup, flag, (text != null) ? LoadManager.GetAddressablesCase(text) : _key, _callback);
		LoadManager.addOrExecLoadTask(_lg, addressableRequestTask, _deferLoading, _loadSync);
		return addressableRequestTask;
	}

	public static LoadManager.AddressableRequestTask<T> LoadAssetFromAddressables<T>(string _folderAddress, string _assetPath, Action<T> _callback = null, LoadManager.LoadGroup _lg = null, bool _deferLoading = false, bool _loadSync = false, bool _ignoreDlcEntitlements = false) where T : Object
	{
		return LoadManager.LoadAssetFromAddressables<T>(_folderAddress + "/" + _assetPath, _callback, _lg, _deferLoading, _loadSync, _ignoreDlcEntitlements);
	}

	public static LoadManager.AddressableAssetsRequestTask<T> LoadAssetsFromAddressables<T>(string _label, Func<string, bool> _addressFilter = null, LoadManager.LoadGroup _lg = null, bool _deferLoading = false, bool _loadSync = false) where T : Object
	{
		if (_lg == null)
		{
			_lg = LoadManager.rootGroup;
		}
		LoadManager.AddressableAssetsRequestTask<T> addressableAssetsRequestTask = new LoadManager.AddressableAssetsRequestTask<T>(_lg, !_loadSync, _label, _addressFilter);
		LoadManager.addOrExecLoadTask(_lg, addressableAssetsRequestTask, _deferLoading, _loadSync);
		return addressableAssetsRequestTask;
	}

	public static void ReleaseAddressable<T>(T _obj)
	{
		Addressables.Release<T>(_obj);
	}

	public static LoadManager.FileLoadTask LoadFile(string _path, LoadManager.FileLoadCallback _callback = null, LoadManager.LoadGroup _lg = null, bool _deferLoading = false, bool _loadSync = false)
	{
		if (_lg == null)
		{
			_lg = LoadManager.rootGroup;
		}
		LoadManager.FileLoadTask fileLoadTask = new LoadManager.FileLoadTask(_lg, !_loadSync, _path, _callback);
		LoadManager.addOrExecLoadTask(_lg, fileLoadTask, _deferLoading, _loadSync);
		return fileLoadTask;
	}

	public static IEnumerator WaitAll(IEnumerable<LoadManager.LoadTask> _tasks)
	{
		foreach (LoadManager.LoadTask task in _tasks)
		{
			while (!task.IsDone)
			{
				yield return null;
			}
			task = null;
		}
		IEnumerator<LoadManager.LoadTask> enumerator = null;
		yield break;
		yield break;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void addOrExecLoadTask(LoadManager.LoadGroup _lg, LoadManager.LoadTask _task, bool _deferLoading = false, bool _loadSync = false)
	{
		if (!LoadManager.forceLoadSync && !_loadSync)
		{
			_lg.IncrementPending();
			if (!_deferLoading)
			{
				LoadManager.AddTask(_lg, _task);
				return;
			}
			List<LoadManager.LoadTask> list = LoadManager.deferedLoadRequests;
			lock (list)
			{
				LoadManager.deferedLoadRequests.Add(_task);
				return;
			}
		}
		_task.LoadSync();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void AddTask(LoadManager.LoadGroup _lg, LoadManager.LoadTask _task)
	{
		if (GameManager.IsDedicatedServer)
		{
			_task.LoadSync();
			_lg.DecrementPending();
			return;
		}
		if (_task.Load())
		{
			LoadManager.loadRequests.Add(_task);
			return;
		}
		_lg.DecrementPending();
		_task.Complete();
	}

	public static string GetAddressablesCase(string key)
	{
		if (LoadManager.addressablesCaseMap == null)
		{
			Log.Error("Addressables Case Map not initialised - are you calling GetAddressablesCase before LoadManager.Init?");
		}
		if (string.IsNullOrEmpty(key))
		{
			return key;
		}
		string text;
		if (LoadManager.addressablesCaseMap.TryGetValue(key, out text))
		{
			return text;
		}
		return key;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static Action<LoadManager.LoadTask> updateRequestAction;

	[PublicizedFrom(EAccessModifier.Private)]
	public static Dictionary<string, string> addressablesCaseMap;

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool forceLoadSync;

	[PublicizedFrom(EAccessModifier.Private)]
	public static LoadManager.LoadGroup rootGroup;

	[PublicizedFrom(EAccessModifier.Private)]
	public static WorkBatch<LoadManager.LoadTask> loadRequests;

	[PublicizedFrom(EAccessModifier.Private)]
	public static List<LoadManager.LoadTask> deferedLoadRequests;

	public delegate void CompletionCallback();

	public delegate void FileLoadCallback(byte[] _content);

	public class LoadGroup
	{
		public bool Pending
		{
			get
			{
				return Interlocked.CompareExchange(ref this.pending, 0, 0) != 0;
			}
		}

		[PublicizedFrom(EAccessModifier.Internal)]
		public LoadGroup(LoadManager.LoadGroup _parent)
		{
			this.parent = _parent;
			this.pending = 0;
		}

		[PublicizedFrom(EAccessModifier.Internal)]
		public void IncrementPending()
		{
			if (GameManager.IsDedicatedServer)
			{
				return;
			}
			for (LoadManager.LoadGroup loadGroup = this; loadGroup != null; loadGroup = loadGroup.parent)
			{
				Interlocked.Increment(ref loadGroup.pending);
			}
		}

		[PublicizedFrom(EAccessModifier.Internal)]
		public void DecrementPending()
		{
			if (GameManager.IsDedicatedServer)
			{
				return;
			}
			for (LoadManager.LoadGroup loadGroup = this; loadGroup != null; loadGroup = loadGroup.parent)
			{
				Interlocked.Decrement(ref loadGroup.pending);
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly LoadManager.LoadGroup parent;

		[PublicizedFrom(EAccessModifier.Private)]
		public int pending;
	}

	public abstract class LoadTask : CustomYieldInstruction
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		public LoadTask(LoadManager.LoadGroup _group, bool _loadAsync)
		{
			this.group = _group;
			this.loadAsync = _loadAsync;
		}

		public abstract bool IsDone { get; }

		public abstract bool INTERNAL_IsPending { get; }

		public override bool keepWaiting
		{
			get
			{
				return this.INTERNAL_IsPending;
			}
		}

		public LoadManager.LoadGroup Group
		{
			get
			{
				return this.group;
			}
		}

		public abstract bool Load();

		public abstract void LoadSync();

		public virtual void Update()
		{
		}

		public abstract void Complete();

		public void WaitForComplete()
		{
			this.CompleteNow();
			LoadManager.RemoveRequest(this);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public abstract void CompleteNow();

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly LoadManager.LoadGroup group;

		[PublicizedFrom(EAccessModifier.Protected)]
		public readonly bool loadAsync;

		[PublicizedFrom(EAccessModifier.Protected)]
		public bool loadStarted;
	}

	public class AssetBundleLoadTask : LoadManager.LoadTask
	{
		public override bool INTERNAL_IsPending
		{
			get
			{
				return true;
			}
		}

		public override bool IsDone
		{
			get
			{
				return false;
			}
		}

		public AssetBundleLoadTask(LoadManager.LoadGroup _group, bool _loadAsync, Action<AssetBundle> _callback)
			: base(_group, _loadAsync)
		{
			this.callback = _callback;
		}

		public override bool Load()
		{
			throw new NotImplementedException();
		}

		public override void LoadSync()
		{
			throw new NotImplementedException();
		}

		public override void Complete()
		{
			throw new NotImplementedException();
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override void CompleteNow()
		{
			throw new NotImplementedException();
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly Action<AssetBundle> callback;
	}

	public abstract class AssetRequestTask<T> : LoadManager.LoadTask where T : Object
	{
		public override bool keepWaiting
		{
			get
			{
				return this.INTERNAL_IsPending || !this.assetRetrieved;
			}
		}

		public override bool IsDone
		{
			get
			{
				return this.assetRetrieved;
			}
		}

		public T Asset
		{
			get
			{
				return this.asset;
			}
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public AssetRequestTask(LoadManager.LoadGroup _group, bool _loadAsync, Action<T> _callback)
			: base(_group, _loadAsync)
		{
			this.callback = _callback;
		}

		public virtual LoadManager.IAssetHandle GetHandle()
		{
			return LoadManager.DummyHandle.Instance;
		}

		public virtual void Release()
		{
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public readonly Action<T> callback;

		[PublicizedFrom(EAccessModifier.Protected)]
		public T asset;

		[PublicizedFrom(EAccessModifier.Protected)]
		public bool assetRetrieved;
	}

	public abstract class AssetsRequestTask<T> : LoadManager.LoadTask where T : Object
	{
		public override bool keepWaiting
		{
			get
			{
				return this.INTERNAL_IsPending || !this.assetsRetrieved;
			}
		}

		public override bool IsDone
		{
			get
			{
				return this.assetsRetrieved;
			}
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public AssetsRequestTask(LoadManager.LoadGroup _group, bool _loadAsync)
			: base(_group, _loadAsync)
		{
		}

		public abstract void CollectResults(List<T> _results);

		[PublicizedFrom(EAccessModifier.Protected)]
		public bool assetsRetrieved;
	}

	public class ResourceRequestTask<T> : LoadManager.AssetRequestTask<T> where T : Object
	{
		public override bool INTERNAL_IsPending
		{
			get
			{
				return this.loadAsync && !this.asyncCancelled && (!this.loadStarted || (this.request != null && !this.request.isDone));
			}
		}

		public ResourceRequestTask(LoadManager.LoadGroup _group, bool _loadAsync, string _assetPath, Action<T> _callback)
			: base(_group, _loadAsync, _callback)
		{
			this.assetPath = _assetPath;
		}

		public override bool Load()
		{
			this.request = Resources.LoadAsync<T>(this.assetPath);
			this.loadStarted = true;
			return this.request != null;
		}

		public override void LoadSync()
		{
			this.asset = Resources.Load<T>(this.assetPath);
			this.assetRetrieved = true;
			if (this.callback != null)
			{
				this.callback(this.asset);
			}
		}

		public override void Complete()
		{
			if (this.INTERNAL_IsPending)
			{
				throw new Exception("ResourceRequestTask still pending.");
			}
			this.asset = this.request.asset as T;
			this.assetRetrieved = true;
			if (this.callback == null)
			{
				return;
			}
			this.callback(this.asset);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override void CompleteNow()
		{
			if (this.loadAsync)
			{
				if (this.assetRetrieved)
				{
					throw new Exception("ResourceRequestTask async already completed.");
				}
				if (this.request != null)
				{
					Log.Warning("ResourceRequestTask async WaitForComplete is not supported, asset " + this.assetPath + " will be loaded again sync");
				}
				this.asyncCancelled = true;
				this.LoadSync();
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly string assetPath;

		[PublicizedFrom(EAccessModifier.Private)]
		public ResourceRequest request;

		[PublicizedFrom(EAccessModifier.Private)]
		public bool asyncCancelled;
	}

	public class AssetBundleRequestTask<T> : LoadManager.AssetRequestTask<T> where T : Object
	{
		public override bool INTERNAL_IsPending
		{
			get
			{
				return this.loadAsync && !this.asyncCancelled && (!this.loadStarted || (this.request != null && !this.request.IsDone));
			}
		}

		public AssetBundleRequestTask(LoadManager.LoadGroup _group, bool _loadAsync, DataLoader.DataPathIdentifier _identifier, Action<T> _callback)
			: base(_group, _loadAsync, _callback)
		{
			this.identifier = _identifier;
			if (typeof(T) == typeof(Transform))
			{
				this.isGameObject = true;
			}
		}

		public override bool Load()
		{
			if (this.isGameObject)
			{
				this.request = AssetBundleManager.Instance.GetAsync<GameObject>(this.identifier.BundlePath, this.identifier.AssetName, this.identifier.FromMod);
			}
			else
			{
				this.request = AssetBundleManager.Instance.GetAsync<T>(this.identifier.BundlePath, this.identifier.AssetName, this.identifier.FromMod);
			}
			this.loadStarted = true;
			return this.request != null;
		}

		public override void LoadSync()
		{
			if (this.isGameObject)
			{
				GameObject gameObject = AssetBundleManager.Instance.Get<GameObject>(this.identifier.BundlePath, this.identifier.AssetName, this.identifier.FromMod);
				if (gameObject != null)
				{
					this.asset = gameObject.GetComponent<T>();
				}
			}
			else
			{
				this.asset = AssetBundleManager.Instance.Get<T>(this.identifier.BundlePath, this.identifier.AssetName, this.identifier.FromMod);
			}
			this.assetRetrieved = true;
			if (this.callback != null)
			{
				this.callback(this.asset);
			}
		}

		public override void Complete()
		{
			if (this.INTERNAL_IsPending)
			{
				throw new Exception("AssetBundleRequestTask still pending.");
			}
			if (this.isGameObject)
			{
				GameObject gameObject = this.request.Asset as GameObject;
				if (gameObject != null)
				{
					this.asset = gameObject.GetComponent<T>();
				}
			}
			else
			{
				this.asset = this.request.Asset as T;
			}
			this.assetRetrieved = true;
			if (this.callback == null)
			{
				return;
			}
			this.callback(this.asset);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override void CompleteNow()
		{
			if (this.loadAsync)
			{
				if (this.assetRetrieved)
				{
					throw new Exception("AssetBundleRequestTask async already completed.");
				}
				if (this.request != null)
				{
					Log.Warning(string.Concat(new string[]
					{
						"AssetBundleRequestTask async WaitForComplete is not supported, asset ",
						this.identifier.AssetName,
						" from ",
						this.identifier.BundlePath,
						" will be loaded again sync"
					}));
				}
				this.asyncCancelled = true;
				this.LoadSync();
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly DataLoader.DataPathIdentifier identifier;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly bool isGameObject;

		[PublicizedFrom(EAccessModifier.Private)]
		public AssetBundleManager.AssetBundleRequestTFP request;

		[PublicizedFrom(EAccessModifier.Private)]
		public bool asyncCancelled;
	}

	public class AddressableRequestTask<T> : LoadManager.AssetRequestTask<T> where T : Object
	{
		public override bool INTERNAL_IsPending
		{
			get
			{
				return this.loadAsync && (!this.loadStarted || (this.request.IsValid() && !this.request.IsDone) || (this.gameObjectRequest.IsValid() && !this.gameObjectRequest.IsDone));
			}
		}

		public AddressableRequestTask(LoadManager.LoadGroup _group, bool _loadAsync, object _key, Action<T> _callback)
			: base(_group, _loadAsync, _callback)
		{
			this.key = _key;
			if (typeof(T) == typeof(Transform))
			{
				this.isGameObject = true;
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void StartRequest()
		{
			if (this.loadStarted)
			{
				return;
			}
			if (this.isGameObject)
			{
				this.gameObjectRequest = Addressables.LoadAssetAsync<GameObject>(this.key);
			}
			else
			{
				this.request = Addressables.LoadAssetAsync<T>(this.key);
			}
			this.loadStarted = true;
		}

		public override bool Load()
		{
			this.StartRequest();
			return this.request.IsValid() || this.gameObjectRequest.IsValid();
		}

		public override void LoadSync()
		{
			this.StartRequest();
			if (this.isGameObject)
			{
				this.gameObjectRequest.WaitForCompletion();
				GameObject result = this.gameObjectRequest.Result;
				if (result != null)
				{
					this.asset = result.GetComponent<T>();
				}
			}
			else
			{
				this.request.WaitForCompletion();
				this.asset = this.request.Result;
			}
			this.assetRetrieved = true;
			if (this.callback != null)
			{
				this.callback(this.asset);
			}
		}

		public override void Complete()
		{
			if (this.INTERNAL_IsPending)
			{
				throw new Exception("AddressableRequestTask still pending.");
			}
			if (this.isGameObject)
			{
				GameObject result = this.gameObjectRequest.Result;
				if (result != null)
				{
					this.asset = result.GetComponent<T>();
				}
			}
			else
			{
				this.asset = this.request.Result;
			}
			this.assetRetrieved = true;
			if (this.callback == null)
			{
				return;
			}
			this.callback(this.asset);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override void CompleteNow()
		{
			if (this.loadAsync)
			{
				if (this.assetRetrieved)
				{
					throw new Exception("AddressableRequestTask async already completed.");
				}
				this.LoadSync();
			}
		}

		public override LoadManager.IAssetHandle GetHandle()
		{
			if (this.isGameObject)
			{
				return new LoadManager.AddressableAssetHandle<GameObject>(this.gameObjectRequest);
			}
			return new LoadManager.AddressableAssetHandle<T>(this.request);
		}

		public override void Release()
		{
			this.asset = default(T);
			if (this.isGameObject)
			{
				Addressables.Release<GameObject>(this.gameObjectRequest);
				return;
			}
			Addressables.Release<T>(this.request);
		}

		public static LoadManager.AddressableRequestTask<T> Empty()
		{
			return LoadManager.AddressableRequestTask<T>.emptyInstance;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly object key;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly bool isGameObject;

		[PublicizedFrom(EAccessModifier.Private)]
		public AsyncOperationHandle<T> request;

		[PublicizedFrom(EAccessModifier.Private)]
		public AsyncOperationHandle<GameObject> gameObjectRequest;

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly LoadManager.EmptyAddressableRequestTask<T> emptyInstance = new LoadManager.EmptyAddressableRequestTask<T>();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public class EmptyAddressableRequestTask<T> : LoadManager.AddressableRequestTask<T> where T : Object
	{
		public EmptyAddressableRequestTask()
			: base(null, true, null, null)
		{
		}

		public override bool INTERNAL_IsPending
		{
			get
			{
				return false;
			}
		}

		public override bool IsDone
		{
			get
			{
				return true;
			}
		}

		public override bool Load()
		{
			return false;
		}

		public override void LoadSync()
		{
		}

		public override void Complete()
		{
		}

		public override void Release()
		{
		}

		public override LoadManager.IAssetHandle GetHandle()
		{
			return LoadManager.DummyHandle.Instance;
		}
	}

	public class AddressableAssetsRequestTask<T> : LoadManager.AssetsRequestTask<T> where T : Object
	{
		public override bool INTERNAL_IsPending
		{
			get
			{
				return this.loadAsync && (!this.loadStarted || !this.assetRequestStarted || (this.assetsRequest.IsValid() && !this.assetsRequest.IsDone));
			}
		}

		public AddressableAssetsRequestTask(LoadManager.LoadGroup _group, bool _loadAsync, string _label, Func<string, bool> _addressFilter = null)
			: base(_group, _loadAsync)
		{
			this.label = _label;
			this.addressFilter = _addressFilter;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void StartLocationsRequest()
		{
			if (this.loadStarted)
			{
				return;
			}
			this.loadStarted = true;
			this.locationRequest = Addressables.LoadResourceLocationsAsync(this.label, typeof(T));
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void StartAssetsRequest()
		{
			if (this.assetRequestStarted)
			{
				return;
			}
			IList<IResourceLocation> list;
			if (this.addressFilter != null)
			{
				list = new List<IResourceLocation>();
				using (IEnumerator<IResourceLocation> enumerator = this.locationRequest.Result.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						IResourceLocation resourceLocation = enumerator.Current;
						if (this.addressFilter(resourceLocation.PrimaryKey))
						{
							list.Add(resourceLocation);
						}
					}
					goto IL_006B;
				}
			}
			list = this.locationRequest.Result;
			IL_006B:
			if (list.Count > 0)
			{
				this.assetsRequest = Addressables.LoadAssetsAsync<T>(list, null);
			}
			this.assetRequestStarted = true;
			if (!this.assetsRequest.IsValid() || this.assetsRequest.IsDone)
			{
				this.Complete();
			}
		}

		public override void Update()
		{
			base.Update();
			if (!this.loadAsync)
			{
				return;
			}
			if (this.locationRequest.IsValid() && this.locationRequest.IsDone)
			{
				this.StartAssetsRequest();
			}
		}

		public override bool Load()
		{
			this.StartLocationsRequest();
			return this.locationRequest.IsValid();
		}

		public override void LoadSync()
		{
			this.StartLocationsRequest();
			if (this.locationRequest.IsValid())
			{
				this.locationRequest.WaitForCompletion();
				this.StartAssetsRequest();
				if (this.assetsRequest.IsValid())
				{
					this.assetsRequest.WaitForCompletion();
				}
			}
			this.Complete();
		}

		public override void Complete()
		{
			if (this.INTERNAL_IsPending)
			{
				throw new Exception("AddressableAssetsRequestTask still pending.");
			}
			this.assetsRetrieved = true;
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override void CompleteNow()
		{
			if (this.loadAsync)
			{
				if (this.assetsRetrieved)
				{
					throw new Exception("AddressableAssetsRequestTask async already completed.");
				}
				this.LoadSync();
			}
		}

		public override void CollectResults(List<T> _results)
		{
			if (!this.assetsRetrieved)
			{
				Log.Warning("Collecting Addressable assets request results before operation has completed");
			}
			if (this.assetsRequest.IsValid() && this.assetsRetrieved)
			{
				foreach (T t in this.assetsRequest.Result)
				{
					_results.Add(t);
				}
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public string label;

		[PublicizedFrom(EAccessModifier.Private)]
		public Func<string, bool> addressFilter;

		[PublicizedFrom(EAccessModifier.Private)]
		public AsyncOperationHandle<IList<IResourceLocation>> locationRequest;

		[PublicizedFrom(EAccessModifier.Private)]
		public bool assetRequestStarted;

		[PublicizedFrom(EAccessModifier.Private)]
		public AsyncOperationHandle<IList<T>> assetsRequest;
	}

	public class CoroutineTask : LoadManager.LoadTask
	{
		public override bool INTERNAL_IsPending
		{
			get
			{
				return !this.isDone;
			}
		}

		public override bool IsDone
		{
			get
			{
				return !this.INTERNAL_IsPending;
			}
		}

		public CoroutineTask(LoadManager.LoadGroup _group, IEnumerator _task, LoadManager.CompletionCallback _callback)
			: base(_group, true)
		{
			this.task = _task;
			this.callback = _callback;
		}

		public override bool Load()
		{
			return true;
		}

		public override void LoadSync()
		{
			throw new Exception("CoroutineTask doesn't support synchronous loading.");
		}

		public override void Update()
		{
			this.isDone = !this.task.MoveNext();
		}

		public override void Complete()
		{
			if (this.callback != null)
			{
				this.callback();
			}
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override void CompleteNow()
		{
			throw new Exception("CoroutineTask doesn't support synchronous loading.");
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly IEnumerator task;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly LoadManager.CompletionCallback callback;

		[PublicizedFrom(EAccessModifier.Private)]
		public bool isDone;
	}

	public class FileLoadTask : LoadManager.LoadTask
	{
		public override bool INTERNAL_IsPending
		{
			get
			{
				return !this.isDone;
			}
		}

		public override bool IsDone
		{
			get
			{
				return !this.INTERNAL_IsPending;
			}
		}

		public FileLoadTask(LoadManager.LoadGroup _group, bool _loadAsync, string _path, LoadManager.FileLoadCallback _callback)
			: base(_group, _loadAsync)
		{
			this.path = _path;
			this.callback = _callback;
		}

		public override bool Load()
		{
			this.asyncLoadTask = ThreadManager.AddSingleTask(delegate(ThreadManager.TaskInfo _threadInfo)
			{
				try
				{
					this.content = SdFile.ReadAllBytes(this.path);
				}
				catch (Exception ex)
				{
					Log.Out("LoadManager.FileLoadTask.Load - Failed to load file: " + ex.Message);
					Log.Out(ex.StackTrace);
				}
				this.isDone = true;
			}, null, null, true);
			this.loadStarted = true;
			return true;
		}

		public override void LoadSync()
		{
			try
			{
				this.content = SdFile.ReadAllBytes(this.path);
			}
			catch (Exception ex)
			{
				Log.Out("LoadManager.FileLoadTask.LoadSync - Failed to load file: " + ex.Message);
				Log.Out(ex.StackTrace);
			}
			this.isDone = true;
			if (this.callback != null)
			{
				this.callback(this.content);
			}
			this.isCompleted = true;
		}

		public override void Complete()
		{
			if (this.isDone)
			{
				if (this.callback != null)
				{
					this.callback(this.content);
				}
				this.isCompleted = true;
				return;
			}
			throw new Exception("[LoadManager] FileLoadTask still pending.");
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override void CompleteNow()
		{
			if (this.loadAsync)
			{
				if (this.isCompleted)
				{
					throw new Exception("AssetBundleRequestTask async already completed.");
				}
				if (this.asyncLoadTask == null)
				{
					throw new Exception("AssetBundleRequestTask async has not started.");
				}
				this.asyncLoadTask.WaitForEnd();
				this.Complete();
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly string path;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly LoadManager.FileLoadCallback callback;

		[PublicizedFrom(EAccessModifier.Private)]
		public byte[] content;

		[PublicizedFrom(EAccessModifier.Private)]
		public volatile bool isDone;

		[PublicizedFrom(EAccessModifier.Private)]
		public ThreadManager.TaskInfo asyncLoadTask;

		[PublicizedFrom(EAccessModifier.Private)]
		public bool isCompleted;
	}

	public interface IAssetHandle
	{
		LoadManager.IAssetHandle Copy();

		void Release();
	}

	public class AddressableAssetHandle<T> : LoadManager.IAssetHandle where T : Object
	{
		public AddressableAssetHandle(AsyncOperationHandle<T> _handle)
		{
			this.handle = Addressables.ResourceManager.Acquire<T>(_handle);
		}

		public LoadManager.IAssetHandle Copy()
		{
			return new LoadManager.AddressableAssetHandle<T>(this.handle);
		}

		public void Release()
		{
			this.handle.Release();
		}

		public override string ToString()
		{
			return this.handle.DebugName ?? "";
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public AsyncOperationHandle<T> handle;
	}

	public class DummyHandle : LoadManager.IAssetHandle
	{
		public LoadManager.IAssetHandle Copy()
		{
			return LoadManager.DummyHandle.Instance;
		}

		public void Release()
		{
		}

		public static LoadManager.DummyHandle Instance = new LoadManager.DummyHandle();
	}
}
