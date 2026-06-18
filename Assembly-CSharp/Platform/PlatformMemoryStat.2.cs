using System;
using System.Runtime.CompilerServices;
using System.Text;

namespace Platform
{
	public static class PlatformMemoryStat
	{
		public static IPlatformMemoryStat<T> Create<T>(string name)
		{
			IPlatformMemoryStat<T> platformMemoryStat = PlatformMemoryStat<T>.Create(name);
			platformMemoryStat.RenderValue = new PlatformMemoryRenderValue<T>(PlatformMemoryStat.<Create>g__RenderValue|0_0<T>);
			return platformMemoryStat;
		}

		public static IPlatformMemoryStat<long> CreateBytes(string name)
		{
			IPlatformMemoryStat<long> platformMemoryStat = PlatformMemoryStat.CreateInt64(name);
			platformMemoryStat.RenderValue = new PlatformMemoryRenderValue<long>(PlatformMemoryStat.<CreateBytes>g__RenderValue|1_0);
			platformMemoryStat.RenderDelta = new PlatformMemoryRenderDelta<long>(PlatformMemoryStat.<CreateBytes>g__RenderDelta|1_1);
			return platformMemoryStat;
		}

		public static IPlatformMemoryStat<int> CreateInt32(string name)
		{
			IPlatformMemoryStat<int> platformMemoryStat = PlatformMemoryStat<int>.Create(name);
			platformMemoryStat.RenderValue = new PlatformMemoryRenderValue<int>(PlatformMemoryStat.<CreateInt32>g__RenderValue|2_0);
			platformMemoryStat.RenderDelta = new PlatformMemoryRenderDelta<int>(PlatformMemoryStat.<CreateInt32>g__RenderDelta|2_1);
			return platformMemoryStat;
		}

		public static IPlatformMemoryStat<uint> CreateUInt32(string name)
		{
			IPlatformMemoryStat<uint> platformMemoryStat = PlatformMemoryStat<uint>.Create(name);
			platformMemoryStat.RenderValue = new PlatformMemoryRenderValue<uint>(PlatformMemoryStat.<CreateUInt32>g__RenderValue|3_0);
			platformMemoryStat.RenderDelta = new PlatformMemoryRenderDelta<uint>(PlatformMemoryStat.<CreateUInt32>g__RenderDelta|3_1);
			return platformMemoryStat;
		}

		public static IPlatformMemoryStat<long> CreateInt64(string name)
		{
			IPlatformMemoryStat<long> platformMemoryStat = PlatformMemoryStat<long>.Create(name);
			platformMemoryStat.RenderValue = new PlatformMemoryRenderValue<long>(PlatformMemoryStat.<CreateInt64>g__RenderValue|4_0);
			platformMemoryStat.RenderDelta = new PlatformMemoryRenderDelta<long>(PlatformMemoryStat.<CreateInt64>g__RenderDelta|4_1);
			return platformMemoryStat;
		}

		public static IPlatformMemoryStat<ulong> CreateUInt64(string name)
		{
			IPlatformMemoryStat<ulong> platformMemoryStat = PlatformMemoryStat<ulong>.Create(name);
			platformMemoryStat.RenderValue = new PlatformMemoryRenderValue<ulong>(PlatformMemoryStat.<CreateUInt64>g__RenderValue|5_0);
			platformMemoryStat.RenderDelta = new PlatformMemoryRenderDelta<ulong>(PlatformMemoryStat.<CreateUInt64>g__RenderDelta|5_1);
			return platformMemoryStat;
		}

		public static IPlatformMemoryStat<float> CreateFloat(string name)
		{
			IPlatformMemoryStat<float> platformMemoryStat = PlatformMemoryStat<float>.Create(name);
			platformMemoryStat.RenderValue = new PlatformMemoryRenderValue<float>(PlatformMemoryStat.<CreateFloat>g__RenderValue|6_0);
			platformMemoryStat.RenderDelta = new PlatformMemoryRenderDelta<float>(PlatformMemoryStat.<CreateFloat>g__RenderDelta|6_1);
			return platformMemoryStat;
		}

		public static IPlatformMemoryStat<double> CreateDouble(string name)
		{
			IPlatformMemoryStat<double> platformMemoryStat = PlatformMemoryStat<double>.Create(name);
			platformMemoryStat.RenderValue = new PlatformMemoryRenderValue<double>(PlatformMemoryStat.<CreateDouble>g__RenderValue|7_0);
			platformMemoryStat.RenderDelta = new PlatformMemoryRenderDelta<double>(PlatformMemoryStat.<CreateDouble>g__RenderDelta|7_1);
			return platformMemoryStat;
		}

		[CompilerGenerated]
		[PublicizedFrom(EAccessModifier.Internal)]
		public static void <Create>g__RenderValue|0_0<T>(StringBuilder builder, T value)
		{
			builder.AppendFormat("{0}", value);
		}

