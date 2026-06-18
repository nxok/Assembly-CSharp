using System;
using System.Collections;
using System.Collections.Generic;
using Platform;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class GUIWindowConsole : GUIWindowUGUI
{
	public override string UIPrefabPath
	{
		get
		{
			return "GUI/Prefabs/ConsoleWindow";
		}
	}

	public GUIWindowConsole()
		: base("GUIWindowConsole")
	{
		Log.LogCallbacks += new Log.LogCallbackDelegate(this.LogCallback);
		this.alwaysUsesMouseCursor = true;
		this.components = this.canvas.GetComponent<GUIWindowConsoleComponents>();
		this.scrollRect = this.components.scrollRect;
		this.contentRect = this.components.contentRect;
		this.commandField = this.components.commandField;
		this.commandField.onSubmit.AddListener(new UnityAction<string>(this.EnterCommand));
		this.commandField.shouldActivateOnSelect = !TouchScreenKeyboard.isSupported;
		this.components.closeButton.onClick.AddListener(new UnityAction(this.CloseConsole));
		this.components.openLogsButton.onClick.AddListener(delegate
		{
			GameIO.OpenExplorer(Application.consoleLogPath);
		});
		if ((DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5).IsCurrent())
		{
			this.components.openLogsButton.gameObject.SetActive(false);
		}
		for (int i = 0; i < 5; i++)
		{
			GameObject gameObject = Object.Instantiate<GameObject>(this.components.consoleLinePrefab);
			gameObject.SetActive(false);
			gameObject.transform.SetParent(this.contentRect, false);
			this.linePool.Push(gameObject.GetComponent<Text>());
		}
		PlatformManager.NativePlatform.Input.OnLastInputStyleChanged += this.Input_OnLastInputStyleChanged;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Input_OnLastInputStyleChanged(PlayerInputManager.InputStyle _inputStyle)
	{
		if (_inputStyle == PlayerInputManager.InputStyle.Keyboard)
		{
			this.components.controllerPrompts.SetActive(false);
			return;
		}
		this.components.controllerPrompts.SetActive(true);
		this.components.RefreshButtonPrompts();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Text AllocText()
	{
		Text text;
		if (this.linePool.TryPop(out text))
		{
			text.gameObject.SetActive(true);
			return text;
		}
		GameObject gameObject = Object.Instantiate<GameObject>(this.components.consoleLinePrefab);
		gameObject.transform.SetParent(this.contentRect, false);
		return gameObject.GetComponent<Text>();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void FreeText(Text _text)
	{
		_text.gameObject.SetActive(false);
		this.linePool.Push(_text);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void AddDisplayedLine(GUIWindowConsole.ConsoleLine _line)
	{
		foreach (StringSpan stringSpan in _line.text.GetSplitAnyEnumerator(GUIWindowConsole.lineSeparators, StringSplitOptions.RemoveEmptyEntries))
		{
			Text text;
			if (this.displayedLines.Count == 300)
			{
				text = this.displayedLines.Dequeue();
			}
			else
			{
				text = this.AllocText();
			}
			if (stringSpan.Length > 500)
			{
				text.text = SpanUtils.Concat(stringSpan.Slice(0, 500), "...");
			}
			else
			{
				text.text = stringSpan.ToString();
			}
			text.color = _line.GetLogColor();
			text.transform.SetAsLastSibling();
			this.displayedLines.Enqueue(text);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ClearDisplayedLines()
	{
		Text text;
		while (this.displayedLines.TryDequeue(out text))
		{
			this.FreeText(text);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void shutdown()
	{
		Log.LogCallbacks -= new Log.LogCallbackDelegate(this.LogCallback);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void LogCallback(string _msg, string _trace, LogType _type)
	{
		switch (_type)
		{
		case 1:
			this.openConsole(_msg);
			break;
		case 4:
			this.openConsole(_msg);
			break;
		}
		this.internalAddLine(new GUIWindowConsole.ConsoleLine(_msg, _trace, _type));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void openConsole(string _logString)
	{
		if (Submission.Enabled)
		{
			return;
		}
		if (_logString.StartsWith("Can't send RPC"))
		{
			return;
		}
		if (_logString.StartsWith("You are trying to load data from"))
		{
			return;
		}
		this.windowManager.Open(this, false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void addLines(List<string> _lines)
	{
		for (int i = 0; i < _lines.Count; i++)
		{
			this.addLine(_lines[i]);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void addLine(string _line)
	{
		this.internalAddLine(new GUIWindowConsole.ConsoleLine(_line, string.Empty, 3));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void internalAddLine(GUIWindowConsole.ConsoleLine consoleLine)
	{
		Queue<GUIWindowConsole.ConsoleLine> queue = this.linesToAdd;
		lock (queue)
		{
			this.linesToAdd.Enqueue(consoleLine);
			while (this.linesToAdd.Count > 300)
			{
				this.linesToAdd.Dequeue();
			}
		}
	}

	public override void Update()
	{
		base.Update();
		this.scrolledToBottom = this.scrollRect.verticalNormalizedPosition < 0.1f;
		bool flag = false;
		Queue<GUIWindowConsole.ConsoleLine> queue = this.linesToAdd;
		lock (queue)
		{
			if (this.linesToAdd.Count > 0)
			{
				flag = true;
				foreach (GUIWindowConsole.ConsoleLine consoleLine in this.linesToAdd)
				{
					this.AddDisplayedLine(consoleLine);
				}
				this.linesToAdd.Clear();
			}
		}
		if (flag && this.scrolledToBottom)
		{
			Canvas.ForceUpdateCanvases();
			this.scrollRect.verticalNormalizedPosition = 0f;
		}
		if (this.bFirstTime)
		{
			if (!TouchScreenKeyboard.isSupported)
			{
				this.commandField.Select();
				this.commandField.ActivateInputField();
			}
			this.scrollRect.verticalNormalizedPosition = 0f;
			this.bFirstTime = false;
		}
		if (this.bUpdateCursor)
		{
			this.commandField.MoveTextEnd(false);
			this.bUpdateCursor = false;
		}
		if (Input.GetKeyDown(273))
		{
			this.PreviousCommand();
		}
		else if (Input.GetKeyDown(274))
		{
			this.NextCommand();
		}
		else if (Input.GetKeyDown(280))
		{
			float num = this.CalculateNormalizedPageSize();
			this.scrollRect.verticalNormalizedPosition = Math.Min(this.scrollRect.verticalNormalizedPosition + num, 1f);
		}
		else if (Input.GetKeyDown(281))
		{
			float num2 = this.CalculateNormalizedPageSize();
			this.scrollRect.verticalNormalizedPosition = Math.Max(this.scrollRect.verticalNormalizedPosition - num2, 0f);
		}
		PlayerActionsLocal playerInput = this.playerUI.playerInput;
		PlayerActionsGUI playerActionsGUI = ((playerInput != null) ? playerInput.GUIActions : null);
		if (playerActionsGUI == null)
		{
			return;
		}
		if (playerActionsGUI.Submit.WasReleased)
		{
			this.EnterCommand(this.commandField.text);
		}
		else if (playerActionsGUI.DPad_Up.WasReleased && playerActionsGUI.DPad_Up.LastDeviceClass != 1)
		{
			this.PreviousCommand();
		}
		else if (playerActionsGUI.DPad_Down.WasReleased && playerActionsGUI.DPad_Down.LastDeviceClass != 1)
		{
			this.NextCommand();
		}
		else if (playerActionsGUI.DPad_Left.WasReleased && playerActionsGUI.DPad_Down.LastDeviceClass != 1)
		{
			IVirtualKeyboard virtualKeyboard = PlatformManager.NativePlatform.VirtualKeyboard;
			if (virtualKeyboard != null)
			{
				virtualKeyboard.Open("Enter Command", this.commandField.text, new Action<bool, string>(this.OnTextReceived), 0, false, 200U);
			}
		}
		else if (this.playerUI.playerInput.PermanentActions.Cancel.WasReleased || PlayerActionsGlobal.Instance.Console.WasPressed)
		{
			ThreadManager.StartCoroutine(this.closeLater());
		}
		float y = playerActionsGUI.Camera.Vector.y;
		if (y != 0f)
		{
			float num3 = this.CalculateNormalizedPageSize();
			this.scrollRect.verticalNormalizedPosition = Math.Max(this.scrollRect.verticalNormalizedPosition + num3 * y * 0.05f, 0f);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public float CalculateNormalizedPageSize()
	{
		float height = this.scrollRect.viewport.rect.height;
		float num = this.scrollRect.content.rect.height - height;
		if (num > height)
		{
			return Math.Max(height / num, 0.01f);
		}
		return 1f;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnTextReceived(bool _success, string _text)
	{
		if (_success)
		{
			this.commandField.text = _text;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator closeLater()
	{
		yield return null;
		this.CloseConsole();
		yield break;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CloseConsole()
	{
		this.windowManager.Close(this, false);
		this.commandField.text = string.Empty;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void EnterCommand(string _command)
	{
		if (_command.Length > 0)
		{
			if (_command == "clear")
			{
				this.Clear();
			}
			else
			{
				this.scrollRect.verticalNormalizedPosition = 0f;
				this.internalAddLine(new GUIWindowConsole.ConsoleLine("> " + _command, string.Empty, 3));
				if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsClient)
				{
					GUIWindowConsole.AddLines(SingletonMonoBehaviour<SdtdConsole>.Instance.ExecuteSync(_command, null));
				}
				else
				{
					SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageConsoleCmdServer>().Setup(_command), false);
				}
			}
			if (this.lastCommands.Count == 0 || !this.lastCommands[this.lastCommands.Count - 1].Equals(_command))
			{
				if (this.lastCommands.Contains(_command))
				{
					this.lastCommands.Remove(_command);
				}
				this.lastCommands.Add(_command);
			}
			this.lastCommandsIdx = this.lastCommands.Count;
			this.commandField.text = "";
			if (!TouchScreenKeyboard.isSupported)
			{
				this.commandField.Select();
				this.commandField.ActivateInputField();
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void PreviousCommand()
	{
		if (this.lastCommands.Count > 0)
		{
			this.lastCommandsIdx = Mathf.Max(0, this.lastCommandsIdx - 1);
			this.commandField.text = this.lastCommands[this.lastCommandsIdx];
			if (!TouchScreenKeyboard.isSupported)
			{
				this.commandField.Select();
				this.commandField.ActivateInputField();
			}
			this.bUpdateCursor = true;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void NextCommand()
	{
		if (this.lastCommands.Count > 0)
		{
			this.lastCommandsIdx = Mathf.Min(this.lastCommands.Count, this.lastCommandsIdx + 1);
			if (this.lastCommandsIdx < this.lastCommands.Count)
			{
				this.commandField.text = this.lastCommands[this.lastCommandsIdx];
				this.bUpdateCursor = true;
				if (!TouchScreenKeyboard.isSupported)
				{
					this.commandField.Select();
					this.commandField.ActivateInputField();
					return;
				}
			}
			else
			{
				this.commandField.text = string.Empty;
			}
		}
	}

	public void Clear()
	{
		this.ClearDisplayedLines();
	}

	public override void OnOpen()
	{
		base.OnOpen();
		if (GamePrefs.GetBool(EnumGamePrefs.DebugMenuEnabled) && this.windowManager.IsWindowOpen(XUiC_InGameDebugMenu.ID))
		{
			this.bShouldReopenGebugMenu = true;
			this.windowManager.Close(XUiC_InGameDebugMenu.ID);
		}
		else
		{
			this.bShouldReopenGebugMenu = false;
		}
		this.commandField.text = string.Empty;
		this.bFirstTime = true;
		this.isInputActive = true;
		if (UIInput.selection != null)
		{
			UIInput.selection.isSelected = false;
		}
	}

	public override void OnClose()
	{
		this.scrollRect.verticalNormalizedPosition = 0f;
		base.OnClose();
		if (GamePrefs.GetBool(EnumGamePrefs.DebugMenuEnabled) && this.bShouldReopenGebugMenu)
		{
			this.windowManager.Open(XUiC_InGameDebugMenu.ID, false);
		}
		this.bShouldReopenGebugMenu = false;
		this.isInputActive = false;
	}

	public static GUIWindowConsole GetNewInstance()
	{
		GUIWindowConsole guiwindowConsole;
		if ((guiwindowConsole = GUIWindowConsole.instance) == null)
		{
			guiwindowConsole = (GUIWindowConsole.instance = new GUIWindowConsole());
		}
		return guiwindowConsole;
	}

	public static void Open()
	{
		LocalPlayerUI.primaryUI.windowManager.Open(GUIWindowConsole.instance, false);
	}

	public static void Close()
	{
		LocalPlayerUI.primaryUI.windowManager.Close(GUIWindowConsole.instance, false);
	}

	public static bool IsOpen()
	{
		return LocalPlayerUI.primaryUI.windowManager.IsWindowOpen(GUIWindowConsole.instance);
	}

	public static void AddLines(List<string> _lines)
	{
		GUIWindowConsole guiwindowConsole = GUIWindowConsole.instance;
		if (guiwindowConsole == null)
		{
			return;
		}
		guiwindowConsole.addLines(_lines);
	}

	public static void Shutdown()
	{
		GUIWindowConsole guiwindowConsole = GUIWindowConsole.instance;
		if (guiwindowConsole != null)
		{
			guiwindowConsole.shutdown();
		}
		GUIWindowConsole.instance = null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static GUIWindowConsole instance;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool scrolledToBottom = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public Queue<GUIWindowConsole.ConsoleLine> linesToAdd = new Queue<GUIWindowConsole.ConsoleLine>(301);

	[PublicizedFrom(EAccessModifier.Private)]
	public Queue<Text> displayedLines = new Queue<Text>();

	[PublicizedFrom(EAccessModifier.Private)]
	public const int maxConsoleLines = 300;

	[PublicizedFrom(EAccessModifier.Private)]
	public Stack<Text> linePool = new Stack<Text>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<string> lastCommands = new List<string>();

	[PublicizedFrom(EAccessModifier.Private)]
	public int lastCommandsIdx;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bFirstTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bUpdateCursor;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bShouldReopenGebugMenu;

	[PublicizedFrom(EAccessModifier.Private)]
	public GUIWindowConsoleComponents components;

	[PublicizedFrom(EAccessModifier.Private)]
	public ScrollRect scrollRect;

	[PublicizedFrom(EAccessModifier.Private)]
	public Transform contentRect;

	[PublicizedFrom(EAccessModifier.Private)]
	public InputField commandField;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly string[] lineSeparators = new string[] { "\r\n", "\r", "\n" };

	[PublicizedFrom(EAccessModifier.Private)]
	public struct ConsoleLine
	{
		public ConsoleLine(string _text, string _stackTrace, LogType _type)
		{
			this.text = _text;
			this.stackTrace = _stackTrace;
			this.type = _type;
		}

		public Color GetLogColor()
		{
			switch (this.type)
			{
			case 0:
			case 1:
			case 4:
				return Color.red;
			case 2:
				return Color.yellow;
			case 3:
				return Color.white;
			default:
				return Color.white;
			}
		}

		public string text;

		public LogType type;

		public string stackTrace;
	}
}
