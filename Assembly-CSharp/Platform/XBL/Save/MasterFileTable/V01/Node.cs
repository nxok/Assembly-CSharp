using System;
using System.Collections.Generic;
using System.Linq;

namespace Platform.XBL.Save.MasterFileTable.V01
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
			this.BlobIds = new List<ulong>();
			this.Children = new SortedDictionary<string, Node>();
			this.Parent = null;
		}

		public override string ToString()
		{
			return string.Format("{0}[{1}=\"{2}\", {3}=[{4}], #{5}={6}]", new object[]
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

		public void Write(PooledBinaryWriter writer)
		{
			writer.Write(this.Name);
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

		public void Read(PooledBinaryReader reader)
		{
			this.Name = reader.ReadString();
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

		public void Dispose()
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

		public bool IsDirectory()
		{
			return this.BlobIds.Count == 0;
		}

		public bool IsFile()
		{
			return this.Children.Count == 0;
		}

		public Node GetChildNode(string name)
		{
			Node node;
			if (!this.Children.TryGetValue(name, out node))
			{
				return null;
			}
			return node;
		}

		public Node GetOrCreateChildNode(string name)
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
			return node;
		}

		public readonly List<ulong> BlobIds;

		public readonly SortedDictionary<string, Node> Children;

		public Node Parent;
	}
}
