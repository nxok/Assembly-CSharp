using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_DropDown : XUiController
{
	public event XUiEvent_InputOnSubmitEventHandler OnSubmitHandler;

	public event XUiC_DropDown.XUiEvent_InputOnChangedEventHandler OnChangeHandler;

	public int PageLength
	{
		get
		{
			XUiC_DropDown.Entry[] array = this.listEntryControllers;
			if (array == null)
			{
				return 1;
			}
			return array.Length;
		}
	}

	[XuiXmlBinding("dropdown_open")]
	public bool DropdownOpen
	{
		get
		{
			return this.dropdownOpen;
		}
		set
		{
			if (value == this.dropdownOpen)
			{
				return;
			}
			this.dropdownOpen = value;
			this.IsDirty = true;
		}
	}

	public int Page
	{
		get
		{
			return this.page;
		}
		set
		{
			int num = Mathf.Clamp(value, 0, this.LastPage);
			if (num == this.page)
			{
				return;
			}
			this.page = num;
			this.IsDirty = true;
		}
	}

	public int LastPage
	{
		get
		{
			return Math.Max(0, Mathf.CeilToInt((float)this.filteredEntries.Count / (float)this.PageLength) - 1);
		}
	}

	public int EntryCount
	{
		get
		{
			return this.filteredEntries.Count;
		}
	}

	public string Text
	{
		get
		{
			return this.TextInput.Text;
		}
		set
		{
			if (value == this.TextInput.Text)
			{
				return;
			}
			this.TextInput.Text = value;
			this.UpdateFilteredList();
		}
	}

	[XuiXmlAttribute("dropdown_hovercolor", false)]
	public Color DropdownHoverColor
	{
		get
		{
			return this.dropdownHoverColor;
		}
		set
		{
			if (value == this.dropdownHoverColor)
			{
				return;
			}
			this.dropdownHoverColor = value;
			this.IsDirty = true;
		}
	}

	[XuiXmlAttribute("dropdown_textcolor", false)]
	public Color DropdownTextColor
	{
		get
		{
			return this.dropdownTextColor;
		}
		set
		{
			if (value == this.dropdownTextColor)
			{
				return;
			}
			this.dropdownTextColor = value;
			this.IsDirty = true;
		}
	}

	[XuiXmlBinding("valid_input")]
	public bool ValidInput
	{
		get
		{
			return this.AllowCustomValue || (this.TextInput != null && this.AllEntries.Contains(this.Text));
		}
	}

	[XuiXmlAttribute("thumbareasize", false)]
	public float ThumbAreaSize
	{
		get
		{
			return this.thumbAreaSize;
		}
		set
		{
			if (Mathf.Approximately(this.thumbAreaSize, value))
			{
				return;
			}
			this.thumbAreaSize = value;
			this.IsDirty = true;
		}
	}

	[XuiXmlAttribute("sortentries", false)]
	public bool SortEntries
	{
		get
		{
			return this.sortEntries;
		}
		set
		{
			if (value == this.sortEntries)
			{
				return;
			}
			this.sortEntries = value;
			this.IsDirty = true;
		}
	}

	[XuiXmlAttribute("clearonopen", false)]
	public bool ClearOnOpen
	{
		get
		{
			return this.clearOnOpen;
		}
		set
		{
			if (value == this.clearOnOpen)
			{
				return;
			}
			this.clearOnOpen = value;
			this.IsDirty = true;
		}
	}

	[XuiXmlAttribute("allowcustomvalue", false)]
	public bool AllowCustomValue
	{
		get
		{
			return this.allowCustomValue;
		}
		set
		{
			if (value == this.allowCustomValue)
			{
				return;
			}
			this.allowCustomValue = value;
			this.IsDirty = true;
		}
	}

	[XuiXmlBinding("thumb_size")]
	public int ThumbSize
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return Mathf.RoundToInt(this.ThumbAreaSize / (float)(this.LastPage + 1));
		}
	}

	[XuiXmlBinding("thumb_position")]
	public int ThumbPosition
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return Mathf.RoundToInt((float)this.Page / (float)(this.LastPage + 1) * this.ThumbAreaSize);
		}
	}

	public override void Init()
	{
		base.Init();
		XUiController childById = base.GetChildById("pageUp");
		if (childById != null)
		{
			childById.OnPress += this.handlePageUpPress;
		}
		XUiController childById2 = base.GetChildById("pageDown");
		if (childById2 != null)
		{
			childById2.OnPress += this.handlePageDownPress;
		}
		this.handlePageDownAction = new Func<bool>(this.handlePageDown);
		this.handlePageUpAction = new Func<bool>(this.handlePageUp);
		for (int i = 0; i < this.listEntryControllers.Length; i++)
		{
			if (this.listEntryControllers[i] != null)
			{
				this.listEntryControllers[i].Owner = this;
			}
			else
			{
				Log.Warning("[XUi] DropDown elements do not have the correct controller set (should be \"XUiC_DropDown+Entry\")");
			}
		}
		XUiController childById3 = base.GetChildById("btnDropdown");
		if (childById3 != null)
		{
			childById3.OnPress += this.BtnDropdown_OnPress;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnDropdown_OnPress(XUiController _sender, int _mouseButton)
	{
		this.DropdownOpen = !this.DropdownOpen;
	}

	[XuiBindEvent("OnInputSelectedHandler", "TextInput")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void OnInputSelected(XUiController _sender, bool _selected)
	{
		if (_selected)
		{
			this.DropdownOpen = true;
			return;
		}
		ThreadManager.StartCoroutine(this.closeDropdownLater());
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator closeDropdownLater()
	{
		while (this.xui.playerUI.playerInput.GUIActions.LeftClick.IsPressed)
		{
			yield return null;
		}
		yield return null;
		if (!this.TextInput.IsSelected)
		{
			this.DropdownOpen = false;
		}
		yield break;
	}

	[XuiBindEvent("OnSubmitHandler", "TextInput")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void OnInputSubmit(XUiController _sender, string _text)
	{
		this.DropdownOpen = false;
		this.TextInput.SetSelected(false, false);
		this.OnInputChanged(_sender, _text, false);
		this.sendSubmitEvent();
	}

	[XuiBindEvent("OnChangeHandler", "TextInput")]
	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnInputChanged(XUiController _sender, string _text, bool _changeFromCode)
	{
		this.UpdateFilteredList();
		base.RefreshBindings();
		this.sendChangedEvent(_changeFromCode);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void sendChangedEvent(bool _changeFromCode)
	{
		XUiC_DropDown.XUiEvent_InputOnChangedEventHandler onChangeHandler = this.OnChangeHandler;
		if (onChangeHandler == null)
		{
			return;
		}
		onChangeHandler(this, this.Text, this.ValidInput, _changeFromCode);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void sendSubmitEvent()
	{
		XUiEvent_InputOnSubmitEventHandler onSubmitHandler = this.OnSubmitHandler;
		if (onSubmitHandler == null)
		{
			return;
		}
		onSubmitHandler(this, this.Text);
	}

	public void UpdateFilteredList()
	{
		XUiC_TextInput textInput = this.TextInput;
		string text = ((textInput != null) ? textInput.Text : null);
		this.filteredEntries.Clear();
		if (!string.IsNullOrEmpty(text))
		{
			using (List<string>.Enumerator enumerator = this.AllEntries.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					string text2 = enumerator.Current;
					if (text2.ContainsCaseInsensitive(text))
					{
						this.filteredEntries.Add(text2);
					}
				}
				goto IL_007B;
			}
		}
		this.filteredEntries.AddRange(this.AllEntries);
		IL_007B:
		if (this.SortEntries)
		{
			this.filteredEntries.Sort();
		}
		this.Page = 0;
		this.IsDirty = true;
	}

	[XuiBindEvent("OnScroll", null)]
	[XuiBindEvent("OnScroll", "listEntryControllers")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleOnScroll(XUiController _sender, float _delta)
	{
		if (_delta > 0f)
		{
			this.handlePageDown();
			return;
		}
		this.handlePageUp();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void handlePageDownPress(XUiController _sender, int _mouseButton)
	{
		this.handlePageDown();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void handlePageUpPress(XUiController _sender, int _mouseButton)
	{
		this.handlePageUp();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool handlePageDown()
	{
		if (!this.TextInput.IsSelected)
		{
			this.TextInput.SetSelected(true, false);
		}
		if (this.page > 0)
		{
			int num = this.Page;
			this.Page = num - 1;
			return true;
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool handlePageUp()
	{
		if (!this.TextInput.IsSelected)
		{
			this.TextInput.SetSelected(true, false);
		}
		if ((this.page + 1) * this.PageLength < this.filteredEntries.Count)
		{
			int num = this.Page;
			this.Page = num + 1;
			return true;
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateCurrentPageContents()
	{
		for (int i = 0; i < this.PageLength; i++)
		{
			int num = i + this.PageLength * this.page;
			this.listEntryControllers[i].Text = ((num < this.filteredEntries.Count) ? this.filteredEntries[num] : null);
		}
	}

	public override void Update(float _dt)
	{
		if (this.IsDirty)
		{
			if (this.page > this.LastPage)
			{
				this.Page = this.LastPage;
			}
			this.updateCurrentPageContents();
			base.RefreshBindings();
			this.IsDirty = false;
		}
		base.Update(_dt);
		if (base.ViewComponent.IsVisible)
		{
			XUi.HandlePaging(this.xui, this.handlePageUpAction, this.handlePageDownAction, false);
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		if (this.ClearOnOpen)
		{
			this.Text = string.Empty;
		}
		this.DropdownOpen = false;
		this.IsDirty = true;
		base.RefreshBindings();
	}

	public readonly List<string> AllEntries = new List<string>();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<string> filteredEntries = new List<string>();

	[XuiBindComponent(true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_DropDown.Entry[] listEntryControllers;

	[XuiBindComponent("input", false)]
	public readonly XUiC_TextInput TextInput;

	[PublicizedFrom(EAccessModifier.Private)]
	public Func<bool> handlePageDownAction;

	[PublicizedFrom(EAccessModifier.Private)]
	public Func<bool> handlePageUpAction;

	[PublicizedFrom(EAccessModifier.Private)]
	public float thumbAreaSize;

	[PublicizedFrom(EAccessModifier.Private)]
	public Color dropdownHoverColor;

	[PublicizedFrom(EAccessModifier.Private)]
	public Color dropdownTextColor;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool sortEntries;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool clearOnOpen;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool allowCustomValue = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public int page;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool dropdownOpen;

	[Preserve]
	public class Entry : XUiController
	{
		[XuiXmlBinding("name")]
		public string Text
		{
			get
			{
				return this.text ?? "";
			}
			set
			{
				if (value == this.text)
				{
					return;
				}
				this.text = value;
				this.IsDirty = true;
			}
		}

		[XuiXmlBinding("textcolor")]
		public Color TextColor
		{
			get
			{
				if (this.Owner == null)
				{
					return new Color(100f, 100f, 100f);
				}
				if (!this.hovered)
				{
					return this.Owner.DropdownTextColor;
				}
				return this.Owner.DropdownHoverColor;
			}
		}

		public override void OnOpen()
		{
			base.OnOpen();
			this.hovered = false;
			this.IsDirty = true;
		}

		[XuiBindEvent("OnPress", null)]
		[PublicizedFrom(EAccessModifier.Protected)]
		public void OnPressed(XUiController _sender, int _mouseButton)
		{
			this.Owner.Text = this.text;
			this.Owner.DropdownOpen = false;
			this.Owner.sendChangedEvent(false);
			this.Owner.sendSubmitEvent();
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override void OnHovered(bool _isOver)
		{
			base.OnHovered(_isOver);
			this.hovered = _isOver;
			this.IsDirty = true;
		}

		public override void Update(float _dt)
		{
			base.Update(_dt);
			base.handleDirtyUpdateDefault();
		}

		public XUiC_DropDown Owner;

		[PublicizedFrom(EAccessModifier.Private)]
		public string text;

		[PublicizedFrom(EAccessModifier.Private)]
		public bool hovered;
	}

	public delegate void XUiEvent_InputOnChangedEventHandler(XUiController _sender, string _text, bool _validInput, bool _changeFromCode);
}
