using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class AssetBundleManager
{
	public static AssetBundleManager Instance
	{
		get
		{
			AssetBundleManager assetBundleManager;
			if ((assetBundleManager = AssetBundleManager.instance) == null)
			{
				assetBundleManager = (AssetBundleManager.instance = new AssetBundleManager());
			}
			return assetBundleManager;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public AssetBundleManager()
	{
	}

	public void LoadAssetBundle(string _name, bool _forceBundle = false)
	{
		string text;
		if (Path.IsPathRooted(_name))
		{
			text = _name;
		}
		else
		{
			text = string.Concat(new string[]
			{
				GameIO.GetApplicationPath(),
				"/Data/Bundles/Standalone",
				BundleTags.Tag,
				"/",
				_name
			});
		}
		string text2 = _name + 1.ToString();
		if (!this.dictAssetBundleRefs.ContainsKey(text2))
		{
			string directoryName = Path.GetDirectoryName(text);
			if (!Directory.Exists(directoryName))
			{
				Log.Error("Loading AssetBundle \"" + text + "\" failed: Parent folder not found!");
				return;
			}
			string fileName = Path.GetFileName(text);
			text = null;
			foreach (string text3 in Directory.EnumerateFiles(directoryName))
			{
				if (Path.GetFileName(text3).EqualsCaseInsensitive(fileName))
				{
					text = text3;
					break;
				}
			}
			if (text == null)
			{
				Log.Error("Loading AssetBundle \"" + fileName + "\" failed: File not found!");
				return;
			}
			AssetBundle assetBundle = AssetBundle.LoadFromFile(text);
			if (assetBundle == null)
			{
				Log.Error("Loading AssetBundle \"" + text + "\" failed!");
				return;
			}
			AssetBundleManager.AssetBundleRef assetBundleRef = new AssetBundleManager.AssetBundleRef(text, 1);
			assetBundleRef.assetBundle = assetBundle;
			this.dictAssetBundleRefs.Add(text2, assetBundleRef);
		}
	}

	public T Get<T>(string _bundleName, string _objName, bool _forceBundle = false) where T : Object
	{
		return this._get<T>(_bundleName, _objName, _forceBundle, false);
	}

	public T Get<T>(DataLoader.DataPathIdentifier _dpi, bool _useRelativePath, bool _forceBundle = false) where T : Object
	{
		return this._get<T>(_dpi.BundlePath, _dpi.AssetName, _forceBundle, _useRelativePath);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public T _get<T>(string _bundleName, string _objName, bool _forceBundle = false, bool _useRelativePath = false) where T : Object
	{
		string text = _bundleName + 1.ToString();
		AssetBundleManager.AssetBundleRef assetBundleRef;
		if (!this.dictAssetBundleRefs.TryGetValue(text, out assetBundleRef))
		{
			return default(T);
		}
		if (!_useRelativePath)
		{
			if (_objName.IndexOf('/') > 0)
			{
				_objName = _objName.Substring(_objName.LastIndexOf('/') + 1);
			}
			_objName = GameIO.RemoveFileExtension(_objName);
		}
		return assetBundleRef.assetBundle.LoadAsset<T>(_objName);
	}

	public AssetBundleManager.AssetBundleRequestTFP GetAsync<T>(string _bundleName, string _objName, bool _forceBundle = false) where T : Object
	{
		string text = _bundleName + 1.ToString();
		AssetBundleManager.AssetBundleRef assetBundleRef;
		if (!this.dictAssetBundleRefs.TryGetValue(text, out assetBundleRef))
		{
			return null;
		}
		if (_objName.IndexOf('/') > 0)
		{
			_objName = _objName.Substring(_objName.LastIndexOf('/') + 1);
		}
		return new AssetBundleManager.AssetBundleRequestTFP(assetBundleRef.assetBundle.LoadAssetAsync<T>(GameIO.RemoveFileExtension(_objName)));
	}

	public bool Contains(string _bundleName, string _objName, bool _forceBundle = false)
	{
		string text = _bundleName + 1.ToString();
		AssetBundleManager.AssetBundleRef assetBundleRef;
		if (!this.dictAssetBundleRefs.TryGetValue(text, out assetBundleRef))
		{
			return false;
		}
		if (_objName.IndexOf('/') > 0)
		{
			_objName = _objName.Substring(_objName.LastIndexOf('/') + 1);
		}
		return assetBundleRef.assetBundle.Contains(GameIO.RemoveFileExtension(_objName));
	}

	public T[] GetAllObjects<T>(string _bundleName, string _subpath = null, bool _forceBundle = false) where T : Object
	{
		string text = _bundleName + 1.ToString();
		AssetBundleManager.AssetBundleRef assetBundleRef;
		if (!this.dictAssetBundleRefs.TryGetValue(text, out assetBundleRef))
		{
			return null;
		}
		return assetBundleRef.assetBundle.LoadAllAssets<T>();
	}

	public AssetBundleManager.AssetBundleMassRequestTFP GetAllObjectsAsync<T>(string _bundleName, string _subpath = null, bool _forceBundle = false) where T : Object
	{
		string text = _bundleName + 1.ToString();
		AssetBundleManager.AssetBundleRef assetBundleRef;
		if (!this.dictAssetBundleRefs.TryGetValue(text, out assetBundleRef))
		{
			return null;
		}
		return new AssetBundleManager.AssetBundleMassRequestTFP(assetBundleRef.assetBundle.LoadAllAssetsAsync<T>());
	}

	public string[] GetAllAssetNames(string _bundleName, bool _forceBundle = false)
	{
		string text = _bundleName + 1.ToString();
		AssetBundleManager.AssetBundleRef assetBundleRef;
		if (!this.dictAssetBundleRefs.TryGetValue(text, out assetBundleRef))
		{
			return null;
		}
		return assetBundleRef.assetBundle.GetAllAssetNames();
	}

	public void Unload(string _name, bool _forceBundle = false)
	{
		string text = _name + 1.ToString();
		AssetBundleManager.AssetBundleRef assetBundleRef;
		if (this.dictAssetBundleRefs.TryGetValue(text, out assetBundleRef))
		{
			assetBundleRef.assetBundle.Unload(true);
			assetBundleRef.assetBundle = null;
			this.dictAssetBundleRefs.Remove(text);
		}
	}

	public void UnloadAll(bool _forceBundle = false)
	{
		foreach (string text in this.dictAssetBundleRefs.Keys)
		{
			this.dictAssetBundleRefs[text].assetBundle.Unload(true);
			this.dictAssetBundleRefs[text].assetBundle = null;
		}
		this.dictAssetBundleRefs.Clear();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static AssetBundleManager instance;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int version = 1;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<string, AssetBundleManager.AssetBundleRef> dictAssetBundleRefs = new CaseInsensitiveStringDictionary<AssetBundleManager.AssetBundleRef>();

	public class AssetBundleRequestTFP : CustomYieldInstruction
	{
		public Object Asset
		{
			get
			{
				if (!this.IsBundleLoad)
				{
					return this.asset;
				}
				return this.request.asset;
			}
		}

		public bool IsDone
		{
			get
			{
				return !this.IsBundleLoad || this.request.isDone;
			}
		}

		public override bool keepWaiting
		{
			get
			{
				return this.IsBundleLoad && !this.request.isDone;
			}
		}

		public bool IsBundleLoad
		{
			[PublicizedFrom(EAccessModifier.Private)]
			get
			{
				return this.request != null;
			}
		}

		public AssetBundleRequestTFP(Object _asset)
		{
			this.asset = _asset;
		}

		public AssetBundleRequestTFP(AssetBundleRequest _request)
		{
			this.request = _request;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly Object asset;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly AssetBundleRequest request;
	}

	public class AssetBundleMassRequestTFP : CustomYieldInstruction
	{
		public Object[] Assets
		{
			get
			{
				if (!this.IsBundleLoad)
				{
					return this.assets;
				}
				return this.request.allAssets;
			}
		}

		public bool IsDone
		{
			get
			{
				return !this.IsBundleLoad || this.request.isDone;
			}
		}

		public override bool keepWaiting
		{
			get
			{
				return this.IsBundleLoad && !this.request.isDone;
			}
		}

		public bool IsBundleLoad
		{
			[PublicizedFrom(EAccessModifier.Private)]
			get
			{
				return this.request != null;
			}
		}

		public AssetBundleMassRequestTFP(List<Object> _assets)
		{
			this.assets = _assets.ToArray();
		}

		public AssetBundleMassRequestTFP(AssetBundleRequest _request)
		{
			this.request = _request;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly Object[] assets;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly AssetBundleRequest request;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public class AssetBundleRef
	{
		public AssetBundleRef(string _url, int _version)
		{
			this.url = _url;
			this.version = _version;
		}

		public AssetBundle assetBundle;

		public int version;

		public string url;
	}
}
