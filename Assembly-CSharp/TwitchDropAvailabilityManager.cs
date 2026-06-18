using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Platform;
using UnityEngine;
using UnityEngine.Networking;

public sealed class TwitchDropAvailabilityManager
{
	public static TwitchDropAvailabilityManager Instance
	{
		get
		{
			TwitchDropAvailabilityManager twitchDropAvailabilityManager;
			if ((twitchDropAvailabilityManager = TwitchDropAvailabilityManager._instance) == null)
			{
				twitchDropAvailabilityManager = (TwitchDropAvailabilityManager._instance = new TwitchDropAvailabilityManager());
			}
			return twitchDropAvailabilityManager;
		}
	}

	public event Action<TwitchDropAvailabilityManager> Updated;

	[PublicizedFrom(EAccessModifier.Private)]
	public TwitchDropAvailabilityManager()
	{
	}

	public void RegisterSource(string uri)
	{
		if (this._sources.ContainsKey(uri))
		{
			return;
		}
		TwitchDropAvailabilityManager.DropSource dropSource = TwitchDropAvailabilityManager.DropSource.FromUri(this, uri);
		this._sources[uri] = dropSource;
		dropSource.RequestData(false);
	}

	public void UpdateAll(bool force = false)
	{
		foreach (KeyValuePair<string, TwitchDropAvailabilityManager.DropSource> keyValuePair in this._sources)
		{
			keyValuePair.Value.RequestData(force);
		}
	}

	public void GetEntries(List<string> sourceUris, List<TwitchDropAvailabilityManager.TwitchDropEntry> target)
	{
		target.Clear();
		foreach (string text in sourceUris)
		{
			TwitchDropAvailabilityManager.DropSource dropSource;
			if (this._sources.TryGetValue(text, out dropSource))
			{
				dropSource.GetData(target);
			}
		}
		target.Sort((TwitchDropAvailabilityManager.TwitchDropEntry a, TwitchDropAvailabilityManager.TwitchDropEntry b) => b.Start.CompareTo(a.Start));
	}

