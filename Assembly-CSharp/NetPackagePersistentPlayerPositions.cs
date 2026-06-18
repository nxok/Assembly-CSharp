using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class NetPackagePersistentPlayerPositions : NetPackage
{
	public override NetPackageDirection PackageDirection
	{
		get
		{
			return NetPackageDirection.ToClient;
		}
	}

	public override bool FlushQueue
	{
		get
		{
			return true;
		}
	}

	public NetPackagePersistentPlayerPositions Setup(PersistentPlayerList _ppl)
	{
		foreach (KeyValuePair<PlatformUserIdentifierAbs, PersistentPlayerData> keyValuePair in _ppl.Players)
		{
			PlatformUserIdentifierAbs platformUserIdentifierAbs;
			PersistentPlayerData persistentPlayerData;
			keyValuePair.Deconstruct(out platformUserIdentifierAbs, out persistentPlayerData);
			PlatformUserIdentifierAbs platformUserIdentifierAbs2 = platformUserIdentifierAbs;
			PersistentPlayerData persistentPlayerData2 = persistentPlayerData;
			if (persistentPlayerData2.EntityId != -1)
			{
				persistentPlayerData2.UpdatePositionFromEntity();
				this.positions[platformUserIdentifierAbs2] = persistentPlayerData2.Position;
			}
		}
		return this;
	}

	public override void read(PooledBinaryReader _reader)
	{
		int num = _reader.ReadInt32();
		for (int i = 0; i < num; i++)
		{
			PlatformUserIdentifierAbs platformUserIdentifierAbs = PlatformUserIdentifierAbs.FromStream(_reader, false, false);
			Vector3i vector3i = StreamUtils.ReadVector3i(_reader);
			this.positions[platformUserIdentifierAbs] = vector3i;
		}
	}

	public override void write(PooledBinaryWriter _writer)
	{
		base.write(_writer);
		_writer.Write(this.positions.Count);
		foreach (KeyValuePair<PlatformUserIdentifierAbs, Vector3i> keyValuePair in this.positions)
		{
			PlatformUserIdentifierAbs platformUserIdentifierAbs;
			Vector3i vector3i;
			keyValuePair.Deconstruct(out platformUserIdentifierAbs, out vector3i);
			PlatformUserIdentifierAbs platformUserIdentifierAbs2 = platformUserIdentifierAbs;
			Vector3i vector3i2 = vector3i;
			platformUserIdentifierAbs2.ToStream(_writer, false);
			StreamUtils.Write(_writer, vector3i2);
		}
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		PersistentPlayerList persistentPlayers = _callbacks.persistentPlayers;
		if (persistentPlayers == null)
		{
			return;
		}
		foreach (KeyValuePair<PlatformUserIdentifierAbs, Vector3i> keyValuePair in this.positions)
		{
			PlatformUserIdentifierAbs platformUserIdentifierAbs;
			Vector3i vector3i;
			keyValuePair.Deconstruct(out platformUserIdentifierAbs, out vector3i);
			PlatformUserIdentifierAbs platformUserIdentifierAbs2 = platformUserIdentifierAbs;
			Vector3i vector3i2 = vector3i;
			PersistentPlayerData playerData = persistentPlayers.GetPlayerData(platformUserIdentifierAbs2);
			if (playerData != null)
			{
				playerData.Position = vector3i2;
			}
		}
		XUiC_SpawnNearFriendsList.Instance.UpdatePlayers();
	}

	public override int GetLength()
	{
		return this.positions.Count * 44;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<PlatformUserIdentifierAbs, Vector3i> positions = new Dictionary<PlatformUserIdentifierAbs, Vector3i>();
}
