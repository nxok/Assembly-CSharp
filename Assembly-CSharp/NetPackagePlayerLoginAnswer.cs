using System;
using System.Runtime.CompilerServices;
using System.Text;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public class NetPackagePlayerLoginAnswer : NetPackage
{
	public override bool FlushQueue
	{
		get
		{
			return true;
		}
	}

	public NetPackagePlayerLoginAnswer Setup(bool _bAllowed, string _data, PlatformLobbyId _platformLobbyId, [TupleElementNames(new string[] { "userId", "token" })] ValueTuple<PlatformUserIdentifierAbs, string> _platformUserAndToken, [TupleElementNames(new string[] { "userId", "token" })] ValueTuple<PlatformUserIdentifierAbs, string> _crossplatformUserAndToken)
	{
		this.bAllowed = _bAllowed;
		this.data = _data;
		this.platformLobbyId = _platformLobbyId;
		this.platformUserAndToken = _platformUserAndToken;
		this.crossplatformUserAndToken = _crossplatformUserAndToken;
		this.RecalcLength();
		return this;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RecalcLength()
	{
		Encoding utf = Encoding.UTF8;
		this.length = 1 + this.data.GetBinaryWriterLength(utf) + this.platformLobbyId.GetWriteLength(utf) + this.platformUserAndToken.Item1.GetToStreamLength(utf, true) + (this.platformUserAndToken.Item2 ?? "").GetBinaryWriterLength(utf) + this.crossplatformUserAndToken.Item1.GetToStreamLength(utf, true) + (this.crossplatformUserAndToken.Item2 ?? "").GetBinaryWriterLength(utf);
	}

	public override void read(PooledBinaryReader _br)
	{
		this.bAllowed = _br.ReadBoolean();
		this.data = _br.ReadString();
		this.platformLobbyId = PlatformLobbyId.Read(_br);
		this.platformUserAndToken = new ValueTuple<PlatformUserIdentifierAbs, string>(PlatformUserIdentifierAbs.FromStream(_br, false, true), _br.ReadString());
		this.crossplatformUserAndToken = new ValueTuple<PlatformUserIdentifierAbs, string>(PlatformUserIdentifierAbs.FromStream(_br, false, true), _br.ReadString());
		this.RecalcLength();
	}

	public override void write(PooledBinaryWriter _bw)
	{
		base.write(_bw);
		_bw.Write(this.bAllowed);
		_bw.Write(this.data);
		this.platformLobbyId.Write(_bw);
		this.platformUserAndToken.Item1.ToStream(_bw, true);
		_bw.Write(this.platformUserAndToken.Item2 ?? "");
		this.crossplatformUserAndToken.Item1.ToStream(_bw, true);
		_bw.Write(this.crossplatformUserAndToken.Item2 ?? "");
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (this.bAllowed)
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.PlayerAllowed(this.data, this.platformLobbyId, this.platformUserAndToken, this.crossplatformUserAndToken);
			return;
		}
		SingletonMonoBehaviour<ConnectionManager>.Instance.PlayerDenied(this.data);
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
		return this.length;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bAllowed;

	[PublicizedFrom(EAccessModifier.Private)]
	public string data;

	[PublicizedFrom(EAccessModifier.Private)]
	public PlatformLobbyId platformLobbyId;

	[TupleElementNames(new string[] { "userId", "token" })]
	[PublicizedFrom(EAccessModifier.Private)]
	public ValueTuple<PlatformUserIdentifierAbs, string> platformUserAndToken;

	[TupleElementNames(new string[] { "userId", "token" })]
	[PublicizedFrom(EAccessModifier.Private)]
	public ValueTuple<PlatformUserIdentifierAbs, string> crossplatformUserAndToken;

	[PublicizedFrom(EAccessModifier.Private)]
	public int length;
}
