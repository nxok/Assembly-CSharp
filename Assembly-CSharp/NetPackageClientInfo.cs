using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageClientInfo : NetPackage
{
	public NetPackageClientInfo Setup(WorldBase _world, IList<ClientInfo> _clients)
	{
		this.playerIds.Clear();
		this.pingTimes.Clear();
		this.admins.Clear();
		if (!GameManager.IsDedicatedServer)
		{
			EntityPlayerLocal primaryPlayer = GameManager.Instance.World.GetPrimaryPlayer();
			if (primaryPlayer != null)
			{
				this.addPlayerEntry(primaryPlayer, null);
			}
		}
		for (int i = 0; i < _clients.Count; i++)
		{
			int entityId = _clients[i].entityId;
			if (entityId != -1)
			{
				EntityAlive entityAlive = (EntityAlive)_world.GetEntity(entityId);
				this.addPlayerEntry(entityAlive, _clients[i]);
			}
		}
		return this;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void addPlayerEntry(EntityAlive _ea, ClientInfo _clientInfo)
	{
		if (_ea == null)
		{
			return;
		}
		EntityPlayer entityPlayer = _ea as EntityPlayer;
		_ea.pingToServer = ((_clientInfo != null) ? _clientInfo.ping : (-1));
		this.playerIds.Add(_ea.entityId);
		this.pingTimes.Add(_ea.pingToServer);
		this.admins.Add(entityPlayer != null && entityPlayer.IsAdmin);
	}

	public override void read(PooledBinaryReader _reader)
	{
		this.playerIds.Clear();
		this.pingTimes.Clear();
		this.admins.Clear();
		int num = (int)_reader.ReadUInt16();
		for (int i = 0; i < num; i++)
		{
			this.playerIds.Add(_reader.ReadInt32());
			this.pingTimes.Add((int)_reader.ReadInt16());
			this.admins.Add(_reader.ReadBoolean());
		}
	}

	public override void write(PooledBinaryWriter _writer)
	{
		base.write(_writer);
		_writer.Write((ushort)this.playerIds.Count);
		for (int i = 0; i < this.playerIds.Count; i++)
		{
			_writer.Write(this.playerIds[i]);
			_writer.Write((short)this.pingTimes[i]);
			_writer.Write(this.admins[i]);
		}
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world == null)
		{
			return;
		}
		for (int i = 0; i < this.playerIds.Count; i++)
		{
			EntityAlive entityAlive = (EntityAlive)_world.GetEntity(this.playerIds[i]);
			if (entityAlive != null)
			{
				entityAlive.pingToServer = this.pingTimes[i];
				EntityPlayer entityPlayer = entityAlive as EntityPlayer;
				if (entityPlayer != null)
				{
					entityPlayer.IsAdmin = this.admins[i];
				}
			}
		}
	}

	public override int GetLength()
	{
		return 40;
	}

	public override NetPackageDirection PackageDirection
	{
		get
		{
			return NetPackageDirection.ToClient;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public List<int> playerIds = new List<int>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<int> pingTimes = new List<int>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<bool> admins = new List<bool>();
}
