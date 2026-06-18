using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_SignStack : XUiC_SelectableEntry
{
	public SignData SignData
	{
		get
		{
			return this.signData;
		}
	}

	public string LibraryName
	{
		get
		{
			return this.libraryName;
		}
	}

	public XUiC_SignStack()
	{
		this.IsDirty = true;
	}

	public GlobalSignId SignId
	{
		get
		{
			return this.signId;
		}
		set
		{
			if (this.signId != value)
			{
				this.signId = value;
				this.IsDirty = true;
				if (!this.signId.IsValid)
				{
					this.SetItemNameText("");
					this.libraryColor = XUiC_SignStack.backgroundColor;
				}
				else
				{
					this.libraryName = SignDataManager.GetLibraryNiceName(this.signId);
					if (!SignDataManager.Instance.TryGetSignData(this.signId, out this.signData))
					{
						this.SetItemNameText("");
						this.libraryColor = XUiC_SignStack.backgroundColor;
						Log.Error("Failed to retrieve sign data for global id: '" + this.signId.ToString() + "'.");
					}
					else
					{
						this.SetItemNameText(this.signData.name + " (" + this.libraryName + ")");
						string libraryId = this.signId.libraryId;
						if (!(libraryId == "[D]"))
						{
							if (!(libraryId == "[U]"))
							{
								this.libraryColor = XUiC_SignStack.libraryMarkerColorPrefab;
							}
							else
							{
								this.libraryColor = XUiC_SignStack.libraryMarkerColorUser;
							}
						}
						else
						{
							this.libraryColor = XUiC_SignStack.libraryMarkerColorDefault;
						}
					}
				}
			}
			this.signMaterial.IsVisible = this.signId.IsValid;
			base.ViewComponent.Enabled = this.signId.IsValid;
			this.SetColor(this.libraryColor);
			base.RefreshBindings();
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void SelectedChanged(bool isSelected)
	{
		if (!isSelected)
		{
			this.SetColor(this.libraryColor);
			return;
		}
		this.SetColor(this.selectColor);
		Action<XUiC_SignStack> onBecameSelected = this.OnBecameSelected;
		if (onBecameSelected == null)
		{
			return;
		}
		onBecameSelected(this);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetColor(Color32 color)
	{
		this.background.Color = color;
	}

	public override void Init()
	{
		base.Init();
		this.highlightOverlay = base.GetChildById("highlightOverlay").ViewComponent as XUiV_Sprite;
		this.background = base.GetChildById("background").ViewComponent as XUiV_Sprite;
		this.signMaterial = base.GetChildById("signMaterial").ViewComponent as XUiV_Texture;
		this.signMaterial.CreateMaterial("Game/SignTech/UI");
		XUiV_Texture xuiV_Texture = this.signMaterial;
		xuiV_Texture.OnRenderTexture = (UIDrawCall.OnRenderCallback)Delegate.Combine(xuiV_Texture.OnRenderTexture, new UIDrawCall.OnRenderCallback(this.OnWillRender));
		this.signMaterial.Texture = Texture2D.whiteTexture;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnWillRender(Material mat)
	{
		SignDataManager.Instance.TryApplyRenderingData(this.signId, 1f, mat, null, SignUIStyle.Standard);
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		PlayerActionsGUI guiactions = this.xui.playerUI.playerInput.GUIActions;
		if (base.WindowGroup.isShowing)
		{
			CursorControllerAbs cursorController = this.xui.playerUI.CursorController;
			cursorController.GetScreenPosition();
			bool mouseButtonUp = cursorController.GetMouseButtonUp(0);
			cursorController.GetMouseButtonDown(0);
			cursorController.GetMouseButton(0);
			cursorController.GetMouseButtonUp(1);
			cursorController.GetMouseButtonDown(1);
			cursorController.GetMouseButton(1);
			if (this.isOver && base.ViewComponent.UiTransformIsHovered && base.ViewComponent.EventOnPress)
			{
				if (guiactions.LastInputType == 1)
				{
					if (guiactions.Submit.WasReleased)
					{
						GlobalSignId globalSignId = this.signId;
						base.IsSelected = true;
					}
				}
				else if (mouseButtonUp)
				{
					GlobalSignId globalSignId2 = this.signId;
					base.IsSelected = true;
				}
			}
			else
			{
				this.currentColor = this.libraryColor;
				if (this.highlightOverlay != null)
				{
					this.highlightOverlay.Color = XUiC_SignStack.backgroundColor;
				}
				if (!base.IsSelected)
				{
					this.background.Color = this.currentColor;
				}
				if (this.isOver)
				{
					this.isOver = false;
				}
			}
		}
		if (this.IsDirty)
		{
			this.IsDirty = false;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetItemNameText(string name)
	{
		XUiView viewComponent = this.viewComponent;
		GlobalSignId globalSignId = this.signId;
		viewComponent.ToolTip = name;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnHovered(bool _isOver)
	{
		this.isOver = _isOver;
		if (!base.IsSelected)
		{
			if (_isOver)
			{
				this.background.Color = XUiC_SignStack.highlightColor;
			}
			else
			{
				this.background.Color = this.libraryColor;
			}
		}
		base.OnHovered(_isOver);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string value, string bindingName)
	{
		return base.GetBindingValueInternal(ref value, bindingName);
	}

	public override bool ParseAttribute(string name, string value)
	{
		bool flag = base.ParseAttribute(name, value);
		if (!flag)
		{
			if (!(name == "select_color"))
			{
				if (!(name == "background_color"))
				{
					if (!(name == "highlight_color"))
					{
						if (!(name == "lib_marker_color_default"))
						{
							if (!(name == "lib_marker_color_prefab"))
							{
								return false;
							}
							XUiC_SignStack.libraryMarkerColorPrefab = StringParsers.ParseColor32(value);
						}
						else
						{
							XUiC_SignStack.libraryMarkerColorDefault = StringParsers.ParseColor32(value);
						}
					}
					else
					{
						XUiC_SignStack.highlightColor = StringParsers.ParseColor32(value);
					}
				}
				else
				{
					XUiC_SignStack.backgroundColor = StringParsers.ParseColor32(value);
				}
			}
			else
			{
				this.selectColor = StringParsers.ParseColor32(value);
			}
			return true;
		}
		return flag;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isOver;

	[PublicizedFrom(EAccessModifier.Private)]
	public Color32 currentColor;

	[PublicizedFrom(EAccessModifier.Private)]
	public Color32 selectColor = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);

	public static Color32 backgroundColor = new Color32(96, 96, 96, byte.MaxValue);

	public static Color32 highlightColor = new Color32(222, 206, 163, byte.MaxValue);

	public static Color32 libraryMarkerColorDefault = new Color32(120, 0, 200, byte.MaxValue);

	public static Color32 libraryMarkerColorPrefab = new Color32(45, 220, 30, byte.MaxValue);

	public static Color32 libraryMarkerColorUser = new Color32(0, 170, 190, byte.MaxValue);

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Sprite highlightOverlay;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Sprite background;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Texture signMaterial;

	[PublicizedFrom(EAccessModifier.Private)]
	public Color32 libraryColor;

	public SignData signData;

	public string libraryName;

	public Action<XUiC_SignStack> OnBecameSelected;

	[PublicizedFrom(EAccessModifier.Private)]
	public GlobalSignId signId;
}
