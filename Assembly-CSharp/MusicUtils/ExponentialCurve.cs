using System;

namespace MusicUtils
{
	public class ExponentialCurve : LinearCurve
	{
		public ExponentialCurve(double _base, float _start, float _end, float _startX, float _endX)
			: base((float)Math.Log((double)_start, _base), (float)Math.Log((double)_end, _base), _startX, _endX)
		{
			this.b = _base;
			if (_start < _end)
			{
				this.min = _start;
				this.max = _end;
				return;
			}
			this.min = _end;
			this.max = _start;
		}

		public override float GetMixerValue(float _param)
		{
			return Utils.FastClamp((float)Math.Pow(this.b, (double)base.GetLine(_param)), this.min, this.max);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly double b;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly float min;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly float max;
	}
}
