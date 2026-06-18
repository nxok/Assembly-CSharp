using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdSpectrum : ConsoleCmdAbstract
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[] { "spectrum" };
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Force a particular lighting spectrum.";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getHelp()
	{
		return "spectrum <Auto, Biome, BloodMoon, Foggy, Rainy, Stormy, Snowy>\n";
	}

	public override bool IsExecuteOnClient
	{
		get
		{
			return true;
		}
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		if (GameManager.IsDedicatedServer)
		{
			return;
		}
		if (_params.Count == 0)
		{
			if (WeatherManager.forcedSpectrum != SpectrumWeatherType.None)
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("forced " + WeatherManager.forcedSpectrum.ToString());
				return;
			}
			if (WeatherManager.Instance == null)
			{
				return;
			}
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output(WeatherManager.Instance.GetSpectrumInfo());
			return;
		}
		else
		{
			if (_params.Count != 1)
			{
				return;
			}
			if (_params[0].EqualsCaseInsensitive("Snowy"))
			{
				WeatherManager.SetForceSpectrum(SpectrumWeatherType.Snowy);
				return;
			}
			if (_params[0].EqualsCaseInsensitive("Rainy"))
			{
				WeatherManager.SetForceSpectrum(SpectrumWeatherType.Rainy);
				return;
			}
			if (_params[0].EqualsCaseInsensitive("Stormy"))
			{
				WeatherManager.SetForceSpectrum(SpectrumWeatherType.Stormy);
				return;
			}
			if (_params[0].EqualsCaseInsensitive("Foggy"))
			{
				WeatherManager.SetForceSpectrum(SpectrumWeatherType.Foggy);
				return;
			}
			if (_params[0].EqualsCaseInsensitive("BloodMoon"))
			{
				WeatherManager.SetForceSpectrum(SpectrumWeatherType.BloodMoon);
				return;
			}
			if (_params[0].EqualsCaseInsensitive("Biome"))
			{
				WeatherManager.SetForceSpectrum(SpectrumWeatherType.Biome);
				return;
			}
			if (_params[0].EqualsCaseInsensitive("Auto"))
			{
				WeatherManager.SetForceSpectrum(SpectrumWeatherType.None);
				return;
			}
			return;
		}
	}
}
