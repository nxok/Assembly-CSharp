using System;
using System.Collections.Generic;
using System.IO;

namespace Platform
{
	public class AchievementData : Serializable
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public static EnumDictionary<EnumAchievementManagerAchievement, EnumAchievementDataStat> CreateAchievementToStat()
		{
			EnumDictionary<EnumAchievementManagerAchievement, EnumAchievementDataStat> enumDictionary = new EnumDictionary<EnumAchievementManagerAchievement, EnumAchievementDataStat>();
			foreach (AchievementData.AchievementStatDecl achievementStatDecl in AchievementData.propertyList)
			{
				foreach (AchievementData.AchievementInfo achievementInfo in achievementStatDecl.achievementInfos)
				{
					enumDictionary.Add(achievementInfo.achievement, achievementStatDecl.name);
				}
			}
			return enumDictionary;
		}

		public static EnumStatType GetStatType(EnumAchievementDataStat _stat)
		{
			if (_stat != EnumAchievementDataStat.Last)
			{
				return AchievementData.propertyList[(int)_stat].type;
			}
			return EnumStatType.Invalid;
		}

		public static AchievementData.EnumUpdateType GetUpdateType(EnumAchievementDataStat _stat)
		{
			if (_stat != EnumAchievementDataStat.Last)
			{
				return AchievementData.propertyList[(int)_stat].updateType;
			}
			return AchievementData.EnumUpdateType.Replace;
		}

		public static List<AchievementData.AchievementInfo> GetAchievementInfos(EnumAchievementDataStat _stat)
		{
			return AchievementData.propertyList[(int)_stat].achievementInfos;
		}

		public static EnumAchievementDataStat GetStat(EnumAchievementManagerAchievement _achievement)
		{
			return AchievementData.achievementToStat[_achievement];
		}

		public bool IsDirty { get; set; }

		public AchievementData()
		{
			this.statValues = new object[AchievementData.propertyList.Length];
			this.achievementStatuses = new EnumDictionary<EnumAchievementManagerAchievement, bool>();
			AchievementData.AchievementStatDecl[] array = AchievementData.propertyList;
			for (int i = 0; i < array.Length; i++)
			{
				int name = (int)array[i].name;
				this.statValues[name] = 0;
			}
			for (int j = 0; j < 48; j++)
			{
				this.achievementStatuses[(EnumAchievementManagerAchievement)j] = false;
			}
		}

		public void UpdateAchievement(EnumAchievementDataStat _stat)
		{
			List<AchievementData.AchievementInfo> achievementInfos = AchievementData.GetAchievementInfos(_stat);
			object achievementStatValue = this.GetAchievementStatValue(_stat);
			EnumStatType statType = AchievementData.GetStatType(_stat);
			for (int i = 0; i < achievementInfos.Count; i++)
			{
				EnumAchievementManagerAchievement achievement = achievementInfos[i].achievement;
				if (statType == EnumStatType.Int)
				{
					if ((int)achievementStatValue >= Convert.ToInt32(achievementInfos[i].triggerPoint) && !this.IsAchievementLocked(achievement))
					{
						this.LockAchievement(achievement);
					}
				}
				else if (Convert.ToSingle(achievementStatValue) >= Convert.ToSingle(achievementInfos[i].triggerPoint) && !this.IsAchievementLocked(achievement))
				{
					this.LockAchievement(achievement);
				}
			}
		}

		public void SetStatCompleteCallback(Action<EnumAchievementManagerAchievement> _statCompleteCallback)
		{
			this.statCompleteCallback = _statCompleteCallback;
		}

		public int GetIntStatValue(EnumAchievementDataStat _stat)
		{
			if (AchievementData.propertyList[(int)_stat].type == EnumStatType.Int)
			{
				return Convert.ToInt32(this.statValues[(int)_stat]);
			}
			return -1;
		}

