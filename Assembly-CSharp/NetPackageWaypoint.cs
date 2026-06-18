using System;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageWaypoint : NetPackage
{
	public NetPackageWaypoint Setup(Waypoint _waypoint, EnumWaypointInviteMode _inviteMode, int _inviterEntityId)
	{
		this.waypoint = _waypoint;
		this.waypoint.InviterEntityId = _inviterEntityId;
		this.inviteMode = _inviteMode;
		this.inviterEntityId = _inviterEntityId;
		return this;
	}

	public override void read(PooledBinaryReader _br)
	{
		this.waypoint = new Waypoint();
		this.waypoint.Read(_br, 7);
		this.inviteMode = (EnumWaypointInviteMode)_br.ReadByte();
		this.inviterEntityId = _br.ReadInt32();
	}

	public override void write(PooledBinaryWriter _bw)
	{
		base.write(_bw);
		this.waypoint.Write(_bw);
		_bw.Write((byte)this.inviteMode);
		_bw.Write(this.inviterEntityId);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (!base.ValidEntityIdForSender(this.inviterEntityId, false))
		{
			return;
		}
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			GameManager.Instance.WaypointInviteServer(this.waypoint, this.inviteMode, this.inviterEntityId);
			return;
		}
		GameManager.Instance.WaypointInviteClient(this.waypoint, this.inviteMode, this.inviterEntityId, null);
	}

	public override int GetLength()
	{
		return 4;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Waypoint waypoint;

	[PublicizedFrom(EAccessModifier.Private)]
	public EnumWaypointInviteMode inviteMode;

	[PublicizedFrom(EAccessModifier.Private)]
	public int inviterEntityId;
}
