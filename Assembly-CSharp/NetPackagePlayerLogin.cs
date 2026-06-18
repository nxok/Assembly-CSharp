using System;
using System.Runtime.CompilerServices;
using UnityEngine.Scripting;

[Preserve]
public class NetPackagePlayerLogin : NetPackage
{
	public override bool AllowedBeforeAuth
	{
		get
		{
			return true;
		}
	}

	public NetPackagePlayerLogin Setup(string _playerName, [TupleElementNames(new string[] { "userId", "token" })] ValueTuple<PlatformUserIdentifierAbs, string> _platformUserAndToken, [TupleElementNames(new string[] { "userId", "token" })] ValueTuple<PlatformUserIdentifierAbs, string> _crossplatformUserAndToken, string _version, string _compVersion, ulong _discordUserId)
	{
		this.playerName = _playerName;
		this.platformUserAndToken = _platformUserAndToken;
		this.crossplatformUserAndToken = _crossplatformUserAndToken;
		this.version = _version;
		this.compVersion = _compVersion;
		this.discordUserId = _discordUserId;
		return this;
	}

	public override void read(PooledBinaryReader _br)
	{
		Log.Out("NPPL.Read");
		this.playerName = _br.ReadString();
		this.platformUserAndToken = new ValueTuple<PlatformUserIdentifierAbs, string>(PlatformUserIdentifierAbs.FromStream(_br, false, true), _br.ReadString());
		this.crossplatformUserAndToken = new ValueTuple<PlatformUserIdentifierAbs, string>(PlatformUserIdentifierAbs.FromStream(_br, false, true), _br.ReadString());
		this.version = _br.ReadString();
		this.compVersion = _br.ReadString();
		this.discordUserId = _br.ReadUInt64();
	}

	public override void write(PooledBinaryWriter _bw)
	{
		Log.Out("NPPL.Write");
		base.write(_bw);
		_bw.Write(this.playerName);
		this.platformUserAndToken.Item1.ToStream(_bw, true);
		_bw.Write(this.platformUserAndToken.Item2 ?? "");
		this.crossplatformUserAndToken.Item1.ToStream(_bw, true);
		_bw.Write(this.crossplatformUserAndToken.Item2 ?? "");
		_bw.Write(this.version);
		_bw.Write(this.compVersion);
		_bw.Write(this.discordUserId);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		_callbacks.PlayerLoginRPC(base.Sender, this.playerName, this.platformUserAndToken, this.crossplatformUserAndToken, this.compVersion, this.discordUserId);
	}

	public override NetPackageDirection PackageDirection
	{
		get
		{
			return NetPackageDirection.ToServer;
		}
	}

	public override int GetLength()
	{
		return 120;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string playerName;

	[TupleElementNames(new string[] { "userId", "token" })]
	[PublicizedFrom(EAccessModifier.Private)]
	public ValueTuple<PlatformUserIdentifierAbs, string> platformUserAndToken;

	[TupleElementNames(new string[] { "userId", "token" })]
	[PublicizedFrom(EAccessModifier.Private)]
	public ValueTuple<PlatformUserIdentifierAbs, string> crossplatformUserAndToken;

	[PublicizedFrom(EAccessModifier.Private)]
	public string version;

	[PublicizedFrom(EAccessModifier.Private)]
	public string compVersion;

	[PublicizedFrom(EAccessModifier.Private)]
	public ulong discordUserId;
}
