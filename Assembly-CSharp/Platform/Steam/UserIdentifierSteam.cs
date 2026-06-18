using System;
using Steamworks;
using UnityEngine.Scripting;

namespace Platform.Steam
{
	[Preserve]
	[DoNotTouchSerializableFlags]
	[Serializable]
	public class UserIdentifierSteam : PlatformUserIdentifierAbs
	{
		public override EPlatformIdentifier PlatformIdentifier
		{
			get
			{
				return EPlatformIdentifier.Steam;
			}
		}

		public override string PlatformIdentifierString { get; } = PlatformManager.PlatformStringFromEnum(EPlatformIdentifier.Steam);

		public override string ReadablePlatformUserIdentifier { get; }

		public override string CombinedString { get; }

		public byte[] Ticket
		{
			get
			{
				return this.ticket;
			}
			[PublicizedFrom(EAccessModifier.Private)]
			set
			{
				this.ticket = value;
			}
		}

		public UserIdentifierSteam(string _steamId)
		{
			ulong num;
			if (_steamId.Length != 17 || !ulong.TryParse(_steamId, out num))
			{
				throw new ArgumentException("Not a valid SteamID: " + _steamId, "_steamId");
			}
			this.SteamId = num;
			this.ReadablePlatformUserIdentifier = _steamId;
			this.CombinedString = this.PlatformIdentifierString + "_" + _steamId;
			this.hashcode = num.GetHashCode() ^ ((int)this.PlatformIdentifier * 397);
		}

		public UserIdentifierSteam(ulong _steamId)
		{
			if (_steamId < 10000000000000000UL || _steamId > 99999999999999999UL)
			{
				throw new ArgumentException("Not a valid SteamID: " + _steamId.ToString(), "_steamId");
			}
			this.SteamId = _steamId;
			this.ReadablePlatformUserIdentifier = _steamId.ToString();
			this.CombinedString = this.PlatformIdentifierString + "_" + _steamId.ToString();
			this.hashcode = _steamId.GetHashCode() ^ ((int)this.PlatformIdentifier * 397);
		}

		public UserIdentifierSteam(CSteamID _steamId)
		{
			this.SteamId = _steamId.m_SteamID;
			this.ReadablePlatformUserIdentifier = _steamId.ToString();
			string platformIdentifierString = this.PlatformIdentifierString;
			string text = "_";
			CSteamID csteamID = _steamId;
			this.CombinedString = platformIdentifierString + text + csteamID.ToString();
			this.hashcode = _steamId.m_SteamID.GetHashCode() ^ ((int)this.PlatformIdentifier * 397);
		}

		public override bool DecodeTicket(string _ticket)
		{
			if (string.IsNullOrEmpty(_ticket))
			{
				return false;
			}
			try
			{
				this.Ticket = Convert.FromBase64String(_ticket);
			}
			catch (FormatException ex)
			{
				Log.Error("Convert.FromBase64String: " + ex.Message);
				Log.Exception(ex);
				return false;
			}
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
			UserIdentifierSteam userIdentifierSteam = _other as UserIdentifierSteam;
			return userIdentifierSteam != null && userIdentifierSteam.SteamId == this.SteamId;
		}

		public override int GetHashCode()
		{
			return this.hashcode;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		[NonSerialized]
		public byte[] ticket;

		public UserIdentifierSteam OwnerId;

		public readonly ulong SteamId;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly int hashcode;
	}
}
