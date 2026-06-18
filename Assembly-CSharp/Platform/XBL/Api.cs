using System;

namespace Platform.XBL
{
	public class Api : XblPlatformApi
	{
		public override string SCID
		{
			get
			{
				return "00000000-0000-0000-0000-0000680ee616";
			}
		}

		public override void Init(IPlatform _owner)
		{
		}

		public override bool InitServerApis()
		{
			return true;
		}

		public override void ServerApiLoaded()
		{
		}

		public const string s_scid = "00000000-0000-0000-0000-0000680ee616";

		public const int TitleId = 1745806870;
	}
}
