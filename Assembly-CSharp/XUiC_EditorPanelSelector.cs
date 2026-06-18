using System;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_EditorPanelSelector : XUiController
{
	public override void Init()
	{
		base.Init();
		XUiC_EditorPanelSelector.ID = base.WindowGroup.Id;
		this.buttons = base.GetChildByType<XUiC_CategoryList>();
		if (this.buttons != null)
		{
			this.buttons.CategoryClickChanged += this.ButtonsOnCategoryClickChanged;
			this.buttons.SetCategoryToFirst();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ButtonsOnCategoryClickChanged(XUiC_CategoryEntry _categoryEntry)
	{
		this.OpenSelectedWindow();
	}

	public void OpenSelectedWindow()
	{
		if (this.buttons == null)
		{
			return;
		}
		XUiC_CategoryEntry currentCategory = this.buttons.CurrentCategory;
		string text = ((currentCategory != null) ? currentCategory.CategoryName : null);
		foreach (XUiC_CategoryEntry xuiC_CategoryEntry in this.buttons.CategoryButtons)
		{
			if (text == null || text != xuiC_CategoryEntry.CategoryName)
			{
				this.xui.playerUI.windowManager.Close(xuiC_CategoryEntry.CategoryName);
			}
		}
		if (text != null)
		{
			this.xui.playerUI.windowManager.Open(text, false);
		}
	}

	public void SetSelected(string _name)
	{
		XUiC_CategoryList xuiC_CategoryList = this.buttons;
		if (xuiC_CategoryList == null)
		{
			return;
		}
		xuiC_CategoryList.SetCategory(_name);
	}

	public override void OnOpen()
	{
		base.OnOpen();
		if (PrefabEditModeManager.Instance.IsActive() && PrefabEditModeManager.Instance.VoxelPrefab != null)
		{
			PrefabEditModeManager.Instance.VoxelPrefab.RenderingCostStats = WorldStats.CaptureWorldStats();
		}
		this.OpenSelectedWindow();
	}

	public override void OnClose()
	{
		base.OnClose();
		if (this.buttons != null)
		{
			foreach (XUiC_CategoryEntry xuiC_CategoryEntry in this.buttons.CategoryButtons)
			{
				this.xui.playerUI.windowManager.Close(xuiC_CategoryEntry.CategoryName);
			}
		}
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
		if (_bindingName == "panelname")
		{
			XUiC_CategoryList xuiC_CategoryList = this.buttons;
			string text;
			if (xuiC_CategoryList == null)
			{
				text = null;
			}
			else
			{
				XUiC_CategoryEntry currentCategory = xuiC_CategoryList.CurrentCategory;
				text = ((currentCategory != null) ? currentCategory.CategoryDisplayName : null);
			}
			_value = text ?? "";
			return true;
		}
		return base.GetBindingValueInternal(ref _value, _bindingName);
	}

	public static string ID = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_CategoryList buttons;
}
