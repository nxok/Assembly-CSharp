using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Platform.XBL.Save.MasterFileTable.V02
{
	public sealed class Node : IDisposable
	{
		public string Name
		{
			get; [PublicizedFrom(EAccessModifier.Private)]
			set;
		}

		public Node()
		{
			this.Name = string.Empty;
			this.LastWriteTimeUtc = Node.DefaultLastWriteTime;
			this.BlobIds = new List<ulong>();
			this.Children = new SortedDictionary<string, Node>();
			this.m_lock = new object();
			this.Parent = null;
		}

		public override string ToString()
		{
			object @lock = this.m_lock;
			string text;
			lock (@lock)
			{
				text = string.Format("{0}[{1}=\"{2}\", {3}=[{4}], #{5}={6}]", new object[]
				{
					"Node",
					"Name",
					this.Name,
					"BlobIds",
					string.Join(", ", this.BlobIds.Select(new Func<ulong, string>(SaveContainer.IdToString))),
					"Children",
					this.Children.Count
				});
			}
			return text;
		}

		public void Write(PooledBinaryWriter writer)
		{
			object @lock = this.m_lock;
			lock (@lock)
			{
				writer.Write(this.Name);
				writer.Write(this.LastWriteTimeUtc.Ticks);
				ushort num = (ushort)this.BlobIds.Count;
				writer.Write(num);
				foreach (ulong num2 in this.BlobIds)
				{
					writer.Write(num2);
				}
				ushort num3 = (ushort)this.Children.Count;
				writer.Write(num3);
				foreach (Node node in this.Children.Values)
				{
					node.Write(writer);
				}
			}
		}

		public void Read(PooledBinaryReader reader)
		{
			object @lock = this.m_lock;
			lock (@lock)
			{
				this.Name = reader.ReadString();
				this.LastWriteTimeUtc = new DateTime(reader.ReadInt64(), DateTimeKind.Utc);
				ushort num = reader.ReadUInt16();
				for (int i = 0; i < (int)num; i++)
				{
					ulong num2 = reader.ReadUInt64();
					this.BlobIds.Add(num2);
				}
				ushort num3 = reader.ReadUInt16();
				for (int j = 0; j < (int)num3; j++)
				{
					Node node = new Node();
					node.Read(reader);
					this.Children.Add(node.Name, node);
					node.Parent = this;
				}
			}
		}

		public void Dispose()
		{
			object @lock = this.m_lock;
			lock (@lock)
			{
				foreach (Node node in this.Children.Values)
				{
					node.Dispose();
				}
				this.Children.Clear();
				this.Parent = null;
				this.BlobIds.Clear();
				this.Name = string.Empty;
			}
		}

		public bool IsDirectory()
		{
			object @lock = this.m_lock;
			bool flag2;
			lock (@lock)
			{
				flag2 = this.BlobIds.Count == 0;
			}
			return flag2;
		}

		public bool IsFile()
		{
			object @lock = this.m_lock;
			bool flag2;
			lock (@lock)
			{
				flag2 = this.Children.Count == 0;
			}
			return flag2;
		}

		public Node GetChildNode(string name)
		{
			object @lock = this.m_lock;
			Node node2;
			lock (@lock)
			{
				Node node;
				if (!this.Children.TryGetValue(name, out node))
				{
					node2 = null;
				}
				else
				{
					node2 = node;
				}
			}
			return node2;
		}

		public Node GetOrCreateChildNode(string name)
		{
			object @lock = this.m_lock;
			Node node2;
			lock (@lock)
			{
				Node node;
				if (!this.Children.TryGetValue(name, out node))
				{
					node = new Node
					{
						Name = name
					};
					node.Parent = this;
					this.Children.Add(name, node);
				}
				node2 = node;
			}
			return node2;
		}

		public IEnumerable<Node> Enumerate(bool includeSelf, bool recursive)
		{
			List<Node> list = new List<Node>();
			if (!recursive)
			{
				if (includeSelf)
				{
					list.Add(this);
				}
				object @lock = this.m_lock;
				lock (@lock)
				{
					list.AddRange(this.Children.Values);
					return list;
				}
			}
			Stack<Node> stack = new Stack<Node>();
			if (includeSelf)
			{
				stack.Push(this);
			}
			else
			{
				Node.<Enumerate>g__AddNodeChildrenToStack|19_0(stack, this);
			}
			while (stack.Count > 0)
			{
				Node node = stack.Pop();
				list.Add(node);
				Node.<Enumerate>g__AddNodeChildrenToStack|19_0(stack, node);
			}
			return list;
		}

		[CompilerGenerated]
		[PublicizedFrom(EAccessModifier.Internal)]
		public static void <Enumerate>g__AddNodeChildrenToStack|19_0(Stack<Node> stack, Node currentNode)
		{
			object @lock = currentNode.m_lock;
			lock (@lock)
			{
				foreach (Node node in currentNode.Children.Values.Reverse<Node>())
				{
					stack.Push(node);
				}
			}
		}

		public static readonly DateTime DefaultLastWriteTime = new DateTime(0L, DateTimeKind.Utc);

		public DateTime LastWriteTimeUtc;

		public readonly List<ulong> BlobIds;

		public readonly SortedDictionary<string, Node> Children;

		public readonly object m_lock;

		public Node Parent;
	}
}
