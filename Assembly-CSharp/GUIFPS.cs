using System;
using UnityEngine;

public class GUIFPS : MonoBehaviour
{
	public bool Enabled
	{
		get
		{
			return this.bEnabled;
		}
		set
		{
			if (this.bEnabled == value)
			{
				return;
			}
			this.bEnabled = value;
			if (!value && this.guiFpsGraphTexture != null && this.guiFpsGraphTexture.enabled)
			{
				this.guiFpsGraphTexture.enabled = false;
			}
		}
	}

	public bool ShowGraph
	{
		get
		{
			return this.bShowGraph;
		}
		set
		{
			if (this.bShowGraph == value)
			{
				return;
			}
			this.bShowGraph = value;
			if (value && this.guiFpsGraphTexture == null)
			{
				this.initFpsGraph();
			}
			if (this.guiFpsGraphTexture.enabled != this.bShowGraph)
			{
				this.guiFpsGraphTexture.enabled = this.bShowGraph;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void Start()
	{
		this.windowManager = base.GetComponentInParent<GUIWindowManager>();
		GamePrefs.OnGamePrefChanged += this.OnGamePrefChanged;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnDestroy()
	{
		GamePrefs.OnGamePrefChanged -= this.OnGamePrefChanged;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnGamePrefChanged(EnumGamePrefs _obj)
	{
		if (_obj == EnumGamePrefs.OptionsUiFpsScaling)
		{
			this.lastResolution = Vector2i.zero;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void Update()
	{
		if (this.fps.Update())
		{
			this.format = string.Format("{0:F1} FPS", this.fps.Counter);
		}
		if (!this.bEnabled)
		{
			return;
		}
		if (this.bShowGraph)
		{
			this.updateFPSGraph();
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void OnGUI()
	{
		if (!this.Enabled || !this.windowManager.IsHUDEnabled())
		{
			return;
		}
		Vector2i vector2i = new Vector2i(Screen.width, Screen.height);
		if (this.lastResolution != vector2i)
		{
			float num = GamePrefs.GetFloat(EnumGamePrefs.OptionsUiFpsScaling) * 13f;
			this.lastResolution = vector2i;
			this.boxStyle = new GUIStyle(GUI.skin.box);
			int num2;
			if (vector2i.y > 1200)
			{
				num2 = Mathf.RoundToInt((float)vector2i.y / (1200f / num));
			}
			else
			{
				num2 = Mathf.RoundToInt(num);
			}
			this.boxStyle.fontSize = num2;
			this.boxAreaHeight = num2 + 10;
			this.boxAreaWidth = num2 * 7;
		}
		if (this.fps.Counter < 30f)
		{
			GUI.color = Color.yellow;
		}
		else if (this.fps.Counter < 10f)
		{
			GUI.color = Color.red;
		}
		else
		{
			GUI.color = Color.green;
		}
		GUI.Box(new Rect(14f, (float)(Screen.height / 2 + 40), (float)this.boxAreaWidth, (float)this.boxAreaHeight), this.format, this.boxStyle);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnApplicationQuit()
	{
		if (this.texture != null)
		{
			Object.Destroy(this.texture);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void initFpsGraph()
	{
		this.texture = GUIFPS.createGUITexture();
		this.guiFpsGraphTexture = NGUITools.AddMissingComponent<UITexture>(base.gameObject);
		this.guiFpsGraphTexture.mainTexture = this.texture;
		this.guiFpsGraphTexture.enabled = false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static Texture2D createGUITexture()
	{
		Texture2D texture2D = new Texture2D(1024, 256, 4, false);
		for (int i = 0; i < texture2D.height; i++)
		{
			for (int j = 0; j < texture2D.width; j++)
			{
				texture2D.SetPixel(j, i, default(Color));
			}
		}
		texture2D.filterMode = 0;
		return texture2D;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateFPSGraph()
	{
		long totalMemory = GC.GetTotalMemory(false);
		if (totalMemory < this.lastTotalMemory)
		{
			this.gcSpikeCounter = 3;
		}
		this.lastTotalMemory = totalMemory;
		int height = this.texture.height;
		int num = (int)Math.Min((float)height, Time.deltaTime * 2500f);
		float num2 = 1f / Time.deltaTime;
		Color color;
		if (num2 > 20f)
		{
			if (num2 <= 40f)
			{
				color = new Color(1f, 1f, 0f, 0.5f);
			}
			else
			{
				color = new Color(0f, 1f, 0f, 0.5f);
			}
		}
		else if (num2 <= 10f)
		{
			color = new Color(1f, 0f, 0f, 0.5f);
		}
		else
		{
			color = new Color(1f, 0.5f, 0f, 0.5f);
		}
		Color color2 = color;
		int num3 = this.gcSpikeCounter;
		this.gcSpikeCounter = num3 - 1;
		if (num3 > 0)
		{
			color2 = Color.magenta;
		}
		for (int i = 0; i <= num; i++)
		{
			this.texture.SetPixel(this.curGraphXPos, i, color2);
		}
		for (int j = num + 1; j < height; j++)
		{
			this.texture.SetPixel(this.curGraphXPos, j, new Color(0f, 0f, 0f, 0f));
		}
		for (int k = 0; k < height; k++)
		{
			this.texture.SetPixel(this.curGraphXPos + 1, k, new Color(0f, 0f, 0f, 0f));
		}
		for (int l = 10; l <= 60; l += 10)
		{
			this.texture.SetPixel(this.curGraphXPos, (int)(2500f / (float)l), new Color(1f, 1f, 1f, 0.5f));
			this.texture.SetPixel(this.curGraphXPos, (int)(2500f / (float)l) - 1, new Color(1f, 1f, 1f, 0.5f));
		}
		this.texture.Apply(false);
		this.curGraphXPos++;
		this.curGraphXPos %= this.texture.width - 1;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool bEnabled;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public FPS fps = new FPS(0.5f);

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public string format;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const int BaseTextSize = 13;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool bShowGraph;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Texture2D texture;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int curGraphXPos;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public UITexture guiFpsGraphTexture;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public long lastTotalMemory;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int gcSpikeCounter;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const float cBarHeight = 2500f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector2i lastResolution;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public GUIStyle boxStyle;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int boxAreaHeight;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int boxAreaWidth;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public GUIWindowManager windowManager;
}
