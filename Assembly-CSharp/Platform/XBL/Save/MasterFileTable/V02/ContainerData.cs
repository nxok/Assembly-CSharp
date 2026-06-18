using System;

namespace Platform.XBL.Save.MasterFileTable.V02
{
	public sealed class ContainerData : IDisposable, IMigratable
	{
		public ushort Version
		{
			get
			{
				return 2;
			}
		}

		public Node RootNode
		{
			get; [PublicizedFrom(EAccessModifier.Private)]
			set;
		} = new Node();

		public void Write(PooledBinaryWriter writer)
		{
			writer.Write(2);
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

		public const ushort VERSION = 2;
	}
}
