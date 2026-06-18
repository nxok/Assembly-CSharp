using System;
using System.Collections;
using Unity.XGamingRuntime;
using Unity.XGamingRuntime.Interop;

namespace Platform.XBL
{
	public class PrivilegeState
	{
		public PrivilegeState(XUserHandle userHandle, XUserPrivilege privilege)
		{
			this.m_userHandle = userHandle;
			this.m_privilege = privilege;
			this.m_has = false;
			this.m_denyReason = -1;
		}

		public bool Has
		{
			get
			{
				return this.m_has;
			}
		}

		public XUserPrivilegeDenyReason DenyReason
		{
			get
			{
				return this.m_denyReason;
			}
		}

		public void ResolveSilent()
		{
			int num = SDK.XUserCheckPrivilege(this.m_userHandle, 0, this.m_privilege, ref this.m_has, ref this.m_denyReason);
			XblHelpers.LogHR(num, string.Format("{0} checked privilege '{1}' = {2} ({3})", new object[] { "XUserCheckPrivilege", this.m_privilege, this.m_has, this.m_denyReason }), false);
			if (HR.SUCCEEDED(num))
			{
				return;
			}
			this.m_has = false;
			this.m_denyReason = -1;
		}

		public IEnumerator ResolveWithPrompt(CoroutineCancellationToken _cancellationToken = null)
		{
			this.ResolveSilent();
			if (this.m_has)
			{
				yield break;
			}
			bool uiOpen = true;
			SDK.XUserResolvePrivilegeWithUiAsync(this.m_userHandle, 0, this.m_privilege, delegate(int hr)
			{
				CoroutineCancellationToken cancellationToken2 = _cancellationToken;
				if (cancellationToken2 != null && cancellationToken2.IsCancelled())
				{
					return;
				}
				try
				{
					XblHelpers.LogHR(hr, "XUserResolvePrivilegeWithUiCompleted", false);
				}
				finally
				{
					uiOpen = false;
				}
			});
			while (uiOpen)
			{
				CoroutineCancellationToken cancellationToken = _cancellationToken;
				if (cancellationToken != null && cancellationToken.IsCancelled())
				{
					yield break;
				}
				yield return null;
			}
			this.ResolveSilent();
			yield break;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly XUserHandle m_userHandle;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly XUserPrivilege m_privilege;

		[PublicizedFrom(EAccessModifier.Private)]
		public bool m_has;

		[PublicizedFrom(EAccessModifier.Private)]
		public XUserPrivilegeDenyReason m_denyReason;
	}
}
