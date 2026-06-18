using System;
using System.Collections.Generic;
using MusicUtils.Enums;
using UnityEngine;

namespace DynamicMusic
{
	public class PlayerTracker : AbstractFilter, IFilter<SectionType>
	{
		public EntityPlayerLocal player
		{
			[PublicizedFrom(EAccessModifier.Private)]
			get
			{
				return GameManager.Instance.World.GetPrimaryPlayer();
			}
		}

		public bool isPlayerInTraderArea
		{
			[PublicizedFrom(EAccessModifier.Private)]
			get
			{
				TraderArea traderAreaAt = GameManager.Instance.World.GetTraderAreaAt(this.player.GetBlockPosition());
				return traderAreaAt != null && this.IsTraderAreaOpen(traderAreaAt);
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public bool IsTraderAreaOpen(TraderArea _ta)
		{
			Vector3 vector = _ta.Position.ToVector3() + _ta.PrefabSize.ToVector3() / 2f;
			Bounds bounds;
			bounds..ctor(vector, _ta.PrefabSize.ToVector3());
			GameManager.Instance.World.GetEntitiesInBounds(typeof(EntityTrader), bounds, this.traders);
			if (this.traders.Count <= 0)
			{
				return false;
			}
			EntityTrader entityTrader = this.traders[0] as EntityTrader;
			if (entityTrader.TraderInfo == null)
			{
				return false;
			}
			this.traders.Clear();
			return entityTrader.TraderInfo.IsOpen;
		}

		public bool isPlayerHome
		{
			[PublicizedFrom(EAccessModifier.Private)]
			get
			{
				return !this.player.GetSpawnPoint().IsUndef() && (this.player.GetSpawnPoint().position - this.player.position).magnitude < 50f;
			}
		}

		public override List<SectionType> Filter(List<SectionType> _sectionTypes)
		{
			if (this.player != null && this.player.IsAlive())
			{
				if (this.isPlayerInTraderArea)
				{
					_sectionTypes.Clear();
					_sectionTypes.Add(this.determineTrader());
					return _sectionTypes;
				}
				_sectionTypes.Remove(SectionType.TraderBob);
				_sectionTypes.Remove(SectionType.TraderHugh);
				_sectionTypes.Remove(SectionType.TraderJen);
				_sectionTypes.Remove(SectionType.TraderJoel);
				_sectionTypes.Remove(SectionType.TraderRekt);
				switch (this.player.ThreatLevel.Category)
				{
				case ThreatLevelType.Safe:
					_sectionTypes.Remove(SectionType.Suspense);
					_sectionTypes.Remove(SectionType.Combat);
					_sectionTypes.Remove(SectionType.Bloodmoon);
					if (this.isPlayerHome)
					{
						_sectionTypes.Remove(SectionType.Exploration);
					}
					else
					{
						_sectionTypes.Remove(SectionType.HomeDay);
						_sectionTypes.Remove(SectionType.HomeNight);
						if (this.isPlayerInPOI())
						{
							_sectionTypes.Remove(SectionType.Exploration);
						}
					}
					break;
				case ThreatLevelType.Spooked:
					_sectionTypes.Remove(SectionType.HomeDay);
					_sectionTypes.Remove(SectionType.HomeNight);
					_sectionTypes.Remove(SectionType.Exploration);
					_sectionTypes.Remove(SectionType.Combat);
					_sectionTypes.Remove(SectionType.Bloodmoon);
					break;
				case ThreatLevelType.Panicked:
					_sectionTypes.Clear();
					_sectionTypes.Add(SectionType.Combat);
					_sectionTypes.Add(SectionType.Bloodmoon);
					break;
				}
			}
			else
			{
				_sectionTypes.Clear();
				_sectionTypes.Add(SectionType.None);
			}
			return _sectionTypes;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public bool isPlayerInPOI()
		{
			return (this.player.prefab != null || GamePrefs.GetString(EnumGamePrefs.GameWorld).Equals("Playtesting")) && this.player.PlayerStats.LightInsidePer > 0.2f;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public SectionType determineTrader()
		{
			PlayerTracker.npcs.Clear();
			GameManager.Instance.World.GetEntitiesInBounds(typeof(EntityTrader), new Bounds(this.player.position, PlayerTracker.boundingBoxRange), PlayerTracker.npcs);
			if (PlayerTracker.npcs.Count > 0)
			{
				EntityTrader entityTrader = PlayerTracker.npcs[0] as EntityTrader;
				if (entityTrader != null)
				{
					NPCInfo npcinfo = entityTrader.NPCInfo;
					if (npcinfo == null)
					{
						return SectionType.None;
					}
					return npcinfo.DmsSectionType;
				}
			}
			return SectionType.None;
		}

		public override string ToString()
		{
			return "PlayerTracker:\n" + string.Format("Is Player in a trader station: {0}\n", this.isPlayerInTraderArea) + string.Format("Is Player home: {0}\n", this.isPlayerHome) + string.Format("Player Threat Level: {0}", this.player.ThreatLevel.Category);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public const float cMaxHomeDistance = 50f;

		[PublicizedFrom(EAccessModifier.Private)]
		public List<Entity> traders = new List<Entity>();

		[PublicizedFrom(EAccessModifier.Private)]
		public static Vector3 boundingBoxRange = new Vector3(200f, 200f, 200f);

		[PublicizedFrom(EAccessModifier.Private)]
		public static List<Entity> npcs = new List<Entity>();
	}
}
