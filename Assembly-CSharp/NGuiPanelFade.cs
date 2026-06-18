using System;
using UnityEngine;

public class NGuiPanelFade : MonoBehaviour
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public void Awake()
	{
		this.mWidgets = base.GetComponentsInChildren<UIWidget>();
		this.alpha = new float[this.mWidgets.Length];
		for (int i = 0; i < this.mWidgets.Length; i++)
		{
			this.alpha[i] = this.mWidgets[i].color.a;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void Start()
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void OnEnable()
	{
		this.bFadeOut = false;
		this.init();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void OnDisable()
	{
		this.reset();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void reset()
	{
		for (int i = 0; i < this.mWidgets.Length; i++)
		{
			Color color = this.mWidgets[i].color;
			color.a = this.alpha[i];
			this.mWidgets[i].color = color;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void init()
	{
		this.mStart = Time.time;
		this.bFadeIn = this.bFadeInWhenEnabled;
		this.mWidgets = base.GetComponentsInChildren<UIWidget>();
		if (this.alpha.Length != this.mWidgets.Length)
		{
			this.alpha = new float[this.mWidgets.Length];
		}
		for (int i = 0; i < this.mWidgets.Length; i++)
		{
			Color color = this.mWidgets[i].color;
			if (color.a != 0f)
			{
				this.alpha[i] = color.a;
			}
			if (this.bFadeIn)
			{
				color.a = 0f;
				this.mWidgets[i].color = color;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void Update()
	{
		float num;
		if (this.bFadeIn)
		{
			num = ((this.duration > 0f) ? Mathf.Clamp01((Time.time - this.mStart) / this.duration) : 1f);
		}
		else
		{
			if (!this.bFadeOut)
			{
				return;
			}
			num = ((this.duration > 0f) ? (1f - Mathf.Clamp01((Time.realtimeSinceStartup - this.mStart) / this.duration)) : 0f);
		}
		for (int i = 0; i < this.mWidgets.Length; i++)
		{
			Color color = this.mWidgets[i].color;
			color.a = num * this.alpha[i];
			this.mWidgets[i].color = color;
		}
		if (this.bFadeOut && num <= 0.001f)
		{
			this.reset();
			base.gameObject.SetActive(false);
			this.bFadeOut = false;
		}
		if (this.bFadeIn && num >= 1f)
		{
			this.bFadeIn = false;
		}
	}

	public void StartFadeOut()
	{
		this.bFadeOut = true;
		this.mStart = Time.time;
	}

	public float duration = 0.3f;

	public bool bFadeInWhenEnabled = true;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float mStart;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public UIWidget[] mWidgets;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float[] alpha;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool bFadeIn;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool bFadeOut;
}
