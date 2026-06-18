using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageEntityWaypointList : NetPackage
{
	public NetPackageEntityWaypointList Setup(eWayPointListType _listType, [TupleElementNames(new string[] { "entityId", "position" })] List<ValueTuple<int, Vector3>> _positions)
	{
		this.listType = (short)_listType;
		this.positions = new List<ValueTuple<int, Vector3>>(_positions);
		return this;
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		EntityPlayerLocal primaryPlayer = _world.GetPrimaryPlayer();
		if (primaryPlayer != null)
		{
			if (this.listType == 0)
			{
				primaryPlayer.Waypoints.SetEntityVehicleWaypointFromVehicleManager(this.positions);
				return;
			}
			primaryPlayer.Waypoints.SetDroneWaypointsFromDroneManager(this.positions);
		}
	}

	public override void read(PooledBinaryReader _reader)
	{
		this.listType = _reader.ReadInt16();
		int num = _reader.ReadInt32();
		this.positions = new List<ValueTuple<int, Vector3>>();
		for (int i = 0; i < num; i++)
		{
			this.positions.Add(new ValueTuple<int, Vector3>(_reader.ReadInt32(), StreamUtils.ReadVector3(_reader)));
		}
	}

	public override void write(PooledBinaryWriter _writer)
	{
		base.write(_writer);
		_writer.Write(this.listType);
		_writer.Write(this.positions.Count);
		for (int i = 0; i < this.positions.Count; i++)
		{
			_writer.Write(this.positions[i].Item1);
			StreamUtils.Write(_writer, this.positions[i].Item2);
		}
	}

	public override int GetLength()
	{
		return 0;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public short listType;

	[TupleElementNames(new string[] { "entityId", "position" })]
	[PublicizedFrom(EAccessModifier.Private)]
	public List<ValueTuple<int, Vector3>> positions;
}
