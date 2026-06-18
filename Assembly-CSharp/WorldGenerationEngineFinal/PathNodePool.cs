using System;
using Unity.Burst;
using Unity.Collections;

namespace WorldGenerationEngineFinal
{
	[BurstCompile(CompileSynchronously = true)]
	public struct PathNodePool
	{
		public PathNodePool(int _size)
		{
			this.pool = new NativeList<PathNode>(_size, 4);
			PathNode pathNode = default(PathNode);
			this.pool.AddReplicate(ref pathNode, _size);
			this.free = 0;
		}

		public void Init(int _size)
		{
			this.pool = new NativeList<PathNode>(_size, 4);
			PathNode pathNode = default(PathNode);
			this.pool.AddReplicate(ref pathNode, _size);
			this.free = 0;
		}

		public int Alloc()
		{
			int length = this.pool.Length;
			if (this.free >= length)
			{
				this.pool.ResizeUninitialized(length + 10000);
			}
			int num = this.free;
			this.free = num + 1;
			return num;
		}

		public void ReturnAll()
		{
			this.free = 0;
		}

		public void Node(int _index, out PathNode _node)
		{
			_node = this.pool[_index];
		}

		public ref PathNode Node(int _index)
		{
			return this.pool.ElementAt(_index);
		}

		public void Cleanup()
		{
			this.pool.Dispose();
		}

		public void LogStats()
		{
			Log.Out(string.Format("PathNodePool: Capacity={0}, Allocated={1}, Free={2}", this.pool.Capacity, this.pool.Length, this.free));
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public NativeList<PathNode> pool;

		[PublicizedFrom(EAccessModifier.Private)]
		public int free;
	}
}
