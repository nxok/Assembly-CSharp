using System;

namespace MusicUtils
{
	public abstract class Curve
	{
		public Curve(float _start, float _end, float _startX, float _endX)
		{
			this.rate = (_end - _start) / (_endX - _startX);
			this.linearStart = _start;
			this.linearEnd = _end;
			this.startX = _startX;
		}

		public abstract float GetMixerValue(float _param);

		[PublicizedFrom(EAccessModifier.Protected)]
		public readonly float rate;

		[PublicizedFrom(EAccessModifier.Protected)]
		public readonly float linearStart;

		[PublicizedFrom(EAccessModifier.Protected)]
		public readonly float linearEnd;

		[PublicizedFrom(EAccessModifier.Protected)]
		public readonly float startX;
	}
}
