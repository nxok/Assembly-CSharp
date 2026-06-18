using System;
using System.Collections.Generic;
using System.Text;

namespace PrefabVolumes
{
	public class PrefabWallVolumeList : PrefabVolumeListAbs<PrefabWallVolumeList, PrefabWallVolume>
	{
		public PrefabWallVolumeList(Prefab _owner)
			: base(_owner)
		{
		}

		public override PrefabVolumeAbs.EVolumeType VolumeType
		{
			get
			{
				return PrefabVolumeAbs.EVolumeType.Wall;
			}
		}

		public override SelectionCategory SelectionCategory
		{
			[PublicizedFrom(EAccessModifier.Protected)]
			get
			{
				return SelectionBoxManager.Instance.CategoryWallVolume;
			}
		}

		public override void ReadFromProperties(DynamicProperties _properties)
		{
			this.List.Clear();
			Dictionary<string, string> values = _properties.Values;
			if (!values.ContainsKey("WallVolumeSize") || !values.ContainsKey("WallVolumeStart"))
			{
				return;
			}
			List<Vector3i> list = StringParsers.ParseList<Vector3i>(values["WallVolumeSize"], '#', (string _s, int _start, int _end) => StringParsers.ParseVector3i(_s, _start, _end, false));
			List<Vector3i> list2 = StringParsers.ParseList<Vector3i>(values["WallVolumeStart"], '#', (string _s, int _start, int _end) => StringParsers.ParseVector3i(_s, _start, _end, false));
			for (int i = 0; i < list2.Count; i++)
			{
				Vector3i vector3i = list2[i];
				Vector3i vector3i2 = ((i < list.Count) ? list[i] : Vector3i.one);
				PrefabWallVolume prefabWallVolume = new PrefabWallVolume();
				prefabWallVolume.Use(vector3i, vector3i2);
				this.List.Add(prefabWallVolume);
			}
		}

		public override void WriteToProperties(DynamicProperties _properties)
		{
			StringBuilder stringBuilder = new StringBuilder();
			StringBuilder stringBuilder2 = new StringBuilder();
			foreach (PrefabWallVolume prefabWallVolume in this.List)
			{
				if (prefabWallVolume.Used)
				{
					if (stringBuilder.Length > 0)
					{
						stringBuilder.Append('#');
						stringBuilder2.Append('#');
					}
					stringBuilder.Append(prefabWallVolume.size.ToString());
					stringBuilder2.Append(prefabWallVolume.startPos.ToString());
				}
			}
			if (stringBuilder.Length > 0)
			{
				_properties.Values["WallVolumeSize"] = stringBuilder.ToString();
				_properties.Values["WallVolumeStart"] = stringBuilder2.ToString();
				return;
			}
			_properties.Values.Remove("WallVolumeSize");
			_properties.Values.Remove("WallVolumeStart");
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override int FindWorldVolume(World _world, Vector3i _min, Vector3i _max)
		{
			return _world.FindWallVolume(_min, _max);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override int CreateWorldVolume(int _prefabVolumeIndex, PrefabWallVolume _prefabVolume, Vector3i _offset, Vector3i _volumeMin, Vector3i _volumeMax, World _world, Vector3i _volumeWorldMin, Vector3i _volumeWorldMax)
		{
			WallVolume wallVolume = WallVolume.Create(_prefabVolume, _volumeWorldMin, _volumeWorldMax);
			return _world.AddWallVolume(wallVolume);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override void AddWorldVolume(Chunk _chunk, int _volumeIndex)
		{
			_chunk.AddWallVolumeId(_volumeIndex);
		}

		public override void CopyVolumesIntoWorld(World _world, Chunk _chunk, Vector3i _offset)
		{
			base.CopyVolumesIntoWorldCommon(_world, _chunk, _offset, Vector3i.zero);
		}
	}
}
