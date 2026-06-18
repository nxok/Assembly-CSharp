using System;
using Audio;
using Twitch;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageTwitchAccess : NetPackage
{
	public NetPackageTwitchAccess Setup()
	{
		return this;
	}

	public NetPackageTwitchAccess Setup(bool _hasAccess)
	{
		this.hasAccess = _hasAccess;
		return this;
	}

	public override void read(PooledBinaryReader _br)
	{
		this.hasAccess = _br.ReadBoolean();
	}

	public override void write(PooledBinaryWriter _bw)
	{
		base.write(_bw);
		_bw.Write(this.hasAccess);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world == null)
		{
			return;
		}
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			AdminTools adminTools = GameManager.Instance.adminTools;
			bool flag = ((adminTools != null) ? adminTools.Users.GetUserPermissionLevel(base.Sender) : 1000) <= GamePrefs.GetInt(EnumGamePrefs.TwitchServerPermission);
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageTwitchAccess>().Setup(flag), false, base.Sender.entityId, -1, -1, null, 192, false);
			return;
		}
		if (this.hasAccess)
		{
			GameEventManager.Current.HandleGameEventAccessApproved();
			return;
		}
		Manager.PlayInsidePlayerHead("Misc/password_fail", -1, 0f, false, false);
		TwitchManager.Current.DeniedPermission();
	}

	public override int GetLength()
	{
		return 30;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool hasAccess;
}
