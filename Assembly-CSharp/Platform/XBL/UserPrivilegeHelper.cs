using System;
using System.Collections;
using System.Linq;
using Unity.XGamingRuntime;

namespace Platform.XBL
{
	public class UserPrivilegeHelper
	{
		public UserPrivilegeHelper(XUserHandle userHandle)
		{
			this.AllAllowed = new PrivilegeState[]
			{
				this.Multiplayer = new PrivilegeState(userHandle, 254),
				this.Communications = new PrivilegeState(userHandle, 252),
				this.CrossPlay = new PrivilegeState(userHandle, 185),
				this.UserGeneratedContent = new PrivilegeState(userHandle, 247)
			};
			this.MultiplayerAllowed = new PrivilegeState[] { this.Multiplayer, this.UserGeneratedContent };
			this.CommunicationAllowed = new PrivilegeState[] { this.Communications };
			this.CrossPlayAllowed = new PrivilegeState[] { this.CrossPlay };
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static IEnumerator ResolveAllowed(bool canPrompt, CoroutineCancellationToken _cancellationToken = null, params PrivilegeState[] privilegeStates)
		{
			if (!canPrompt)
			{
				privilegeStates.ResolveSilent();
				return Enumerable.Empty<object>().GetEnumerator();
			}
			return privilegeStates.ResolveWithPrompt(_cancellationToken);
		}

		public IEnumerator ResolvePermissions(EUserPerms _perms, bool _canPrompt, CoroutineCancellationToken _cancellationToken = null)
		{
			if (_perms.HasMultiplayer() || _perms.HasHostMultiplayer())
			{
				yield return UserPrivilegeHelper.ResolveAllowed(_canPrompt, _cancellationToken, this.MultiplayerAllowed);
				if (_cancellationToken != null && _cancellationToken.IsCancelled())
				{
					yield break;
				}
			}
			if (_perms.HasCommunication())
			{
				yield return UserPrivilegeHelper.ResolveAllowed(_canPrompt, _cancellationToken, this.CommunicationAllowed);
				if (_cancellationToken != null && _cancellationToken.IsCancelled())
				{
					yield break;
				}
			}
			if (_perms.HasCrossplay())
			{
				yield return UserPrivilegeHelper.ResolveAllowed(_canPrompt, _cancellationToken, this.CrossPlayAllowed);
				if (_cancellationToken != null)
				{
					_cancellationToken.IsCancelled();
				}
				yield break;
			}
			yield break;
		}

		public readonly PrivilegeState Multiplayer;

		public readonly PrivilegeState Communications;

		public readonly PrivilegeState CrossPlay;

		public readonly PrivilegeState UserGeneratedContent;

		public readonly PrivilegeState[] AllAllowed;

		public readonly PrivilegeState[] MultiplayerAllowed;

		public readonly PrivilegeState[] CommunicationAllowed;

		public readonly PrivilegeState[] CrossPlayAllowed;
	}
}
