using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Video;

public class XUiV_Video : XUiV_TextureBased
{
	public event XUiV_Video.VideoErrorDelegate VideoError;

	public event XUiV_Video.VideoFinishedDelegate VideoFinished;

	[XuiXmlAttribute("loop", false)]
	public bool Loop
	{
		get
		{
			return this.loop;
		}
		set
		{
			if (this.loop == value)
			{
				return;
			}
			this.loop = value;
			base.SetDirty();
		}
	}

	[XuiXmlAttribute("uri", false)]
	public string VideoUri
	{
		get
		{
			return this.videoUri;
		}
		set
		{
			if (this.videoUri == value)
			{
				return;
			}
			this.videoUri = value;
			this.videoUriChanged = true;
			base.SetDirty();
		}
	}

	[XuiXmlAttribute("videoaspect", false)]
	public VideoAspectRatio VideoAspect
	{
		get
		{
			return this.videoAspect;
		}
		set
		{
			if (this.videoAspect == value)
			{
				return;
			}
			this.videoAspect = value;
			base.SetDirty();
		}
	}

	public bool Playing
	{
		get
		{
			return this.videoPlayer.isPlaying;
		}
		set
		{
			if (this.videoPlayer.isPlaying == value)
			{
				return;
			}
			if (value)
			{
				this.videoPlayer.Play();
				return;
			}
			this.videoPlayer.Stop();
		}
	}

	public EnumGamePrefs VolumeSetting
	{
		get
		{
			return this.volumeSetting;
		}
		set
		{
			if (this.volumeSetting == value)
			{
				return;
			}
			this.volumeSetting = value;
			base.SetDirty();
		}
	}

	public double CurrentTime
	{
		get
		{
			return this.videoPlayer.time;
		}
		set
		{
			this.videoPlayer.time = value;
		}
	}

