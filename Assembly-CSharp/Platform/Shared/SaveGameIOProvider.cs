using System;
using System.Collections.Generic;
using System.IO;

namespace Platform.Shared
{
	public abstract class SaveGameIOProvider : IPlatformSaveGameIOProvider
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		public abstract string GetPath(SaveDataManagedPath path);

		public void ManagedFileRead(SaveDataManagedPath path, Stream dest)
		{
			using (FileStream fileStream = File.OpenRead(this.GetPath(path)))
			{
				StreamUtils.StreamCopy(fileStream, dest, null, true);
			}
		}

		public void ManagedFileWrite(SaveDataManagedPath path, Stream src)
		{
			using (FileStream fileStream = File.Open(this.GetPath(path), FileMode.Create, FileAccess.Write, FileShare.Read))
			{
				StreamUtils.StreamCopy(src, fileStream, null, true);
			}
		}

		public void ManagedFileCopy(SaveDataManagedPath sourceFileName, SaveDataManagedPath destFileName, bool overwrite = false)
		{
			File.Copy(this.GetPath(sourceFileName), this.GetPath(destFileName), overwrite);
		}

		public void ManagedFileDelete(SaveDataManagedPath path)
		{
			File.Delete(this.GetPath(path));
		}

		public bool ManagedFileExists(SaveDataManagedPath path)
		{
			return File.Exists(this.GetPath(path));
		}

		public DateTime ManagedFileGetLastWriteTimeUtc(SaveDataManagedPath path)
		{
			return File.GetLastWriteTimeUtc(this.GetPath(path));
		}

		public void ManagedFileMove(SaveDataManagedPath sourceFileName, SaveDataManagedPath destFileName)
		{
			File.Move(this.GetPath(sourceFileName), this.GetPath(destFileName));
		}

		public SdDirectoryInfo ManagedDirectoryCreateDirectory(SaveDataManagedPath path)
		{
			return new SdDirectoryInfo(Directory.CreateDirectory(this.GetPath(path)));
		}

		public DateTime ManagedDirectoryGetLastWriteTimeUtc(SaveDataManagedPath path)
		{
			return Directory.GetLastWriteTimeUtc(this.GetPath(path));
		}

		public bool ManagedDirectoryExists(SaveDataManagedPath path)
		{
			return Directory.Exists(this.GetPath(path));
		}

		public IEnumerable<SaveDataManagedPath> ManagedDirectoryEnumerateDirectories(SaveDataManagedPath path, string searchPattern, SearchOption searchOption)
		{
			string path2 = this.GetPath(path);
			return SaveGameProviderHelper.GetManagedPathsFromBaseAndSubPaths(path, path2, Directory.EnumerateDirectories(path2, searchPattern, searchOption));
		}

		public IEnumerable<SaveDataManagedPath> ManagedDirectoryEnumerateFiles(SaveDataManagedPath path, string searchPattern, SearchOption searchOption)
		{
			string path2 = this.GetPath(path);
			return SaveGameProviderHelper.GetManagedPathsFromBaseAndSubPaths(path, path2, Directory.EnumerateFiles(path2, searchPattern, searchOption));
		}

		public IEnumerable<SaveDataManagedPath> ManagedDirectoryEnumerateFileSystemEntries(SaveDataManagedPath path, string searchPattern, SearchOption searchOption)
		{
			string path2 = this.GetPath(path);
			return SaveGameProviderHelper.GetManagedPathsFromBaseAndSubPaths(path, path2, Directory.EnumerateFileSystemEntries(path2, searchPattern, searchOption));
		}

		public void ManagedDirectoryDelete(SaveDataManagedPath path, bool recursive)
		{
			Directory.Delete(this.GetPath(path), recursive);
		}

		public long ManagedFileInfoLength(SaveDataManagedPath path)
		{
			return new FileInfo(this.GetPath(path)).Length;
		}

		public IEnumerable<SdDirectoryInfo> ManagedDirectoryInfoEnumerateDirectories(SaveDataManagedPath path, string searchPattern, SearchOption searchOption)
		{
			string basePath = this.GetPath(path);
			DirectoryInfo directoryInfo = new DirectoryInfo(basePath);
			foreach (DirectoryInfo directoryInfo2 in directoryInfo.EnumerateDirectories(searchPattern, searchOption))
			{
				string fullName = directoryInfo2.FullName;
				SaveDataManagedPath managedPathFromBaseAndSubPath = SaveGameProviderHelper.GetManagedPathFromBaseAndSubPath(path, basePath, fullName);
				yield return new SdDirectoryInfo(managedPathFromBaseAndSubPath);
			}
			IEnumerator<DirectoryInfo> enumerator = null;
			yield break;
			yield break;
		}

		public IEnumerable<SdFileInfo> ManagedDirectoryInfoEnumerateFiles(SaveDataManagedPath path, string searchPattern, SearchOption searchOption)
		{
			string basePath = this.GetPath(path);
			DirectoryInfo directoryInfo = new DirectoryInfo(basePath);
			foreach (FileInfo fileInfo in directoryInfo.EnumerateFiles(searchPattern, searchOption))
			{
				string fullName = fileInfo.FullName;
				SaveDataManagedPath managedPathFromBaseAndSubPath = SaveGameProviderHelper.GetManagedPathFromBaseAndSubPath(path, basePath, fullName);
				yield return new SdFileInfo(managedPathFromBaseAndSubPath);
			}
			IEnumerator<FileInfo> enumerator = null;
			yield break;
			yield break;
		}

		public IEnumerable<SdFileSystemInfo> ManagedDirectoryInfoEnumerateFileSystemInfos(SaveDataManagedPath path, string searchPattern, SearchOption searchOption)
		{
			string basePath = this.GetPath(path);
			DirectoryInfo directoryInfo = new DirectoryInfo(basePath);
			foreach (FileSystemInfo fileSystemInfo in directoryInfo.EnumerateFileSystemInfos(searchPattern, searchOption))
			{
				string fullName = fileSystemInfo.FullName;
				SaveDataManagedPath managedPathFromBaseAndSubPath = SaveGameProviderHelper.GetManagedPathFromBaseAndSubPath(path, basePath, fullName);
				SdFileSystemInfo sdFileSystemInfo;
				if (!(fileSystemInfo is FileInfo))
				{
					if (!(fileSystemInfo is DirectoryInfo))
					{
						throw new NotImplementedException("Unsupported implementation of FileSystemInfo: " + fileSystemInfo.GetType().FullName + ".");
					}
					sdFileSystemInfo = new SdDirectoryInfo(managedPathFromBaseAndSubPath);
				}
				else
				{
					sdFileSystemInfo = new SdFileInfo(managedPathFromBaseAndSubPath);
				}
				yield return sdFileSystemInfo;
			}
			IEnumerator<FileSystemInfo> enumerator = null;
			yield break;
			yield break;
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public SaveGameIOProvider()
		{
		}
	}
}
