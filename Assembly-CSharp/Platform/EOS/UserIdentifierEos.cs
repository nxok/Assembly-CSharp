using System;
using System.Text.RegularExpressions;
using Epic.OnlineServices;
using UnityEngine;
using UnityEngine.Scripting;

namespace Platform.EOS
{
	[Preserve]
	[DoNotTouchSerializableFlags]
	[Serializable]
	public class UserIdentifierEos : PlatformUserIdentifierAbs
	{
		public override EPlatformIdentifier PlatformIdentifier
		{
			get
			{
				return EPlatformIdentifier.EOS;
			}
		}

		public override string PlatformIdentifierString { get; } = PlatformManager.PlatformStringFromEnum(EPlatformIdentifier.EOS);

		public override string ReadablePlatformUserIdentifier
		{
			get
			{
				return this.ProductUserIdString;
			}
		}

		public override string CombinedString { get; }

		public static string CreateCombinedString(string _puidString)
		{
			return PlatformManager.PlatformStringFromEnum(EPlatformIdentifier.EOS) + "_" + _puidString;
		}

		public static string CreateCombinedString(ProductUserId _puid)
		{
			return PlatformManager.PlatformStringFromEnum(EPlatformIdentifier.EOS) + "_" + UserIdentifierEos.CreateStringFromPuid(_puid);
		}

		public string ProductUserIdString
		{
			get
			{
				string text;
				if ((text = this.productUserIdString) == null)
				{
					text = (this.productUserIdString = UserIdentifierEos.CreateStringFromPuid(this.productUserId));
				}
				return text;
			}
		}

		public ProductUserId ProductUserId
		{
			get
			{
				ProductUserId productUserId;
				if ((productUserId = this.productUserId) == null)
				{
					productUserId = (this.productUserId = UserIdentifierEos.CreatePuidFromString(this.productUserIdString));
				}
				return productUserId;
			}
		}

		public string Ticket
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

		public UserIdentifierEos(string _puid)
		{
			if (string.IsNullOrEmpty(_puid))
			{
				throw new ArgumentException("Empty or null PUID", "_puid");
			}
			if (!UserIdentifierEos.puidMatcher.IsMatch(_puid))
			{
				throw new ArgumentException("Invalid PUID '" + _puid + "'", "_puid");
			}
			this.productUserIdString = _puid;
			this.CombinedString = UserIdentifierEos.CreateCombinedString(_puid);
			this.hashcode = this.ProductUserIdString.GetHashCode() ^ ((int)this.PlatformIdentifier * 397);
		}

		public UserIdentifierEos(ProductUserId _puid)
		{
			if (_puid == null)
			{
				throw new ArgumentException("Null PUID", "_puid");
			}
			this.productUserId = _puid;
			this.CombinedString = UserIdentifierEos.CreateCombinedString(this.ProductUserIdString);
			this.hashcode = this.ProductUserIdString.GetHashCode() ^ ((int)this.PlatformIdentifier * 397);
		}

		public static string CreateStringFromPuid(ProductUserId _puid)
		{
			if (!ThreadManager.IsMainThread())
			{
				Log.Warning("CreateStringFromPuid NOT ON MAIN THREAD! From:\n" + StackTraceUtility.ExtractStackTrace() + "\n");
			}
			if (_puid == null)
			{
				Log.Error("CreateStringFromPuid with null PUID! From:\n" + StackTraceUtility.ExtractStackTrace() + "\n");
				return null;
			}
			return _puid.ToString();
		}

		public static ProductUserId CreatePuidFromString(string _puidString)
		{
			if (_puidString == null)
			{
				Log.Error("CreatePuidFromString with null PUID string! From:\n" + StackTraceUtility.ExtractStackTrace() + "\n");
				return null;
			}
			return ProductUserId.FromString(_puidString);
		}

		public override bool DecodeTicket(string _ticket)
		{
			if (string.IsNullOrEmpty(_ticket))
			{
				return false;
			}
			this.Ticket = _ticket;
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
			UserIdentifierEos userIdentifierEos = _other as UserIdentifierEos;
			return userIdentifierEos != null && string.Equals(userIdentifierEos.ProductUserIdString, this.ProductUserIdString, StringComparison.Ordinal);
		}

		public override int GetHashCode()
		{
			return this.hashcode;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly Regex puidMatcher = new Regex("^[0-9a-fA-F]{8,32}$", RegexOptions.Compiled);

		[PublicizedFrom(EAccessModifier.Private)]
		[NonSerialized]
		public string ticket;

		[PublicizedFrom(EAccessModifier.Private)]
		public string productUserIdString;

		[PublicizedFrom(EAccessModifier.Private)]
		[NonSerialized]
		public ProductUserId productUserId;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly int hashcode;
	}
}
