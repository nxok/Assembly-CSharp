using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine.Profiling;

public static class NetPackageManager
{
	[PublicizedFrom(EAccessModifier.Private)]
	static NetPackageManager()
	{
		Log.Out("NetPackageManager Init");
		ReflectionHelpers.FindTypesImplementingBase(typeof(NetPackage), delegate(Type _type)
		{
			NetPackageManager.knownPackageTypes.Add(_type.Name, _type);
		}, false);
	}

	public static void ResetMappings()
	{
		NetPackageManager.packageIdToClass = null;
		NetPackageManager.packageIdToPackageInformation = null;
		NetPackageManager.packageClassToPackageId = null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void AddPackageMapping(int _id, Type _type)
	{
		NetPackageManager.packageIdToClass[_id] = _type;
		NetPackageManager.packageClassToPackageId[_type] = _id;
		NetPackageManager.IPackageInformation packageInformation = (NetPackageManager.IPackageInformation)typeof(NetPackageManager.NetPackageInformation<>).MakeGenericType(new Type[] { _type }).GetProperty("Instance").GetValue(null, null);
		NetPackageManager.packageIdToPackageInformation[_id] = packageInformation;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void SetupBaseMapping()
	{
		NetPackageManager.packageIdToClass = new Type[NetPackageManager.KnownPackageCount];
		NetPackageManager.packageIdToPackageInformation = new NetPackageManager.IPackageInformation[NetPackageManager.KnownPackageCount];
		NetPackageManager.packageClassToPackageId = new Dictionary<Type, int>();
		NetPackageManager.AddPackageMapping(0, NetPackageManager.packageIdsType);
	}

	public static void StartServer()
	{
		NetPackageManager.ResetMappings();
		NetPackageManager.SetupBaseMapping();
		int num = 1;
		foreach (KeyValuePair<string, Type> keyValuePair in NetPackageManager.knownPackageTypes)
		{
			if (!(keyValuePair.Value == NetPackageManager.packageIdsType))
			{
				NetPackageManager.AddPackageMapping(num, keyValuePair.Value);
				num++;
			}
		}
	}

	public static void StartClient()
	{
		NetPackageManager.ResetMappings();
		NetPackageManager.SetupBaseMapping();
	}

	public static void IdMappingsReceived(string[] _mappings)
	{
		for (int i = 0; i < _mappings.Length; i++)
		{
			Type type;
			if (!NetPackageManager.knownPackageTypes.TryGetValue(_mappings[i], out type))
			{
				Log.Error("[NET] Unknown package type " + _mappings[i] + ", can not proceed connecting to server");
				SingletonMonoBehaviour<ConnectionManager>.Instance.Disconnect();
				GameManager.Instance.ShowMessagePlayerDenied(new GameUtils.KickPlayerData(GameUtils.EKickReason.UnknownNetPackage, 0, default(DateTime), ""));
				return;
			}
			if (!(type == NetPackageManager.packageIdsType))
			{
				NetPackageManager.AddPackageMapping(i, type);
			}
		}
	}

	public static NetPackage ParsePackage(PooledBinaryReader _reader, ClientInfo _sender)
	{
		NetPackage rawPackage = NetPackageManager.getPackageInfoByType((int)_reader.ReadUInt16()).GetRawPackage();
		rawPackage.Sender = _sender;
		rawPackage.read(_reader);
		return rawPackage;
	}

	public static void FreePackage(NetPackage _package)
	{
		NetPackageManager.getPackageInfoByType(_package.PackageId).FreePackage(_package);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static NetPackageManager.IPackageInformation getPackageInfoByType(int _packageTypeId)
	{
		if (_packageTypeId >= NetPackageManager.packageIdToPackageInformation.Length || NetPackageManager.packageIdToPackageInformation[_packageTypeId] == null)
		{
			throw new NetPackageManager.UnknownNetPackageException(_packageTypeId);
		}
		return NetPackageManager.packageIdToPackageInformation[_packageTypeId];
	}

	public static TPackage GetPackage<TPackage>() where TPackage : NetPackage
	{
		return NetPackageManager.NetPackageInformation<TPackage>.Instance.GetPackage();
	}

	public static int GetPackageId(Type _type)
	{
		return NetPackageManager.packageClassToPackageId[_type];
	}

	public static string GetPackageName(int _id)
	{
		return NetPackageManager.packageIdToClass[_id].ToString();
	}

	public static int KnownPackageCount
	{
		get
		{
			return NetPackageManager.knownPackageTypes.Count;
		}
	}

	public static Type[] PackageMappings
	{
		get
		{
			return NetPackageManager.packageIdToClass;
		}
	}

	public static void LogStats()
	{
		Log.Out("NetPackage pool stats:");
		int num = 0;
		int num2 = 0;
		for (int i = 0; i < NetPackageManager.packageIdToPackageInformation.Length; i++)
		{
			NetPackageManager.IPackageInformation packageInformation = NetPackageManager.packageIdToPackageInformation[i];
			if (packageInformation != null)
			{
				int num3;
				int num4;
				packageInformation.GetStats(out num3, out num4);
				Log.Out("    {0}: {1} packages, {2} Bytes", new object[]
				{
					NetPackageManager.GetPackageName(i),
					num3,
					num4
				});
				num += num3;
				num2 += num4;
			}
		}
		Log.Out("  Total: {0} packages, {1} Bytes", new object[] { num, num2 });
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static Type[] packageIdToClass;

	[PublicizedFrom(EAccessModifier.Private)]
	public static NetPackageManager.IPackageInformation[] packageIdToPackageInformation;

	[PublicizedFrom(EAccessModifier.Private)]
	public static Dictionary<Type, int> packageClassToPackageId;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Dictionary<string, Type> knownPackageTypes = new CaseInsensitiveStringDictionary<Type>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Type packageIdsType = typeof(NetPackagePackageIds);

	[PublicizedFrom(EAccessModifier.Private)]
	public interface IPackageInformation
	{
		NetPackage GetRawPackage();

		void FreePackage(NetPackage _package);

		void GetStats(out int _packages, out int _totalSize);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public class NetPackageInformation<TPackage> : NetPackageManager.IPackageInformation where TPackage : NetPackage
	{
		public static NetPackageManager.NetPackageInformation<TPackage> Instance
		{
			get
			{
				if (NetPackageManager.NetPackageInformation<TPackage>.instance == null)
				{
					NetPackageManager.NetPackageInformation<TPackage>.instance = new NetPackageManager.NetPackageInformation<TPackage>();
				}
				return NetPackageManager.NetPackageInformation<TPackage>.instance;
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public NetPackageInformation()
		{
			Type typeFromHandle = typeof(TPackage);
			BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
			this.ctor = typeFromHandle.GetConstructor(bindingFlags, null, CallingConventions.Any, Type.EmptyTypes, null);
			if (typeof(IMemoryPoolableObject).IsAssignableFrom(typeFromHandle))
			{
				this.capacity = 10;
				MethodInfo method = typeFromHandle.GetMethod("GetPoolSize", BindingFlags.Static | BindingFlags.Public, null, Type.EmptyTypes, null);
				if (method != null)
				{
					if (method.ReturnType == typeof(int))
					{
						this.capacity = (int)method.Invoke(null, null);
					}
					else
					{
						Log.Warning("Poolable NetPackage has GetPoolSize method with wrong return type");
					}
				}
				this.pool = new TPackage[this.capacity];
			}
		}

		public TPackage GetPackage()
		{
			if (this.pool != null)
			{
				TPackage[] array = this.pool;
				lock (array)
				{
					if (this.poolSize > 0)
					{
						this.poolSize--;
						TPackage tpackage = this.pool[this.poolSize];
						this.pool[this.poolSize] = default(TPackage);
						return tpackage;
					}
				}
			}
			return (TPackage)((object)this.ctor.Invoke(null));
		}

		public NetPackage GetRawPackage()
		{
			return this.GetPackage();
		}

		public void FreePackage(NetPackage _package)
		{
			if (this.pool == null)
			{
				return;
			}
			IMemoryPoolableObject memoryPoolableObject = (IMemoryPoolableObject)_package;
			TPackage[] array = this.pool;
			lock (array)
			{
				if (this.poolSize < this.capacity)
				{
					memoryPoolableObject.Reset();
					this.pool[this.poolSize] = (TPackage)((object)_package);
					this.poolSize++;
				}
				else
				{
					memoryPoolableObject.Cleanup();
				}
			}
		}

		public void GetStats(out int _packages, out int _totalSize)
		{
			_packages = 0;
			_totalSize = 0;
			if (this.pool == null)
			{
				return;
			}
			TPackage[] array = this.pool;
			lock (array)
			{
				_packages = this.poolSize;
			}
		}

		public void Cleanup()
		{
			if (this.pool == null)
			{
				return;
			}
			TPackage[] array = this.pool;
			lock (array)
			{
				for (int i = 0; i < this.poolSize; i++)
				{
					((IMemoryPoolableObject)((object)this.pool[i])).Cleanup();
				}
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static NetPackageManager.NetPackageInformation<TPackage> instance;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly TPackage[] pool;

		[PublicizedFrom(EAccessModifier.Private)]
		public int poolSize;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly int capacity;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly ConstructorInfo ctor;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly CustomSampler getSampler = CustomSampler.Create("NPM.PI.GetPackage", false);

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly CustomSampler getSamplerPool = CustomSampler.Create("Pooled", false);

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly CustomSampler getSamplerNew = CustomSampler.Create("New", false);

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly CustomSampler getSamplerType = CustomSampler.Create(typeof(TPackage).Name, false);

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly CustomSampler freeSampler = CustomSampler.Create("NPM.PI.FreePackage", false);

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly CustomSampler freeSamplerPool = CustomSampler.Create("ToPool", false);

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly CustomSampler freeSamplerCleanup = CustomSampler.Create("Cleanup", false);
	}

	public class UnknownNetPackageException : Exception
	{
		public UnknownNetPackageException(int _packageId)
			: base("Unknown NetPackage ID: " + _packageId.ToString())
		{
		}
	}
}
