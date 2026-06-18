using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

public readonly ref struct StringSpan
{
	[PublicizedFrom(EAccessModifier.Private)]
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public StringSpan(ReadOnlySpan<char> span)
	{
		this.m_span = span;
	}

	public int Length
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return this.m_span.Length;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public ReadOnlySpan<char> AsSpan()
	{
		return this.m_span;
	}

	public override string ToString()
	{
		return new string(this.m_span);
	}

	public ref char this[int index]
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return this.m_span[index];
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public StringSpan Slice(int start)
	{
		return this.m_span.Slice(start);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public StringSpan Slice(int start, int length)
	{
		return this.m_span.Slice(start, length);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool Equals(StringSpan other, StringComparison comparisonType = StringComparison.Ordinal)
	{
		return this.m_span == other.m_span || this.m_span.Equals(other.AsSpan(), comparisonType);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int CompareTo(StringSpan other, StringComparison comparisonType = StringComparison.Ordinal)
	{
		if (!(this.m_span == other.m_span))
		{
			return this.m_span.CompareTo(other.AsSpan(), comparisonType);
		}
		return 0;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool Contains(char value)
	{
		return this.IndexOf(value) >= 0;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool Contains(char value, StringComparison comparisonType)
	{
		return this.IndexOf(value, comparisonType) >= 0;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool Contains(string value)
	{
		return this.IndexOf(value, StringComparison.Ordinal) >= 0;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool Contains(string value, StringComparison comparisonType)
	{
		return this.IndexOf(value, comparisonType) >= 0;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int IndexOf(char value)
	{
		return this.m_span.IndexOf(value);
	}

	public int IndexOf(char value, StringComparison comparisonType)
	{
		return this.m_span.IndexOf(MemoryMarshal.CreateReadOnlySpan<char>(ref value, 1), comparisonType);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int IndexOf(StringSpan value)
	{
		return this.m_span.IndexOf(value.AsSpan());
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int IndexOf(StringSpan value, StringComparison comparisonType)
	{
		return this.m_span.IndexOf(value.AsSpan(), comparisonType);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int LastIndexOf(char value)
	{
		return this.m_span.LastIndexOf(value);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int LastIndexOf(StringSpan value)
	{
		return this.m_span.LastIndexOf(value.AsSpan());
	}

	public int IndexOfAny(StringSpan value)
	{
		return this.m_span.IndexOfAny(value.AsSpan());
	}

	public int LastIndexOfAny(StringSpan value)
	{
		return this.m_span.LastIndexOfAny(value.AsSpan());
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public StringSpan.WhitespaceSplitEnumerator GetSplitEnumerator(StringSplitOptions options = StringSplitOptions.None)
	{
		return new StringSpan.WhitespaceSplitEnumerator(this.m_span, options);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public StringSpan.CharSplitEnumerator GetSplitEnumerator(char separator, StringSplitOptions options = StringSplitOptions.None)
	{
		return new StringSpan.CharSplitEnumerator(this.m_span, separator, options);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public StringSpan.StringSplitEnumerator GetSplitEnumerator(ReadOnlySpan<char> separator, StringSplitOptions options = StringSplitOptions.None)
	{
		return new StringSpan.StringSplitEnumerator(this.m_span, separator, options);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public StringSpan.SeparatorSplitAnyEnumerator GetSplitAnyEnumerator(string[] separator, StringSplitOptions options = StringSplitOptions.None)
	{
		return new StringSpan.SeparatorSplitAnyEnumerator(this.m_span, options, separator);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public StringSpan Substring(int startIndex)
	{
		StringSpan stringSpan = this;
		return stringSpan.Slice(startIndex, stringSpan.Length - startIndex);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public StringSpan Substring(int startIndex, int length)
	{
		StringSpan stringSpan = this;
		return stringSpan.Slice(startIndex, startIndex + length - startIndex);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe StringSpan Trim()
	{
		int num = 0;
		while (num < this.Length && char.IsWhiteSpace((char)(*this[num])))
		{
			num++;
		}
		int num2 = this.Length - 1;
		while (num2 >= num && char.IsWhiteSpace((char)(*this[num2])))
		{
			num2--;
		}
		if (num > num2)
		{
			return default(StringSpan);
		}
		return this.Slice(num, num2 - num + 1);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static implicit operator StringSpan(string str)
	{
		return new StringSpan(str);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static implicit operator StringSpan(ReadOnlySpan<char> span)
	{
		return new StringSpan(span);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static explicit operator string(StringSpan span)
	{
		return new string(span.m_span);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static explicit operator ReadOnlySpan<char>(StringSpan span)
	{
		return span.m_span;
	}

	public override bool Equals(object obj)
	{
		throw new NotSupportedException("StringSpan.Equals(object) is not supported. Use another method or the operator == instead.");
	}

	public override int GetHashCode()
	{
		return SpanUtils.GetHashCode<char>(this.m_span);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator ==(StringSpan left, StringSpan right)
	{
		return left.Equals(right, StringComparison.Ordinal);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator !=(StringSpan left, StringSpan right)
	{
		return !(left == right);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly ReadOnlySpan<char> m_span;

	public ref struct CharSplitEnumerator
	{
		public CharSplitEnumerator(ReadOnlySpan<char> span, char separator, StringSplitOptions options = StringSplitOptions.None)
		{
			this.m_remainder = span;
			this.m_separator = separator;
			this.m_removeEmptyEntries = options.HasFlag(StringSplitOptions.RemoveEmptyEntries);
			this.m_current = default(StringSpan);
			this.m_done = false;
		}

		public StringSpan.CharSplitEnumerator GetEnumerator()
		{
			return this;
		}

		public StringSpan Current
		{
			get
			{
				return this.m_current;
			}
		}

		public bool MoveNext()
		{
			while (this.MoveToNextInternal())
			{
				if (!this.m_removeEmptyEntries || this.m_current.Length != 0)
				{
					return true;
				}
			}
			return false;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public unsafe bool MoveToNextInternal()
		{
			if (this.m_done)
			{
				return false;
			}
			for (int i = 0; i < this.m_remainder.Length; i++)
			{
				if (*this.m_remainder[i] == (ushort)this.m_separator)
				{
					ReadOnlySpan<char> readOnlySpan = this.m_remainder;
					this.m_current = readOnlySpan.Slice(0, i);
					readOnlySpan = this.m_remainder;
					int num = i + 1;
					this.m_remainder = readOnlySpan.Slice(num, readOnlySpan.Length - num);
					return true;
				}
			}
			this.m_current = this.m_remainder;
			this.m_remainder = default(ReadOnlySpan<char>);
			this.m_done = true;
			return true;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public ReadOnlySpan<char> m_remainder;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly char m_separator;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly bool m_removeEmptyEntries;

		[PublicizedFrom(EAccessModifier.Private)]
		public StringSpan m_current;

		[PublicizedFrom(EAccessModifier.Private)]
		public bool m_done;
	}

	public ref struct StringSplitEnumerator
	{
		public StringSplitEnumerator(ReadOnlySpan<char> span, ReadOnlySpan<char> separator, StringSplitOptions options = StringSplitOptions.None)
		{
			this.m_remainder = span;
			this.m_separator = separator;
			this.m_removeEmptyEntries = options.HasFlag(StringSplitOptions.RemoveEmptyEntries);
			this.m_current = default(StringSpan);
			this.m_done = false;
		}

		public StringSpan.StringSplitEnumerator GetEnumerator()
		{
			return this;
		}

		public StringSpan Current
		{
			get
			{
				return this.m_current;
			}
		}

		public bool MoveNext()
		{
			while (this.MoveToNextInternal())
			{
				if (!this.m_removeEmptyEntries || this.m_current.Length != 0)
				{
					return true;
				}
			}
			return false;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public bool MoveToNextInternal()
		{
			if (this.m_done)
			{
				return false;
			}
			if (this.m_separator.Length <= 0)
			{
				this.m_current = this.m_remainder;
				this.m_remainder = default(ReadOnlySpan<char>);
				this.m_done = true;
				return true;
			}
			int num = this.m_remainder.Length + 1 - this.m_separator.Length;
			for (int i = 0; i < num; i++)
			{
				if (this.m_remainder.Slice(i, this.m_separator.Length).CompareTo(this.m_separator, StringComparison.Ordinal) == 0)
				{
					ReadOnlySpan<char> readOnlySpan = this.m_remainder;
					this.m_current = readOnlySpan.Slice(0, i);
					readOnlySpan = this.m_remainder;
					int num2 = i + this.m_separator.Length;
					this.m_remainder = readOnlySpan.Slice(num2, readOnlySpan.Length - num2);
					return true;
				}
			}
			this.m_current = this.m_remainder;
			this.m_remainder = default(ReadOnlySpan<char>);
			this.m_done = true;
			return true;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public ReadOnlySpan<char> m_remainder;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly ReadOnlySpan<char> m_separator;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly bool m_removeEmptyEntries;

		[PublicizedFrom(EAccessModifier.Private)]
		public StringSpan m_current;

		[PublicizedFrom(EAccessModifier.Private)]
		public bool m_done;
	}

	public ref struct WhitespaceSplitEnumerator
	{
		public WhitespaceSplitEnumerator(ReadOnlySpan<char> text, StringSplitOptions options = StringSplitOptions.None)
		{
			this.m_remainder = text;
			this.m_removeEmptyEntries = options.HasFlag(StringSplitOptions.RemoveEmptyEntries);
			this.m_current = default(StringSpan);
			this.m_done = false;
		}

		public StringSpan.WhitespaceSplitEnumerator GetEnumerator()
		{
			return this;
		}

		public StringSpan Current
		{
			get
			{
				return this.m_current;
			}
		}

		public bool MoveNext()
		{
			while (this.MoveToNextInternal())
			{
				if (!this.m_removeEmptyEntries || this.m_current.Length != 0)
				{
					return true;
				}
			}
			return false;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public unsafe bool MoveToNextInternal()
		{
			if (this.m_done)
			{
				return false;
			}
			for (int i = 0; i < this.m_remainder.Length; i++)
			{
				if (char.IsWhiteSpace((char)(*this.m_remainder[i])))
				{
					StringSpan stringSpan = this.m_remainder;
					this.m_current = stringSpan.Slice(0, i);
					stringSpan = this.m_remainder;
					int num = i + 1;
					this.m_remainder = stringSpan.Slice(num, stringSpan.Length - num);
					return true;
				}
			}
			this.m_current = this.m_remainder;
			this.m_remainder = default(StringSpan);
			this.m_done = true;
			return true;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public StringSpan m_remainder;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly bool m_removeEmptyEntries;

		[PublicizedFrom(EAccessModifier.Private)]
		public StringSpan m_current;

		[PublicizedFrom(EAccessModifier.Private)]
		public bool m_done;
	}

	public ref struct SeparatorSplitAnyEnumerator
	{
		public SeparatorSplitAnyEnumerator(ReadOnlySpan<char> text, StringSplitOptions options = StringSplitOptions.None, params string[] separators)
		{
			this.m_remainder = text;
			this.m_removeEmptyEntries = options.HasFlag(StringSplitOptions.RemoveEmptyEntries);
			bool flag = false;
			if (separators != null)
			{
				for (int i = 0; i < separators.Length; i++)
				{
					if (!string.IsNullOrEmpty(separators[i]))
					{
						flag = true;
						break;
					}
				}
			}
			if (!flag)
			{
				throw new ArgumentException("StringSplitEnumerator requires at least one non-empty separator");
			}
			this.m_separators = separators;
			this.m_current = default(StringSpan);
			this.m_done = false;
		}

		public StringSpan.SeparatorSplitAnyEnumerator GetEnumerator()
		{
			return this;
		}

		public StringSpan Current
		{
			get
			{
				return this.m_current;
			}
		}

		public bool MoveNext()
		{
			while (this.MoveToNextInternal())
			{
				if (!this.m_removeEmptyEntries || this.m_current.Length != 0)
				{
					return true;
				}
			}
			return false;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public unsafe bool MoveToNextInternal()
		{
			if (this.m_done)
			{
				return false;
			}
			for (int i = 0; i < this.m_remainder.Length; i++)
			{
				foreach (string text in this.m_separators)
				{
					if (!string.IsNullOrEmpty(text) && *this.m_remainder[i] == (ushort)text[0] && i + text.Length <= this.m_remainder.Length && this.m_remainder.Slice(i, text.Length).CompareTo(text.AsSpan(), StringComparison.Ordinal) == 0)
					{
						StringSpan stringSpan = this.m_remainder;
						this.m_current = stringSpan.Slice(0, i);
						stringSpan = this.m_remainder;
						int num = i + text.Length;
						this.m_remainder = stringSpan.Slice(num, stringSpan.Length - num);
						return true;
					}
				}
			}
			this.m_current = this.m_remainder;
			this.m_remainder = default(StringSpan);
			this.m_done = true;
			return true;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public StringSpan m_remainder;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly bool m_removeEmptyEntries;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly string[] m_separators;

		[PublicizedFrom(EAccessModifier.Private)]
		public StringSpan m_current;

		[PublicizedFrom(EAccessModifier.Private)]
		public bool m_done;
	}
}
