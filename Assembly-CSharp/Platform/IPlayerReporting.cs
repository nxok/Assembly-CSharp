using System;
using System.Collections.Generic;

namespace Platform
{
	public interface IPlayerReporting
	{
		void Init(IPlatform _owner);

		IList<IPlayerReporting.PlayerReportCategory> ReportCategories();

		void ReportPlayer(PlatformUserIdentifierAbs _reportedUserCross, IPlayerReporting.PlayerReportCategory _reportCategory, string _message, Action<bool> _reportCompleteCallback);

		IPlayerReporting.PlayerReportCategory GetPlayerReportCategoryMapping(EnumReportCategory _reportCategory);

		public abstract class PlayerReportCategory
		{
			public abstract override string ToString();

			[PublicizedFrom(EAccessModifier.Protected)]
			public PlayerReportCategory()
			{
			}
		}
	}
}
