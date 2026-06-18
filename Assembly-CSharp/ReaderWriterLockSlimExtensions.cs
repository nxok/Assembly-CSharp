using System;
using System.Threading;

public static class ReaderWriterLockSlimExtensions
{
	public static ReaderWriterLockSlimExtensions.ReadScope ReadLockScope(this ReaderWriterLockSlim lockSlim)
	{
		return new ReaderWriterLockSlimExtensions.ReadScope(lockSlim);
	}

	public static ReaderWriterLockSlimExtensions.WriteScope WriteLockScope(this ReaderWriterLockSlim lockSlim)
	{
		return new ReaderWriterLockSlimExtensions.WriteScope(lockSlim);
	}

	public static ReaderWriterLockSlimExtensions.UpgradeableReadScope UpgradableReadLockScope(this ReaderWriterLockSlim lockSlim)
	{
		return new ReaderWriterLockSlimExtensions.UpgradeableReadScope(lockSlim);
	}

	public readonly struct ReadScope : IDisposable
	{
		public ReadScope(ReaderWriterLockSlim lockSlim)
		{
			this.m_lockSlim = lockSlim;
			this.m_lockSlim.EnterReadLock();
		}

		public void Dispose()
		{
			this.m_lockSlim.ExitReadLock();
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly ReaderWriterLockSlim m_lockSlim;
	}

	public readonly struct WriteScope : IDisposable
	{
		public WriteScope(ReaderWriterLockSlim lockSlim)
		{
			this.m_lockSlim = lockSlim;
			this.m_lockSlim.EnterWriteLock();
		}

		public void Dispose()
		{
			this.m_lockSlim.ExitWriteLock();
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly ReaderWriterLockSlim m_lockSlim;
	}

	public readonly struct UpgradeableReadScope : IDisposable
	{
		public UpgradeableReadScope(ReaderWriterLockSlim lockSlim)
		{
			this.m_lockSlim = lockSlim;
			this.m_lockSlim.EnterUpgradeableReadLock();
		}

		public void Dispose()
		{
			this.m_lockSlim.ExitUpgradeableReadLock();
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly ReaderWriterLockSlim m_lockSlim;
	}
}
