using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class DecoChunk
{
	public DecoChunk(int _x, int _z, int _drawX, int _drawZ)
	{
		this.Reset(_x, _z, _drawX, _drawZ);
	}

	public void Reset(int _x, int _z, int _drawX, int _drawZ)
	{
		this.decoChunkX = _x;
		this.decoChunkZ = _z;
		this.drawX = _drawX;
		this.drawZ = _drawZ;
		this.decosPerSmallChunks.Clear();
		this.isDecorated = false;
		this.isModelsUpdated = false;
		this.isGameObjectUpdated = false;
	}

	public void RestoreGeneratedDecos(Predicate<DecoObject> decoObjectValidator = null)
	{
		foreach (long num in this.decosPerSmallChunks.Keys)
		{
			this.RestoreGeneratedDecos(num, decoObjectValidator);
		}
	}

	public void RestoreGeneratedDecos(long smallChunkKey, Predicate<DecoObject> decoObjectValidator = null)
	{
		List<DecoObject> list;
		if (this.decosPerSmallChunks.TryGetValue(smallChunkKey, out list))
		{
			for (int i = list.Count - 1; i >= 0; i--)
			{
				DecoObject decoObject = list[i];
				if (decoObjectValidator == null || decoObjectValidator(decoObject))
				{
					switch (decoObject.state)
					{
					case DecoState.GeneratedInactive:
						decoObject.state = DecoState.GeneratedActive;
						this.isModelsUpdated = false;
						break;
					case DecoState.Dynamic:
						this.RemoveDecoObject(decoObject);
						break;
					}
				}
			}
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int MakeKey16(int _x, int _z)
	{
		return (_x << 16) | (_z & 65535);
	}

	public static int ToDecoChunkPos(float _worldPos)
	{
		return Utils.Fastfloor(_worldPos / 128f);
	}

	public static int ToDecoChunkPos(int _worldPos)
	{
		if (_worldPos >= 0)
		{
			return _worldPos / 128;
		}
		return (_worldPos - 128 + 1) / 128;
	}

	public void UpdateGameObject()
	{
		if (!this.rootObj)
		{
			this.rootObj = new GameObject();
		}
		this.SetVisible(true);
		this.rootObj.name = "DecoC_" + this.decoChunkX.ToString() + "_" + this.decoChunkZ.ToString();
		this.rootObj.transform.position = new Vector3((float)(this.drawX * 128), 0f, (float)(this.drawZ * 128)) - Origin.position;
		this.isGameObjectUpdated = true;
	}

	public IEnumerator UpdateModels(World _world, MicroStopwatch ms)
	{
		this.SetVisible(true);
		foreach (KeyValuePair<long, List<DecoObject>> keyValuePair in this.decosPerSmallChunks)
		{
			List<DecoObject> value = keyValuePair.Value;
			for (int i = 0; i < value.Count; i++)
			{
				DecoObject decoObject = value[i];
				if (decoObject.state != DecoState.GeneratedInactive && !decoObject.go && decoObject.asyncItem == null)
				{
					string modelName = decoObject.GetModelName();
					List<DecoObject> list;
					if (!this.models.TryGetValue(modelName, out list))
					{
						list = new List<DecoObject>();
						this.models.Add(modelName, list);
					}
					list.Add(decoObject);
				}
			}
		}
		foreach (KeyValuePair<string, List<DecoObject>> keyValuePair2 in this.models)
		{
			List<DecoObject> value2 = keyValuePair2.Value;
			GameObjectPool.AsyncItem objectsForTypeAsync = GameObjectPool.Instance.GetObjectsForTypeAsync(keyValuePair2.Key, value2.Count, new GameObjectPool.CreateAsyncCallback(this.CreateGameObjectCallback), value2);
			if (objectsForTypeAsync != null)
			{
				this.asyncItems.Add(objectsForTypeAsync);
				for (int j = 0; j < value2.Count; j++)
				{
					value2[j].asyncItem = objectsForTypeAsync;
				}
			}
			if (ms.ElapsedMicroseconds > 900L)
			{
				yield return null;
				ms.ResetAndRestart();
			}
		}
		Dictionary<string, List<DecoObject>>.Enumerator enumerator2 = default(Dictionary<string, List<DecoObject>>.Enumerator);
		this.models.Clear();
		this.isModelsUpdated = true;
		yield break;
		yield break;
	}

	public void CreateGameObjectCallback(object _userData, Object[] _objs, int _objsCount, bool _isAsync)
	{
		List<DecoObject> list = (List<DecoObject>)_userData;
		Transform transform = this.rootObj.transform;
		for (int i = 0; i < _objsCount; i++)
		{
			GameObject gameObject = (GameObject)_objs[i];
			list[i].CreateGameObjectCallback(gameObject, transform, _isAsync);
			this.occlusionTs.Add(gameObject.transform);
		}
		if (this.occlusionTs.Count > 0)
		{
			if (OcclusionManager.Instance.cullDecorations)
			{
				OcclusionManager.Instance.AddDeco(this, this.occlusionTs);
			}
			this.occlusionTs.Clear();
		}
	}

	public void AddDecoObject(DecoObject _decoObject, bool _tryInstantiate = false)
	{
		long num = WorldChunkCache.MakeChunkKey(World.toChunkXZ(_decoObject.pos.x), World.toChunkXZ(_decoObject.pos.z));
		List<DecoObject> list;
		if (!this.decosPerSmallChunks.TryGetValue(num, out list))
		{
			list = new List<DecoObject>(64);
			this.decosPerSmallChunks.Add(num, list);
		}
		list.Add(_decoObject);
		if (_tryInstantiate)
		{
			if (ThreadManager.IsMainThread() && this.rootObj)
			{
				_decoObject.CreateGameObject(this, this.rootObj.transform);
				if (OcclusionManager.Instance.cullDecorations && _decoObject.go)
				{
					this.occlusionTs.Add(_decoObject.go.transform);
					OcclusionManager.Instance.AddDeco(this, this.occlusionTs);
					this.occlusionTs.Clear();
					return;
				}
			}
			else
			{
				this.isModelsUpdated = false;
			}
		}
	}

	public DecoObject GetDecoObjectAt(Vector3i _worldBlockPos)
	{
		long num = WorldChunkCache.MakeChunkKey(World.toChunkXZ(_worldBlockPos.x), World.toChunkXZ(_worldBlockPos.z));
		List<DecoObject> list;
		if (!this.decosPerSmallChunks.TryGetValue(num, out list))
		{
			return null;
		}
		foreach (DecoObject decoObject in list)
		{
			if (decoObject.pos.x == _worldBlockPos.x && decoObject.pos.z == _worldBlockPos.z && decoObject.state != DecoState.GeneratedInactive)
			{
				return decoObject;
			}
		}
		return null;
	}

	public bool RemoveDecoObject(Vector3i _worldBlockPos)
	{
		DecoObject decoObjectAt = this.GetDecoObjectAt(_worldBlockPos);
		if (decoObjectAt == null)
		{
			return false;
		}
		this.RemoveDecoObject(decoObjectAt);
		return true;
	}

	public void RemoveDecoObject(DecoObject deco)
	{
		if (deco.state == DecoState.Dynamic)
		{
			long num = WorldChunkCache.MakeChunkKey(World.toChunkXZ(deco.pos.x), World.toChunkXZ(deco.pos.z));
			List<DecoObject> list;
			if (this.decosPerSmallChunks.TryGetValue(num, out list))
			{
				list.Remove(deco);
			}
		}
		else
		{
			deco.state = DecoState.GeneratedInactive;
		}
		if (OcclusionManager.Instance.cullDecorations && deco.go)
		{
			OcclusionManager.Instance.RemoveDeco(this, deco.go.transform);
		}
		deco.Destroy();
	}

	public void ClearDecoObjectsInArea(Vector3i _boundsPosition, Vector3i _boundsSize)
	{
		Vector3i vector3i = _boundsPosition + _boundsSize;
		int num = World.toChunkXZ(_boundsPosition.x);
		int num2 = World.toChunkXZ(_boundsPosition.z);
		int num3 = World.toChunkXZ(vector3i.x);
		int num4 = World.toChunkXZ(vector3i.z);
		for (int i = num; i <= num3; i++)
		{
			for (int j = num2; j <= num4; j++)
			{
				long num5 = WorldChunkCache.MakeChunkKey(i, j);
				List<DecoObject> list;
				if (this.decosPerSmallChunks.TryGetValue(num5, out list))
				{
					for (int k = list.Count - 1; k >= 0; k--)
					{
						DecoObject decoObject = list[k];
						Vector3i pos = decoObject.pos;
						if (pos.x >= _boundsPosition.x && pos.x < vector3i.x && pos.y >= _boundsPosition.y && pos.y < vector3i.y && pos.z >= _boundsPosition.z && pos.z < vector3i.z)
						{
							if (OcclusionManager.Instance.cullDecorations && decoObject.go)
							{
								OcclusionManager.Instance.RemoveDeco(this, decoObject.go.transform);
							}
							decoObject.Destroy();
							list.RemoveAt(k);
						}
					}
					if (list.Count == 0)
					{
						this.decosPerSmallChunks.Remove(num5);
					}
				}
			}
		}
	}

	public void Destroy()
	{
		if (OcclusionManager.Instance.cullDecorations)
		{
			OcclusionManager.Instance.RemoveDecoChunk(this);
		}
		foreach (KeyValuePair<long, List<DecoObject>> keyValuePair in this.decosPerSmallChunks)
		{
			List<DecoObject> value = keyValuePair.Value;
			for (int i = 0; i < value.Count; i++)
			{
				value[i].Destroy();
			}
		}
		for (int j = 0; j < this.asyncItems.Count; j++)
		{
			GameObjectPool.Instance.CancelAsync(this.asyncItems[j]);
		}
		this.asyncItems.Clear();
		this.isModelsUpdated = false;
		this.isGameObjectUpdated = false;
		Object.Destroy(this.rootObj);
	}

	public void SetVisible(bool _bVisible)
	{
		if (this.rootObj && this.rootObj.activeSelf != _bVisible)
		{
			this.rootObj.SetActive(_bVisible);
		}
	}

	public override string ToString()
	{
		return string.Format("DecoChunk {0},{1}", this.decoChunkX, this.decoChunkZ);
	}

	public int decoChunkX;

	public int decoChunkZ;

	public int drawX;

	public int drawZ;

	public bool isDecorated;

	public bool isModelsUpdated;

	public bool isGameObjectUpdated;

	public GameObject rootObj;

	public Dictionary<long, List<DecoObject>> decosPerSmallChunks = new Dictionary<long, List<DecoObject>>(64);

	public OcclusionManager.OccludeeZone occludeeZone;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<Transform> occlusionTs = new List<Transform>();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<GameObjectPool.AsyncItem> asyncItems = new List<GameObjectPool.AsyncItem>();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<string, List<DecoObject>> models = new Dictionary<string, List<DecoObject>>();
}
