using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_EulaWindow : XUiController
{
	[XuiXmlBinding("pages")]
	public Dictionary<string, string> PagesDict
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return this.pagesDict;
		}
	}

	[XuiXmlBinding("pagecount")]
	public int PageCount
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return this.pagesDict.Count;
		}
	}

	[XuiXmlBinding("viewmode")]
	public bool ViewMode
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return XUiC_EulaWindow.viewMode;
		}
	}

	[XuiXmlBinding("allowaccept")]
	public bool AllowAccept
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return this.allowAccept;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			if (this.allowAccept == value)
			{
				return;
			}
			this.allowAccept = value;
			this.IsDirty = true;
		}
	}

	[XuiXmlBinding("uivisible")]
	public bool UIVisible
	{
		get
		{
			return this.uiVisible;
		}
		set
		{
			if (this.uiVisible == value)
			{
				return;
			}
			this.uiVisible = value;
			this.IsDirty = true;
		}
	}

	public override void Init()
	{
		base.Init();
		XUiC_EulaWindow.id = base.WindowGroup.Id;
		TextAsset textAsset = Resources.Load<TextAsset>("Data/EULA/eula_" + Localization.ActiveLanguage.ToLower());
		if (textAsset != null)
		{
			this.loadDefaultXML(textAsset.bytes);
			return;
		}
		Log.Error("Could not load default EULA text asset");
	}

	public static void Open(XUi _xui, bool _viewMode = false)
	{
		XUiC_EulaWindow.viewMode = _viewMode;
		_xui.playerUI.windowManager.Open(XUiC_EulaWindow.id, true, true);
	}

	public override void OnOpen()
	{
		base.OnOpen();
		this.scrollBar.ScrollPosition = 0f;
		this.pageFormatted = false;
		this.UIVisible = false;
		this.AllowAccept = false;
		if (XUiC_EulaWindow.viewMode)
		{
			this.btnDone.SelectCursorElement(true, false);
		}
	}

	public override void OnClose()
	{
		base.OnClose();
		this.scrollBar.ScrollPosition = 0f;
		this.scrollView.ResetPosition();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void close()
	{
		this.xui.playerUI.windowManager.Close(this.windowGroup, false);
		if (XUiC_EulaWindow.viewMode)
		{
			this.xui.playerUI.windowManager.Open(XUiC_OptionsGeneral.ID, true);
			return;
		}
		XUiC_MainMenu.Open(this.xui);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void loadDefaultXML(byte[] _data)
	{
		XmlFile xmlFile;
		try
		{
			xmlFile = new XmlFile(_data, true);
		}
		catch (Exception ex)
		{
			Log.Error("Failed loading default EULA XML: {0}", new object[] { ex.Message });
			return;
		}
		XElement root = xmlFile.XmlDoc.Root;
		if (root == null)
		{
			return;
		}
		this.defaultEulaVersion = int.Parse(root.GetAttribute("version").Trim());
		this.defaultEula = root.Value;
		if (this.defaultEulaVersion > GamePrefs.GetInt(EnumGamePrefs.EulaLatestVersion))
		{
			GamePrefs.Set(EnumGamePrefs.EulaLatestVersion, this.defaultEulaVersion);
		}
		Log.Out("Loaded default EULA");
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		base.handleDirtyUpdateDefault();
		if (!this.pageFormatted)
		{
			if (GameManager.UpdatingRemoteResources || !GameManager.RemoteResourcesLoaded)
			{
				return;
			}
			this.pageFormatted = true;
			this.displayEulaOrClose();
		}
		if (!XUiC_EulaWindow.viewMode && (double)this.scrollBar.ScrollPosition >= 0.98 && !this.AllowAccept)
		{
			this.xui.playerUI.CursorController.SetNavigationTarget(this.btnDecline.ViewComponent);
			this.AllowAccept = true;
		}
		if (this.xui.playerUI.playerInput.PermanentActions.Cancel.WasReleased)
		{
			this.close();
		}
		if (!XUiC_EulaWindow.viewMode && this.xui.playerUI.playerInput.GUIActions.Apply.WasReleased)
		{
			this.btnAccept_OnPressed(this, -1);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void displayEulaOrClose()
	{
		bool flag = string.IsNullOrEmpty(XUiC_EulaWindow.RetrievedEula);
		bool flag2 = (flag ? (GamePrefs.GetInt(EnumGamePrefs.EulaVersionAccepted) >= this.defaultEulaVersion) : GameManager.HasAcceptedLatestEula());
		if (!XUiC_EulaWindow.viewMode && flag2)
		{
			this.close();
			return;
		}
		this.UIVisible = true;
		string text = (flag ? this.defaultEula : XUiC_EulaWindow.RetrievedEula);
		this.formatPages(text);
		ThreadManager.StartCoroutine(this.updateScrolling());
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator updateScrolling()
	{
		this.textTable.Reposition();
		yield return null;
		this.textTable.Reposition();
		this.scrollView.ResetPosition();
		int num;
		for (int i = 0; i < 5; i = num + 1)
		{
			yield return null;
			this.scrollView.UpdatePosition();
			num = i;
		}
		yield break;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void formatPages(string _content)
	{
		_content = _content.Replace("\t", "  ");
		this.pagesDict.Clear();
		string[] array = _content.Split('\n', StringSplitOptions.None);
		string activeLanguage = Localization.ActiveLanguage;
		int num;
		if (activeLanguage == "japanese" || activeLanguage == "koreana" || activeLanguage == "schinese" || activeLanguage == "tchinese")
		{
			num = 1000;
		}
		else
		{
			num = 2000;
		}
		int num2 = num;
		StringBuilder stringBuilder = new StringBuilder();
		int i = 0;
		while (i < array.Length)
		{
			stringBuilder.AppendLine(array[i]);
			stringBuilder.AppendLine();
			i++;
			while (stringBuilder.Length < num2 && i < array.Length)
			{
				stringBuilder.AppendLine(array[i]);
				stringBuilder.AppendLine();
				i++;
			}
			stringBuilder.Length--;
			this.pagesDict.Add(this.pagesDict.Count.ToString(), stringBuilder.ToString());
			stringBuilder.Clear();
		}
	}

	[XuiBindEvent("OnPress", "btnAccept")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void btnAccept_OnPressed(XUiController _sender, int _mouseButton)
	{
		if (!this.btnAccept.ViewComponent.Enabled)
		{
			return;
		}
		GamePrefs.Set(EnumGamePrefs.EulaVersionAccepted, GamePrefs.GetInt(EnumGamePrefs.EulaLatestVersion));
		GamePrefs.Instance.Save();
		this.close();
	}

	[XuiBindEvent("OnPress", "btnDecline")]
	[XuiBindEvent("OnPress", "btnDone")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void btnDecline_OnPressed(XUiController _sender, int _mouseButton)
	{
		this.close();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const int AlphanumericPageCharacterLimit = 2000;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int ScriptPageCharacterLimit = 1000;

	public static string RetrievedEula;

	[PublicizedFrom(EAccessModifier.Private)]
	public static string id;

	[XuiBindComponent("btnAccept", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button btnAccept;

	[XuiBindComponent("btnDecline", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button btnDecline;

	[XuiBindComponent("btnDone", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button btnDone;

	[XuiBindComponent(true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiV_ScrollView scrollView;

	[XuiBindComponent(true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiV_ScrollBar scrollBar;

	[XuiBindComponent(true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiV_Table textTable;

	[PublicizedFrom(EAccessModifier.Private)]
	public string defaultEula;

	[PublicizedFrom(EAccessModifier.Private)]
	public int defaultEulaVersion = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<string, string> pagesDict = new Dictionary<string, string>();

	[PublicizedFrom(EAccessModifier.Private)]
	public bool pageFormatted;

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool viewMode;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool allowAccept;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool uiVisible;
}
