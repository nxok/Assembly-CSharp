using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;

public struct UnsafeBitArraySetIndicesEnumerator : IEnumerator<int>, IEnumerator, IDisposable
{
	public unsafe UnsafeBitArraySetIndicesEnumerator(UnsafeBitArray bitArray)
	{
		this.bitArray = bitArray;
		this.currentSlice = *bitArray.Ptr;
		this.sliceIndex = 0;
		this.numSlices = bitArray.Length / 64;
		this.currentIndex = -1;
		this.leadingZeroCount = 0;
		this.numSetBits = bitArray.CountBits(0, bitArray.Length);
		this.foundBits = 0;
	}

	public int Current
	{
		get
		{
			return this.currentIndex;
		}
	}

	public object System.Collections.IEnumerator.Current
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return this.Current;
		}
	}

	public unsafe bool MoveNext()
	{
		while (this.foundBits < this.numSetBits && this.sliceIndex < this.numSlices)
		{
			if (this.currentSlice == 0UL)
			{
				this.sliceIndex++;
				if (this.sliceIndex < this.numSlices)
				{
					this.currentSlice = this.bitArray.Ptr[this.sliceIndex];
					this.leadingZeroCount = 0;
				}
			}
			else
			{
				if ((this.currentSlice & 1UL) != 0UL)
				{
					this.currentIndex = this.leadingZeroCount + this.sliceIndex * 64;
					this.leadingZeroCount++;
					this.currentSlice >>= 1;
					this.foundBits++;
					return true;
				}
				if ((this.currentSlice & (ulong)(-1)) == 0UL)
				{
					this.leadingZeroCount += 32;
					this.currentSlice >>= 32;
				}
				if ((this.currentSlice & 65535UL) == 0UL)
				{
					this.leadingZeroCount += 16;
					this.currentSlice >>= 16;
				}
				if ((this.currentSlice & 255UL) == 0UL)
				{
					this.leadingZeroCount += 8;
					this.currentSlice >>= 8;
				}
				if ((this.currentSlice & 15UL) == 0UL)
				{
					this.leadingZeroCount += 4;
					this.currentSlice >>= 4;
				}
				if ((this.currentSlice & 3UL) == 0UL)
				{
					this.leadingZeroCount += 2;
					this.currentSlice >>= 2;
				}
				if ((this.currentSlice & 1UL) == 0UL)
				{
					this.leadingZeroCount++;
					this.currentSlice >>= 1;
				}
			}
		}
		return false;
	}

	public unsafe void Reset()
	{
		this.sliceIndex = 0;
		this.currentSlice = *this.bitArray.Ptr;
		this.currentIndex = -1;
		this.leadingZeroCount = 0;
		this.foundBits = 0;
	}

	public void Dispose()
	{
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public UnsafeBitArray bitArray;

	[PublicizedFrom(EAccessModifier.Private)]
	public ulong currentSlice;

	[PublicizedFrom(EAccessModifier.Private)]
	public int sliceIndex;

	[PublicizedFrom(EAccessModifier.Private)]
	public int numSlices;

	[PublicizedFrom(EAccessModifier.Private)]
	public int leadingZeroCount;

	[PublicizedFrom(EAccessModifier.Private)]
	public int currentIndex;

	[PublicizedFrom(EAccessModifier.Private)]
	public int numSetBits;

	[PublicizedFrom(EAccessModifier.Private)]
	public int foundBits;
}
