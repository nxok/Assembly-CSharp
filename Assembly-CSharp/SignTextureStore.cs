using System;
using System.Collections.Generic;
using System.Text;
using Unity.Profiling;
using UnityEngine;

public sealed class SignTextureStore
{
	public void RebuildPools(IReadOnlyList<SignTextureManager.TierSpec> tiers)
	{
		using (SignTextureStore.s_SignTextureManagerRebuildPools.Auto())
		{
			this.Clear();
			this._pools.Clear();
			long num = 0L;
			for (int i = 0; i < tiers.Count; i++)
			{
				SignTextureManager.TierSpec tierSpec = tiers[i];
				SignTextureStore.Pool pool = new SignTextureStore.Pool();
				pool.Resolution = tierSpec.Resolution;
				this._pools.Add(pool);
				int totalCount = tierSpec.TotalCount;
				for (int j = 0; j < totalCount; j++)
				{
					RenderTexture renderTexture = SignTextureStore.CreateRT(tierSpec.Resolution, tierSpec.Resolution, "SignTech_Pool");
					this.EnqueueFreeRT(i, renderTexture);
					num += (long)(tierSpec.Resolution * tierSpec.Resolution * 4);
				}
			}
			SignTextureStore.s_TotalTextureMemory.Value = num;
		}
	}

	public void BeginFrameMarkAllNotInUse()
	{
		using (SignTextureStore.s_SignTextureManagerMarkUnused.Auto())
		{
			foreach (KeyValuePair<GlobalSignId, SignTextureStore.Entry> keyValuePair in this._entries)
			{
				keyValuePair.Value.InUse.Clear();
			}
		}
	}

	public bool Has(GlobalSignId signId, int tier)
	{
		bool flag;
		using (SignTextureStore.s_SignTextureManagerHas.Auto())
		{
			SignTextureStore.Entry entry;
			if (this._entries.TryGetValue(signId, out entry))
			{
				flag = entry.TierToRT.ContainsKey(tier);
			}
			else
			{
				flag = false;
			}
		}
		return flag;
	}

	public void MarkInUse(GlobalSignId signId, int tier)
	{
		using (SignTextureStore.s_SignTextureManagerMarkUsed.Auto())
		{
			SignTextureStore.Entry entry;
			if (this._entries.TryGetValue(signId, out entry))
			{
				entry.InUse.Add(tier);
				RenderTexture renderTexture;
				if (entry.TierToRT.TryGetValue(tier, out renderTexture) && !(renderTexture == null))
				{
					if (tier < this._pools.Count)
					{
						this._pools[tier].TryRemove(renderTexture);
					}
				}
			}
		}
	}

	public RenderTexture Get(GlobalSignId signId, int tier)
	{
		RenderTexture renderTexture2;
		using (SignTextureStore.s_SignTextureManagerGet.Auto())
		{
			SignTextureStore.Entry entry;
			RenderTexture renderTexture;
			if (this._entries.TryGetValue(signId, out entry) && entry.TierToRT.TryGetValue(tier, out renderTexture))
			{
				renderTexture2 = renderTexture;
			}
			else
			{
				renderTexture2 = null;
			}
		}
		return renderTexture2;
	}

	public RenderTexture GetBestUpToTier(GlobalSignId signId, int maxTier)
	{
		RenderTexture renderTexture;
		using (SignTextureStore.s_SignTextureManagerGetBestUpToTier.Auto())
		{
			SignTextureStore.Entry entry;
			if (!this._entries.TryGetValue(signId, out entry))
			{
				renderTexture = null;
			}
			else
			{
				int num = -1;
				RenderTexture renderTexture2 = null;
				foreach (KeyValuePair<int, RenderTexture> keyValuePair in entry.TierToRT)
				{
					int key = keyValuePair.Key;
					if (key <= maxTier && key > num)
					{
						num = key;
						renderTexture2 = keyValuePair.Value;
					}
				}
				renderTexture = renderTexture2;
			}
		}
		return renderTexture;
	}

	public RenderTexture AcquireForBake(int tier)
	{
		RenderTexture renderTexture;
		using (SignTextureStore.s_SignTextureManagerAcquireForBake.Auto())
		{
			RenderTexture renderTexture2;
			if (tier >= this._pools.Count)
			{
				renderTexture = null;
			}
			else if (!this._pools[tier].TryUnpool(out renderTexture2))
			{
				renderTexture = null;
			}
			else
			{
				SignTextureStore.RtOwner rtOwner;
				if (this._ownerByRT.TryGetValue(renderTexture2, out rtOwner))
				{
					SignTextureStore.Entry entry;
					RenderTexture renderTexture3;
					if (this._entries.TryGetValue(rtOwner.SignId, out entry) && entry.TierToRT.TryGetValue(rtOwner.Tier, out renderTexture3) && renderTexture3 == renderTexture2)
					{
						entry.TierToRT.Remove(rtOwner.Tier);
					}
					this._ownerByRT.Remove(renderTexture2);
				}
				renderTexture = renderTexture2;
			}
		}
		return renderTexture;
	}

