using System;

namespace DynamicMusic
{
	public class LayerParams
	{
		public LayerParams(float _volume, float _mix)
		{
			this.Volume = _volume;
			this.Mix = _mix;
		}

		public float Volume;

		public float Mix;
	}
}