	public TwitchDropAvailabilityManager.TwitchDropEntry GetLatestForBenefit(string benefitId, List<string> sourceUris)
	{
		List<TwitchDropAvailabilityManager.TwitchDropEntry> list = new List<TwitchDropAvailabilityManager.TwitchDropEntry>();
		this.GetEntries(sourceUris, list);
		for (int i = 0; i < list.Count; i++)
		{
			if (string.Equals(list[i].BenefitId, benefitId, StringComparison.OrdinalIgnoreCase))
			{
				return list[i];
			}
		}
		return null;
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public void Notify()
	{
		Action<TwitchDropAvailabilityManager> updated = this.Updated;
		if (updated == null)
		{
			return;
		}
		updated(this);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static List<TwitchDropAvailabilityManager.TwitchDropEntry> ParseEntries(byte[] bytes)
	{
		List<TwitchDropAvailabilityManager.TwitchDropEntry> list = new List<TwitchDropAvailabilityManager.TwitchDropEntry>();
		if (bytes == null || bytes.Length == 0)
		{
			return list;
		}
		string @string = Encoding.UTF8.GetString(bytes, 0, bytes.Length);
		XDocument xdocument;
		try
		{
			xdocument = XDocument.Parse(@string, LoadOptions.SetLineInfo);
		}
		catch (Exception ex)
		{
			Log.Error("TwitchDrop XML parse failed:");
			Log.Exception(ex);
			return list;
		}
		XElement root = xdocument.Root;
		if (root == null)
		{
			return list;
		}
		if (!string.Equals(root.Name.LocalName, "drops", StringComparison.OrdinalIgnoreCase))
		{
			IXmlLineInfo xmlLineInfo = root;
			Log.Warning(string.Format("TwitchDrop XML unexpected root <{0}> at line {1}, expected <drops>", root.Name.LocalName, xmlLineInfo.LineNumber));
			return list;
		}
		HashSet<string> hashSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		foreach (XElement xelement in root.Elements())
		{
			TwitchDropAvailabilityManager.TwitchDropEntry twitchDropEntry = TwitchDropAvailabilityManager.FromXmlBenefit(xelement);
			if (twitchDropEntry != null)
			{
				if (hashSet.Contains(twitchDropEntry.BenefitId))
				{
					IXmlLineInfo xmlLineInfo2 = xelement;
					Log.Warning(string.Format("TwitchDrop XML duplicate id '{0}' at line {1}; skipping duplicate.", twitchDropEntry.BenefitId, xmlLineInfo2.LineNumber));
				}
				else
				{
					hashSet.Add(twitchDropEntry.BenefitId);
					list.Add(twitchDropEntry);
				}
			}
		}
		return list;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static TwitchDropAvailabilityManager.TwitchDropEntry FromXmlBenefit(XElement element)
	{
		if (!string.Equals(element.Name.LocalName, "benefit", StringComparison.OrdinalIgnoreCase))
		{
			Log.Warning(string.Format("TwitchDrop XML unknown node <{0}> at line {1}", element.Name.LocalName, ((IXmlLineInfo)element).LineNumber));
			return null;
		}
		XAttribute xattribute = element.Attribute("id");
		XAttribute xattribute2 = element.Attribute("entitlementset");
		XAttribute xattribute3 = element.Attribute("start");
		if (xattribute == null || xattribute3 == null)
		{
			Log.Warning(string.Format("TwitchDrop XML missing required attributes on <benefit> at line {0} (need id,start)", ((IXmlLineInfo)element).LineNumber));
			return null;
		}
		string text = (xattribute.Value ?? string.Empty).Trim();
		if (text.Length == 0)
		{
			Log.Warning(string.Format("TwitchDrop XML empty id at line {0}", ((IXmlLineInfo)element).LineNumber));
			return null;
		}
		DateTime dateTime;
		if (!DateTime.TryParseExact(xattribute3.Value, "u", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out dateTime))
		{
			Log.Warning(string.Format("TwitchDrop XML invalid start '{0}' at line {1}", xattribute3.Value, ((IXmlLineInfo)element).LineNumber));
			return null;
		}
		dateTime = DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
		EntitlementSetEnum entitlementSetEnum = EntitlementSetEnum.None;
		if (xattribute2 != null)
		{
			EntitlementSetEnum entitlementSetEnum2;
			if (!Enum.TryParse<EntitlementSetEnum>(xattribute2.Value.Trim(), true, out entitlementSetEnum2))
			{
				Log.Warning(string.Format("TwitchDrop XML invalid entitlementset '{0}' for id '{1}' at line {2}", xattribute2.Value, text, ((IXmlLineInfo)element).LineNumber));
				return null;
			}
			entitlementSetEnum = entitlementSetEnum2;
		}
		return new TwitchDropAvailabilityManager.TwitchDropEntry(text, dateTime, entitlementSetEnum);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static TwitchDropAvailabilityManager _instance;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<string, TwitchDropAvailabilityManager.DropSource> _sources = new CaseInsensitiveStringDictionary<TwitchDropAvailabilityManager.DropSource>();

	public sealed class TwitchDropEntry : IEquatable<TwitchDropAvailabilityManager.TwitchDropEntry>
	{
		public TwitchDropEntry(string benefitId, DateTime startUtc, EntitlementSetEnum entitlementSet)
		{
			this.BenefitId = benefitId;
			this.Start = startUtc;
			this.EntitlementSet = entitlementSet;
		}

		public bool IsAvailable(DateTime localNow)
		{
			return localNow.ToUniversalTime() >= this.Start;
		}

		public bool Equals(TwitchDropAvailabilityManager.TwitchDropEntry other)
		{
			return other != null && (this == other || (string.Equals(this.BenefitId, other.BenefitId, StringComparison.OrdinalIgnoreCase) && this.Start.Equals(other.Start) && this.EntitlementSet == other.EntitlementSet));
		}

		public override bool Equals(object obj)
		{
			TwitchDropAvailabilityManager.TwitchDropEntry twitchDropEntry = obj as TwitchDropAvailabilityManager.TwitchDropEntry;
			return twitchDropEntry != null && this.Equals(twitchDropEntry);
		}

		public override int GetHashCode()
		{
			return (((StringComparer.OrdinalIgnoreCase.GetHashCode(this.BenefitId ?? string.Empty) * 397) ^ this.Start.GetHashCode()) * 397) ^ (int)this.EntitlementSet;
		}

		public readonly string BenefitId;

		public readonly DateTime Start;

		public readonly EntitlementSetEnum EntitlementSet;
	}

	public abstract class DropSource
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		public DropSource(TwitchDropAvailabilityManager owner, string uri)
		{
			this.Owner = owner;
			this.OrigUri = uri;
		}

		public void RequestData(bool force)
		{
			if (this._isUpdating)
			{
				return;
			}
			DateTime now = DateTime.Now;
			TwitchDropAvailabilityManager owner = this.Owner;
			lock (owner)
			{
				if (!force && this._entries.Count > 0 && (now - this._lastUpdated).TotalMinutes < 1.0)
				{
					return;
				}
			}
			this._lastUpdated = now;
			this._isUpdating = true;
			ThreadManager.StartCoroutine(this.GetDataCo());
		}

		public void GetData(List<TwitchDropAvailabilityManager.TwitchDropEntry> target)
		{
			TwitchDropAvailabilityManager owner = this.Owner;
			lock (owner)
			{
				target.AddRange(this._entries);
			}
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public void ApplyBytes(byte[] bytes)
		{
			this._isUpdating = false;
			List<TwitchDropAvailabilityManager.TwitchDropEntry> list = TwitchDropAvailabilityManager.ParseEntries(bytes);
			TwitchDropAvailabilityManager owner = this.Owner;
			lock (owner)
			{
				this._entries.Clear();
				this._entries.AddRange(list);
			}
			this.Owner.Notify();
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public abstract IEnumerator GetDataCo();

		public static TwitchDropAvailabilityManager.DropSource FromUri(TwitchDropAvailabilityManager owner, string uri)
		{
			if (uri.StartsWith("rfs://", StringComparison.OrdinalIgnoreCase))
			{
				return new TwitchDropAvailabilityManager.DropSourceRfs(owner, uri);
			}
			return new TwitchDropAvailabilityManager.DropSourceWww(owner, uri);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public const string RfsScheme = "rfs://";

		[PublicizedFrom(EAccessModifier.Protected)]
		public readonly TwitchDropAvailabilityManager Owner;

		[PublicizedFrom(EAccessModifier.Protected)]
		public readonly string OrigUri;

		[PublicizedFrom(EAccessModifier.Protected)]
		public bool _isUpdating;

		[PublicizedFrom(EAccessModifier.Protected)]
		public DateTime _lastUpdated;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly List<TwitchDropAvailabilityManager.TwitchDropEntry> _entries = new List<TwitchDropAvailabilityManager.TwitchDropEntry>();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public sealed class DropSourceRfs : TwitchDropAvailabilityManager.DropSource
	{
		public DropSourceRfs(TwitchDropAvailabilityManager owner, string uri)
			: base(owner, uri)
		{
			this._rfsKey = uri.Substring("rfs://".Length);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override IEnumerator GetDataCo()
		{
			IRemoteFileStorage storage = PlatformManager.MultiPlatform.RemoteFileStorage;
			if (storage == null)
			{
				this._isUpdating = false;
				yield break;
			}
			if (PlatformManager.NativePlatform.User.UserStatus != EUserStatus.LoggedIn)
			{
				storage.GetCachedFile(this._rfsKey, new IRemoteFileStorage.FileDownloadCompleteCallback(this.<GetDataCo>g__Callback|2_0));
				this._lastUpdated = DateTime.MinValue;
				yield break;
			}
			bool warned = false;
			float startTime = Time.time;
			while (!storage.IsReady)
			{
				if (storage.Unavailable)
				{
					Log.Warning("Remote Storage is unavailable");
					this._isUpdating = false;
					yield break;
				}
				yield return null;
				if (!warned && Time.time > startTime + 30f)
				{
					warned = true;
					Log.Warning("Waiting for drop XML from remote storage exceeded 30s");
				}
			}
			storage.GetFile(this._rfsKey, new IRemoteFileStorage.FileDownloadCompleteCallback(this.<GetDataCo>g__Callback|2_0));
			yield break;
		}

		[CompilerGenerated]
		[PublicizedFrom(EAccessModifier.Private)]
		public void <GetDataCo>g__Callback|2_0(IRemoteFileStorage.EFileDownloadResult result, string error, byte[] data)
		{
			if (result != IRemoteFileStorage.EFileDownloadResult.Ok)
			{
				Log.Warning(string.Concat(new string[]
				{
					"Retrieving drop XML '",
					this._rfsKey,
					"' failed: ",
					result.ToStringCached<IRemoteFileStorage.EFileDownloadResult>(),
					" (",
					error,
					")"
				}));
				base.ApplyBytes(null);
				return;
			}
			base.ApplyBytes(data);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly string _rfsKey;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public sealed class DropSourceWww : TwitchDropAvailabilityManager.DropSource
	{
		public DropSourceWww(TwitchDropAvailabilityManager owner, string uri)
			: base(owner, uri)
		{
			string text = uri;
			if (!text.StartsWith("http", StringComparison.Ordinal))
			{
				string text2 = ModManager.PatchModPathString(text);
				if (text2 == null)
				{
					throw new ArgumentException("Drop source '" + uri + "' cannot be retrieved: Not http(s) and not '@modfolder:'");
				}
				text = "file://" + text2;
			}
			text = text.Replace("#", "%23").Replace("+", "%2B");
			this._patchedUri = text;
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override IEnumerator GetDataCo()
		{
			UnityWebRequest req = UnityWebRequest.Get(this._patchedUri);
			yield return req.SendWebRequest();
			if (req.result == 1)
			{
				string text = req.downloadHandler.text ?? string.Empty;
				byte[] bytes = Encoding.UTF8.GetBytes(text);
				base.ApplyBytes(bytes);
			}
			else
			{
				Log.Warning("Retrieving drop XML from '" + this.OrigUri + "' failed: " + req.error);
				base.ApplyBytes(null);
			}
			yield break;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly string _patchedUri;
	}
}
