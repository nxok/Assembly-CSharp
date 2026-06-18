using System;
using System.Collections.Generic;

public abstract class AIDirectorData
{
	public static void InitStatic()
	{
		AIDirectorData.noisySounds = new CaseInsensitiveStringDictionary<AIDirectorData.Noise>();
	}

	public static void Cleanup()
	{
		if (AIDirectorData.noisySounds != null)
		{
			AIDirectorData.noisySounds.Clear();
		}
	}

	public static void AddNoisySound(string _name, AIDirectorData.Noise _noise)
	{
		AIDirectorData.noisySounds.Add(_name, _noise);
	}

	public static bool FindNoise(string name, out AIDirectorData.Noise noise)
	{
		if (name == null)
		{
			noise = default(AIDirectorData.Noise);
			return false;
		}
		return AIDirectorData.noisySounds.TryGetValue(name, out noise);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public AIDirectorData()
	{
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static Dictionary<string, AIDirectorData.Noise> noisySounds;

	public struct Noise
	{
		public Noise(string _source, float _volume, float _duration, float _muffledWhenCrouched, float _heatMapStrength, ulong _heatMapWorldTimeToLive)
		{
			this.volume = _volume;
			this.duration = _duration;
			this.muffledWhenCrouched = _muffledWhenCrouched;
			this.heatMapStrength = _heatMapStrength;
			this.heatMapWorldTimeToLive = _heatMapWorldTimeToLive;
		}

		public float volume;

		public float duration;

		public float muffledWhenCrouched;

		public float heatMapStrength;

		public ulong heatMapWorldTimeToLive;
	}
}
