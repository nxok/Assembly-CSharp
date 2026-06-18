using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_CompanionEntry : XUiController
{
	public EntityAlive Companion { get; set; }

	public override void Init()
	{
		base.Init();
		this.IsDirty = true;
		XUiController[] array = base.GetChildrenById("BarHealth", null);
		if (array != null)
		{
			this.barHealth = new XUiV_Sprite[array.Length];
			for (int i = 0; i < array.Length; i++)
			{
				this.barHealth[i] = (XUiV_Sprite)array[i].ViewComponent;
			}
		}
		array = base.GetChildrenById("BarHealthModifiedMax", null);
		if (array != null)
		{
			this.barHealthModifiedMax = new XUiV_Sprite[array.Length];
			for (int j = 0; j < array.Length; j++)
			{
				this.barHealthModifiedMax[j] = (XUiV_Sprite)array[j].ViewComponent;
			}
		}
		array = base.GetChildrenById("BarStamina", null);
		if (array != null)
		{
			this.barStamina = new XUiV_Sprite[array.Length];
			for (int k = 0; k < array.Length; k++)
			{
				this.barStamina[k] = (XUiV_Sprite)array[k].ViewComponent;
			}
		}
		array = base.GetChildrenById("BarStaminaModifiedMax", null);
		if (array != null)
		{
			this.barStaminaModifiedMax = new XUiV_Sprite[array.Length];
			for (int l = 0; l < array.Length; l++)
			{
				this.barStaminaModifiedMax[l] = (XUiV_Sprite)array[l].ViewComponent;
			}
		}
		XUiController childById = base.GetChildById("arrowContent");
		if (childById != null)
		{
			this.arrowContent = (XUiV_Sprite)childById.ViewComponent;
		}
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		this.deltaTime = _dt;
		if (this.Companion == null || !XUi.IsGameRunning())
		{
			return;
		}
		this.RefreshFill();
		if (Time.time > this.updateTime)
		{
			this.updateTime = Time.time + 1f;
			if (this.HasChanged() || this.IsDirty)
			{
				if (this.IsDirty)
				{
					base.RefreshBindings();
					this.IsDirty = false;
				}
				else
				{
					base.RefreshBindings();
				}
			}
		}
		if (this.Companion != null && this.arrowContent != null)
		{
			this.arrowRotation = this.ReturnRotation(this.xui.playerUI.entityPlayer, this.Companion);
			if (this.lastArrowRotation < 15f && this.arrowRotation > 345f)
			{
				this.lastArrowRotation = this.arrowRotation;
			}
			else if (this.lastArrowRotation > 345f && this.arrowRotation < 15f)
			{
				this.lastArrowRotation = this.arrowRotation;
			}
			else
			{
				this.lastArrowRotation = Mathf.Lerp(this.lastArrowRotation, this.arrowRotation, _dt * 3f);
			}
			this.arrowContent.UiTransform.localEulerAngles = new Vector3(0f, 0f, this.lastArrowRotation - 180f);
		}
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public void SetCompanion(EntityAlive entity)
	{
		this.Companion = entity;
		if (this.Companion == null)
		{
			base.RefreshBindings();
			return;
		}
		this.IsDirty = true;
	}

	public bool HasChanged()
	{
		EntityPlayer entityPlayer = this.xui.playerUI.entityPlayer;
		float magnitude = (this.Companion.GetPosition() - entityPlayer.GetPosition()).magnitude;
		bool flag = this.oldValue != magnitude;
		this.oldValue = magnitude;
		this.distance = magnitude;
		return flag;
	}

	public void RefreshFill()
	{
		if (this.Companion == null)
		{
			return;
		}
		float num = Time.deltaTime * 3f;
		if (this.barHealth != null)
		{
			float valuePercentUI = this.Companion.Stats.Health.ValuePercentUI;
			float num2 = Math.Max(this.lastHealthValue, 0f) * 1.01f;
			this.lastHealthValue = Mathf.Lerp(this.lastHealthValue, valuePercentUI, num);
			for (int i = 0; i < this.barHealth.Length; i++)
			{
				this.barHealth[i].Fill = num2;
			}
		}
		if (this.barHealthModifiedMax != null)
		{
			for (int j = 0; j < this.barHealthModifiedMax.Length; j++)
			{
				this.barHealthModifiedMax[j].Fill = this.Companion.Stats.Health.ModifiedMax / this.Companion.Stats.Health.Max;
			}
		}
		if (this.barStamina != null)
		{
			float valuePercentUI2 = this.Companion.Stats.Stamina.ValuePercentUI;
			float num3 = Math.Max(this.lastStaminaValue, 0f) * 1.01f;
			this.lastStaminaValue = Mathf.Lerp(this.lastStaminaValue, valuePercentUI2, num);
			for (int k = 0; k < this.barStamina.Length; k++)
			{
				this.barStamina[k].Fill = num3;
			}
		}
		if (this.barStaminaModifiedMax != null)
		{
			for (int l = 0; l < this.barStaminaModifiedMax.Length; l++)
			{
				this.barStaminaModifiedMax[l].Fill = this.Companion.Stats.Stamina.ModifiedMax / this.Companion.Stats.Stamina.Max;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string value, string bindingName)
	{
		uint num = <PrivateImplementationDetails>.ComputeStringHash(bindingName);
		if (num <= 1339791005U)
		{
			if (num <= 783488098U)
			{
				if (num != 365230134U)
				{
					if (num != 679423209U)
					{
						if (num == 783488098U)
						{
							if (bindingName == "distance")
							{
								if (this.Companion == null)
								{
									value = "";
									return true;
								}
								value = this.distanceFormatter.Format(this.distance);
								return true;
							}
						}
					}
					else if (bindingName == "arrowcolor")
					{
						Color32 color = Color.white;
						if (this.Companion == null)
						{
							value = "255,255,255,255";
							return true;
						}
						int num2 = this.xui.playerUI.entityPlayer.Companions.IndexOf(this.Companion);
						color = Constants.TrackedFriendColors[num2 % Constants.TrackedFriendColors.Length];
						value = this.arrowcolorFormatter.Format(color);
						return true;
					}
				}
				else if (bindingName == "healthfill")
				{
					if (this.Companion == null)
					{
						value = "0";
						return true;
					}
					float valuePercentUI = this.Companion.Stats.Health.ValuePercentUI;
					value = this.healthfillFormatter.Format(valuePercentUI);
					return true;
				}
			}
			else if (num != 833193568U)
			{
				if (num != 1129104269U)
				{
					if (num == 1339791005U)
					{
						if (bindingName == "showarrow")
						{
							if (this.Companion == null)
							{
								value = "false";
								return true;
							}
							value = this.Companion.IsAlive().ToString();
							return true;
						}
					}
				}
				else if (bindingName == "showicon")
				{
					if (this.Companion == null)
					{
						value = "false";
						return true;
					}
					value = this.Companion.IsDead().ToString();
					return true;
				}
			}
			else if (bindingName == "healthcurrent")
			{
				if (this.Companion == null)
				{
					value = "";
					return true;
				}
				value = this.healthcurrentFormatter.Format(this.Companion.Health);
				return true;
			}
		}
		else if (num <= 2535347189U)
		{
			if (num != 1810125418U)
			{
				if (num != 2369371622U)
				{
					if (num == 2535347189U)
					{
						if (bindingName == "distancecolor")
						{
							Color32 color2 = Color.white;
							if (this.Companion == null)
							{
								value = "255,255,255,255";
								return true;
							}
							if (this.distance > 100f)
							{
								color2 = Color.grey;
							}
							value = this.itemicontintcolorFormatter.Format(color2);
							return true;
						}
					}
				}
				else if (bindingName == "name")
				{
					if (this.Companion == null)
					{
						value = "";
						return true;
					}
					value = this.Companion.EntityName;
					return true;
				}
			}
			else if (bindingName == "healthmodifiedmax")
			{
				if (this.Companion == null || this.xui.playerUI.entityPlayer.IsDead())
				{
					value = "0";
					return true;
				}
				value = (this.Companion.Stats.Health.ModifiedMax / this.Companion.Stats.Health.Max).ToCultureInvariantString();
				return true;
			}
		}
		else if (num != 3004043574U)
		{
			if (num != 3862959600U)
			{
				if (num == 3910386971U)
				{
					if (bindingName == "partyvisible")
					{
						if (this.Companion == null)
						{
							value = "false";
							return true;
						}
						value = "true";
						return true;
					}
				}
			}
			else if (bindingName == "icon")
			{
				if (this.Companion == null || GameStats.GetBool(EnumGameStats.AutoParty))
				{
					value = "";
					return true;
				}
				if (this.Companion.IsDead())
				{
					value = "ui_game_symbol_death";
				}
				else
				{
					value = "";
				}
				return true;
			}
		}
		else if (bindingName == "healthcurrentwithmax")
		{
			if (this.Companion == null)
			{
				value = "";
				return true;
			}
			value = this.healthcurrentWMaxFormatter.Format(this.Companion.Health, this.Companion.GetMaxHealth());
			return true;
		}
		return base.GetBindingValueInternal(ref value, bindingName);
	}

	public override void OnOpen()
	{
		base.OnOpen();
		this.IsDirty = true;
	}

	public override void OnClose()
	{
		base.OnClose();
		this.SetCompanion(null);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public float ReturnRotation(EntityAlive _self, EntityAlive _other)
	{
		Vector2 vector = new Vector2(_self.transform.forward.x, _self.transform.forward.z);
		Vector3 normalized = (_self.transform.position - _other.transform.position).normalized;
		Vector2 vector2;
		vector2..ctor(normalized.x, normalized.z);
		Vector3 vector3 = Vector3.Cross(vector, vector2);
		float num = Vector2.Angle(vector, vector2);
		if (vector3.z < 0f)
		{
			num = 360f - num;
		}
		return num;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Sprite[] barHealth;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Sprite[] barHealthModifiedMax;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Sprite[] barStamina;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Sprite[] barStaminaModifiedMax;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Sprite arrowContent;

	[PublicizedFrom(EAccessModifier.Private)]
	public float lastHealthValue;

	[PublicizedFrom(EAccessModifier.Private)]
	public float lastStaminaValue;

	[PublicizedFrom(EAccessModifier.Private)]
	public float distance;

	[PublicizedFrom(EAccessModifier.Private)]
	public float deltaTime;

	[PublicizedFrom(EAccessModifier.Protected)]
	public float updateTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public float arrowRotation;

	[PublicizedFrom(EAccessModifier.Private)]
	public float lastArrowRotation;

	[PublicizedFrom(EAccessModifier.Private)]
	public float oldValue;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterInt healthcurrentFormatter = new CachedStringFormatterInt();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatter<int, int> healthcurrentWMaxFormatter = new CachedStringFormatter<int, int>((int _i, int _i2) => string.Format("{0}/{1}", _i, _i2));

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterFloat healthfillFormatter = new CachedStringFormatterFloat(null);

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatter<float> distanceFormatter = new CachedStringFormatter<float>(delegate(float _f)
	{
		if (_f > 1000f)
		{
			return (_f / 1000f).ToCultureInvariantString("0.0") + " KM";
		}
		return _f.ToCultureInvariantString("0.0") + " M";
	});

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterXuiRgbaColor itemicontintcolorFormatter = new CachedStringFormatterXuiRgbaColor();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterXuiRgbaColor arrowcolorFormatter = new CachedStringFormatterXuiRgbaColor();
}
