using System;
using System.Security.Cryptography;
using System.Threading;

namespace Platform.XBL.Save.MasterFileTable.V04
{
	public sealed class BlobRef : IEquatable<BlobRef>
	{
		public BlobRef(ulong id, uint length)
			: this(id, length, BlobRef.s_emptyHash.AsMemory<byte>())
		{
		}

		public BlobRef(ulong id, uint length, ReadOnlySpan<byte> hash)
		{
			this.Id = id;
			this.Length = length;
			if (16 != hash.Length)
			{
				throw new InvalidOperationException("Expected hash to be of length " + 16.ToString());
			}
			this.Hash = hash.ToArray();
		}

		public BlobRef(ulong id, uint length, ReadOnlyMemory<byte> hash)
		{
			this.Id = id;
			this.Length = length;
			if (16 != hash.Length)
			{
				throw new InvalidOperationException("Expected hash to be of length " + 16.ToString());
			}
			this.Hash = hash;
		}

		public BlobRef(ulong id, RefCountedBuffer buffer)
		{
			this.Id = id;
			this.Length = (uint)buffer.Length;
			HashAlgorithm hashAlgorithm = BlobRef.GetHashAlgorithm();
			if (128 != hashAlgorithm.HashSize)
			{
				throw new InvalidOperationException("Unexpected hash algorithm hash size.");
			}
			byte[] array = new byte[16];
			int num;
			if (!hashAlgorithm.TryComputeHash(buffer.Span, array, out num))
			{
				throw new InvalidOperationException("Expected to be able to compute the hash.");
			}
			this.Hash = array;
		}

		public void Write(PooledBinaryWriter writer)
		{
			writer.Write(this.Id);
			writer.Write(this.Length);
			if (16 == this.Hash.Length)
			{
				writer.Write(this.Hash.Span);
				return;
			}
			Log.Error("[BlobRef] Expected a hash size of exactly " + 16.ToString() + ", padding or truncating as needed.");
			if (this.Hash.Length < 16)
			{
				writer.Write(this.Hash.Span);
				for (int i = this.Hash.Length; i < 16; i++)
				{
					writer.Write(0);
				}
				return;
			}
			writer.Write(this.Hash.Span.Slice(0, 16));
		}

		public static BlobRef Read(PooledBinaryReader reader)
		{
			ulong num = reader.ReadUInt64();
			uint num2 = reader.ReadUInt32();
			byte[] array = new byte[16];
			int num3;
			if (!reader.TryReadAllBytes(array, out num3))
			{
				Log.Error(string.Format("[BlobRef] Expected {0} for the hash bytes, but reached the end of stream after reading {1} bytes?", 16, num3));
			}
			return new BlobRef(num, num2, array.AsMemory<byte>());
		}

		public override string ToString()
		{
			return string.Concat(new string[]
			{
				"BlobRef[Id=",
				SaveContainer.IdToString(this.Id),
				", Length=",
				this.Length.FormatSize(false),
				", Hash=",
				this.Hash.ToHexString(),
				"]"
			});
		}

		public static HashAlgorithm GetHashAlgorithm()
		{
			return BlobRef.s_md5.Value;
		}

		public bool Equals(BlobRef other)
		{
			return other != null && (this == other || (this.Id == other.Id && this.Length == other.Length && (this.Hash.Equals(other.Hash) || this.Hash.Span.SequenceEqual(other.Hash.Span))));
		}

		public override bool Equals(object obj)
		{
			if (this != obj)
			{
				BlobRef blobRef = obj as BlobRef;
				return blobRef != null && this.Equals(blobRef);
			}
			return true;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine<ulong, uint, int>(this.Id, this.Length, SpanUtils.GetHashCode<byte>(this.Hash.Span));
		}

		public static bool operator ==(BlobRef left, BlobRef right)
		{
			return object.Equals(left, right);
		}

		public static bool operator !=(BlobRef left, BlobRef right)
		{
			return !object.Equals(left, right);
		}

		public const int HashSizeBytes = 16;

		public const int SizeBytes = 28;

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly ThreadLocal<MD5> s_md5 = new ThreadLocal<MD5>(new Func<MD5>(MD5.Create));

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly byte[] s_emptyHash = new byte[16];

		public readonly ulong Id;

		public readonly uint Length;

		public readonly ReadOnlyMemory<byte> Hash;
	}
}