		[CompilerGenerated]
		[PublicizedFrom(EAccessModifier.Internal)]
		public static void <CreateBytes>g__RenderValue|1_0(StringBuilder builder, long value)
		{
			PlatformMemoryStat.<CreateBytes>g__RenderSize|1_2(builder, value);
		}

		[CompilerGenerated]
		[PublicizedFrom(EAccessModifier.Internal)]
		public static void <CreateBytes>g__RenderDelta|1_1(StringBuilder builder, long current, long last)
		{
			if (current == last)
			{
				return;
			}
			PlatformMemoryStat.<CreateBytes>g__RenderSize|1_2(builder, current - last);
		}

		[CompilerGenerated]
		[PublicizedFrom(EAccessModifier.Internal)]
		public static void <CreateBytes>g__RenderSize|1_2(StringBuilder builder, long sizeBytes)
		{
			if (Math.Abs(sizeBytes) < 1024L)
			{
				builder.Append(sizeBytes).Append("  ").Append('B');
				return;
			}
			double num = (double)sizeBytes / 1024.0;
			foreach (char c in "kMGTPE")
			{
				if (Math.Abs(num) < 1024.0)
				{
					builder.AppendFormat("{0:F3} ", num).Append(c).Append('B');
					return;
				}
				num /= 1024.0;
			}
			throw new InvalidOperationException("Should not be reachable... Are there enough prefixes?");
		}

		[CompilerGenerated]
		[PublicizedFrom(EAccessModifier.Internal)]
		public static void <CreateInt32>g__RenderValue|2_0(StringBuilder builder, int value)
		{
			builder.AppendFormat("{0}", value);
		}

		[CompilerGenerated]
		[PublicizedFrom(EAccessModifier.Internal)]
		public static void <CreateInt32>g__RenderDelta|2_1(StringBuilder builder, int current, int last)
		{
			if (current == last)
			{
				return;
			}
			if (current >= last)
			{
				builder.AppendFormat("{0}", current - last);
				return;
			}
			builder.AppendFormat("-{0}", last - current);
		}

		[CompilerGenerated]
		[PublicizedFrom(EAccessModifier.Internal)]
		public static void <CreateUInt32>g__RenderValue|3_0(StringBuilder builder, uint value)
		{
			builder.AppendFormat("{0}", value);
		}

		[CompilerGenerated]
		[PublicizedFrom(EAccessModifier.Internal)]
		public static void <CreateUInt32>g__RenderDelta|3_1(StringBuilder builder, uint current, uint last)
		{
			if (current == last)
			{
				return;
			}
			if (current >= last)
			{
				builder.AppendFormat("{0}", current - last);
				return;
			}
			builder.AppendFormat("-{0}", last - current);
		}

		[CompilerGenerated]
		[PublicizedFrom(EAccessModifier.Internal)]
		public static void <CreateInt64>g__RenderValue|4_0(StringBuilder builder, long value)
		{
			builder.AppendFormat("{0}", value);
		}

		[CompilerGenerated]
		[PublicizedFrom(EAccessModifier.Internal)]
		public static void <CreateInt64>g__RenderDelta|4_1(StringBuilder builder, long current, long last)
		{
			if (current == last)
			{
				return;
			}
			if (current >= last)
			{
				builder.AppendFormat("{0}", current - last);
				return;
			}
			builder.AppendFormat("-{0}", last - current);
		}

		[CompilerGenerated]
		[PublicizedFrom(EAccessModifier.Internal)]
		public static void <CreateUInt64>g__RenderValue|5_0(StringBuilder builder, ulong value)
		{
			builder.AppendFormat("{0}", value);
		}

		[CompilerGenerated]
		[PublicizedFrom(EAccessModifier.Internal)]
		public static void <CreateUInt64>g__RenderDelta|5_1(StringBuilder builder, ulong current, ulong last)
		{
			if (current == last)
			{
				return;
			}
			if (current >= last)
			{
				builder.AppendFormat("{0}", current - last);
				return;
			}
			builder.AppendFormat("-{0}", last - current);
		}

		[CompilerGenerated]
		[PublicizedFrom(EAccessModifier.Internal)]
		public static void <CreateFloat>g__RenderValue|6_0(StringBuilder builder, float value)
		{
			builder.AppendFormat("{0}", value);
		}

		[CompilerGenerated]
		[PublicizedFrom(EAccessModifier.Internal)]
		public static void <CreateFloat>g__RenderDelta|6_1(StringBuilder builder, float current, float last)
		{
			if (current == last)
			{
				return;
			}
			builder.AppendFormat("{0}", current - last);
		}

		[CompilerGenerated]
		[PublicizedFrom(EAccessModifier.Internal)]
		public static void <CreateDouble>g__RenderValue|7_0(StringBuilder builder, double value)
		{
			builder.AppendFormat("{0}", value);
		}

		[CompilerGenerated]
		[PublicizedFrom(EAccessModifier.Internal)]
		public static void <CreateDouble>g__RenderDelta|7_1(StringBuilder builder, double current, double last)
		{
			if (current == last)
			{
				return;
			}
			builder.AppendFormat("{0}", current - last);
		}
	}
}
