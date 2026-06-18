using System;
using System.Collections;
using System.Runtime.CompilerServices;
using Platform;

public class DiscordInviteListener : IJoinSessionGameInviteListener
{
	public static DiscordInviteListener ListenerInstance
	{
		get
		{
			DiscordInviteListener discordInviteListener;
			if ((discordInviteListener = DiscordInviteListener.listenerInstance) == null)
			{
				discordInviteListener = (DiscordInviteListener.listenerInstance = new DiscordInviteListener());
			}
			return discordInviteListener;
		}
	}

	public void Init(IPlatform _owner)
	{
	}

	public void SetPendingInvite(string _sessionId, string _password)
	{
		this.pendingActivityInvite = new ValueTuple<string, string>?(new ValueTuple<string, string>(_sessionId, _password));
	}

	[return: TupleElementNames(new string[] { "invite", "password" })]
	public ValueTuple<string, string> TakePendingInvite()
	{
		if (this.pendingActivityInvite == null)
		{
			return new ValueTuple<string, string>(null, null);
		}
		ValueTuple<string, string> value = this.pendingActivityInvite.Value;
		this.pendingActivityInvite = null;
		return value;
	}

	public IEnumerator ConnectToInvite(string _invite, string _password = null, Action<bool> _onFinished = null)
	{
		yield return InviteManager.HandleSessionIdInvite(_invite, _password, _onFinished);
		yield break;
	}

	public string GetListenerIdentifier()
	{
		return "DCD";
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static DiscordInviteListener listenerInstance;

	[TupleElementNames(new string[] { "session", "password" })]
	[PublicizedFrom(EAccessModifier.Private)]
	public ValueTuple<string, string>? pendingActivityInvite;
}
