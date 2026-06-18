using System;
using UnityEngine;

public static class MathUtils
{
	public static float Clamp(float value, float min, float max)
	{
		if (value < min)
		{
			return min;
		}
		if (value <= max)
		{
			return value;
		}
		return max;
	}

	public static float Lerp(float a, float b, float x)
	{
		return a + (b - a) * x;
	}

	public static int Min(int a, int b)
	{
		if (a < b)
		{
			return a;
		}
		return b;
	}

	public static int Min(int a, int b, int c)
	{
		if (a >= c && b >= c)
		{
			return c;
		}
		if (a < b)
		{
			return a;
		}
		return b;
	}

	public static int Min(int a, int b, int c, int d)
	{
		if (a >= d && b >= d && c >= d)
		{
			return d;
		}
		if (a >= c && b >= c)
		{
			return c;
		}
		if (a < b)
		{
			return a;
		}
		return b;
	}

	public static int Max(int a, int b)
	{
		if (a > b)
		{
			return a;
		}
		return b;
	}

	public static int Max(int a, int b, int c)
	{
		if (a <= c && b <= c)
		{
			return c;
		}
		if (a > b)
		{
			return a;
		}
		return b;
	}

	public static int Max(int a, int b, int c, int d)
	{
		if (a <= d && b <= d && c <= d)
		{
			return d;
		}
		if (a <= c && b <= c)
		{
			return c;
		}
		if (a > b)
		{
			return a;
		}
		return b;
	}

	public static double RoundToSignificantDigits(double value, int digits)
	{
		if (value == 0.0)
		{
			return 0.0;
		}
		double num = Math.Pow(10.0, Math.Floor(Math.Log10(Math.Abs(value))) + 1.0);
		return num * Math.Round(value / num, digits);
	}

	public static double TruncateToSignificantDigits(double value, int digits)
	{
		if (value == 0.0)
		{
			return 0.0;
		}
		double num = Math.Pow(10.0, Math.Floor(Math.Log10(Math.Abs(value))) + 1.0 - (double)digits);
		return num * Math.Truncate(value / num);
	}

	public static void Swap(ref int x, ref int z)
	{
		int num = x;
		x = z;
		z = num;
	}

	public static int Mod(int _value, int _modulus)
	{
		return (_value % _modulus + _modulus) % _modulus;
	}

	public static float Mod(float _value, float _modulus)
	{
		return (_value % _modulus + _modulus) % _modulus;
	}

	public static uint ToNextPowerOfTwo(uint _x, bool _allowCurrent = false)
	{
		if (_allowCurrent)
		{
			_x -= 1U;
		}
		_x |= _x >> 1;
		_x |= _x >> 2;
		_x |= _x >> 4;
		_x |= _x >> 8;
		_x |= _x >> 16;
		return _x + 1U;
	}

	public static uint ToPreviousPowerOfTwo(uint _x, bool _allowCurrent = false)
	{
		return MathUtils.ToNextPowerOfTwo(_x, !_allowCurrent) >> 1;
	}

	public static float ClampAxis(float Angle)
	{
		Angle %= 360f;
		if (Angle < 0f)
		{
			Angle += 360f;
		}
		return Angle;
	}

	public static float NormalizeAxis(float Angle)
	{
		Angle = MathUtils.ClampAxis(Angle);
		if (Angle > 180f)
		{
			Angle -= 360f;
		}
		return Angle;
	}

	public static float YawForDirection(Vector3 direction)
	{
		float x = direction.x;
		return -(float)(Math.Atan2((double)direction.z, (double)x) * 180.0 / 3.141592653589793) + 90f;
	}

	public static float DistanceToSegment(Vector3 point, Vector3 start, Vector3 end)
	{
		Vector3 vector = end - start;
		float sqrMagnitude = vector.sqrMagnitude;
		if (sqrMagnitude == 0f)
		{
			return Vector3.Distance(point, start);
		}
		float num = Vector3.Dot(point - start, vector) / sqrMagnitude;
		num = Mathf.Clamp01(num);
		Vector3 vector2 = start + num * vector;
		return Vector3.Distance(point, vector2);
	}
}
