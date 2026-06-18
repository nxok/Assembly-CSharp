using System;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Unity.XGamingRuntime;

namespace Platform.XBL
{
	[PublicizedFrom(EAccessModifier.Internal)]
	public class JoinSessionGameInviteListener : IJoinSessionGameInviteListener
	{
		public void Init(IPlatform _owner)
		{
			PlatformManager.NativePlatform.User.UserLoggedIn += delegate(IPlatform _platform)
			{
				XGameInviteRegistrationToken xgameInviteRegistrationToken;
				XblHelpers.Succeeded(SDK.XGameInviteRegisterForEvent(new XGameInviteEventCallback(this.InviteReceivedCallback), ref xgameInviteRegistrationToken), "Register for invite event", true, true);
			};
		}

		[return: TupleElementNames(new string[] { "invite", "password" })]
		public ValueTuple<string, string> TakePendingInvite()
		{
			object obj = this.inviteLock;
			ValueTuple<string, string> valueTuple;
			lock (obj)
			{
				string text = this.pendingInvite;
				this.pendingInvite = null;
				string text2 = this.pendingPassword;
				this.pendingPassword = null;
				valueTuple = new ValueTuple<string, string>(text, text2);
			}
			return valueTuple;
		}

		public IEnumerator ConnectToInvite(string _invite, string _password = null, Action<bool> _onFinished = null)
		{
			yield return InviteManager.HandleSessionIdInvite(_invite, _password, _onFinished);
			yield break;
		}

		public string GetListenerIdentifier()
		{
			return "XBL";
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void InviteReceivedCallback(IntPtr _, string _inviteuri)
		{
			Log.Out("[XBL] Invite received: '" + _inviteuri + "'");
			string text = this.ParseInviteUri(_inviteuri);
			if (text == null)
			{
				Log.Error("[XBL] Received invite but could not extract connect information");
				return;
			}
			object obj = this.inviteLock;
			lock (obj)
			{
				this.pendingInvite = text;
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public string ParseInviteUri(string _inviteUri)
		{
			Match match = JoinSessionGameInviteListener.msInviteUriMatcher.Match(_inviteUri);
			if (!match.Success)
			{
				return null;
			}
			string[] array = match.Groups[3].Value.Split('&', StringSplitOptions.None);
			for (int i = 0; i < array.Length; i++)
			{
				string[] array2 = array[i].Split('=', StringSplitOptions.None);
				if (array2[0].EqualsCaseInsensitive("connectionString"))
				{
					return array2[1];
				}
			}
			return null;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public object inviteLock = new object();

		[PublicizedFrom(EAccessModifier.Private)]
		public string pendingInvite;

		[PublicizedFrom(EAccessModifier.Private)]
		public string pendingPassword;

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly Regex msInviteUriMatcher = new Regex("^ms-xbl-(\\w+):\\/\\/(\\w+)\\/?\\?(.*)$", RegexOptions.Compiled);
	}
}
