using System;

public enum NetworkConnectionError
{
	InternalDirectConnectFailed = -5,
	EmptyConnectTarget,
	IncorrectParameters,
	CreateSocketOrThreadFailure,
	AlreadyConnectedToAnotherServer,
	NoError,
	ConnectionFailed = 15,
	AlreadyConnectedToServer,
	TooManyConnectedPlayers = 18,
	RSAPublicKeyMismatch = 21,
	ConnectionBanned,
	InvalidPassword,
	NATTargetNotConnected = 69,
	NATTargetConnectionLost = 71,
	NATPunchthroughFailed = 73,
	InvalidPort,
	RestartRequired
}
