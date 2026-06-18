using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using InControl;
using UnityEngine;

public class ControllerDebugMacros : MonoBehaviour
{
	public ControllerDebugMacros()
	{
		this.m_debugMacros = new SortedDictionary<string, Action<ControllerDebugMacros.DebugDirection>>();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnEnable()
	{
		this.m_debugLabel = base.GetComponent<ControllerDebugLabel>();
		this.m_debugLabel.AddDebugProvider("Debug Macros", new Action<StringBuilder>(this.BuildDebugMacroStatus));
		this.AddDebugMacro("Do Nothing", delegate
		{
		});
		this.AddDebugMacro("Open Console", new Action(this.MacroOpenConsole));
		this.AddDebugMacro("Toggle God Mode", new Action(this.MacroToggleGodMode));
		this.AddDebugMacro("Toggle Flying", new Action(this.MacroToggleFlying));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnDisable()
	{
		this.m_debugLabel.RemoveDebugProvider("Debug Macros");
		this.RemoveDebugMacro("Open Console");
		this.RemoveDebugMacro("Toggle God Mode");
		this.RemoveDebugMacro("Toggle Flying");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Update()
	{
		if (this.m_currentMacro == null || !this.m_debugMacros.ContainsKey(this.m_currentMacro))
		{
			if (this.m_lastIndex >= 0 && this.m_lastIndex < this.m_debugMacros.Keys.Count)
			{
				this.m_currentMacro = this.m_debugMacros.Keys.Skip(this.m_lastIndex).FirstOrDefault<string>();
			}
			else
			{
				this.m_currentMacro = this.m_debugMacros.Keys.LastOrDefault<string>();
				this.m_lastIndex = this.m_debugMacros.Keys.Count - 1;
			}
		}
		if (this.m_currentMacro == null)
		{
			return;
		}
		ControllerDebugMacros.DebugDirection? executeMacroKeybind = this.GetExecuteMacroKeybind();
		if (executeMacroKeybind != null)
		{
			ControllerDebugMacros.DebugDirection valueOrDefault = executeMacroKeybind.GetValueOrDefault();
			if (this.m_keybindPressed)
			{
				return;
			}
			this.m_keybindPressed = true;
			if (this.m_debugMacros.ContainsKey(this.m_currentMacro))
			{
				this.m_debugMacros[this.m_currentMacro](valueOrDefault);
				return;
			}
		}
		else
		{
			if (this.HasNextMacroKeybind())
			{
				if (this.m_keybindPressed)
				{
					return;
				}
				this.m_keybindPressed = true;
				bool flag = false;
				int num = 0;
				using (SortedDictionary<string, Action<ControllerDebugMacros.DebugDirection>>.KeyCollection.Enumerator enumerator = this.m_debugMacros.Keys.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						string text = enumerator.Current;
						if (flag)
						{
							this.m_currentMacro = text;
							this.m_lastIndex = num;
							break;
						}
						if (text == this.m_currentMacro)
						{
							flag = true;
						}
						num++;
					}
					return;
				}
			}
			if (this.HasPreviousMacroKeybind())
			{
				if (this.m_keybindPressed)
				{
					return;
				}
				this.m_keybindPressed = true;
				string text2 = null;
				int num2 = 0;
				using (SortedDictionary<string, Action<ControllerDebugMacros.DebugDirection>>.KeyCollection.Enumerator enumerator = this.m_debugMacros.Keys.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						string text3 = enumerator.Current;
						if (text3 == this.m_currentMacro)
						{
							if (text2 != null)
							{
								this.m_currentMacro = text2;
								this.m_lastIndex = num2 - 1;
								break;
							}
							break;
						}
						else
						{
							text2 = text3;
							num2++;
						}
					}
					return;
				}
			}
			this.m_keybindPressed = false;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BuildDebugMacroStatus(StringBuilder builder)
	{
		foreach (string text in this.m_debugMacros.Keys)
		{
			if (builder.Length > 0)
			{
				builder.Append(' ');
			}
			bool flag = text == this.m_currentMacro;
			if (flag)
			{
				builder.Append('[');
			}
			builder.Append(text);
			if (flag)
			{
				builder.Append(']');
			}
		}
	}

	public void AddDebugMacro(string macroName, Action macro)
	{
		this.m_debugMacros[macroName] = delegate(ControllerDebugMacros.DebugDirection _)
		{
			macro();
		};
	}

	public void AddDebugMacro(string macroName, Action<ControllerDebugMacros.DebugDirection> macro)
	{
		this.m_debugMacros[macroName] = macro;
	}

	public void RemoveDebugMacro(string macroName)
	{
		this.m_debugMacros.Remove(macroName);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool HasPreviousMacroKeybind()
	{
		return ControllerDebugMacros.<HasPreviousMacroKeybind>g__HasKeyboardKeybind|14_0() || ControllerDebugMacros.<HasPreviousMacroKeybind>g__HasControllerKeybind|14_1();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool HasNextMacroKeybind()
	{
		return ControllerDebugMacros.<HasNextMacroKeybind>g__HasKeyboardKeybind|15_0() || ControllerDebugMacros.<HasNextMacroKeybind>g__HasControllerKeybind|15_1();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public ControllerDebugMacros.DebugDirection? GetExecuteMacroKeybind()
	{
		ControllerDebugMacros.DebugDirection? debugDirection = ControllerDebugMacros.<GetExecuteMacroKeybind>g__GetKeyboardKeybind|16_0();
		if (debugDirection == null)
		{
			return ControllerDebugMacros.<GetExecuteMacroKeybind>g__GetControllerKeybind|16_1();
		}
		return debugDirection;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void MacroOpenConsole()
	{
		Object @object = Object.FindAnyObjectByType<GUIWindowManager>();
		GameManager instance = GameManager.Instance;
		if (!@object || !instance)
		{
			return;
		}
		GUIWindowConsole.Open();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static EntityPlayerLocal GetPrimaryPlayer()
	{
		World world = GameManager.Instance.World;
		if (world == null)
		{
			return null;
		}
		return world.GetPrimaryPlayer();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void MacroToggleGodMode()
	{
		EntityPlayerLocal primaryPlayer = ControllerDebugMacros.GetPrimaryPlayer();
		if (!primaryPlayer)
		{
			return;
		}
		DataItem<bool> isGodMode = primaryPlayer.IsGodMode;
		if (isGodMode.Value)
		{
			isGodMode.Value = false;
			primaryPlayer.Buffs.RemoveBuff("god", -1, true);
		}
		else
		{
			isGodMode.Value = true;
			primaryPlayer.Buffs.AddBuff("god", -1, true, false, -1f);
		}
		primaryPlayer.bEntityAliveFlagsChanged = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void MacroToggleFlying()
	{
		EntityPlayerLocal primaryPlayer = ControllerDebugMacros.GetPrimaryPlayer();
		if (!primaryPlayer)
		{
			return;
		}
		DataItem<bool> isFlyMode = primaryPlayer.IsFlyMode;
		isFlyMode.Value = !isFlyMode.Value;
		primaryPlayer.bEntityAliveFlagsChanged = true;
	}

	[CompilerGenerated]
	[PublicizedFrom(EAccessModifier.Internal)]
	public static bool <HasPreviousMacroKeybind>g__HasKeyboardKeybind|14_0()
	{
		return InputUtils.ControlKeyPressed && InputUtils.ShiftKeyPressed && InputUtils.AltKeyPressed && Input.GetKey(276);
	}

	[CompilerGenerated]
	[PublicizedFrom(EAccessModifier.Internal)]
	public static bool <HasPreviousMacroKeybind>g__HasControllerKeybind|14_1()
	{
		return InputManager.Enabled && InputManager.ActiveDevice.LeftBumper.IsPressed && InputManager.ActiveDevice.RightBumper.IsPressed && InputManager.ActiveDevice.DPadLeft.IsPressed;
	}

	[CompilerGenerated]
	[PublicizedFrom(EAccessModifier.Internal)]
	public static bool <HasNextMacroKeybind>g__HasKeyboardKeybind|15_0()
	{
		return InputUtils.ControlKeyPressed && InputUtils.ShiftKeyPressed && InputUtils.AltKeyPressed && Input.GetKey(275);
	}

	[CompilerGenerated]
	[PublicizedFrom(EAccessModifier.Internal)]
	public static bool <HasNextMacroKeybind>g__HasControllerKeybind|15_1()
	{
		return InputManager.Enabled && InputManager.ActiveDevice.LeftBumper.IsPressed && InputManager.ActiveDevice.RightBumper.IsPressed && InputManager.ActiveDevice.DPadRight.IsPressed;
	}

	[CompilerGenerated]
	[PublicizedFrom(EAccessModifier.Internal)]
	public static ControllerDebugMacros.DebugDirection? <GetExecuteMacroKeybind>g__GetKeyboardKeybind|16_0()
	{
		if (!InputUtils.ControlKeyPressed)
		{
			return null;
		}
		if (!InputUtils.ShiftKeyPressed)
		{
			return null;
		}
		if (!InputUtils.AltKeyPressed)
		{
			return null;
		}
		if (Input.GetKey(319))
		{
			return new ControllerDebugMacros.DebugDirection?(ControllerDebugMacros.DebugDirection.Neutral);
		}
		if (Input.GetKey(274))
		{
			return new ControllerDebugMacros.DebugDirection?(ControllerDebugMacros.DebugDirection.Down);
		}
		if (Input.GetKey(273))
		{
			return new ControllerDebugMacros.DebugDirection?(ControllerDebugMacros.DebugDirection.Up);
		}
		return null;
	}

	[CompilerGenerated]
	[PublicizedFrom(EAccessModifier.Internal)]
	public static ControllerDebugMacros.DebugDirection? <GetExecuteMacroKeybind>g__GetControllerKeybind|16_1()
	{
		if (!InputManager.Enabled)
		{
			return null;
		}
		if (!InputManager.ActiveDevice.LeftBumper.IsPressed)
		{
			return null;
		}
		if (!InputManager.ActiveDevice.RightBumper.IsPressed)
		{
			return null;
		}
		if (InputManager.ActiveDevice.Action1.IsPressed)
		{
			return new ControllerDebugMacros.DebugDirection?(ControllerDebugMacros.DebugDirection.Neutral);
		}
		if (InputManager.ActiveDevice.DPadDown.IsPressed)
		{
			return new ControllerDebugMacros.DebugDirection?(ControllerDebugMacros.DebugDirection.Down);
		}
		if (InputManager.ActiveDevice.DPadUp.IsPressed)
		{
			return new ControllerDebugMacros.DebugDirection?(ControllerDebugMacros.DebugDirection.Up);
		}
		return null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public readonly SortedDictionary<string, Action<ControllerDebugMacros.DebugDirection>> m_debugMacros;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public string m_currentMacro;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int m_lastIndex;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool m_keybindPressed;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public ControllerDebugLabel m_debugLabel;

	public enum DebugDirection
	{
		Up,
		Down,
		Neutral
	}
}
