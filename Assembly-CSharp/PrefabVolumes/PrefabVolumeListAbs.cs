using System;
using System.Runtime.CompilerServices;

namespace PrefabVolumes
{
	public abstract class PrefabVolumeListAbs
	{
		public abstract PrefabVolumeAbs.EVolumeType VolumeType { get; }

		public abstract bool AnyUsedEntry { get; }

		public abstract int Count { get; }

		public abstract PrefabVolumeAbs Get(int _index);

		public abstract void CopyFrom(PrefabVolumeListAbs _other);

		public abstract void ReadFromProperties(DynamicProperties _properties);

		public abstract void WriteToProperties(DynamicProperties _properties);

		public abstract void SendAllVolumesToClient(ClientInfo _clientInfo, int _prefabInstanceId);

		public abstract void Move(Vector3i _moveDistance);

		public abstract void RotateY(bool _bLeft, Vector3i _prefabSize);

		public abstract bool CanCreateVolume(string _prefabInstanceName, Vector3i _bbPos, Vector3i _startPos, Vector3i _size);

		[return: TupleElementNames(new string[] { "volumeIndex", "volume", "box" })]
		public abstract ValueTuple<int, PrefabVolumeAbs, SelectionBox> AddNewVolume(string _prefabInstanceName, Vector3i _bbPos, Vector3i _startPos, Vector3i _size);

		[return: TupleElementNames(new string[] { "volumeIndex", "volume", "box" })]
		public abstract ValueTuple<int, PrefabVolumeAbs, SelectionBox> CloneVolume(string _prefabInstanceName, Vector3i _bbPos, int _existingIndex, Vector3i _offset);

		public abstract void SetVolume(PrefabInstance _prefabInstance, int _index, PrefabVolumeAbs _volumeSettings);

		public abstract void CreateSelectionBoxes(PrefabInstance _prefabInstance);

		public abstract void ApplyVolumesToSelectionBoxes(PrefabInstance _prefabInstance);

		public abstract void RemoveSelectionBoxes(PrefabInstance _prefabInstance);

		public abstract void RemoveVolumes(PrefabInstance _prefabInstance);

		public abstract void CopyVolumesIntoWorld(World _world, Chunk _chunk, Vector3i _offset);

		[PublicizedFrom(EAccessModifier.Protected)]
		public PrefabVolumeListAbs()
		{
		}
	}
}
