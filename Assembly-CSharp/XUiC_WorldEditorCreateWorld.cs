using System;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_WorldEditorCreateWorld : XUiController
{
	[XuiXmlBinding("minworldsize")]
	public int MinWorldSize
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return 1024;
		}
	}

	[XuiXmlBinding("maxworldsize")]
	public int MaxWorldSize
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return 16384;
		}
	}

	[XuiXmlBinding("iscustomsize")]
	public bool IsCustomSize
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			XUiC_ComboBoxList<int> xuiC_ComboBoxList = this.cmbSize;
			return xuiC_ComboBoxList != null && xuiC_ComboBoxList.Value == -1;
		}
	}

	public int NewWorldSize
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			if (!this.IsCustomSize)
			{
				XUiC_ComboBoxList<int> xuiC_ComboBoxList = this.cmbSize;
				if (xuiC_ComboBoxList == null)
				{
					return -1;
				}
				return xuiC_ComboBoxList.Value;
			}
			else
			{
				int num;
				if (this.txtSize != null && int.TryParse(this.txtSize.Text, out num))
				{
					return num;
				}
				return -1;
			}
		}
	}

	[XuiXmlBinding("customsizevalid")]
	public bool CustomSizeValid
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			int num;
			return this.txtSize != null && int.TryParse(this.txtSize.Text, out num) && num >= this.MinWorldSize && num <= this.MaxWorldSize && num % this.MinWorldSize == 0;
		}
	}

	[XuiXmlBinding("customnamevalid")]
	public bool CustomNameValid
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return this.txtName != null && this.txtName.Text.Trim().Length > 0 && !SdDirectory.Exists(GameIO.GetGameDir("Data/Worlds") + "/" + this.txtName.Text);
		}
	}

	[XuiXmlBinding("startable")]
	public bool Startable
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return this.CustomNameValid && (!this.IsCustomSize || this.CustomSizeValid);
		}
	}

	public override void Init()
	{
		base.Init();
		this.txtSize.UIInputController.OnScroll += delegate(XUiController _sender, float _args)
		{
			this.cmbSize.ScrollEvent(_sender, _args);
		};
	}

	public override void OnOpen()
	{
		base.OnOpen();
		this.txtName.Text = GamePrefs.GetString(EnumGamePrefs.CreateLevelName);
		int num;
		if (int.TryParse(GamePrefs.GetString(EnumGamePrefs.CreateLevelDim), out num))
		{
			if (this.cmbSize.Elements.Contains(num))
			{
				this.cmbSize.Value = num;
			}
			else
			{
				this.cmbSize.SelectedIndex = 0;
				this.txtSize.Text = num.ToString();
			}
		}
		else
		{
			this.txtSize.Text = "";
		}
		this.IsDirty = true;
	}

	public override void OnClose()
	{
		base.OnClose();
		this.owner = null;
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		base.handleDirtyUpdateDefault();
		if (XUiUtils.HotkeysAllowedFor(this.viewComponent ?? this.children[0].ViewComponent, false))
		{
			if (this.xui.playerUI.playerInput.PermanentActions.Cancel.WasReleased)
			{
				this.BtnCancel_OnPressed(this, 0);
			}
			if (this.xui.playerUI.playerInput.GUIActions.Apply.WasReleased)
			{
				this.BtnConfirm_OnPressed(this, 0);
			}
		}
	}

	[XuiBindEvent("OnChangeHandler", "txtName")]
	[XuiBindEvent("OnChangeHandler", "txtSize")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void TxtInput_OnChangeHandler(XUiController _sender, string _text, bool _changeFromCode)
	{
		this.IsDirty = true;
	}

	[XuiBindEvent("OnValueChanged", "cmbSize")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void CmbSize_OnValueChanged(XUiController _sender, int _oldValue, int _newValue)
	{
		if (_newValue < 0 && PlatformManager.NativePlatform.Input.CurrentInputStyle == PlayerInputManager.InputStyle.Keyboard)
		{
			this.txtSize.SetSelected(true, true);
		}
		this.IsDirty = true;
	}

	[XuiBindEvent("OnPress", "btnCancel")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnCancel_OnPressed(XUiController _sender, int _mouseButton)
	{
		this.xui.playerUI.windowManager.Close(this.windowGroup, false);
	}

	[XuiBindEvent("OnPress", "btnConfirm")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnConfirm_OnPressed(XUiController _sender, int _mouseButton)
	{
		if (!this.btnConfirm.ViewComponent.Enabled)
		{
			return;
		}
		new GameModeEditWorld().ResetGamePrefs();
		string text = this.txtName.Text.Trim();
		int newWorldSize = this.NewWorldSize;
		GamePrefs.Set(EnumGamePrefs.GameWorld, text);
		GamePrefs.Set(EnumGamePrefs.CreateLevelName, text);
		GamePrefs.Set(EnumGamePrefs.CreateLevelDim, newWorldSize.ToString());
		MicroStopwatch microStopwatch = new MicroStopwatch(true);
		MicroStopwatch microStopwatch2 = new MicroStopwatch(true);
		WorldStaticData.Cleanup(null);
		Log.Out(string.Format("WSD.Cleanup took {0} ms", microStopwatch2.ElapsedMilliseconds));
		microStopwatch2.ResetAndRestart();
		WorldStaticData.Reset(null);
		Log.Out(string.Format("WSD.Reset took {0} ms", microStopwatch2.ElapsedMilliseconds));
		microStopwatch2.ResetAndRestart();
		PathAbstractions.AbstractedLocation abstractedLocation = XUiC_WorldEditor.LocationForNewWorld(text);
		GameUtils.CreateEmptyFlatLevel(text, abstractedLocation, newWorldSize, 60);
		Log.Out(string.Format("Creating empty world took {0} ms", microStopwatch.ElapsedMilliseconds));
		XUiC_WorldEditor xuiC_WorldEditor = this.owner;
		this.xui.playerUI.windowManager.Close(this.windowGroup, false);
		xuiC_WorldEditor.StartEditor();
	}

	public static void Open(XUiC_WorldEditor _owner)
	{
		XUiC_WorldEditorCreateWorld childByType = _owner.xui.GetChildByType<XUiC_WorldEditorCreateWorld>();
		childByType.owner = _owner;
		_owner.xui.playerUI.windowManager.Open(childByType.windowGroup, false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_WorldEditor owner;

	[XuiBindComponent("btnConfirm", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button btnConfirm;

	[XuiBindComponent("btnCancel", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button btnCancel;

	[XuiBindComponent("txtName", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_TextInput txtName;

	[XuiBindComponent("txtSize", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_TextInput txtSize;

	[XuiBindComponent("cmbSize", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_ComboBoxList<int> cmbSize;
}
