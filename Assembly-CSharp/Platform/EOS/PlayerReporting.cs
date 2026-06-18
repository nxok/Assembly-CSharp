using System;
using System.Collections.Generic;
using Epic.OnlineServices;
using Epic.OnlineServices.Reports;

namespace Platform.EOS
{
	public class PlayerReporting : IPlayerReporting
	{
		public ReportsInterface reportsInterface
		{
			[PublicizedFrom(EAccessModifier.Private)]
			get
			{
				return ((Api)this.owner.Api).PlatformInterface.GetReportsInterface();
			}
		}

		public ProductUserId localProductUserId
		{
			[PublicizedFrom(EAccessModifier.Private)]
			get
			{
				return ((UserIdentifierEos)this.owner.User.PlatformUserId).ProductUserId;
			}
		}

		public void Init(IPlatform _owner)
		{
			this.owner = _owner;
		}

		public IList<IPlayerReporting.PlayerReportCategory> ReportCategories()
		{
			if (this.reportCategories != null)
			{
				return this.reportCategories.list;
			}
			this.reportCategories = new DictionaryList<PlayerReportsCategory, IPlayerReporting.PlayerReportCategory>();
			foreach (PlayerReportsCategory playerReportsCategory in EnumUtils.Values<PlayerReportsCategory>())
			{
				if (playerReportsCategory != null)
				{
					this.reportCategories.Add(playerReportsCategory, new PlayerReporting.PlayerReportCategoryEos(playerReportsCategory, Localization.Get("xuiCategoryPlayerReport" + playerReportsCategory.ToStringCached<PlayerReportsCategory>(), false, null)));
				}
			}
			return this.reportCategories.list;
		}

		public void ReportPlayer(PlatformUserIdentifierAbs _reportedUserCross, IPlayerReporting.PlayerReportCategory _reportCategory, string _message, Action<bool> _reportCompleteCallback)
		{
			if (_message != null && _message.Length > 256)
			{
				Log.Out("[EOS-Report] Long message, might get truncated");
			}
			EosHelpers.AssertMainThread("PRep.Send");
			SendPlayerBehaviorReportOptions sendPlayerBehaviorReportOptions = default(SendPlayerBehaviorReportOptions);
			sendPlayerBehaviorReportOptions.ReporterUserId = this.localProductUserId;
			sendPlayerBehaviorReportOptions.ReportedUserId = ((UserIdentifierEos)_reportedUserCross).ProductUserId;
			sendPlayerBehaviorReportOptions.Category = ((PlayerReporting.PlayerReportCategoryEos)_reportCategory).Category;
			sendPlayerBehaviorReportOptions.Message = _message;
			SendPlayerBehaviorReportOptions sendPlayerBehaviorReportOptions2 = sendPlayerBehaviorReportOptions;
			object lockObject = AntiCheatCommon.LockObject;
			lock (lockObject)
			{
				this.reportsInterface.SendPlayerBehaviorReport(ref sendPlayerBehaviorReportOptions2, null, delegate(ref SendPlayerBehaviorReportCompleteCallbackInfo _callbackData)
				{
					if (_callbackData.ResultCode != null)
					{
						Log.Error("[EOS-Report] Reporting player failed: " + _callbackData.ResultCode.ToStringCached<Result>());
						_reportCompleteCallback(false);
						return;
					}
					Log.Out("[EOS-Report] Sent player report");
					_reportCompleteCallback(true);
				});
			}
		}

		public IPlayerReporting.PlayerReportCategory GetPlayerReportCategoryMapping(EnumReportCategory _reportCategory)
		{
			PlayerReportsCategory playerReportsCategory;
			if (_reportCategory != EnumReportCategory.Cheating)
			{
				if (_reportCategory != EnumReportCategory.VerbalAbuse)
				{
					playerReportsCategory = 7;
				}
				else
				{
					playerReportsCategory = 4;
				}
			}
			else
			{
				playerReportsCategory = 1;
			}
			PlayerReportsCategory playerReportsCategory2 = playerReportsCategory;
			IPlayerReporting.PlayerReportCategory playerReportCategory;
			if (!this.reportCategories.dict.TryGetValue(playerReportsCategory2, out playerReportCategory))
			{
				return null;
			}
			return playerReportCategory;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public IPlatform owner;

		[PublicizedFrom(EAccessModifier.Private)]
		public DictionaryList<PlayerReportsCategory, IPlayerReporting.PlayerReportCategory> reportCategories;

		[PublicizedFrom(EAccessModifier.Private)]
		public class PlayerReportCategoryEos : IPlayerReporting.PlayerReportCategory
		{
			public PlayerReportCategoryEos(PlayerReportsCategory _category, string _displayString)
			{
				this.Category = _category;
				this.displayString = _displayString;
			}

			public override string ToString()
			{
				return this.displayString;
			}

			public readonly PlayerReportsCategory Category;

			[PublicizedFrom(EAccessModifier.Private)]
			public readonly string displayString;
		}
	}
}
