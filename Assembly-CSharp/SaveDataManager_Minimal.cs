using System;
using System.Collections.Generic;
using System.IO;

public class SaveDataManager_Minimal : SaveDataManagerBase
{
	public static SaveDataManager_Minimal Instance
	{
		get
		{
			if (SaveDataManager_Minimal.instance == null)
			{
				SaveDataManager_Minimal.instance = new SaveDataManager_Minimal();
			}
			return SaveDataManager_Minimal.instance;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override Stream GetStream(SaveDataManagedPath path, FileMode mode, FileAccess access, FileShare share)
	{
		object obj = this.lockObj;
		Stream stream2;
		lock (obj)
		{
			Stream stream = new FileStream(path.GetOriginalPath(), mode, access, share);
			if (SaveDataManager_Minimal.concurrencyCheckEnabled)
			{
				this.streamToPathMap[stream] = path;
				if (!this.openCountMap.ContainsKey(path))
				{
					this.openCountMap[path] = 1;
				}
				else
				{
					Dictionary<SaveDataManagedPath, int> dictionary = this.openCountMap;
					dictionary[path]++;
					if (this.openCountMap[path] > 1)
					{
						Log.Error(string.Format("[SaveDataManager] Detected {0} concurrent operations on file: {1}", this.openCountMap[path], path));
					}
				}
			}
			stream2 = stream;
		}
		return stream2;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void ReturnStream(Stream stream)
	{
		object obj = this.lockObj;
		lock (obj)
		{
			if (SaveDataManager_Minimal.concurrencyCheckEnabled)
			{
				SaveDataManagedPath saveDataManagedPath = this.streamToPathMap[stream];
				stream.Dispose();
				this.openCountMap[saveDataManagedPath] = this.openCountMap[saveDataManagedPath] - 1;
			}
			else
			{
				stream.Dispose();
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly bool concurrencyCheckEnabled = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<SaveDataManagedPath, int> openCountMap = new Dictionary<SaveDataManagedPath, int>();

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<Stream, SaveDataManagedPath> streamToPathMap = new Dictionary<Stream, SaveDataManagedPath>();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly object lockObj = new object();

	[PublicizedFrom(EAccessModifier.Private)]
	public static SaveDataManager_Minimal instance;
}
