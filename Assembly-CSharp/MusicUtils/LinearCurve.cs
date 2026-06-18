using System;

namespace MusicUtils
{
	public class LinearCurve : Curve
	{
		public LinearCurve(float _startY, float _endY, float _startX, float _endX)
			: base(_startY, _endY, _startX, _endX)
		{
		}

		public override float GetMixerValue(float _param)
		{
			return Utils.FastClamp(this.GetLine(_param), this.linearStart, this.linearEnd);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public float GetLine(float _param)
		{
			return this.rate * (_param - this.startX) + this.linearStart;
		}
	}
}
