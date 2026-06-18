using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class NavObjectManager
{
	public static NavObjectManager Instance
	{
		get
		{
			if (NavObjectManager.instance == null)
			{
				NavObjectManager.instance = new NavObjectManager();
			}
			return NavObjectManager.instance;
		}
	}

	public static bool HasInstance
	{
		get
		{
			return NavObjectManager.instance != null;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public NavObjectManager()
	{
		NavObjectManager.instance = this;
	}

	public void Cleanup()
	{
		NavObjectManager.instance = null;
		this.NavObjectList.Clear();
		this.removedNavObjectPool.Clear();
		this.tagList.Clear();
	}

	public event NavObjectManager.NavObjectChangedDelegate OnNavObjectAdded;

	public event NavObjectManager.NavObjectChangedDelegate OnNavObjectRemoved;

	public event NavObjectManager.NavObjectChangedDelegate OnNavObjectRefreshed;

	public NavObject RegisterNavObject(string className, Transform trackedTransform, string overrideSprite = "", bool hiddenOnCompass = false)
	{
		for (int i = 0; i < this.NavObjectList.Count; i++)
		{
			if (this.NavObjectList[i].IsTrackedTransform(trackedTransform))
			{
				return this.NavObjectList[i];
			}
		}
		NavObject navObject;
		if (this.removedNavObjectPool.Count > 0)
		{
			navObject = this.removedNavObjectPool[0];
			this.removedNavObjectPool.RemoveAt(0);
			navObject.IsActive = true;
			navObject.Reset(className);
		}
		else
		{
			navObject = new NavObject(className);
		}
		navObject.hiddenOnCompass = hiddenOnCompass;
		navObject.TrackedTransform = trackedTransform;
		navObject.OverrideSpriteName = overrideSprite;
		this.AddNavObjectTag(navObject);
		if (this.OnNavObjectAdded != null)
		{
			this.OnNavObjectAdded(navObject);
		}
		this.NavObjectList.Add(navObject);
		EntityPlayerLocal primaryPlayer = GameManager.Instance.World.GetPrimaryPlayer();
		if (primaryPlayer != null)
		{
			navObject.HandleActiveNavClass(primaryPlayer);
		}
		return navObject;
	}

	public NavObject RegisterNavObject(string className, Vector3 trackedPosition, string overrideSprite = "", bool hiddenOnCompass = false, int entityId = -1, Entity ownerEntity = null)
	{
		for (int i = 0; i < this.NavObjectList.Count; i++)
		{
			if (this.NavObjectList[i].NavObjectClass != null && !(className != this.NavObjectList[i].NavObjectClass.NavObjectClassName) && (!(ownerEntity != null) || !(ownerEntity != this.NavObjectList[i].OwnerEntity)))
			{
				if (entityId != -1 && this.NavObjectList[i].EntityID == entityId)
				{
					return this.NavObjectList[i];
				}
				if (this.NavObjectList[i].IsTrackedPosition(trackedPosition))
				{
					return this.NavObjectList[i];
				}
			}
		}
		NavObject navObject;
		if (this.removedNavObjectPool.Count > 0)
		{
			navObject = this.removedNavObjectPool[0];
			this.removedNavObjectPool.RemoveAt(0);
			navObject.IsActive = true;
			navObject.Reset(className);
		}
		else
		{
			navObject = new NavObject(className);
		}
		navObject.EntityID = entityId;
		navObject.OwnerEntity = ownerEntity;
		navObject.hiddenOnCompass = hiddenOnCompass;
		navObject.TrackedPosition = trackedPosition;
		navObject.OverrideSpriteName = overrideSprite;
		this.AddNavObjectTag(navObject);
		if (this.OnNavObjectAdded != null)
		{
			this.OnNavObjectAdded(navObject);
		}
		this.NavObjectList.Add(navObject);
		EntityPlayerLocal primaryPlayer = GameManager.Instance.World.GetPrimaryPlayer();
		if (primaryPlayer != null)
		{
			navObject.HandleActiveNavClass(primaryPlayer);
		}
		return navObject;
	}

	public NavObject RegisterNavObject(string className, Entity trackedEntity, string overrideSprite = "", bool hiddenOnCompass = false)
	{
		for (int i = 0; i < this.NavObjectList.Count; i++)
		{
			if (this.NavObjectList[i].IsTrackedEntity(trackedEntity) && (this.NavObjectList[i].NavObjectClass == null || this.NavObjectList[i].NavObjectClass.NavObjectClassName == className))
			{
				this.NavObjectList[i].TrackedEntity = trackedEntity;
				return this.NavObjectList[i];
			}
		}
		NavObject navObject;
		if (this.removedNavObjectPool.Count > 0)
		{
			navObject = this.removedNavObjectPool[0];
			this.removedNavObjectPool.RemoveAt(0);
			navObject.IsActive = true;
			navObject.Reset(className);
		}
		else
		{
			navObject = new NavObject(className);
		}
		navObject.OverrideSpriteName = overrideSprite;
		navObject.TrackedEntity = trackedEntity;
		navObject.hiddenOnCompass = hiddenOnCompass;
		this.AddNavObjectTag(navObject);
		if (this.OnNavObjectAdded != null)
		{
			this.OnNavObjectAdded(navObject);
		}
		this.NavObjectList.Add(navObject);
		EntityPlayerLocal primaryPlayer = GameManager.Instance.World.GetPrimaryPlayer();
		if (primaryPlayer != null)
		{
			navObject.HandleActiveNavClass(primaryPlayer);
		}
		return navObject;
	}

	public void UnRegisterNavObject(NavObject navObject)
	{
		if (navObject == null)
		{
			return;
		}
		bool flag = false;
		if (this.NavObjectList.Contains(navObject))
		{
			flag = true;
			this.NavObjectList.Remove(navObject);
		}
		if (!this.removedNavObjectPool.Contains(navObject))
		{
			this.removedNavObjectPool.Add(navObject);
		}
		if (flag)
		{
			this.RemoveNavObjectTag(navObject);
			if (this.OnNavObjectRemoved != null)
			{
				this.OnNavObjectRemoved(navObject);
			}
		}
	}

	public void UnRegisterNavObjectByPosition(Vector3 position, string navObjectClass)
	{
		this.unRegisterNavObjects((NavObject navObject) => navObject.IsTrackedPosition(position) && navObject.NavObjectClass != null && navObject.NavObjectClass.NavObjectClassName == navObjectClass, string.Format("UnRegisterNavObjectByOwnerEntity {0}", position));
	}

	public void UnRegisterNavObjectByOwnerEntity(Entity ownerEntity, string navObjectClass)
	{
		this.unRegisterNavObjects((NavObject navObject) => navObject.OwnerEntity == ownerEntity && navObject.NavObjectClass != null && navObject.NavObjectClass.NavObjectClassName == navObjectClass, string.Format("\"UnRegisterNavObjectByOwnerEntity {0}", ownerEntity));
	}

	public void UnRegisterNavObjectByEntityID(int entityId)
	{
		this.unRegisterNavObjects((NavObject navObject) => navObject.EntityID == entityId, string.Format("UnRegisterNavObjectByEntityID {0}", entityId));
	}

	public void UnRegisterNavObjectByClass(string className)
	{
		this.unRegisterNavObjects((NavObject navObject) => navObject.NavObjectClass != null && navObject.NavObjectClass.NavObjectClassName == className, string.Format("UnRegisterNavObjectByClass {0}", className));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void unRegisterNavObjects(NavObjectManager.includeInList includeTest, string logStringPrefix)
	{
		foreach (NavObject navObject in this.NavObjectList)
		{
			if (includeTest(navObject))
			{
				this.removeList.Add(navObject);
			}
		}
		foreach (NavObject navObject2 in this.removeList)
		{
			this.NavObjectList.Remove(navObject2);
			this.RemoveNavObjectTag(navObject2);
			if (!this.removedNavObjectPool.Contains(navObject2))
			{
				this.removedNavObjectPool.Add(navObject2);
			}
			NavObjectManager.NavObjectChangedDelegate onNavObjectRemoved = this.OnNavObjectRemoved;
			if (onNavObjectRemoved != null)
			{
				onNavObjectRemoved(navObject2);
			}
		}
		this.removeList.Clear();
	}

	public void AddNavObjectTag(NavObject navObject)
	{
		for (int i = 0; i < navObject.NavObjectClassList.Count; i++)
		{
			if (!string.IsNullOrEmpty(navObject.NavObjectClassList[i].Tag))
			{
				if (!this.tagList.ContainsKey(navObject.NavObjectClassList[i].Tag))
				{
					this.tagList.Add(navObject.NavObjectClassList[i].Tag, new List<NavObject>());
				}
				this.tagList[navObject.NavObjectClassList[i].Tag].Add(navObject);
			}
		}
	}

	public void RemoveNavObjectTag(NavObject navObject)
	{
		for (int i = 0; i < navObject.NavObjectClassList.Count; i++)
		{
			if (!string.IsNullOrEmpty(navObject.NavObjectClassList[i].Tag))
			{
				if (!this.tagList.ContainsKey(navObject.NavObjectClassList[i].Tag))
				{
					this.tagList.Add(navObject.NavObjectClassList[i].Tag, new List<NavObject>());
				}
				this.tagList[navObject.NavObjectClassList[i].Tag].Remove(navObject);
			}
		}
	}

	public bool HasNavObjectTag(string tag)
	{
		if (this.tagList.ContainsKey(tag))
		{
			for (int i = 0; i < this.tagList[tag].Count; i++)
			{
				if (this.tagList[tag][i].NavObjectClass != null && this.tagList[tag][i].NavObjectClass.Tag == tag)
				{
					return true;
				}
			}
		}
		return false;
	}

	public NavObject GetNavObjectByEntityID(int entityId)
	{
		for (int i = this.NavObjectList.Count - 1; i >= 0; i--)
		{
			NavObject navObject = this.NavObjectList[i];
			if (navObject != null && navObject.EntityID == entityId)
			{
				return this.NavObjectList[i];
			}
		}
		return null;
	}

	public void RefreshNavObjects()
	{
		for (int i = 0; i < this.NavObjectList.Count; i++)
		{
			bool flag = false;
			if (this.NavObjectList[i].NavObjectClass != null)
			{
				this.NavObjectList[i].NavObjectClass = NavObjectClass.GetNavObjectClass(this.NavObjectList[i].NavObjectClass.NavObjectClassName);
				flag = true;
			}
			if (this.NavObjectList[i].NavObjectClassList != null)
			{
				for (int j = 0; j < this.NavObjectList[i].NavObjectClassList.Count; j++)
				{
					this.NavObjectList[i].NavObjectClassList[j] = NavObjectClass.GetNavObjectClass(this.NavObjectList[i].NavObjectClassList[j].NavObjectClassName);
					flag = true;
				}
			}
			if (flag && this.OnNavObjectRefreshed != null)
			{
				this.OnNavObjectRefreshed(this.NavObjectList[i]);
			}
		}
	}

	public void Update()
	{
		if (GameManager.Instance.World == null)
		{
			return;
		}
		EntityPlayerLocal primaryPlayer = GameManager.Instance.World.GetPrimaryPlayer();
		for (int i = this.NavObjectList.Count - 1; i >= 0; i--)
		{
			NavObject navObject = this.NavObjectList[i];
			if (!navObject.IsValid())
			{
				this.UnRegisterNavObject(navObject);
			}
			else if (primaryPlayer != null)
			{
				navObject.HandleActiveNavClass(primaryPlayer);
			}
		}
	}

	[Conditional("DEBUG_NAV")]
	public static void LogNav(string _format = "", params object[] _args)
	{
		_format = string.Format("{0} Nav {1}", GameManager.frameCount, _format);
		Log.Out(_format, _args);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static NavObjectManager instance;

	public List<NavObject> NavObjectList = new List<NavObject>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<NavObject> removedNavObjectPool = new List<NavObject>();

	public Dictionary<string, List<NavObject>> tagList = new Dictionary<string, List<NavObject>>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<NavObject> removeList = new List<NavObject>();

	public delegate void NavObjectChangedDelegate(NavObject newNavObject);

	[PublicizedFrom(EAccessModifier.Private)]
	public delegate bool includeInList(NavObject navObject);
}
