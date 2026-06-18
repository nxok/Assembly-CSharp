using System;
using System.Collections;
using System.Runtime.CompilerServices;

namespace Platform
{
	public interface IJoinSessionGameInviteListener
	{
		void Init(IPlatform _owner);

		[return: TupleElementNames(new string[] { "invite", "password" })]
		ValueTuple<string, string> TakePendingInvite();

		IEnumerator ConnectToInvite(string _invite, string _password = null, Action<bool> _onFinished = null);

		string GetListenerIdentifier();
	}
}
