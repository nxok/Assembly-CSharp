using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class SplashScreenScript : MonoBehaviour
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public void Awake()
	{
		if (!GameEntrypoint.EntrypointSuccess)
		{
			return;
		}
		if (GameManager.IsDedicatedServer)
		{
			SceneManager.LoadScene(SplashScreenScript.MainSceneName);
			return;
		}
		if (GameUtils.GetLaunchArgument("skipintro") != null || !string.IsNullOrEmpty(LaunchPrefs.RunAutomation.Value))
		{
			SceneManager.LoadScene(SplashScreenScript.MainSceneName);
			return;
		}
		GameOptionsManager.ApplyTextureQuality(-1);
		this.labelEaWarning.text = Localization.Get("splashMessageEarlyAccessWarning", false, null);
		this.videoPlayer.prepareCompleted += new VideoPlayer.EventHandler(this.OnVideoPrepared);
		this.videoPlayer.loopPointReached += new VideoPlayer.EventHandler(this.OnVideoFinished);
		this.videoPlayer.errorReceived += new VideoPlayer.ErrorEventHandler(this.OnVideoErrorReceived);
		this.videoPlayer.url = Application.streamingAssetsPath + "/Video/TFP_Intro.webm";
		this.videoPlayer.Prepare();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void Update()
	{
		if ((this.videoPlayer.isPlaying && Input.anyKey) || this.videoFinished)
		{
			SceneManager.LoadScene(SplashScreenScript.MainSceneName);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void OnVideoPrepared(VideoPlayer player)
	{
		base.StartCoroutine(this.DelayVideoRoutine());
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator DelayVideoRoutine()
	{
		yield return new WaitForSecondsRealtime(0.3f);
		this.videoPlayer.Play();
		yield break;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void OnVideoFinished(VideoPlayer player)
	{
		this.videoFinished = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnVideoErrorReceived(VideoPlayer player, string message)
	{
		Log.Error("SplashScreen video error: " + message);
		this.videoFinished = true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void OnGUI()
	{
		GUI.contentColor = new Color(0f, 0f, 0f, 0f);
		GUILayout.Label("Test", Array.Empty<GUILayoutOption>());
	}

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static readonly string MainSceneName = "SceneGame";

	public Transform wdwSplashScreen;

	public UILabel labelEaWarning;

	public VideoPlayer videoPlayer;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool videoFinished;
}
