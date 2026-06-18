using System;
using System.Collections.Generic;
using UnityEngine;

public class vp_PainHUD : MonoBehaviour
{
	[PublicizedFrom(EAccessModifier.Private)]
	public void Awake()
	{
		this.m_Player = base.transform.GetComponent<vp_FPPlayerEventHandler>();
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

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnGUI()
	{
		this.UpdatePainFlash();
		this.UpdateInflictorArrows();
		this.UpdateDeathTexture();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdatePainFlash()
	{
		if (this.m_PainColor.a < 0.01f)
		{
			this.m_PainColor.a = 0f;
			return;
		}
		this.m_PainColor = Color.Lerp(this.m_PainColor, this.m_FlashInvisibleColor, Time.deltaTime * 0.4f);
		GUI.color = this.m_PainColor;
		if (this.PainTexture != null)
		{
			GUI.DrawTexture(new Rect(0f, 0f, (float)Screen.width, (float)Screen.height), this.PainTexture);
		}
		GUI.color = Color.white;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateInflictorArrows()
	{
		if (this.ArrowTexture == null)
		{
			return;
		}
		for (int i = this.m_Inflictors.Count - 1; i > -1; i--)
		{
			if (this.m_Inflictors[i] == null || this.m_Inflictors[i].Transform == null || !vp_Utility.IsActive(this.m_Inflictors[i].Transform.gameObject))
			{
				this.m_Inflictors.Remove(this.m_Inflictors[i]);
			}
			else
			{
				this.m_ArrowColor.a = (this.ArrowVisibleDuration - (Time.time - this.m_Inflictors[i].DamageTime)) / this.ArrowVisibleDuration;
				if (this.m_ArrowColor.a >= 0f)
				{
					Vector2 vector;
					vector..ctor((float)Screen.width * 0.5f, (float)Screen.height * 0.5f);
					float num = vp_3DUtility.LookAtAngleHorizontal(base.transform.position, base.transform.forward, this.m_Inflictors[i].Transform.position) + this.ArrowAngleOffset;
					float num2 = (float)Screen.width * this.ArrowScale;
					float num3 = (this.ArrowShakeDuration - (Time.time - this.m_LastInflictorTime)) / this.ArrowShakeDuration;
					num3 = Mathf.Lerp(0f, 1f, num3);
					num2 += (float)(Screen.width / 100) * num3;
					Matrix4x4 matrix = GUI.matrix;
					GUIUtility.RotateAroundPivot(num, vector);
					GUI.color = this.m_ArrowColor;
					GUI.DrawTexture(new Rect(vector.x, vector.y, num2, num2), this.ArrowTexture);
					GUI.matrix = matrix;
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateDeathTexture()
	{
		if (this.DeathTexture == null)
		{
			return;
		}
		if (!this.m_Player.Dead.Active)
		{
			return;
		}
		GUI.color = this.m_SplatColor;
		GUI.DrawTexture(this.m_SplatRect, this.DeathTexture);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnMessage_HUDDamageFlash(vp_DamageInfo damageInfo)
	{
		if (damageInfo == null || damageInfo.Damage == 0f)
		{
			this.m_PainColor.a = 0f;
			return;
		}
		this.m_PainColor.a = this.m_PainColor.a + damageInfo.Damage * this.PainIntensity;
		if (damageInfo.Source != null)
		{
			this.m_LastInflictorTime = Time.time;
			bool flag = true;
			foreach (vp_PainHUD.Inflictor inflictor in this.m_Inflictors)
			{
				if (inflictor.Transform == damageInfo.Source.transform)
				{
					inflictor.DamageTime = Time.time;
					flag = false;
				}
			}
			if (flag)
			{
				this.m_Inflictors.Add(new vp_PainHUD.Inflictor(damageInfo.Source, Time.time));
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnStart_Dead()
	{
		float num = Random.value * 0.6f + 0.4f;
		this.m_SplatColor = new Color(num, num, num, 1f);
		float num2 = (float)((Random.value < 0.5f) ? (Screen.width / Random.Range(5, 10)) : (Screen.width / Random.Range(4, 7)));
		this.m_SplatRect = new Rect(Random.Range(-num2, 0f), Random.Range(-num2, 0f), (float)Screen.width + num2, (float)Screen.height + num2);
		if (Random.value < 0.5f)
		{
			this.m_SplatRect.x = (float)Screen.width - this.m_SplatRect.x;
			this.m_SplatRect.width = -this.m_SplatRect.width;
		}
		if (Random.value < 0.125f)
		{
			num *= 0.5f;
			this.m_SplatColor = new Color(num, num, num, 1f);
			this.m_SplatRect.y = (float)Screen.height - this.m_SplatRect.y;
			this.m_SplatRect.height = -this.m_SplatRect.height;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnStop_Dead()
	{
		this.m_PainColor.a = 0f;
		for (int i = this.m_Inflictors.Count - 1; i > -1; i--)
		{
			this.m_Inflictors[i].DamageTime = 0f;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public List<vp_PainHUD.Inflictor> m_Inflictors = new List<vp_PainHUD.Inflictor>();

	public Texture PainTexture;

	public Texture DeathTexture;

	public Texture ArrowTexture;

	public float PainIntensity = 0.2f;

	[Range(0.01f, 0.5f)]
	public float ArrowScale = 0.083f;

	public float ArrowAngleOffset = -135f;

	public float ArrowVisibleDuration = 1.5f;

	public float ArrowShakeDuration = 0.125f;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public float m_LastInflictorTime;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Color m_PainColor = new Color(0.8f, 0f, 0f, 0f);

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Color m_ArrowColor = new Color(0.8f, 0f, 0f, 0f);

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Color m_FlashInvisibleColor = new Color(1f, 0f, 0f, 0f);

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Color m_SplatColor = Color.white;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Rect m_SplatRect;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public vp_FPPlayerEventHandler m_Player;

	[PublicizedFrom(EAccessModifier.Protected)]
	public class Inflictor
	{
		public Inflictor(Transform transform, float damageTime)
		{
			this.Transform = transform;
			this.DamageTime = damageTime;
		}

		public Transform Transform;

		public float DamageTime;
	}
}
