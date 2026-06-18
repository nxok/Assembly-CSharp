using System;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageLandClaimRepair : NetPackage
{
	public NetPackageLandClaimRepair Setup(Vector3i _blockPosition, bool _beginRepair)
	{
		this.blockPosition = _blockPosition;
		this.beginRepair = _beginRepair;
		return this;
	}

	public override void read(PooledBinaryReader _br)
	{
		long num = _br.ReadInt64();
		long num2 = _br.ReadInt64();
		long num3 = _br.ReadInt64();
		this.blockPosition = new Vector3i((float)num, (float)num2, (float)num3);
		this.beginRepair = _br.ReadBoolean();
	}

	public override void write(PooledBinaryWriter _bw)
	{
		base.write(_bw);
		_bw.Write((long)this.blockPosition.x);
		_bw.Write((long)this.blockPosition.y);
		_bw.Write((long)this.blockPosition.z);
		_bw.Write(this.beginRepair);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		TEFeatureAreaRepair selfOrFeature = _world.GetTileEntity(this.blockPosition).GetSelfOrFeature<TEFeatureAreaRepair>();
		if (selfOrFeature != null)
		{
			if (this.beginRepair)
			{
				if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
				{
					selfOrFeature.RepairAll(_world, this.blockPosition, base.Sender.entityId);
					return;
				}
			}
			else if (selfOrFeature.Parent.Owner.Equals(PlatformManager.InternalLocalUserIdentifier))
			{
				selfOrFeature.IsRepairing = false;
			}
		}
	}

	public override int GetLength()
	{
		return 25;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3i blockPosition;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool beginRepair;
}
