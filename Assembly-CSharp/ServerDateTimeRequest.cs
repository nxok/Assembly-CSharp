using System;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

[PublicizedFrom(EAccessModifier.Internal)]
public class ServerDateTimeRequest
{
	public static void GetNtpTimeAsync(Action<ServerDateTimeResult> _onComplete, string _ntpServer = "pool.ntp.org", int _timeoutMilliseconds = 5000)
	{
		ServerDateTimeRequest.<>c__DisplayClass0_0 CS$<>8__locals1 = new ServerDateTimeRequest.<>c__DisplayClass0_0();
		CS$<>8__locals1._ntpServer = _ntpServer;
		CS$<>8__locals1._timeoutMilliseconds = _timeoutMilliseconds;
		CS$<>8__locals1._onComplete = _onComplete;
		Task.Run(delegate
		{
			ServerDateTimeRequest.<>c__DisplayClass0_0.<<GetNtpTimeAsync>b__0>d <<GetNtpTimeAsync>b__0>d;
			<<GetNtpTimeAsync>b__0>d.<>t__builder = AsyncTaskMethodBuilder.Create();
			<<GetNtpTimeAsync>b__0>d.<>4__this = CS$<>8__locals1;
			<<GetNtpTimeAsync>b__0>d.<>1__state = -1;
			<<GetNtpTimeAsync>b__0>d.<>t__builder.Start<ServerDateTimeRequest.<>c__DisplayClass0_0.<<GetNtpTimeAsync>b__0>d>(ref <<GetNtpTimeAsync>b__0>d);
			return <<GetNtpTimeAsync>b__0>d.<>t__builder.Task;
		});
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static async Task<ServerDateTimeResult> FetchNtpTimeAsync(string _ntpServer, int _timeoutMilliseconds)
	{
		byte[] array = new byte[48];
		int num = 0;
		int num2 = 3;
		int num3 = 3;
		array[0] = (byte)((num << 6) | (num2 << 3) | num3);
		using (UdpClient client = new UdpClient())
		{
			try
			{
				client.Client.ReceiveTimeout = _timeoutMilliseconds;
				await client.SendAsync(array, array.Length, _ntpServer, 123);
				array = (await client.ReceiveAsync()).Buffer;
			}
			catch (SocketException ex) when (ex.SocketErrorCode == SocketError.TimedOut)
			{
				Log.Warning(string.Format("Request to NTP server '{0}' timed out after {1} ms.", _ntpServer, _timeoutMilliseconds), new object[] { ex });
				return new ServerDateTimeResult(true, true, 0);
			}
			catch (Exception ex2)
			{
				Log.Error("Network error while communicating with NTP server '" + _ntpServer + "': " + ex2.Message, new object[] { ex2 });
				return new ServerDateTimeResult(true, true, 0);
			}
		}
		UdpClient client = null;
		ServerDateTimeResult serverDateTimeResult;
		try
		{
			ulong num4 = (ulong)BitConverter.ToUInt32(array, 40);
			ulong num5 = (ulong)BitConverter.ToUInt32(array, 44);
			num4 = (ulong)ServerDateTimeRequest.SwapEndianness(num4);
			ulong num6 = (ulong)ServerDateTimeRequest.SwapEndianness(num5);
			ulong num7 = num4 - (ulong)(-2085978496);
			double num8 = num6 * 1000.0 / 4294967296.0;
			serverDateTimeResult = new ServerDateTimeResult(true, false, (int)(DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(num7).AddMilliseconds(num8).ToLocalTime()).TotalSeconds);
		}
		catch
		{
			Log.Error("Error parsing the NTP server response.");
			serverDateTimeResult = new ServerDateTimeResult(true, true, 0);
		}
		return serverDateTimeResult;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static uint SwapEndianness(ulong _x)
	{
		return (uint)(((_x & 255UL) << 24) + ((_x & 65280UL) << 8) + ((_x & 16711680UL) >> 8) + ((_x & (ulong)(-16777216)) >> 24));
	}
}
