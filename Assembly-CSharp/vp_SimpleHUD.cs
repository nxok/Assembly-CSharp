using System;
using UnityEngine;

public class vp_SimpleHUD : MonoBehaviour
{
	public float m_HealthWidth
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return this.HealthStyle.CalcSize(new GUIContent(this.FormattedHealth)).x;
		}
	}

	public GUIStyle MessageStyle
	{
		get
		{
			if (vp_SimpleHUD.m_MessageStyle == null)
			{
				vp_SimpleHUD.m_MessageStyle = new GUIStyle("Label");
				vp_SimpleHUD.m_MessageStyle.alignment = 4;
				vp_SimpleHUD.m_MessageStyle.font = this.MessageFont;
			}
			return vp_SimpleHUD.m_MessageStyle;
		}
	}

	public GUIStyle HealthStyle
	{
		get
		{
			if (this.m_HealthStyle == null)
			{
				this.m_HealthStyle = new GUIStyle("Label");
				this.m_HealthStyle.font = this.BigFont;
				this.m_HealthStyle.alignment = 5;
				this.m_HealthStyle.fontSize = 28;
				this.m_HealthStyle.wordWrap = false;
			}
			return this.m_HealthStyle;
		}
	}

	public GUIStyle AmmoStyle
	{
		get
		{
			if (this.m_AmmoStyle == null)
			{
				this.m_AmmoStyle = new GUIStyle("Label");
				this.m_AmmoStyle.font = this.BigFont;
				this.m_AmmoStyle.alignment = 5;
				this.m_AmmoStyle.fontSize = 28;
				this.m_AmmoStyle.wordWrap = false;
			}
			return this.m_AmmoStyle;
		}
	}

	public GUIStyle AmmoStyleSmall
	{
		get
		{
			if (this.m_AmmoStyleSmall == null)
			{
				this.m_AmmoStyleSmall = new GUIStyle("Label");
				this.m_AmmoStyleSmall.font = this.SmallFont;
				this.m_AmmoStyleSmall.alignment = 0;
				this.m_AmmoStyleSmall.fontSize = 15;
				this.m_AmmoStyleSmall.wordWrap = false;
			}
			return this.m_AmmoStyleSmall;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void Awake()
	{
		this.m_Player = base.transform.GetComponent<vp_FPPlayerEventHandler>();
		this.m_Audio = this.m_Player.transform.GetComponent<AudioSource>();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnEnable()
	{
		if (this.m_Player != null)
		{
			this.m_Player.Register(this);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnDisable()
	{
		if (this.m_Player != null)
		{
			this.m_Player.Unregister(this);
		}
	}

	public string FormattedHealth
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			this.m_FormattedHealth = this.m_Player.Health.Get() * this.HealthMultiplier;
			if (this.m_FormattedHealth < 1f)
			{
				this.m_FormattedHealth = (this.m_Player.Dead.Active ? Mathf.Min(this.m_FormattedHealth, 0f) : 1f);
			}
			if (this.m_Player.Dead.Active && this.m_FormattedHealth > 0f)
			{
				this.m_FormattedHealth = 0f;
			}
			return ((int)this.m_FormattedHealth).ToString();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Update()
	{
		this.m_CurrentAmmoOffset = Mathf.SmoothStep(this.m_CurrentAmmoOffset, this.m_TargetAmmoOffset, Time.deltaTime * 10f);
		this.m_CurrentHealthOffset = Mathf.SmoothStep(this.m_CurrentHealthOffset, this.m_TargetHealthOffset, Time.deltaTime * 10f);
		if (this.m_Player.CurrentWeaponIndex.Get() == 0 || this.m_Player.CurrentWeaponType.Get() == 2)
		{
			this.m_TargetAmmoOffset = 200f;
		}
		else
		{
			this.m_TargetAmmoOffset = 10f;
		}
		if (this.m_Player.Dead.Active)
		{
			this.HealthColor = Color.black;
		}
		else if (this.m_Player.Health.Get() < this.HealthLowLevel)
		{
			this.HealthColor = Color.Lerp(Color.white, this.HealthLowColor, vp_MathUtility.Sinus(6f, 0.1f, 0f) * 5f + 0.5f);
			if (this.HealthLowSound != null && Time.time >= this.m_NextAllowedPlayHealthLowSoundTime)
			{
				this.m_NextAllowedPlayHealthLowSoundTime = Time.time + this.HealthLowSoundInterval;
				this.m_Audio.pitch = 1f;
				this.m_Audio.PlayOneShot(this.HealthLowSound);
			}
		}
		else
		{
			this.HealthColor = Color.white;
		}
		if (this.m_Player.CurrentWeaponAmmoCount.Get() < 1 && this.m_Player.CurrentWeaponType.Get() != 3)
		{
			this.AmmoColor = Color.Lerp(Color.white, this.AmmoLowColor, vp_MathUtility.Sinus(8f, 0.1f, 0f) * 5f + 0.5f);
			return;
		}
		this.AmmoColor = Color.white;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnGUI()
	{
		if (!this.ShowHUD)
		{
			return;
		}
		this.DrawHealth();
		this.DrawAmmo();
		this.DrawText();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void DrawHealth()
	{
		this.DrawLabel("", new Vector2(this.m_CurrentHealthOffset, (float)(Screen.height - 68)), new Vector2(80f + this.m_HealthWidth, 52f), this.AmmoStyle, Color.white, this.m_TranspBlack, null);
		if (this.HealthIcon != null)
		{
			this.DrawLabel("", new Vector2(this.m_CurrentHealthOffset + 10f, (float)(Screen.height - 58)), new Vector2(32f, 32f), this.AmmoStyle, Color.white, this.HealthColor, this.HealthIcon);
		}
		this.DrawLabel(this.FormattedHealth, new Vector2(this.m_CurrentHealthOffset - 18f - (45f - this.m_HealthWidth), (float)Screen.height - this.BigFontOffset), new Vector2(110f, 60f), this.HealthStyle, this.HealthColor, Color.clear, null);
		this.DrawLabel("%", new Vector2(this.m_CurrentHealthOffset + 50f + this.m_HealthWidth, (float)Screen.height - this.SmallFontOffset), new Vector2(110f, 60f), this.AmmoStyleSmall, this.HealthColor, Color.clear, null);
		GUI.color = Color.white;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void DrawAmmo()
	{
		if (this.m_Player.CurrentWeaponType.Get() == 3)
		{
			this.DrawLabel("", new Vector2(this.m_CurrentAmmoOffset + (float)Screen.width - 93f - this.AmmoStyle.CalcSize(new GUIContent(this.m_Player.CurrentWeaponAmmoCount.Get().ToString())).x, (float)(Screen.height - 68)), new Vector2(200f, 52f), this.AmmoStyle, this.AmmoColor, this.m_TranspBlack, null);
			if (this.m_Player.CurrentAmmoIcon.Get() != null)
			{
				this.DrawLabel("", new Vector2(this.m_CurrentAmmoOffset + (float)Screen.width - 83f - this.AmmoStyle.CalcSize(new GUIContent(this.m_Player.CurrentWeaponAmmoCount.Get().ToString())).x, (float)(Screen.height - 58)), new Vector2(32f, 32f), this.AmmoStyle, Color.white, this.AmmoColor, this.m_Player.CurrentAmmoIcon.Get());
			}
			this.DrawLabel((this.m_Player.CurrentWeaponAmmoCount.Get() + this.m_Player.CurrentWeaponClipCount.Get()).ToString(), new Vector2(this.m_CurrentAmmoOffset + (float)Screen.width - 145f, (float)Screen.height - this.BigFontOffset), new Vector2(110f, 60f), this.AmmoStyle, this.AmmoColor, Color.clear, null);
			return;
		}
		this.DrawLabel("", new Vector2(this.m_CurrentAmmoOffset + (float)Screen.width - 115f - this.AmmoStyle.CalcSize(new GUIContent(this.m_Player.CurrentWeaponAmmoCount.Get().ToString())).x, (float)(Screen.height - 68)), new Vector2(200f, 52f), this.AmmoStyle, this.AmmoColor, this.m_TranspBlack, null);
		if (this.m_Player.CurrentAmmoIcon.Get() != null)
		{
			this.DrawLabel("", new Vector2(this.m_CurrentAmmoOffset + (float)Screen.width - 105f - this.AmmoStyle.CalcSize(new GUIContent(this.m_Player.CurrentWeaponAmmoCount.Get().ToString())).x, (float)(Screen.height - 58)), new Vector2(32f, 32f), this.AmmoStyle, Color.white, this.AmmoColor, this.m_Player.CurrentAmmoIcon.Get());
		}
		this.DrawLabel(this.m_Player.CurrentWeaponAmmoCount.Get().ToString(), new Vector2(this.m_CurrentAmmoOffset + (float)Screen.width - 177f, (float)Screen.height - this.BigFontOffset), new Vector2(110f, 60f), this.AmmoStyle, this.AmmoColor, Color.clear, null);
		this.DrawLabel("/ " + this.m_Player.CurrentWeaponClipCount.Get().ToString(), new Vector2(this.m_CurrentAmmoOffset + (float)Screen.width - 60f, (float)Screen.height - this.SmallFontOffset), new Vector2(110f, 60f), this.AmmoStyleSmall, this.AmmoColor, Color.clear, null);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void DrawText()
	{
		if (this.m_PickupMessage == null)
		{
			return;
		}
		if (this.m_MessageColor.a < 0.01f)
		{
			return;
		}
		this.m_MessageColor = Color.Lerp(this.m_MessageColor, this.m_InvisibleColor, Time.deltaTime * 0.4f);
		GUI.color = this.m_MessageColor;
		GUI.Box(new Rect(200f, 150f, (float)(Screen.width - 400), (float)(Screen.height - 400)), this.m_PickupMessage, this.MessageStyle);
		GUI.color = Color.white;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnMessage_HUDText(string message)
	{
		this.m_MessageColor = Color.white;
		this.m_PickupMessage = message;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void DrawLabel(string text, Vector2 position, Vector2 scale, GUIStyle textStyle, Color textColor, Color bgColor, Texture texture)
	{
		if (texture == null)
		{
			texture = this.Background;
		}
		if (scale.x == 0f)
		{
			scale.x = textStyle.CalcSize(new GUIContent(text)).x;
		}
		if (scale.y == 0f)
		{
			scale.y = textStyle.CalcSize(new GUIContent(text)).y;
		}
		this.m_DrawLabelRect.x = (this.m_DrawPos.x = position.x);
		this.m_DrawLabelRect.y = (this.m_DrawPos.y = position.y);
		this.m_DrawLabelRect.width = (this.m_DrawSize.x = scale.x);
		this.m_DrawLabelRect.height = (this.m_DrawSize.y = scale.y);
		if (bgColor != Color.clear)
		{
			GUI.color = bgColor;
			if (texture != null)
			{
				GUI.DrawTexture(this.m_DrawLabelRect, texture);
			}
		}
		GUI.color = textColor;
		GUI.Label(this.m_DrawLabelRect, text, textStyle);
		GUI.color = Color.white;
		this.m_DrawPos.x = this.m_DrawPos.x + this.m_DrawSize.x;
		this.m_DrawPos.y = this.m_DrawPos.y + this.m_DrawSize.y;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnStart_SetWeapon()
	{
		this.m_TargetAmmoOffset = 200f;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnStop_SetWeapon()
	{
		this.m_TargetAmmoOffset = 10f;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnStop_Dead()
	{
		this.m_CurrentHealthOffset = -200f;
		this.m_TargetHealthOffset = 0f;
		this.HealthColor = Color.white;
	}

	public bool ShowHUD = true;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public vp_FPPlayerEventHandler m_Player;

	public Font BigFont;

	public Font SmallFont;

	public Font MessageFont;

	public float BigFontOffset = 69f;

	public float SmallFontOffset = 56f;

	public Texture Background;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Vector2 m_DrawPos = Vector2.zero;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Vector2 m_DrawSize = Vector2.zero;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Rect m_DrawLabelRect = new Rect(0f, 0f, 0f, 0f);

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Rect m_DrawShadowRect = new Rect(0f, 0f, 0f, 0f);

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public float m_TargetHealthOffset;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public float m_CurrentHealthOffset;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public float m_TargetAmmoOffset = 200f;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public float m_CurrentAmmoOffset = 200f;

	public Texture2D HealthIcon;

	public float HealthMultiplier = 10f;

	public Color HealthColor = Color.white;

	public float HealthLowLevel = 2.5f;

	public Color HealthLowColor = new Color(0.75f, 0f, 0f, 1f);

	public AudioClip HealthLowSound;

	public float HealthLowSoundInterval = 1f;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public float m_FormattedHealth;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public float m_NextAllowedPlayHealthLowSoundTime;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public AudioSource m_Audio;

	public Color AmmoColor = Color.white;

	public Color AmmoLowColor = new Color(0f, 0f, 0f, 1f);

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public string m_PickupMessage = "";

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Color m_MessageColor = new Color(1f, 1f, 1f, 2f);

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Color m_InvisibleColor = new Color(0f, 0f, 0f, 0f);

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Color m_TranspBlack = new Color(0f, 0f, 0f, 0.5f);

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Color m_TranspWhite = new Color(1f, 1f, 1f, 0.5f);

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public static GUIStyle m_MessageStyle;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public GUIStyle m_HealthStyle;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public GUIStyle m_AmmoStyle;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public GUIStyle m_AmmoStyleSmall;
}
