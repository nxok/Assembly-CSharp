using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class GameObjectPool
{
	public static GameObjectPool Instance
	{
		get
		{
			if (GameObjectPool.instance == null)
			{
				GameObjectPool.Instantiate();
			}
			return GameObjectPool.instance;
		}
	}

	public int MaxPooledInstancesPerItem
	{
		get
		{
			return this.maxPooledInstancesPerItem;
		}
		set
		{
			this.maxPooledInstancesPerItem = value;
			Log.Out(string.Format("[GameObjectPool] {0} set to {1}", "MaxPooledInstancesPerItem", this.maxPooledInstancesPerItem));
		}
	}

	public int MaxDestroysPerUpdate
	{
		get
		{
			return this.maxDestroysPerUpdate;
		}
		set
		{
			this.maxDestroysPerUpdate = value;
			Log.Out(string.Format("[GameObjectPool] {0} set to {1}", "MaxDestroysPerUpdate", this.maxDestroysPerUpdate));
		}
	}

	public GameObjectPool.ShrinkThreshold ShrinkThresholdHigh
	{
		get
		{
			return this.shrinkThresholdHigh;
		}
		set
		{
			this.shrinkThresholdHigh = value;
			Log.Out(string.Format("[GameObjectPool] {0} set to {1}", "ShrinkThresholdHigh", this.shrinkThresholdHigh));
		}
	}

	public GameObjectPool.ShrinkThreshold ShrinkThresholdMedium
	{
		get
		{
			return this.shrinkThresholdMedium;
		}
		set
		{
			this.shrinkThresholdMedium = value;
			Log.Out(string.Format("[GameObjectPool] {0} set to {1}", "ShrinkThresholdMedium", this.shrinkThresholdMedium));
		}
	}

	public GameObjectPool.ShrinkThreshold ShrinkThresholdLow
	{
		get
		{
			return this.shrinkThresholdLow;
		}
		set
		{
			this.shrinkThresholdLow = value;
			Log.Out(string.Format("[GameObjectPool] {0} set to {1}", "ShrinkThresholdLow", this.shrinkThresholdLow));
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void Instantiate()
	{
		GameObjectPool.instance = new GameObjectPool();
	}

	public void Init()
	{
		PlatformOptimizations.ConfigureGameObjectPoolForPlatform(this);
		this.tintMaskShader = GlobalAssets.FindShader("Game/Entity Tint Mask");
	}

	public void Cleanup()
	{
		foreach (KeyValuePair<string, GameObjectPool.PoolItem> keyValuePair in this.pool)
		{
			List<GameObject> objs = keyValuePair.Value.objs;
			for (int i = 0; i < objs.Count; i++)
			{
				this.DestroyObject(objs[i].gameObject);
			}
			objs.Clear();
		}
		this.activePool.Clear();
		for (int j = 0; j < this.asyncItems.Count; j++)
		{
			GameObjectPool.AsyncItem asyncItem = this.asyncItems[j];
			if (!asyncItem.async.isDone)
			{
				asyncItem.async.Cancel();
			}
		}
		this.asyncItems.Clear();
	}

	public void FrameUpdate()
	{
		float time = Time.time;
		int num = 0;
		for (int i = this.activePool.Count - 1; i >= 0; i--)
		{
			GameObjectPool.PoolItem poolItem = this.activePool[i];
			if (poolItem.updateTime - time <= 0f)
			{
				int num2 = poolItem.objs.Count;
				if (num2 <= 0)
				{
					if (poolItem.activeCount <= 0)
					{
						this.activePool.RemoveAt(i);
					}
				}
				else
				{
					GameObjectPool.ShrinkThreshold shrinkThreshold = this.GetShrinkThreshold(num2);
					poolItem.updateTime = time + shrinkThreshold.Delay;
					int num3 = Mathf.Min(num2, shrinkThreshold.DestroyCount);
					int num4 = 0;
					while (num4 < num3 && num < this.maxDestroysPerUpdate)
					{
						num2--;
						GameObject gameObject = poolItem.objs[num2];
						poolItem.objs.RemoveAt(num2);
						poolItem.activeCount--;
						this.DestroyObject(gameObject);
						num++;
						num4++;
					}
					if (num >= this.maxDestroysPerUpdate)
					{
						break;
					}
				}
			}
		}
		for (int j = this.asyncItems.Count - 1; j >= 0; j--)
		{
			GameObjectPool.AsyncItem asyncItem = this.asyncItems[j];
			if (asyncItem.async.isDone)
			{
				Object[] result = asyncItem.async.Result;
				int num5 = result.Length;
				GameObjectPool.PoolItem item = asyncItem.item;
				item.activeCount += num5;
				for (int k = 0; k < num5; k++)
				{
					GameObject gameObject2 = (GameObject)result[k];
					gameObject2.name = item.name;
					if (item.createOnceToAllCallback != null)
					{
						item.createOnceToAllCallback(gameObject2);
						item.createOnceToAllCallback = null;
					}
					if (item.createCallback != null)
					{
						item.createCallback(gameObject2);
					}
				}
				asyncItem.callback(asyncItem.userData, result, num5, true);
				this.asyncItems.RemoveAt(j);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public GameObjectPool.ShrinkThreshold GetShrinkThreshold(int count)
	{
		if (count >= this.shrinkThresholdHigh.Count)
		{
			return this.shrinkThresholdHigh;
		}
		if (count >= this.shrinkThresholdMedium.Count)
		{
			return this.shrinkThresholdMedium;
		}
		if (count >= this.shrinkThresholdLow.Count)
		{
			return this.shrinkThresholdLow;
		}
		return this.shrinkThresholdMin;
	}

	public void AddPooledObject(string name, GameObjectPool.LoadCallback _loadCallback, GameObjectPool.CreateCallback _createOnceToAllCallback, GameObjectPool.CreateCallback _createCallback)
	{
		GameObjectPool.PoolItem poolItem;
		if (!this.pool.TryGetValue(name, out poolItem))
		{
			poolItem = new GameObjectPool.PoolItem();
			poolItem.name = name;
			poolItem.loadCallback = _loadCallback;
			poolItem.createOnceToAllCallback = _createOnceToAllCallback;
			poolItem.createCallback = _createCallback;
			poolItem.objs = new List<GameObject>();
			this.pool.Add(name, poolItem);
		}
		else
		{
			GameObjectPool.PoolItem poolItem2 = poolItem;
			poolItem2.createOnceToAllCallback = (GameObjectPool.CreateCallback)Delegate.Combine(poolItem2.createOnceToAllCallback, _createOnceToAllCallback);
		}
		Transform transform = poolItem.loadCallback();
		if (transform)
		{
			this.setItemPrefab(poolItem, transform.gameObject);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setItemPrefab(GameObjectPool.PoolItem item, GameObject go)
	{
		item.prefab = go;
		this.getOriginalTint(item, go);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void getOriginalTint(GameObjectPool.PoolItem item, GameObject go)
	{
		bool flag = false;
		List<Color> list = new List<Color>();
		Renderer[] componentsInChildren = go.GetComponentsInChildren<Renderer>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			foreach (Material material in componentsInChildren[i].sharedMaterials)
			{
				if (!(material != null) || !(material.shader == this.tintMaskShader))
				{
					flag = true;
					break;
				}
				list.Add(material.color);
			}
		}
		item.originalTint = Color.clear;
		if (!flag && list.Count > 0)
		{
			int num = 1;
			while (num < list.Count && !(list[0] != list[num]))
			{
				num++;
			}
			if (num == list.Count)
			{
				item.originalTint = list[0];
			}
		}
	}

	public GameObject GetObjectForType(string objectType)
	{
		Color color;
		return this.GetObjectForType(objectType, out color);
	}

	public GameObject GetObjectForType(string objectType, out Color originalTint)
	{
		GameObjectPool.PoolItem poolItem;
		if (!this.pool.TryGetValue(objectType, out poolItem))
		{
			Log.Error("GameObjectPool GetObjectForType {0} unknown", new object[] { objectType });
			originalTint = Color.white;
			return null;
		}
		GameObject gameObject = poolItem.prefab;
		if (!gameObject)
		{
			Transform transform = poolItem.loadCallback();
			if (transform)
			{
				gameObject = transform.gameObject;
				this.setItemPrefab(poolItem, gameObject);
			}
		}
		originalTint = poolItem.originalTint;
		if (!gameObject)
		{
			return null;
		}
		List<GameObject> objs = poolItem.objs;
		int count = objs.Count;
		if (count > 0)
		{
			poolItem.updateTime = Time.time + 5f;
			GameObject gameObject2 = objs[count - 1];
			objs.RemoveAt(count - 1);
			return gameObject2;
		}
		return poolItem.Instantiate();
	}

	public GameObjectPool.AsyncItem GetObjectsForTypeAsync(string objectType, int _count, GameObjectPool.CreateAsyncCallback _callback, object _userData)
	{
		GameObjectPool.PoolItem poolItem;
		if (!this.pool.TryGetValue(objectType, out poolItem))
		{
			Log.Error("GameObjectPool GetObjectForType {0} unknown", new object[] { objectType });
			return null;
		}
		GameObject gameObject = poolItem.prefab;
		if (!gameObject)
		{
			Transform transform = poolItem.loadCallback();
			if (transform)
			{
				gameObject = transform.gameObject;
				this.setItemPrefab(poolItem, gameObject);
			}
		}
		if (!gameObject)
		{
			return null;
		}
		List<GameObject> objs = poolItem.objs;
		int count = objs.Count;
		if (count >= _count && count <= 128)
		{
			poolItem.updateTime = Time.time + 5f;
			for (int i = 0; i < _count; i++)
			{
				int num = count - 1 - i;
				GameObject gameObject2 = objs[num];
				objs.RemoveAt(num);
				this.asyncPoolObjs[i] = gameObject2;
			}
			Object[] array = this.asyncPoolObjs;
			_callback(_userData, array, _count, false);
			return null;
		}
		if (_count <= 3)
		{
			for (int j = 0; j < _count; j++)
			{
				GameObject gameObject3 = poolItem.Instantiate();
				this.asyncPoolObjs[j] = gameObject3;
			}
			Object[] array = this.asyncPoolObjs;
			_callback(_userData, array, _count, false);
			return null;
		}
		GameObjectPool.AsyncItem asyncItem = new GameObjectPool.AsyncItem();
		asyncItem.item = poolItem;
		asyncItem.callback = _callback;
		asyncItem.userData = _userData;
		asyncItem.async = Object.InstantiateAsync<GameObject>(gameObject, _count);
		this.asyncItems.Add(asyncItem);
		return asyncItem;
	}

	public void CancelAsync(GameObjectPool.AsyncItem _ai)
	{
		if (this.asyncItems.Remove(_ai))
		{
			if (_ai.async.isDone)
			{
				Object[] result = _ai.async.Result;
				for (int i = 0; i < result.Length; i++)
				{
					Object.Destroy(result[i]);
				}
				return;
			}
			_ai.async.Cancel();
		}
	}

	public void PoolObjectAsync(GameObject obj)
	{
		this.PoolObject(obj);
	}

	public void PoolObject(GameObject obj)
	{
		if (!obj)
		{
			return;
		}
		string name = obj.name;
		GameObjectPool.PoolItem poolItem;
		if (!this.pool.TryGetValue(name, out poolItem))
		{
			return;
		}
		List<GameObject> objs = poolItem.objs;
		if (objs.Count < this.maxPooledInstancesPerItem)
		{
			obj.SetActive(false);
			obj.transform.SetParent(null, false);
			objs.Add(obj);
			if (objs.Count >= 1 && !this.activePool.Contains(poolItem))
			{
				this.activePool.Add(poolItem);
				return;
			}
		}
		else
		{
			poolItem.activeCount--;
			obj.SetActive(false);
			this.DestroyObject(obj);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void DestroyObject(GameObject obj)
	{
		obj.GetComponentsInChildren<Renderer>(this.tempRenderers);
		Utils.CleanupMaterialsOfRenderers<List<Renderer>>(this.tempRenderers);
		this.tempRenderers.Clear();
		Object.Destroy(obj);
	}

	public void CmdList(string _mode)
	{
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Pool objects:");
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		List<GameObject> list = new List<GameObject>();
		List<GameObject> list2 = new List<GameObject>();
		foreach (KeyValuePair<string, GameObjectPool.PoolItem> keyValuePair in this.pool)
		{
			GameObjectPool.PoolItem value = keyValuePair.Value;
			if (value.prefab != null)
			{
				list.Add(value.prefab);
			}
			num += value.activeCount;
			num2 += value.objs.Count;
			if (value.activeCount > 0)
			{
				num3++;
				list2.Add(value.prefab);
			}
			if (_mode == "all" || (_mode == "active" && value.activeCount > 0))
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output(" {0}, prefab {1}, active {2}, count {3}", new object[]
				{
					value.name,
					value.prefab ? "1" : "0",
					value.activeCount,
					value.objs.Count
				});
			}
		}
		string text = string.Format(" types {0}, used {1}, pooled {2}, active {3}", new object[]
		{
			this.pool.Count,
			num3,
			num2,
			num
		});
		if (Application.isEditor)
		{
			long num4;
			long num5;
			ProfilerUtils.CalculateDependentBytes(list.ToArray(), out num4, out num5);
			long num6;
			long num7;
			ProfilerUtils.CalculateDependentBytes(list2.ToArray(), out num6, out num7);
			text += string.Format(", used mesh {0:F2} MB, used texture {1:F2} MB, required mesh {2:F2} MB, required texture {3:F2} MB", new object[]
			{
				(double)num4 * 9.5367431640625E-07,
				(double)num5 * 9.5367431640625E-07,
				(double)num6 * 9.5367431640625E-07,
				(double)num7 * 9.5367431640625E-07
			});
		}
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output(text);
	}

	public void CmdShrink()
	{
		bool flag;
		do
		{
			flag = false;
			for (int i = this.activePool.Count - 1; i >= 0; i--)
			{
				GameObjectPool.PoolItem poolItem = this.activePool[i];
				if (poolItem.objs.Count > 0)
				{
					poolItem.updateTime = 0f;
					this.FrameUpdate();
					flag = true;
					break;
				}
			}
		}
		while (flag);
	}

	[Conditional("DEBUG_GOPOOL_PROFILE")]
	[PublicizedFrom(EAccessModifier.Private)]
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void ProfilerBegin(string _name)
	{
	}

	[Conditional("DEBUG_GOPOOL_PROFILE")]
	[PublicizedFrom(EAccessModifier.Private)]
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void ProfilerEnd()
	{
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cActivePoolAddAtCount = 1;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cActivePoolMinCount = 0;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cActivePoolRemoveDelay = 10f;

	[PublicizedFrom(EAccessModifier.Private)]
	public static GameObjectPool instance;

	[PublicizedFrom(EAccessModifier.Private)]
	public Shader tintMaskShader;

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<string, GameObjectPool.PoolItem> pool = new Dictionary<string, GameObjectPool.PoolItem>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<GameObjectPool.PoolItem> activePool = new List<GameObjectPool.PoolItem>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<GameObjectPool.AsyncItem> asyncItems = new List<GameObjectPool.AsyncItem>();

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cAsyncPoolObjsCount = 128;

	[PublicizedFrom(EAccessModifier.Private)]
	public GameObject[] asyncPoolObjs = new GameObject[128];

	[PublicizedFrom(EAccessModifier.Private)]
	public List<Renderer> tempRenderers = new List<Renderer>();

	[PublicizedFrom(EAccessModifier.Private)]
	public int maxPooledInstancesPerItem = 200;

	[PublicizedFrom(EAccessModifier.Private)]
	public int maxDestroysPerUpdate = 1;

	[PublicizedFrom(EAccessModifier.Private)]
	public GameObjectPool.ShrinkThreshold shrinkThresholdHigh = new GameObjectPool.ShrinkThreshold(100, 1, 0.1f);

	[PublicizedFrom(EAccessModifier.Private)]
	public GameObjectPool.ShrinkThreshold shrinkThresholdMedium = new GameObjectPool.ShrinkThreshold(40, 1, 0.5f);

	[PublicizedFrom(EAccessModifier.Private)]
	public GameObjectPool.ShrinkThreshold shrinkThresholdLow = new GameObjectPool.ShrinkThreshold(12, 1, 3f);

	[PublicizedFrom(EAccessModifier.Private)]
	public GameObjectPool.ShrinkThreshold shrinkThresholdMin = new GameObjectPool.ShrinkThreshold(0, 1, 10f);

	public delegate Transform LoadCallback();

	public delegate void CreateCallback(GameObject obj);

	public delegate void CreateAsyncCallback(object _userData, Object[] _objs, int _objsCount, bool _isAsync);

	public class PoolItem
	{
		public GameObject Instantiate()
		{
			this.activeCount++;
			GameObject gameObject = Object.Instantiate<GameObject>(this.prefab);
			gameObject.name = this.name;
			if (this.createOnceToAllCallback != null)
			{
				this.createOnceToAllCallback(gameObject);
				this.createOnceToAllCallback = null;
			}
			if (this.createCallback != null)
			{
				this.createCallback(gameObject);
			}
			return gameObject;
		}

		public string name;

		public GameObject prefab;

		public GameObjectPool.LoadCallback loadCallback;

		public GameObjectPool.CreateCallback createOnceToAllCallback;

		public GameObjectPool.CreateCallback createCallback;

		public List<GameObject> objs;

		public float updateTime;

		public int activeCount;

		public Color originalTint;
	}

	public class AsyncItem
	{
		public GameObjectPool.PoolItem item;

		public GameObjectPool.CreateAsyncCallback callback;

		public AsyncInstantiateOperation async;

		public object userData;
	}

	public struct ShrinkThreshold
	{
		public ShrinkThreshold(int count, int destroyCount, float delay)
		{
			this.Count = count;
			this.DestroyCount = destroyCount;
			this.Delay = delay;
		}

		public override string ToString()
		{
			return string.Format("({0} = {1}, {2} = {3}, {4} = {5:F2}s)", new object[] { "Count", this.Count, "DestroyCount", this.DestroyCount, "Delay", this.Delay });
		}

		public int Count;

		public int DestroyCount;

		public float Delay;
	}
}
