using System;
using System.Collections.Generic;
using GUI_2;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_GamepadCalloutWindow : XUiController
{
	public override void Init()
	{
		base.Init();
		this.xui.CalloutWindow = this;
		XUiC_GamepadCalloutWindow.Callout.calloutFont = this.xui.GetUIFontByName("ReferenceFont", true);
		for (int i = 0; i < 17; i++)
		{
			XUiC_GamepadCalloutWindow.CalloutType calloutType = (XUiC_GamepadCalloutWindow.CalloutType)i;
			this.calloutGroups.Add(calloutType, new List<XUiC_GamepadCalloutWindow.Callout>());
			this.typeVisible[i] = new XUiC_GamepadCalloutWindow.VisibilityData();
		}
		this.InitWorldCallouts();
		this.InitContextMenuCallouts();
		this.InitRWGCallouts();
		this.InitCharacterPreviewCallouts();
		this.InitCameraZoomCallouts();
		this.controllerStyle = PlatformManager.NativePlatform.Input.CurrentControllerInputStyle;
		base.registerForInputStyleChanges();
	}

	public override void Cleanup()
	{
		base.Cleanup();
		this.xui.CalloutWindow = null;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void inputStyleChanged(PlayerInputManager.InputStyle _oldStyle, PlayerInputManager.InputStyle _newStyle)
	{
		base.inputStyleChanged(_oldStyle, _newStyle);
		for (int i = 0; i < this.callouts.Count; i++)
		{
			this.callouts[i].RefreshIcon();
		}
		this.HideCallouts(base.CurrentInputStyle == PlayerInputManager.InputStyle.Keyboard);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void InitWorldCallouts()
	{
		this.AddCallout(UIUtils.ButtonIcon.FaceButtonWest, "igcoInventory", XUiC_GamepadCalloutWindow.CalloutType.World);
		this.AddCallout(UIUtils.ButtonIcon.FaceButtonWest, "igcoRadialMenu", XUiC_GamepadCalloutWindow.CalloutType.World);
		this.AddCallout(UIUtils.ButtonIcon.FaceButtonSouth, "igcoJump", XUiC_GamepadCalloutWindow.CalloutType.World);
		this.AddCallout(UIUtils.ButtonIcon.DPadUp, "igcoQuickSlot1", XUiC_GamepadCalloutWindow.CalloutType.World);
		this.AddCallout(UIUtils.ButtonIcon.DPadRight, "igcoQuickSlot2", XUiC_GamepadCalloutWindow.CalloutType.World);
		this.AddCallout(UIUtils.ButtonIcon.DPadDown, "igcoQuickSlot3", XUiC_GamepadCalloutWindow.CalloutType.World);
		this.AddCallout(UIUtils.ButtonIcon.DPadLeft, "igcoToggleLight", XUiC_GamepadCalloutWindow.CalloutType.World);
		this.AddCallout(UIUtils.ButtonIcon.FaceButtonNorth, "igcoActivate", XUiC_GamepadCalloutWindow.CalloutType.World);
		this.AddCallout(UIUtils.ButtonIcon.FaceButtonEast, "igcoReload", XUiC_GamepadCalloutWindow.CalloutType.World);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void InitContextMenuCallouts()
	{
		this.AddCallout(UIUtils.ButtonIcon.RightStickLeftRight, "igcoPageSelection", XUiC_GamepadCalloutWindow.CalloutType.MenuComboBox);
		this.AddCallout(UIUtils.ButtonIcon.RightStickLeftRight, "igcoPaging", XUiC_GamepadCalloutWindow.CalloutType.MenuPaging);
		this.xui.CalloutWindow.AddCallout(UIUtils.ButtonIcon.LeftTrigger, "igcoCategoryLeft", XUiC_GamepadCalloutWindow.CalloutType.MenuCategory);
		this.xui.CalloutWindow.AddCallout(UIUtils.ButtonIcon.RightTrigger, "igcoCategoryRight", XUiC_GamepadCalloutWindow.CalloutType.MenuCategory);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void InitRWGCallouts()
	{
		this.xui.CalloutWindow.AddCallout(UIUtils.ButtonIcon.RightTrigger, "igco_rwgCameraMode", XUiC_GamepadCalloutWindow.CalloutType.RWGEditor);
		this.xui.CalloutWindow.AddCallout(UIUtils.ButtonIcon.LeftStick, "igco_moveCamera", XUiC_GamepadCalloutWindow.CalloutType.RWGCamera);
		this.xui.CalloutWindow.AddCallout(UIUtils.ButtonIcon.RightStick, "igco_pivotCamera", XUiC_GamepadCalloutWindow.CalloutType.RWGCamera);
		this.xui.CalloutWindow.AddCallout(UIUtils.ButtonIcon.LeftTrigger, "igco_rwgCameraSpeed", XUiC_GamepadCalloutWindow.CalloutType.RWGCamera);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void InitCharacterPreviewCallouts()
	{
		this.xui.CalloutWindow.AddCallout(UIUtils.ButtonIcon.RightStickLeftRight, "igco_rwgRotateCharacterPreview", XUiC_GamepadCalloutWindow.CalloutType.CharacterPreview);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void InitCameraZoomCallouts()
	{
		this.xui.CalloutWindow.AddCallout(UIUtils.ButtonIcon.RightTrigger, "igcoZoomInCamera", XUiC_GamepadCalloutWindow.CalloutType.CameraZoom);
		this.xui.CalloutWindow.AddCallout(UIUtils.ButtonIcon.LeftTrigger, "igcoZoomOutCamera", XUiC_GamepadCalloutWindow.CalloutType.CameraZoom);
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		bool flag = this.xui.playerUI != null && this.xui.playerUI.playerInput != null && this.xui.playerUI.playerInput.Enabled;
		if (this.localActionsEnabled != flag)
		{
			this.localActionsEnabled = flag;
			if (flag)
			{
				this.DisableCallouts(XUiC_GamepadCalloutWindow.CalloutType.MenuHoverItem);
			}
		}
		this.UpdateVisibility(_dt);
		if (this.stackObject != null && !this.stackObject.activeInHierarchy)
		{
			this.ClearCallouts(XUiC_GamepadCalloutWindow.CalloutType.MenuHoverItem);
			this.stackObject = null;
		}
		if (this.IsDirty)
		{
			this.ResetFreeCallouts();
			this.ShowCallouts();
			this.IsDirty = false;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ResetFreeCallouts()
	{
		for (int i = 0; i < this.callouts.Count; i++)
		{
			XUiC_GamepadCalloutWindow.Callout callout = this.callouts[i];
			if (callout != null && callout.isFree)
			{
				callout.gameObject.SetActive(false);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ShowCallouts()
	{
		int num = 0;
		for (int i = 0; i < 17; i++)
		{
			XUiC_GamepadCalloutWindow.VisibilityData visibilityData = this.typeVisible[i];
			if (!this.hideCallouts)
			{
				this.ShowCallouts((XUiC_GamepadCalloutWindow.CalloutType)i, visibilityData.isVisible, ref num);
			}
			else
			{
				this.ShowCallouts((XUiC_GamepadCalloutWindow.CalloutType)i, false, ref num);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ShowCallouts(XUiC_GamepadCalloutWindow.CalloutType _type, bool _visible, ref int _currentOffset)
	{
		List<XUiC_GamepadCalloutWindow.Callout> list = this.calloutGroups[_type];
		if (list != null)
		{
			for (int i = 0; i < list.Count; i++)
			{
				XUiC_GamepadCalloutWindow.Callout callout = list[i];
				bool flag = _visible && callout.bIsVisible;
				if (flag)
				{
					callout.transform.localPosition = new Vector2(0f, (float)_currentOffset);
					_currentOffset -= 5 + callout.iconSprite.height;
				}
				if (callout != null)
				{
					callout.gameObject.SetActive(flag);
				}
			}
		}
	}

	public void AddCallout(UIUtils.ButtonIcon _button, string _action, XUiC_GamepadCalloutWindow.CalloutType _type)
	{
		if (this.ContainsCallout(_button, _action))
		{
			return;
		}
		XUiC_GamepadCalloutWindow.Callout callout = this.GetCallout(_type);
		callout.SetupCallout(_button, _action);
		List<XUiC_GamepadCalloutWindow.Callout> list = this.calloutGroups[_type];
		if (list != null)
		{
			list.Add(callout);
		}
		this.IsDirty = true;
	}

	public void RemoveCallout(UIUtils.ButtonIcon _button, string _action, XUiC_GamepadCalloutWindow.CalloutType _type)
	{
		XUiC_GamepadCalloutWindow.Callout callout = null;
		foreach (XUiC_GamepadCalloutWindow.Callout callout2 in this.callouts)
		{
			if (callout2 != null && callout2.icon == _button && callout2.action == _action && callout2.type == _type)
			{
				callout = callout2;
				break;
			}
		}
		if (callout != null)
		{
			callout.FreeCallout();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ShowCallout(UIUtils.ButtonIcon _button, XUiC_GamepadCalloutWindow.CalloutType _type, bool _visible)
	{
		List<XUiC_GamepadCalloutWindow.Callout> list = this.calloutGroups[_type];
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i] != null && list[i].iconSprite.spriteName == UIUtils.GetSpriteName(_button))
			{
				list[i].bIsVisible = _visible;
				break;
			}
		}
		this.IsDirty = true;
	}

	public bool ContainsCallout(UIUtils.ButtonIcon _button, string _action)
	{
		foreach (XUiC_GamepadCalloutWindow.Callout callout in this.callouts)
		{
			if (callout != null && callout.icon == _button && callout.action == _action)
			{
				return true;
			}
		}
		return false;
	}

	public void ClearCallouts(XUiC_GamepadCalloutWindow.CalloutType _type)
	{
		List<XUiC_GamepadCalloutWindow.Callout> list;
		if (this.calloutGroups.TryGetValue(_type, out list) && list.Count > 0)
		{
			for (int i = 0; i < this.callouts.Count; i++)
			{
				XUiC_GamepadCalloutWindow.Callout callout = this.callouts[i];
				if (callout.type == _type)
				{
					callout.FreeCallout();
				}
			}
			list.Clear();
			this.IsDirty = true;
		}
	}

	public void SetCalloutsEnabled(XUiC_GamepadCalloutWindow.CalloutType _type, bool _enabled)
	{
		XUiC_GamepadCalloutWindow.VisibilityData visibilityData = this.typeVisible[(int)_type];
		if (visibilityData != null && visibilityData.isVisible != _enabled)
		{
			visibilityData.isVisible = _enabled;
			this.IsDirty = true;
		}
		if (_enabled && _type != XUiC_GamepadCalloutWindow.CalloutType.World)
		{
			this.DisableCallouts(XUiC_GamepadCalloutWindow.CalloutType.World);
		}
	}

	public void EnableCallouts(XUiC_GamepadCalloutWindow.CalloutType _type, float _duration = 0f)
	{
		this.SetCalloutsEnabled(_type, true);
		XUiC_GamepadCalloutWindow.VisibilityData visibilityData = this.typeVisible[(int)_type];
		visibilityData.activeDuration = 0f;
		visibilityData.duration = _duration;
	}

	public void DisableCallouts(XUiC_GamepadCalloutWindow.CalloutType _type)
	{
		this.SetCalloutsEnabled(_type, false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateVisibility(float _dt)
	{
		for (int i = 0; i < 17; i++)
		{
			XUiC_GamepadCalloutWindow.VisibilityData visibilityData = this.typeVisible[i];
			if (visibilityData.isVisible && visibilityData.duration != 0f)
			{
				visibilityData.activeDuration += Time.unscaledDeltaTime;
				if (visibilityData.activeDuration > visibilityData.duration)
				{
					this.DisableCallouts((XUiC_GamepadCalloutWindow.CalloutType)i);
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_GamepadCalloutWindow.Callout GetCallout(XUiC_GamepadCalloutWindow.CalloutType _type)
	{
		XUiC_GamepadCalloutWindow.Callout callout = null;
		bool flag = false;
		for (int i = 0; i < this.callouts.Count; i++)
		{
			XUiC_GamepadCalloutWindow.Callout callout2 = this.callouts[i];
			if (callout2 != null && callout2.isFree)
			{
				callout = callout2;
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			callout = NGUITools.AddChild<XUiC_GamepadCalloutWindow.Callout>(this.viewComponent.UiTransform.gameObject);
			if (callout.iconSprite == null)
			{
				callout.iconSprite = NGUITools.AddChild<UISprite>(callout.gameObject);
			}
			this.callouts.Add(callout);
			callout.SetAtlas(UIUtils.IconAtlas);
		}
		callout.type = _type;
		callout.isFree = false;
		callout.bIsVisible = true;
		return callout;
	}

	public void UpdateCalloutsForItemStack(GameObject _stackObject, ItemStack _itemStack, bool _isHovered, bool _canSwap = true, bool _canRemove = true, bool _canPlaceOne = true)
	{
		XUiC_DragAndDropWindow dragAndDropWindow = this.xui.DragAndDropWindow;
		this.ClearCallouts(XUiC_GamepadCalloutWindow.CalloutType.MenuHoverItem);
		if (_isHovered)
		{
			this.stackObject = _stackObject;
			if (_itemStack.itemValue.ItemClass != null)
			{
				if (dragAndDropWindow.CurrentStack.IsEmpty())
				{
					if (_canRemove)
					{
						if (_itemStack.itemValue.ItemClass.CanStack() && _itemStack.count > 1)
						{
							this.AddCallout(UIUtils.ButtonIcon.FaceButtonSouth, "igcoTakeAll", XUiC_GamepadCalloutWindow.CalloutType.MenuHoverItem);
							this.AddCallout(UIUtils.ButtonIcon.FaceButtonWest, "igcoTakeHalf", XUiC_GamepadCalloutWindow.CalloutType.MenuHoverItem);
							this.ShowCallout(UIUtils.ButtonIcon.FaceButtonSouth, XUiC_GamepadCalloutWindow.CalloutType.Menu, false);
						}
						else
						{
							this.AddCallout(UIUtils.ButtonIcon.FaceButtonSouth, "igcoTake", XUiC_GamepadCalloutWindow.CalloutType.MenuHoverItem);
							this.ShowCallout(UIUtils.ButtonIcon.FaceButtonSouth, XUiC_GamepadCalloutWindow.CalloutType.Menu, false);
						}
						this.AddCallout(UIUtils.ButtonIcon.RightStick, "igcoQuickMove", XUiC_GamepadCalloutWindow.CalloutType.MenuHoverItem);
					}
				}
				else if (_itemStack.CanStackWith(dragAndDropWindow.CurrentStack, false))
				{
					this.AddCallout(UIUtils.ButtonIcon.FaceButtonSouth, "igcoPlaceAll", XUiC_GamepadCalloutWindow.CalloutType.MenuHoverItem);
					if (_canPlaceOne)
					{
						this.AddCallout(UIUtils.ButtonIcon.FaceButtonWest, "igcoPlaceOne", XUiC_GamepadCalloutWindow.CalloutType.MenuHoverItem);
					}
				}
				else if (_canSwap)
				{
					this.AddCallout(UIUtils.ButtonIcon.FaceButtonSouth, "igcoSwap", XUiC_GamepadCalloutWindow.CalloutType.MenuHoverItem);
				}
			}
			else if (!dragAndDropWindow.CurrentStack.IsEmpty())
			{
				ItemClass itemClass = dragAndDropWindow.CurrentStack.itemValue.ItemClass;
				if (itemClass != null && _canSwap)
				{
					if (itemClass.CanStack())
					{
						this.AddCallout(UIUtils.ButtonIcon.FaceButtonSouth, "igcoPlaceAll", XUiC_GamepadCalloutWindow.CalloutType.MenuHoverItem);
						this.AddCallout(UIUtils.ButtonIcon.FaceButtonWest, "igcoPlaceOne", XUiC_GamepadCalloutWindow.CalloutType.MenuHoverItem);
					}
					else
					{
						this.AddCallout(UIUtils.ButtonIcon.FaceButtonSouth, "igcoPlace", XUiC_GamepadCalloutWindow.CalloutType.MenuHoverItem);
					}
				}
			}
			this.EnableCallouts(XUiC_GamepadCalloutWindow.CalloutType.MenuHoverItem, 0f);
			return;
		}
		this.stackObject = null;
		this.DisableCallouts(XUiC_GamepadCalloutWindow.CalloutType.MenuHoverItem);
		this.ShowCallout(UIUtils.ButtonIcon.FaceButtonSouth, XUiC_GamepadCalloutWindow.CalloutType.Menu, true);
	}

	public void HideCallouts(bool _hideCallouts)
	{
		this.hideCallouts = _hideCallouts;
		this.ShowCallouts();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const int Y_OFFSET = 5;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int X_OFFSET = 0;

	[PublicizedFrom(EAccessModifier.Private)]
	public PlayerInputManager.InputStyle controllerStyle;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<XUiC_GamepadCalloutWindow.Callout> callouts = new List<XUiC_GamepadCalloutWindow.Callout>();

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<XUiC_GamepadCalloutWindow.CalloutType, List<XUiC_GamepadCalloutWindow.Callout>> calloutGroups = new EnumDictionary<XUiC_GamepadCalloutWindow.CalloutType, List<XUiC_GamepadCalloutWindow.Callout>>();

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_GamepadCalloutWindow.VisibilityData[] typeVisible = new XUiC_GamepadCalloutWindow.VisibilityData[17];

	[PublicizedFrom(EAccessModifier.Private)]
	public bool localActionsEnabled;

	[PublicizedFrom(EAccessModifier.Private)]
	public GameObject stackObject;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool hideCallouts = true;

	public enum CalloutType
	{
		Menu,
		MenuLoot,
		MenuHoverItem,
		MenuHoverAir,
		SelectedOption,
		MenuCategory,
		MenuPaging,
		MenuComboBox,
		MenuShortcuts,
		World,
		Tabs,
		ColorPicker,
		CharacterEditor,
		RWGEditor,
		RWGCamera,
		CharacterPreview,
		CameraZoom,
		Count
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public class VisibilityData
	{
		public bool isVisible;

		public float duration;

		public float activeDuration;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public class Callout : MonoBehaviour
	{
		public void Awake()
		{
			this.bIsVisible = true;
			base.gameObject.SetActive(false);
			if (this.iconSprite == null)
			{
				this.iconSprite = NGUITools.AddChild<UISprite>(base.gameObject);
			}
			this.iconSprite.pivot = 0;
			this.iconSprite.height = 35;
			this.iconSprite.width = 35;
			this.iconSprite.transform.localPosition = Vector3.zero;
			if (this.atlasToSet != null)
			{
				this.iconSprite.atlas = this.atlasToSet;
				this.atlasToSet = null;
			}
			this.iconSprite.fixedAspect = true;
			this.actionLabel = NGUITools.AddChild<UILabel>(base.gameObject);
			this.actionLabel.font = XUiC_GamepadCalloutWindow.Callout.calloutFont;
			this.actionLabel.fontSize = 32;
			this.actionLabel.pivot = 0;
			this.actionLabel.overflowMethod = 2;
			this.actionLabel.alignment = 1;
			this.actionLabel.transform.localPosition = new Vector2(40f, 0f);
			this.actionLabel.effectStyle = 2;
			this.actionLabel.effectColor = new Color32(0, 0, 0, byte.MaxValue);
			this.actionLabel.effectDistance = new Vector2(1.5f, 1.5f);
		}

		public void SetupCallout(UIUtils.ButtonIcon _icon, string _action)
		{
			if (this.iconSprite != null && this.actionLabel != null)
			{
				this.icon = _icon;
				this.action = _action;
				this.iconSprite.spriteName = UIUtils.GetSpriteName(_icon);
				this.actionLabel.text = Localization.Get(_action, false, null);
			}
		}

		public void SetAtlas(INGUIAtlas _atlas)
		{
			if (this.iconSprite != null)
			{
				this.iconSprite.atlas = _atlas;
				return;
			}
			this.atlasToSet = _atlas;
		}

		public void FreeCallout()
		{
			this.isFree = true;
			this.icon = UIUtils.ButtonIcon.Count;
			this.action = "";
		}

		public void RefreshIcon()
		{
			if (!this.isFree && this.icon != UIUtils.ButtonIcon.Count && this.iconSprite != null)
			{
				this.iconSprite.spriteName = UIUtils.GetSpriteName(this.icon);
			}
		}

		public UIUtils.ButtonIcon icon;

		public string action;

		public static NGUIFont calloutFont;

		public UISprite iconSprite;

		public UILabel actionLabel;

		public XUiC_GamepadCalloutWindow.CalloutType type;

		public bool bIsVisible = true;

		public bool isFree = true;

		[PublicizedFrom(EAccessModifier.Private)]
		[NonSerialized]
		public INGUIAtlas atlasToSet;
	}
}
