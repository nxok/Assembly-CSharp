using System;

public static class MetricConversion
{
	public static string ToShortestBytesString(long bytes)
	{
		if (bytes < 1024L)
		{
			return string.Format("{0} B", bytes);
		}
		if (bytes < 1048576L)
		{
			return string.Format("{0:F2} KB", 0.0009765625 * (double)bytes);
		}
		if (bytes < 1073741824L)
		{
			return string.Format("{0:F2} MB", 9.5367431640625E-07 * (double)bytes);
		}
		return string.Format("{0:F2} GB", 9.313225746154785E-10 * (double)bytes);
	}

	public const double nanoToMilli = 1E-06;

	public const int kilobyte = 1024;

	public const int megabyte = 1048576;

	public const int gigabyte = 1073741824;

	public const double bytesToKilobyte = 0.0009765625;

	public const double bytesToMegabyte = 9.5367431640625E-07;

	public const double bytesToGigabyte = 9.313225746154785E-10;
}
