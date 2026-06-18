using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageBlockLimitTracking : NetPackage
{
	public NetPackageBlockLimitTracking()
	{
		this.amounts = new List<int>();
	}

	public NetPackageBlockLimitTracking Setup(List<int> _amounts)
	{
		this.amounts = _amounts;
		return this;
	}

	public override void read(PooledBinaryReader _br)
	{
		this.amounts.Clear();
		int num = _br.ReadInt32();
		for (int i = 0; i < num; i++)
		{
			this.amounts.Add(_br.ReadInt32());
		}
	}

	public override void write(PooledBinaryWriter _bw)
	{
		base.write(_bw);
		_bw.Write(this.amounts.Count);
		for (int i = 0; i < this.amounts.Count; i++)
		{
			_bw.Write(this.amounts[i]);
		}
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			Log.Warning("Server should not receive a NetPackageBlockLimitTracking");
			return;
		}
		BlockLimitTracker.instance.UpdateClientAmounts(this.amounts);
	}

	public override int GetLength()
	{
		return 4 + this.amounts.Count * 4;
	}

	public List<int> amounts;
}
