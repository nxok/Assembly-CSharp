using System;

public interface IGamePrefsChangedListener
{
	void OnGamePrefChanged(EnumGamePrefs _enum);
}
