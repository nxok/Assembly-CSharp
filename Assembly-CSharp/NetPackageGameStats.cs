using System;
using System.Collections;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageGameStats : NetPackage
{
	public NetPackageGameStats Setup(GameStats _gs)
	{
		using (PooledBinaryWriter pooledBinaryWriter = MemoryPools.poolBinaryWriter.AllocSync(false))
		{
			pooledBinaryWriter.SetBaseStream(this.ms);
			_gs.Write(pooledBinaryWriter);
		}
		return this;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public ~NetPackageGameStats()
	{
		MemoryPools.poolMemoryStream.FreeSync(this.ms);
	}

	public override void read(PooledBinaryReader _reader)
	{
		int num = (int)_reader.ReadInt16();
		StreamUtils.StreamCopy(_reader.BaseStream, this.ms, num, null, true);
	}

	public override void write(PooledBinaryWriter _writer)
	{
		base.write(_writer);
		_writer.Write((short)this.ms.Length);
		this.ms.WriteTo(_writer.BaseStream);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		ThreadManager.StartCoroutine(this.readStatsCo());
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator readStatsCo()
	{
		while (GameManager.Instance.World == null && SingletonMonoBehaviour<ConnectionManager>.Instance.IsConnected)
		{
			yield return null;
		}
		if (GameManager.Instance.World == null)
		{
			yield break;
		}
		using (PooledBinaryReader pooledBinaryReader = MemoryPools.poolBinaryReader.AllocSync(false))
		{
			PooledExpandableMemoryStream pooledExpandableMemoryStream = this.ms;
			lock (pooledExpandableMemoryStream)
			{
				pooledBinaryReader.SetBaseStream(this.ms);
				this.ms.Position = 0L;
				GameStats.Instance.Read(pooledBinaryReader);
			}
		}
		GameManager.Instance.GetGameStateManager().OnUpdateTick();
		yield break;
	}

	public override int GetLength()
	{
		return (int)this.ms.Length;
	}

	public override NetPackageDirection PackageDirection
	{
		get
		{
			return NetPackageDirection.ToClient;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public PooledExpandableMemoryStream ms = MemoryPools.poolMemoryStream.AllocSync(true);
}
