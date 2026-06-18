using System;
using System.Diagnostics;
using System.Globalization;

namespace SharpEXR
{
	[Serializable]
	public struct Half : IComparable, IFormattable, IConvertible, IComparable<Half>, IEquatable<Half>
	{
		public Half(float value)
		{
			this = HalfHelper.SingleToHalf(value);
		}

		public Half(int value)
		{
			this = new Half((float)value);
		}

		public Half(long value)
		{
			this = new Half((float)value);
		}

		public Half(double value)
		{
			this = new Half((float)value);
		}

		public Half(decimal value)
		{
			this = new Half((float)value);
		}

		public Half(uint value)
		{
			this = new Half(value);
		}

		public Half(ulong value)
		{
			this = new Half(value);
		}

		public static Half Negate(Half half)
		{
			return -half;
		}

		public static Half Add(Half half1, Half half2)
		{
			return half1 + half2;
		}

		public static Half Subtract(Half half1, Half half2)
		{
			return half1 - half2;
		}

		public static Half Multiply(Half half1, Half half2)
		{
			return half1 * half2;
		}

		public static Half Divide(Half half1, Half half2)
		{
			return half1 / half2;
		}

		public static Half operator +(Half half)
		{
			return half;
		}

		public static Half operator -(Half half)
		{
			return HalfHelper.Negate(half);
		}

		public static Half operator ++(Half half)
		{
			return (Half)(half + 1f);
		}

		public static Half operator --(Half half)
		{
			return (Half)(half - 1f);
		}

		public static Half operator +(Half half1, Half half2)
		{
			return (Half)(half1 + half2);
		}

		public static Half operator -(Half half1, Half half2)
		{
			return (Half)(half1 - half2);
		}

		public static Half operator *(Half half1, Half half2)
		{
			return (Half)(half1 * half2);
		}

		public static Half operator /(Half half1, Half half2)
		{
			return (Half)(half1 / half2);
		}

		public static bool operator ==(Half half1, Half half2)
		{
			return !Half.IsNaN(half1) && half1.value == half2.value;
		}

		public static bool operator !=(Half half1, Half half2)
		{
			return half1.value != half2.value;
		}

		public static bool operator <(Half half1, Half half2)
		{
			return half1 < half2;
		}

		public static bool operator >(Half half1, Half half2)
		{
			return half1 > half2;
		}

		public static bool operator <=(Half half1, Half half2)
		{
			return half1 == half2 || half1 < half2;
		}

		public static bool operator >=(Half half1, Half half2)
		{
			return half1 == half2 || half1 > half2;
		}

		public static implicit operator Half(byte value)
		{
			return new Half((float)value);
		}

		public static implicit operator Half(short value)
		{
			return new Half((float)value);
		}

		public static implicit operator Half(char value)
		{
			return new Half((float)value);
		}

		public static implicit operator Half(int value)
		{
			return new Half((float)value);
		}

		public static implicit operator Half(long value)
		{
			return new Half((float)value);
		}

		public static explicit operator Half(float value)
		{
			return new Half(value);
		}

		public static explicit operator Half(double value)
		{
			return new Half((float)value);
		}

		public static explicit operator Half(decimal value)
		{
			return new Half((float)value);
		}

		public static explicit operator byte(Half value)
		{
			return (byte)value;
		}

		public static explicit operator char(Half value)
		{
			return (char)value;
		}

		public static explicit operator short(Half value)
		{
			return (short)value;
		}

		public static explicit operator int(Half value)
		{
			return (int)value;
		}

		public static explicit operator long(Half value)
		{
			return (long)value;
		}

		public static implicit operator float(Half value)
		{
			return HalfHelper.HalfToSingle(value);
		}

		public static implicit operator double(Half value)
		{
			return (double)value;
		}

		public static explicit operator decimal(Half value)
		{
			return (decimal)value;
		}

		public static implicit operator Half(sbyte value)
		{
			return new Half((float)value);
		}

		public static implicit operator Half(ushort value)
		{
			return new Half((float)value);
		}

		public static implicit operator Half(uint value)
		{
			return new Half(value);
		}

		public static implicit operator Half(ulong value)
		{
			return new Half(value);
		}

		public static explicit operator sbyte(Half value)
		{
			return (sbyte)value;
		}

		public static explicit operator ushort(Half value)
		{
			return (ushort)value;
		}

		public static explicit operator uint(Half value)
		{
			return (uint)value;
		}

		public static explicit operator ulong(Half value)
		{
			return (ulong)value;
		}

		public int CompareTo(Half other)
		{
			int num = 0;
			if (this < other)
			{
				num = -1;
			}
			else if (this > other)
			{
				num = 1;
			}
			else if (this != other)
			{
				if (!Half.IsNaN(this))
				{
					num = 1;
				}
				else if (!Half.IsNaN(other))
				{
					num = -1;
				}
			}
			return num;
		}

		public int CompareTo(object obj)
		{
			int num;
			if (obj == null)
			{
				num = 1;
			}
			else
			{
				if (!(obj is Half))
				{
					throw new ArgumentException("Object must be of type Half.");
				}
				num = this.CompareTo((Half)obj);
			}
			return num;
		}

		public bool Equals(Half other)
		{
			return other == this || (Half.IsNaN(other) && Half.IsNaN(this));
		}

		public override bool Equals(object obj)
		{
			bool flag = false;
			if (obj is Half)
			{
				Half half = (Half)obj;
				if (half == this || (Half.IsNaN(half) && Half.IsNaN(this)))
				{
					flag = true;
				}
			}
			return flag;
		}

		public override int GetHashCode()
		{
			return this.value.GetHashCode();
		}

		public TypeCode GetTypeCode()
		{
			return (TypeCode)255;
		}