	public void SetBaked(GlobalSignId signId, int tier, RenderTexture rt)
	{
		using (SignTextureStore.s_SignTextureManagerSetBaked.Auto())
		{
			if (!(rt == null))
			{
				SignTextureStore.Entry entry;
				if (!this._entries.TryGetValue(signId, out entry))
				{
					entry = new SignTextureStore.Entry();
					this._entries.Add(signId, entry);
				}
				entry.TierToRT[tier] = rt;
				entry.InUse.Add(tier);
				this._ownerByRT[rt] = new SignTextureStore.RtOwner(signId, tier);
			}
		}
	}

	public void InvalidateSign(GlobalSignId signId)
	{
		using (SignTextureStore.s_SignTextureManagerInvalidateSign.Auto())
		{
			SignTextureStore.Entry entry;
			if (this._entries.TryGetValue(signId, out entry))
			{
				foreach (KeyValuePair<int, RenderTexture> keyValuePair in entry.TierToRT)
				{
					int key = keyValuePair.Key;
					RenderTexture value = keyValuePair.Value;
					if (!(value == null))
					{
						if (key < this._pools.Count)
						{
							this._pools[key].TryRemove(value);
						}
						this._ownerByRT.Remove(value);
						this.EnqueueFreeRT(key, value);
					}
				}
				this._entries.Remove(signId);
			}
		}
	}

	public void EvictUnused()
	{
		using (SignTextureStore.s_SignTextureManagerEvictUnused.Auto())
		{
			foreach (KeyValuePair<GlobalSignId, SignTextureStore.Entry> keyValuePair in this._entries)
			{
				SignTextureStore.Entry value = keyValuePair.Value;
				foreach (KeyValuePair<int, RenderTexture> keyValuePair2 in value.TierToRT)
				{
					int key = keyValuePair2.Key;
					RenderTexture value2 = keyValuePair2.Value;
					if (value2 != null && !value.InUse.Contains(key))
					{
						this.EnqueueFreeRT(key, value2);
					}
				}
			}
		}
	}

