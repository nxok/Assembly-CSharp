using System;

namespace Platform
{
	public class UserDataRoamingMultiPlatform : UserDataRoamingAbs
	{
		public override SaveRoamingMode SaveRoamingMode
		{
			get
			{
				if (GameManager.IsDedicatedServer)
				{
					return SaveRoamingMode.None;
				}
				IUserDataRoaming userDataRoaming = PlatformManager.NativePlatform.UserDataRoaming;
				if (userDataRoaming == null)
				{
					return SaveRoamingMode.None;
				}
				return userDataRoaming.SaveRoamingMode;
			}
		}

		public override UserDataStorageType DefaultSaveStorage
		{
			get
			{
				if (GameManager.IsDedicatedServer)
				{
					return UserDataStorageType.DeviceLocal;
				}
				IUserDataRoaming userDataRoaming = PlatformManager.NativePlatform.UserDataRoaming;
				if (userDataRoaming == null)
				{
					return UserDataStorageType.DeviceLocal;
				}
				return userDataRoaming.DefaultSaveStorage;
			}
		}
	}
}