		public float GetFloatStatValue(EnumAchievementDataStat _stat)
		{
			if (AchievementData.propertyList[(int)_stat].type == EnumStatType.Float)
			{
				return Convert.ToSingle(this.statValues[(int)_stat]);
			}
			return -1f;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void SetStatValue(EnumAchievementDataStat _stat, object _value)
		{
			AchievementData.EnumUpdateType updateType = AchievementData.propertyList[(int)_stat].updateType;
			EnumStatType type = AchievementData.propertyList[(int)_stat].type;
			object[] array = this.statValues;
			lock (array)
			{
				switch (updateType)
				{
				case AchievementData.EnumUpdateType.Sum:
					if (type == EnumStatType.Int)
					{
						this.statValues[(int)_stat] = Convert.ToInt32(this.statValues[(int)_stat]) + Convert.ToInt32(_value);
					}
					else
					{
						this.statValues[(int)_stat] = Convert.ToSingle(this.statValues[(int)_stat]) + Convert.ToSingle(_value);
					}
					break;
				case AchievementData.EnumUpdateType.Replace:
					this.statValues[(int)_stat] = _value;
					break;
				case AchievementData.EnumUpdateType.Max:
					if (type == EnumStatType.Int)
					{
						this.statValues[(int)_stat] = ((Convert.ToInt32(_value) > Convert.ToInt32(this.statValues[(int)_stat])) ? _value : this.statValues[(int)_stat]);
					}
					else
					{
						this.statValues[(int)_stat] = ((Convert.ToSingle(_value) > Convert.ToSingle(this.statValues[(int)_stat])) ? _value : this.statValues[(int)_stat]);
					}
					break;
				}
			}
			this.IsDirty = true;
			this.UpdateAchievement(_stat);
		}

		public virtual void SetAchievementStat(EnumAchievementDataStat _stat, int _value)
		{
			this.SetStatValue(_stat, _value);
		}

		public virtual void SetAchievementStat(EnumAchievementDataStat _stat, float _value)
		{
			this.SetStatValue(_stat, _value);
		}

		public object GetAchievementStatValue(EnumAchievementDataStat _stat)
		{
			if (_stat == EnumAchievementDataStat.Last)
			{
				return 0;
			}
			return this.statValues[(int)_stat];
		}

		public bool IsAchievementLocked(EnumAchievementManagerAchievement _achievement)
		{
			return this.achievementStatuses[_achievement];
		}

		public void LockAchievement(EnumAchievementManagerAchievement _achievement)
		{
			Dictionary<EnumAchievementManagerAchievement, bool> dictionary = this.achievementStatuses;
			lock (dictionary)
			{
				this.achievementStatuses[_achievement] = true;
			}
			Action<EnumAchievementManagerAchievement> action = this.statCompleteCallback;
			if (action == null)
			{
				return;
			}
			action(_achievement);
		}

		public float GetGameProgress()
		{
			float num = 0f;
			foreach (AchievementData.AchievementStatDecl achievementStatDecl in AchievementData.propertyList)
			{
				int count = achievementStatDecl.achievementInfos.Count;
				for (int j = 0; j < count; j++)
				{
					AchievementData.AchievementInfo achievementInfo = achievementStatDecl.achievementInfos[j];
					if (this.IsAchievementLocked(achievementInfo.achievement))
					{
						num += achievementInfo.progressContribution;
					}
				}
			}
			return num;
		}

		public void DebugPrintStats()
		{
			for (int i = 0; i < 19; i++)
			{
				string[] array = new string[6];
				array[0] = "Stat: ";
				array[1] = i.ToString();
				array[2] = ", ";
				int num = 3;
				EnumAchievementDataStat enumAchievementDataStat = (EnumAchievementDataStat)i;
				array[num] = enumAchievementDataStat.ToString();
				array[4] = " = ";
				int num2 = 5;
				object obj = this.statValues[i];
				array[num2] = ((obj != null) ? obj.ToString() : null);
				string text = string.Concat(array);
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output(text);
			}
		}

		public byte[] Serialize()
		{
			byte[] array = null;
			using (MemoryStream memoryStream = new MemoryStream())
			{
				try
				{
					BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
					binaryWriter.Write('t');
					binaryWriter.Write('t');
					binaryWriter.Write('w');
					binaryWriter.Write(0);
					binaryWriter.Write(1U);
					binaryWriter.Write(Constants.cVersionInformation.LongString);
					for (int i = 0; i < 19; i++)
					{
						BinaryWriter binaryWriter2 = binaryWriter;
						EnumAchievementDataStat enumAchievementDataStat = (EnumAchievementDataStat)i;
						binaryWriter2.Write(enumAchievementDataStat.ToString());
						binaryWriter.Write(AchievementData.propertyList[i].type.ToString());
						object[] array2 = this.statValues;
						lock (array2)
						{
							if (AchievementData.propertyList[i].type == EnumStatType.Int)
							{
								binaryWriter.Write(Convert.ToInt32(this.statValues[i]));
							}
							else
							{
								binaryWriter.Write(Convert.ToSingle(this.statValues[i]));
							}
						}
					}
					foreach (KeyValuePair<EnumAchievementManagerAchievement, bool> keyValuePair in this.achievementStatuses)
					{
						binaryWriter.Write(keyValuePair.Key.ToString());
						Dictionary<EnumAchievementManagerAchievement, bool> dictionary = this.achievementStatuses;
						lock (dictionary)
						{
							binaryWriter.Write(keyValuePair.Value);
						}
					}
					array = memoryStream.ToArray();
				}
				catch (Exception ex)
				{
					Log.Error("Writing header of achievement data: " + ex.Message);
				}
			}
			return array;
		}

		public void DeserializeBytes(byte[] _bytes)
		{
			Stream stream = null;
			try
			{
				stream = new MemoryStream(_bytes, false);
				this.DeserializeFromStream(stream);
				stream.Close();
			}
			catch (Exception ex)
			{
				Log.Error("Reading header of achievements: " + ex.Message);
			}
			if (stream != null)
			{
				stream.Close();
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public bool DeserializeFromStream(Stream _stream)
		{
			BinaryReader binaryReader = new BinaryReader(_stream);
			long num = 0L;
			long length = binaryReader.BaseStream.Length;
			if (binaryReader.ReadChar() != 't' || binaryReader.ReadChar() != 't' || binaryReader.ReadChar() != 'w' || binaryReader.ReadChar() != '\0')
			{
				return false;
			}
			num += 2L;
			this.version = binaryReader.ReadUInt32();
			num += 4L;
			if (this.version != 1U)
			{
				return false;
			}
			string text = binaryReader.ReadString();
			num += (long)(text.Length * 2);
			if (text != Constants.cVersionInformation.LongString)
			{
				Log.Warning("Loaded achievement data from different version: '" + text + "'");
			}
			for (int i = 0; i < 19; i++)
			{
				string text2 = binaryReader.ReadString();
				num += (long)(text2.Length * 2);
				if ((EnumAchievementDataStat)Enum.Parse(typeof(EnumAchievementDataStat), text2) != (EnumAchievementDataStat)i)
				{
					return false;
				}
				string text3 = binaryReader.ReadString();
				num += (long)(text3.Length * 2);
				EnumStatType enumStatType = (EnumStatType)Enum.Parse(typeof(EnumStatType), text3);
				if (AchievementData.propertyList[i].type != enumStatType)
				{
					return false;
				}
				if (enumStatType == EnumStatType.Int)
				{
					this.statValues[i] = binaryReader.ReadInt32();
					num += 4L;
				}
				else
				{
					this.statValues[i] = binaryReader.ReadSingle();
					num += 4L;
				}
			}
			foreach (KeyValuePair<EnumAchievementManagerAchievement, bool> keyValuePair in this.achievementStatuses)
			{
				string text4 = binaryReader.ReadString();
				num += (long)(text4.Length * 2);
				EnumAchievementManagerAchievement enumAchievementManagerAchievement = (EnumAchievementManagerAchievement)Enum.Parse(typeof(EnumAchievementManagerAchievement), text4);
				binaryReader.ReadBoolean();
				num += 1L;
				this.achievementStatuses[enumAchievementManagerAchievement] = false;
			}
			return true;
		}

		public static void Deserialize(byte[] _bytes, Action<AchievementData> _callback)
		{
			AchievementData achievementData = null;
			TaskManager.Schedule(delegate
			{
				achievementData = new AchievementData();
				try
				{
					achievementData.DeserializeBytes(_bytes);
				}
				catch (Exception)
				{
					achievementData = null;
				}
			}, delegate
			{
				Action<AchievementData> callback = _callback;
				if (callback == null)
				{
					return;
				}
				callback(achievementData);
			});
		}

		public const string cDataName = "achievements.bin";

		[PublicizedFrom(EAccessModifier.Private)]
		public const int CurrentSaveVersion = 1;

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly AchievementData.AchievementStatDecl[] propertyList = new AchievementData.AchievementStatDecl[]
		{
			new AchievementData.AchievementStatDecl(EnumAchievementDataStat.StoneAxeCrafted, EnumStatType.Int, AchievementData.EnumUpdateType.Max, new List<AchievementData.AchievementInfo>
			{
				new AchievementData.AchievementInfo(1, EnumAchievementManagerAchievement.StoneAxe, 1.5f)
			}),
			new AchievementData.AchievementStatDecl(EnumAchievementDataStat.BedrollPlaced, EnumStatType.Int, AchievementData.EnumUpdateType.Max, new List<AchievementData.AchievementInfo>
			{
				new AchievementData.AchievementInfo(1, EnumAchievementManagerAchievement.Bedroll, 1f)
			}),
			new AchievementData.AchievementStatDecl(EnumAchievementDataStat.BleedOutStopped, EnumStatType.Int, AchievementData.EnumUpdateType.Max, new List<AchievementData.AchievementInfo>
			{
				new AchievementData.AchievementInfo(1, EnumAchievementManagerAchievement.BleedOut, 1f)
			}),
			new AchievementData.AchievementStatDecl(EnumAchievementDataStat.WoodFrameCrafted, EnumStatType.Int, AchievementData.EnumUpdateType.Max, new List<AchievementData.AchievementInfo>
			{
				new AchievementData.AchievementInfo(1, EnumAchievementManagerAchievement.WoodFrame, 1f)
			}),
			new AchievementData.AchievementStatDecl(EnumAchievementDataStat.LandClaimPlaced, EnumStatType.Int, AchievementData.EnumUpdateType.Max, new List<AchievementData.AchievementInfo>
			{
				new AchievementData.AchievementInfo(1, EnumAchievementManagerAchievement.LandClaim, 1.5f)
			}),
			new AchievementData.AchievementStatDecl(EnumAchievementDataStat.ItemsCrafted, EnumStatType.Int, AchievementData.EnumUpdateType.Sum, new List<AchievementData.AchievementInfo>
			{
				new AchievementData.AchievementInfo(50, EnumAchievementManagerAchievement.Items50, 2f),
				new AchievementData.AchievementInfo(500, EnumAchievementManagerAchievement.Items500, 2f),
				new AchievementData.AchievementInfo(1500, EnumAchievementManagerAchievement.Items1500, 2f),
				new AchievementData.AchievementInfo(5000, EnumAchievementManagerAchievement.Items5000, 5f)
			}),
			new AchievementData.AchievementStatDecl(EnumAchievementDataStat.ZombiesKilled, EnumStatType.Int, AchievementData.EnumUpdateType.Sum, new List<AchievementData.AchievementInfo>
			{
				new AchievementData.AchievementInfo(10, EnumAchievementManagerAchievement.Zombies10, 2f),
				new AchievementData.AchievementInfo(100, EnumAchievementManagerAchievement.Zombies100, 2f),
				new AchievementData.AchievementInfo(500, EnumAchievementManagerAchievement.Zombies500, 2f),
				new AchievementData.AchievementInfo(2500, EnumAchievementManagerAchievement.Zombies2500, 5f)
			}),
			new AchievementData.AchievementStatDecl(EnumAchievementDataStat.PlayersKilled, EnumStatType.Int, AchievementData.EnumUpdateType.Sum, new List<AchievementData.AchievementInfo>
			{
				new AchievementData.AchievementInfo(1, EnumAchievementManagerAchievement.Players1, 1f),
				new AchievementData.AchievementInfo(5, EnumAchievementManagerAchievement.Players5, 2f),
				new AchievementData.AchievementInfo(10, EnumAchievementManagerAchievement.Players10, 2f),
				new AchievementData.AchievementInfo(25, EnumAchievementManagerAchievement.Players25, 5f)
			}),
			new AchievementData.AchievementStatDecl(EnumAchievementDataStat.KMTravelled, EnumStatType.Float, AchievementData.EnumUpdateType.Sum, new List<AchievementData.AchievementInfo>
			{
				new AchievementData.AchievementInfo(10, EnumAchievementManagerAchievement.Travel10, 0.5f),
				new AchievementData.AchievementInfo(50, EnumAchievementManagerAchievement.Travel50, 1f),
				new AchievementData.AchievementInfo(250, EnumAchievementManagerAchievement.Travel250, 2f),
				new AchievementData.AchievementInfo(1000, EnumAchievementManagerAchievement.Travel1000, 5f)
			}),
			new AchievementData.AchievementStatDecl(EnumAchievementDataStat.LongestLifeLived, EnumStatType.Int, AchievementData.EnumUpdateType.Max, new List<AchievementData.AchievementInfo>
			{
				new AchievementData.AchievementInfo(60, EnumAchievementManagerAchievement.Life60Minute, 1f),
				new AchievementData.AchievementInfo(180, EnumAchievementManagerAchievement.Life180Minute, 2f),
				new AchievementData.AchievementInfo(600, EnumAchievementManagerAchievement.Life600Minute, 2.5f),
				new AchievementData.AchievementInfo(1680, EnumAchievementManagerAchievement.Life1680Minute, 7.5f)
			}),
			new AchievementData.AchievementStatDecl(EnumAchievementDataStat.Deaths, EnumStatType.Int, AchievementData.EnumUpdateType.Sum, new List<AchievementData.AchievementInfo>
			{
				new AchievementData.AchievementInfo(1, EnumAchievementManagerAchievement.Die1, 1f),
				new AchievementData.AchievementInfo(7, EnumAchievementManagerAchievement.Die7, 1.5f),
				new AchievementData.AchievementInfo(14, EnumAchievementManagerAchievement.Die14, 2f),
				new AchievementData.AchievementInfo(28, EnumAchievementManagerAchievement.Die28, 2.5f)
			}),
			new AchievementData.AchievementStatDecl(EnumAchievementDataStat.HeightAchieved, EnumStatType.Int, AchievementData.EnumUpdateType.Replace, new List<AchievementData.AchievementInfo>
			{
				new AchievementData.AchievementInfo(1, EnumAchievementManagerAchievement.Height255, 1f)
			}),
			new AchievementData.AchievementStatDecl(EnumAchievementDataStat.DepthAchieved, EnumStatType.Int, AchievementData.EnumUpdateType.Replace, new List<AchievementData.AchievementInfo>
			{
				new AchievementData.AchievementInfo(1, EnumAchievementManagerAchievement.Height0, 1f)
			}),
			new AchievementData.AchievementStatDecl(EnumAchievementDataStat.SubZeroNakedSwim, EnumStatType.Int, AchievementData.EnumUpdateType.Replace, new List<AchievementData.AchievementInfo>
			{
				new AchievementData.AchievementInfo(1, EnumAchievementManagerAchievement.SubZeroNaked, 1f)
			}),
			new AchievementData.AchievementStatDecl(EnumAchievementDataStat.KilledWith44Magnum, EnumStatType.Int, AchievementData.EnumUpdateType.Sum, new List<AchievementData.AchievementInfo>
			{
				new AchievementData.AchievementInfo(44, EnumAchievementManagerAchievement.Kills44Mag, 1f)
			}),
			new AchievementData.AchievementStatDecl(EnumAchievementDataStat.LegBroken, EnumStatType.Int, AchievementData.EnumUpdateType.Max, new List<AchievementData.AchievementInfo>
			{
				new AchievementData.AchievementInfo(1, EnumAchievementManagerAchievement.LegBreak, 1f)
			}),
			new AchievementData.AchievementStatDecl(EnumAchievementDataStat.HighestFortitude, EnumStatType.Int, AchievementData.EnumUpdateType.Max, new List<AchievementData.AchievementInfo>
			{
				new AchievementData.AchievementInfo(4, EnumAchievementManagerAchievement.Fortitude4, 1f),
				new AchievementData.AchievementInfo(6, EnumAchievementManagerAchievement.Fortitude6, 2f),
				new AchievementData.AchievementInfo(8, EnumAchievementManagerAchievement.Fortitude8, 2f),
				new AchievementData.AchievementInfo(10, EnumAchievementManagerAchievement.Fortitude10, 5f)
			}),
			new AchievementData.AchievementStatDecl(EnumAchievementDataStat.HighestGamestage, EnumStatType.Int, AchievementData.EnumUpdateType.Max, new List<AchievementData.AchievementInfo>
			{
				new AchievementData.AchievementInfo(10, EnumAchievementManagerAchievement.Gamestage10, 0.5f),
				new AchievementData.AchievementInfo(25, EnumAchievementManagerAchievement.Gamestage25, 1f),
				new AchievementData.AchievementInfo(50, EnumAchievementManagerAchievement.Gamestage50, 2f),
				new AchievementData.AchievementInfo(100, EnumAchievementManagerAchievement.Gamestage100, 5f),
				new AchievementData.AchievementInfo(200, EnumAchievementManagerAchievement.Gamestage200, 10f)
			}),
			new AchievementData.AchievementStatDecl(EnumAchievementDataStat.HighestPlayerLevel, EnumStatType.Int, AchievementData.EnumUpdateType.Max, new List<AchievementData.AchievementInfo>
			{
				new AchievementData.AchievementInfo(7, EnumAchievementManagerAchievement.PlayerLevel7, 0.5f),
				new AchievementData.AchievementInfo(28, EnumAchievementManagerAchievement.PlayerLevel28, 1f),
				new AchievementData.AchievementInfo(70, EnumAchievementManagerAchievement.PlayerLevel70, 2f),
				new AchievementData.AchievementInfo(140, EnumAchievementManagerAchievement.PlayerLevel140, 5f),
				new AchievementData.AchievementInfo(300, EnumAchievementManagerAchievement.PlayerLevel300, 10f)
			})
		};

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly EnumDictionary<EnumAchievementManagerAchievement, EnumAchievementDataStat> achievementToStat = AchievementData.CreateAchievementToStat();

		[PublicizedFrom(EAccessModifier.Private)]
		public uint version;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly object[] statValues;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly Dictionary<EnumAchievementManagerAchievement, bool> achievementStatuses;

		[PublicizedFrom(EAccessModifier.Private)]
		public Action<EnumAchievementManagerAchievement> statCompleteCallback;

		public enum EnumUpdateType
		{
			Sum,
			Replace,
			Max
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly struct AchievementStatDecl
		{
			public AchievementStatDecl(EnumAchievementDataStat _name, EnumStatType _type, AchievementData.EnumUpdateType _updateType, List<AchievementData.AchievementInfo> _achievementPairs)
			{
				this.name = _name;
				this.type = _type;
				this.updateType = _updateType;
				this.achievementInfos = _achievementPairs;
			}

			public readonly EnumAchievementDataStat name;

			public readonly EnumStatType type;

			public readonly AchievementData.EnumUpdateType updateType;

			public readonly List<AchievementData.AchievementInfo> achievementInfos;
		}

		public readonly struct AchievementInfo
		{
			public AchievementInfo(object _triggerPoint, EnumAchievementManagerAchievement _achievement, float _progressContribution)
			{
				this.triggerPoint = _triggerPoint;
				this.achievement = _achievement;
				this.progressContribution = _progressContribution;
			}

			public readonly object triggerPoint;

			public readonly EnumAchievementManagerAchievement achievement;

			public readonly float progressContribution;
		}
	}
}
