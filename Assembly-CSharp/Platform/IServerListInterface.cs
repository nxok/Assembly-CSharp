using System;
using System.Collections.Generic;

namespace Platform
{
	public interface IServerListInterface
	{
		bool IsPrefiltered { get; }

		void Init(IPlatform _owner);

		void RegisterGameServerFoundCallback(GameServerFoundCallback _serverFound, MaxResultsReachedCallback _maxResultsCallback, ServerSearchErrorCallback _sessionSearchErrorCallback);

		bool IsRefreshing { get; }

		void StartSearch(IList<IServerListInterface.ServerFilter> _activeFilters);

		void StopSearch();

		void Disconnect();

		void GetSingleServerDetails(GameServerInfo _serverInfo, EServerRelationType _relation, GameServerFoundCallback _callback);

		public class ServerFilter
		{
			public ServerFilter(string _name, IServerListInterface.ServerFilter.EServerFilterType _type = IServerListInterface.ServerFilter.EServerFilterType.Any, int _intMinValue = 0, int _intMaxValue = 0, bool _boolValue = false, string _stringNeedle = null)
			{
				this.Name = _name;
				this.Type = _type;
				this.IntMinValue = _intMinValue;
				this.IntMaxValue = _intMaxValue;
				this.BoolValue = _boolValue;
				this.StringNeedle = _stringNeedle;
			}

			public readonly string Name;

			public readonly IServerListInterface.ServerFilter.EServerFilterType Type;

			public readonly int IntMinValue;

			public readonly int IntMaxValue;

			public readonly bool BoolValue;

			public readonly string StringNeedle;

			public enum EServerFilterType
			{
				Any,
				BoolValue,
				IntValue,
				IntNotValue,
				IntMin,
				IntMax,
				IntRange,
				StringValue,
				StringContains
			}
		}
	}
}
