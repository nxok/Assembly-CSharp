using System;

namespace Platform.XBL
{
	public class ApplicationStateController : IApplicationStateController
	{
		public event IApplicationStateController.ApplicationStateChanged OnApplicationStateChanged;

		public event IApplicationStateController.NetworkStateChanged OnNetworkStateChanged;

		public ApplicationState CurrentApplicationState
		{
			get
			{
				return ApplicationState.Foreground;
			}
		}

		public bool NetworkConnectionState
		{
			get
			{
				return true;
			}
		}

		public void Init(IPlatform owner)
		{
		}

		public void Destroy()
		{
		}

		public void Update()
		{
		}
	}
}
