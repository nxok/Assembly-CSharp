using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_SubtitlesDisplay : XUiController
{
	public static bool IsDisplaying
	{
		get
		{
			return XUiC_SubtitlesDisplay._isDisplaying;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			XUiC_SubtitlesDisplay._isDisplaying = value;
			GameManager instance = GameManager.Instance;
			EntityPlayerLocal primaryPlayer = GameManager.Instance.World.GetPrimaryPlayer();
			instance.SetToolTipPause((primaryPlayer != null) ? primaryPlayer.PlayerUI.nguiWindowManager : null, value);
		}
	}

	public override void Init()
	{
		base.Init();
		XUiC_SubtitlesDisplay.ID = this.windowGroup.Id;
		this.speakerLabel = (XUiV_Label)base.GetChildById("lblSpeaker").ViewComponent;
		this.subtitlesLabel = (XUiV_Label)base.GetChildById("lblSubtitle").ViewComponent;
		this.subtitlesLabel.MaxLineCount = 2;
		this.subtitlesLabel.Overflow = 0;
	}

	public static void DisplaySubtitle(LocalPlayerUI ui, string speakerText, string contentText, float duration = 3f, bool centerAlign = false)
	{
		XUiC_SubtitlesDisplay instance = XUiC_SubtitlesDisplay.GetInstance(ui.xui);
		if (instance != null)
		{
			ui.windowManager.Open("SubtitlesDisplay", false);
			instance.SetSubtitle(speakerText, contentText, Mathf.Max(duration, 3f) + 1f, centerAlign);
			if (GameManager.Instance.World != null)
			{
				EntityPlayerLocal primaryPlayer = GameManager.Instance.World.GetPrimaryPlayer();
				NGUIWindowManager nguiwindowManager = ((primaryPlayer != null) ? primaryPlayer.PlayerUI.nguiWindowManager : null);
				if (nguiwindowManager != null)
				{
					GameManager.Instance.ClearCurrentTooltip(nguiwindowManager);
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static XUiC_SubtitlesDisplay GetInstance(XUi _xui)
	{
		XUiWindowGroup xuiWindowGroup = (XUiWindowGroup)_xui.playerUI.windowManager.GetWindow(XUiC_SubtitlesDisplay.ID);
		if (xuiWindowGroup == null)
		{
			return null;
		}
		XUiController controller = xuiWindowGroup.Controller;
		if (controller == null)
		{
			return null;
		}
		return controller.GetChildByType<XUiC_SubtitlesDisplay>();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetSubtitle(string speaker, string content, float duration, bool centerAlign)
	{
		this.speakerLabel.Text = speaker;
		this.subtitlesLabel.Text = content;
		this.alignment = (centerAlign ? 2 : 1);
		this.subtitlesLabel.Alignment = this.alignment;
		this.openTime = Time.time;
		this.duration = duration;
		XUiC_SubtitlesDisplay.IsDisplaying = true;
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		this.subtitlesLabel.Alignment = this.alignment;
		this.subtitlesLabel.Overflow = 0;
		this.subtitlesLabel.Size = new Vector2i(610, this.subtitlesLabel.Size.y);
		if (Time.time - this.openTime >= this.duration)
		{
			this.xui.playerUI.windowManager.Close("SubtitlesDisplay");
			XUiC_SubtitlesDisplay.IsDisplaying = false;
		}
	}

	public override void OnClose()
	{
		base.OnClose();
		XUiC_SubtitlesDisplay.IsDisplaying = false;
	}

	public static string ID = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label speakerLabel;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label subtitlesLabel;

	[PublicizedFrom(EAccessModifier.Private)]
	public float openTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public float duration;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float minDuration = 3f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float durationAdd = 1f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int labelPadding = 28;

	[PublicizedFrom(EAccessModifier.Private)]
	public int targetHeight = 64;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool pendingUpdate;

	[PublicizedFrom(EAccessModifier.Private)]
	public string pendingText = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public NGUIText.Alignment alignment = 1;

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool _isDisplaying;
}
