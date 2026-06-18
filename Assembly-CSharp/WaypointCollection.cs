using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using UnityEngine;

public class WaypointCollection
{
	public void Read(BinaryReader _br)
	{
		this.Collection.Clear();
		int num = (int)_br.ReadByte();
		int num2 = (int)_br.ReadUInt16();
		for (int i = 0; i < num2; i++)
		{
			Waypoint waypoint = new Waypoint();
			waypoint.Read(_br, num);
			this.Collection.Add(waypoint);
		}
	}

	public void Write(BinaryWriter _bw)
	{
		_bw.Write(7);
		int num = 0;
		for (int i = 0; i < this.Collection.list.Count; i++)
		{
			if (this.Collection.list[i].IsSaved)
			{
				num++;
			}
		}
		_bw.Write((ushort)num);
		for (int j = 0; j < this.Collection.list.Count; j++)
		{
			if (this.Collection.list[j].IsSaved)
			{
				this.Collection.list[j].Write(_bw);
			}
		}
	}

	public WaypointCollection Clone()
	{
		WaypointCollection waypointCollection = new WaypointCollection();
		for (int i = 0; i < this.Collection.list.Count; i++)
		{
			waypointCollection.Collection.Add(this.Collection.list[i].Clone());
		}
		return waypointCollection;
	}

	public bool ContainsWaypoint(Waypoint _wp)
	{
		return this.Collection.hashSet.Contains(_wp);
	}

