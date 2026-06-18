using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;

namespace PrefabVolumes
{
	public class PrefabSleeperVolumeList : PrefabVolumeListAbs<PrefabSleeperVolumeList, PrefabSleeperVolume>
	{
		public PrefabSleeperVolumeList(Prefab _owner)
			: base(_owner)
		{
		}

		public override PrefabVolumeAbs.EVolumeType VolumeType
		{
			get
			{
				return PrefabVolumeAbs.EVolumeType.Sleeper;
			}
		}

		public override SelectionCategory SelectionCategory
		{
			[PublicizedFrom(EAccessModifier.Protected)]
			get
			{
				return SelectionBoxManager.Instance.CategorySleeperVolume;
			}
		}

		public override void ReadFromProperties(DynamicProperties _properties)
		{
			this.List.Clear();
			Dictionary<string, string> values = _properties.Values;
			if (!values.ContainsKey("SleeperVolumeSize") || !values.ContainsKey("SleeperVolumeStart"))
			{
				return;
			}
			List<Vector3i> list = StringParsers.ParseList<Vector3i>(values["SleeperVolumeSize"], '#', (string _s, int _start, int _end) => StringParsers.ParseVector3i(_s, _start, _end, false));
			List<Vector3i> list2 = StringParsers.ParseList<Vector3i>(values["SleeperVolumeStart"], '#', (string _s, int _start, int _end) => StringParsers.ParseVector3i(_s, _start, _end, false));
			List<string> list3 = null;
			string text;
			if (values.TryGetValue("SleeperVolumeGroupId", out text))
			{
				list3 = new List<string>(text.Split(',', StringSplitOptions.None));
			}
			List<string> list4 = null;
			if (values.TryGetValue("SleeperVolumeGroup", out text))
			{
				list4 = new List<string>(text.Split(',', StringSplitOptions.None));
			}
			List<bool> list5;
			if (!values.ContainsKey("SleeperIsLootVolume"))
			{
				list5 = new List<bool>();
			}
			else
			{
				list5 = StringParsers.ParseList<bool>(values["SleeperIsLootVolume"], ',', (string _s, int _start, int _end) => StringParsers.ParseBool(_s, _start, _end, true));
			}
			List<bool> list6 = list5;
			List<bool> list7;
			if (!values.ContainsKey("SleeperIsQuestExclude"))
			{
				list7 = new List<bool>();
			}
			else
			{
				list7 = StringParsers.ParseList<bool>(values["SleeperIsQuestExclude"], ',', (string _s, int _start, int _end) => StringParsers.ParseBool(_s, _start, _end, true));
			}
			List<bool> list8 = list7;
			List<int> list9 = null;
			if (values.TryGetValue("SleeperVolumeFlags", out text))
			{
				list9 = StringParsers.ParseList<int>(text, ',', (string _s, int _start, int _end) => StringParsers.ParseSInt32(_s, _start, _end, NumberStyles.HexNumber));
			}
			List<string> list10 = null;
			if (values.TryGetValue("SleeperVolumeTriggeredBy", out text))
			{
				list10 = StringParsers.ParseList<string>(text, '#', (string _s, int _start, int _end) => _s.Substring(_start, (_end == -1) ? (_s.Length - _start) : (_end + 1 - _start)));
			}
			for (int i = 0; i < list2.Count; i++)
			{
				Vector3i vector3i = list2[i];
				Vector3i vector3i2 = ((i < list.Count) ? list[i] : Vector3i.one);
				short num = 0;
				string text2 = "???";
				short num2 = 5;
				short num3 = 5;
				if (list3 != null)
				{
					num = StringParsers.ParseSInt16(list3[i], 0, -1, NumberStyles.Integer);
				}
				if (list4 != null)
				{
					if (list4.Count == list2.Count)
					{
						text2 = list4[i];
					}
					else if (list4.Count == list2.Count * 3)
					{
						int num4 = i * 3;
						text2 = list4[num4];
						num2 = StringParsers.ParseSInt16(list4[num4 + 1], 0, -1, NumberStyles.Integer);
						num3 = StringParsers.ParseSInt16(list4[num4 + 2], 0, -1, NumberStyles.Integer);
					}
					text2 = GameStageGroup.CleanName(text2);
				}
				bool flag = i < list6.Count && list6[i];
				bool flag2 = i < list8.Count && list8[i];
				int num5 = 0;
				if (list9 != null && i < list9.Count)
				{
					num5 = list9[i];
				}
				PrefabSleeperVolume prefabSleeperVolume = new PrefabSleeperVolume();
				prefabSleeperVolume.Use(vector3i, vector3i2);
				prefabSleeperVolume.groupId = num;
				prefabSleeperVolume.groupName = text2;
				prefabSleeperVolume.isPriority = flag;
				prefabSleeperVolume.isQuestExclude = flag2;
				prefabSleeperVolume.spawnCountMin = num2;
				prefabSleeperVolume.spawnCountMax = num3;
				prefabSleeperVolume.flags = num5;
				string @string = _properties.GetString("SVS" + i.ToString());
				if (@string.Length > 0)
				{
					prefabSleeperVolume.minScript = @string;
				}
				if (list10 != null && list10[i].Trim() != "")
				{
					prefabSleeperVolume.triggeredByIndices.AddRange(StringParsers.ParseList<byte>(list10[i], ',', (string _s, int _start, int _end) => StringParsers.ParseUInt8(_s, _start, _end, NumberStyles.Integer)));
				}
				this.List.Add(prefabSleeperVolume);
			}
		}

