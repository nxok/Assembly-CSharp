using System;
using Audio;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_VideoPlayer : XUiController
{
	public override void Init()
	{
		base.Init();
		XUiC_VideoPlayer.ID = this.windowGroup.Id;
		this.skipPrompt.IsVisible = false;
	}

	public static void PlayVideo(XUi _xui, VideoData _videoData, bool _skippable, XUiC_VideoPlayer.DelegateOnVideoFinished _videoFinishedCallback = null)
	{
		XUiC_VideoPlayer instance = XUiC_VideoPlayer.GetInstance(_xui);
		_xui.playerUI.windowManager.Open("VideoPlayer", true);
		instance.PlayVideo(_videoData, _skippable, _videoFinishedCallback);
	}

	public static void EndVideo(XUiC_VideoPlayer _videoPlayer)
	{
		_videoPlayer.FinishAndClose(true);
	}

	public static XUiC_VideoPlayer GetInstance(XUi _xui)
	{
		XUiWindowGroup xuiWindowGroup = (XUiWindowGroup)_xui.playerUI.windowManager.GetWindow(XUiC_VideoPlayer.ID);
		if (xuiWindowGroup == null)
		{
			return null;
		}
		XUiController controller = xuiWindowGroup.Controller;
		if (controller == null)
		{
			return null;
		}
		return controller.GetChildByType<XUiC_VideoPlayer>();
	}

	public void PlayVideo(VideoData _videoData, bool _skippable, XUiC_VideoPlayer.DelegateOnVideoFinished _videoFinishedCallback = null)
	{
		this.currentVideo = _videoData;
		this.subtitlesEnabled = GamePrefs.GetBool(EnumGamePrefs.OptionsSubtitlesEnabled);
		this.wasVideoSkipped = false;
		this.skippable = _skippable;
		this.skipPrompt.IsVisible = false;
		if (_videoFinishedCallback != null)
		{
			this.onVideoFinished = (XUiC_VideoPlayer.DelegateOnVideoFinished)Delegate.Combine(this.onVideoFinished, _videoFinishedCallback);
		}
		string bindingXuiMarkupString = this.xui.playerUI.playerInput.GUIActions.Cancel.GetBindingXuiMarkupString(XUiUtils.EmptyBindingStyle.EmptyString, XUiUtils.DisplayStyle.Plain, null);
		this.skipLabel.Text = string.Format(Localization.Get("ui_video_skip", false, null), bindingXuiMarkupString);
		this.previousTimestamp = 0.0;
		this.videoView.VideoUri = _videoData.url;
		XUiC_VideoPlayer.IsVideoPlaying = true;
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (this.subtitlesEnabled && this.videoView.Playing)
		{
			double currentTime = this.videoView.CurrentTime;
			foreach (VideoSubtitle videoSubtitle in this.currentVideo.subtitles)
			{
				if (videoSubtitle.timestamp >= this.previousTimestamp && videoSubtitle.timestamp <= currentTime)
				{
					GameManager.ShowSubtitle(LocalPlayerUI.primaryUI.xui, Manager.GetFormattedSubtitleSpeaker(videoSubtitle.subtitleId), Manager.GetFormattedSubtitle(videoSubtitle.subtitleId), videoSubtitle.duration, true);
					break;
				}
			}
			this.previousTimestamp = currentTime;
		}
		this.backgroundSprite.Color = Color.black;
		if (this.skippable && this.xui.playerUI.playerInput != null)
		{
			if (!this.skipPrompt.IsVisible && this.xui.playerUI.playerInput.AnyGUIActionPressed())
			{
				this.skipPrompt.IsVisible = true;
				this.skipVisibleTime = Time.time;
			}
			else if (this.skipPrompt.IsVisible && this.xui.playerUI.playerInput.GUIActions.Cancel.WasPressed)
			{
				this.FinishAndClose(true);
			}
			if (this.skipPrompt.IsVisible && Time.time - this.skipVisibleTime >= 3f)
			{
				this.skipPrompt.IsVisible = false;
			}
		}
	}

	[XuiBindEvent("VideoError", "videoView")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void OnVideoErrorReceived(XUiV_Video _source)
	{
		this.FinishAndClose(true);
	}

	[XuiBindEvent("VideoFinished", "videoView")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void OnVideoFinished(XUiV_Video _source)
	{
		this.FinishAndClose(false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void FinishAndClose(bool _skipped)
	{
		this.wasVideoSkipped = _skipped;
		this.xui.playerUI.windowManager.Close(XUiC_SubtitlesDisplay.ID);
		this.xui.playerUI.windowManager.Close(this.windowGroup, false);
	}

	public override void OnClose()
	{
		base.OnClose();
		XUiC_VideoPlayer.IsVideoPlaying = false;
		XUiC_VideoPlayer.DelegateOnVideoFinished delegateOnVideoFinished = this.onVideoFinished;
		if (delegateOnVideoFinished == null)
		{
			return;
		}
		delegateOnVideoFinished(this.wasVideoSkipped);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const float SkipVisibleDuration = 3f;

	public static string ID = "";

	[XuiBindComponent(true)]
	[PublicizedFrom(EAccessModifier.Protected)]
	public readonly XUiV_Video videoView;

	[XuiBindComponent("videoBackground", true)]
	[PublicizedFrom(EAccessModifier.Protected)]
	public readonly XUiV_Sprite backgroundSprite;

	[XuiBindComponent("skipPrompt", true)]
	[PublicizedFrom(EAccessModifier.Protected)]
	public readonly XUiView skipPrompt;

	[XuiBindComponent("lblSkip", true)]
	[PublicizedFrom(EAccessModifier.Protected)]
	public readonly XUiV_Label skipLabel;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_VideoPlayer.DelegateOnVideoFinished onVideoFinished;

	[PublicizedFrom(EAccessModifier.Private)]
	public VideoData currentVideo;

	[PublicizedFrom(EAccessModifier.Private)]
	public double previousTimestamp;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool skippable;

	[PublicizedFrom(EAccessModifier.Private)]
	public float skipVisibleTime;

	public static bool IsVideoPlaying;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool wasVideoSkipped;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool subtitlesEnabled;

	public delegate void DelegateOnVideoFinished(bool _skipped);
}
