using System;
using System.IO;
using System.Text;

namespace Platform
{
	public class PlatformLobbyId
	{
		public PlatformLobbyId(EPlatformIdentifier _platformId, string _lobbyId)
		{
			this.PlatformIdentifier = _platformId;
			this.LobbyId = _lobbyId;
		}

		public int GetWriteLength(Encoding encoding)
		{
			return 1 + this.LobbyId.GetBinaryWriterLength(encoding);
		}

		public void Write(BinaryWriter _writer)
		{
			_writer.Write((byte)this.PlatformIdentifier);
			if (this.PlatformIdentifier != EPlatformIdentifier.None)
			{
				_writer.Write(this.LobbyId);
			}
		}

		public static PlatformLobbyId Read(BinaryReader _reader)
		{
			byte b = _reader.ReadByte();
			string text = ((b != 0) ? _reader.ReadString() : string.Empty);
			return new PlatformLobbyId((EPlatformIdentifier)b, text);
		}

		public static readonly PlatformLobbyId None = new PlatformLobbyId(EPlatformIdentifier.None, string.Empty);

		public readonly EPlatformIdentifier PlatformIdentifier;

		public readonly string LobbyId;
	}
}
