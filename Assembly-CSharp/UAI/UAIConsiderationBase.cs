using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.Scripting;

namespace UAI
{
	[Preserve]
	public class UAIConsiderationBase
	{
		public string Name { get; set; }

		public virtual void Init(Dictionary<string, string> parameters)
		{
			if (parameters.ContainsKey("curve"))
			{
				this.curveType = EnumUtils.Parse<CurveType>(parameters["curve"], true);
			}
			else
			{
				this.curveType = CurveType.Linear;
			}
			if (parameters.ContainsKey("flip_x"))
			{
				this.flipX = StringParsers.ParseBool(parameters["flip_x"], 0, -1, true);
			}
			else
			{
				this.flipX = false;
			}
			if (parameters.ContainsKey("flip_y"))
			{
				this.flipY = StringParsers.ParseBool(parameters["flip_y"], 0, -1, true);
			}
			else
			{
				this.flipY = false;
			}
			if (parameters.ContainsKey("x_intercept"))
			{
				this.xIntercept = StringParsers.ParseFloat(parameters["x_intercept"], 0, -1, NumberStyles.Any);
			}
			if (parameters.ContainsKey("y_intercept"))
			{
				this.yIntercept = StringParsers.ParseFloat(parameters["y_intercept"], 0, -1, NumberStyles.Any);
			}
			if (parameters.ContainsKey("slope_intercept"))
			{
				this.slopeIntercept = StringParsers.ParseFloat(parameters["slope_intercept"], 0, -1, NumberStyles.Any);
			}
			if (parameters.ContainsKey("exponent"))
			{
				this.exponent = StringParsers.ParseFloat(parameters["exponent"], 0, -1, NumberStyles.Any);
			}
		}

		public virtual float GetScore(Context _context, object currentTargetConsideration)
		{
			return 1f;
		}

		public float ComputeResponseCurve(float x)
		{
			if (this.flipX)
			{
				x = 1f - x;
			}
			float num = 0f;
			switch (this.curveType)
			{
			case CurveType.Constant:
				num = this.yIntercept;
				break;
			case CurveType.Linear:
				num = this.slopeIntercept * (x - this.xIntercept) + this.yIntercept;
				break;
			case CurveType.Quadratic:
				num = this.slopeIntercept * x * Mathf.Pow(Mathf.Abs(x + this.xIntercept), this.exponent) + this.yIntercept;
				break;
			case CurveType.Logistic:
				num = this.exponent * (1f / (1f + Mathf.Pow(Mathf.Abs(1000f * this.slopeIntercept), -1f * x + this.xIntercept + 0.5f))) + this.yIntercept;
				break;
			case CurveType.Logit:
				num = -Mathf.Log(1f / Mathf.Pow(Mathf.Abs(x - this.xIntercept), this.exponent) - 1f) * 0.05f * this.slopeIntercept + (0.5f + this.yIntercept);
				break;
			case CurveType.Threshold:
				num = ((x > this.xIntercept) ? (1f - this.yIntercept) : (0f - (1f - this.slopeIntercept)));
				break;
			case CurveType.Sine:
				num = Mathf.Sin(this.slopeIntercept * Mathf.Pow(x + this.xIntercept, this.exponent)) * 0.5f + 0.5f + this.yIntercept;
				break;
			case CurveType.Parabolic:
				num = Mathf.Pow(this.slopeIntercept * (x + this.xIntercept), 2f) + this.exponent * (x + this.xIntercept) + this.yIntercept;
				break;
			case CurveType.NormalDistribution:
				num = this.exponent / Mathf.Sqrt(6.283192f) * Mathf.Pow(2f, -(1f / (Mathf.Abs(this.slopeIntercept) * 0.01f)) * Mathf.Pow(x - (this.xIntercept + 0.5f), 2f)) + this.yIntercept;
				break;
			case CurveType.Bounce:
				num = Mathf.Abs(Mathf.Sin(6.28f * this.exponent * (x + this.xIntercept + 1f) * (x + this.xIntercept + 1f)) * (1f - x) * this.slopeIntercept) + this.yIntercept;
				break;
			}
			if (this.flipY)
			{
				num = 1f - num;
			}
			return Mathf.Clamp01(num);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public CurveType curveType;

		[PublicizedFrom(EAccessModifier.Private)]
		public float xIntercept;

		[PublicizedFrom(EAccessModifier.Private)]
		public float yIntercept;

		[PublicizedFrom(EAccessModifier.Private)]
		public float slopeIntercept = 1f;

		[PublicizedFrom(EAccessModifier.Private)]
		public float exponent = 1f;

		[PublicizedFrom(EAccessModifier.Private)]
		public bool flipY;

		[PublicizedFrom(EAccessModifier.Private)]
		public bool flipX;
	}
}
