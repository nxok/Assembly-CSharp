using System;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace System.Collections.Generic
{
	[DebuggerDisplay("Count={Count}")]
	[Serializable]
	public class HashSetLong : ICollection<long>, IEnumerable<long>, IEnumerable, ISerializable, IDeserializationCallback
	{
		public int Count
		{
			get
			{
				return this.count;
			}
		}

		public HashSetLong()
		{
			this.Init(10, null);
		}

		public HashSetLong(IEqualityComparer<long> comparer)
		{
			this.Init(10, comparer);
		}

		public HashSetLong(IEnumerable<long> collection)
			: this(collection, null)
		{
		}

		public HashSetLong(IEnumerable<long> collection, IEqualityComparer<long> comparer)
		{
			if (collection == null)
			{
				throw new ArgumentNullException("collection");
			}
			int num = 0;
			ICollection<long> collection2 = collection as ICollection<long>;
			if (collection2 != null)
			{
				num = collection2.Count;
			}
			this.Init(num, comparer);
			foreach (long num2 in collection)
			{
				this.Add(num2);
			}
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public HashSetLong(SerializationInfo info, StreamingContext context)
		{
			this.si = info;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void Init(int capacity, IEqualityComparer<long> comparer)
		{
			if (capacity < 0)
			{
				throw new ArgumentOutOfRangeException("capacity");
			}
			this.comparer = comparer ?? EqualityComparer<long>.Default;
			if (capacity == 0)
			{
				capacity = 10;
			}
			capacity = (int)((float)capacity / 0.9f) + 1;
			this.InitArrays(capacity);
			this.generation = 0;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void InitArrays(int size)
		{
			this.table = new int[size];
			this.links = new HashSetLong.Link[size];
			this.empty_slot = -1;
			this.slots = new long[size];
			this.touched = 0;
			this.threshold = (int)((float)this.table.Length * 0.9f);
			if (this.threshold == 0 && this.table.Length != 0)
			{
				this.threshold = 1;
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public bool SlotsContainsAt(int index, int hash, long item)
		{
			HashSetLong.Link link;
			for (int num = this.table[index] - 1; num != -1; num = link.Next)
			{
				link = this.links[num];
				if (link.HashCode == hash && item == this.slots[num])
				{
					return true;
				}
			}
			return false;
		}

		public void CopyTo(long[] array)
		{
			this.CopyTo(array, 0, this.count);
		}

		public void CopyTo(long[] array, int arrayIndex)
		{
			this.CopyTo(array, arrayIndex, this.count);
		}

		public void CopyTo(long[] array, int arrayIndex, int count)
		{
			if (array == null)
			{
				throw new ArgumentNullException("array");
			}
			if (arrayIndex < 0)
			{
				throw new ArgumentOutOfRangeException("arrayIndex");
			}
			if (arrayIndex > array.Length)
			{
				throw new ArgumentException("index larger than largest valid index of array");
			}
			if (array.Length - arrayIndex < count)
			{
				throw new ArgumentException("Destination array cannot hold the requested elements!");
			}
			int num = 0;
			int num2 = 0;
			while (num < this.touched && num2 < count)
			{
				if (this.GetLinkHashCode(num) != 0)
				{
					array[arrayIndex++] = this.slots[num];
				}
				num++;
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void Resize()
		{
			int num = HashSetLong.PrimeHelper.ToPrime((this.table.Length << 1) | 1);
			int[] array = new int[num];
			HashSetLong.Link[] array2 = new HashSetLong.Link[num];
			for (int i = 0; i < this.table.Length; i++)
			{
				for (int num2 = this.table[i] - 1; num2 != -1; num2 = this.links[num2].Next)
				{
					int num3 = ((array2[num2].HashCode = ((int)this.slots[num2] ^ (int)(this.slots[num2] >> 32)) | int.MinValue) & int.MaxValue) % num;
					array2[num2].Next = array[num3] - 1;
					array[num3] = num2 + 1;
				}
			}
			this.table = array;
			this.links = array2;
			long[] array3 = new long[num];
			Array.Copy(this.slots, 0, array3, 0, this.touched);
			this.slots = array3;
			this.threshold = (int)((float)num * 0.9f);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public int GetLinkHashCode(int index)
		{
			return this.links[index].HashCode & int.MinValue;
		}

		public bool Add(long item)
		{
			int num = ((int)item ^ (int)(item >> 32)) | int.MinValue;
			int num2 = (num & int.MaxValue) % this.table.Length;
			if (this.SlotsContainsAt(num2, num, item))
			{
				return false;
			}
			int num3 = this.count + 1;
			this.count = num3;
			if (num3 > this.threshold)
			{
				this.Resize();
				num2 = (num & int.MaxValue) % this.table.Length;
			}
			int num4 = this.empty_slot;
			if (num4 == -1)
			{
				num3 = this.touched;
				this.touched = num3 + 1;
				num4 = num3;
			}
			else
			{
				this.empty_slot = this.links[num4].Next;
			}
			this.links[num4].HashCode = num;
			this.links[num4].Next = this.table[num2] - 1;
			this.table[num2] = num4 + 1;
			this.slots[num4] = item;
			this.generation++;
			return true;
		}

		public IEqualityComparer<long> Comparer
		{
			get
			{
				return this.comparer;
			}
		}

		public void Clear()
		{
			this.count = 0;
			Array.Clear(this.table, 0, this.table.Length);
			Array.Clear(this.slots, 0, this.slots.Length);
			Array.Clear(this.links, 0, this.links.Length);
			this.empty_slot = -1;
			this.touched = 0;
			this.generation++;
		}

		public bool Contains(long item)
		{
			int num = ((int)item ^ (int)(item >> 32)) | int.MinValue;
			int num2 = (num & int.MaxValue) % this.table.Length;
			return this.SlotsContainsAt(num2, num, item);
		}

		public bool Remove(long item)
		{
			int num = ((int)item ^ (int)(item >> 32)) | int.MinValue;
			int num2 = (num & int.MaxValue) % this.table.Length;
			int num3 = this.table[num2] - 1;
			if (num3 == -1)
			{
				return false;
			}
			int num4 = -1;
			do
			{
				HashSetLong.Link link = this.links[num3];
				if (link.HashCode == num && this.slots[num3] == item)
				{
					break;
				}
				num4 = num3;
				num3 = link.Next;
			}
			while (num3 != -1);
			if (num3 == -1)
			{
				return false;
			}
			this.count--;
			if (num4 == -1)
			{
				this.table[num2] = this.links[num3].Next + 1;
			}
			else
			{
				this.links[num4].Next = this.links[num3].Next;
			}
			this.links[num3].Next = this.empty_slot;
			this.empty_slot = num3;
			this.links[num3].HashCode = 0;
			this.slots[num3] = 0L;
			this.generation++;
			return true;
		}

		public int RemoveWhere(Predicate<long> match)
		{
			if (match == null)
			{
				throw new ArgumentNullException("match");
			}
			List<long> list = new List<long>();
			foreach (long num in this)
			{
				if (match(num))
				{
					list.Add(num);
				}
			}
			foreach (long num2 in list)
			{
				this.Remove(num2);
			}
			return list.Count;
		}

		public void TrimExcess()
		{
			this.Resize();
		}

		public void IntersectWith(IEnumerable<long> other)
		{
			if (other == null)
			{
				throw new ArgumentNullException("other");
			}
			HashSetLong other_set = this.ToSet(other);
			this.RemoveWhere((long item) => !other_set.Contains(item));
		}

		public void ExceptWithHashSetLong(HashSetLong other)
		{
			if (other == null)
			{
				throw new ArgumentNullException("other");
			}
			foreach (long num in other)
			{
				this.Remove(num);
			}
		}

		public void ExceptWith(IEnumerable<long> other)
		{
			if (other == null)
			{
				throw new ArgumentNullException("other");
			}
			foreach (long num in other)
			{
				this.Remove(num);
			}
		}

		public bool Overlaps(IEnumerable<long> other)
		{
			if (other == null)
			{
				throw new ArgumentNullException("other");
			}
			foreach (long num in other)
			{
				if (this.Contains(num))
				{
					return true;
				}
			}
			return false;
		}

		public bool SetEquals(IEnumerable<long> other)
		{
			if (other == null)
			{
				throw new ArgumentNullException("other");
			}
			HashSetLong hashSetLong = this.ToSet(other);
			if (this.count != hashSetLong.Count)
			{
				return false;
			}
			foreach (long num in this)
			{
				if (!hashSetLong.Contains(num))
				{
					return false;
				}
			}
			return true;
		}

		public void SymmetricExceptWith(IEnumerable<long> other)
		{
			if (other == null)
			{
				throw new ArgumentNullException("other");
			}
			foreach (long num in this.ToSet(other))
			{
				if (!this.Add(num))
				{
					this.Remove(num);
				}
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public HashSetLong ToSet(IEnumerable<long> enumerable)
		{
			HashSetLong hashSetLong = enumerable as HashSetLong;
			if (hashSetLong == null || !this.Comparer.Equals(hashSetLong.Comparer))
			{
				hashSetLong = new HashSetLong(enumerable, this.Comparer);
			}
			return hashSetLong;
		}

		public void UnionWithHashSetLong(HashSetLong other)
		{
			if (other == null)
			{
				throw new ArgumentNullException("other");
			}
			foreach (long num in other)
			{
				this.Add(num);
			}
		}

		public void UnionWith(IEnumerable<long> other)
		{
			if (other == null)
			{
				throw new ArgumentNullException("other");
			}
			foreach (long num in other)
			{
				this.Add(num);
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public bool CheckIsSubsetOf(HashSetLong other)
		{
			if (other == null)
			{
				throw new ArgumentNullException("other");
			}
			foreach (long num in this)
			{
				if (!other.Contains(num))
				{
					return false;
				}
			}
			return true;
		}

		public bool IsSubsetOf(IEnumerable<long> other)
		{
			if (other == null)
			{
				throw new ArgumentNullException("other");
			}
			if (this.count == 0)
			{
				return true;
			}
			HashSetLong hashSetLong = this.ToSet(other);
			return this.count <= hashSetLong.Count && this.CheckIsSubsetOf(hashSetLong);
		}

		public bool IsProperSubsetOf(IEnumerable<long> other)
		{
			if (other == null)
			{
				throw new ArgumentNullException("other");
			}
			if (this.count == 0)
			{
				return true;
			}
			HashSetLong hashSetLong = this.ToSet(other);
			return this.count < hashSetLong.Count && this.CheckIsSubsetOf(hashSetLong);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public bool CheckIsSupersetOf(HashSetLong other)
		{
			if (other == null)
			{
				throw new ArgumentNullException("other");
			}
			foreach (long num in other)
			{
				if (!this.Contains(num))
				{
					return false;
				}
			}
			return true;
		}

		public bool IsSupersetOf(IEnumerable<long> other)
		{
			if (other == null)
			{
				throw new ArgumentNullException("other");
			}
			HashSetLong hashSetLong = this.ToSet(other);
			return this.count >= hashSetLong.Count && this.CheckIsSupersetOf(hashSetLong);
		}

		public bool IsProperSupersetOf(IEnumerable<long> other)
		{
			if (other == null)
			{
				throw new ArgumentNullException("other");
			}
			HashSetLong hashSetLong = this.ToSet(other);
			return this.count > hashSetLong.Count && this.CheckIsSupersetOf(hashSetLong);
		}

		public static IEqualityComparer<HashSetLong> CreateSetComparer()
		{
			return HashSetLong.setComparer;
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
		public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			if (info == null)
			{
				throw new ArgumentNullException("info");
			}
			info.AddValue("Version", this.generation);
			info.AddValue("Comparer", this.comparer, typeof(IEqualityComparer<long>));
			info.AddValue("Capacity", (this.table == null) ? 0 : this.table.Length);
			if (this.table != null)
			{
				long[] array = new long[this.count];
				this.CopyTo(array);
				info.AddValue("Elements", array, typeof(long[]));
			}
		}

		public virtual void OnDeserialization(object sender)
		{
			if (this.si != null)
			{
				this.generation = (int)this.si.GetValue("Version", typeof(int));
				this.comparer = (IEqualityComparer<long>)this.si.GetValue("Comparer", typeof(IEqualityComparer<long>));
				int num = (int)this.si.GetValue("Capacity", typeof(int));
				this.empty_slot = -1;
				if (num > 0)
				{
					this.table = new int[num];
					this.slots = new long[num];
					long[] array = (long[])this.si.GetValue("Elements", typeof(long[]));
					if (array == null)
					{
						throw new SerializationException("Missing Elements");
					}
					for (int i = 0; i < array.Length; i++)
					{
						this.Add(array[i]);
					}
				}
				else
				{
					this.table = null;
				}
				this.si = null;
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public IEnumerator<long> System.Collections.Generic.IEnumerable<System.Int64>.GetEnumerator()
		{
			return new HashSetLong.Enumerator(this);
		}

		public bool System.Collections.Generic.ICollection<System.Int64>.IsReadOnly
		{
			[PublicizedFrom(EAccessModifier.Private)]
			get
			{
				return false;
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void System.Collections.Generic.ICollection<System.Int64>.Add(long item)
		{
			this.Add(item);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return new HashSetLong.Enumerator(this);
		}

		public HashSetLong.Enumerator GetEnumerator()
		{
			return new HashSetLong.Enumerator(this);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		[NonSerialized]
		public const int INITIAL_SIZE = 10;

		[PublicizedFrom(EAccessModifier.Private)]
		[NonSerialized]
		public const float DEFAULT_LOAD_FACTOR = 0.9f;

		[PublicizedFrom(EAccessModifier.Private)]
		[NonSerialized]
		public const int NO_SLOT = -1;

		[PublicizedFrom(EAccessModifier.Private)]
		[NonSerialized]
		public const int HASH_FLAG = -2147483648;

		[PublicizedFrom(EAccessModifier.Private)]
		[NonSerialized]
		public int[] table;

		[PublicizedFrom(EAccessModifier.Private)]
		[NonSerialized]
		public HashSetLong.Link[] links;

		[PublicizedFrom(EAccessModifier.Private)]
		[NonSerialized]
		public long[] slots;

		[PublicizedFrom(EAccessModifier.Private)]
		[NonSerialized]
		public int touched;

		[PublicizedFrom(EAccessModifier.Private)]
		[NonSerialized]
		public int empty_slot;

		[PublicizedFrom(EAccessModifier.Private)]
		[NonSerialized]
		public int count;

		[PublicizedFrom(EAccessModifier.Private)]
		[NonSerialized]
		public int threshold;

		[PublicizedFrom(EAccessModifier.Private)]
		[NonSerialized]
		public IEqualityComparer<long> comparer;

		[PublicizedFrom(EAccessModifier.Private)]
		[NonSerialized]
		public SerializationInfo si;

		[PublicizedFrom(EAccessModifier.Private)]
		[NonSerialized]
		public int generation;

		[PublicizedFrom(EAccessModifier.Private)]
		[NonSerialized]
		public static readonly HashSetLong.HashSetEqualityComparer setComparer = new HashSetLong.HashSetEqualityComparer();

		[PublicizedFrom(EAccessModifier.Private)]
		public struct Link
		{
			public int HashCode;

			public int Next;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public class HashSetEqualityComparer : IEqualityComparer<HashSetLong>
		{
			public bool Equals(HashSetLong lhs, HashSetLong rhs)
			{
				if (lhs == rhs)
				{
					return true;
				}
				if (lhs == null || rhs == null || lhs.Count != rhs.Count)
				{
					return false;
				}
				foreach (long num in lhs)
				{
					if (!rhs.Contains(num))
					{
						return false;
					}
				}
				return true;
			}

			public int GetHashCode(HashSetLong hashset)
			{
				if (hashset == null)
				{
					return 0;
				}
				IEqualityComparer<long> @default = EqualityComparer<long>.Default;
				int num = 0;
				foreach (long num2 in hashset)
				{
					num ^= @default.GetHashCode(num2);
				}
				return num;
			}
		}

		[Serializable]
		public struct Enumerator : IEnumerator<long>, IEnumerator, IDisposable
		{
			[PublicizedFrom(EAccessModifier.Internal)]
			public Enumerator(HashSetLong hashset)
			{
				this = default(HashSetLong.Enumerator);
				this.hashset = hashset;
				this.stamp = hashset.generation;
			}

			public bool MoveNext()
			{
				this.CheckState();
				if (this.next < 0)
				{
					return false;
				}
				while (this.next < this.hashset.touched)
				{
					int num = this.next;
					this.next = num + 1;
					int num2 = num;
					if (this.hashset.GetLinkHashCode(num2) != 0)
					{
						this.current = this.hashset.slots[num2];
						return true;
					}
				}
				this.next = -1;
				return false;
			}

			public long Current
			{
				get
				{
					return this.current;
				}
			}

			public object System.Collections.IEnumerator.Current
			{
				[PublicizedFrom(EAccessModifier.Private)]
				get
				{
					this.CheckState();
					if (this.next <= 0)
					{
						throw new InvalidOperationException("Current is not valid");
					}
					return this.current;
				}
			}

			[PublicizedFrom(EAccessModifier.Private)]
			public void System.Collections.IEnumerator.Reset()
			{
				this.CheckState();
				this.next = 0;
			}

			public void Dispose()
			{
				this.hashset = null;
			}

			[PublicizedFrom(EAccessModifier.Private)]
			public void CheckState()
			{
				if (this.hashset == null)
				{
					throw new ObjectDisposedException(null);
				}
				if (this.hashset.generation != this.stamp)
				{
					throw new InvalidOperationException("HashSet have been modified while it was iterated over");
				}
			}

			[PublicizedFrom(EAccessModifier.Private)]
			[NonSerialized]
			public HashSetLong hashset;

			[PublicizedFrom(EAccessModifier.Private)]
			[NonSerialized]
			public int next;

			[PublicizedFrom(EAccessModifier.Private)]
			[NonSerialized]
			public int stamp;

			[PublicizedFrom(EAccessModifier.Private)]
			[NonSerialized]
			public long current;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static class PrimeHelper
		{
			[PublicizedFrom(EAccessModifier.Private)]
			public static bool TestPrime(int x)
			{
				if ((x & 1) != 0)
				{
					int num = (int)Math.Sqrt((double)x);
					for (int i = 3; i < num; i += 2)
					{
						if (x % i == 0)
						{
							return false;
						}
					}
					return true;
				}
				return x == 2;
			}

			[PublicizedFrom(EAccessModifier.Private)]
			public static int CalcPrime(int x)
			{
				for (int i = (x & -2) - 1; i < 2147483647; i += 2)
				{
					if (HashSetLong.PrimeHelper.TestPrime(i))
					{
						return i;
					}
				}
				return x;
			}

			public static int ToPrime(int x)
			{
				for (int i = 0; i < HashSetLong.PrimeHelper.primes_table.Length; i++)
				{
					if (x <= HashSetLong.PrimeHelper.primes_table[i])
					{
						return HashSetLong.PrimeHelper.primes_table[i];
					}
				}
				return HashSetLong.PrimeHelper.CalcPrime(x);
			}

			[PublicizedFrom(EAccessModifier.Private)]
			public static readonly int[] primes_table = new int[]
			{
				11, 19, 37, 73, 109, 163, 251, 367, 557, 823,
				1237, 1861, 2777, 4177, 6247, 9371, 14057, 21089, 31627, 47431,
				71143, 106721, 160073, 240101, 360163, 540217, 810343, 1215497, 1823231, 2734867,
				4102283, 6153409, 9230113, 13845163
			};
		}
	}
}
