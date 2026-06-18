using System;

namespace Platform.XBL.Save.MasterFileTable.V01
{
	public sealed class ContainerData : IDisposable, IMigratable
	{
		public ushort Version
		{
			get
			{
				return 1;
			}
		}

		public Node RootNode
		{
			get; [PublicizedFrom(EAccessModifier.Private)]
			set;
		} = new Node();

		public void Write(PooledBinaryWriter writer)
		{
			writer.Write(1);
			this.RootNode.Write(writer);
		}

		public void Read(PooledBinaryReader reader)
		{
			reader.ReadUInt16();
			this.RootNode.Read(reader);
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

		public const ushort VERSION = 1;
	}
}
