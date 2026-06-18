using System;

namespace Platform.GameCore
{
	public class UserDataRoamingGameCore : UserDataRoamingAbs
	{
		public override SaveRoamingMode SaveRoamingMode
		{
			get
			{
				return SaveRoamingMode.Forced;
			}
		}

		public override UserDataStorageType DefaultSaveStorage
		{
			get
			{
				return UserDataStorageType.Roaming;
			}
		}
	}
}
