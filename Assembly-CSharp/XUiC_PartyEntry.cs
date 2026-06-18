using System;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_PartyEntry : XUiController
{
	public EntityPlayer Player { get; set; }

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
		XUiController xuiController = base.GetChildById("icon1");
		if (xuiController != null)
		{
			this.iconSprite1 = (XUiV_Sprite)xuiController.ViewComponent;
			this.defaultIconColor = this.iconSprite1.Color;
			this.iconSpriteSize = new Vector2((float)this.iconSprite1.Size.x, (float)this.iconSprite1.Size.y);
		}
		xuiController = base.GetChildById("icon2");
		if (xuiController != null)
		{
			this.iconSprite2 = (XUiV_Sprite)xuiController.ViewComponent;
		}
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (this.Player == null || !XUi.IsGameRunning())
		{
			return;
		}
		this.RefreshFill();
		this.updateVoiceState();
		if (Time.time > this.updateTime)
		{
			this.updateTime = Time.time + 0.5f;
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
		if (this.Player != null && this.arrowContent != null)
		{
			this.arrowRotation = this.ReturnRotation(this.xui.playerUI.entityPlayer, this.Player);
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
		if (this.Player != null)
		{
			if (this.Player.TwitchActionsEnabled == EntityPlayer.TwitchActionsStates.TempDisabledEnding)
			{
				float num = Mathf.PingPong(Time.time, 0.5f);
				float num2 = 1f;
				if (num > 0.25f)
				{
					num2 = 1f + num - 0.25f;
				}
				if (this.Player.Party.Leader == this.Player)
				{
					this.iconSprite2.Color = Color.Lerp(this.defaultIconColor, this.iconBlinkColor, num * 4f);
					this.iconSprite2.Size = new Vector2i(this.iconSpriteSize * num2);
					return;
				}
				this.iconSprite1.Color = Color.Lerp(this.defaultIconColor, this.iconBlinkColor, num * 4f);
				this.iconSprite1.Size = new Vector2i(this.iconSpriteSize * num2);
				return;
			}
			else
			{
				this.iconSprite1.Color = this.defaultIconColor;
				this.iconSprite2.Color = this.defaultIconColor;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateVoiceState()
	{
		IPartyVoice.EVoiceMemberState playerVoiceState = VoiceHelpers.GetPlayerVoiceState(this.Player, true);
		if (playerVoiceState != this.voiceState)
		{
			this.voiceState = playerVoiceState;
			this.IsDirty = true;
		}
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public void SetPlayer(EntityPlayer player)
	{
		this.Player = player;
		if (this.Player == null)
		{
			base.RefreshBindings();
			return;
		}
		this.IsDirty = true;
	}

	public bool HasChanged()
	{
		EntityPlayer entityPlayer = this.xui.playerUI.entityPlayer;
		float magnitude = (this.Player.GetPosition() - entityPlayer.GetPosition()).magnitude;
		bool flag = this.oldValue != magnitude || this.Player.TwitchEnabled != this.oldTwitch || this.Player.TwitchSafe != this.oldSafe || this.Player.TwitchActionsEnabled != this.oldTwitchActions;
		this.oldValue = magnitude;
		this.distance = magnitude;
		this.oldTwitch = this.Player.TwitchEnabled;
		this.oldSafe = this.Player.TwitchSafe;
		this.oldTwitchActions = this.Player.TwitchActionsEnabled;
		return flag;
	}

	public void RefreshFill()
	{
		if (this.Player == null)
		{
			return;
		}
		float num = Time.deltaTime * 3f;
		if (this.barHealth != null)
		{
			float valuePercentUI = this.Player.Stats.Health.ValuePercentUI;
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
				this.barHealthModifiedMax[j].Fill = this.Player.Stats.Health.ModifiedMax / this.Player.Stats.Health.Max;
			}
		}
		if (this.barStamina != null)
		{
			float valuePercentUI2 = this.Player.Stats.Stamina.ValuePercentUI;
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
				this.barStaminaModifiedMax[l].Fill = this.Player.Stats.Stamina.ModifiedMax / this.Player.Stats.Stamina.Max;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string value, string bindingName)
	{
		uint num = <PrivateImplementationDetails>.ComputeStringHash(bindingName);
		if (num <= 1810125418U)
		{
			if (num <= 833193568U)
			{
				if (num <= 679423209U)
				{
					if (num != 365230134U)
					{
						if (num == 679423209U)
						{
							if (bindingName == "arrowcolor")
							{
								if (this.Player == null)
								{
									value = "255,255,255,255";
									return true;
								}
								int num2 = this.Player.Party.MemberList.IndexOf(this.Player);
								Color32 color = Constants.TrackedFriendColors[num2 % Constants.TrackedFriendColors.Length];
								value = this.arrowcolorFormatter.Format(color);
								return true;
							}
						}
					}
					else if (bindingName == "healthfill")
					{
						if (this.Player == null)
						{
							value = "0";
							return true;
						}
						float valuePercentUI = this.Player.Stats.Health.ValuePercentUI;
						value = this.healthfillFormatter.Format(valuePercentUI);
						return true;
					}
				}
				else if (num != 783488098U)
				{
					if (num == 833193568U)
					{
						if (bindingName == "healthcurrent")
						{
							if (this.Player == null)
							{
								value = "";
								return true;
							}
							value = this.healthcurrentFormatter.Format(this.Player.Health);
							return true;
						}
					}
				}
				else if (bindingName == "distance")
				{
					if (this.Player == null)
					{
						value = "";
						return true;
					}
					value = this.distanceFormatter.Format(this.distance);
					return true;
				}
			}
			else if (num <= 954351718U)
			{
				if (num != 937574099U)
				{
					if (num == 954351718U)
					{
						if (bindingName == "icon2")
						{
							if (this.Player == null || GameStats.GetBool(EnumGameStats.AutoParty))
							{
								value = "";
								return true;
							}
							if (!this.Player.IsDead() && !this.Player.IsPartyLead())
							{
								value = "";
							}
							else if (this.Player.TwitchActionsEnabled != EntityPlayer.TwitchActionsStates.Enabled)
							{
								value = this.twitchDisabledIcon;
							}
							else if (this.Player.TwitchSafe)
							{
								value = this.twitchSafeIcon;
							}
							else
							{
								value = "";
							}
							return true;
						}
					}
				}
				else if (bindingName == "icon1")
				{
					if (this.Player == null || GameStats.GetBool(EnumGameStats.AutoParty))
					{
						value = "";
						return true;
					}
					if (this.Player.IsDead())
					{
						value = this.deathIcon;
					}
					else if (this.Player.IsPartyLead())
					{
						value = this.leaderIcon;
					}
					else if (this.Player.TwitchActionsEnabled != EntityPlayer.TwitchActionsStates.Enabled)
					{
						value = this.twitchDisabledIcon;
					}
					else if (this.Player.TwitchSafe)
					{
						value = this.twitchSafeIcon;
					}
					else
					{
						value = "";
					}
					return true;
				}
			}
			else if (num != 1339791005U)
			{
				if (num != 1766231739U)
				{
					if (num == 1810125418U)
					{
						if (bindingName == "healthmodifiedmax")
						{
							if (this.Player == null || this.xui.playerUI.entityPlayer.IsDead())
							{
								value = "0";
								return true;
							}
							value = (this.Player.Stats.Health.ModifiedMax / this.Player.Stats.Health.Max).ToCultureInvariantString();
							return true;
						}
					}
				}
				else if (bindingName == "voiceactive")
				{
					value = (this.voiceState == IPartyVoice.EVoiceMemberState.VoiceActive).ToString();
					return true;
				}
			}
			else if (bindingName == "showarrow")
			{
				if (this.Player == null)
				{
					value = "false";
					return true;
				}
				value = this.Player.IsAlive().ToString();
				return true;
			}
		}
		else if (num <= 2535347189U)
		{
			if (num <= 2369371622U)
			{
				if (num != 1926871326U)
				{
					if (num == 2369371622U)
					{
						if (bindingName == "name")
						{
							if (this.Player == null)
							{
								value = "";
								return true;
							}
							value = GameUtils.SafeStringFormat(this.Player.PlayerDisplayName);
							return true;
						}
					}
				}
				else if (bindingName == "healthcolor")
				{
					if (this.Player == null)
					{
						value = "255,255,255,255";
						return true;
					}
					value = (this.Player.TwitchEnabled ? this.twitchHealthColor : this.defaultHealthColor);
					return true;
				}
			}
			else if (num != 2529325253U)
			{
				if (num == 2535347189U)
				{
					if (bindingName == "distancecolor")
					{
						Color32 color2 = Color.white;
						if (this.Player == null)
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
			else if (bindingName == "voicevisible")
			{
				value = (this.voiceState > IPartyVoice.EVoiceMemberState.Disabled).ToString();
				return true;
			}
		}
		else if (num <= 2966955437U)
		{
			if (num != 2916622580U)
			{
				if (num == 2966955437U)
				{
					if (bindingName == "showicon2")
					{
						if (this.Player == null)
						{
							value = "false";
							return true;
						}
						value = ((this.Player.IsPartyLead() || this.Player.IsDead()) && (this.Player.TwitchActionsEnabled != EntityPlayer.TwitchActionsStates.Enabled || this.Player.TwitchSafe) && this.Player.HasTwitchMember()).ToString();
						return true;
					}
				}
			}
			else if (bindingName == "showicon1")
			{
				if (this.Player == null)
				{
					value = "false";
					return true;
				}
				value = (this.Player.IsPartyLead() || this.Player.IsDead() || ((this.Player.TwitchActionsEnabled != EntityPlayer.TwitchActionsStates.Enabled || this.Player.TwitchSafe) && this.Player.HasTwitchMember())).ToString();
				return true;
			}
		}
		else if (num != 3004043574U)
		{
			if (num != 3388762708U)
			{
				if (num == 3910386971U)
				{
					if (bindingName == "partyvisible")
					{
						if (this.Player == null || this.xui.playerUI.entityPlayer.IsDead())
						{
							value = "false";
							return true;
						}
						value = "true";
						return true;
					}
				}
			}
			else if (bindingName == "voicemuted")
			{
				value = (this.voiceState == IPartyVoice.EVoiceMemberState.Muted).ToString();
				return true;
			}
		}
		else if (bindingName == "healthcurrentwithmax")
		{
			if (this.Player == null)
			{
				value = "";
				return true;
			}
			value = this.healthcurrentWMaxFormatter.Format(this.Player.Health, this.Player.GetMaxHealth());
			return true;
		}
		return base.GetBindingValueInternal(ref value, bindingName);
	}

	public XUiC_PartyEntry()
	{
		this.oldTwitch = false;
	}

	public override bool ParseAttribute(string name, string value)
	{
		uint num = <PrivateImplementationDetails>.ComputeStringHash(name);
		if (num <= 1003795756U)
		{
			if (num != 547217411U)
			{
				if (num != 933825086U)
				{
					if (num == 1003795756U)
					{
						if (name == "leader_icon")
						{
							this.leaderIcon = value;
							return true;
						}
					}
				}
				else if (name == "twitch_icon")
				{
					this.twitchActiveIcon = value;
					return true;
				}
			}
			else if (name == "death_icon")
			{
				this.deathIcon = value;
				return true;
			}
		}
		else if (num <= 3319615829U)
		{
			if (num != 2710568369U)
			{
				if (num == 3319615829U)
				{
					if (name == "twitch_health_color")
					{
						this.twitchHealthColor = value;
						return true;
					}
				}
			}
			else if (name == "default_health_color")
			{
				this.defaultHealthColor = value;
				return true;
			}
		}
		else if (num != 3463996801U)
		{
			if (num == 4202079582U)
			{
				if (name == "twitch_safe_icon")
				{
					this.twitchSafeIcon = value;
					return true;
				}
			}
		}
		else if (name == "twitch_disabled_icon")
		{
			this.twitchDisabledIcon = value;
			return true;
		}
		return base.ParseAttribute(name, value);
	}

	public override void OnOpen()
	{
		base.OnOpen();
		this.IsDirty = true;
	}

	public override void OnClose()
	{
		base.OnClose();
		this.SetPlayer(null);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public float ReturnRotation(EntityAlive _self, EntityAlive _other)
	{
		Transform transform = _self.transform;
		Vector3 forward = transform.forward;
		Vector2 vector;
		vector..ctor(forward.x, forward.z);
		Vector3 normalized = (transform.position - _other.transform.position).normalized;
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
	public const float ForcedRefreshTime = 0.5f;

	[PublicizedFrom(EAccessModifier.Private)]
	public float lastHealthValue;

	[PublicizedFrom(EAccessModifier.Private)]
	public float lastStaminaValue;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Sprite arrowContent;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Sprite[] barHealth;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Sprite[] barHealthModifiedMax;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Sprite[] barStamina;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Sprite[] barStaminaModifiedMax;

	[PublicizedFrom(EAccessModifier.Private)]
	public float distance;

	public string defaultHealthColor = "255,0,0,128";

	public string twitchHealthColor = "100,65,165,128";

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Sprite iconSprite1;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Sprite iconSprite2;

	[PublicizedFrom(EAccessModifier.Private)]
	public Color defaultIconColor;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2 iconSpriteSize;

	[PublicizedFrom(EAccessModifier.Private)]
	public Color iconBlinkColor = new Color32(byte.MaxValue, 180, 0, byte.MaxValue);

	[PublicizedFrom(EAccessModifier.Protected)]
	public float updateTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public float arrowRotation;

	[PublicizedFrom(EAccessModifier.Private)]
	public float lastArrowRotation;

	[PublicizedFrom(EAccessModifier.Private)]
	public IPartyVoice.EVoiceMemberState voiceState;

	[PublicizedFrom(EAccessModifier.Private)]
	public float oldValue;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool oldTwitch;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool oldSafe;

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityPlayer.TwitchActionsStates oldTwitchActions;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterInt healthcurrentFormatter = new CachedStringFormatterInt();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatter<int, int> healthcurrentWMaxFormatter = new CachedStringFormatter<int, int>((int _i, int _i2) => string.Format("{0}/{1}", _i, _i2));

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterFloat healthfillFormatter = new CachedStringFormatterFloat(null);

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatter<float> distanceFormatter = new CachedStringFormatter<float>(new Func<float, string>(ValueDisplayFormatters.Distance));

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterXuiRgbaColor itemicontintcolorFormatter = new CachedStringFormatterXuiRgbaColor();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterXuiRgbaColor arrowcolorFormatter = new CachedStringFormatterXuiRgbaColor();

	public string deathIcon = "ui_game_symbol_death";

	public string leaderIcon = "server_favorite";

	public string twitchActiveIcon = "ui_game_symbol_twitch_actions";

	public string twitchDisabledIcon = "ui_game_symbol_twitch_action_disabled";

	public string twitchSafeIcon = "ui_game_symbol_brick";
}
