using System;
using System.Collections.Generic;
using UnityEngine;

public class MapObjectManager
{
	public event MapObjectManager.MapObjectListChangedDelegate ChangedDelegates;

	public static void Reset()
	{
		MapObjectManager.entityList.Clear();
	}

	public MapObjectManager()
	{
		for (int i = 0; i < 17; i++)
		{
			this.mapObjects.Add(new DictionaryList<int, MapObject>());
		}
		foreach (MapObject mapObject in MapObjectManager.entityList)
		{
			if (mapObject is MapObjectVehicle)
			{
				this.Add(new MapObjectVehicle(mapObject as MapObjectVehicle));
			}
			else
			{
				this.Add(new MapObject(mapObject));
			}
		}
	}

	public static void ClearEntityList()
	{
		MapObjectManager.entityList.Clear();
	}

	public void Add(MapObject _mapObject)
	{
		if (this.mapObjects[(int)_mapObject.type].dict.ContainsKey((int)_mapObject.key))
		{
			this.Remove(_mapObject.type, (int)_mapObject.key);
		}
		this.mapObjects[(int)_mapObject.type].Add((int)_mapObject.key, _mapObject);
		if (this.ChangedDelegates != null)
		{
			this.ChangedDelegates(_mapObject.type, _mapObject, true);
		}
		if (_mapObject.type == EnumMapObjectType.Entity && !MapObjectManager.entityList.Contains(_mapObject))
		{
			MapObjectManager.entityList.Add(_mapObject);
		}
	}

	public void Remove(EnumMapObjectType _type, int _key)
	{
		if (this.mapObjects[(int)_type].dict.ContainsKey(_key))
		{
			MapObject mapObject = this.mapObjects[(int)_type].dict[_key];
			if (mapObject.type == EnumMapObjectType.Entity && MapObjectManager.entityList.Contains(mapObject))
			{
				MapObjectManager.entityList.Remove(mapObject);
			}
			this.mapObjects[(int)_type].Remove(_key);
			if (this.ChangedDelegates != null)
			{
				this.ChangedDelegates(_type, mapObject, false);
			}
		}
	}

	public void RemoveByPosition(EnumMapObjectType _type, Vector3 _position)
	{
		for (int i = this.mapObjects[(int)_type].list.Count - 1; i >= 0; i--)
		{
			Vector3 position = this.mapObjects[(int)_type].list[i].GetPosition();
			if (position.x == _position.x && position.z == _position.z)
			{
				this.mapObjects[(int)_type].list.RemoveAt(i);
			}
		}
	}

	public void RemoveByType(EnumMapObjectType _type)
	{
		for (int i = this.mapObjects[(int)_type].list.Count - 1; i >= 0; i--)
		{
			if (this.mapObjects[(int)_type].list[i].type == _type)
			{
				this.mapObjects[(int)_type].list.RemoveAt(i);
			}
		}
	}

	public void Clear()
	{
		this.mapObjects.Clear();
	}

	public List<MapObject> GetList(EnumMapObjectType _type)
	{
		return this.mapObjects[(int)_type].list;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public List<DictionaryList<int, MapObject>> mapObjects = new List<DictionaryList<int, MapObject>>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static List<MapObject> entityList = new List<MapObject>();

	public delegate void MapObjectListChangedDelegate(EnumMapObjectType _type, MapObject _mapObject, bool _bAdded);
}
