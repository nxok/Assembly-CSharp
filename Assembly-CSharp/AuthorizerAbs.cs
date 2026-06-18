using System;
using Platform;

public abstract class AuthorizerAbs : IAuthorizer
{
	public abstract int Order { get; }

	public abstract string AuthorizerName { get; }

	public abstract string StateLocalizationKey { get; }

	public virtual EPlatformIdentifier PlatformRestriction
	{
		get
		{
			return EPlatformIdentifier.Count;
		}
	}

	public virtual bool AuthorizerActive
	{
		get
		{
			return true;
		}
	}

	public virtual void Init(IAuthorizationResponses _authResponsesHandler)
	{
		this.authResponsesHandler = _authResponsesHandler;
	}

	public virtual void Cleanup()
	{
	}

	public virtual void ServerStart()
	{
	}

	public virtual void ServerStop()
	{
	}

	public abstract ValueTuple<EAuthorizerSyncResult, GameUtils.KickPlayerData?> Authorize(ClientInfo _clientInfo);

	public virtual void Disconnect(ClientInfo _clientInfo)
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public AuthorizerAbs()
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public IAuthorizationResponses authResponsesHandler;
}
