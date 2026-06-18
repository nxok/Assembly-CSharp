using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using Audio;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_LoadingScreen : XUiController
{
	[XuiXmlAttribute("browse_sound", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public void attribBrowseSound(string _value)
	{
		this.xui.LoadData<AudioClip>(_value, delegate(AudioClip _o)
		{
			this.browseSound = _o;
		});
	}

	[XuiXmlBinding("background_texture")]
	public string CurrentBackground
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return this.currentBackground;
		}
	}

	[XuiXmlBinding("show_tips")]
	public bool ShowTips
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return this.showTips;
		}
	}

	[XuiXmlBinding("count")]
	public int TipCount
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return XUiC_LoadingScreen.tips.Count;
		}
	}

	[XuiXmlBinding("index")]
	public int TipIndex
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return this.currentTipIndex + 1;
		}
	}

	[XuiXmlBinding("title")]
	public string TipTitle
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			if (this.currentTipIndex >= 0)
			{
				return Localization.Get(XUiC_LoadingScreen.tips[this.currentTipIndex] + "_title", false, null);
			}
			return "";
		}
	}

	[XuiXmlBinding("text")]
	public string TipText
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			if (this.currentTipIndex >= 0)
			{
				return Localization.Get(XUiC_LoadingScreen.tips[this.currentTipIndex], false, null);
			}
			return "";
		}
	}

	public override void Init()
	{
		base.Init();
		XUiC_LoadingScreen.ID = base.WindowGroup.Id;
		XUiController childById = base.GetChildById("loading_image");
		if (childById != null)
		{
			this.backgroundTextureView = childById.ViewComponent as XUiV_Texture;
		}
		XUiController childById2 = base.GetChildById("pnlBlack");
		if (childById2 != null)
		{
			childById2.OnPress += delegate(XUiController _, int _)
			{
				this.cycle(1);
			};
			childById2.OnRightPress += delegate(XUiController _, int _)
			{
				this.cycle(-1);
			};
		}
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (PlatformManager.NativePlatform.Input.CurrentInputStyle != PlayerInputManager.InputStyle.Keyboard || !XUiC_SpawnSelectionWindow.IsInSpawnSelection(this.xui.playerUI))
		{
			if (this.xui.playerUI.playerInput.PermanentActions.PageTipsForward.WasPressed)
			{
				this.cycle(1);
			}
			else if (this.xui.playerUI.playerInput.PermanentActions.PageTipsBack.WasPressed)
			{
				this.cycle(-1);
			}
		}
		if (this.IsDirty)
		{
			base.RefreshBindings();
			this.IsDirty = false;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void cycle(int _increment)
	{
		if (!this.showTips || XUiC_VideoPlayer.IsVideoPlaying)
		{
			return;
		}
		this.currentTipIndex += _increment;
		if (this.currentTipIndex >= XUiC_LoadingScreen.tips.Count)
		{
			this.currentTipIndex = 0;
		}
		else if (this.currentTipIndex < 0)
		{
			this.currentTipIndex = XUiC_LoadingScreen.tips.Count - 1;
		}
		if (this.browseSound != null)
		{
			Manager.PlayXUiSound(this.browseSound, 1f);
		}
		this.IsDirty = true;
	}

	public override void OnOpen()
	{
		base.OnOpen();
		((XUiV_Window)base.ViewComponent).ForceVisible(1f);
		if (XUiC_LoadingScreen.backgrounds.Count > 0)
		{
			Random.InitState(Time.frameCount);
			this.currentBackground = XUiC_LoadingScreen.backgrounds[Random.Range(0, XUiC_LoadingScreen.backgrounds.Count)];
		}
		this.showTips = true;
		this.currentTipIndex = GamePrefs.GetInt(EnumGamePrefs.LastLoadingTipRead) + 1;
		if (this.currentTipIndex >= XUiC_LoadingScreen.tips.Count)
		{
			this.currentTipIndex = 0;
		}
		this.currentTipIndex = Mathf.Clamp(this.currentTipIndex, 0, XUiC_LoadingScreen.tips.Count - 1);
		base.RefreshBindings();
	}

	public override void OnClose()
	{
		base.OnClose();
		GamePrefs.Set(EnumGamePrefs.LastLoadingTipRead, this.currentTipIndex);
		XUiV_Texture xuiV_Texture = this.backgroundTextureView;
		if (xuiV_Texture != null)
		{
			xuiV_Texture.UnloadTexture();
		}
		this.xui.playerUI.CursorController.SetCursorHidden(false);
	}

	public void SetTipsVisible(bool _visible)
	{
		if (this.showTips == _visible)
		{
			return;
		}
		this.showTips = _visible;
		this.IsDirty = true;
	}

	public static IEnumerator LoadXml(XmlFile _xmlFile)
	{
		XContainer root = _xmlFile.XmlDoc.Root;
		XUiC_LoadingScreen.backgrounds.Clear();
		XUiC_LoadingScreen.tips.Clear();
		using (IEnumerator<XElement> enumerator = root.Elements().GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				XElement xelement = enumerator.Current;
				if (xelement.Name == "backgrounds")
				{
					using (IEnumerator<XElement> enumerator2 = xelement.Elements("tex").GetEnumerator())
					{
						while (enumerator2.MoveNext())
						{
							XElement xelement2 = enumerator2.Current;
							if (!xelement2.HasAttribute("file"))
							{
								Log.Warning("Backgrounds entry is missing file attribute, skipping.");
							}
							else
							{
								XUiC_LoadingScreen.backgrounds.Add(xelement2.GetAttribute("file"));
							}
						}
						continue;
					}
				}
				if (xelement.Name == "tips")
				{
					foreach (XElement xelement3 in xelement.Elements("tip"))
					{
						if (!xelement3.HasAttribute("key"))
						{
							Log.Warning("Loading tips entry is missing file attribute, skipping.");
						}
						else
						{
							XUiC_LoadingScreen.tips.Add(xelement3.GetAttribute("key"));
						}
					}
				}
			}
			yield break;
		}
		yield break;
	}

	public static string ID = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public int currentTipIndex = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	public string currentBackground = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public bool showTips = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public AudioClip browseSound;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Texture backgroundTextureView;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly List<string> backgrounds = new List<string>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly List<string> tips = new List<string>();
}
