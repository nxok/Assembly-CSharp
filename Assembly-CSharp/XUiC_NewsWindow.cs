using System;
using System.Collections.Generic;
using Audio;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_NewsWindow : XUiController
{
	[XuiXmlBinding("has_news_provider")]
	public bool HasProviders
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get;
		[PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[XuiXmlAttribute("max_entries", false)]
	public int MaxEntries
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get;
		[PublicizedFrom(EAccessModifier.Private)]
		set;
	} = int.MaxValue;

	[XuiXmlAttribute("auto_cycle_time_per_entry", false)]
	public float AutoCycleTimePerEntry
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get;
		[PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public event XUiC_NewsWindow.NewsEntryClickedDelegate NewsEntryClicked;

	public int CurrentIndex
	{
		get
		{
			return this.currentIndex;
		}
		set
		{
			if (value >= this.entries.Count)
			{
				value = this.entries.Count - 1;
			}
			if (value >= this.MaxEntries)
			{
				value = this.MaxEntries - 1;
			}
			if (value < 0)
			{
				value = 0;
			}
			this.currentIndex = value;
			this.IsDirty = true;
		}
	}

	public NewsManager.NewsEntry CurrentEntry
	{
		get
		{
			if (this.CurrentIndex < 0)
			{
				return null;
			}
			if (this.CurrentIndex >= this.entries.Count)
			{
				return null;
			}
			object obj = this.newsLock;
			NewsManager.NewsEntry newsEntry;
			lock (obj)
			{
				newsEntry = this.entries[this.CurrentIndex];
			}
			return newsEntry;
		}
	}

	public NewsManager.NewsEntry CurrentEntryOrEmpty
	{
		get
		{
			return this.CurrentEntry ?? NewsManager.EmptyEntry;
		}
	}

	[XuiXmlAttribute("browse_sound", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public void attribBrowseSound(string _value)
	{
		this.xui.LoadData<AudioClip>(_value, delegate(AudioClip _o)
		{
			this.browseSound = _o;
		});
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void parseSourcesString(string _value)
	{
		string[] array = _value.Split(',', StringSplitOptions.None);
		for (int i = 0; i < array.Length; i++)
		{
			string text = array[i].Trim();
			if (text.Length != 0)
			{
				this.newsProviders.Add(text);
				NewsManager.Instance.RegisterNewsSource(text);
			}
		}
	}

	[XuiXmlAttribute("additional_sources", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public void attribAdditionalSources(string _value)
	{
		this.parseSourcesString(_value);
	}

	[XuiXmlAttribute("sources", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public void attribSources(string _value)
	{
		this.parseSourcesString(_value);
	}

	[XuiXmlBinding("news_count")]
	public int NewsCount
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return Mathf.Min(this.entries.Count, this.MaxEntries);
		}
	}

	[XuiXmlBinding("is_custom")]
	public bool NewsIsCustom
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return this.CurrentEntryOrEmpty.IsCustom;
		}
	}

	[XuiXmlBinding("source")]
	public string NewsSource
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return this.CurrentEntryOrEmpty.CustomListName ?? "";
		}
	}

	[XuiXmlBinding("has_text")]
	public bool NewsHasText
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return !string.IsNullOrEmpty(this.CurrentEntryOrEmpty.Text);
		}
	}

	[XuiXmlBinding("headline")]
	public string NewsHeadline
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return this.CurrentEntryOrEmpty.Headline ?? "";
		}
	}

	[XuiXmlBinding("headline2")]
	public string NewsHeadline2
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return this.CurrentEntryOrEmpty.Headline2 ?? "";
		}
	}

	[XuiXmlBinding("date")]
	public string NewsDate
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return this.CurrentEntryOrEmpty.Date.ToString("yyyy-MM-dd");
		}
	}

	[XuiXmlBinding("age")]
	public string NewsAge
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return ValueDisplayFormatters.DateAge(this.CurrentEntryOrEmpty.Date);
		}
	}

	[XuiXmlBinding("text")]
	public string NewsText
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return this.CurrentEntryOrEmpty.Text ?? "";
		}
	}

	[XuiXmlBinding("has_link")]
	public bool NewsHasLink
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return !string.IsNullOrEmpty(this.CurrentEntryOrEmpty.Url);
		}
	}

	[XuiXmlBinding("link")]
	public string NewsLink
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return this.CurrentEntryOrEmpty.Url ?? "";
		}
	}

	[XuiXmlBinding("has_younger")]
	public bool HasYounger
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return this.entries.Count > 0 && this.CurrentIndex > 0;
		}
	}

	[XuiXmlBinding("has_older")]
	public bool HasOlder
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return this.entries.Count > 0 && this.CurrentIndex < this.entries.Count - 1;
		}
	}

	public override void Init()
	{
		base.Init();
		XUiController childById = base.GetChildById("btnYounger");
		if (childById != null)
		{
			childById.OnPress += delegate(XUiController _, int _)
			{
				this.cycle(-1, false);
			};
		}
		XUiController childById2 = base.GetChildById("btnOlder");
		if (childById2 != null)
		{
			childById2.OnPress += delegate(XUiController _, int _)
			{
				this.cycle(1, false);
			};
		}
		XUiC_SimpleButton xuiC_SimpleButton = base.GetChildById("btnLink") as XUiC_SimpleButton;
		if (xuiC_SimpleButton != null)
		{
			xuiC_SimpleButton.OnPressed += this.BtnLink_OnPressed;
		}
		else
		{
			XUiController childById3 = base.GetChildById("btnLink");
			if (childById3 != null && childById3.ViewComponent is XUiV_Button)
			{
				childById3.OnPress += this.BtnLink_OnPressed;
				childById3.OnScroll += this.OnScrollEvent;
			}
		}
		base.OnScroll += this.OnScrollEvent;
		List<XUiV_Texture> list = new List<XUiV_Texture>();
		foreach (XUiController xuiController in base.GetChildrenById("newsImage", null))
		{
			XUiV_Texture xuiV_Texture = ((xuiController != null) ? xuiController.ViewComponent : null) as XUiV_Texture;
			if (xuiV_Texture != null)
			{
				list.Add(xuiV_Texture);
			}
		}
		this.bannerTextures = list.ToArray();
		this.selector = base.GetChildByType<XUiC_ComboBoxFloat>();
		if (this.selector != null)
		{
			this.selector.OnValueChanged += this.Selector_OnValueChanged;
		}
		this.HasProviders = this.newsProviders.Count > 0;
		if (!this.HasProviders)
		{
			Log.Warning(string.Concat(new string[]
			{
				"[XUi] News controller with no sources specified (window group '",
				base.WindowGroup.Id,
				"', window '",
				base.ViewComponent.ID,
				"')"
			}));
		}
		NewsManager.Instance.Updated += this.newsUpdated;
		this.newsUpdated(NewsManager.Instance);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnScrollEvent(XUiController _sender, float _delta)
	{
		if (this.selector == null)
		{
			this.cycle(Math.Sign(_delta), true);
			return;
		}
		this.selector.ScrollEvent(_sender, _delta);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Selector_OnValueChanged(XUiController _sender, double _oldValue, double _newValue)
	{
		this.CurrentIndex = Mathf.CeilToInt((float)_newValue) - 1;
		this.selector.Value = (double)(this.CurrentIndex + 1);
		this.autoCycle = false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void newsUpdated(NewsManager _newsManager)
	{
		NewsManager.NewsEntry currentEntry = this.CurrentEntry;
		object obj = this.newsLock;
		lock (obj)
		{
			_newsManager.GetNewsData(this.newsProviders, this.entries);
		}
		int num = 0;
		while (num < this.MaxEntries && num < this.entries.Count)
		{
			this.entries[num].RequestImage();
			num++;
		}
		this.IsDirty = true;
		if (currentEntry == null || !currentEntry.Equals(this.CurrentEntry))
		{
			this.resetIndex();
		}
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		this.doAutoCycle(_dt);
		if (this.IsDirty)
		{
			base.RefreshBindings();
			if (this.bannerTextures != null && this.bannerTextures.Length != 0)
			{
				NewsManager.NewsEntry currentEntry = this.CurrentEntry;
				Texture2D texture2D = ((currentEntry != null) ? currentEntry.Image : null);
				XUiV_Texture[] array = this.bannerTextures;
				for (int i = 0; i < array.Length; i++)
				{
					array[i].Texture = texture2D;
				}
			}
			this.IsDirty = false;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void doAutoCycle(float _dt)
	{
		if (this.selector == null || !this.autoCycle)
		{
			return;
		}
		NewsManager.NewsEntry currentEntry = this.CurrentEntry;
		if (currentEntry == null || (currentEntry.HasImage && !currentEntry.ImageLoaded))
		{
			return;
		}
		float num = (float)this.selector.Value + _dt / this.AutoCycleTimePerEntry;
		if ((double)num > this.selector.Max)
		{
			num = 0f;
		}
		this.selector.Value = (double)num;
		this.CurrentIndex = Mathf.CeilToInt((float)this.selector.Value) - 1;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnLink_OnPressed(XUiController _sender, int _mousebutton)
	{
		NewsManager.NewsEntry currentEntry = this.CurrentEntry;
		if (currentEntry == null)
		{
			return;
		}
		if (this.NewsEntryClicked != null)
		{
			this.NewsEntryClicked(currentEntry);
			return;
		}
		string url = currentEntry.Url;
		if (url == null || !url.StartsWith("openstore://"))
		{
			XUiC_MessageBoxWindowGroup.ShowUrlConfirmationDialog(this.xui, currentEntry.Url, false, null, null, null, null, false);
			return;
		}
		EntitlementSetEnum entitlementSetEnum;
		try
		{
			entitlementSetEnum = Enum.Parse<EntitlementSetEnum>(currentEntry.Url.Substring("openstore://".Length), true);
		}
		catch (Exception)
		{
			Log.Error("DLC link uses incorrect value!");
			entitlementSetEnum = EntitlementSetEnum.None;
		}
		if (entitlementSetEnum != EntitlementSetEnum.None)
		{
			EntitlementManager.Instance.OpenStore(entitlementSetEnum, delegate(EntitlementSetEnum _)
			{
				Log.Out("DLC dialog complete!");
			});
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void cycle(int _increment, bool _sound)
	{
		object obj = this.newsLock;
		lock (obj)
		{
			int num = this.CurrentIndex;
			this.CurrentIndex += _increment;
			if (this.CurrentIndex != num && _sound && this.browseSound != null)
			{
				Manager.PlayXUiSound(this.browseSound, 1f);
			}
			this.autoCycle = false;
			if (this.selector != null)
			{
				this.selector.Value = (double)(this.CurrentIndex + 1);
			}
			this.IsDirty = true;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void resetIndex()
	{
		this.CurrentIndex = 0;
		if (this.selector != null)
		{
			this.selector.Value = (double)(this.autoCycle ? 0 : 1);
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		this.autoCycle = this.AutoCycleTimePerEntry > 0f;
		this.resetIndex();
		base.RefreshBindings();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const string PlatformStoreLink = "openstore://";

	[PublicizedFrom(EAccessModifier.Private)]
	public AudioClip browseSound;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxFloat selector;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Texture[] bannerTextures;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly object newsLock = new object();

	[PublicizedFrom(EAccessModifier.Private)]
	public int currentIndex;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<NewsManager.NewsEntry> entries = new List<NewsManager.NewsEntry>();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<string> newsProviders = new List<string>();

	[PublicizedFrom(EAccessModifier.Private)]
	public bool autoCycle;

	public delegate void NewsEntryClickedDelegate(NewsManager.NewsEntry _newsEntry);
}