		public static byte[] GetBytes(Half value)
		{
			return BitConverter.GetBytes(value.value);
		}

		public static ushort GetBits(Half value)
		{
			return value.value;
		}

		public static Half ToHalf(byte[] value, int startIndex)
		{
			return Half.ToHalf((ushort)BitConverter.ToInt16(value, startIndex));
		}

		public static Half ToHalf(ushort bits)
		{
			return new Half
			{
				value = bits
			};
		}

		public static int Sign(Half value)
		{
			if (value < 0)
			{
				return -1;
			}
			if (value > 0)
			{
				return 1;
			}
			if (value != 0)
			{
				throw new ArithmeticException("Function does not accept floating point Not-a-Number values.");
			}
			return 0;
		}

		public static Half Abs(Half value)
		{
			return HalfHelper.Abs(value);
		}

		public static Half Max(Half value1, Half value2)
		{
			if (!(value1 < value2))
			{
				return value1;
			}
			return value2;
		}

		public static Half Min(Half value1, Half value2)
		{
			if (!(value1 < value2))
			{
				return value2;
			}
			return value1;
		}

		public static bool IsNaN(Half half)
		{
			return HalfHelper.IsNaN(half);
		}

		public static bool IsInfinity(Half half)
		{
			return HalfHelper.IsInfinity(half);
		}

		public static bool IsNegativeInfinity(Half half)
		{
			return HalfHelper.IsNegativeInfinity(half);
		}

		public static bool IsPositiveInfinity(Half half)
		{
			return HalfHelper.IsPositiveInfinity(half);
		}

		public static Half Parse(string value)
		{
			return (Half)float.Parse(value, CultureInfo.InvariantCulture);
		}

		public static Half Parse(string value, IFormatProvider provider)
		{
			return (Half)float.Parse(value, provider);
		}

		public static Half Parse(string value, NumberStyles style)
		{
			return (Half)float.Parse(value, style, CultureInfo.InvariantCulture);
		}

		public static Half Parse(string value, NumberStyles style, IFormatProvider provider)
		{
			return (Half)float.Parse(value, style, provider);
		}

		public static bool TryParse(string value, out Half result)
		{
			float num;
			if (float.TryParse(value, out num))
			{
				result = (Half)num;
				return true;
			}
			result = default(Half);
			return false;
		}

		public static bool TryParse(string value, NumberStyles style, IFormatProvider provider, out Half result)
		{
			bool flag = false;
			float num;
			if (float.TryParse(value, style, provider, out num))
			{
				result = (Half)num;
				flag = true;
			}
			else
			{
				result = default(Half);
			}
			return flag;
		}

		public override string ToString()
		{
			return this.ToString(CultureInfo.InvariantCulture);
		}

		public string ToString(IFormatProvider formatProvider)
		{
			return this.ToString(formatProvider);
		}

		public string ToString(string format)
		{
			return this.ToString(format, CultureInfo.InvariantCulture);
		}

		public string ToString(string format, IFormatProvider formatProvider)
		{
			return this.ToString(format, formatProvider);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public float System.IConvertible.ToSingle(IFormatProvider provider)
		{
			return this;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public TypeCode System.IConvertible.GetTypeCode()
		{
			return this.GetTypeCode();
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public bool System.IConvertible.ToBoolean(IFormatProvider provider)
		{
			return Convert.ToBoolean(this);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public byte System.IConvertible.ToByte(IFormatProvider provider)
		{
			return Convert.ToByte(this);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public char System.IConvertible.ToChar(IFormatProvider provider)
		{
			throw new InvalidCastException(string.Format(CultureInfo.CurrentCulture, "Invalid cast from '{0}' to '{1}'.", "Half", "Char"));
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public DateTime System.IConvertible.ToDateTime(IFormatProvider provider)
		{
			throw new InvalidCastException(string.Format(CultureInfo.CurrentCulture, "Invalid cast from '{0}' to '{1}'.", "Half", "DateTime"));
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public decimal System.IConvertible.ToDecimal(IFormatProvider provider)
		{
			return Convert.ToDecimal(this);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public double System.IConvertible.ToDouble(IFormatProvider provider)
		{
			return Convert.ToDouble(this);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public short System.IConvertible.ToInt16(IFormatProvider provider)
		{
			return Convert.ToInt16(this);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public int System.IConvertible.ToInt32(IFormatProvider provider)
		{
			return Convert.ToInt32(this);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public long System.IConvertible.ToInt64(IFormatProvider provider)
		{
			return Convert.ToInt64(this);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public sbyte System.IConvertible.ToSByte(IFormatProvider provider)
		{
			return Convert.ToSByte(this);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public string System.IConvertible.ToString(IFormatProvider provider)
		{
			return Convert.ToString(this, CultureInfo.InvariantCulture);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public object System.IConvertible.ToType(Type conversionType, IFormatProvider provider)
		{
			return ((IConvertible)this).ToType(conversionType, provider);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public ushort System.IConvertible.ToUInt16(IFormatProvider provider)
		{
			return Convert.ToUInt16(this);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public uint System.IConvertible.ToUInt32(IFormatProvider provider)
		{
			return Convert.ToUInt32(this);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public ulong System.IConvertible.ToUInt64(IFormatProvider provider)
		{
			return Convert.ToUInt64(this);
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		[PublicizedFrom(EAccessModifier.Internal)]
		[NonSerialized]
		public ushort value;

		public static readonly Half Epsilon = Half.ToHalf(1);

		public static readonly Half MaxValue = Half.ToHalf(31743);

		public static readonly Half MinValue = Half.ToHalf(64511);

		public static readonly Half NaN = Half.ToHalf(65024);

		public static readonly Half NegativeInfinity = Half.ToHalf(64512);

		public static readonly Half PositiveInfinity = Half.ToHalf(31744);
	}
}