		public override void WriteToProperties(DynamicProperties _properties)
		{
			List<string> list = new List<string>();
			foreach (KeyValuePair<string, string> keyValuePair in _properties.Values)
			{
				if (keyValuePair.Key.StartsWith("SVS"))
				{
					list.Add(keyValuePair.Key);
				}
			}
			foreach (string text in list)
			{
				_properties.Values.Remove(text);
			}
			StringBuilder stringBuilder = new StringBuilder();
			StringBuilder stringBuilder2 = new StringBuilder();
			StringBuilder stringBuilder3 = new StringBuilder();
			StringBuilder stringBuilder4 = new StringBuilder();
			StringBuilder stringBuilder5 = new StringBuilder();
			StringBuilder stringBuilder6 = new StringBuilder();
			StringBuilder stringBuilder7 = new StringBuilder();
			StringBuilder stringBuilder8 = new StringBuilder();
			foreach (PrefabSleeperVolume prefabSleeperVolume in this.List)
			{
				if (prefabSleeperVolume.Used)
				{
					if (stringBuilder.Length > 0)
					{
						stringBuilder.Append('#');
						stringBuilder2.Append('#');
						stringBuilder3.Append(',');
						stringBuilder4.Append(',');
						stringBuilder5.Append(',');
						stringBuilder6.Append(',');
						stringBuilder7.Append(',');
						stringBuilder8.Append('#');
					}
					stringBuilder.Append(prefabSleeperVolume.size.ToString());
					stringBuilder2.Append(prefabSleeperVolume.startPos.ToString());
					stringBuilder3.Append(prefabSleeperVolume.groupId);
					stringBuilder4.Append(prefabSleeperVolume.groupName);
					stringBuilder4.Append(',');
					stringBuilder4.Append(prefabSleeperVolume.spawnCountMin.ToString());
					stringBuilder4.Append(',');
					stringBuilder4.Append(prefabSleeperVolume.spawnCountMax.ToString());
					stringBuilder5.Append(prefabSleeperVolume.isPriority.ToString());
					stringBuilder6.Append(prefabSleeperVolume.isQuestExclude.ToString());
					stringBuilder7.Append(prefabSleeperVolume.flags.ToString("x"));
					for (int i = 0; i < prefabSleeperVolume.triggeredByIndices.Count; i++)
					{
						if (i > 0)
						{
							stringBuilder8.Append(',');
						}
						stringBuilder8.Append(prefabSleeperVolume.triggeredByIndices[i].ToString());
					}
					if (prefabSleeperVolume.triggeredByIndices.Count == 0)
					{
						stringBuilder8.Append(" ");
					}
				}
			}
			if (stringBuilder.Length > 0)
			{
				_properties.Values["SleeperVolumeSize"] = stringBuilder.ToString();
				_properties.Values["SleeperVolumeStart"] = stringBuilder2.ToString();
				_properties.Values["SleeperVolumeGroupId"] = stringBuilder3.ToString();
				_properties.Values["SleeperVolumeGroup"] = stringBuilder4.ToString();
				_properties.Values["SleeperIsLootVolume"] = stringBuilder5.ToString();
				_properties.Values["SleeperIsQuestExclude"] = stringBuilder6.ToString();
				_properties.Values["SleeperVolumeFlags"] = stringBuilder7.ToString();
				_properties.Values["SleeperVolumeTriggeredBy"] = stringBuilder8.ToString();
				int num = 0;
				using (List<PrefabSleeperVolume>.Enumerator enumerator3 = this.List.GetEnumerator())
				{
					while (enumerator3.MoveNext())
					{
						PrefabSleeperVolume prefabSleeperVolume2 = enumerator3.Current;
						if (prefabSleeperVolume2.Used)
						{
							if (prefabSleeperVolume2.minScript != null)
							{
								_properties.Values["SVS" + num.ToString()] = prefabSleeperVolume2.minScript;
							}
							num++;
						}
					}
					return;
				}
			}
			_properties.Values.Remove("SleeperVolumeSize");
			_properties.Values.Remove("SleeperVolumeStart");
			_properties.Values.Remove("SleeperVolumeGroupId");
			_properties.Values.Remove("SleeperVolumeGroup");
			_properties.Values.Remove("SleeperIsLootVolume");
			_properties.Values.Remove("SleeperIsQuestExclude");
			_properties.Values.Remove("SleeperVolumeFlags");
			_properties.Values.Remove("SleeperVolumeTriggeredBy");
		}

