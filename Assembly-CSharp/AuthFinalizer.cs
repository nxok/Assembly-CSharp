using System;
using UnityEngine.Scripting;

[Preserve]
public class AuthFinalizer : AuthorizerAbs
{
	public AuthFinalizer()
	{
		AuthFinalizer.Instance = this;
	}

	public override int Order
	{
		get
		{
			return 999;
		}
	}

	public override string AuthorizerName
	{
		get
		{
			return "Finalizer";
		}
	}

	public override string StateLocalizationKey
	{
		get
		{
			return null;
		}
	}

	public override ValueTuple<EAuthorizerSyncResult, GameUtils.KickPlayerData?> Authorize(ClientInfo _clientInfo)
	{
		_clientInfo.SendPackage(NetPackageManager.GetPackage<NetPackageAuthConfirmation>().Setup());
		return new ValueTuple<EAuthorizerSyncResult, GameUtils.KickPlayerData?>(EAuthorizerSyncResult.WaitAsync, null);
	}

	public void ReplyReceived(ClientInfo _cInfo)
	{
		this.authResponsesHandler.AuthorizationAccepted(this, _cInfo);
	}

	public static AuthFinalizer Instance;
}
