using System;
using UnityEngine.Scripting;

namespace Platform.Local
{
	[Preserve]
	[DoNotTouchSerializableFlags]
	[Serializable]
	public class UserIdentifierLocal : PlatformUserIdentifierAbs
	{
		public override EPlatformIdentifier PlatformIdentifier
		{
			get
			{
				return EPlatformIdentifier.Local;
			}
		}

		public override string PlatformIdentifierString { get; } = PlatformManager.PlatformStringFromEnum(EPlatformIdentifier.Local);

		public override string ReadablePlatformUserIdentifier { get; }

		public override string CombinedString { get; }

		public UserIdentifierLocal(string _playername)
		{
			if (string.IsNullOrEmpty(_playername))
			{
				throw new ArgumentException("Playername must not be empty", "_playername");
			}
			this.PlayerName = _playername;
			this.ReadablePlatformUserIdentifier = _playername;
			this.CombinedString = this.PlatformIdentifierString + "_" + _playername;
			this.hashcode = _playername.GetHashCode() ^ ((int)this.PlatformIdentifier * 397);
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
			UserIdentifierLocal userIdentifierLocal = _other as UserIdentifierLocal;
			return userIdentifierLocal != null && userIdentifierLocal.PlayerName == this.PlayerName;
		}

		public override int GetHashCode()
		{
			return this.hashcode;
		}

		public readonly string PlayerName;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly int hashcode;
	}
}
