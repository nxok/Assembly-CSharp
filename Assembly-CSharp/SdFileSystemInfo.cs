using System;
using System.IO;

public abstract class SdFileSystemInfo
{
	public bool IsManaged
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public SaveDataManagedPath ManagedPath
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public SdFileSystemInfo(FileSystemInfo fileSystemInfo)
	{
		this.Reinitialize(fileSystemInfo);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public SdFileSystemInfo(FileSystemInfo fileSystemInfo, SaveDataManagedPath managedPath)
	{
		this.m_fileSystemInfo = fileSystemInfo;
		this.IsManaged = true;
		this.ManagedPath = managedPath;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void Reinitialize(FileSystemInfo fileSystemInfo)
	{
		this.m_fileSystemInfo = fileSystemInfo;
		SaveDataManagedPath saveDataManagedPath;
		this.IsManaged = SaveDataUtils.TryGetManagedPath(fileSystemInfo.FullName, out saveDataManagedPath);
		this.ManagedPath = saveDataManagedPath;
	}

	public virtual string FullName
	{
		get
		{
			return this.m_fileSystemInfo.FullName;
		}
	}

	public string Extension
	{
		get
		{
			return this.m_fileSystemInfo.Extension;
		}
	}

	public abstract string Name { get; }

	public abstract bool Exists { get; }

	public abstract void Delete();

	public DateTime LastWriteTime
	{
		get
		{
			return this.LastWriteTimeUtc.ToLocalTime();
		}
	}

	public DateTime LastWriteTimeUtc
	{
		get
		{
			if (!this.IsManaged)
			{
				return this.m_fileSystemInfo.LastWriteTimeUtc;
			}
			if (this.m_fileSystemInfo is DirectoryInfo)
			{
				return SdDirectory.ManagedGetLastWriteTimeUtc(this.ManagedPath);
			}
			return SdFile.ManagedGetLastWriteTimeUtc(this.ManagedPath);
		}
	}

	public void Refresh()
	{
		this.m_fileSystemInfo.Refresh();
		this.Reinitialize(this.m_fileSystemInfo);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public FileSystemInfo m_fileSystemInfo;
}
