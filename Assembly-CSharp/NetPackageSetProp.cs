using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageSetProp : NetPackage
{
	public NetPackageSetProp Setup(PersistentPlayerData persistentPlayerData, List<PropChangeInfo> propChanges, int localPlayerThatChanged)
	{
		this.m_persistentPlayerId = ((persistentPlayerData != null) ? persistentPlayerData.PrimaryId : null);
		this.m_propChanges = propChanges;
		this.m_localPlayerThatChanged = localPlayerThatChanged;
		return this;
	}

	public override void read(PooledBinaryReader _br)
	{
		this.m_persistentPlayerId = PlatformUserIdentifierAbs.FromStream(_br, false, false);
		int num = (int)_br.ReadInt16();
		this.m_propChanges = new List<PropChangeInfo>();
		for (int i = 0; i < num; i++)
		{
			this.m_propChanges.Add(PropChangeInfo.Read(_br));
		}
		this.m_localPlayerThatChanged = _br.ReadInt32();
	}

	public override void write(PooledBinaryWriter _bw)
	{
		base.write(_bw);
		this.m_persistentPlayerId.ToStream(_bw, false);
		int count = this.m_propChanges.Count;
		_bw.Write((short)count);
		for (int i = 0; i < count; i++)
		{
			this.m_propChanges[i].Write(_bw);
		}
		_bw.Write(this.m_localPlayerThatChanged);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (!base.ValidUserIdForSender(this.m_persistentPlayerId))
		{
			return;
		}
		if (!base.ValidEntityIdForSender(this.m_localPlayerThatChanged, false))
		{
			return;
		}
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			_callbacks.SetPropsOnClients(this.m_localPlayerThatChanged, this);
		}
		if (((_world != null) ? _world.ChunkCache : null) == null)
		{
			return;
		}
		_callbacks.ChangeProps(this.m_persistentPlayerId, this.m_propChanges);
	}

	public override int GetLength()
	{
		return this.m_propChanges.Count * 16;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public List<PropChangeInfo> m_propChanges;

	[PublicizedFrom(EAccessModifier.Private)]
	public PlatformUserIdentifierAbs m_persistentPlayerId;

	[PublicizedFrom(EAccessModifier.Private)]
	public int m_localPlayerThatChanged;
}
