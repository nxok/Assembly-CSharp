using System;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_SignWindow : XUiController
{
	public override void Init()
	{
		base.Init();
		XUiView xuiView = (XUiV_Button)base.GetChildById("clickable").ViewComponent;
		this.textInput1 = base.GetChildById("input1") as XUiC_TextInput;
		this.textInput1.OnSubmitHandler += this.TextInput_OnSubmitHandler;
		this.textInput1.SupportBbCode = false;
		this.textInput2 = base.GetChildById("input2") as XUiC_TextInput;
		if (this.textInput2 != null)
		{
			this.textInput2.OnSubmitHandler += this.TextInput_OnSubmitHandler;
			this.textInput2.SupportBbCode = false;
			this.textInput3 = base.GetChildById("input3") as XUiC_TextInput;
			this.textInput3.OnSubmitHandler += this.TextInput_OnSubmitHandler;
			this.textInput3.SupportBbCode = false;
			this.textInput1.SelectOnTab = this.textInput2;
			this.textInput2.SelectOnTab = this.textInput3;
			this.textInput3.SelectOnTab = this.textInput1;
			this.separateLineMode = true;
		}
		xuiView.Controller.OnPress += this.closeButton_OnPress;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void TextInput_OnSubmitHandler(XUiController _sender, string _text)
	{
		if (this.separateLineMode)
		{
			XUiC_TextInput xuiC_TextInput = _sender as XUiC_TextInput;
			if (xuiC_TextInput != null)
			{
				xuiC_TextInput.SelectOnTab.SelectCursorElement(false, false);
				xuiC_TextInput.SelectOnTab.SetSelected(true, false);
				return;
			}
		}
		else
		{
			this.xui.playerUI.windowManager.Close(base.WindowGroup, false);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void closeButton_OnPress(XUiController _sender, int _mouseButton)
	{
		this.xui.playerUI.windowManager.Close(base.WindowGroup, false);
	}

	public void SetTileEntitySign(ITileEntitySignable _te)
	{
		this.SignTileEntity = _te;
	}

	public override void OnOpen()
	{
		base.OnOpen();
		string displayTextImmediately = GeneratedTextManager.GetDisplayTextImmediately(this.SignTileEntity.GetAuthoredText(), true, GeneratedTextManager.TextFilteringMode.Filter, GeneratedTextManager.BbCodeSupportMode.NotSupported);
		if (this.separateLineMode)
		{
			this.textInput1.Text = (this.textInput2.Text = (this.textInput3.Text = ""));
			string[] array = displayTextImmediately.Split('\n', StringSplitOptions.None);
			if (array.Length != 0)
			{
				this.textInput1.Text = array[0];
				if (array.Length > 1)
				{
					this.textInput2.Text = array[1];
					if (array.Length > 2)
					{
						this.textInput3.Text = array[2];
					}
				}
			}
		}
		else
		{
			this.textInput1.Text = displayTextImmediately;
		}
		this.xui.playerUI.entityPlayer.PlayOneShot("open_sign", false, false, false, null, 1f);
		this.xui.playerUI.CursorController.SetNavigationLockView(base.ViewComponent, null);
	}

	public override void OnClose()
	{
		base.OnClose();
		this.xui.playerUI.entityPlayer.PlayOneShot("close_sign", false, false, false, null, 1f);
		this.xui.playerUI.CursorController.SetNavigationLockView(null, null);
		if (GameManager.Instance.World.GetTileEntity(this.SignTileEntity.ToWorldPos()).GetSelfOrFeature<ITileEntitySignable>() != this.SignTileEntity)
		{
			this.FinishClosing();
			return;
		}
		string text = (this.separateLineMode ? string.Format("{0}\n{1}\n{2}", this.textInput1.Text, this.textInput2.Text, this.textInput3.Text) : this.textInput1.Text);
		if (!this.SignTileEntity.CanRenderString(text))
		{
			GameManager.ShowTooltip(this.xui.playerUI.entityPlayer, "uiInvalidCharacters", false, false, 0f);
			this.FinishClosing();
			return;
		}
		PersistentPlayerData playerDataFromEntityID = GameManager.Instance.persistentPlayers.GetPlayerDataFromEntityID(this.xui.playerUI.entityPlayer.entityId);
		this.SignTileEntity.SetText(text, true, (playerDataFromEntityID != null) ? playerDataFromEntityID.PrimaryId : null);
		GeneratedTextManager.GetDisplayText(this.SignTileEntity.GetAuthoredText(), delegate(string _)
		{
			this.FinishClosing();
		}, true, false, GeneratedTextManager.TextFilteringMode.FilterWithSafeString, GeneratedTextManager.BbCodeSupportMode.NotSupported);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void FinishClosing()
	{
		this.SignTileEntity.SetUserAccessing(false);
		LockManager.Instance.UnlockRequestLocal();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public ITileEntitySignable SignTileEntity;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool separateLineMode;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TextInput textInput1;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TextInput textInput2;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TextInput textInput3;
}
