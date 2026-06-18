using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageDecoUpdate : NetPackage
{
	public override NetPackageDirection PackageDirection
	{
		get
		{
			return NetPackageDirection.ToClient;
		}
	}

	public NetPackageDecoUpdate Setup(List<DecoObject> _decoList, ref int _currentIndex)
	{
		this.firstPackage = _currentIndex == 0;
		int num = Math.Min(32768, _decoList.Count - _currentIndex);
		int num2 = _currentIndex + num;
		using (PooledBinaryWriter pooledBinaryWriter = MemoryPools.poolBinaryWriter.AllocSync(false))
		{
			pooledBinaryWriter.SetBaseStream(this.ms);
			pooledBinaryWriter.Write(num);
			for (int i = _currentIndex; i < num2; i++)
			{
				_decoList[i].Write(pooledBinaryWriter, null);
			}
		}
		_currentIndex = num2;
		return this;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public ~NetPackageDecoUpdate()
	{
		MemoryPools.poolMemoryStream.FreeSync(this.ms);
	}

	public override void read(PooledBinaryReader _br)
	{
		this.firstPackage = _br.ReadBoolean();
		int num = _br.ReadInt32();
		StreamUtils.StreamCopy(_br.BaseStream, this.ms, num, null, true);
	}

	public override void write(PooledBinaryWriter _bw)
	{
		base.write(_bw);
		_bw.Write(this.firstPackage);
		_bw.Write((int)this.ms.Length);
		this.ms.WriteTo(_bw.BaseStream);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		using (PooledBinaryReader pooledBinaryReader = MemoryPools.poolBinaryReader.AllocSync(false))
		{
			PooledExpandableMemoryStream pooledExpandableMemoryStream = this.ms;
			lock (pooledExpandableMemoryStream)
			{
				pooledBinaryReader.SetBaseStream(this.ms);
				this.ms.Position = 0L;
				DecoManager.Instance.Read(pooledBinaryReader, int.MaxValue, this.firstPackage);
			}
		}
	}

	public override int GetLength()
	{
		return (int)this.ms.Length;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public PooledExpandableMemoryStream ms = MemoryPools.poolMemoryStream.AllocSync(true);

	[PublicizedFrom(EAccessModifier.Private)]
	public bool firstPackage = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int decoSize = 16;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int decosPerPackage = 32768;
}
