using System;
using UnityEngine.Scripting;

namespace Platform.XBL
{
	[Preserve]
	[DoNotTouchSerializableFlags]
	[Serializable]
	public class UserIdentifierXbl : PlatformUserIdentifierAbs
	{
		public override EPlatformIdentifier PlatformIdentifier
		{
			get
			{
				return EPlatformIdentifier.XBL;
			}
		}

		public override string PlatformIdentifierString { get; } = PlatformManager.PlatformStringFromEnum(EPlatformIdentifier.XBL);

		public override string ReadablePlatformUserIdentifier { get; }

		public override string CombinedString { get; }

		public ulong Xuid
		{
			get
			{
				return XblXuidMapper.GetXuid(this);
			}
		}

		public UserIdentifierXbl(string _pxuid)
		{
			this.pxuid = _pxuid;
			this.ReadablePlatformUserIdentifier = _pxuid;
			this.CombinedString = this.PlatformIdentifierString + "_" + _pxuid;
			this.hashcode = _pxuid.GetHashCode() ^ ((int)this.PlatformIdentifier * 397);
		}

		public override bool DecodeTicket(string _ticket)
		{
			return true;
		}

		public override bool Equals(PlatformUserIdentifierAbs _other)
		{
			if (_other == null)
			{
				return false;
			}
			if (this == _other)
			{
				return true;
			}
			UserIdentifierXbl userIdentifierXbl = _other as UserIdentifierXbl;
			return userIdentifierXbl != null && string.Equals(userIdentifierXbl.pxuid, this.pxuid, StringComparison.OrdinalIgnoreCase);
		}

		public override int GetHashCode()
		{
			return this.hashcode;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly string pxuid;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly int hashcode;
	}
}
