using System;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_WorldSelectionPopup : XUiController
{
	[XuiXmlBinding("msgTitle")]
	public string MsgTitle
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get;
		[PublicizedFrom(EAccessModifier.Private)]
		set;
	} = string.Empty;

	[XuiBindEvent("OnPress", "btnCancel")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void OnCancelButtonPressed(XUiController _sender, int _mouseButton)
	{
		this.cancel();
	}

	[XuiBindEvent("OnPress", "btnConfirm")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void OnConfirmButtonPressed(XUiController _sender, int _mouseButton)
	{
		this.confirm();
	}

	[XuiXmlBinding("isWorldSelected")]
	public bool IsWorldSelected
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			XUiC_WorldSelectionList xuiC_WorldSelectionList = this.worldList;
			return xuiC_WorldSelectionList != null && xuiC_WorldSelectionList.HasSelection;
		}
	}

	[XuiBindEvent("SelectionChanged", "worldList")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void WorldList_SelectionChanged(XUiC_List<XUiC_WorldSelectionList.Entry> _list, XUiC_WorldSelectionList.Entry _previousEntry, XUiC_WorldSelectionList.Entry _newEntry)
	{
		this.IsDirty = true;
	}

	public override void Init()
	{
		base.Init();
		XUiC_WorldSelectionPopup.id = this.windowGroup.Id;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void open(string _titleCaptionKey, string _worldName, Action _onCancel, Action<PathAbstractions.AbstractedLocation> _onConfirm)
	{
		this.MsgTitle = _titleCaptionKey;
		this.onCancel = _onCancel;
		this.onConfirm = _onConfirm;
		this.worldList.SetFilter(_worldName);
		if (this.worldList.EntryCount == 1)
		{
			if (_onConfirm != null)
			{
				_onConfirm(this.worldList.GetEntry(0).Location);
			}
			return;
		}
		this.xui.playerUI.windowManager.Open(this.windowGroup, false);
	}

	public static void Open(XUi _xui, string _titleCaptionKey, string _worldName, Action _onCancel, Action<PathAbstractions.AbstractedLocation> _onConfirm)
	{
		((XUiC_WorldSelectionPopup)_xui.FindWindowGroupByName(XUiC_WorldSelectionPopup.id)).open(_titleCaptionKey, _worldName, _onCancel, _onConfirm);
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (XUiUtils.HotkeysAllowedFor(this.viewComponent ?? this.children[0].ViewComponent, false))
		{
			if (this.xui.playerUI.playerInput.GUIActions.Cancel.WasReleased)
			{
				this.cancel();
				return;
			}
			if (this.xui.playerUI.playerInput.GUIActions.Apply.WasPressed)
			{
				this.confirm();
				return;
			}
		}
		if (this.IsDirty)
		{
			base.RefreshBindings();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void cancel()
	{
		Action action = this.onCancel;
		if (action != null)
		{
			action();
		}
		this.xui.playerUI.windowManager.Close(this.windowGroup, false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void confirm()
	{
		if (!this.worldList.HasSelection)
		{
			return;
		}
		Action<PathAbstractions.AbstractedLocation> action = this.onConfirm;
		if (action != null)
		{
			action(this.worldList.SelectedEntryData.Location);
		}
		this.xui.playerUI.windowManager.Close(this.windowGroup, false);
	}

	public override void OnClose()
	{
		base.OnClose();
		this.onCancel = null;
		this.onConfirm = null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static string id;

	[XuiBindComponent("btnCancel", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button btnCancel;

	[PublicizedFrom(EAccessModifier.Private)]
	public Action onCancel;

	[XuiBindComponent("btnConfirm", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button btnConfirm;

	[PublicizedFrom(EAccessModifier.Private)]
	public Action<PathAbstractions.AbstractedLocation> onConfirm;

	[XuiBindComponent(true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_WorldSelectionList worldList;
}
