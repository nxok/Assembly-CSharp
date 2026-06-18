using System;
using UnityEngine.Scripting;

[Preserve]
public class NetPackagePersistentPlayerState : NetPackage
{
	public override NetPackageDirection PackageDirection
	{
		get
		{
			return NetPackageDirection.ToClient;
		}
	}

	public NetPackagePersistentPlayerState Setup(PersistentPlayerData ppData, EnumPersistentPlayerDataReason reason)
	{
		this.m_ppData = ppData;
		this.m_reason = reason;
		return this;
	}

	public override void read(PooledBinaryReader _reader)
	{
		this.m_reason = (EnumPersistentPlayerDataReason)_reader.ReadByte();
		this.m_ppData = PersistentPlayerData.Read(_reader);
	}

	public override void write(PooledBinaryWriter _writer)
	{
		base.write(_writer);
		_writer.Write((byte)this.m_reason);
		this.m_ppData.Write(_writer);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		_callbacks.PersistentPlayerLogin(this.m_ppData);
	}

	public override int GetLength()
	{
		return 1000;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public PersistentPlayerData m_ppData;

	[PublicizedFrom(EAccessModifier.Private)]
	public EnumPersistentPlayerDataReason m_reason;
}
