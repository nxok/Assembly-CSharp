using System;

namespace Platform
{
	public interface IUserDataRoaming
	{
		void Init(IPlatform platform);

		bool IsSupported { get; }

		SaveRoamingMode SaveRoamingMode { get; }

		bool SaveRoamingEnabled { get; }

		bool IsRoamingOptional { get; }

		UserDataStorageType DefaultSaveStorage { get; }

		void ValidateRoamingMode();
	}
}
