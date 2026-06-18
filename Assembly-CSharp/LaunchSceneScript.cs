using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LaunchSceneScript : MonoBehaviour
{
	[PublicizedFrom(EAccessModifier.Private)]
	public void Awake()
	{
		Cursor.visible = false;
		base.StartCoroutine(this.GoToNextSceneCo());
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator GoToNextSceneCo()
	{
		string nextScene;
		bool flag;
		if (GameStartupHelper.GetCommandLineArgs().ContainsCaseInsensitive("-skipintro"))
		{
			nextScene = "SceneGame";
			flag = true;
		}
		else
		{
			nextScene = "SceneSplash";
			flag = false;
		}
		this.fadeInUIPanel.alpha = 0f;
		if (flag)
		{
			float timer = 0.6f;
			while (timer > 0f)
			{
				this.fadeInUIPanel.alpha = Mathf.Lerp(1f, 0f, Mathf.Clamp01(timer / 0.6f));
				timer -= Time.deltaTime;
				yield return null;
			}
			this.fadeInUIPanel.alpha = 1f;
		}
		yield return new WaitForEndOfFrame();
		yield return GameEntrypoint.EntrypointCoroutine();
		if (!GameEntrypoint.EntrypointSuccess)
		{
			yield break;
		}
		SceneManager.LoadScene(nextScene);
		yield break;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const string MainSceneName = "SceneGame";

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const string SplashSceneName = "SceneSplash";

	public UIPanel fadeInUIPanel;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const float fadeInDuration = 0.6f;
}
