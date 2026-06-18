using System;
using System.Runtime.CompilerServices;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public class NetPackagePackageIds : NetPackage
{
	public override bool FlushQueue
	{
		get
		{
			return true;
		}
	}

	public override NetPackageDirection PackageDirection
	{
		get
		{
			return NetPackageDirection.ToClient;
		}
	}

	public override bool AllowedBeforeAuth
	{
		get
		{
			return true;
		}
	}

	public NetPackagePackageIds Setup()
	{
		this.toSendCount = NetPackageManager.KnownPackageCount;
		IPlatform crossplatformPlatform = PlatformManager.CrossplatformPlatform;
		bool? flag;
		if (crossplatformPlatform == null)
		{
			flag = null;
		}
		else
		{
			IAntiCheatServer antiCheatServer = crossplatformPlatform.AntiCheatServer;
			flag = ((antiCheatServer != null) ? new bool?(antiCheatServer.ServerEacEnabled()) : null);
		}
		bool? flag2 = flag;
		this.serverUseEAC = flag2.GetValueOrDefault();
		if (this.serverUseEAC)
		{
			IPlatform crossplatformPlatform2 = PlatformManager.CrossplatformPlatform;
			bool? flag3;
			if (crossplatformPlatform2 == null)
			{
				flag3 = null;
			}
			else
			{
				IAntiCheatServer antiCheatServer2 = crossplatformPlatform2.AntiCheatServer;
				flag3 = ((antiCheatServer2 != null) ? new bool?(antiCheatServer2.GetHostUserIdAndToken(out this.hostUserAndToken)) : null);
			}
			flag2 = flag3;
			this.hasHostUserAndToken = flag2.GetValueOrDefault();
		}
		return this;
	}

	public override void read(PooledBinaryReader _reader)
	{
		this.compatVersion = VersionInformation.Read(_reader);
		int num = _reader.ReadInt32();
		this.mappings = new string[num];
		for (int i = 0; i < num; i++)
		{
			this.mappings[i] = _reader.ReadString();
		}
		this.serverUseEAC = _reader.ReadBoolean();
		this.hasHostUserAndToken = _reader.ReadBoolean();
		if (this.hasHostUserAndToken)
		{
			this.hostUserAndToken = new ValueTuple<PlatformUserIdentifierAbs, string>(PlatformUserIdentifierAbs.FromStream(_reader, false, true), _reader.ReadString());
		}
	}

	public override void write(PooledBinaryWriter _writer)
	{
		base.write(_writer);
		Constants.cVersionInformation.Write(_writer);
		Type[] packageMappings = NetPackageManager.PackageMappings;
		_writer.Write(packageMappings.Length);
		foreach (Type type in packageMappings)
		{
			_writer.Write(type.Name);
		}
		_writer.Write(this.serverUseEAC);
		_writer.Write(this.hasHostUserAndToken);
		if (this.hasHostUserAndToken)
		{
			this.hostUserAndToken.Item1.ToStream(_writer, true);
			_writer.Write(this.hostUserAndToken.Item2 ?? "");
		}
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (!this.compatVersion.EqualsMinor(Constants.cVersionInformation))
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.Disconnect();
			GameUtils.EKickReason ekickReason = GameUtils.EKickReason.VersionMismatch;
			int num = 0;
			string longStringNoBuild = this.compatVersion.LongStringNoBuild;
			GameUtils.KickPlayerData kickPlayerData = new GameUtils.KickPlayerData(ekickReason, num, default(DateTime), longStringNoBuild);
			GameManager.Instance.ShowMessageServerAuthFailed(kickPlayerData.LocalizedMessage());
			return;
		}
		NetPackageManager.IdMappingsReceived(this.mappings);
		if (this.serverUseEAC)
		{
			IAntiCheatClient antiCheatClient = PlatformManager.MultiPlatform.AntiCheatClient;
			if (antiCheatClient == null || !antiCheatClient.ClientAntiCheatEnabled())
			{
				SingletonMonoBehaviour<ConnectionManager>.Instance.Disconnect();
				GameUtils.KickPlayerData kickPlayerData2 = new GameUtils.KickPlayerData(GameUtils.EKickReason.EosEacViolation, 4, default(DateTime), "");
				GameManager.Instance.ShowMessageServerAuthFailed(kickPlayerData2.LocalizedMessage());
				return;
			}
			INetConnection[] connectionToServer = SingletonMonoBehaviour<ConnectionManager>.Instance.GetConnectionToServer();
			for (int i = 0; i < connectionToServer.Length; i++)
			{
				connectionToServer[i].SetEncryptionModule(PlatformManager.MultiPlatform.AntiCheatClient);
			}
			PlatformManager.MultiPlatform.AntiCheatClient.ConnectToServer(this.hostUserAndToken, delegate
			{
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendLogin();
			}, delegate(string errorMessage)
			{
				SingletonMonoBehaviour<ConnectionManager>.Instance.Disconnect();
				GameUtils.KickPlayerData kickPlayerData3 = new GameUtils.KickPlayerData(GameUtils.EKickReason.CrossPlatformAuthenticationFailed, 50, default(DateTime), errorMessage);
				GameManager.Instance.ShowMessageServerAuthFailed(kickPlayerData3.LocalizedMessage());
			});
			return;
		}
		else
		{
			if (!this.hasHostUserAndToken && (DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5).IsCurrent() && Submission.Enabled)
			{
				SingletonMonoBehaviour<ConnectionManager>.Instance.Disconnect();
				GameManager.Instance.ShowMessageServerAuthFailed(Localization.Get("auth_serverEACRequired", false, null));
				return;
			}
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendLogin();
			return;
		}
	}

	public override int GetLength()
	{
		return 2 + this.toSendCount * 32;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int toSendCount;

	[PublicizedFrom(EAccessModifier.Private)]
	public string[] mappings;

	[PublicizedFrom(EAccessModifier.Private)]
	public VersionInformation compatVersion;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool serverUseEAC;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool hasHostUserAndToken;

	[TupleElementNames(new string[] { "userId", "token" })]
	[PublicizedFrom(EAccessModifier.Private)]
	public ValueTuple<PlatformUserIdentifierAbs, string> hostUserAndToken;
}
