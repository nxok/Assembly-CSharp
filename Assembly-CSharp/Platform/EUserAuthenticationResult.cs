using System;

namespace Platform
{
	public enum EUserAuthenticationResult
	{
		Ok,
		UserNotConnectedToPlatform,
		NoLicenseOrExpired,
		PlatformBanned,
		LoggedInElseWhere,
		PlatformBanCheckTimedOut,
		AuthTicketCanceled,
		AuthTicketInvalidAlreadyUsed,
		AuthTicketInvalid,
		PublisherIssuedBan,
		EosTicketFailed = 50
	}
}
