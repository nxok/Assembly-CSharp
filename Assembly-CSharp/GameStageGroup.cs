using System;
using System.Collections.Generic;
using System.Text;

public sealed class GameStageGroup
{
	public GameStageGroup(GameStageDefinition _spawner)
	{
		this.spawner = _spawner;
	}

	public static void AddGameStageGroup(string _fullName, GameStageGroup _group)
	{
		string text = GameStageGroup.CleanName(_fullName);
		GameStageGroup.groups.Add(text, _group);
		GameStageGroup.groupsFullName.Add(_fullName, _group);
	}

	public static GameStageGroup TryGet(string _name)
	{
		GameStageGroup gameStageGroup;
		if (GameStageGroup.groups.TryGetValue(_name, out gameStageGroup))
		{
			return gameStageGroup;
		}
		return null;
	}

	public static void Clear()
	{
		GameStageGroup.groups.Clear();
		GameStageGroup.groupsFullName.Clear();
	}

	public static Dictionary<string, GameStageGroup> Groups
	{
		get
		{
			return GameStageGroup.groupsFullName;
		}
	}

	public static string CleanName(string _name)
	{
		if (_name.Length > 0 && char.IsDigit(_name[0]))
		{
			_name = _name.Substring(1);
		}
		else if (_name.StartsWith("S_"))
		{
			int num = 2;
			if (_name.StartsWith("S_-"))
			{
				num = 3;
			}
			return _name.Substring(num).Replace("_", "");
		}
		return _name;
	}

	public static string MakeDisplayName(string _name)
	{
		bool flag = false;
		foreach (char c in _name)
		{
			if (!char.IsDigit(c))
			{
				if (char.IsUpper(c) && flag)
				{
					GameStageGroup.stringBuilder.Append(' ');
				}
				GameStageGroup.stringBuilder.Append(c);
				flag = true;
			}
		}
		_name = GameStageGroup.stringBuilder.ToString();
		GameStageGroup.stringBuilder.Clear();
		return _name;
	}

	public const string cDefaultGroupName = "GroupGenericZombie";

	public readonly GameStageDefinition spawner;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Dictionary<string, GameStageGroup> groups = new Dictionary<string, GameStageGroup>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Dictionary<string, GameStageGroup> groupsFullName = new Dictionary<string, GameStageGroup>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static StringBuilder stringBuilder = new StringBuilder();
}
