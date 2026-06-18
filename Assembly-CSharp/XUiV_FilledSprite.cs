using System;
using UnityEngine;

public class XUiV_FilledSprite : XUiV_Sprite
{
	public override UIBasicSprite.Type Type
	{
		get
		{
			return this.type;
		}
		set
		{
		}
	}

	public XUiV_FilledSprite(XUi _xui, string _id)
		: base(_xui, _id)
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void updateData()
	{
		if (base.applyAtlasAndSprite(false))
		{
			Vector4 border = this.sprite.border;
			this.spriteBorder = new Vector2i(Mathf.RoundToInt(border.x + border.z), Mathf.RoundToInt(border.y + border.w));
		}
		if (this.fillSpritePad)
		{
			XUiUtils.ApplyFillPaddedSprite(this.sprite, this.spriteName);
		}
		this.sprite.centerType = (this.fillCenter ? 1 : 0);
		int num = ((this.fillDirection == null) ? Mathf.RoundToInt(this.fillAmount * (float)this.size.x) : this.size.x);
		int num2 = ((this.fillDirection == 1) ? Mathf.RoundToInt(this.fillAmount * (float)this.size.y) : this.size.y);
		if (num != this.sprite.width || num2 != this.sprite.height || this.positionDirty)
		{
			this.positionDirty = false;
			this.sprite.SetDimensions(num, num2);
			this.hideFill = (this.fillDirection == null && num < this.spriteBorder.x) || (this.fillDirection == 1 && num2 < this.spriteBorder.y);
			if (this.hideFill)
			{
				this.sprite.color = Color.clear;
			}
			int num3 = 0;
			int num4 = 0;
			int num5 = 0;
			int num6 = 0;
			int num7 = 0;
			int num8 = 0;
			if (this.fillDirection == null)
			{
				if (!this.fillInvert)
				{
					num3 = 0;
					num4 = Mathf.FloorToInt((float)(-(float)this.size.x + num) / 2f);
					num5 = -this.size.x + num;
				}
				else
				{
					num3 = this.size.x - num;
					num4 = Mathf.CeilToInt((float)(this.size.x - num) / 2f);
					num5 = 0;
				}
			}
			else
			{
				if (this.fillDirection != 1)
				{
					Log.Warning("[XUi] FilledSprite only allows FillDirections Horizontal and Vertical. On " + base.Controller.GetParentWindow().ID + "." + base.ID);
					return;
				}
				if (!this.fillInvert)
				{
					num6 = 0;
					num7 = Mathf.FloorToInt((float)(-(float)this.size.y + num2) / 2f);
					num8 = -this.size.y + num2;
				}
				else
				{
					num6 = this.size.y - num2;
					num7 = Mathf.CeilToInt((float)(this.size.y - num2) / 2f);
					num8 = 0;
				}
			}
			int num9;
			switch (this.pivot)
			{
			case 0:
			case 3:
			case 6:
				num9 = num3;
				break;
			case 1:
			case 4:
			case 7:
				num9 = num4;
				break;
			case 2:
			case 5:
			case 8:
				num9 = num5;
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			int num10 = num9;
			switch (this.pivot)
			{
			case 0:
			case 1:
			case 2:
				num9 = num8;
				break;
			case 3:
			case 4:
			case 5:
				num9 = num7;
				break;
			case 6:
			case 7:
			case 8:
				num9 = num6;
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			int num11 = num9;
			this.positionDirty = false;
			this.uiTransform.localPosition = new Vector3((float)(base.PaddedPosition.x + num10), (float)(base.PaddedPosition.y + num11), 0f);
		}
		if (!this.hideFill)
		{
			this.sprite.color = base.opacityModColor(this.color);
		}
		this.sprite.type = this.type;
		this.sprite.flip = this.flip;
		this.refreshBoxCollider();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void refreshBoxCollider()
	{
		float num = (float)this.size.x * 0.5f;
		float num2 = (float)this.size.y * 0.5f;
		float num3;
		float num4;
		switch (this.pivot)
		{
		case 0:
			num3 = num;
			num4 = 0f - num2;
			break;
		case 1:
			num3 = 0f;
			num4 = 0f - num2;
			break;
		case 2:
			num3 = 0f - num;
			num4 = 0f - num2;
			break;
		case 3:
			num3 = num;
			num4 = 0f;
			break;
		case 4:
			num3 = 0f;
			num4 = 0f;
			break;
		case 5:
			num3 = 0f - num;
			num4 = 0f;
			break;
		case 6:
			num3 = num;
			num4 = num2;
			break;
		case 7:
			num3 = 0f;
			num4 = num2;
			break;
		case 8:
			num3 = 0f - num;
			num4 = num2;
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
		this.collider.center = new Vector3(num3, num4, 0f);
		this.collider.size = new Vector3((float)this.size.x * base.ColliderScale + (float)(2 * base.ColliderPadding.x), (float)this.size.y * base.ColliderScale + (float)(2 * base.ColliderPadding.y), 0f);
	}

	public override void SetDefaults(XUiController _parent)
	{
		base.SetDefaults(_parent);
		base.FillCenter = true;
		this.type = 1;
		base.FillDirection = 0;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2i spriteBorder;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool hideFill;
}
