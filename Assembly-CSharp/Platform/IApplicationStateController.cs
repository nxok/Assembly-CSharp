using System;

namespace Platform
{
	public interface IApplicationStateController
	{
		event IApplicationStateController.ApplicationStateChanged OnApplicationStateChanged;

		event IApplicationStateController.NetworkStateChanged OnNetworkStateChanged;

		bool NetworkConnectionState { get; }

		ApplicationState CurrentApplicationState { get; }

		void Init(IPlatform owner);

		void Destroy();

		void Update();

		public delegate void ApplicationStateChanged(ApplicationState newState);

		public delegate void NetworkStateChanged(bool connectionState);
	}
}
