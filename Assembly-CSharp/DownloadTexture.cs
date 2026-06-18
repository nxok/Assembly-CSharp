using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(UITexture))]
public class DownloadTexture : MonoBehaviour
{
	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator Start()
	{
		UnityWebRequest www = UnityWebRequest.Get(this.url);
		yield return www.SendWebRequest();
		this.mTex = DownloadHandlerTexture.GetContent(www);
		if (this.mTex != null)
		{
			UITexture component = base.GetComponent<UITexture>();
			component.mainTexture = this.mTex;
			if (this.pixelPerfect)
			{
				component.MakePixelPerfect();
			}
		}
		www.Dispose();
		yield break;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnDestroy()
	{
		if (this.mTex != null)
		{
			Object.Destroy(this.mTex);
		}
	}

	public string url = "http://www.yourwebsite.com/logo.png";

	public bool pixelPerfect = true;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Texture2D mTex;
}
