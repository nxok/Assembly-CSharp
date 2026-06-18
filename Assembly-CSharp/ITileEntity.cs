using System;
using System.Collections.Generic;
using UnityEngine;

public interface ITileEntity : ILockTarget
{
	event XUiEvent_TileEntityDestroyed Destroyed;

	List<ITileEntityChangedListener> listeners { get; }

	void SetUserAccessing(bool _bUserAccessing);

	bool IsUserAccessing();

	void SetModified();

	Chunk GetChunk();

	Vector3i ToWorldPos();

	Vector3 ToWorldCenterPos();

	BlockValue blockValue { get; }

	bool IsRemoving { get; set; }
}
