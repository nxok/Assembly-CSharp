using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageConsoleCmdClient : NetPackage
{
	public NetPackageConsoleCmdClient Setup(List<string> _lines, bool _bExecute)
	{
		this.lines = _lines;
		this.bExecute = _bExecute;
		return this;
	}

	public NetPackageConsoleCmdClient Setup(string _line, bool _bExecute)
	{
		this.lines = new List<string> { _line };
		this.bExecute = _bExecute;
		return this;
	}

	public override void read(PooledBinaryReader _br)
	{
		int num = _br.ReadInt32();
		this.lines = new List<string>(num);
		for (int i = 0; i < num; i++)
		{
			this.lines.Add(_br.ReadString());
		}
		this.bExecute = _br.ReadBoolean();
	}

	public override void write(PooledBinaryWriter _bw)
	{
		base.write(_bw);
		_bw.Write(this.lines.Count);
		for (int i = 0; i < this.lines.Count; i++)
		{
			_bw.Write(this.lines[i]);
		}
		_bw.Write(this.bExecute);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world == null)
		{
			return;
		}
		if (this.bExecute)
		{
			GUIWindowConsole.AddLines(SingletonMonoBehaviour<SdtdConsole>.Instance.ExecuteSync(this.lines[0], null));
			return;
		}
		GUIWindowConsole.AddLines(this.lines);
	}

	public override NetPackageDirection PackageDirection
	{
		get
		{
			return NetPackageDirection.ToClient;
		}
	}

	public override int GetLength()
	{
		return 40;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public List<string> lines;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bExecute;
}
