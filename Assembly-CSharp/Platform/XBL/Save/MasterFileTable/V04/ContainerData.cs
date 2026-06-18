using System;

namespace Platform.XBL.Save.MasterFileTable.V04
{
	public sealed class ContainerData : IDisposable, IMigratable
	{
		public ushort Version
		{
			get
			{
				return 4;
			}
		}

		public Node RootNode
		{
			get; [PublicizedFrom(EAccessModifier.Private)]
			set;
		} = new Node
		{
			Attributes = NodeAttributes.Directory
		};

		public void Write(PooledBinaryWriter writer)
		{
			this.RootNode.Attributes = NodeAttributes.Directory;
			writer.Write(4);
			this.RootNode.Write(writer);
		}

		public void Read(PooledBinaryReader reader)
		{
			reader.ReadUInt16();
			this.RootNode.Read(reader);
			this.RootNode.Attributes = NodeAttributes.Directory;
		}

		public void Dispose()
		{
			Node rootNode = this.RootNode;
			if (rootNode != null)
			{
				rootNode.Dispose();
			}
			this.RootNode = null;
		}

		public const ushort VERSION = 4;
	}
}
