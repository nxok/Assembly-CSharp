using System;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageSignDataResponse : NetPackage
{
	public override NetPackageDirection PackageDirection
	{
		get
		{
			return NetPackageDirection.ToClient;
		}
	}

	public override bool Compress
	{
		get
		{
			return true;
		}
	}

	public NetPackageSignDataResponse Setup(byte[] _data, bool _isLastBatch)
	{
		this.data = _data;
		this.isLastBatch = _isLastBatch;
		return this;
	}

	public override void read(PooledBinaryReader _reader)
	{
		this.isLastBatch = _reader.ReadBoolean();
		int num = _reader.ReadInt32();
		this.data = ((num > 0) ? _reader.ReadBytes(num) : null);
	}

	public override void write(PooledBinaryWriter _writer)
	{
		base.write(_writer);
		_writer.Write(this.isLastBatch);
		int num = ((this.data != null) ? this.data.Length : 0);
		_writer.Write(num);
		if (num > 0)
		{
			_writer.Write(this.data);
		}
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		SignDataManager.Instance.ProcessSignDataBatchReceived(this.data, this.isLastBatch);
	}

	public override int GetLength()
	{
		return 0;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public byte[] data;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isLastBatch;
}
