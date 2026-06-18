using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class UIItemSlot : MonoBehaviour
{
	public abstract InvGameItem observedItem
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public abstract InvGameItem Replace(InvGameItem item);

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnTooltip(bool show)
	{
		InvGameItem invGameItem = (show ? this.mItem : null);
		if (invGameItem != null)
		{
			InvBaseItem baseItem = invGameItem.baseItem;
			if (baseItem != null)
			{
				string text = string.Concat(new string[]
				{
					"[",
					NGUIText.EncodeColor(invGameItem.color),
					"]",
					invGameItem.name,
					"[-]\n"
				});
				text = string.Concat(new string[]
				{
					text,
					"[AFAFAF]Level ",
					invGameItem.itemLevel.ToString(),
					" ",
					baseItem.slot.ToString()
				});
				List<InvStat> list = invGameItem.CalculateStats();
				int i = 0;
				int count = list.Count;
				while (i < count)
				{
					InvStat invStat = list[i];
					if (invStat.amount != 0)
					{
						if (invStat.amount < 0)
						{
							text = text + "\n[FF0000]" + invStat.amount.ToString();
						}
						else
						{
							text = text + "\n[00FF00]+" + invStat.amount.ToString();
						}
						if (invStat.modifier == InvStat.Modifier.Percent)
						{
							text += "%";
						}
						text = text + " " + invStat.id.ToString();
						text += "[-]";
					}
					i++;
				}
				if (!string.IsNullOrEmpty(baseItem.description))
				{
					text = text + "\n[FF9900]" + baseItem.description;
				}
				UITooltip.Show(text);
				return;
			}
		}
		UITooltip.Hide();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnClick()
	{
		if (UIItemSlot.mDraggedItem != null)
		{
			this.OnDrop(null);
			return;
		}
		if (this.mItem != null)
		{
			UIItemSlot.mDraggedItem = this.Replace(null);
			if (UIItemSlot.mDraggedItem != null)
			{
				NGUITools.PlaySound(this.grabSound);
			}
			this.UpdateCursor();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnDrag(Vector2 delta)
	{
		if (UIItemSlot.mDraggedItem == null && this.mItem != null)
		{
			UICamera.currentTouch.clickNotification = 2;
			UIItemSlot.mDraggedItem = this.Replace(null);
			NGUITools.PlaySound(this.grabSound);
			this.UpdateCursor();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnDrop(GameObject go)
	{
		InvGameItem invGameItem = this.Replace(UIItemSlot.mDraggedItem);
		if (UIItemSlot.mDraggedItem == invGameItem)
		{
			NGUITools.PlaySound(this.errorSound);
		}
		else if (invGameItem != null)
		{
			NGUITools.PlaySound(this.grabSound);
		}
		else
		{
			NGUITools.PlaySound(this.placeSound);
		}
		UIItemSlot.mDraggedItem = invGameItem;
		this.UpdateCursor();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateCursor()
	{
		if (UIItemSlot.mDraggedItem != null && UIItemSlot.mDraggedItem.baseItem != null)
		{
			UICursor.Set(UIItemSlot.mDraggedItem.baseItem.iconAtlas as INGUIAtlas, UIItemSlot.mDraggedItem.baseItem.iconName);
			return;
		}
		UICursor.Clear();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Update()
	{
		InvGameItem observedItem = this.observedItem;
		if (this.mItem != observedItem)
		{
			this.mItem = observedItem;
			InvBaseItem invBaseItem = ((observedItem != null) ? observedItem.baseItem : null);
			if (this.label != null)
			{
				string text = ((observedItem != null) ? observedItem.name : null);
				if (string.IsNullOrEmpty(this.mText))
				{
					this.mText = this.label.text;
				}
				this.label.text = ((text != null) ? text : this.mText);
			}
			if (this.icon != null)
			{
				if (invBaseItem == null || invBaseItem.iconAtlas == null)
				{
					this.icon.enabled = false;
				}
				else
				{
					this.icon.atlas = invBaseItem.iconAtlas as INGUIAtlas;
					this.icon.spriteName = invBaseItem.iconName;
					this.icon.enabled = true;
					this.icon.MakePixelPerfect();
				}
			}
			if (this.background != null)
			{
				this.background.color = ((observedItem != null) ? observedItem.color : Color.white);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public UIItemSlot()
	{
	}

	public UISprite icon;

	public UIWidget background;

	public UILabel label;

	public AudioClip grabSound;

	public AudioClip placeSound;

	public AudioClip errorSound;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public InvGameItem mItem;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public string mText = "";

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static InvGameItem mDraggedItem;
}
