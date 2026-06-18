using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_SaveDirtyPrefab : XUiController
{
	public override void Init()
	{
		base.Init();
		XUiC_SaveDirtyPrefab.ID = base.WindowGroup.Id;
		((XUiC_SimpleButton)base.GetChildById("btnSave")).OnPressed += this.BtnSave_OnPressed;
		((XUiC_SimpleButton)base.GetChildById("btnCancel")).OnPressed += this.BtnCancel_OnPressed;
		((XUiC_SimpleButton)base.GetChildById("btnDontSave")).OnPressed += this.BtnDontSave_OnPressed;
		this.txtSaveName = (XUiC_TextInput)base.GetChildById("txtSaveName");
		this.txtSaveName.OnChangeHandler += this.TxtSaveNameOnOnChangeHandler;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void TxtSaveNameOnOnChangeHandler(XUiController _sender, string _text, bool _changeFromCode)
	{
		if (this.nameRequired)
		{
			this.nameInvalid = _text.Length <= 0 || _text.Contains(" ") || !GameUtils.ValidateGameName(_text);
			this.nameExists = !this.nameInvalid && Prefab.LocationForNewPrefab(_text, null).Exists();
		}
		else
		{
			this.nameInvalid = false;
			this.nameExists = false;
		}
		base.RefreshBindings();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnSave_OnPressed(XUiController _sender, int _mouseButton)
	{
		this.CloseWith(XUiC_SaveDirtyPrefab.ESelectedAction.Save);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnCancel_OnPressed(XUiController _sender, int _mouseButton)
	{
		this.CloseWith(XUiC_SaveDirtyPrefab.ESelectedAction.Cancel);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnDontSave_OnPressed(XUiController _sender, int _mouseButton)
	{
		this.CloseWith(XUiC_SaveDirtyPrefab.ESelectedAction.DontSave);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CloseWith(XUiC_SaveDirtyPrefab.ESelectedAction _action)
	{
		this.xui.playerUI.windowManager.Close(this.windowGroup, false);
		if (_action == XUiC_SaveDirtyPrefab.ESelectedAction.Save)
		{
			if (this.nameRequired)
			{
				string text = this.txtSaveName.Text;
				PrefabEditModeManager.Instance.VoxelPrefab.location = Prefab.LocationForNewPrefab(text, null);
			}
			if (PrefabEditModeManager.Instance.SaveVoxelPrefab())
			{
				GameManager.ShowTooltip(this.xui.playerUI.entityPlayer, string.Format(Localization.Get("xuiPrefabsPrefabSaved", false, null), PrefabEditModeManager.Instance.LoadedPrefab.Name), false, false, 0f);
			}
			else
			{
				GameManager.ShowTooltip(this.xui.playerUI.entityPlayer, Localization.Get("xuiPrefabsPrefabSavingError", false, null), false, false, 0f);
				_action = XUiC_SaveDirtyPrefab.ESelectedAction.Cancel;
			}
		}
		ThreadManager.StartCoroutine(this.delayCallback(_action));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator delayCallback(XUiC_SaveDirtyPrefab.ESelectedAction _action)
	{
		yield return new WaitForSeconds(0.1f);
		Action<XUiC_SaveDirtyPrefab.ESelectedAction> action = this.onCloseAction;
		if (action != null)
		{
			action(_action);
		}
		yield break;
	}

	public override void OnOpen()
	{
		base.OnOpen();
		this.txtSaveName.Text = "";
		this.TxtSaveNameOnOnChangeHandler(this, "", true);
		this.IsDirty = true;
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (this.IsDirty)
		{
			base.RefreshBindings();
			this.IsDirty = false;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string _value, string _bindingName)
	{
		PathAbstractions.AbstractedLocation loadedPrefab = PrefabEditModeManager.Instance.LoadedPrefab;
		if (_bindingName == "is_save_new")
		{
			_value = (loadedPrefab.Type == PathAbstractions.EAbstractedLocationType.None).ToString();
			return true;
		}
		if (_bindingName == "current_prefab_name")
		{
			_value = ((loadedPrefab.Type != PathAbstractions.EAbstractedLocationType.None) ? loadedPrefab.Name : "");
			return true;
		}
		if (_bindingName == "request_name")
		{
			_value = this.nameRequired.ToString();
			return true;
		}
		if (_bindingName == "prefab_name_exists")
		{
			_value = this.nameExists.ToString();
			return true;
		}
		if (!(_bindingName == "prefab_name_invalid"))
		{
			return base.GetBindingValueInternal(ref _value, _bindingName);
		}
		_value = this.nameInvalid.ToString();
		return true;
	}

	public static void Show(XUi _xui, Action<XUiC_SaveDirtyPrefab.ESelectedAction> _onCloseAction, XUiC_SaveDirtyPrefab.EMode _mode = XUiC_SaveDirtyPrefab.EMode.AskSaveIfDirty)
	{
		PathAbstractions.AbstractedLocation loadedPrefab = PrefabEditModeManager.Instance.LoadedPrefab;
		if (_mode != XUiC_SaveDirtyPrefab.EMode.AskSaveIfDirty)
		{
			if (_mode == XUiC_SaveDirtyPrefab.EMode.ForceSave)
			{
				if (loadedPrefab.Type != PathAbstractions.EAbstractedLocationType.None && Prefab.CanSaveIn(loadedPrefab))
				{
					if (PrefabEditModeManager.Instance.SaveVoxelPrefab())
					{
						GameManager.ShowTooltip(_xui.playerUI.entityPlayer, string.Format(Localization.Get("xuiPrefabsPrefabSaved", false, null), loadedPrefab.Name), false, false, 0f);
						if (_onCloseAction != null)
						{
							_onCloseAction(XUiC_SaveDirtyPrefab.ESelectedAction.Save);
							return;
						}
					}
					else
					{
						GameManager.ShowTooltip(_xui.playerUI.entityPlayer, Localization.Get("xuiPrefabsPrefabSavingError", false, null), false, false, 0f);
						if (_onCloseAction != null)
						{
							_onCloseAction(XUiC_SaveDirtyPrefab.ESelectedAction.Cancel);
						}
					}
					return;
				}
			}
		}
		else if (PrefabEditModeManager.Instance.VoxelPrefab == null || !PrefabEditModeManager.Instance.NeedsSaving)
		{
			if (_onCloseAction != null)
			{
				_onCloseAction(XUiC_SaveDirtyPrefab.ESelectedAction.DontSave);
			}
			return;
		}
		XUiC_SaveDirtyPrefab childByType = ((XUiWindowGroup)_xui.playerUI.windowManager.GetWindow(XUiC_SaveDirtyPrefab.ID)).Controller.GetChildByType<XUiC_SaveDirtyPrefab>();
		childByType.nameRequired = loadedPrefab.Type == PathAbstractions.EAbstractedLocationType.None || !Prefab.CanSaveIn(loadedPrefab);
		childByType.onCloseAction = _onCloseAction;
		_xui.playerUI.windowManager.Open(XUiC_SaveDirtyPrefab.ID, true, true);
	}

	public static string ID = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TextInput txtSaveName;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool nameRequired;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool nameInvalid;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool nameExists;

	[PublicizedFrom(EAccessModifier.Private)]
	public Action<XUiC_SaveDirtyPrefab.ESelectedAction> onCloseAction;

	public enum EMode
	{
		AskSaveIfDirty,
		ForceSave
	}

	public enum ESelectedAction
	{
		Save,
		Cancel,
		DontSave
	}
}
