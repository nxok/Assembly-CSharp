using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class vp_DemoManager
{
	public vp_DemoManager()
	{
		this.DesktopResolution = Screen.currentResolution;
		this.LastInputTime = Time.time;
		this.m_FullScreenFadeTexture = new Texture2D(1, 1, 3, false);
	}

	public virtual void Update()
	{
		if (double.IsNaN((double)Camera.main.fieldOfView))
		{
			Camera.main.fieldOfView = 60f;
		}
		if (Input.GetKeyDown(117))
		{
			Screen.fullScreen = !Screen.fullScreen;
		}
		if (Screen.fullScreen && Screen.currentResolution.width != this.DesktopResolution.width)
		{
			Screen.SetResolution(this.DesktopResolution.width, this.DesktopResolution.height, true);
		}
		if (Input.GetKeyDown(108))
		{
			this.m_SimulateLowFPS = !this.m_SimulateLowFPS;
		}
		if (this.m_SimulateLowFPS)
		{
			for (int i = 0; i < 20000000; i++)
			{
			}
		}
	}

	public bool ButtonToggle(Rect rect, string label, bool state, bool arrow, Texture imageUpPointer)
	{
		if (!this.ShowGUI)
		{
			return false;
		}
		GUIStyle guistyle = this.UpStyle;
		GUIStyle guistyle2 = this.DownStyle;
		float num = 0f;
		if (state)
		{
			guistyle = this.DownStyle;
			guistyle2 = this.UpStyle;
			num = rect.width * 0.5f + 2f;
		}
		GUI.Label(new Rect(rect.x, rect.y - 30f, rect.width, rect.height), label, this.CenterStyle);
		if (GUI.Button(new Rect(rect.x, rect.y, rect.width * 0.5f - 2f, rect.height), "OFF", guistyle2))
		{
			state = false;
		}
		if (GUI.Button(new Rect(rect.x + rect.width * 0.5f + 2f, rect.y, rect.width * 0.5f, rect.height), "ON", guistyle))
		{
			state = true;
		}
		if (arrow)
		{
			GUI.Label(new Rect(rect.x + rect.width * 0.5f * 0.5f - 14f + num, rect.y + rect.height, 32f, 32f), imageUpPointer);
		}
		return state;
	}

	public void DrawBoxes(string caption, string description, Texture imageLeftArrow, Texture imageRightArrow, vp_DemoManager.LoadLevelCallback nextLevelCallback = null, vp_DemoManager.LoadLevelCallback prevLevelCallback = null, bool drawBox = true)
	{
		if (!this.ShowGUI)
		{
			return;
		}
		GUI.color = new Color(1f, 1f, 1f, 1f * this.GlobalAlpha);
		GUILayout.BeginArea(new Rect((float)(Screen.width / 2) - 400f, 30f, 800f, 100f));
		if (imageLeftArrow != null)
		{
			GUI.Box(new Rect(30f, 10f, 80f, 80f), "");
		}
		if (drawBox)
		{
			GUI.Box(new Rect(120f, 0f, 560f, 100f), "");
		}
		GUI.color = new Color(1f, 1f, 1f, this.m_TextAlpha * this.GlobalAlpha);
		for (int i = 0; i < 3; i++)
		{
			GUILayout.BeginArea(new Rect(130f, 10f, 540f, 80f));
			GUILayout.Label("--- " + caption.ToUpper() + " ---\n" + description, this.LabelStyle, Array.Empty<GUILayoutOption>());
			GUILayout.EndArea();
		}
		GUI.color = new Color(1f, 1f, 1f, 1f * this.GlobalAlpha);
		if (imageRightArrow != null)
		{
			GUI.Box(new Rect(690f, 10f, 80f, 80f), "");
		}
		if (imageLeftArrow != null && GUI.Button(new Rect(35f, 15f, 80f, 80f), imageLeftArrow, "Label"))
		{
			if (prevLevelCallback == null)
			{
				this.m_FadeToScreen = Mathf.Max(this.CurrentScreen - 1, 1);
				this.m_FadeState = vp_DemoManager.FadeState.FadeOut;
			}
			else
			{
				prevLevelCallback();
			}
		}
		if (Time.time < this.LastInputTime + 30f)
		{
			this.m_BigArrowFadeAlpha = 1f;
		}
		else
		{
			this.m_BigArrowFadeAlpha = 0.5f - Mathf.Sin((Time.time - 0.5f) * 6f) * 1f;
		}
		GUI.color = new Color(1f, 1f, 1f, this.m_BigArrowFadeAlpha * this.GlobalAlpha);
		if (imageRightArrow != null && GUI.Button(new Rect(700f, 15f, 80f, 80f), imageRightArrow, "Label"))
		{
			if (nextLevelCallback == null)
			{
				this.m_FadeToScreen = this.CurrentScreen + 1;
				this.m_FadeState = vp_DemoManager.FadeState.FadeOut;
			}
			else
			{
				nextLevelCallback();
			}
		}
		GUI.color = new Color(1f, 1f, 1f, 1f * this.GlobalAlpha);
		GUILayout.EndArea();
		GUI.color = new Color(1f, 1f, 1f, this.m_TextAlpha * this.GlobalAlpha);
	}

	public int ToggleColumn(int width, int y, int sel, string[] strings, bool center, bool arrow, Texture imageRightPointer, Texture imageLeftPointer)
	{
		if (!this.ShowGUI)
		{
			return 0;
		}
		float num = (float)(strings.Length * 30);
		Vector2 vector;
		vector..ctor((float)(Screen.width / 2), (float)(Screen.height / 2));
		Rect rect;
		if (center)
		{
			rect..ctor(vector.x - (float)width, (float)y, (float)width, 30f);
		}
		else
		{
			rect..ctor((float)(Screen.width - width - 10), vector.y - num / 2f, (float)width, 30f);
		}
		int num2 = 0;
		foreach (string text in strings)
		{
			if (center)
			{
				rect.x = vector.x - (float)(width / 2);
			}
			else
			{
				rect.x = 10f;
			}
			rect.width = (float)width;
			GUIStyle guistyle = this.UpStyle;
			if (num2 == sel)
			{
				Color color = GUI.color;
				GUI.color = new Color(1f, 1f, 1f, 1f * this.GlobalAlpha);
				guistyle = this.DownStyle;
				if (center)
				{
					rect.x = vector.x - (float)(width / 2) + 10f;
				}
				else
				{
					rect.x = 20f;
				}
				rect.width = (float)(width - 20);
				if (arrow && !this.ClosingDown)
				{
					if (center)
					{
						GUI.Label(new Rect(rect.x - 27f, rect.y, 32f, 32f), imageRightPointer);
					}
					else
					{
						GUI.Label(new Rect(rect.x + rect.width + 5f, rect.y, 32f, 32f), imageLeftPointer);
					}
				}
				GUI.color = color;
			}
			if (GUI.Button(rect, text, guistyle))
			{
				sel = num2;
			}
			rect.y += 35f;
			num2++;
		}
		return sel;
	}

	public int ButtonColumn(int y, int sel, string[] strings, Texture imagePointer)
	{
		if (!this.ShowGUI)
		{
			return 0;
		}
		float num = (float)(Screen.width / 2);
		Rect rect;
		rect..ctor(num - 100f, (float)y, 200f, 30f);
		int num2 = 0;
		foreach (string text in strings)
		{
			rect.x = num - 100f;
			rect.width = 200f;
			if (GUI.Button(rect, text))
			{
				sel = num2;
				this.ButtonColumnClickTime = Time.time;
				this.ButtonColumnArrowY = rect.y;
			}
			rect.y += 35f;
			num2++;
		}
		if (Time.time < this.ButtonColumnArrowFadeoutTime)
		{
			this.ButtonColumnClickTime = Time.time;
		}
		GUI.color = new Color(1f, 1f, 1f, Mathf.Max(0f, 1f - (Time.time - this.ButtonColumnClickTime) * 1f * this.GlobalAlpha));
		GUI.Label(new Rect(rect.x - 27f, this.ButtonColumnArrowY, 32f, 32f), imagePointer);
		GUI.color = new Color(1f, 1f, 1f, 1f * this.GlobalAlpha);
		return sel;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void Reset()
	{
		this.ButtonSelection = 0;
		this.FirstFrame = true;
		this.LastInputTime = Time.time;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void InitGUIStyles()
	{
		this.LabelStyle = new GUIStyle("Label");
		this.LabelStyle.alignment = 7;
		this.UpStyle = new GUIStyle("Button");
		this.DownStyle = new GUIStyle("Button");
		this.DownStyle.normal = this.DownStyle.active;
		this.CenterStyle = new GUIStyle("Label");
		this.CenterStyle.alignment = 4;
		this.m_StylesInitialized = true;
	}

	public void DrawImage(Texture image, float xOffset, float yOffset)
	{
		if (!this.ShowGUI)
		{
			return;
		}
		if (image == null)
		{
			return;
		}
		float num = (float)(Screen.width / 2);
		float num2 = (float)Mathf.Min(image.width, Screen.width);
		float num3 = (float)image.height / (float)image.width;
		GUI.DrawTexture(new Rect(num - num2 / 2f + xOffset, 140f + yOffset, num2, num2 * num3), image);
	}

	public void DrawImage(Texture image)
	{
		this.DrawImage(image, 0f, 0f);
	}

	public void DrawEditorPreview(Texture section, Texture imageEditorPreview, Texture imageEditorScreenshot)
	{
		if (!this.ShowGUI)
		{
			return;
		}
		Color color = GUI.color;
		Vector2 vector;
		vector..ctor(Input.mousePosition.x, (float)Screen.height - Input.mousePosition.y);
		float num = 0f;
		if (this.EditorPreviewSectionExpanded)
		{
			float num2 = (float)(Screen.height - section.height - imageEditorPreview.height);
			float num3 = (float)(Screen.height - section.height);
			GUI.DrawTexture(new Rect(num, num2, (float)imageEditorPreview.width, (float)imageEditorPreview.height), imageEditorPreview);
			GUI.DrawTexture(new Rect(num, num3, (float)section.width, (float)section.height), section);
			if (vector.x > num && vector.x < num + (float)section.width && vector.y > num2 && vector.y < (float)(Screen.height - imageEditorPreview.height))
			{
				this.m_EditorPreviewScreenshotTextAlpha = Mathf.Min(1f, this.m_EditorPreviewScreenshotTextAlpha + 0.01f);
				if (Input.GetMouseButtonDown(0))
				{
					this.EditorPreviewSectionExpanded = false;
				}
			}
			else
			{
				this.m_EditorPreviewScreenshotTextAlpha = Mathf.Max(0f, this.m_EditorPreviewScreenshotTextAlpha - 0.03f);
			}
			GUI.color = new Color(1f, 1f, 1f, color.a * 0.5f * this.m_EditorPreviewScreenshotTextAlpha * this.GlobalAlpha);
			GUI.DrawTexture(new Rect(num + 48f, num3 + (float)(section.height / 2) - (float)(imageEditorScreenshot.height / 2), (float)imageEditorScreenshot.width, (float)imageEditorScreenshot.height), imageEditorScreenshot);
		}
		else
		{
			float num4 = (float)(Screen.height - imageEditorPreview.height);
			GUI.DrawTexture(new Rect(num, num4, (float)imageEditorPreview.width, (float)imageEditorPreview.height), imageEditorPreview);
			if (vector.x > num && vector.x < num + (float)section.width && vector.y > num4 && Input.GetMouseButtonUp(0))
			{
				this.EditorPreviewSectionExpanded = true;
			}
		}
		GUI.color = color;
	}

	public void DrawFullScreenText(Texture imageFullScreen)
	{
		if (!this.ShowGUI)
		{
			return;
		}
		if (Time.realtimeSinceStartup > 5f)
		{
			return;
		}
		if (Time.realtimeSinceStartup > 3f)
		{
			this.m_FullScreenTextAlpha -= this.m_FadeSpeed * Time.deltaTime * 15f;
		}
		GUI.color = new Color(1f, 1f, 1f, this.m_FullScreenTextAlpha * this.GlobalAlpha);
		GUI.DrawTexture(new Rect((float)(Screen.width / 2 - 120), (float)(Screen.height / 2 - 16), 240f, 32f), imageFullScreen);
		GUI.color = new Color(1f, 1f, 1f, 1f * this.GlobalAlpha);
	}

	public void DoScreenTransition()
	{
		if (!this.ShowGUI)
		{
			return;
		}
		if (this.m_FadeState == vp_DemoManager.FadeState.FadeOut)
		{
			this.m_TextAlpha -= this.m_FadeSpeed;
			if (this.m_TextAlpha <= 0f)
			{
				this.m_TextAlpha = 0f;
				this.Reset();
				this.CurrentScreen = this.m_FadeToScreen;
				this.m_FadeState = vp_DemoManager.FadeState.FadeIn;
				return;
			}
		}
		else if (this.m_FadeState == vp_DemoManager.FadeState.FadeIn && !this.ClosingDown)
		{
			this.m_TextAlpha += this.m_FadeSpeed;
			if (this.m_TextAlpha >= 1f)
			{
				this.m_TextAlpha = 1f;
				this.m_FadeState = vp_DemoManager.FadeState.None;
			}
		}
	}

	public void SetScreen(int screen)
	{
		this.m_FadeToScreen = screen;
		this.m_FadeState = vp_DemoManager.FadeState.FadeOut;
	}

	public void OnGUI()
	{
		if (!this.m_StylesInitialized)
		{
			this.InitGUIStyles();
		}
		this.DoScreenTransition();
		if (vp_Utility.LockCursor && this.FadeGUIOnCursorLock)
		{
			this.GlobalAlpha = 0.35f;
		}
		else if (!this.ClosingDown)
		{
			this.GlobalAlpha = 1f;
		}
		if (Time.time - this.CurrentFullScreenFadeTime < this.m_FullScreenFadeInDuration)
		{
			this.GlobalAlpha = Time.time - this.CurrentFullScreenFadeTime;
			GUI.color = new Color(0f, 0f, 0f, (this.m_FullScreenFadeInDuration - this.GlobalAlpha) / this.m_FullScreenFadeInDuration);
			GUI.DrawTexture(new Rect(0f, 0f, (float)Screen.width, (float)Screen.height), this.m_FullScreenFadeTexture);
			GUI.color = Color.white;
		}
		if (this.CurrentFullScreenFadeTime > Time.time)
		{
			this.GlobalAlpha = this.CurrentFullScreenFadeTime - Time.time;
			GUI.color = new Color(0f, 0f, 0f, (this.m_FullScreenFadeOutDuration - this.GlobalAlpha) / this.m_FullScreenFadeOutDuration);
			GUI.DrawTexture(new Rect(0f, 0f, (float)Screen.width, (float)Screen.height), this.m_FullScreenFadeTexture);
			GUI.color = Color.white;
		}
	}

	public void LoadLevel(int level)
	{
		this.ClosingDown = true;
		vp_Timer.CancelAll();
		vp_TimeUtility.TimeScale = 1f;
		this.m_FadeState = vp_DemoManager.FadeState.FadeOut;
		this.CurrentFullScreenFadeTime = Time.time + this.m_FullScreenFadeOutDuration;
		vp_Timer.In(this.m_FullScreenFadeOutDuration, delegate
		{
			SceneManager.LoadScene(level);
		}, null);
	}

	public GUIStyle UpStyle;

	public GUIStyle LabelStyle;

	public GUIStyle DownStyle;

	public GUIStyle CenterStyle;

	public int CurrentScreen = 1;

	public Resolution DesktopResolution;

	public bool FirstFrame = true;

	public bool EditorPreviewSectionExpanded = true;

	public bool ShowGUI = true;

	public float ButtonColumnClickTime;

	public float ButtonColumnArrowY = -100f;

	public float ButtonColumnArrowFadeoutTime;

	public int ButtonSelection;

	public float LastInputTime;

	public bool FadeGUIOnCursorLock = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public float m_FadeSpeed = 0.03f;

	[PublicizedFrom(EAccessModifier.Private)]
	public int m_FadeToScreen;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool m_StylesInitialized;

	[PublicizedFrom(EAccessModifier.Private)]
	public vp_DemoManager.FadeState m_FadeState;

	[PublicizedFrom(EAccessModifier.Private)]
	public Texture2D m_FullScreenFadeTexture;

	[PublicizedFrom(EAccessModifier.Private)]
	public float m_FullScreenFadeOutDuration = 0.5f;

	[PublicizedFrom(EAccessModifier.Private)]
	public float m_FullScreenFadeInDuration = 0.75f;

	public float CurrentFullScreenFadeTime;

	public bool ClosingDown;

	public float GlobalAlpha = 0.35f;

	[PublicizedFrom(EAccessModifier.Private)]
	public float m_TextAlpha = 1f;

	[PublicizedFrom(EAccessModifier.Private)]
	public float m_EditorPreviewScreenshotTextAlpha;

	[PublicizedFrom(EAccessModifier.Private)]
	public float m_FullScreenTextAlpha = 1f;

	[PublicizedFrom(EAccessModifier.Private)]
	public float m_BigArrowFadeAlpha = 1f;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool m_SimulateLowFPS;

	[PublicizedFrom(EAccessModifier.Private)]
	public enum FadeState
	{
		None,
		FadeOut,
		FadeIn
	}

	public delegate void LoadLevelCallback();
}
