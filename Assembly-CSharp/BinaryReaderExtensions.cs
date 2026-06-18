using System;
using System.IO;

public static class BinaryReaderExtensions
{
	public static bool TryReadAllBytes(this BinaryReader reader, Span<byte> dest)
	{
		int num;
		return reader.TryReadAllBytes(dest, out num);
	}

	public static bool TryReadAllBytes(this BinaryReader reader, Span<byte> dest, out int totalBytesRead)
	{
		int num2;
		for (totalBytesRead = 0; totalBytesRead < dest.Length; totalBytesRead += num2)
		{
			Span<byte> span = dest;
			int num = totalBytesRead;
			num2 = reader.Read(span.Slice(num, span.Length - num));
			if (num2 <= 0)
			{
				return false;
			}
		}
		return true;
	}
}
