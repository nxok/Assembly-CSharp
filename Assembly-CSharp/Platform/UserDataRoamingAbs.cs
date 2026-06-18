using System;

namespace Platform
{
	public abstract class UserDataRoamingAbs : IUserDataRoaming
	{
		public void Init(IPlatform platform)
		{
			this.platform = platform;
		}

		public bool IsSupported
		{
			get
			{
				return this.platform.SaveGameProvider != null;
			}
		}

		public abstract SaveRoamingMode SaveRoamingMode { get; }

		public bool SaveRoamingEnabled
		{
			get
			{
				return this.IsSupported && (this.SaveRoamingMode == SaveRoamingMode.Optional || this.SaveRoamingMode == SaveRoamingMode.Forced);
			}
		}

		public bool IsRoamingOptional
		{
			get
			{
				return this.SaveRoamingMode == SaveRoamingMode.Optional;
			}
		}

		public abstract UserDataStorageType DefaultSaveStorage { get; }

		public void ValidateRoamingMode()
		{
			this.ValidateStoragePref(EnumGamePrefs.GameSaveStorageType);
			this.ValidateStoragePref(EnumGamePrefs.UserWorldStorageType);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void ValidateStoragePref(EnumGamePrefs pref)
		{
			UserDataStorageType @int = (UserDataStorageType)GamePrefs.GetInt(pref);
			UserDataStorageType userDataStorageType = @int;
			SaveRoamingMode saveRoamingMode = this.SaveRoamingMode;
			if (saveRoamingMode != SaveRoamingMode.None)
			{
				if (saveRoamingMode == SaveRoamingMode.Forced)
				{
					userDataStorageType = UserDataStorageType.Roaming;
				}
			}
			else
			{
				userDataStorageType = UserDataStorageType.DeviceLocal;
			}
			if (@int != userDataStorageType)
			{
				Log.Out(string.Format("UserDataRoaming invalid storage pref {0} configured for {1}. Platform roaming mode is {2}. Changing to {3}", new object[]
				{
					@int,
					pref.ToStringCached<EnumGamePrefs>(),
					this.SaveRoamingMode,
					userDataStorageType
				}));
				GamePrefs.Set(pref, (int)userDataStorageType);
			}
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public UserDataRoamingAbs()
		{
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public IPlatform platform;
	}
}