		[return: TupleElementNames(new string[] { "volumeIndex", "volume", "box" })]
		public override ValueTuple<int, PrefabVolumeAbs, SelectionBox> AddNewVolume(string _prefabInstanceName, Vector3i _bbPos, Vector3i _startPos, Vector3i _size)
		{
			ValueTuple<PrefabSleeperVolume, int, string> valueTuple = base.PrepareNewEntry(_prefabInstanceName, _startPos, _size);
			PrefabSleeperVolume item = valueTuple.Item1;
			int item2 = valueTuple.Item2;
			string item3 = valueTuple.Item3;
			SelectionBox selectionBox = this.AddSelectionBox(item, item3, _bbPos + _startPos);
			SelectionBoxManager.Instance.SetActive(selectionBox, true);
			return new ValueTuple<int, PrefabVolumeAbs, SelectionBox>(item2, item, selectionBox);
		}

		public override void SetVolume(PrefabInstance _prefabInstance, int _index, PrefabSleeperVolume _volumeSettings)
		{
			base.SetVolume(_prefabInstance, _index, _volumeSettings);
			XUiC_WoPropsSleeperVolume instance = XUiC_WoPropsSleeperVolume.Instance;
			if (instance == null)
			{
				return;
			}
			instance.SleeperVolumeChanged(_prefabInstance.id, _index);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override int FindWorldVolume(World _world, Vector3i _min, Vector3i _max)
		{
			return _world.FindSleeperVolume(_min, _max);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override int CreateWorldVolume(int _prefabVolumeIndex, PrefabSleeperVolume _prefabVolume, Vector3i _offset, Vector3i _volumeMin, Vector3i _volumeMax, World _world, Vector3i _volumeWorldMin, Vector3i _volumeWorldMax)
		{
			SleeperVolume sleeperVolume = SleeperVolume.Create(_prefabVolume, _volumeWorldMin, _volumeWorldMax);
			int num = _world.AddSleeperVolume(sleeperVolume);
			this.Owner.CopySleeperBlocksContainedInVolume(_prefabVolumeIndex, _offset, sleeperVolume, _volumeMin, _volumeMax);
			return num;
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override void AddWorldVolume(Chunk _chunk, int _volumeIndex)
		{
			_chunk.AddSleeperVolumeId(_volumeIndex);
		}

		public override void CopyVolumesIntoWorld(World _world, Chunk _chunk, Vector3i _offset)
		{
			base.CopyVolumesIntoWorldCommon(_world, _chunk, _offset, SleeperVolume.chunkPadding);
		}
	}
}
