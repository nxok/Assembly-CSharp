using System;
using Challenges;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_ChallengeEntry : XUiController
{
	public Challenge Entry
	{
		get
		{
			return this.entry;
		}
		set
		{
			base.ViewComponent.Enabled = value != null;
			this.entry = value;
			this.challengeClass = ((this.entry != null) ? this.entry.ChallengeClass : null);
			if (this.challengeClass != null)
			{
				this.IsChallengeVisible = this.challengeClass.ChallengeGroup.IsVisible(this.entry.Owner.Player);
			}
			else
			{
				this.IsChallengeVisible = true;
			}
			this.IsDirty = true;
		}
	}

	public XUiC_ChallengeWindowGroup JournalUIHandler { get; set; }

	public bool Tracked { get; set; }

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string value, string bindingName)
	{
		bool flag = this.entry != null;
		uint num = <PrivateImplementationDetails>.ComputeStringHash(bindingName);
		if (num <= 765459171U)
		{
			if (num != 21245043U)
			{
				if (num != 480848564U)
				{
					if (num == 765459171U)
					{
						if (bindingName == "rowstatecolor")
						{
							value = this.rowColor;
							if (flag)
							{
								if (this.Selected)
								{
									value = this.selectedColor;
								}
								else if (this.IsHovered)
								{
									value = this.hoverColor;
								}
							}
							return true;
						}
					}
				}
				else if (bindingName == "fillamount")
				{
					value = ((flag && this.entry.IsActive && this.IsChallengeVisible) ? this.entry.FillAmount.ToString() : "0");
					return true;
				}
			}
			else if (bindingName == "iconname")
			{
				value = "";
				if (flag)
				{
					value = (this.IsChallengeVisible ? this.challengeClass.Icon : "ui_game_symbol_other");
				}
				return true;
			}
		}
		else if (num <= 1566407741U)
		{
			if (num != 1149663213U)
			{
				if (num == 1566407741U)
				{
					if (bindingName == "hasentry")
					{
						value = (flag ? "true" : "false");
						return true;
					}
				}
			}
			else if (bindingName == "tracked")
			{
				value = (flag ? this.entry.IsTracked.ToString() : "false");
				return true;
			}
		}
		else if (num != 2240895362U)
		{
			if (num == 3106195591U)
			{
				if (bindingName == "iconcolor")
				{
					value = "255,255,255,255";
					if (flag)
					{
						if (!this.IsChallengeVisible)
						{
							value = this.disabledColor;
						}
						else if (this.entry.ChallengeState == Challenge.ChallengeStates.Redeemed)
						{
							value = this.disabledColor;
						}
						else if (this.entry.ReadyToComplete)
						{
							value = this.redeemableColor;
						}
						else if (this.entry.IsTracked)
						{
							value = this.trackedColor;
						}
						else if (this.IsHovered)
						{
							value = this.hoverColor;
						}
						else
						{
							value = this.enabledColor;
						}
					}
					return true;
				}
			}
		}
		else if (bindingName == "fillactive")
		{
			if (flag)
			{
				if (!this.IsChallengeVisible)
				{
					value = "false";
				}
				else
				{
					value = this.entry.IsActive.ToString();
				}
			}
			else
			{
				value = "false";
			}
			return true;
		}
		return base.GetBindingValueInternal(ref value, bindingName);
	}

	public override void Init()
	{
		base.Init();
		XUiController childById = base.GetChildById("itemIcon");
		if (childById != null)
		{
			this.itemIconSprite = (XUiV_Sprite)childById.ViewComponent;
			this.iconSize = this.itemIconSprite.Size;
		}
		this.tweenScale = this.itemIconSprite.UiTransform.gameObject.AddComponent<TweenScale>();
		this.IsDirty = true;
	}

	public override void OnCursorSelected(bool _isActualElement)
	{
		base.OnCursorSelected(_isActualElement);
		XUiC_ChallengeEntryList.SelectedEntry = this;
	}

	public override void OnCursorUnSelected()
	{
		base.OnCursorUnSelected();
		this.Selected = false;
		this.IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnHovered(bool _isOver)
	{
		if (!this.IsChallengeVisible)
		{
			this.IsHovered = false;
			return;
		}
		base.OnHovered(_isOver);
		if (this.Entry == null)
		{
			this.IsHovered = false;
			return;
		}
		if (this.Entry != null && !this.IsRedeemBlinking)
		{
			if (_isOver)
			{
				this.tweenScale.from = Vector3.one;
				this.tweenScale.to = Vector3.one * 1.1f;
				this.tweenScale.enabled = true;
				this.tweenScale.duration = 0.5f;
			}
			else
			{
				this.tweenScale.from = Vector3.one * 1.1f;
				this.tweenScale.to = Vector3.one;
				this.tweenScale.enabled = true;
				this.tweenScale.duration = 0.5f;
			}
		}
		if (this.IsHovered != _isOver)
		{
			this.IsHovered = _isOver;
			base.RefreshBindings();
		}
	}

	public override void Update(float _dt)
	{
		if (this.IsDirty || (this.entry != null && this.entry.NeedsUIUpdate))
		{
			if (this.challengeClass != null)
			{
				this.IsChallengeVisible = this.challengeClass.ChallengeGroup.IsVisible(this.entry.Owner.Player);
			}
			else
			{
				this.IsChallengeVisible = true;
			}
			base.ViewComponent.SoundPlayOnHover = this.IsChallengeVisible;
			base.ViewComponent.SoundPlayOnClick = this.IsChallengeVisible;
		}
		base.RefreshBindings();
		this.IsDirty = false;
		base.Update(_dt);
		if (this.IsRedeemBlinking && !this.Selected)
		{
			this.tweenScale.enabled = false;
			float num = Mathf.PingPong(Time.time, 0.5f);
			float num2 = 1f;
			if (num > 0.25f)
			{
				num2 = 1f + num - 0.25f;
			}
			this.itemIconSprite.Size = new Vector2i((int)((float)this.iconSize.x * num2), (int)((float)this.iconSize.y * num2));
			return;
		}
		if (this.Selected)
		{
			this.itemIconSprite.Size = this.iconSize;
		}
	}

	public override bool ParseAttribute(string name, string value)
	{
		if (name == "enabled_color")
		{
			this.enabledColor = value;
			return true;
		}
		if (name == "disabled_color")
		{
			this.disabledColor = value;
			return true;
		}
		if (name == "row_color")
		{
			this.rowColor = value;
			return true;
		}
		if (name == "hover_color")
		{
			this.hoverColor = value;
			return true;
		}
		if (!(name == "selected_color"))
		{
			return base.ParseAttribute(name, value);
		}
		this.selectedColor = value;
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string enabledColor;

	[PublicizedFrom(EAccessModifier.Private)]
	public string disabledColor;

	[PublicizedFrom(EAccessModifier.Private)]
	public string rowColor;

	[PublicizedFrom(EAccessModifier.Private)]
	public string hoverColor;

	[PublicizedFrom(EAccessModifier.Private)]
	public string selectedColor;

	[PublicizedFrom(EAccessModifier.Private)]
	public string trackedColor = "255, 180, 0, 255";

	[PublicizedFrom(EAccessModifier.Private)]
	public string redeemableColor = "0,255,0,255";

	public new bool Selected;

	public bool IsHovered;

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiV_Sprite itemIconSprite;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2i iconSize;

	public bool IsRedeemBlinking;

	public bool IsChallengeVisible;

	[PublicizedFrom(EAccessModifier.Private)]
	public Challenge entry;

	[PublicizedFrom(EAccessModifier.Private)]
	public ChallengeClass challengeClass;

	[PublicizedFrom(EAccessModifier.Protected)]
	public TweenScale tweenScale;

	public XUiC_ChallengeEntryList Owner;
}