	public void Clear()
	{
		using (SignTextureStore.s_SignTextureManagerClear.Auto())
		{
			foreach (KeyValuePair<GlobalSignId, SignTextureStore.Entry> keyValuePair in this._entries)
			{
				foreach (KeyValuePair<int, RenderTexture> keyValuePair2 in keyValuePair.Value.TierToRT)
				{
					this.ReleaseToPool(keyValuePair2.Key, keyValuePair2.Value);
				}
			}
			this._entries.Clear();
			this._ownerByRT.Clear();
			for (int i = 0; i < this._pools.Count; i++)
			{
				this._pools[i].DestroyTexturesAndClear();
			}
			this._pools.Clear();
			this._tiersToRemove.Clear();
			this._signsToRemove.Clear();
			this._bestTierScratch.Clear();
			SignTextureStore.s_TotalTextureMemory.Value = 0L;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ReleaseToPool(int tier, RenderTexture rt)
	{
		using (SignTextureStore.s_SignTextureManagerReleaseToPool.Auto())
		{
			if (!(rt == null))
			{
				if (tier >= this._pools.Count)
				{
					rt.Release();
					Object.DestroyImmediate(rt);
				}
				else
				{
					this.EnqueueFreeRT(tier, rt);
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void EnqueueFreeRT(int tier, RenderTexture rt)
	{
		using (SignTextureStore.s_SignTextureManagerEnqueueFreeRT.Auto())
		{
			if (!(rt == null))
			{
				if (tier >= this._pools.Count)
				{
					rt.Release();
					Object.DestroyImmediate(rt);
				}
				else
				{
					this._pools[tier].Add(rt);
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static RenderTexture CreateRT(int w, int h, string name)
	{
		RenderTexture renderTexture2;
		using (SignTextureStore.s_SignTextureManagerCreateRT.Auto())
		{
			RenderTexture renderTexture = new RenderTexture(w, h, 0, 0, 2);
			renderTexture.name = name;
			renderTexture.useMipMap = false;
			renderTexture.autoGenerateMips = false;
			renderTexture.wrapMode = 1;
			renderTexture.filterMode = 1;
			renderTexture.antiAliasing = 1;
			renderTexture.Create();
			renderTexture2 = renderTexture;
		}
		return renderTexture2;
	}

	public void WriteTextureStoreInfo(StringBuilder sb)
	{
		sb.AppendLine("**************************************************");
		sb.AppendLine(string.Format("TextureStore: Pools ({0})", this._pools.Count));
		sb.AppendLine("**************************************************");
		if (this._pools.Count > 0)
		{
			for (int i = 0; i < this._pools.Count; i++)
			{
				sb.AppendLine();
				sb.AppendLine(string.Format("Pool Tier {0}", i));
				SignTextureStore.Pool pool = this._pools[i];
				if (pool == null)
				{
					sb.AppendLine("\tNULL");
				}
				else
				{
					sb.AppendLine(string.Format("\tResolution: {0}", pool.Resolution));
					sb.AppendLine(string.Format("\tFree Count: {0}", pool.Count));
				}
			}
		}
		else
		{
			sb.AppendLine();
			sb.AppendLine("\tNONE");
		}
		sb.AppendLine();
		sb.AppendLine("**************************************************");
		sb.AppendLine(string.Format("TextureStore: Entries ({0})", this._entries.Count));
		sb.AppendLine("**************************************************");
		Dictionary<int, List<GlobalSignId>> dictionary = new Dictionary<int, List<GlobalSignId>>();
		Dictionary<int, List<GlobalSignId>> dictionary2 = new Dictionary<int, List<GlobalSignId>>();
		foreach (KeyValuePair<GlobalSignId, SignTextureStore.Entry> keyValuePair in this._entries)
		{
			SignTextureStore.Entry value = keyValuePair.Value;
			sb.AppendLine();
			sb.AppendLine(string.Format("{0}", keyValuePair.Key));
			bool flag = false;
			for (int j = 0; j < this._pools.Count; j++)
			{
				if (value.TierToRT.ContainsKey(j))
				{
					flag = true;
					if (value.InUse.Contains(j))
					{
						sb.AppendLine(string.Format("\t{0} (in use)", j));
						List<GlobalSignId> list;
						if (!dictionary.TryGetValue(j, out list))
						{
							list = new List<GlobalSignId>();
							dictionary[j] = list;
						}
						list.Add(keyValuePair.Key);
					}
					else
					{
						sb.AppendLine(string.Format("\t{0} (in reserve)", j));
						List<GlobalSignId> list2;
						if (!dictionary2.TryGetValue(j, out list2))
						{
							list2 = new List<GlobalSignId>();
							dictionary2[j] = list2;
						}
						list2.Add(keyValuePair.Key);
					}
				}
			}
			if (!flag)
			{
				sb.AppendLine("\tNONE");
			}
		}
		sb.AppendLine();
		sb.AppendLine("**************************************************");
		sb.AppendLine("TextureStore: IDs By Tier");
		sb.AppendLine("**************************************************");
		for (int k = 0; k < this._pools.Count; k++)
		{
			sb.AppendLine();
			sb.AppendLine(string.Format("Tier {0}:", k));
			List<GlobalSignId> list3;
			if (!dictionary.TryGetValue(k, out list3))
			{
				sb.AppendLine("\t--- In-Use (Count: 0) ---");
				sb.AppendLine("\tNONE");
			}
			else
			{
				sb.AppendLine(string.Format("\t--- In-Use (Count: {0}) ---", list3.Count));
				foreach (GlobalSignId globalSignId in list3)
				{
					sb.AppendLine(string.Format("\t{0}", globalSignId));
				}
			}
			sb.AppendLine();
			List<GlobalSignId> list4;
			if (!dictionary2.TryGetValue(k, out list4))
			{
				sb.AppendLine("\t--- Pooled (Count: 0) ---");
				sb.AppendLine("\tNONE");
			}
			else
			{
				sb.AppendLine(string.Format("\t--- Pooled (Count: {0}) ---", list4.Count));
				foreach (GlobalSignId globalSignId2 in list4)
				{
					sb.AppendLine(string.Format("\t{0}", globalSignId2));
				}
			}
		}
		sb.AppendLine();
	}

	public static readonly ProfilerMarker s_SignTextureManagerRebuildPools = new ProfilerMarker("SignTextureManager.SignTextureStore.RebuildPools");

	public static readonly ProfilerMarker s_SignTextureManagerMarkUnused = new ProfilerMarker("SignTextureManager.SignTextureStore.MarkUnused");

	public static readonly ProfilerMarker s_SignTextureManagerMarkUsed = new ProfilerMarker("SignTextureManager.SignTextureStore.MarkUsed");

	public static readonly ProfilerMarker s_SignTextureManagerHas = new ProfilerMarker("SignTextureManager.SignTextureStore.Has");

	public static readonly ProfilerMarker s_SignTextureManagerGet = new ProfilerMarker("SignTextureManager.SignTextureStore.Get");

	public static readonly ProfilerMarker s_SignTextureManagerGetBestUpToTier = new ProfilerMarker("SignTextureManager.SignTextureStore.GetBestUpToTier");

	public static readonly ProfilerMarker s_SignTextureManagerAcquireForBake = new ProfilerMarker("SignTextureManager.SignTextureStore.AcquireForBake");

	public static readonly ProfilerMarker s_SignTextureManagerSetBaked = new ProfilerMarker("SignTextureManager.SignTextureStore.SetBaked");

	public static readonly ProfilerMarker s_SignTextureManagerInvalidateSign = new ProfilerMarker("SignTextureManager.SignTextureStore.InvalidateSign");

	public static readonly ProfilerMarker s_SignTextureManagerEvictUnused = new ProfilerMarker("SignTextureManager.SignTextureStore.EvictUnused");

	public static readonly ProfilerMarker s_SignTextureManagerClear = new ProfilerMarker("SignTextureManager.SignTextureStore.Clear");

	public static readonly ProfilerMarker s_SignTextureManagerReleaseToPool = new ProfilerMarker("SignTextureManager.SignTextureStore.ReleaseToPool");

	public static readonly ProfilerMarker s_SignTextureManagerEnqueueFreeRT = new ProfilerMarker("SignTextureManager.SignTextureStore.EnqueueFreeRT");

	public static readonly ProfilerMarker s_SignTextureManagerCreateRT = new ProfilerMarker("SignTextureManager.SignTextureStore.CreateRT");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ProfilerCounterValue<long> s_TotalTextureMemory = new ProfilerCounterValue<long>(ProfilerCategory.Scripts, "STM Texture Memory", 2, 2);

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<SignTextureStore.Pool> _pools = new List<SignTextureStore.Pool>(8);

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<GlobalSignId, SignTextureStore.Entry> _entries = new Dictionary<GlobalSignId, SignTextureStore.Entry>(256);

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<RenderTexture, SignTextureStore.RtOwner> _ownerByRT = new Dictionary<RenderTexture, SignTextureStore.RtOwner>(512);

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<int> _tiersToRemove = new List<int>(8);

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<GlobalSignId> _signsToRemove = new List<GlobalSignId>(16);

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<int> _bestTierScratch = new List<int>(8);

	[PublicizedFrom(EAccessModifier.Private)]
	public sealed class Pool
	{
		public int Count
		{
			get
			{
				return this.FreeNodes.Count;
			}
		}

		public void DestroyTexturesAndClear()
		{
			for (LinkedListNode<RenderTexture> linkedListNode = this.Free.First; linkedListNode != null; linkedListNode = linkedListNode.Next)
			{
				RenderTexture value = linkedListNode.Value;
				if (value != null)
				{
					value.Release();
					Object.DestroyImmediate(value);
				}
			}
			this.Free.Clear();
			this.FreeNodes.Clear();
		}

		public bool TryRemove(RenderTexture rt)
		{
			LinkedListNode<RenderTexture> linkedListNode;
			if (this.FreeNodes.TryGetValue(rt, out linkedListNode))
			{
				this.Free.Remove(linkedListNode);
				this.FreeNodes.Remove(rt);
				return true;
			}
			return false;
		}

		public void Add(RenderTexture rt)
		{
			if (this.FreeNodes.ContainsKey(rt))
			{
				return;
			}
			LinkedListNode<RenderTexture> linkedListNode = this.Free.AddLast(rt);
			this.FreeNodes.Add(rt, linkedListNode);
		}

		public bool TryUnpool(out RenderTexture rt)
		{
			LinkedListNode<RenderTexture> first = this.Free.First;
			if (first == null)
			{
				rt = null;
				return false;
			}
			rt = first.Value;
			this.TryRemove(rt);
			return true;
		}

		public int Resolution;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly LinkedList<RenderTexture> Free = new LinkedList<RenderTexture>();

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly Dictionary<RenderTexture, LinkedListNode<RenderTexture>> FreeNodes = new Dictionary<RenderTexture, LinkedListNode<RenderTexture>>(128);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public sealed class Entry
	{
		public readonly Dictionary<int, RenderTexture> TierToRT = new Dictionary<int, RenderTexture>(8);

		public readonly HashSet<int> InUse = new HashSet<int>();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly struct RtOwner
	{
		public RtOwner(GlobalSignId signId, int tier)
		{
			this.SignId = signId;
			this.Tier = tier;
		}

		public readonly GlobalSignId SignId;

		public readonly int Tier;
	}
}
