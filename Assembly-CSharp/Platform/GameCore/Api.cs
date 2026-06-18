using System;
using Platform.XBL;

namespace Platform.GameCore
{
	public class Api : XblPlatformApi
	{
		public override string SCID
		{
			get
			{
				return this.scid;
			}
		}

		public override void Init(IPlatform owner)
		{
			Log.Out("[XBL] API Startup. SCID: " + this.SCID);
		}

		public override bool InitServerApis()
		{
			return false;
		}

		public override void ServerApiLoaded()
		{
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public string scid;
	}
}
