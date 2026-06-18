using System;
using System.Collections.Generic;
using Platform;
using UnityEngine;

public class ServerListManager
{
	public static ServerListManager Instance
	{
		get
		{
			ServerListManager serverListManager;
			if ((serverListManager = ServerListManager.instance) == null)
			{
				serverListManager = (ServerListManager.instance = new ServerListManager());
			}
			return serverListManager;
		}
	}

	public bool IsRefreshing
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public bool IsPrefilteredSearch
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public void StartSearch(List<IServerListInterface.ServerFilter> _activeFilters)
	{
		this.IsRefreshing = true;
		foreach (IServerListInterface serverListInterface in this.serverLists)
		{
			serverListInterface.StartSearch(_activeFilters);
		}
	}

	public void StopSearch()
	{
		this.IsRefreshing = false;
		foreach (IServerListInterface serverListInterface in this.serverLists)
		{
			serverListInterface.StopSearch();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public ServerListManager()
	{
		if (GameManager.IsDedicatedServer)
		{
			return;
		}
		Application.wantsToQuit += delegate
		{
			this.Disconnect();
			return true;
		};
		IList<IServerListInterface> serverListInterfaces = PlatformManager.MultiPlatform.ServerListInterfaces;
		if (serverListInterfaces != null)
		{
			this.serverLists.AddRange(serverListInterfaces);
		}
		using (List<IServerListInterface>.Enumerator enumerator = this.serverLists.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				if (enumerator.Current.IsPrefiltered)
				{
					this.IsPrefilteredSearch = true;
				}
			}
		}
	}

	public void RegisterAdditionalServerList(IServerListInterface _serverList)
	{
		this.serverLists.Add(_serverList);
		if (_serverList.IsPrefiltered)
		{
			this.IsPrefilteredSearch = true;
		}
	}

	public void Disconnect()
	{
		foreach (IServerListInterface serverListInterface in this.serverLists)
		{
			serverListInterface.Disconnect();
		}
		this.IsRefreshing = false;
	}

	public void RegisterGameServerFoundCallback(GameServerFoundCallback _serverFound, MaxResultsReachedCallback _maxResultsCallback, ServerSearchErrorCallback _errorCallback)
	{
		foreach (IServerListInterface serverListInterface in this.serverLists)
		{
			serverListInterface.RegisterGameServerFoundCallback(_serverFound, _maxResultsCallback, _errorCallback);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static ServerListManager instance;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<IServerListInterface> serverLists = new List<IServerListInterface>();
}
