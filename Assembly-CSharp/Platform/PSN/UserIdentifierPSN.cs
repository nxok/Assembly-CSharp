using System;
using UnityEngine.Scripting;

namespace Platform.PSN
{
	[Preserve]
	[DoNotTouchSerializableFlags]
	[Serializable]
	public class UserIdentifierPSN : PlatformUserIdentifierAbs
	{
		public override EPlatformIdentifier PlatformIdentifier
		{
			get
			{
				return EPlatformIdentifier.PSN;
			}
		}

		public override string PlatformIdentifierString { get; } = PlatformManager.PlatformStringFromEnum(EPlatformIdentifier.PSN);

		public override string ReadablePlatformUserIdentifier { get; }

		public override string CombinedString { get; }

		public ulong AccountId
		{
			get; [PublicizedFrom(EAccessModifier.Private)]
			set;
		}

		public UserIdentifierPSN(ulong _accountId)
		{
			this.AccountId = _accountId;
			this.ReadablePlatformUserIdentifier = this.AccountId.ToString();
			this.CombinedString = this.PlatformIdentifierString + "_" + this.ReadablePlatformUserIdentifier;
		}

		public override bool DecodeTicket(string _ticket)
		{
			return true;
		}

		public override bool Equals(PlatformUserIdentifierAbs _other)
		{
			return _other != null && (this == _other || (_other is UserIdentifierPSN && this.AccountId == (_other as UserIdentifierPSN).AccountId));
		}

		public override int GetHashCode()
		{
			return this.AccountId.GetHashCode();
		}
	}
}
