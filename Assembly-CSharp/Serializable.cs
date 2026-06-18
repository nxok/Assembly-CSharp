using System;

public interface Serializable
{
	byte[] Serialize();

	bool IsDirty { get; set; }
}
