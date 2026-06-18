using System;

namespace MusicUtils
{
	public class LogarithmicCurve : LinearCurve
	{
		public LogarithmicCurve(double _base, double _scale, float _start, float _end, float _startX, float _endX)
			: base((float)Math.Pow(_base, (double)_start / _scale), (float)Math.Pow(_base, (double)_end / _scale), _startX, _endX)
		{
			this.b = Math.Pow(_base, 1.0 / _scale);
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
			return Utils.FastClamp((float)Math.Log((double)Math.Max(base.GetLine(_param), 0f), this.b), this.min, this.max);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly double b;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly float min;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly float max;
	}
}
