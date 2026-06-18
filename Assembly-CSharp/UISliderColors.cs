using System;
using UnityEngine;

[AddComponentMenu("NGUI/Examples/Slider Colors")]
public class UISliderColors : MonoBehaviour
{
	[PublicizedFrom(EAccessModifier.Private)]
	public void Start()
	{
		this.mBar = base.GetComponent<UIProgressBar>();
		this.mSprite = base.GetComponent<UIBasicSprite>();
		this.Update();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Update()
	{
		if (this.sprite == null || this.colors.Length == 0)
		{
			return;
		}
		float num = ((this.mBar != null) ? this.mBar.value : this.mSprite.fillAmount);
		num *= (float)(this.colors.Length - 1);
		int num2 = Mathf.FloorToInt(num);
		Color color = this.colors[0];
		if (num2 >= 0)
		{
			if (num2 + 1 < this.colors.Length)
			{
				float num3 = num - (float)num2;
				color = Color.Lerp(this.colors[num2], this.colors[num2 + 1], num3);
			}
			else if (num2 < this.colors.Length)
			{
				color = this.colors[num2];
			}
			else
			{
				color = this.colors[this.colors.Length - 1];
			}
		}
		color.a = this.sprite.color.a;
		this.sprite.color = color;
	}

	public UISprite sprite;

	public Color[] colors = new Color[]
	{
		Color.red,
		Color.yellow,
		Color.green
	};

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public UIProgressBar mBar;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public UIBasicSprite mSprite;
}
