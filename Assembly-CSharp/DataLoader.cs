using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.AddressableAssets;

public static class DataLoader
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsInResources(string _uri)
	{
		return _uri.IndexOf('#') < 0 && _uri.IndexOf('@') < 0;
	}

	public static DataLoader.DataPathIdentifier ParseDataPathIdentifier(string _inputUri)
	{
		if (_inputUri == null)
		{
			return new DataLoader.DataPathIdentifier(null, DataLoader.DataPathIdentifier.AssetLocation.Resources, false);
		}
		string text = ModManager.PatchModPathString(_inputUri);
		if (text != null)
		{
			_inputUri = text;
		}
		if (_inputUri.IndexOf('#') == 0 && _inputUri.IndexOf('?') > 0)
		{
			int num = _inputUri.IndexOf('?');
			string text2 = _inputUri.Substring(1, num - 1);
			_inputUri = _inputUri.Substring(num + 1);
			return new DataLoader.DataPathIdentifier(_inputUri, text2, text != null);
		}
		if (_inputUri.IndexOf("@:") == 0)
		{
			return new DataLoader.DataPathIdentifier(_inputUri.Substring(2), DataLoader.DataPathIdentifier.AssetLocation.Addressable, text != null);
		}
		return new DataLoader.DataPathIdentifier(_inputUri, DataLoader.DataPathIdentifier.AssetLocation.Resources, false);
	}

	public static T LoadAsset<T>(DataLoader.DataPathIdentifier _identifier, bool _ignoreDlcEntitlements = false) where T : Object
	{
		return LoadManager.LoadAsset<T>(_identifier, null, null, false, true, _ignoreDlcEntitlements).Asset;
	}

	public static T LoadAsset<T>(string _uri, bool _ignoreDlcEntitlements = false) where T : Object
	{
		return DataLoader.LoadAsset<T>(DataLoader.ParseDataPathIdentifier(_uri), _ignoreDlcEntitlements);
	}

	public static T LoadAsset<T>(AssetReference assetReference, bool _ignoreDlcEntitlements = false) where T : Object
	{
		return LoadManager.LoadAssetFromAddressables<T>(assetReference, null, null, false, true, _ignoreDlcEntitlements).Asset;
	}

	public static void UnloadAsset(DataLoader.DataPathIdentifier _srcIdentifier, Object _obj)
	{
		if (_srcIdentifier.IsBundle)
		{
			Resources.UnloadUnusedAssets();
			return;
		}
		Resources.UnloadAsset(_obj);
		if (_srcIdentifier.Location == DataLoader.DataPathIdentifier.AssetLocation.Addressable)
		{
			LoadManager.ReleaseAddressable<Object>(_obj);
		}
	}

	public static void UnloadAsset(string _uri, Object _obj)
	{
		DataLoader.UnloadAsset(DataLoader.ParseDataPathIdentifier(_uri), _obj);
	}

	public static void PreloadBundle(DataLoader.DataPathIdentifier _identifier)
	{
		if (_identifier.IsBundle)
		{
			AssetBundleManager.Instance.LoadAssetBundle(_identifier.BundlePath, _identifier.FromMod);
		}
	}

	public static void PreloadBundle(string _uri)
	{
		DataLoader.PreloadBundle(DataLoader.ParseDataPathIdentifier(_uri));
	}

	public struct DataPathIdentifier
	{
		public bool IsBundle
		{
			get
			{
				return this.Location == DataLoader.DataPathIdentifier.AssetLocation.Bundle;
			}
		}

		public DataPathIdentifier(string _assetName, DataLoader.DataPathIdentifier.AssetLocation _location = DataLoader.DataPathIdentifier.AssetLocation.Resources, bool _fromMod = false)
		{
			this.BundlePath = null;
			this.AssetName = _assetName;
			this.Location = _location;
			this.FromMod = _fromMod;
		}

		public DataPathIdentifier(string _assetName, string _bundlePath, bool _fromMod = false)
		{
			this.BundlePath = _bundlePath;
			this.AssetName = _assetName;
			this.Location = DataLoader.DataPathIdentifier.AssetLocation.Bundle;
			this.FromMod = _fromMod;
		}

		public readonly DataLoader.DataPathIdentifier.AssetLocation Location;

		public readonly string BundlePath;

		public readonly string AssetName;

		public readonly bool FromMod;

		public enum AssetLocation
		{
			Resources,
			Bundle,
			Addressable
		}
	}
}
