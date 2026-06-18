using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Xml;
using System.Xml.Linq;
using Platform;
using UnityEngine;

public class NewsManager
{
	public static NewsManager Instance
	{
		get
		{
			NewsManager newsManager;
			if ((newsManager = NewsManager.instance) == null)
			{
				newsManager = (NewsManager.instance = new NewsManager());
			}
			return newsManager;
		}
	}

	public event Action<NewsManager> Updated;

	public void UpdateNews(bool _force = false)
	{
		foreach (KeyValuePair<string, NewsManager.NewsSource> keyValuePair in this.sources)
		{
			string text;
			NewsManager.NewsSource newsSource;
			keyValuePair.Deconstruct(out text, out newsSource);
			newsSource.RequestData(_force);
		}
	}

	public void RegisterNewsSource(string _uri)
	{
		if (this.sources.ContainsKey(_uri))
		{
			return;
		}
		NewsManager.NewsSource newsSource = NewsManager.NewsSource.FromUri(this, _uri);
		this.sources[_uri] = newsSource;
		newsSource.RequestData(false);
	}

	public void GetNewsData(List<string> _sources, List<NewsManager.NewsEntry> _target)
	{
		_target.Clear();
		foreach (string text in _sources)
		{
			NewsManager.NewsSource newsSource;
			if (this.sources.TryGetValue(text, out newsSource))
			{
				newsSource.GetData(_target);
			}
		}
		this.sortNewsByAge(_target);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void sortNewsByAge(List<NewsManager.NewsEntry> _list)
	{
		_list.Sort((NewsManager.NewsEntry _entryA, NewsManager.NewsEntry _entryB) => _entryB.Date.CompareTo(_entryA.Date));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void notifyListeners()
	{
		Action<NewsManager> updated = this.Updated;
		if (updated == null)
		{
			return;
		}
		updated(this);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static NewsManager instance;

	public static readonly NewsManager.NewsEntry EmptyEntry = new NewsManager.NewsEntry(null, null, null, "- No Entries -", null, "", null, DateTime.Now);

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<string, NewsManager.NewsSource> sources = new CaseInsensitiveStringDictionary<NewsManager.NewsSource>();

	public abstract class NewsSource
	{
		public abstract bool IsCustom { get; }

		[PublicizedFrom(EAccessModifier.Protected)]
		public NewsSource(NewsManager _owner, string _uri)
		{
			this.Owner = _owner;
			this.OrigUri = _uri;
		}

		public void RequestData(bool _force)
		{
			if (this.isUpdating)
			{
				return;
			}
			DateTime now = DateTime.Now;
			NewsManager owner = this.Owner;
			lock (owner)
			{
				if (!_force && this.entries.Count > 0 && (now - this.lastUpdated).TotalMinutes < 1.0)
				{
					return;
				}
			}
			this.lastUpdated = now;
			this.isUpdating = true;
			ThreadManager.StartCoroutine(this.GetDataCo());
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public abstract IEnumerator GetDataCo();

		public void GetData(List<NewsManager.NewsEntry> _target)
		{
			NewsManager owner = this.Owner;
			lock (owner)
			{
				_target.AddRange(this.entries);
			}
		}

		public abstract void RequestImage(string _imageRelPath, Action<Texture2D> _callback);

		[PublicizedFrom(EAccessModifier.Protected)]
		public void LoadXml(XmlFile _xml)
		{
			this.isUpdating = false;
			NewsManager newsManager = this.Owner;
			lock (newsManager)
			{
				this.entries.Clear();
			}
			XElement xelement = ((_xml != null) ? _xml.XmlDoc.Root : null);
			if (xelement == null)
			{
				this.Owner.notifyListeners();
				return;
			}
			string text = xelement.GetAttribute("name").Trim();
			if (text == "")
			{
				text = null;
			}
			newsManager = this.Owner;
			lock (newsManager)
			{
				foreach (XElement xelement2 in xelement.Elements("entry"))
				{
					NewsManager.NewsEntry newsEntry = NewsManager.NewsEntry.FromXml(this, text, xelement2);
					if (newsEntry != null)
					{
						this.entries.Add(newsEntry);
					}
				}
			}
			this.Owner.notifyListeners();
		}

		public static NewsManager.NewsSource FromUri(NewsManager _owner, string _uri)
		{
			if (_uri.StartsWith("rfs://"))
			{
				return new NewsManager.NewsSourceRfs(_owner, _uri);
			}
			return new NewsManager.NewsSourceWww(_owner, _uri);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public const string RemoteFileStorageProtocol = "rfs://";

		public readonly NewsManager Owner;

		[PublicizedFrom(EAccessModifier.Protected)]
		public readonly string OrigUri;

		[PublicizedFrom(EAccessModifier.Protected)]
		public bool isUpdating;

		[PublicizedFrom(EAccessModifier.Protected)]
		public DateTime lastUpdated;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly List<NewsManager.NewsEntry> entries = new List<NewsManager.NewsEntry>();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public class NewsSourceRfs : NewsManager.NewsSource
	{
		public NewsSourceRfs(NewsManager _owner, string _uri)
			: base(_owner, _uri)
		{
			this.rfsFilename = _uri.Substring("rfs://".Length);
		}

		public override bool IsCustom
		{
			get
			{
				return false;
			}
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override IEnumerator GetDataCo()
		{
			IRemoteFileStorage storage = PlatformManager.MultiPlatform.RemoteFileStorage;
			if (storage == null)
			{
				this.isUpdating = false;
				yield break;
			}
			if (PlatformManager.NativePlatform.User.UserStatus != EUserStatus.LoggedIn)
			{
				storage.GetCachedFile(this.rfsFilename, new IRemoteFileStorage.FileDownloadCompleteCallback(this.<GetDataCo>g__fileDownloadedCallback|4_0));
				this.lastUpdated = DateTime.MinValue;
				yield break;
			}
			bool loggedSlow = false;
			float startTime = Time.time;
			while (!storage.IsReady)
			{
				if (storage.Unavailable)
				{
					Log.Warning("Remote Storage is unavailable");
					this.isUpdating = false;
					yield break;
				}
				yield return null;
				if (!loggedSlow && Time.time > startTime + 30f)
				{
					loggedSlow = true;
					Log.Warning("Waiting for news from remote storage exceeded 30s");
				}
			}
			storage.GetFile(this.rfsFilename, new IRemoteFileStorage.FileDownloadCompleteCallback(this.<GetDataCo>g__fileDownloadedCallback|4_0));
			yield break;
		}

		public override void RequestImage(string _imageRelPath, Action<Texture2D> _callback)
		{
			Queue<ValueTuple<string, Action<Texture2D>>> queue = this.requestedImagesQueue;
			lock (queue)
			{
				this.requestedImagesQueue.Enqueue(new ValueTuple<string, Action<Texture2D>>(_imageRelPath, _callback));
			}
			this.startNextImageRequest();
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void startNextImageRequest()
		{
			Queue<ValueTuple<string, Action<Texture2D>>> queue = this.requestedImagesQueue;
			lock (queue)
			{
				ValueTuple<string, Action<Texture2D>> valueTuple;
				if (this.runningImageRequests < 3 && this.requestedImagesQueue.TryDequeue(out valueTuple))
				{
					ThreadManager.StartCoroutine(this.requestFromRemoteStorage(valueTuple.Item1, valueTuple.Item2));
				}
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void imageRequestCompleted()
		{
			Queue<ValueTuple<string, Action<Texture2D>>> queue = this.requestedImagesQueue;
			lock (queue)
			{
				this.runningImageRequests--;
				this.startNextImageRequest();
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public IEnumerator requestFromRemoteStorage(string _imageRelPath, Action<Texture2D> _callback)
		{
			NewsManager.NewsSourceRfs.<>c__DisplayClass11_0 CS$<>8__locals1 = new NewsManager.NewsSourceRfs.<>c__DisplayClass11_0();
			CS$<>8__locals1.<>4__this = this;
			CS$<>8__locals1._imageRelPath = _imageRelPath;
			CS$<>8__locals1._callback = _callback;
			IRemoteFileStorage remoteFileStorage = PlatformManager.MultiPlatform.RemoteFileStorage;
			if (remoteFileStorage == null)
			{
				yield break;
			}
			Queue<ValueTuple<string, Action<Texture2D>>> queue = this.requestedImagesQueue;
			lock (queue)
			{
				this.runningImageRequests++;
			}
			if (remoteFileStorage.Unavailable)
			{
				remoteFileStorage.GetCachedFile(CS$<>8__locals1._imageRelPath, new IRemoteFileStorage.FileDownloadCompleteCallback(CS$<>8__locals1.<requestFromRemoteStorage>g__imageDownloadedCallback|0));
			}
			else
			{
				remoteFileStorage.GetFile(CS$<>8__locals1._imageRelPath, new IRemoteFileStorage.FileDownloadCompleteCallback(CS$<>8__locals1.<requestFromRemoteStorage>g__imageDownloadedCallback|0));
			}
			yield break;
		}

		[CompilerGenerated]
		[PublicizedFrom(EAccessModifier.Private)]
		public void <GetDataCo>g__fileDownloadedCallback|4_0(IRemoteFileStorage.EFileDownloadResult _result, string _errorDetails, byte[] _data)
		{
			if (_result != IRemoteFileStorage.EFileDownloadResult.Ok)
			{
				Log.Warning(string.Concat(new string[]
				{
					"Retrieving remote news file '",
					this.rfsFilename,
					"' failed: ",
					_result.ToStringCached<IRemoteFileStorage.EFileDownloadResult>(),
					" (",
					_errorDetails,
					")"
				}));
				base.LoadXml(null);
				return;
			}
			XmlFile xmlFile = null;
			if (_data != null && _data.Length != 0)
			{
				try
				{
					xmlFile = new XmlFile(_data, true);
				}
				catch (Exception ex)
				{
					Log.Error("Failed loading news XML:");
					Log.Exception(ex);
					return;
				}
			}
			base.LoadXml(xmlFile);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly string rfsFilename;

		[PublicizedFrom(EAccessModifier.Private)]
		public const int MaxParallelImageRequests = 3;

		[TupleElementNames(new string[] { "imageRelPath", "callback" })]
		[PublicizedFrom(EAccessModifier.Private)]
		public readonly Queue<ValueTuple<string, Action<Texture2D>>> requestedImagesQueue = new Queue<ValueTuple<string, Action<Texture2D>>>();

		[PublicizedFrom(EAccessModifier.Private)]
		public int runningImageRequests;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public class NewsSourceWww : NewsManager.NewsSource
	{
		public override bool IsCustom
		{
			get
			{
				return true;
			}
		}

		public NewsSourceWww(NewsManager _owner, string _uri)
			: base(_owner, _uri)
		{
			string text = _uri;
			if (!text.StartsWith("http", StringComparison.Ordinal))
			{
				string text2 = ModManager.PatchModPathString(text);
				if (text2 == null)
				{
					throw new ArgumentException("WWW news source '" + _uri + "' can not be retrieved: Neither is a 'http(s)://' URI nor a '@modfolder:' reference.");
				}
				text = "file://" + text2;
			}
			text = text.Replace("#", "%23").Replace("+", "%2B");
			this.patchedUri = text;
			int num = text.LastIndexOf('/');
			if (num < 0)
			{
				throw new ArgumentException("WWW news source '" + _uri + "' does not have a valid path");
			}
			this.baseUri = text.Substring(0, num + 1);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override IEnumerator GetDataCo()
		{
			NewsManager.NewsSourceWww.<GetDataCo>d__5 <GetDataCo>d__ = new NewsManager.NewsSourceWww.<GetDataCo>d__5(0);
			<GetDataCo>d__.<>4__this = this;
			return <GetDataCo>d__;
		}

		public override void RequestImage(string _imageRelPath, Action<Texture2D> _callback)
		{
			ThreadManager.StartCoroutine(this.requestFromUri(_imageRelPath, _callback));
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public IEnumerator requestFromUri(string _imageRelPath, Action<Texture2D> _callback)
		{
			NewsManager.NewsSourceWww.<requestFromUri>d__7 <requestFromUri>d__ = new NewsManager.NewsSourceWww.<requestFromUri>d__7(0);
			<requestFromUri>d__.<>4__this = this;
			<requestFromUri>d__._imageRelPath = _imageRelPath;
			<requestFromUri>d__._callback = _callback;
			return <requestFromUri>d__;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly string patchedUri;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly string baseUri;
	}

	public class NewsEntry : IEquatable<NewsManager.NewsEntry>
	{
		public bool IsCustom
		{
			get
			{
				return this.owner == null || this.owner.IsCustom;
			}
		}

		public NewsEntry(NewsManager.NewsSource _owner, string _customListName, string _imageRelPath, string _headline, string _headline2, string _text, string _url, DateTime _date)
		{
			this.owner = _owner;
			this.CustomListName = _customListName;
			this.imageRelPath = _imageRelPath;
			this.Headline = _headline;
			this.Headline2 = _headline2;
			this.Text = _text;
			this.Url = _url;
			this.Date = _date;
		}

		public bool HasImage
		{
			get
			{
				return !string.IsNullOrEmpty(this.imageRelPath);
			}
		}

		public bool ImageLoaded
		{
			get
			{
				return this.image != null;
			}
		}

		public Texture2D Image
		{
			get
			{
				return this.image;
			}
		}

		public void RequestImage()
		{
			if (!this.HasImage)
			{
				return;
			}
			if (this.requestedImage)
			{
				return;
			}
			this.requestedImage = true;
			this.owner.RequestImage(this.imageRelPath, new Action<Texture2D>(this.setImage));
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void setImage(Texture2D _image)
		{
			this.image = _image;
			this.image.name = "NewsImage_" + this.imageRelPath;
			this.image.Compress(true);
			this.image.Apply(false, true);
			this.owner.Owner.notifyListeners();
		}

		public bool Equals(NewsManager.NewsEntry _other)
		{
			return _other != null && (this == _other || (this.CustomListName == _other.CustomListName && this.imageRelPath == _other.imageRelPath && this.Headline == _other.Headline && this.Headline2 == _other.Headline2 && this.Text == _other.Text && this.Url == _other.Url && this.Date.Equals(_other.Date)));
		}

		public override bool Equals(object _obj)
		{
			return _obj != null && (this == _obj || (!(_obj.GetType() != base.GetType()) && this.Equals((NewsManager.NewsEntry)_obj)));
		}

		public override int GetHashCode()
		{
			return (((((((((((((this.CustomListName != null) ? this.CustomListName.GetHashCode() : 0) * 397) ^ ((this.imageRelPath != null) ? this.imageRelPath.GetHashCode() : 0)) * 397) ^ ((this.Headline != null) ? this.Headline.GetHashCode() : 0)) * 397) ^ ((this.Headline2 != null) ? this.Headline2.GetHashCode() : 0)) * 397) ^ ((this.Text != null) ? this.Text.GetHashCode() : 0)) * 397) ^ ((this.Url != null) ? this.Url.GetHashCode() : 0)) * 397) ^ this.Date.GetHashCode();
		}

		public static NewsManager.NewsEntry FromXml(NewsManager.NewsSource _owner, string _customListName, XElement _element)
		{
			string text = null;
			string text2 = "";
			string text3 = "";
			string text4 = "";
			string text5 = null;
			DateTime minValue = DateTime.MinValue;
			DateTime maxValue = DateTime.MaxValue;
			bool flag = true;
			foreach (XElement xelement in _element.Elements())
			{
				string localName = xelement.Name.LocalName;
				uint num = <PrivateImplementationDetails>.ComputeStringHash(localName);
				if (num <= 1990630727U)
				{
					if (num <= 561879464U)
					{
						if (num != 232457833U)
						{
							if (num == 561879464U)
							{
								if (localName == "devicetypes")
								{
									bool flag2 = false;
									string[] array = xelement.Value.Split(',', StringSplitOptions.None);
									for (int i = 0; i < array.Length; i++)
									{
										string text6 = array[i].Trim();
										if (!string.IsNullOrEmpty(text6))
										{
											EDeviceType edeviceType;
											if (!EnumUtils.TryParse<EDeviceType>(text6, out edeviceType, true))
											{
												Log.Warning(string.Format("News XML has an entry with an invalid 'devicetypes' element '{0}', devicetype '{1}' unknown at line {2}", xelement.Value, text6, ((IXmlLineInfo)_element).LineNumber));
												return null;
											}
											if (edeviceType == PlatformManager.DeviceType)
											{
												flag2 = true;
												break;
											}
										}
									}
									if (!flag2)
									{
										return null;
									}
									continue;
								}
							}
						}
						else if (localName == "link")
						{
							text5 = xelement.Value.Trim();
							continue;
						}
					}
					else if (num != 589056993U)
					{
						if (num != 1406002643U)
						{
							if (num == 1990630727U)
							{
								if (localName == "showbefore")
								{
									if (!bool.TryParse(xelement.Value, out flag))
									{
										Log.Warning(string.Format("News XML has an entry with an invalid 'showbefore' element '{0}' at line {1}", xelement.Value, ((IXmlLineInfo)_element).LineNumber));
										return null;
									}
									continue;
								}
							}
						}
						else if (localName == "platforms")
						{
							bool flag3 = false;
							string[] array = xelement.Value.Split(',', StringSplitOptions.None);
							for (int i = 0; i < array.Length; i++)
							{
								string text7 = array[i].Trim();
								if (!string.IsNullOrEmpty(text7))
								{
									EPlatformIdentifier eplatformIdentifier;
									if (!EnumUtils.TryParse<EPlatformIdentifier>(text7, out eplatformIdentifier, true))
									{
										Log.Warning(string.Format("News XML has an entry with an invalid 'platforms' element '{0}', platform '{1}' unknown at line {2}", xelement.Value, text7, ((IXmlLineInfo)_element).LineNumber));
										return null;
									}
									if (eplatformIdentifier != PlatformManager.NativePlatform.PlatformIdentifier)
									{
										EPlatformIdentifier eplatformIdentifier2 = eplatformIdentifier;
										IPlatform crossplatformPlatform = PlatformManager.CrossplatformPlatform;
										EPlatformIdentifier? eplatformIdentifier3 = ((crossplatformPlatform != null) ? new EPlatformIdentifier?(crossplatformPlatform.PlatformIdentifier) : null);
										if (!((eplatformIdentifier2 == eplatformIdentifier3.GetValueOrDefault()) & (eplatformIdentifier3 != null)))
										{
											goto IL_03A0;
										}
									}
									flag3 = true;
									break;
								}
								IL_03A0:;
							}
							if (!flag3)
							{
								return null;
							}
							continue;
						}
					}
					else if (localName == "title2")
					{
						text3 = xelement.Value;
						continue;
					}
				}
				else if (num <= 2556802313U)
				{
					if (num != 2428985098U)
					{
						if (num == 2556802313U)
						{
							if (localName == "title")
							{
								text2 = xelement.Value;
								continue;
							}
						}
					}
					else if (localName == "imagerelpath")
					{
						text = xelement.Value;
						continue;
					}
				}
				else if (num != 3185987134U)
				{
					if (num != 3564297305U)
					{
						if (num == 3808839532U)
						{
							if (localName == "showuntil")
							{
								if (!DateTime.TryParseExact(xelement.Value, "u", null, DateTimeStyles.AssumeUniversal, out maxValue))
								{
									Log.Warning(string.Format("News XML has an entry with an invalid 'showuntil' element '{0}' at line {1}", xelement.Value, ((IXmlLineInfo)_element).LineNumber));
									return null;
								}
								continue;
							}
						}
					}
					else if (localName == "date")
					{
						if (!DateTime.TryParseExact(xelement.Value, "u", null, DateTimeStyles.AssumeUniversal, out minValue))
						{
							Log.Warning(string.Format("News XML has an entry with an invalid 'date' element '{0}' at line {1}", xelement.Value, ((IXmlLineInfo)_element).LineNumber));
							return null;
						}
						continue;
					}
				}
				else if (localName == "text")
				{
					text4 = xelement.Value;
					continue;
				}
				Log.Warning(string.Format("News XML has an entry with an unknown element '{0}' at line {1}", xelement.Name.LocalName, ((IXmlLineInfo)_element).LineNumber));
			}
			if (minValue == DateTime.MinValue)
			{
				Log.Warning(string.Format("News XML has an entry without a date element at line {0}", ((IXmlLineInfo)_element).LineNumber));
				return null;
			}
			DateTime now = DateTime.Now;
			if (!flag && minValue > now)
			{
				return null;
			}
			if (maxValue < now)
			{
				return null;
			}
			return new NewsManager.NewsEntry(_owner, _customListName, text, text2, text3, text4, text5, minValue);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly NewsManager.NewsSource owner;

		public readonly string CustomListName;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly string imageRelPath;

		public readonly string Headline;

		public readonly string Headline2;

		public readonly string Text;

		public readonly string Url;

		public readonly DateTime Date;

		[PublicizedFrom(EAccessModifier.Private)]
		public bool requestedImage;

		[PublicizedFrom(EAccessModifier.Private)]
		public Texture2D image;
	}
}
