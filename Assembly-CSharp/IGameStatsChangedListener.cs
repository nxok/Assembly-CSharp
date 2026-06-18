using System;

public interface IGameStatsChangedListener
{
	void OnGameStatChanged(EnumGameStats _enum);
}