	public XUiV_Video(XUi _xui, string _id)
		: base(_xui, _id)
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void createComponents(GameObject _go)
	{
		base.createComponents(_go);
		_go.AddComponent<VideoPlayer>();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void captureComponents()
	{
		base.captureComponents();
		this.videoPlayer = this.uiTransform.gameObject.GetComponent<VideoPlayer>();
	}

	public override void InitView()
	{
		base.InitView();
		this.videoPlayer.playOnAwake = false;
		this.videoPlayer.renderMode = 2;
		this.videoPlayer.prepareCompleted += new VideoPlayer.EventHandler(this.OnVideoPrepared);
		this.videoPlayer.loopPointReached += new VideoPlayer.EventHandler(this.OnVideoFinished);
		this.videoPlayer.errorReceived += new VideoPlayer.ErrorEventHandler(this.OnVideoErrorReceived);
		GamePrefs.OnGamePrefChanged += this.OnGamePrefChanged;
	}

	public override void Cleanup()
	{
		base.Cleanup();
		GamePrefs.OnGamePrefChanged -= this.OnGamePrefChanged;
		this.destroyRenderTexture();
	}

	public override void OnOpen()
	{
		base.OnOpen();
		ThreadManager.StartCoroutine(this.startVideoEndOfFrame());
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator startVideoEndOfFrame()
	{
		yield return new WaitForEndOfFrame();
		this.startVideo();
		yield break;
	}

	public override void OnClose()
	{
		base.OnClose();
		this.destroyRenderTexture();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnGamePrefChanged(EnumGamePrefs _pref)
	{
		if (_pref != this.VolumeSetting)
		{
			return;
		}
		this.updateVolumeLevel();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void startVideo()
	{
		if (string.IsNullOrEmpty(this.VideoUri))
		{
			this.destroyRenderTexture();
			return;
		}
		this.updateRenderTexture();
		string text = null;
		string text2;
		if (ModManager.TryPatchModPathString(this.VideoUri, out text2))
		{
			text = text2;
		}
		else if (this.VideoUri[0] == '@' && this.VideoUri[1] != ':')
		{
			string text3 = this.VideoUri.Substring(1);
			if (text3.StartsWith("file:", StringComparison.OrdinalIgnoreCase))
			{
				text = text3.Substring(5);
				if (text[0] != '/' && text[0] != '\\')
				{
					text = new Uri(((Application.platform == 1 || Application.platform == 45) ? (Application.dataPath + "/../../") : (Application.dataPath + "/../")) + text).AbsoluteUri;
				}
			}
			if (text == null)
			{
				this.videoPlayer.url = text3;
			}
		}
		else
		{
			text = Application.streamingAssetsPath + this.VideoUri;
		}
		if (text != null)
		{
			string extension = Path.GetExtension(text);
			if (string.IsNullOrEmpty(extension) || extension.Length > 5)
			{
				string text4;
				if (Application.platform == 38)
				{
					text4 = ".mp4";
				}
				else
				{
					text4 = ".webm";
				}
				string text5 = text4;
				if (Application.platform == 38)
				{
					text4 = ".webm";
				}
				else
				{
					text4 = ".mp4";
				}
				string text6 = text4;
				if (File.Exists(text + text5))
				{
					text += text5;
				}
				else if (File.Exists(text + text6))
				{
					text += text6;
				}
				else
				{
					text = null;
				}
			}
			this.videoPlayer.url = text;
		}
		this.videoPlayer.Prepare();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void stopVideo()
	{
		if (!this.videoPlayer.isPlaying)
		{
			return;
		}
		this.videoPlayer.Stop();
		this.videoPlayer.url = null;
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		this.updateRenderTexture();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void updateData()
	{
		base.updateData();
		this.uiTexture.enabled = true;
		this.videoPlayer.aspectRatio = this.VideoAspect;
		this.videoPlayer.isLooping = this.Loop;
		if (this.videoUriChanged)
		{
			this.stopVideo();
			this.startVideo();
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void updateVolumeLevel()
	{
		if (!this.videoPlayer.isPrepared || this.videoPlayer.audioTrackCount < 1)
		{
			return;
		}
		float @float = GamePrefs.GetFloat(this.VolumeSetting);
		this.videoPlayer.SetDirectAudioVolume(0, @float);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateRenderTexture()
	{
		Vector3[] worldCorners = this.uiTexture.worldCorners;
		Vector3 vector = this.xui.playerUI.camera.WorldToScreenPoint(worldCorners[2]);
		Vector3 vector2 = this.xui.playerUI.camera.WorldToScreenPoint(worldCorners[0]);
		int num = Mathf.RoundToInt(vector.x - vector2.x);
		int num2 = Mathf.RoundToInt(vector.y - vector2.y);
		if (num < 2 || num2 < 2)
		{
			return;
		}
		if (this.rt != null && this.rt.width == num && this.rt.height == num2)
		{
			return;
		}
		this.destroyRenderTexture();
		this.rt = new RenderTexture(num, num2, 16);
		this.rt.Create();
		this.Texture = (this.videoPlayer.targetTexture = this.rt);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void destroyRenderTexture()
	{
		if (this.rt == null)
		{
			return;
		}
		this.Texture = (this.videoPlayer.targetTexture = null);
		this.rt.Release();
		Object.Destroy(this.rt);
		this.rt = null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnVideoPrepared(VideoPlayer _source)
	{
		this.videoPlayer.Play();
		this.updateVolumeLevel();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnVideoErrorReceived(VideoPlayer _source, string _message)
	{
		Log.Error("[XUi] Video player encountered an error. Skipping video. Message: " + _message);
		XUiV_Video.VideoErrorDelegate videoError = this.VideoError;
		if (videoError == null)
		{
			return;
		}
		videoError(this);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnVideoFinished(VideoPlayer _source)
	{
		XUiV_Video.VideoFinishedDelegate videoFinished = this.VideoFinished;
		if (videoFinished == null)
		{
			return;
		}
		videoFinished(this);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public VideoPlayer videoPlayer;

	[PublicizedFrom(EAccessModifier.Private)]
	public RenderTexture rt;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool loop;

	[PublicizedFrom(EAccessModifier.Private)]
	public string videoUri;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool videoUriChanged;

	[PublicizedFrom(EAccessModifier.Private)]
	public VideoAspectRatio videoAspect = 3;

	[PublicizedFrom(EAccessModifier.Private)]
	public EnumGamePrefs volumeSetting = EnumGamePrefs.OptionsMenuMusicVolumeLevel;

	public delegate void VideoErrorDelegate(XUiV_Video _sender);

	public delegate void VideoFinishedDelegate(XUiV_Video _sender);
}
