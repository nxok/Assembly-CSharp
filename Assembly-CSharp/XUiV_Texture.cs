using System;
using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Networking;

public class XUiV_Texture : XUiV_TextureBased
{
	public override Texture Texture
	{
		set
		{
			if (this.AutoUnload && base.Texture != null)
			{
				this.UnloadTexture();
			}
			base.Texture = value;
			if (value == null)
			{
				this.isExternalTexture = false;
			}
		}
	}

	[XuiXmlAttribute("textures", false)]
	public string[] TextureUris
	{
		get
		{
			return this.textureUris;
		}
		set
		{
			if (value == null || value.Length == 0)
			{
				this.textureUris = Array.Empty<string>();
				this.Texture = null;
				base.SetDirty();
				return;
			}
			this.textureUris = value;
			this.loadTexture(0);
		}
	}

	[XuiXmlAttribute("texture", false)]
	public string Path
	{
		get
		{
			return this.textureUris[0];
		}
		set
		{
			if (string.IsNullOrEmpty(value))
			{
				this.TextureUris = null;
				return;
			}
			if (this.TextureUris.Length == 1 && this.TextureUris[0] == value)
			{
				return;
			}
			this.TextureUris = new string[] { value };
		}
	}

	[XuiXmlAttribute("autounload", false)]
	public bool AutoUnload
	{
		get
		{
			return this.autoUnload;
		}
		set
		{
			if (this.autoUnload == value)
			{
				return;
			}
			this.autoUnload = value;
		}
	}

	public XUiV_Texture(XUi _xui, string _id)
		: base(_xui, _id)
	{
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void loadTexture(int _index = 0)
	{
		if (_index >= this.TextureUris.Length)
		{
			return;
		}
		string text = this.TextureUris[_index];
		try
		{
			string text2;
			if (ModManager.TryPatchModPathString(text, out text2))
			{
				this.<loadTexture>g__FetchWwwTexture|15_0(_index, "file://" + text2);
			}
			else if (text[0] == '@' && text[1] != ':')
			{
				string text3 = text.Substring(1);
				if (text3.StartsWith("file:", StringComparison.OrdinalIgnoreCase))
				{
					string text4 = text3.Substring(5);
					if (text4[0] != '/' && text4[0] != '\\')
					{
						text3 = new Uri(((Application.platform == 1 || Application.platform == 45) ? (Application.dataPath + "/../../") : (Application.dataPath + "/../")) + text4).AbsoluteUri;
					}
				}
				this.<loadTexture>g__FetchWwwTexture|15_0(_index, text3);
			}
			else
			{
				this.xui.LoadData<Texture>(text, delegate(Texture _o)
				{
					this.Texture = _o;
					this.isExternalTexture = false;
				});
			}
		}
		catch (Exception ex)
		{
			Log.Error("[XUi] Could not load texture: " + text + ", on " + base.GetXuiHierarchy());
			Log.Exception(ex);
		}
		base.SetDirty();
	}

	public override void UnloadTexture()
	{
		Texture texture = this.Texture;
		base.UnloadTexture();
		this.TextureUris = null;
		if (texture == null)
		{
			return;
		}
		if (!this.isExternalTexture)
		{
			Resources.UnloadAsset(texture);
			return;
		}
		Object.DestroyImmediate(texture);
	}

	[CompilerGenerated]
	[PublicizedFrom(EAccessModifier.Private)]
	public void <loadTexture>g__FetchWwwTexture|15_0(int _currentIndex, string _fetchUri)
	{
		_fetchUri = _fetchUri.Replace("#", "%23").Replace("+", "%2B");
		UnityWebRequest texture = UnityWebRequestTexture.GetTexture(_fetchUri);
		texture.SendWebRequest();
		ThreadManager.StartCoroutine(this.<loadTexture>g__WaitForWwwData|15_1(_currentIndex, texture, _fetchUri));
	}

	[CompilerGenerated]
	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator <loadTexture>g__WaitForWwwData|15_1(int _currentIndex, UnityWebRequest _www, string _fetchUri)
	{
		while (!_www.isDone)
		{
			yield return null;
		}
		if (_www.result != 1)
		{
			if (this.TextureUris.Length == 1)
			{
				Log.Warning(string.Concat(new string[] { "[XUi] Retrieving texture file from '", _fetchUri, "' failed (", _www.error, ")." }));
			}
			else if (_currentIndex + 1 < this.TextureUris.Length)
			{
				Log.Warning(string.Format("[XUi] Retrieving texture file {0} from '{1}' failed: ({2}). Trying next URI.", _currentIndex + 1, _fetchUri, _www.error));
				this.loadTexture(_currentIndex + 1);
			}
			else
			{
				Log.Warning(string.Format("[XUi] Retrieving texture file {0} from '{1}' failed: ({2}). No URIs successful.", _currentIndex + 1, _fetchUri, _www.error));
			}
			yield break;
		}
		Texture2D texture = ((DownloadHandlerTexture)_www.downloadHandler).texture;
		this.Texture = TextureUtils.CloneTexture(texture, false, false, true);
		Object.DestroyImmediate(texture);
		_www.Dispose();
		this.isExternalTexture = true;
		yield break;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string[] textureUris = Array.Empty<string>();

	[PublicizedFrom(EAccessModifier.Private)]
	public bool autoUnload;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool isExternalTexture;
}
