using System;
using System.Collections.ObjectModel;
using Platform;

public static class ConsoleHelper
{
	public static bool ParseParamBool(string _param, bool _invalidStringsAsFalse = false)
	{
		if (_param.EqualsCaseInsensitive("y") || _param.EqualsCaseInsensitive("yes") || _param.EqualsCaseInsensitive("true") || _param.EqualsCaseInsensitive("on") || _param.EqualsCaseInsensitive("1"))
		{
			return true;
		}
		if (_param.EqualsCaseInsensitive("n") || _param.EqualsCaseInsensitive("no") || _param.EqualsCaseInsensitive("false") || _param.EqualsCaseInsensitive("off") || _param.EqualsCaseInsensitive("0"))
		{
			return false;
		}
		if (_invalidStringsAsFalse)
		{
			return false;
		}
		throw new ArgumentException("Not a bool value");
	}

	public static Entity ParseParamEntityIdToEntity(string _param, bool _playersOnly = true)
	{
		int num;
		if (!int.TryParse(_param, out num))
		{
			return null;
		}
		if (_playersOnly)
		{
			if (GameManager.Instance.World.Players.dict.ContainsKey(num))
			{
				return GameManager.Instance.World.Players.dict[num];
			}
		}
		else if (GameManager.Instance.World.Entities.dict.ContainsKey(num))
		{
			return GameManager.Instance.World.Entities.dict[num];
		}
		return null;
	}

	public static ClientInfo ParseParamEntityIdToClientInfo(string _param)
	{
		int num;
		if (!int.TryParse(_param, out num))
		{
			return null;
		}
		return SingletonMonoBehaviour<ConnectionManager>.Instance.Clients.ForEntityId(num);
	}

	public static bool ParseParamSteamGroupIdValid(string _param)
	{
		ulong num;
		return _param.Length == 18 && ulong.TryParse(_param, out num);
	}

	public static bool ParamIsLocalPlayer(string _param, bool _ignoreCase = true, bool _ignoreBlanks = false)
	{
		if (GameManager.IsDedicatedServer)
		{
			return false;
		}
		EntityPlayerLocal primaryPlayer = GameManager.Instance.World.GetPrimaryPlayer();
		PlatformUserIdentifierAbs platformUserIdentifierAbs;
		if (PlatformUserIdentifierAbs.TryFromCombinedString(_param, out platformUserIdentifierAbs) && platformUserIdentifierAbs.Equals(PlatformManager.InternalLocalUserIdentifier))
		{
			return true;
		}
		int num;
		if (int.TryParse(_param, out num) && primaryPlayer.entityId == num)
		{
			return true;
		}
		if (_ignoreBlanks)
		{
			_param = _param.Replace(" ", "");
		}
		string text = primaryPlayer.EntityName ?? string.Empty;
		if (_ignoreBlanks)
		{
			text = text.Replace(" ", "");
		}
		return string.Equals(text, _param, _ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);
	}

	public static ClientInfo ParseParamPlayerName(string _param, bool _ignoreCase = true, bool _ignoreBlanks = false)
	{
		return SingletonMonoBehaviour<ConnectionManager>.Instance.Clients.GetForPlayerName(_param, _ignoreCase, _ignoreBlanks);
	}

	public static PlatformUserIdentifierAbs ParseParamUserId(string _param)
	{
		return PlatformUserIdentifierAbs.FromCombinedString(_param, true);
	}

	public static ClientInfo ParseParamIdOrName(string _param, bool _ignoreCase = true, bool _ignoreBlanks = false)
	{
		return SingletonMonoBehaviour<ConnectionManager>.Instance.Clients.GetForNameOrId(_param, _ignoreCase, _ignoreBlanks);
	}

	public static int ParseParamPartialNameOrId(string _param, out PlatformUserIdentifierAbs _id, out ClientInfo _cInfo, bool _sendError = true)
	{
		_id = null;
		_cInfo = null;
		ClientInfo clientInfo = ConsoleHelper.ParseParamIdOrName(_param, true, false);
		if (clientInfo != null)
		{
			_id = clientInfo.InternalId;
			_cInfo = clientInfo;
			return 1;
		}
		if (PlatformUserIdentifierAbs.TryFromCombinedString(_param, out _id))
		{
			return 1;
		}
		ClientInfo clientInfo2 = null;
		int num = 0;
		ReadOnlyCollection<ClientInfo> list = SingletonMonoBehaviour<ConnectionManager>.Instance.Clients.List;
		for (int i = 0; i < list.Count; i++)
		{
			ClientInfo clientInfo3 = list[i];
			if (clientInfo3.playerName.ContainsCaseInsensitive(_param))
			{
				num++;
				clientInfo2 = clientInfo3;
			}
		}
		if (num == 1)
		{
			_id = clientInfo2.InternalId;
			_cInfo = clientInfo2;
		}
		else
		{
			_id = null;
			_cInfo = null;
			if (_sendError)
			{
				if (num == 0)
				{
					SingletonMonoBehaviour<SdtdConsole>.Instance.Output("\"" + _param + "\" is not a valid entity id, player name or user id.");
				}
				else if (num > 1)
				{
					SingletonMonoBehaviour<SdtdConsole>.Instance.Output("\"" + _param + "\" matches multiple player names.");
				}
			}
		}
		return num;
	}
}
