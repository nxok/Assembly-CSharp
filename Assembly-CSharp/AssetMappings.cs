using System;
using System.Collections.Generic;
using UnityEngine;

public class AssetMappings
{
	public int Count
	{
		get
		{
			return this.list.Count;
		}
	}

	public void Add(string name, string address)
	{
		this.list.Add(new AssetMappings.AssetAddress
		{
			name = name,
			address = address
		});
	}

	public Dictionary<string, string> ToDictionary()
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		foreach (AssetMappings.AssetAddress assetAddress in this.list)
		{
			dictionary.Add(assetAddress.name, assetAddress.address);
		}
		return dictionary;
	}

	[SerializeField]
	[PublicizedFrom(EAccessModifier.Private)]
	public List<AssetMappings.AssetAddress> list = new List<AssetMappings.AssetAddress>();

	[Serializable]
	public class AssetAddress
	{
		public string name;

		public string address;
	}
}
