using System;
using System.Collections.Generic;

namespace Platform.LAN
{
	public class LANServerCacheControl
	{
		public LANServerCacheControl(TimeSpan updateInterval, TimeSpan timeout)
		{
			this.timeout = timeout;
			this.updateInterval = updateInterval;
		}

		public bool IsUpdateRequired(string addressString, int port)
		{
			LANServerCacheControl.ServerKey serverKey = new LANServerCacheControl.ServerKey(addressString, port);
			LANServerCacheControl.UpdateTimes updateTimes;
			if (!this.lastServerUpdateTimes.TryGetValue(serverKey, out updateTimes))
			{
				this.lastServerUpdateTimes[serverKey] = new LANServerCacheControl.UpdateTimes
				{
					lastServerCheckedTime = DateTime.Now
				};
				return true;
			}
			TimeSpan timeSpan = DateTime.Now - updateTimes.lastServerCheckedTime;
			updateTimes.lastServerCheckedTime = DateTime.Now;
			this.lastServerUpdateTimes[serverKey] = updateTimes;
			if (timeSpan > this.timeout)
			{
				Log.Out(string.Format("[{0}] server timed out, update needed. Last checked {1:F2} seconds ago", "LANServerCacheControl", timeSpan.TotalSeconds));
				return true;
			}
			TimeSpan timeSpan2 = DateTime.Now - updateTimes.lastRulesUpdateTime;
			if (timeSpan2 > this.updateInterval)
			{
				Log.Out(string.Format("[{0}] found known server, last updated {1:F2} seconds ago", "LANServerCacheControl", timeSpan2.TotalSeconds));
				return true;
			}
			return false;
		}

		public void SetUpdated(string addressString, int port)
		{
			LANServerCacheControl.ServerKey serverKey = new LANServerCacheControl.ServerKey(addressString, port);
			LANServerCacheControl.UpdateTimes updateTimes;
			if (!this.lastServerUpdateTimes.TryGetValue(serverKey, out updateTimes))
			{
				this.lastServerUpdateTimes[serverKey] = new LANServerCacheControl.UpdateTimes
				{
					lastServerCheckedTime = DateTime.Now,
					lastRulesUpdateTime = DateTime.Now
				};
				return;
			}
			updateTimes.lastRulesUpdateTime = DateTime.Now;
			this.lastServerUpdateTimes[serverKey] = updateTimes;
		}

		public void Clear()
		{
			this.lastServerUpdateTimes.Clear();
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public TimeSpan timeout;

		[PublicizedFrom(EAccessModifier.Private)]
		public TimeSpan updateInterval;

		[PublicizedFrom(EAccessModifier.Private)]
		public Dictionary<LANServerCacheControl.ServerKey, LANServerCacheControl.UpdateTimes> lastServerUpdateTimes = new Dictionary<LANServerCacheControl.ServerKey, LANServerCacheControl.UpdateTimes>();

		[PublicizedFrom(EAccessModifier.Private)]
		public struct ServerKey : IEquatable<LANServerCacheControl.ServerKey>
		{
			public ServerKey(string _ipAddress, int _port)
			{
				this.ipAddress = _ipAddress;
				this.port = _port;
			}

			public override int GetHashCode()
			{
				return this.ipAddress.GetHashCode() ^ this.port;
			}

			public bool Equals(LANServerCacheControl.ServerKey _other)
			{
				return this.ipAddress.Equals(_other.ipAddress) && this.port == _other.port;
			}

			public readonly string ipAddress;

			public readonly int port;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public struct UpdateTimes
		{
			public DateTime lastServerCheckedTime;

			public DateTime lastRulesUpdateTime;
		}
	}
}
