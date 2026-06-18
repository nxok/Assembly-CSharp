using System;
using System.Collections.Generic;
using System.Threading;

namespace Platform.MultiPlatform
{
	public class ServerListAnnouncer : IMasterServerAnnouncer
	{
		public void Init(IPlatform _owner)
		{
		}

		public void Update()
		{
		}

		public bool GameServerInitialized
		{
			get
			{
				IMasterServerAnnouncer serverListAnnouncer = PlatformManager.NativePlatform.ServerListAnnouncer;
				if (serverListAnnouncer == null || serverListAnnouncer.GameServerInitialized)
				{
					IPlatform crossplatformPlatform = PlatformManager.CrossplatformPlatform;
					bool? flag;
					if (crossplatformPlatform == null)
					{
						flag = null;
					}
					else
					{
						IMasterServerAnnouncer serverListAnnouncer2 = crossplatformPlatform.ServerListAnnouncer;
						flag = ((serverListAnnouncer2 != null) ? new bool?(serverListAnnouncer2.GameServerInitialized) : null);
					}
					return flag ?? true;
				}
				return false;
			}
		}

		public string GetServerPorts()
		{
			string text = "";
			IMasterServerAnnouncer serverListAnnouncer = PlatformManager.NativePlatform.ServerListAnnouncer;
			string text2 = ((serverListAnnouncer != null) ? serverListAnnouncer.GetServerPorts() : null);
			if (!string.IsNullOrEmpty(text2))
			{
				if (!string.IsNullOrEmpty(text))
				{
					text += ", ";
				}
				text += text2;
			}
			IPlatform crossplatformPlatform = PlatformManager.CrossplatformPlatform;
			string text3;
			if (crossplatformPlatform == null)
			{
				text3 = null;
			}
			else
			{
				IMasterServerAnnouncer serverListAnnouncer2 = crossplatformPlatform.ServerListAnnouncer;
				text3 = ((serverListAnnouncer2 != null) ? serverListAnnouncer2.GetServerPorts() : null);
			}
			string text4 = text3;
			if (!string.IsNullOrEmpty(text4))
			{
				if (!string.IsNullOrEmpty(text))
				{
					text += ", ";
				}
				text += text4;
			}
			foreach (KeyValuePair<EPlatformIdentifier, IPlatform> keyValuePair in PlatformManager.ServerPlatforms)
			{
				if (keyValuePair.Value.AsServerOnly)
				{
					IMasterServerAnnouncer serverListAnnouncer3 = keyValuePair.Value.ServerListAnnouncer;
					string text5 = ((serverListAnnouncer3 != null) ? serverListAnnouncer3.GetServerPorts() : null);
					if (!string.IsNullOrEmpty(text5))
					{
						if (!string.IsNullOrEmpty(text))
						{
							text += ", ";
						}
						text += text5;
					}
				}
			}
			return text;
		}

		public void AdvertiseServer(Action _onServerRegistered)
		{
			if (PlatformManager.NativePlatform.ServerListAnnouncer != null)
			{
				Interlocked.Increment(ref this.platformsAdvertising);
				PlatformManager.NativePlatform.ServerListAnnouncer.AdvertiseServer(new Action(this.serverRegisteredCallback));
			}
			IPlatform crossplatformPlatform = PlatformManager.CrossplatformPlatform;
			if (((crossplatformPlatform != null) ? crossplatformPlatform.ServerListAnnouncer : null) != null)
			{
				Interlocked.Increment(ref this.platformsAdvertising);
				IPlatform crossplatformPlatform2 = PlatformManager.CrossplatformPlatform;
				if (crossplatformPlatform2 != null)
				{
					IMasterServerAnnouncer serverListAnnouncer = crossplatformPlatform2.ServerListAnnouncer;
					if (serverListAnnouncer != null)
					{
						serverListAnnouncer.AdvertiseServer(new Action(this.serverRegisteredCallback));
					}
				}
			}
			foreach (KeyValuePair<EPlatformIdentifier, IPlatform> keyValuePair in PlatformManager.ServerPlatforms)
			{
				if (keyValuePair.Value.AsServerOnly && keyValuePair.Value.ServerListAnnouncer != null)
				{
					Interlocked.Increment(ref this.platformsAdvertising);
					keyValuePair.Value.ServerListAnnouncer.AdvertiseServer(new Action(this.serverRegisteredCallback));
				}
			}
			if (this.platformsAdvertising == 0)
			{
				_onServerRegistered();
				this.onServerRegistered = null;
				return;
			}
			this.onServerRegistered = _onServerRegistered;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void serverRegisteredCallback()
		{
			if (Interlocked.Decrement(ref this.platformsAdvertising) == 0)
			{
				Action action = this.onServerRegistered;
				if (action != null)
				{
					action();
				}
				this.onServerRegistered = null;
			}
		}

		public void StopServer()
		{
			foreach (KeyValuePair<EPlatformIdentifier, IPlatform> keyValuePair in PlatformManager.ServerPlatforms)
			{
				if (keyValuePair.Value.AsServerOnly)
				{
					IMasterServerAnnouncer serverListAnnouncer = keyValuePair.Value.ServerListAnnouncer;
					if (serverListAnnouncer != null)
					{
						serverListAnnouncer.StopServer();
					}
				}
			}
			IPlatform crossplatformPlatform = PlatformManager.CrossplatformPlatform;
			if (crossplatformPlatform != null)
			{
				IMasterServerAnnouncer serverListAnnouncer2 = crossplatformPlatform.ServerListAnnouncer;
				if (serverListAnnouncer2 != null)
				{
					serverListAnnouncer2.StopServer();
				}
			}
			IMasterServerAnnouncer serverListAnnouncer3 = PlatformManager.NativePlatform.ServerListAnnouncer;
			if (serverListAnnouncer3 == null)
			{
				return;
			}
			serverListAnnouncer3.StopServer();
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public int platformsAdvertising;

		[PublicizedFrom(EAccessModifier.Private)]
		public Action onServerRegistered;
	}
}
