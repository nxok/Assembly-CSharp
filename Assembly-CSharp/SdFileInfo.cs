using System;
using System.IO;

public class SdFileInfo : SdFileSystemInfo
{
	public SdFileInfo(string fileName)
		: this(new FileInfo(fileName))
	{
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public SdFileInfo(SaveDataManagedPath managedPath)
		: this(new FileInfo(managedPath.GetOriginalPath()), managedPath)
	{
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public SdFileInfo(FileInfo fileInfo)
		: base(fileInfo)
	{
		this.m_fileInfo = fileInfo;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public SdFileInfo(FileInfo fileInfo, SaveDataManagedPath managedPath)
		: base(fileInfo, managedPath)
	{
		this.m_fileInfo = fileInfo;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Reinitialize(FileInfo fileInfo)
	{
		base.Reinitialize(fileInfo);
		this.m_fileInfo = fileInfo;
	}

	public override string Name
	{
		get
		{
			return this.m_fileInfo.Name;
		}
	}

	public long Length
	{
		get
		{
			if (base.IsManaged)
			{
				return SaveDataUtils.SaveDataManager.ManagedFileInfoLength(base.ManagedPath);
			}
			return this.m_fileInfo.Length;
		}
	}

	public string DirectoryName
	{
		get
		{
			return this.m_fileInfo.DirectoryName;
		}
	}

	public SdDirectoryInfo Directory
	{
		get
		{
			return new SdDirectoryInfo(this.m_fileInfo.Directory);
		}
	}

	public StreamReader OpenText()
	{
		if (!base.IsManaged)
		{
			return this.m_fileInfo.OpenText();
		}
		return SdFile.ManagedOpenText(base.ManagedPath);
	}

	public StreamWriter CreateText()
	{
		if (!base.IsManaged)
		{
			return this.m_fileInfo.CreateText();
		}
		return SdFile.ManagedCreateText(base.ManagedPath);
	}

	public StreamWriter AppendText()
	{
		if (!base.IsManaged)
		{
			return this.m_fileInfo.AppendText();
		}
		return SdFile.ManagedAppendText(base.ManagedPath);
	}

	public SdFileInfo CopyTo(string destFileName)
	{
		bool isManaged = base.IsManaged;
		SaveDataManagedPath saveDataManagedPath;
		bool flag = SaveDataUtils.TryGetManagedPath(destFileName, out saveDataManagedPath);
		if (isManaged && flag)
		{
			SdFile.ManagedToManagedCopy(base.ManagedPath, saveDataManagedPath, false);
		}
		else if (isManaged)
		{
			SdFile.ManagedToUnmanagedCopy(base.ManagedPath, destFileName, false);
		}
		else if (flag)
		{
			SdFile.UnmanagedToManagedCopy(this.FullName, saveDataManagedPath, false);
		}
		else
		{
			this.m_fileInfo.CopyTo(destFileName, false);
		}
		return new SdFileInfo(destFileName);
	}

	public SdFileInfo CopyTo(string destFileName, bool overwrite)
	{
		bool isManaged = base.IsManaged;
		SaveDataManagedPath saveDataManagedPath;
		bool flag = SaveDataUtils.TryGetManagedPath(destFileName, out saveDataManagedPath);
		if (isManaged && flag)
		{
			SdFile.ManagedToManagedCopy(base.ManagedPath, saveDataManagedPath, overwrite);
		}
		else if (isManaged)
		{
			SdFile.ManagedToUnmanagedCopy(base.ManagedPath, destFileName, overwrite);
		}
		else if (flag)
		{
			SdFile.UnmanagedToManagedCopy(this.FullName, saveDataManagedPath, overwrite);
		}
		else
		{
			this.m_fileInfo.CopyTo(destFileName, overwrite);
		}
		return new SdFileInfo(destFileName);
	}

	public Stream Create()
	{
		if (!base.IsManaged)
		{
			return this.m_fileInfo.Create();
		}
		return SdFile.ManagedCreate(base.ManagedPath);
	}

	public override void Delete()
	{
		if (base.IsManaged)
		{
			SdFile.ManagedDelete(base.ManagedPath);
			return;
		}
		this.m_fileInfo.Delete();
	}

	public override bool Exists
	{
		get
		{
			if (!base.IsManaged)
			{
				return this.m_fileInfo.Exists;
			}
			return SdFile.ManagedExists(base.ManagedPath);
		}
	}

	public Stream Open(FileMode mode)
	{
		if (base.IsManaged)
		{
			return SdFile.ManagedOpen(base.ManagedPath, mode, FileAccess.ReadWrite, FileShare.None);
		}
		return this.m_fileInfo.Open(mode);
	}

	public Stream Open(FileMode mode, FileAccess access)
	{
		if (base.IsManaged)
		{
			return SdFile.ManagedOpen(base.ManagedPath, mode, access, FileShare.None);
		}
		return this.m_fileInfo.Open(mode, access);
	}

	public Stream Open(FileMode mode, FileAccess access, FileShare share)
	{
		if (base.IsManaged)
		{
			return SdFile.ManagedOpen(base.ManagedPath, mode, access, share);
		}
		return this.m_fileInfo.Open(mode, access, share);
	}

	public Stream OpenRead()
	{
		if (base.IsManaged)
		{
			return SdFile.ManagedOpen(base.ManagedPath, FileMode.Open, FileAccess.Read, FileShare.Read);
		}
		return this.m_fileInfo.OpenRead();
	}

	public Stream OpenWrite()
	{
		if (base.IsManaged)
		{
			return SdFile.ManagedOpen(base.ManagedPath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);
		}
		return this.m_fileInfo.OpenWrite();
	}

	public override string ToString()
	{
		return this.m_fileInfo.ToString();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public FileInfo m_fileInfo;
}
