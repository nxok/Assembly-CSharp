using System;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;

public static class IncrementalHashExtensions
{
	public unsafe static void AppendDataNoAlloc(this IncrementalHash hash, short value)
	{
		*Unsafe.As<byte, short>(ref IncrementalHashExtensions.scratch[0]) = value;
		if (!BitConverter.IsLittleEndian)
		{
			Array.Reverse<byte>(IncrementalHashExtensions.scratch, 0, 2);
		}
		hash.AppendData(IncrementalHashExtensions.scratch, 0, 2);
	}

	public unsafe static void AppendDataNoAlloc(this IncrementalHash hash, ushort value)
	{
		*Unsafe.As<byte, ushort>(ref IncrementalHashExtensions.scratch[0]) = value;
		if (!BitConverter.IsLittleEndian)
		{
			Array.Reverse<byte>(IncrementalHashExtensions.scratch, 0, 2);
		}
		hash.AppendData(IncrementalHashExtensions.scratch, 0, 2);
	}

	public unsafe static void AppendDataNoAlloc(this IncrementalHash hash, int value)
	{
		*Unsafe.As<byte, int>(ref IncrementalHashExtensions.scratch[0]) = value;
		if (!BitConverter.IsLittleEndian)
		{
			Array.Reverse<byte>(IncrementalHashExtensions.scratch, 0, 4);
		}
		hash.AppendData(IncrementalHashExtensions.scratch, 0, 4);
	}

	public unsafe static void AppendDataNoAlloc(this IncrementalHash hash, uint value)
	{
		*Unsafe.As<byte, uint>(ref IncrementalHashExtensions.scratch[0]) = value;
		if (!BitConverter.IsLittleEndian)
		{
			Array.Reverse<byte>(IncrementalHashExtensions.scratch, 0, 4);
		}
		hash.AppendData(IncrementalHashExtensions.scratch, 0, 4);
	}

	public unsafe static void AppendDataNoAlloc(this IncrementalHash hash, long value)
	{
		*Unsafe.As<byte, long>(ref IncrementalHashExtensions.scratch[0]) = value;
		if (!BitConverter.IsLittleEndian)
		{
			Array.Reverse<byte>(IncrementalHashExtensions.scratch, 0, 8);
		}
		hash.AppendData(IncrementalHashExtensions.scratch, 0, 8);
	}

	public unsafe static void AppendDataNoAlloc(this IncrementalHash hash, ulong value)
	{
		*Unsafe.As<byte, ulong>(ref IncrementalHashExtensions.scratch[0]) = value;
		if (!BitConverter.IsLittleEndian)
		{
			Array.Reverse<byte>(IncrementalHashExtensions.scratch, 0, 8);
		}
		hash.AppendData(IncrementalHashExtensions.scratch, 0, 8);
	}

	public unsafe static void AppendDataNoAlloc(this IncrementalHash hash, float value)
	{
		*Unsafe.As<byte, float>(ref IncrementalHashExtensions.scratch[0]) = value;
		if (!BitConverter.IsLittleEndian)
		{
			Array.Reverse<byte>(IncrementalHashExtensions.scratch, 0, 4);
		}
		hash.AppendData(IncrementalHashExtensions.scratch, 0, 4);
	}

	public static void AppendDataNoAlloc(this IncrementalHash hash, bool value)
	{
		IncrementalHashExtensions.scratch[0] = (value ? 1 : 0);
		hash.AppendData(IncrementalHashExtensions.scratch, 0, 1);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly byte[] scratch = new byte[8];
}
