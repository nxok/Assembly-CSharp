using System;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageSetBlockResponse : NetPackage
{
	public NetPackageSetBlockResponse Setup(eSetBlockResponse _response)
	{
		this.response = _response;
		return this;
	}

	public override void read(PooledBinaryReader _reader)
	{
		this.response = (eSetBlockResponse)_reader.ReadUInt16();
	}

	public override void write(PooledBinaryWriter _writer)
	{
		base.write(_writer);
		_writer.Write((ushort)this.response);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		eSetBlockResponse eSetBlockResponse = this.response;
		if (eSetBlockResponse == eSetBlockResponse.PowerBlockLimitExceeded)
		{
			GameManager.ShowTooltip(GameManager.Instance.World.GetPrimaryPlayer(), "uicannotaddpowerblock", false, false, 0f);
			return;
		}
		if (eSetBlockResponse != eSetBlockResponse.StorageBlockLimitExceeded)
		{
			return;
		}
		GameManager.ShowTooltip(GameManager.Instance.World.GetPrimaryPlayer(), "uicannotaddstorageblock", false, false, 0f);
	}

	public override int GetLength()
	{
		return 4;
	}

	public eSetBlockResponse response;
}
