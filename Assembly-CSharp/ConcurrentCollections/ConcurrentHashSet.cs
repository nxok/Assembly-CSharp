using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace ConcurrentCollections
{
	[DebuggerDisplay("Count = {Count}")]
	public class ConcurrentHashSet<T> : IReadOnlyCollection<T>, IEnumerable<T>, IEnumerable, ICollection<T>
	{
		public static int DefaultConcurrencyLevel
		{
			[PublicizedFrom(EAccessModifier.Private)]
			get
			{
				return Math.Max(2, Environment.ProcessorCount);
			}
		}

		public int Count
		{
			get
			{
				int num = 0;
				int num2 = 0;
				try
				{
					this.AcquireAllLocks(ref num2);
					for (int i = 0; i < this._tables.CountPerLock.Length; i++)
					{
						num += this._tables.CountPerLock[i];
					}
				}
				finally
				{
					this.ReleaseLocks(0, num2);
				}
				return num;
			}
		}

		public bool IsEmpty
		{
			get
			{
				int num = 0;
				try
				{
					this.AcquireAllLocks(ref num);
					for (int i = 0; i < this._tables.CountPerLock.Length; i++)
					{
						if (this._tables.CountPerLock[i] != 0)
						{
							return false;
						}
					}
				}
				finally
				{
					this.ReleaseLocks(0, num);
				}
				return true;
			}
		}

		public ConcurrentHashSet()
			: this(ConcurrentHashSet<T>.DefaultConcurrencyLevel, 31, true, null)
		{
		}

		public ConcurrentHashSet(Action<T> onRemovalFaiuire)
			: this(ConcurrentHashSet<T>.DefaultConcurrencyLevel, 31, true, null)
		{
			this.OnRemovalFailure = onRemovalFaiuire;
		}

		public ConcurrentHashSet(int concurrencyLevel, int capacity)
			: this(concurrencyLevel, capacity, false, null)
		{
		}

		public ConcurrentHashSet(IEnumerable<T> collection)
			: this(collection, null)
		{
		}

		public ConcurrentHashSet(IEqualityComparer<T> comparer)
			: this(ConcurrentHashSet<T>.DefaultConcurrencyLevel, 31, true, comparer)
		{
		}

		public ConcurrentHashSet(IEnumerable<T> collection, IEqualityComparer<T> comparer)
			: this(comparer)
		{
			if (collection == null)
			{
				throw new ArgumentNullException("collection");
			}
			this.InitializeFromCollection(collection);
		}

		public ConcurrentHashSet(int concurrencyLevel, IEnumerable<T> collection, IEqualityComparer<T> comparer)
			: this(concurrencyLevel, 31, false, comparer)
		{
			if (collection == null)
			{
				throw new ArgumentNullException("collection");
			}
			this.InitializeFromCollection(collection);
		}

		public ConcurrentHashSet(int concurrencyLevel, int capacity, IEqualityComparer<T> comparer)
			: this(concurrencyLevel, capacity, false, comparer)
		{
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public ConcurrentHashSet(int concurrencyLevel, int capacity, bool growLockArray, IEqualityComparer<T> comparer)
		{
			if (concurrencyLevel < 1)
			{
				throw new ArgumentOutOfRangeException("concurrencyLevel");
			}
			if (capacity < 0)
			{
				throw new ArgumentOutOfRangeException("capacity");
			}
			if (capacity < concurrencyLevel)
			{
				capacity = concurrencyLevel;
			}
			object[] array = new object[concurrencyLevel];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = new object();
			}
			int[] array2 = new int[array.Length];
			ConcurrentHashSet<T>.Node[] array3 = new ConcurrentHashSet<T>.Node[capacity];
			this._tables = new ConcurrentHashSet<T>.Tables(array3, array, array2);
			this._growLockArray = growLockArray;
			this._budget = array3.Length / array.Length;
			this._comparer = comparer ?? EqualityComparer<T>.Default;
		}

		public bool Add(T item)
		{
			return this.AddInternal(item, this._comparer.GetHashCode(item), true);
		}

		public void Clear()
		{
			int num = 0;
			try
			{
				this.AcquireAllLocks(ref num);
				ConcurrentHashSet<T>.Tables tables = new ConcurrentHashSet<T>.Tables(new ConcurrentHashSet<T>.Node[31], this._tables.Locks, new int[this._tables.CountPerLock.Length]);
				this._tables = tables;
				this._budget = Math.Max(1, tables.Buckets.Length / tables.Locks.Length);
			}
			finally
			{
				this.ReleaseLocks(0, num);
			}
		}

		public bool Contains(T item)
		{
			int hashCode = this._comparer.GetHashCode(item);
			ConcurrentHashSet<T>.Tables tables = this._tables;
			int bucket = ConcurrentHashSet<T>.GetBucket(hashCode, tables.Buckets.Length);
			for (ConcurrentHashSet<T>.Node node = Volatile.Read<ConcurrentHashSet<T>.Node>(ref tables.Buckets[bucket]); node != null; node = node.Next)
			{
				if (hashCode == node.Hashcode && this._comparer.Equals(node.Item, item))
				{
					return true;
				}
			}
			return false;
		}

		public bool TryRemove(T item)
		{
			int hashCode = this._comparer.GetHashCode(item);
			for (;;)
			{
				ConcurrentHashSet<T>.Tables tables = this._tables;
				int num;
				int num2;
				ConcurrentHashSet<T>.GetBucketAndLockNo(hashCode, out num, out num2, tables.Buckets.Length, tables.Locks.Length);
				object obj = tables.Locks[num2];
				lock (obj)
				{
					if (tables != this._tables)
					{
						continue;
					}
					ConcurrentHashSet<T>.Node node = null;
					for (ConcurrentHashSet<T>.Node node2 = tables.Buckets[num]; node2 != null; node2 = node2.Next)
					{
						if (hashCode == node2.Hashcode && this._comparer.Equals(node2.Item, item))
						{
							if (node == null)
							{
								Volatile.Write<ConcurrentHashSet<T>.Node>(ref tables.Buckets[num], node2.Next);
							}
							else
							{
								node.Next = node2.Next;
							}
							tables.CountPerLock[num2]--;
							return true;
						}
						node = node2;
					}
				}
				break;
			}
			Action<T> onRemovalFailure = this.OnRemovalFailure;
			if (onRemovalFailure != null)
			{
				onRemovalFailure(item);
			}
			return false;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}

		public IEnumerator<T> GetEnumerator()
		{
			ConcurrentHashSet<T>.Node[] buckets = this._tables.Buckets;
			int num;
			for (int i = 0; i < buckets.Length; i = num + 1)
			{
				ConcurrentHashSet<T>.Node current;
				for (current = Volatile.Read<ConcurrentHashSet<T>.Node>(ref buckets[i]); current != null; current = current.Next)
				{
					yield return current.Item;
				}
				current = null;
				num = i;
			}
			yield break;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void System.Collections.Generic.ICollection<T>.Add(T item)
		{
			this.Add(item);
		}

		public bool System.Collections.Generic.ICollection<T>.IsReadOnly
		{
			[PublicizedFrom(EAccessModifier.Private)]
			get
			{
				return false;
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void System.Collections.Generic.ICollection<T>.CopyTo(T[] array, int arrayIndex)
		{
			if (array == null)
			{
				throw new ArgumentNullException("array");
			}
			if (arrayIndex < 0)
			{
				throw new ArgumentOutOfRangeException("arrayIndex");
			}
			int num = 0;
			try
			{
				this.AcquireAllLocks(ref num);
				int num2 = 0;
				int num3 = 0;
				while (num3 < this._tables.Locks.Length && num2 >= 0)
				{
					num2 += this._tables.CountPerLock[num3];
					num3++;
				}
				if (array.Length - num2 < arrayIndex || num2 < 0)
				{
					throw new ArgumentException("The index is equal to or greater than the length of the array, or the number of elements in the set is greater than the available space from index to the end of the destination array.");
				}
				this.CopyToItems(array, arrayIndex);
			}
			finally
			{
				this.ReleaseLocks(0, num);
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public bool System.Collections.Generic.ICollection<T>.Remove(T item)
		{
			return this.TryRemove(item);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void InitializeFromCollection(IEnumerable<T> collection)
		{
			foreach (T t in collection)
			{
				this.AddInternal(t, this._comparer.GetHashCode(t), false);
			}
			if (this._budget == 0)
			{
				this._budget = this._tables.Buckets.Length / this._tables.Locks.Length;
			}
		}

		public bool TryFirst(out T returnValue)
		{
			if (this._tables == null || this._tables.Buckets == null || this._tables.Buckets.Length == 0)
			{
				returnValue = default(T);
				return false;
			}
			ConcurrentHashSet<T>.Node node = this._tables.Buckets.FirstOrDefault((ConcurrentHashSet<T>.Node d) => d != null);
			if (node == null)
			{
				returnValue = default(T);
				return false;
			}
			returnValue = node.Item;
			return true;
		}

		public bool TryRemoveFirst(out T returnValue)
		{
			if (this._tables == null || this._tables.Buckets == null || this._tables.Buckets.Length == 0)
			{
				returnValue = default(T);
				return false;
			}
			ConcurrentHashSet<T>.Node node = this._tables.Buckets.FirstOrDefault((ConcurrentHashSet<T>.Node d) => d != null);
			if (node == null)
			{
				returnValue = default(T);
				return false;
			}
			returnValue = node.Item;
			this.TryRemove(returnValue);
			return true;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public bool AddInternal(T item, int hashcode, bool acquireLock)
		{
			checked
			{
				ConcurrentHashSet<T>.Tables tables;
				bool flag;
				for (;;)
				{
					tables = this._tables;
					int num;
					int num2;
					ConcurrentHashSet<T>.GetBucketAndLockNo(hashcode, out num, out num2, tables.Buckets.Length, tables.Locks.Length);
					flag = false;
					bool flag2 = false;
					try
					{
						if (acquireLock)
						{
							Monitor.Enter(tables.Locks[num2], ref flag2);
						}
						if (tables != this._tables)
						{
							continue;
						}
						for (ConcurrentHashSet<T>.Node node = tables.Buckets[num]; node != null; node = node.Next)
						{
							if (hashcode == node.Hashcode && this._comparer.Equals(node.Item, item))
							{
								return false;
							}
						}
						Volatile.Write<ConcurrentHashSet<T>.Node>(ref tables.Buckets[num], new ConcurrentHashSet<T>.Node(item, hashcode, tables.Buckets[num]));
						tables.CountPerLock[num2]++;
						if (tables.CountPerLock[num2] > this._budget)
						{
							flag = true;
						}
					}
					finally
					{
						if (flag2)
						{
							Monitor.Exit(tables.Locks[num2]);
						}
					}
					break;
				}
				if (flag)
				{
					this.GrowTable(tables);
				}
				return true;
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static int GetBucket(int hashcode, int bucketCount)
		{
			return (hashcode & int.MaxValue) % bucketCount;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static void GetBucketAndLockNo(int hashcode, out int bucketNo, out int lockNo, int bucketCount, int lockCount)
		{
			bucketNo = (hashcode & int.MaxValue) % bucketCount;
			lockNo = bucketNo % lockCount;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void GrowTable(ConcurrentHashSet<T>.Tables tables)
		{
			int num = 0;
			try
			{
				this.AcquireLocks(0, 1, ref num);
				if (tables == this._tables)
				{
					long num2 = 0L;
					for (int i = 0; i < tables.CountPerLock.Length; i++)
					{
						num2 += (long)tables.CountPerLock[i];
					}
					if (num2 < (long)(tables.Buckets.Length / 4))
					{
						this._budget = 2 * this._budget;
						if (this._budget < 0)
						{
							this._budget = int.MaxValue;
						}
					}
					else
					{
						int num3 = 0;
						bool flag = false;
						object[] array;
						checked
						{
							try
							{
								num3 = tables.Buckets.Length * 2 + 1;
								while (num3 % 3 == 0 || num3 % 5 == 0 || num3 % 7 == 0)
								{
									num3 += 2;
								}
								if (num3 > 2146435071)
								{
									flag = true;
								}
							}
							catch (OverflowException)
							{
								flag = true;
							}
							if (flag)
							{
								num3 = 2146435071;
								this._budget = int.MaxValue;
							}
							this.AcquireLocks(1, tables.Locks.Length, ref num);
							array = tables.Locks;
						}
						if (this._growLockArray && tables.Locks.Length < 1024)
						{
							array = new object[tables.Locks.Length * 2];
							Array.Copy(tables.Locks, 0, array, 0, tables.Locks.Length);
							for (int j = tables.Locks.Length; j < array.Length; j++)
							{
								array[j] = new object();
							}
						}
						ConcurrentHashSet<T>.Node[] array2 = new ConcurrentHashSet<T>.Node[num3];
						int[] array3 = new int[array.Length];
						for (int k = 0; k < tables.Buckets.Length; k++)
						{
							checked
							{
								ConcurrentHashSet<T>.Node next;
								for (ConcurrentHashSet<T>.Node node = tables.Buckets[k]; node != null; node = next)
								{
									next = node.Next;
									int num4;
									int num5;
									ConcurrentHashSet<T>.GetBucketAndLockNo(node.Hashcode, out num4, out num5, array2.Length, array.Length);
									array2[num4] = new ConcurrentHashSet<T>.Node(node.Item, node.Hashcode, array2[num4]);
									array3[num5]++;
								}
							}
						}
						this._budget = Math.Max(1, array2.Length / array.Length);
						this._tables = new ConcurrentHashSet<T>.Tables(array2, array, array3);
					}
				}
			}
			finally
			{
				this.ReleaseLocks(0, num);
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void AcquireAllLocks(ref int locksAcquired)
		{
			this.AcquireLocks(0, 1, ref locksAcquired);
			this.AcquireLocks(1, this._tables.Locks.Length, ref locksAcquired);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void AcquireLocks(int fromInclusive, int toExclusive, ref int locksAcquired)
		{
			object[] locks = this._tables.Locks;
			for (int i = fromInclusive; i < toExclusive; i++)
			{
				bool flag = false;
				try
				{
					Monitor.Enter(locks[i], ref flag);
				}
				finally
				{
					if (flag)
					{
						locksAcquired++;
					}
				}
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void ReleaseLocks(int fromInclusive, int toExclusive)
		{
			for (int i = fromInclusive; i < toExclusive; i++)
			{
				Monitor.Exit(this._tables.Locks[i]);
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void CopyToItems(T[] array, int index)
		{
			foreach (ConcurrentHashSet<T>.Node node in this._tables.Buckets)
			{
				while (node != null)
				{
					array[index] = node.Item;
					index++;
					node = node.Next;
				}
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public const int DefaultCapacity = 31;

		[PublicizedFrom(EAccessModifier.Private)]
		public const int MaxLockNumber = 1024;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly IEqualityComparer<T> _comparer;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly bool _growLockArray;

		[PublicizedFrom(EAccessModifier.Private)]
		public int _budget;

		[PublicizedFrom(EAccessModifier.Private)]
		public volatile ConcurrentHashSet<T>.Tables _tables;

		public Action<T> OnRemovalFailure;

		[PublicizedFrom(EAccessModifier.Private)]
		public class Tables
		{
			public Tables(ConcurrentHashSet<T>.Node[] buckets, object[] locks, int[] countPerLock)
			{
				this.Buckets = buckets;
				this.Locks = locks;
				this.CountPerLock = countPerLock;
			}

			public readonly ConcurrentHashSet<T>.Node[] Buckets;

			public readonly object[] Locks;

			public volatile int[] CountPerLock;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public class Node
		{
			public Node(T item, int hashcode, ConcurrentHashSet<T>.Node next)
			{
				this.Item = item;
				this.Hashcode = hashcode;
				this.Next = next;
			}

			public readonly T Item;

			public readonly int Hashcode;

			public volatile ConcurrentHashSet<T>.Node Next;
		}
	}
}