	public bool ContainsLastKnownPositionWaypoint(int _entityId)
	{
		using (List<Waypoint>.Enumerator enumerator = this.Collection.list.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				if (enumerator.Current.lastKnownPositionEntityId == _entityId)
				{
					return true;
				}
			}
		}
		return false;
	}

	public Waypoint GetLastKnownPositionWaypoint(int _entityID)
	{
		foreach (Waypoint waypoint in this.Collection.list)
		{
			if (waypoint.lastKnownPositionEntityId == _entityID)
			{
				return waypoint;
			}
		}
		return null;
	}

	public void UpdateEntityVehicleWayPoint(EntityVehicle vehicle, bool unloaded = false)
	{
		if (!vehicle.LocalPlayerIsOwner())
		{
			return;
		}
		Waypoint waypoint = null;
		foreach (Waypoint waypoint2 in this.Collection.list)
		{
			if (waypoint2.lastKnownPositionEntityId == vehicle.entityId)
			{
				waypoint = waypoint2;
				break;
			}
		}
		if (waypoint != null)
		{
			Vector3i vector3i = Vector3i.FromVector3Rounded(vehicle.position);
			if (waypoint.pos != vector3i)
			{
				this.Collection.Remove(waypoint);
				waypoint.pos = vector3i;
				waypoint.navObject.TrackedPosition = vehicle.position;
				this.Collection.Add(waypoint);
			}
			waypoint.navObject.hiddenOnCompass = !unloaded;
			this.SetWaypointHiddenOnMap(vehicle.entityId, !unloaded);
			return;
		}
		((XUiC_MapArea)GameManager.Instance.World.GetPrimaryPlayer().PlayerUI.xui.GetWindow("mapArea").Controller).RefreshVehiclePositionWaypoint(vehicle, false);
	}

	public void UpdateEntityAnimalWayPoint(EntityAnimal _animal, bool unloaded = false)
	{
		Waypoint waypoint = null;
		foreach (Waypoint waypoint2 in this.Collection.list)
		{
			if (waypoint2.lastKnownPositionEntityId == _animal.entityId)
			{
				waypoint = waypoint2;
				break;
			}
		}
		if (waypoint != null)
		{
			Vector3i vector3i = Vector3i.FromVector3Rounded(_animal.position);
			if (waypoint.pos != vector3i)
			{
				this.Collection.Remove(waypoint);
				waypoint.pos = vector3i;
				waypoint.navObject.TrackedPosition = _animal.position;
				this.Collection.Add(waypoint);
			}
			waypoint.navObject.hiddenOnCompass = !unloaded;
			this.SetWaypointHiddenOnMap(_animal.entityId, !unloaded);
			return;
		}
		((XUiC_MapArea)GameManager.Instance.World.GetPrimaryPlayer().PlayerUI.xui.GetWindow("mapArea").Controller).RefreshAnimalPositionWaypoint(_animal, false);
	}

	public void SetEntityVehicleWaypointFromVehicleManager([TupleElementNames(new string[] { "entityId", "position" })] List<ValueTuple<int, Vector3>> _positions)
	{
		List<Waypoint> list = new List<Waypoint>();
		for (int i = 0; i < this.Collection.list.Count; i++)
		{
			if (this.Collection.list[i].lastKnownPositionEntityId != -1 && this.Collection.list[i].lastKnownPositionEntityType == eLastKnownPositionEntityType.Vehicle)
			{
				Waypoint waypoint = this.Collection.list[i];
				bool flag = false;
				int j = 0;
				while (j < _positions.Count)
				{
					if (waypoint.lastKnownPositionEntityId == _positions[j].Item1)
					{
						flag = true;
						Vector3i vector3i = Vector3i.FromVector3Rounded(_positions[j].Item2);
						if (waypoint.pos != vector3i)
						{
							this.Collection.Remove(waypoint);
							waypoint.pos = vector3i;
							waypoint.navObject.TrackedPosition = _positions[j].Item2;
							this.Collection.Add(waypoint);
							break;
						}
						break;
					}
					else
					{
						j++;
					}
				}
				if (!flag)
				{
					list.Add(waypoint);
				}
			}
		}
		foreach (Waypoint waypoint2 in list)
		{
			this.Collection.Remove(waypoint2);
			NavObjectManager.Instance.UnRegisterNavObject(waypoint2.navObject);
		}
	}

	public void SetDroneWaypointsFromDroneManager([TupleElementNames(new string[] { "entityId", "position" })] List<ValueTuple<int, Vector3>> _drones)
	{
		for (int i = 0; i < _drones.Count; i++)
		{
			Waypoint waypoint = null;
			foreach (Waypoint waypoint2 in this.Collection.list)
			{
				if (waypoint2.lastKnownPositionEntityId == _drones[i].Item1)
				{
					waypoint = waypoint2;
					break;
				}
			}
			if (waypoint != null)
			{
				Vector3i vector3i = Vector3i.FromVector3Rounded(_drones[i].Item2);
				if (waypoint.pos != vector3i)
				{
					this.Collection.Remove(waypoint);
					waypoint.pos = vector3i;
					waypoint.navObject.TrackedPosition = _drones[i].Item2;
					this.Collection.Add(waypoint);
				}
			}
			else
			{
				((XUiC_MapArea)GameManager.Instance.World.GetPrimaryPlayer().PlayerUI.xui.GetWindow("mapArea").Controller).RefreshDronePositionWaypoint(_drones[i].Item1, Vector3i.FromVector3Rounded(_drones[i].Item2), true);
			}
		}
		List<Waypoint> list = new List<Waypoint>();
		for (int j = 0; j < this.Collection.list.Count; j++)
		{
			if (this.Collection.list[j].lastKnownPositionEntityId != -1 && this.Collection.list[j].lastKnownPositionEntityType == eLastKnownPositionEntityType.Drone)
			{
				Waypoint waypoint3 = this.Collection.list[j];
				bool flag = false;
				for (int k = 0; k < _drones.Count; k++)
				{
					if (waypoint3.lastKnownPositionEntityId == _drones[k].Item1)
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					list.Add(waypoint3);
				}
			}
		}
		foreach (Waypoint waypoint4 in list)
		{
			this.Collection.Remove(waypoint4);
			NavObjectManager.Instance.UnRegisterNavObject(waypoint4.navObject);
		}
	}

	public void UpdateEntityDroneWayPoint(EntityDrone drone, bool following, bool unloaded = false)
	{
		if (!drone.LocalPlayerIsOwner())
		{
			return;
		}
		Waypoint waypoint = null;
		foreach (Waypoint waypoint2 in this.Collection.list)
		{
			if (waypoint2.lastKnownPositionEntityId == drone.entityId)
			{
				waypoint = waypoint2;
				break;
			}
		}
		if (waypoint != null)
		{
			Vector3i vector3i = Vector3i.FromVector3Rounded(drone.position);
			if (waypoint.pos != vector3i)
			{
				this.Collection.Remove(waypoint);
				waypoint.pos = vector3i;
				waypoint.navObject.TrackedPosition = drone.position;
				this.Collection.Add(waypoint);
			}
			waypoint.navObject.hiddenOnCompass = !unloaded;
			this.SetWaypointHiddenOnMap(drone.entityId, !unloaded);
			return;
		}
		((XUiC_MapArea)GameManager.Instance.World.GetPrimaryPlayer().PlayerUI.xui.GetWindow("mapArea").Controller).RefreshDronePositionWaypoint(drone, false);
	}

	public void SetWaypointHiddenOnMap(int _entityId, bool _hidden)
	{
		foreach (Waypoint waypoint in this.Collection.list)
		{
			if (waypoint.lastKnownPositionEntityId == _entityId)
			{
				waypoint.HiddenOnMap = _hidden;
				waypoint.navObject.hiddenOnMap = _hidden;
				break;
			}
		}
	}

	public bool TryRemoveLastKnownPositionWaypoint(int _entityId)
	{
		Waypoint lastKnownPositionWaypoint = this.GetLastKnownPositionWaypoint(_entityId);
		if (lastKnownPositionWaypoint != null)
		{
			this.Collection.Remove(lastKnownPositionWaypoint);
			NavObjectManager.Instance.UnRegisterNavObject(lastKnownPositionWaypoint.navObject);
			return true;
		}
		return false;
	}

	public Waypoint GetWaypointForNavObject(NavObject nav)
	{
		foreach (Waypoint waypoint in this.Collection.list)
		{
			if (nav == waypoint.navObject)
			{
				return waypoint;
			}
		}
		return null;
	}

	public const int cCurrentSaveVersion = 7;

	public HashSetList<Waypoint> Collection = new HashSetList<Waypoint>();
}
