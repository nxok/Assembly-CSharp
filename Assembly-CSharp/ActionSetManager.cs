using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using InControl;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ActionSetManager
{
	public bool Empty
	{
		get
		{
			return this.PlayerActions.Count <= 0;
		}
	}

	public PlayerActionSet Top
	{
		get
		{
			return this.PlayerActions[this.PlayerActions.Count - 1];
		}
	}

	public static ActionSetManager.EDebugLevel DebugLevel
	{
		get
		{
			return ActionSetManager.debug;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	static ActionSetManager()
	{
		string launchArgument = GameUtils.GetLaunchArgument("debuginput");
		if (launchArgument != null)
		{
			if (launchArgument == "verbose")
			{
				ActionSetManager.debug = ActionSetManager.EDebugLevel.Verbose;
				return;
			}
			ActionSetManager.debug = ActionSetManager.EDebugLevel.Normal;
		}
	}

	public void Insert(PlayerActionSet _playerAction, int _index, string _windowName = null)
	{
		if (ActionSetManager.debug != ActionSetManager.EDebugLevel.Off)
		{
			Log.Out("LocalPlayerInput.Insert ({2} - {0}):{1}", new object[]
			{
				_playerAction.GetType().FullName,
				(ActionSetManager.debug == ActionSetManager.EDebugLevel.Verbose) ? ("\n" + StackTraceUtility.ExtractStackTrace()) : "",
				_windowName
			});
		}
		if (_playerAction == null)
		{
			Log.Warning("LocalPlayerInput::Insert - Inserting a null input onto stack.");
		}
		if (!this.Empty)
		{
			this.Top.Enabled = false;
		}
		_playerAction.Enabled = false;
		this.PlayerActions.Insert(_index, _playerAction);
		this.Top.Enabled = true;
		if (ActionSetManager.debug != ActionSetManager.EDebugLevel.Off)
		{
			this.LogActionSets();
		}
	}

	public void Remove(PlayerActionSet _playerAction, int _minIndex, string _windowName = null)
	{
		if (ActionSetManager.debug != ActionSetManager.EDebugLevel.Off)
		{
			Log.Out("LocalPlayerInput.Remove ({2} - {0}):{1}", new object[]
			{
				_playerAction.GetType().FullName,
				(ActionSetManager.debug == ActionSetManager.EDebugLevel.Verbose) ? ("\n" + StackTraceUtility.ExtractStackTrace()) : "",
				_windowName
			});
		}
		if (_playerAction == null)
		{
			Log.Warning("LocalPlayerInput::Remove - Trying to remove a null input from the stack.");
		}
		if (this.Empty)
		{
			Log.Warning("LocalPlayerInput::Remove - Removing input from an empty stack.");
			return;
		}
		this.Top.Enabled = false;
		_playerAction.Enabled = false;
		int num = -1;
		for (int i = this.PlayerActions.Count - 1; i >= _minIndex; i--)
		{
			if (this.PlayerActions[i] == _playerAction)
			{
				num = i;
				break;
			}
		}
		if (num >= 0)
		{
			this.PlayerActions.RemoveAt(num);
		}
		else
		{
			Log.Warning(string.Format("LocalPlayerInput::Remove - Failed to find action set of type '{0}' with a min index of {1} to remove.", _playerAction.GetType().FullName, _minIndex));
		}
		if (!this.Empty)
		{
			this.Top.Enabled = true;
		}
		if (ActionSetManager.debug != ActionSetManager.EDebugLevel.Off)
		{
			this.LogActionSets();
		}
	}

	public void Push(PlayerActionSet _playerAction)
	{
		if (_playerAction == null)
		{
			Log.Warning("LocalPlayerInput::Push - Pushing a null input onto stack.");
		}
		this.PushInternal(_playerAction, null);
	}

	public void Push(GUIWindow _window)
	{
		if (((_window != null) ? _window.GetActionSet() : null) == null)
		{
			Log.Warning("LocalPlayerInput::Push - Pushing a null input onto stack.");
		}
		this.PushInternal((_window != null) ? _window.GetActionSet() : null, (_window != null) ? _window.Id : null);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void PushInternal(PlayerActionSet _playerAction, string _windowName = null)
	{
		if (ActionSetManager.debug != ActionSetManager.EDebugLevel.Off)
		{
			Log.Out("LocalPlayerInput.Push ({2} - {0}):{1}", new object[]
			{
				_playerAction.GetType().FullName,
				(ActionSetManager.debug == ActionSetManager.EDebugLevel.Verbose) ? ("\n" + StackTraceUtility.ExtractStackTrace()) : "",
				_windowName
			});
		}
		if (!this.Empty)
		{
			this.Top.Enabled = false;
		}
		this.PlayerActions.Add(_playerAction);
		this.Top.Enabled = true;
		if (ActionSetManager.debug != ActionSetManager.EDebugLevel.Off)
		{
			this.LogActionSets();
		}
	}

	public void Pop(GUIWindow _window = null)
	{
		if (ActionSetManager.debug != ActionSetManager.EDebugLevel.Off)
		{
			Log.Out("LocalPlayerInput.Pop ({1}):{0}", new object[]
			{
				(ActionSetManager.debug == ActionSetManager.EDebugLevel.Verbose) ? ("\n" + StackTraceUtility.ExtractStackTrace()) : "",
				(_window != null) ? _window.Id : null
			});
		}
		if (this.Empty)
		{
			Log.Warning("LocalPlayerInput::Pop - Popping input from an empty stack.");
			return;
		}
		int num = this.PlayerActions.Count - 1;
		if (_window != null)
		{
			PlayerActionsBase actionSet = _window.GetActionSet();
			if (actionSet != null && actionSet != this.PlayerActions[num])
			{
				Log.Warning("LocalPlayerInput::Pop - Tried to pop a different action set from what belongs to window " + _window.Id);
				return;
			}
		}
		this.Top.Enabled = false;
		this.PlayerActions.RemoveAt(num);
		if (!this.Empty)
		{
			this.Top.Enabled = true;
		}
		if (ActionSetManager.debug != ActionSetManager.EDebugLevel.Off)
		{
			this.LogActionSets();
		}
	}

	public void LogActionSets()
	{
		string text = "";
		for (int i = 0; i < this.PlayerActions.Count; i++)
		{
			text = string.Concat(new string[]
			{
				text,
				this.PlayerActions[i].GetType().Name,
				" (",
				this.PlayerActions[i].Enabled.ToString(),
				"), "
			});
		}
		string text2 = "";
		IPlatform nativePlatform = PlatformManager.NativePlatform;
		int? num;
		if (nativePlatform == null)
		{
			num = null;
		}
		else
		{
			PlayerInputManager input = nativePlatform.Input;
			if (input == null)
			{
				num = null;
			}
			else
			{
				ReadOnlyCollection<PlayerActionsBase> actionSets = input.ActionSets;
				num = ((actionSets != null) ? new int?(actionSets.Count) : null);
			}
		}
		int? num2 = num;
		if (num2.GetValueOrDefault() > 0)
		{
			for (int j = 0; j < PlatformManager.NativePlatform.Input.ActionSets.Count; j++)
			{
				text2 += string.Format("{0} ({1}), ", PlatformManager.NativePlatform.Input.ActionSets[j].GetType().Name, PlatformManager.NativePlatform.Input.ActionSets[j].Enabled);
			}
		}
		Log.Out("ActionSets: Stack: {0} --- All: {1}", new object[] { text, text2 });
	}

	public void Reset()
	{
		while (!this.Empty)
		{
			this.Pop(null);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<PlayerActionSet> PlayerActions = new List<PlayerActionSet>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ActionSetManager.EDebugLevel debug = ActionSetManager.EDebugLevel.Off;

	public enum EDebugLevel
	{
		Off,
		Normal,
		Verbose
	}
}
