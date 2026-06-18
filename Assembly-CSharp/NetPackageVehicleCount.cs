using System;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageVehicleCount : NetPackage
{
	public NetPackageVehicleCount Setup()
	{
		this.vehicleCount = VehicleManager.GetServerVehicleCount();
		this.turretCount = TurretTracker.GetServerTurretCount();
		this.droneCount = DroneManager.GetServerDroneCount();
		return this;
	}

	public override void read(PooledBinaryReader _reader)
	{
		this.vehicleCount = _reader.ReadInt32();
		this.turretCount = _reader.ReadInt32();
		this.droneCount = _reader.ReadInt32();
	}

	public override void write(PooledBinaryWriter _writer)
	{
		base.write(_writer);
		_writer.Write(this.vehicleCount);
		_writer.Write(this.turretCount);
		_writer.Write(this.droneCount);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		VehicleManager.SetServerVehicleCount(this.vehicleCount);
		TurretTracker.SetServerTurretCount(this.turretCount);
		DroneManager.SetServerDroneCount(this.droneCount);
	}

	public override int GetLength()
	{
		return 12;
	}

	public int vehicleCount;

	public int turretCount;

	public int droneCount;
}
