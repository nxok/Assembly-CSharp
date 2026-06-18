using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public sealed class SdDirectoryInfo : SdFileSystemInfo
{
	public SdDirectoryInfo(string path)
		: this(new DirectoryInfo(path))
	{
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public SdDirectoryInfo(SaveDataManagedPath saveDataManagedPath)
		: this(new DirectoryInfo(saveDataManagedPath.GetOriginalPath()), saveDataManagedPath)
	{
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public SdDirectoryInfo(DirectoryInfo directoryInfo)
		: base(directoryInfo)
	{
		this.m_directoryInfo = directoryInfo;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public SdDirectoryInfo(DirectoryInfo directoryInfo, SaveDataManagedPath saveDataManagedPath)
		: base(directoryInfo, saveDataManagedPath)
	{
		this.m_directoryInfo = directoryInfo;
	}

	public override string Name
	{
		get
		{
			return this.m_directoryInfo.Name;
		}
	}

	public override string FullName
	{
		get
		{
			return this.m_directoryInfo.FullName;
		}
	}

	public SdDirectoryInfo Parent
	{
		get
		{
			return new SdDirectoryInfo(this.m_directoryInfo.Parent);
		}
	}

	public SdDirectoryInfo CreateSubdirectory(string path)
	{
		if (base.IsManaged)
		{
			return this.ManagedCreateSubdirectory(path);
		}
		return new SdDirectoryInfo(this.m_directoryInfo.CreateSubdirectory(path));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public SdDirectoryInfo ManagedCreateSubdirectory(string path)
	{
		return SdDirectory.CreateDirectory(Path.Combine(this.FullName, path));
	}

	public void Create()
	{
		if (base.IsManaged)
		{
			this.ManagedCreateDirectory();
			return;
		}
		this.m_directoryInfo.Create();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ManagedCreateDirectory()
	{
		SdDirectory.ManagedCreateDirectory(base.ManagedPath);
	}

	public override bool Exists
	{
		get
		{
			if (!base.IsManaged)
			{
				return this.m_directoryInfo.Exists;
			}
			return SdDirectory.ManagedExists(base.ManagedPath);
		}
	}

	public SdFileInfo[] GetFiles(string searchPattern)
	{
		return this.EnumerateFiles(searchPattern).ToArray<SdFileInfo>();
	}

	public SdFileInfo[] GetFiles(string searchPattern, SearchOption searchOption)
	{
		return this.EnumerateFiles(searchPattern, searchOption).ToArray<SdFileInfo>();
	}

	public SdFileInfo[] GetFiles()
	{
		return this.EnumerateFiles().ToArray<SdFileInfo>();
	}

	public SdDirectoryInfo[] GetDirectories()
	{
		return this.EnumerateDirectories().ToArray<SdDirectoryInfo>();
	}

	public SdDirectoryInfo[] GetDirectories(string searchPattern)
	{
		return this.EnumerateDirectories(searchPattern).ToArray<SdDirectoryInfo>();
	}

	public SdDirectoryInfo[] GetDirectories(string searchPattern, SearchOption searchOption)
	{
		return this.EnumerateDirectories(searchPattern, searchOption).ToArray<SdDirectoryInfo>();
	}

	public SdFileSystemInfo[] GetFileSystemInfos()
	{
		return this.EnumerateFileSystemInfos().ToArray<SdFileSystemInfo>();
	}

	public SdFileSystemInfo[] GetFileSystemInfos(string searchPattern)
	{
		return this.EnumerateFileSystemInfos(searchPattern).ToArray<SdFileSystemInfo>();
	}

	public SdFileSystemInfo[] GetFileSystemInfos(string searchPattern, SearchOption searchOption)
	{
		return this.EnumerateFileSystemInfos(searchPattern, searchOption).ToArray<SdFileSystemInfo>();
	}

	public IEnumerable<SdDirectoryInfo> EnumerateDirectories()
	{
		if (base.IsManaged)
		{
			return this.ManagedEnumerateDirectories("*", SearchOption.TopDirectoryOnly);
		}
		return from x in this.m_directoryInfo.EnumerateDirectories()
			select new SdDirectoryInfo(x);
	}

	public IEnumerable<SdDirectoryInfo> EnumerateDirectories(string searchPattern)
	{
		if (base.IsManaged)
		{
			return this.ManagedEnumerateDirectories(searchPattern, SearchOption.TopDirectoryOnly);
		}
		return from x in this.m_directoryInfo.EnumerateDirectories(searchPattern)
			select new SdDirectoryInfo(x);
	}

	public IEnumerable<SdDirectoryInfo> EnumerateDirectories(string searchPattern, SearchOption searchOption)
	{
		if (base.IsManaged)
		{
			return this.ManagedEnumerateDirectories(searchPattern, searchOption);
		}
		return from x in this.m_directoryInfo.EnumerateDirectories(searchPattern, searchOption)
			select new SdDirectoryInfo(x);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerable<SdDirectoryInfo> ManagedEnumerateDirectories(string searchPattern, SearchOption searchOption)
	{
		return SaveDataUtils.SaveDataManager.ManagedDirectoryInfoEnumerateDirectories(base.ManagedPath, searchPattern, searchOption);
	}

	public IEnumerable<SdFileInfo> EnumerateFiles()
	{
		if (base.IsManaged)
		{
			return this.ManagedEnumerateFiles("*", SearchOption.TopDirectoryOnly);
		}
		return from x in this.m_directoryInfo.EnumerateFiles()
			select new SdFileInfo(x);
	}

	public IEnumerable<SdFileInfo> EnumerateFiles(string searchPattern)
	{
		if (base.IsManaged)
		{
			return this.ManagedEnumerateFiles(searchPattern, SearchOption.TopDirectoryOnly);
		}
		return from x in this.m_directoryInfo.EnumerateFiles(searchPattern)
			select new SdFileInfo(x);
	}

	public IEnumerable<SdFileInfo> EnumerateFiles(string searchPattern, SearchOption searchOption)
	{
		if (base.IsManaged)
		{
			return this.ManagedEnumerateFiles(searchPattern, searchOption);
		}
		return from x in this.m_directoryInfo.EnumerateFiles(searchPattern, searchOption)
			select new SdFileInfo(x);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerable<SdFileInfo> ManagedEnumerateFiles(string searchPattern, SearchOption searchOption)
	{
		return SaveDataUtils.SaveDataManager.ManagedDirectoryInfoEnumerateFiles(base.ManagedPath, searchPattern, searchOption);
	}

	public IEnumerable<SdFileSystemInfo> EnumerateFileSystemInfos()
	{
		if (base.IsManaged)
		{
			return this.ManagedEnumerateFileSystemInfos("*", SearchOption.TopDirectoryOnly);
		}
		return this.m_directoryInfo.EnumerateFileSystemInfos().Select(new Func<FileSystemInfo, SdFileSystemInfo>(SdDirectoryInfo.WrapFileSystemInfo));
	}

	public IEnumerable<SdFileSystemInfo> EnumerateFileSystemInfos(string searchPattern)
	{
		if (base.IsManaged)
		{
			return this.ManagedEnumerateFileSystemInfos(searchPattern, SearchOption.TopDirectoryOnly);
		}
		return this.m_directoryInfo.EnumerateFileSystemInfos(searchPattern).Select(new Func<FileSystemInfo, SdFileSystemInfo>(SdDirectoryInfo.WrapFileSystemInfo));
	}

	public IEnumerable<SdFileSystemInfo> EnumerateFileSystemInfos(string searchPattern, SearchOption searchOption)
	{
		if (base.IsManaged)
		{
			return this.ManagedEnumerateFileSystemInfos(searchPattern, searchOption);
		}
		return this.m_directoryInfo.EnumerateFileSystemInfos(searchPattern, searchOption).Select(new Func<FileSystemInfo, SdFileSystemInfo>(SdDirectoryInfo.WrapFileSystemInfo));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerable<SdFileSystemInfo> ManagedEnumerateFileSystemInfos(string searchPattern, SearchOption searchOption)
	{
		return SaveDataUtils.SaveDataManager.ManagedDirectoryInfoEnumerateFileSystemInfos(base.ManagedPath, searchPattern, searchOption);
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public static SdFileSystemInfo WrapFileSystemInfo(FileSystemInfo fileSystemInfo)
	{
		FileInfo fileInfo = fileSystemInfo as FileInfo;
		SdFileSystemInfo sdFileSystemInfo;
		if (fileInfo == null)
		{
			DirectoryInfo directoryInfo = fileSystemInfo as DirectoryInfo;
			if (directoryInfo == null)
			{
				throw new NotImplementedException("Unsupported implementation of FileSystemInfo: " + fileSystemInfo.GetType().FullName + ".");
			}
			sdFileSystemInfo = new SdDirectoryInfo(directoryInfo);
		}
		else
		{
			sdFileSystemInfo = new SdFileInfo(fileInfo);
		}
		return sdFileSystemInfo;
	}

	public bool IsDirEmpty()
	{
		bool flag;
		using (IEnumerator<SdFileSystemInfo> enumerator = this.EnumerateFileSystemInfos("*", SearchOption.AllDirectories).GetEnumerator())
		{
			flag = !enumerator.MoveNext();
		}
		return flag;
	}

	public SdDirectoryInfo Root
	{
		get
		{
			return new SdDirectoryInfo(this.m_directoryInfo.Root);
		}
	}

	public override void Delete()
	{
		if (base.IsManaged)
		{
			this.ManagedDelete(false);
			return;
		}
		this.m_directoryInfo.Delete();
	}

	public void Delete(bool recursive)
	{
		if (base.IsManaged)
		{
			this.ManagedDelete(recursive);
			return;
		}
		this.m_directoryInfo.Delete(recursive);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ManagedDelete(bool recursive)
	{
		SdDirectory.ManagedDelete(base.ManagedPath, recursive);
	}

	public override string ToString()
	{
		return this.m_directoryInfo.ToString();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly DirectoryInfo m_directoryInfo;
}
