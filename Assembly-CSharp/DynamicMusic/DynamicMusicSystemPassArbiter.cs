using System;
using UnityEngine.Scripting;

namespace DynamicMusic
{
	[Preserve]
	public struct DynamicMusicSystemPassArbiter : IPassArbiter, IGamePrefsChangedListener, IPauseable
	{
		public bool WillAllowPass
		{
			get
			{
				return this.BoolContainer.Equals(224);
			}
		}

		public bool DoesPlayerExist
		{
			set
			{
				this.SetBoolContainer(value, 64);
			}
		}

		public bool IsGameUnPaused
		{
			[PublicizedFrom(EAccessModifier.Private)]
			set
			{
				this.SetBoolContainer(value, 32);
			}
		}

		public bool IsDynamicMusicEnabled
		{
			set
			{
				this.SetBoolContainer(value, 128);
			}
		}

		public DynamicMusicSystemPassArbiter(bool _enabled)
		{
			this.BoolContainer = 0;
			this.IsDynamicMusicEnabled = _enabled;
			GamePrefs.AddChangeListener(this);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void SetBoolContainer(bool _value, byte _place)
		{
			if (_value)
			{
				this.BoolContainer |= _place;
				return;
			}
			this.BoolContainer &= ~_place;
		}

		public void OnGamePrefChanged(EnumGamePrefs _enum)
		{
			if (_enum == EnumGamePrefs.OptionsDynamicMusicEnabled)
			{
				this.IsDynamicMusicEnabled = GamePrefs.GetBool(_enum);
			}
		}

		public void OnPause()
		{
			this.IsGameUnPaused = false;
		}

		public void OnUnPause()
		{
			this.IsGameUnPaused = true;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public byte BoolContainer;
	}
}
