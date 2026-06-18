using System;

public class WeatherPackage
{
	public WeatherPackage()
	{
		this.param = new float[5];
	}

	public void CopyTo(WeatherManager.BiomeWeather _bw)
	{
		_bw.remainingSeconds = this.remainingSeconds;
		for (int i = 0; i < this.param.Length; i++)
		{
			_bw.parameters[i].target = this.param[i];
		}
	}

	public override string ToString()
	{
		string text = string.Format("id {0}, grp {1}, rsec {2}, params ", this.biomeId, this.groupIndex, this.remainingSeconds);
		for (int i = 0; i < this.param.Length; i++)
		{
			text += string.Format("{0}, ", this.param[i]);
		}
		return text;
	}

	public byte biomeId;

	public byte groupIndex;

	public byte remainingSeconds;

	public float[] param;
}
