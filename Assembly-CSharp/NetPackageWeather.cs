using System;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageWeather : NetPackage
{
	public override NetPackageDirection PackageDirection
	{
		get
		{
			return NetPackageDirection.ToClient;
		}
	}

	public NetPackageWeather Setup(WeatherPackage[] _packages)
	{
		this.weatherPackages = _packages;
		return this;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void InitPackages()
	{
		if (WeatherManager.Instance)
		{
			int count = WeatherManager.Instance.biomeWeather.Count;
			this.weatherPackages = new WeatherPackage[count];
			for (int i = 0; i < count; i++)
			{
				this.weatherPackages[i] = new WeatherPackage();
			}
		}
	}

	public override void read(PooledBinaryReader _br)
	{
		if (this.weatherPackages == null)
		{
			this.InitPackages();
			if (this.weatherPackages == null)
			{
				return;
			}
		}
		for (int i = 0; i < this.weatherPackages.Length; i++)
		{
			WeatherPackage weatherPackage = this.weatherPackages[i];
			weatherPackage.biomeId = _br.ReadByte();
			weatherPackage.groupIndex = _br.ReadByte();
			weatherPackage.remainingSeconds = _br.ReadByte();
			for (int j = 0; j < weatherPackage.param.Length; j++)
			{
				weatherPackage.param[j] = _br.ReadSingle();
			}
		}
	}

	public override void write(PooledBinaryWriter _bw)
	{
		base.write(_bw);
		for (int i = 0; i < this.weatherPackages.Length; i++)
		{
			WeatherPackage weatherPackage = this.weatherPackages[i];
			_bw.Write(weatherPackage.biomeId);
			_bw.Write(weatherPackage.groupIndex);
			_bw.Write(weatherPackage.remainingSeconds);
			for (int j = 0; j < weatherPackage.param.Length; j++)
			{
				_bw.Write(weatherPackage.param[j]);
			}
		}
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world == null)
		{
			return;
		}
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsClient && this.weatherPackages != null && WeatherManager.Instance)
		{
			WeatherManager.Instance.ClientProcessPackages(this.weatherPackages);
		}
	}

	public override int GetLength()
	{
		return 0;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public WeatherPackage[] weatherPackages;
}
