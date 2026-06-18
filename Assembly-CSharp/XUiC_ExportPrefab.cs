using System;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_ExportPrefab : XUiController
{
	public override void Init()
	{
		base.Init();
		XUiC_ExportPrefab.ID = base.WindowGroup.Id;
		this.btnSave = (XUiC_SimpleButton)base.GetChildById("btnSave");
		this.btnSave.OnPressed += this.BtnSave_OnPressed;
		this.btnSaveLocal = (XUiC_SimpleButton)base.GetChildById("btnSaveLocal");
		this.btnSaveLocal.OnPressed += this.BtnSaveLocal_OnPressed;
		((XUiC_SimpleButton)base.GetChildById("btnCancel")).OnPressed += this.BtnCancel_OnPressed;
		this.txtSaveName = (XUiC_TextInput)base.GetChildById("txtSaveName");
		this.txtSaveName.OnChangeHandler += this.TxtSaveNameOnOnChangeHandler;
		this.lblPrefabExists = base.GetChildById("lblPrefabExists").ViewComponent as XUiV_Label;
		this.lblInvalidName = base.GetChildById("lblInvalidName").ViewComponent as XUiV_Label;
		this.toggleAsPart = base.GetChildByType<XUiC_ToggleButton>();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void TxtSaveNameOnOnChangeHandler(XUiController _sender, string _text, bool _changeFromCode)
	{
		bool flag = _text.Length > 0 && !_text.Contains(" ") && GameUtils.ValidateGameName(_text);
		bool flag2 = flag && false;
		this.lblPrefabExists.IsVisible = flag2;
		this.lblInvalidName.IsVisible = !flag;
		this.btnSave.Enabled = flag && SingletonMonoBehaviour<ConnectionManager>.Instance.IsClient && !flag2;
		this.btnSaveLocal.Enabled = flag && !flag2;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnSave_OnPressed(XUiController _sender, int _mouseButton)
	{
		this.SaveAndClose(false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnSaveLocal_OnPressed(XUiController _sender, int _mouseButton)
	{
		this.SaveAndClose(true);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnCancel_OnPressed(XUiController _sender, int _mouseButton)
	{
		this.xui.playerUI.windowManager.Close(this.windowGroup, false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SaveAndClose(bool _local)
	{
		this.xui.playerUI.windowManager.Close(this.windowGroup, false);
		string text = ConsoleCmdExportPrefab.BuildCommandString(this.txtSaveName.Text, BlockToolSelection.Instance.SelectionStart, BlockToolSelection.Instance.SelectionEnd, this.toggleAsPart.Value);
		if (_local || SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			GUIWindowConsole.AddLines(SingletonMonoBehaviour<SdtdConsole>.Instance.ExecuteSync(text, null));
		}
		else
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageConsoleCmdServer>().Setup(text), false);
		}
		GUIWindowConsole.Open();
	}

	public override void OnOpen()
	{
		base.OnOpen();
		this.txtSaveName.Text = "";
		this.TxtSaveNameOnOnChangeHandler(this, "", true);
		this.IsDirty = true;
	}

	public static void Open(XUi _xui)
	{
		_xui.playerUI.windowManager.Open(XUiC_ExportPrefab.ID, true);
	}

	public static string ID = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TextInput txtSaveName;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnSave;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnSaveLocal;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label lblPrefabExists;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label lblInvalidName;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ToggleButton toggleAsPart;
}
