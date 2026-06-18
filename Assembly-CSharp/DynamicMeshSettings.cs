using System;

public static class DynamicMeshSettings
{
	public static bool UseImposterValues { get; set; } = true;

	public static bool OnlyPlayerAreas { get; set; } = false;

	public static int PlayerAreaChunkBuffer { get; set; } = 3;

	public static int MaxViewDistance
	{
		get
		{
			return DynamicMeshSettings._maxViewDistance;
		}
		set
		{
			DynamicMeshSettings._maxViewDistance = Math.Min(3000, value);
			PrefabLODManager.lodPoiDistance = DynamicMeshSettings._maxViewDistance;
		}
	}

	public static bool NewWorldFullRegen { get; set; } = false;

	[PublicizedFrom(EAccessModifier.Private)]
	static DynamicMeshSettings()
	{
		GamePrefs.OnGamePrefChanged += DynamicMeshSettings.OnGamePrefChanged;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void OnGamePrefChanged(EnumGamePrefs _pref)
	{
		if (_pref == EnumGamePrefs.DynamicMeshDistance)
		{
			DynamicMeshSettings.MaxViewDistance = GamePrefs.GetInt(_pref);
		}
	}

	public static void LogSettings()
	{
		Log.Out("Dynamic Mesh Settings");
		Log.Out("Use Imposter Values: " + DynamicMeshSettings.UseImposterValues.ToString());
		Log.Out("Only Player Areas: " + DynamicMeshSettings.OnlyPlayerAreas.ToString());
		Log.Out("Player Area Buffer: " + DynamicMeshSettings.PlayerAreaChunkBuffer.ToString());
		Log.Out("Max View Distance: " + DynamicMeshSettings.MaxViewDistance.ToString());
		Log.Out("Regen all on new world: " + DynamicMeshSettings.NewWorldFullRegen.ToString());
	}

	public static void Validate()
	{
	}

	public static int MaxRegionMeshData = 1;

	public static int MaxRegionLoadMsPerFrame = 2;

	public static int MaxDyMeshData = 3;

	[PublicizedFrom(EAccessModifier.Private)]
	public static int _maxViewDistance = 1000;
}
