using System;

namespace Audio
{
	public class NoiseData
	{
		public NoiseData()
		{
			this.volume = 0f;
			this.time = 1f;
			this.heatMapStrength = 0f;
			this.heatMapTime = 100UL;
			this.crouchMuffle = 1f;
		}

		public float volume;

		public float time;

		public float heatMapStrength;

		public ulong heatMapTime;

		public float crouchMuffle;
	}
}
