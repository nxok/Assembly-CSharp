using System;
using System.Collections.Generic;
using System.Xml;
using ICSharpCode.WpfDesign.XamlDom;
using UnityEngine.Scripting;
using XMLData.Exceptions;
using XMLData.Parsers;

namespace XMLData.Item
{
	[Preserve]
	public class ItemActionData : IXMLData
	{
		public DataItem<float> Delay
		{
			get
			{
				return this.pDelay;
			}
			set
			{
				this.pDelay = value;
			}
		}

		public DataItem<float> Range
		{
			get
			{
				return this.pRange;
			}
			set
			{
				this.pRange = value;
			}
		}

		public DataItem<string> SoundStart
		{
			get
			{
				return this.pSoundStart;
			}
			set
			{
				this.pSoundStart = value;
			}
		}

		public DataItem<string> SoundRepeat
		{
			get
			{
				return this.pSoundRepeat;
			}
			set
			{
				this.pSoundRepeat = value;
			}
		}

		public DataItem<string> SoundEnd
		{
			get
			{
				return this.pSoundEnd;
			}
			set
			{
				this.pSoundEnd = value;
			}
		}

		public DataItem<string> SoundEmpty
		{
			get
			{
				return this.pSoundEmpty;
			}
			set
			{
				this.pSoundEmpty = value;
			}
		}

		public DataItem<string> SoundReload
		{
			get
			{
				return this.pSoundReload;
			}
			set
			{
				this.pSoundReload = value;
			}
		}

		public DataItem<string> SoundWarning
		{
			get
			{
				return this.pSoundWarning;
			}
			set
			{
				this.pSoundWarning = value;
			}
		}

		public DataItem<string> StaminaUsage
		{
			get
			{
				return this.pStaminaUsage;
			}
			set
			{
				this.pStaminaUsage = value;
			}
		}

		public DataItem<string> UseTime
		{
			get
			{
				return this.pUseTime;
			}
			set
			{
				this.pUseTime = value;
			}
		}

		public DataItem<string> FocusedBlockname1
		{
			get
			{
				return this.pFocusedBlockname1;
			}
			set
			{
				this.pFocusedBlockname1 = value;
			}
		}

		public DataItem<string> FocusedBlockname2
		{
			get
			{
				return this.pFocusedBlockname2;
			}
			set
			{
				this.pFocusedBlockname2 = value;
			}
		}

		public DataItem<string> FocusedBlockname3
		{
			get
			{
				return this.pFocusedBlockname3;
			}
			set
			{
				this.pFocusedBlockname3 = value;
			}
		}

		public DataItem<string> FocusedBlockname4
		{
			get
			{
				return this.pFocusedBlockname4;
			}
			set
			{
				this.pFocusedBlockname4 = value;
			}
		}

		public DataItem<string> FocusedBlockname5
		{
			get
			{
				return this.pFocusedBlockname5;
			}
			set
			{
				this.pFocusedBlockname5 = value;
			}
		}

		public DataItem<string> FocusedBlockname6
		{
			get
			{
				return this.pFocusedBlockname6;
			}
			set
			{
				this.pFocusedBlockname6 = value;
			}
		}

		public DataItem<string> FocusedBlockname7
		{
			get
			{
				return this.pFocusedBlockname7;
			}
			set
			{
				this.pFocusedBlockname7 = value;
			}
		}

		public DataItem<string> FocusedBlockname8
		{
			get
			{
				return this.pFocusedBlockname8;
			}
			set
			{
				this.pFocusedBlockname8 = value;
			}
		}

		public DataItem<string> FocusedBlockname9
		{
			get
			{
				return this.pFocusedBlockname9;
			}
			set
			{
				this.pFocusedBlockname9 = value;
			}
		}

		public DataItem<string> ChangeItemTo
		{
			get
			{
				return this.pChangeItemTo;
			}
			set
			{
				this.pChangeItemTo = value;
			}
		}

		public DataItem<string> ChangeBlockTo
		{
			get
			{
				return this.pChangeBlockTo;
			}
			set
			{
				this.pChangeBlockTo = value;
			}
		}

		public DataItem<string> DoBlockAction
		{
			get
			{
				return this.pDoBlockAction;
			}
			set
			{
				this.pDoBlockAction = value;
			}
		}

		public DataItem<float> GainHealth
		{
			get
			{
				return this.pGainHealth;
			}
			set
			{
				this.pGainHealth = value;
			}
		}

		public DataItem<float> GainFood
		{
			get
			{
				return this.pGainFood;
			}
			set
			{
				this.pGainFood = value;
			}
		}

		public DataItem<float> GainWater
		{
			get
			{
				return this.pGainWater;
			}
			set
			{
				this.pGainWater = value;
			}
		}

		public DataItem<float> GainStamina
		{
			get
			{
				return this.pGainStamina;
			}
			set
			{
				this.pGainStamina = value;
			}
		}

		public DataItem<float> GainSickness
		{
			get
			{
				return this.pGainSickness;
			}
			set
			{
				this.pGainSickness = value;
			}
		}

		public DataItem<float> GainWellness
		{
			get
			{
				return this.pGainWellness;
			}
			set
			{
				this.pGainWellness = value;
			}
		}

		public DataItem<string> Buff
		{
			get
			{
				return this.pBuff;
			}
			set
			{
				this.pBuff = value;
			}
		}

		public DataItem<string> BuffChance
		{
			get
			{
				return this.pBuffChance;
			}
			set
			{
				this.pBuffChance = value;
			}
		}

		public DataItem<string> Debuff
		{
			get
			{
				return this.pDebuff;
			}
			set
			{
				this.pDebuff = value;
			}
		}

		public DataItem<string> CreateItem
		{
			get
			{
				return this.pCreateItem;
			}
			set
			{
				this.pCreateItem = value;
			}
		}

		public DataItem<int> ConditionRaycastBlock
		{
			get
			{
				return this.pConditionRaycastBlock;
			}
			set
			{
				this.pConditionRaycastBlock = value;
			}
		}

		public DataItem<int> GainGas
		{
			get
			{
				return this.pGainGas;
			}
			set
			{
				this.pGainGas = value;
			}
		}

		public DataItem<bool> Consume
		{
			get
			{
				return this.pConsume;
			}
			set
			{
				this.pConsume = value;
			}
		}

		public DataItem<string> Blockname
		{
			get
			{
				return this.pBlockname;
			}
			set
			{
				this.pBlockname = value;
			}
		}

		public DataItem<float> ThrowStrengthDefault
		{
			get
			{
				return this.pThrowStrengthDefault;
			}
			set
			{
				this.pThrowStrengthDefault = value;
			}
		}

		public DataItem<float> ThrowStrengthMax
		{
			get
			{
				return this.pThrowStrengthMax;
			}
			set
			{
				this.pThrowStrengthMax = value;
			}
		}

		public DataItem<float> MaxStrainTime
		{
			get
			{
				return this.pMaxStrainTime;
			}
			set
			{
				this.pMaxStrainTime = value;
			}
		}

		public DataItem<int> MagazineSize
		{
			get
			{
				return this.pMagazineSize;
			}
			set
			{
				this.pMagazineSize = value;
			}
		}

		public DataItem<string> MagazineItem
		{
			get
			{
				return this.pMagazineItem;
			}
			set
			{
				this.pMagazineItem = value;
			}
		}

		public DataItem<float> ReloadTime
		{
			get
			{
				return this.pReloadTime;
			}
			set
			{
				this.pReloadTime = value;
			}
		}

		public DataItem<string> BulletIcon
		{
			get
			{
				return this.pBulletIcon;
			}
			set
			{
				this.pBulletIcon = value;
			}
		}

		public DataItem<int> RaysPerShot
		{
			get
			{
				return this.pRaysPerShot;
			}
			set
			{
				this.pRaysPerShot = value;
			}
		}

		public DataItem<float> RaysSpread
		{
			get
			{
				return this.pRaysSpread;
			}
			set
			{
				this.pRaysSpread = value;
			}
		}

		public DataItem<float> Sphere
		{
			get
			{
				return this.pSphere;
			}
			set
			{
				this.pSphere = value;
			}
		}

		public DataItem<int> CrosshairMinDistance
		{
			get
			{
				return this.pCrosshairMinDistance;
			}
			set
			{
				this.pCrosshairMinDistance = value;
			}
		}

		public DataItem<int> CrosshairMaxDistance
		{
			get
			{
				return this.pCrosshairMaxDistance;
			}
			set
			{
				this.pCrosshairMaxDistance = value;
			}
		}

		public DataItem<int> DamageEntity
		{
			get
			{
				return this.pDamageEntity;
			}
			set
			{
				this.pDamageEntity = value;
			}
		}

		public DataItem<float> DamageBlock
		{
			get
			{
				return this.pDamageBlock;
			}
			set
			{
				this.pDamageBlock = value;
			}
		}

		public DataItem<string> ParticlesMuzzleFire
		{
			get
			{
				return this.pParticlesMuzzleFire;
			}
			set
			{
				this.pParticlesMuzzleFire = value;
			}
		}

		public DataItem<string> ParticlesMuzzleSmoke
		{
			get
			{
				return this.pParticlesMuzzleSmoke;
			}
			set
			{
				this.pParticlesMuzzleSmoke = value;
			}
		}

		public DataItem<float> BlockRange
		{
			get
			{
				return this.pBlockRange;
			}
			set
			{
				this.pBlockRange = value;
			}
		}

		public DataItem<bool> AutoFire
		{
			get
			{
				return this.pAutoFire;
			}
			set
			{
				this.pAutoFire = value;
			}
		}

		public DataItem<float> HordeMeterRate
		{
			get
			{
				return this.pHordeMeterRate;
			}
			set
			{
				this.pHordeMeterRate = value;
			}
		}

		public DataItem<float> HordeMeterDistance
		{
			get
			{
				return this.pHordeMeterDistance;
			}
			set
			{
				this.pHordeMeterDistance = value;
			}
		}

		public DataItem<string> HitmaskOverride
		{
			get
			{
				return this.pHitmaskOverride;
			}
			set
			{
				this.pHitmaskOverride = value;
			}
		}

		public DataItem<bool> SingleMagazineUsage
		{
			get
			{
				return this.pSingleMagazineUsage;
			}
			set
			{
				this.pSingleMagazineUsage = value;
			}
		}

		public DataItem<string> BulletMaterial
		{
			get
			{
				return this.pBulletMaterial;
			}
			set
			{
				this.pBulletMaterial = value;
			}
		}

		public DataItem<bool> InfiniteAmmo
		{
			get
			{
				return this.pInfiniteAmmo;
			}
			set
			{
				this.pInfiniteAmmo = value;
			}
		}

		public DataItem<float> ZoomMaxOut
		{
			get
			{
				return this.pZoomMaxOut;
			}
			set
			{
				this.pZoomMaxOut = value;
			}
		}

		public DataItem<float> ZoomMaxIn
		{
			get
			{
				return this.pZoomMaxIn;
			}
			set
			{
				this.pZoomMaxIn = value;
			}
		}

		public DataItem<string> ZoomOverlay
		{
			get
			{
				return this.pZoomOverlay;
			}
			set
			{
				this.pZoomOverlay = value;
			}
		}

		public DataItem<int> Velocity
		{
			get
			{
				return this.pVelocity;
			}
			set
			{
				this.pVelocity = value;
			}
		}

		public DataItem<float> FlyTime
		{
			get
			{
				return this.pFlyTime;
			}
			set
			{
				this.pFlyTime = value;
			}
		}

		public DataItem<float> LifeTime
		{
			get
			{
				return this.pLifeTime;
			}
			set
			{
				this.pLifeTime = value;
			}
		}

		public DataItem<float> CollisionRadius
		{
			get
			{
				return this.pCollisionRadius;
			}
			set
			{
				this.pCollisionRadius = value;
			}
		}

		public DataItem<int> ProjectileInitialVelocity
		{
			get
			{
				return this.pProjectileInitialVelocity;
			}
			set
			{
				this.pProjectileInitialVelocity = value;
			}
		}

		public DataItem<string> Fertileblock
		{
			get
			{
				return this.pFertileblock;
			}
			set
			{
				this.pFertileblock = value;
			}
		}

		public DataItem<string> Adjacentblock
		{
			get
			{
				return this.pAdjacentblock;
			}
			set
			{
				this.pAdjacentblock = value;
			}
		}

		public DataItem<int> RepairAmount
		{
			get
			{
				return this.pRepairAmount;
			}
			set
			{
				this.pRepairAmount = value;
			}
		}

		public DataItem<int> UpgradeHitOffset
		{
			get
			{
				return this.pUpgradeHitOffset;
			}
			set
			{
				this.pUpgradeHitOffset = value;
			}
		}

		public DataItem<string> AllowedUpgradeItems
		{
			get
			{
				return this.pAllowedUpgradeItems;
			}
			set
			{
				this.pAllowedUpgradeItems = value;
			}
		}

		public DataItem<string> RestrictedUpgradeItems
		{
			get
			{
				return this.pRestrictedUpgradeItems;
			}
			set
			{
				this.pRestrictedUpgradeItems = value;
			}
		}

		public DataItem<string> UpgradeActionSound
		{
			get
			{
				return this.pUpgradeActionSound;
			}
			set
			{
				this.pUpgradeActionSound = value;
			}
		}

		public DataItem<string> RepairActionSound
		{
			get
			{
				return this.pRepairActionSound;
			}
			set
			{
				this.pRepairActionSound = value;
			}
		}

		public DataItem<string> ReferenceItem
		{
			get
			{
				return this.pReferenceItem;
			}
			set
			{
				this.pReferenceItem = value;
			}
		}

		public DataItem<string> Mesh
		{
			get
			{
				return this.pMesh;
			}
			set
			{
				this.pMesh = value;
			}
		}

		public DataItem<int> ActionIdx
		{
			get
			{
				return this.pActionIdx;
			}
			set
			{
				this.pActionIdx = value;
			}
		}

		public DataItem<string> Title
		{
			get
			{
				return this.pTitle;
			}
			set
			{
				this.pTitle = value;
			}
		}

		public DataItem<string> Description
		{
			get
			{
				return this.pDescription;
			}
			set
			{
				this.pDescription = value;
			}
		}

		public DataItem<string> RecipesToLearn
		{
			get
			{
				return this.pRecipesToLearn;
			}
			set
			{
				this.pRecipesToLearn = value;
			}
		}

		public DataItem<string> InstantiateOnLoad
		{
			get
			{
				return this.pInstantiateOnLoad;
			}
			set
			{
				this.pInstantiateOnLoad = value;
			}
		}

		public DataItem<string> SoundDraw
		{
			get
			{
				return this.pSoundDraw;
			}
			set
			{
				this.pSoundDraw = value;
			}
		}

		public DataItem<DamageBonusData> DamageBonus
		{
			get
			{
				return this.pDamageBonus;
			}
			set
			{
				this.pDamageBonus = value;
			}
		}

		public DataItem<ExplosionData> Explosion
		{
			get
			{
				return this.pExplosion;
			}
			set
			{
				this.pExplosion = value;
			}
		}

		public List<IDataItem> GetDisplayValues(bool _recursive = true)
		{
			List<IDataItem> list = new List<IDataItem>();
			if (_recursive && this.pDamageBonus != null)
			{
				list.AddRange(this.pDamageBonus.Value.GetDisplayValues(true));
			}
			if (_recursive && this.pExplosion != null)
			{
				list.AddRange(this.pExplosion.Value.GetDisplayValues(true));
			}
			return list;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<float> pDelay;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<float> pRange;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<string> pSoundStart;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<string> pSoundRepeat;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<string> pSoundEnd;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<string> pSoundEmpty;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<string> pSoundReload;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<string> pSoundWarning;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<string> pStaminaUsage;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<string> pUseTime;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<string> pFocusedBlockname1;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<string> pFocusedBlockname2;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<string> pFocusedBlockname3;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<string> pFocusedBlockname4;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<string> pFocusedBlockname5;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<string> pFocusedBlockname6;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<string> pFocusedBlockname7;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<string> pFocusedBlockname8;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<string> pFocusedBlockname9;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<string> pChangeItemTo;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<string> pChangeBlockTo;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<string> pDoBlockAction;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<float> pGainHealth;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<float> pGainFood;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<float> pGainWater;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<float> pGainStamina;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<float> pGainSickness;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<float> pGainWellness;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<string> pBuff;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<string> pBuffChance;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<string> pDebuff;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<string> pCreateItem;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<int> pConditionRaycastBlock;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<int> pGainGas;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<bool> pConsume;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<string> pBlockname;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<float> pThrowStrengthDefault;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<float> pThrowStrengthMax;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<float> pMaxStrainTime;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<int> pMagazineSize;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<string> pMagazineItem;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<float> pReloadTime;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<string> pBulletIcon;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<int> pRaysPerShot;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<float> pRaysSpread;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<float> pSphere;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<int> pCrosshairMinDistance;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<int> pCrosshairMaxDistance;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<int> pDamageEntity;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<float> pDamageBlock;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<string> pParticlesMuzzleFire;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<string> pParticlesMuzzleSmoke;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<float> pBlockRange;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<bool> pAutoFire;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<float> pHordeMeterRate;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<float> pHordeMeterDistance;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<string> pHitmaskOverride;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<bool> pSingleMagazineUsage;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<string> pBulletMaterial;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<bool> pInfiniteAmmo;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<float> pZoomMaxOut;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<float> pZoomMaxIn;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<string> pZoomOverlay;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<int> pVelocity;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<float> pFlyTime;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<float> pLifeTime;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<float> pCollisionRadius;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<int> pProjectileInitialVelocity;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<string> pFertileblock;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<string> pAdjacentblock;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<int> pRepairAmount;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<int> pUpgradeHitOffset;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<string> pAllowedUpgradeItems;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<string> pRestrictedUpgradeItems;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<string> pUpgradeActionSound;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<string> pRepairActionSound;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<string> pReferenceItem;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<string> pMesh;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<int> pActionIdx;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<string> pTitle;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<string> pDescription;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<string> pRecipesToLearn;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<string> pInstantiateOnLoad;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<string> pSoundDraw;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<DamageBonusData> pDamageBonus;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<ExplosionData> pExplosion;

		public static class Parser
		{
			[PublicizedFrom(EAccessModifier.Private)]
			public static DataItem<string> ParseItem(string _string, PositionXmlElement _elem)
			{
				string text;
				try
				{
					text = stringParser.Parse(ParserUtils.ParseStringAttribute(_elem, "value", true, null));
				}
				catch (Exception ex)
				{
					throw new InvalidValueException(string.Concat(new string[]
					{
						"Could not parse attribute \"",
						_elem.Name,
						"\" value \"",
						ParserUtils.ParseStringAttribute(_elem, "value", true, null),
						"\""
					}), _elem.LineNumber, ex);
				}
				return new DataItem<string>(_string, text);
			}

			public static ItemAction Parse(PositionXmlElement _elem, Dictionary<PositionXmlElement, DataItem<ItemClass>> _updateLater)
			{
				string text = (_elem.HasAttribute("class") ? _elem.GetAttribute("class") : "ItemAction");
				Type type = Type.GetType(typeof(ItemActionData.Parser).Namespace + "." + text);
				if (type == null)
				{
					type = Type.GetType(text);
					if (type == null)
					{
						throw new InvalidValueException("Specified class \"" + text + "\" not found", _elem.LineNumber);
					}
				}
				ItemAction itemAction = (ItemAction)Activator.CreateInstance(type);
				Dictionary<string, int> dictionary = new Dictionary<string, int>();
				foreach (object obj in _elem.ChildNodes)
				{
					XmlNode xmlNode = (XmlNode)obj;
					XmlNodeType nodeType = xmlNode.NodeType;
					if (nodeType != XmlNodeType.Element)
					{
						if (nodeType != XmlNodeType.Comment)
						{
							throw new UnexpectedElementException("Unknown node \"" + xmlNode.NodeType.ToString() + "\" found while parsing ItemAction", ((IXmlLineInfo)xmlNode).LineNumber);
						}
					}
					else
					{
						PositionXmlElement positionXmlElement = (PositionXmlElement)xmlNode;
						if (!ItemActionData.Parser.knownAttributesMultiplicity.ContainsKey(positionXmlElement.Name))
						{
							throw new UnexpectedElementException("Unknown element \"" + xmlNode.Name + "\" found while parsing ItemAction", ((IXmlLineInfo)xmlNode).LineNumber);
						}
						string text2 = positionXmlElement.Name;
						uint num = <PrivateImplementationDetails>.ComputeStringHash(text2);
						if (num <= 2050383924U)
						{
							if (num <= 973038343U)
							{
								if (num <= 477934085U)
								{
									if (num <= 264004213U)
									{
										if (num <= 91459346U)
										{
											if (num != 32426346U)
											{
												if (num == 91459346U)
												{
													if (text2 == "CrosshairMinDistance")
													{
														int num2;
														try
														{
															num2 = intParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
														}
														catch (Exception ex)
														{
															throw new InvalidValueException(string.Concat(new string[]
															{
																"Could not parse attribute \"",
																positionXmlElement.Name,
																"\" value \"",
																ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
																"\""
															}), positionXmlElement.LineNumber, ex);
														}
														DataItem<int> dataItem = new DataItem<int>("CrosshairMinDistance", num2);
														itemAction.pCrosshairMinDistance = dataItem;
													}
												}
											}
											else if (text2 == "HitmaskOverride")
											{
												string text3;
												try
												{
													text3 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
												}
												catch (Exception ex2)
												{
													throw new InvalidValueException(string.Concat(new string[]
													{
														"Could not parse attribute \"",
														positionXmlElement.Name,
														"\" value \"",
														ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
														"\""
													}), positionXmlElement.LineNumber, ex2);
												}
												DataItem<string> dataItem2 = new DataItem<string>("HitmaskOverride", text3);
												itemAction.pHitmaskOverride = dataItem2;
											}
										}
										else if (num != 132142556U)
										{
											if (num != 248841283U)
											{
												if (num == 264004213U)
												{
													if (text2 == "ProjectileInitialVelocity")
													{
														int num3;
														try
														{
															num3 = intParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
														}
														catch (Exception ex3)
														{
															throw new InvalidValueException(string.Concat(new string[]
															{
																"Could not parse attribute \"",
																positionXmlElement.Name,
																"\" value \"",
																ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
																"\""
															}), positionXmlElement.LineNumber, ex3);
														}
														DataItem<int> dataItem3 = new DataItem<int>("ProjectileInitialVelocity", num3);
														itemAction.pProjectileInitialVelocity = dataItem3;
													}
												}
											}
											else if (text2 == "RaysSpread")
											{
												float num4;
												try
												{
													num4 = floatParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
												}
												catch (Exception ex4)
												{
													throw new InvalidValueException(string.Concat(new string[]
													{
														"Could not parse attribute \"",
														positionXmlElement.Name,
														"\" value \"",
														ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
														"\""
													}), positionXmlElement.LineNumber, ex4);
												}
												DataItem<float> dataItem4 = new DataItem<float>("RaysSpread", num4);
												itemAction.pRaysSpread = dataItem4;
											}
										}
										else if (text2 == "Mesh")
										{
											string text4;
											try
											{
												text4 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
											}
											catch (Exception ex5)
											{
												throw new InvalidValueException(string.Concat(new string[]
												{
													"Could not parse attribute \"",
													positionXmlElement.Name,
													"\" value \"",
													ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
													"\""
												}), positionXmlElement.LineNumber, ex5);
											}
											DataItem<string> dataItem5 = new DataItem<string>("Mesh", text4);
											itemAction.pMesh = dataItem5;
										}
									}
									else if (num <= 388984571U)
									{
										if (num != 383713930U)
										{
											if (num == 388984571U)
											{
												if (text2 == "DamageEntity")
												{
													int num5;
													try
													{
														num5 = intParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
													}
													catch (Exception ex6)
													{
														throw new InvalidValueException(string.Concat(new string[]
														{
															"Could not parse attribute \"",
															positionXmlElement.Name,
															"\" value \"",
															ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
															"\""
														}), positionXmlElement.LineNumber, ex6);
													}
													DataItem<int> dataItem6 = new DataItem<int>("DamageEntity", num5);
													itemAction.pDamageEntity = dataItem6;
												}
											}
										}
										else if (text2 == "AutoFire")
										{
											bool flag;
											try
											{
												flag = boolParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
											}
											catch (Exception ex7)
											{
												throw new InvalidValueException(string.Concat(new string[]
												{
													"Could not parse attribute \"",
													positionXmlElement.Name,
													"\" value \"",
													ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
													"\""
												}), positionXmlElement.LineNumber, ex7);
											}
											DataItem<bool> dataItem7 = new DataItem<bool>("AutoFire", flag);
											itemAction.pAutoFire = dataItem7;
										}
									}
									else if (num != 422257095U)
									{
										if (num != 441323083U)
										{
											if (num == 477934085U)
											{
												if (text2 == "MaxStrainTime")
												{
													float num6;
													try
													{
														num6 = floatParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
													}
													catch (Exception ex8)
													{
														throw new InvalidValueException(string.Concat(new string[]
														{
															"Could not parse attribute \"",
															positionXmlElement.Name,
															"\" value \"",
															ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
															"\""
														}), positionXmlElement.LineNumber, ex8);
													}
													DataItem<float> dataItem8 = new DataItem<float>("MaxStrainTime", num6);
													itemAction.pMaxStrainTime = dataItem8;
												}
											}
										}
										else if (text2 == "RaysPerShot")
										{
											int num7;
											try
											{
												num7 = intParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
											}
											catch (Exception ex9)
											{
												throw new InvalidValueException(string.Concat(new string[]
												{
													"Could not parse attribute \"",
													positionXmlElement.Name,
													"\" value \"",
													ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
													"\""
												}), positionXmlElement.LineNumber, ex9);
											}
											DataItem<int> dataItem9 = new DataItem<int>("RaysPerShot", num7);
											itemAction.pRaysPerShot = dataItem9;
										}
									}
									else if (text2 == "GainGas")
									{
										int num8;
										try
										{
											num8 = intParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
										}
										catch (Exception ex10)
										{
											throw new InvalidValueException(string.Concat(new string[]
											{
												"Could not parse attribute \"",
												positionXmlElement.Name,
												"\" value \"",
												ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
												"\""
											}), positionXmlElement.LineNumber, ex10);
										}
										DataItem<int> dataItem10 = new DataItem<int>("GainGas", num8);
										itemAction.pGainGas = dataItem10;
									}
								}
								else if (num <= 678358751U)
								{
									if (num <= 573162709U)
									{
										if (num != 550270072U)
										{
											if (num == 573162709U)
											{
												if (text2 == "SoundEmpty")
												{
													string text5;
													try
													{
														text5 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
													}
													catch (Exception ex11)
													{
														throw new InvalidValueException(string.Concat(new string[]
														{
															"Could not parse attribute \"",
															positionXmlElement.Name,
															"\" value \"",
															ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
															"\""
														}), positionXmlElement.LineNumber, ex11);
													}
													DataItem<string> dataItem11 = new DataItem<string>("SoundEmpty", text5);
													itemAction.pSoundEmpty = dataItem11;
												}
											}
										}
										else if (text2 == "Delay")
										{
											float num9;
											try
											{
												num9 = floatParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
											}
											catch (Exception ex12)
											{
												throw new InvalidValueException(string.Concat(new string[]
												{
													"Could not parse attribute \"",
													positionXmlElement.Name,
													"\" value \"",
													ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
													"\""
												}), positionXmlElement.LineNumber, ex12);
											}
											DataItem<float> dataItem12 = new DataItem<float>("Delay", num9);
											itemAction.pDelay = dataItem12;
										}
									}
									else if (num != 573416248U)
									{
										if (num != 617902505U)
										{
											if (num == 678358751U)
											{
												if (text2 == "GainStamina")
												{
													float num10;
													try
													{
														num10 = floatParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
													}
													catch (Exception ex13)
													{
														throw new InvalidValueException(string.Concat(new string[]
														{
															"Could not parse attribute \"",
															positionXmlElement.Name,
															"\" value \"",
															ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
															"\""
														}), positionXmlElement.LineNumber, ex13);
													}
													DataItem<float> dataItem13 = new DataItem<float>("GainStamina", num10);
													itemAction.pGainStamina = dataItem13;
												}
											}
										}
										else if (text2 == "Title")
										{
											string text6;
											try
											{
												text6 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
											}
											catch (Exception ex14)
											{
												throw new InvalidValueException(string.Concat(new string[]
												{
													"Could not parse attribute \"",
													positionXmlElement.Name,
													"\" value \"",
													ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
													"\""
												}), positionXmlElement.LineNumber, ex14);
											}
											DataItem<string> dataItem14 = new DataItem<string>("Title", text6);
											itemAction.pTitle = dataItem14;
										}
									}
									else if (text2 == "Sphere")
									{
										float num11;
										try
										{
											num11 = floatParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
										}
										catch (Exception ex15)
										{
											throw new InvalidValueException(string.Concat(new string[]
											{
												"Could not parse attribute \"",
												positionXmlElement.Name,
												"\" value \"",
												ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
												"\""
											}), positionXmlElement.LineNumber, ex15);
										}
										DataItem<float> dataItem15 = new DataItem<float>("Sphere", num11);
										itemAction.pSphere = dataItem15;
									}
								}
								else if (num <= 859861643U)
								{
									if (num != 738624775U)
									{
										if (num != 843848237U)
										{
											if (num == 859861643U)
											{
												if (text2 == "SoundRepeat")
												{
													string text7;
													try
													{
														text7 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
													}
													catch (Exception ex16)
													{
														throw new InvalidValueException(string.Concat(new string[]
														{
															"Could not parse attribute \"",
															positionXmlElement.Name,
															"\" value \"",
															ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
															"\""
														}), positionXmlElement.LineNumber, ex16);
													}
													DataItem<string> dataItem16 = new DataItem<string>("SoundRepeat", text7);
													itemAction.pSoundRepeat = dataItem16;
												}
											}
										}
										else if (text2 == "CollisionRadius")
										{
											float num12;
											try
											{
												num12 = floatParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
											}
											catch (Exception ex17)
											{
												throw new InvalidValueException(string.Concat(new string[]
												{
													"Could not parse attribute \"",
													positionXmlElement.Name,
													"\" value \"",
													ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
													"\""
												}), positionXmlElement.LineNumber, ex17);
											}
											DataItem<float> dataItem17 = new DataItem<float>("CollisionRadius", num12);
											itemAction.pCollisionRadius = dataItem17;
										}
									}
									else if (text2 == "ThrowStrengthDefault")
									{
										float num13;
										try
										{
											num13 = floatParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
										}
										catch (Exception ex18)
										{
											throw new InvalidValueException(string.Concat(new string[]
											{
												"Could not parse attribute \"",
												positionXmlElement.Name,
												"\" value \"",
												ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
												"\""
											}), positionXmlElement.LineNumber, ex18);
										}
										DataItem<float> dataItem18 = new DataItem<float>("ThrowStrengthDefault", num13);
										itemAction.pThrowStrengthDefault = dataItem18;
									}
								}
								else if (num != 883311634U)
								{
									if (num != 906214847U)
									{
										if (num == 973038343U)
										{
											if (text2 == "InfiniteAmmo")
											{
												bool flag2;
												try
												{
													flag2 = boolParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
												}
												catch (Exception ex19)
												{
													throw new InvalidValueException(string.Concat(new string[]
													{
														"Could not parse attribute \"",
														positionXmlElement.Name,
														"\" value \"",
														ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
														"\""
													}), positionXmlElement.LineNumber, ex19);
												}
												DataItem<bool> dataItem19 = new DataItem<bool>("InfiniteAmmo", flag2);
												itemAction.pInfiniteAmmo = dataItem19;
											}
										}
									}
									else if (text2 == "ChangeBlockTo")
									{
										string text8;
										try
										{
											text8 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
										}
										catch (Exception ex20)
										{
											throw new InvalidValueException(string.Concat(new string[]
											{
												"Could not parse attribute \"",
												positionXmlElement.Name,
												"\" value \"",
												ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
												"\""
											}), positionXmlElement.LineNumber, ex20);
										}
										DataItem<string> dataItem20 = new DataItem<string>("ChangeBlockTo", text8);
										itemAction.pChangeBlockTo = dataItem20;
									}
								}
								else if (text2 == "ZoomOverlay")
								{
									string text9;
									try
									{
										text9 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
									}
									catch (Exception ex21)
									{
										throw new InvalidValueException(string.Concat(new string[]
										{
											"Could not parse attribute \"",
											positionXmlElement.Name,
											"\" value \"",
											ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
											"\""
										}), positionXmlElement.LineNumber, ex21);
									}
									DataItem<string> dataItem21 = new DataItem<string>("ZoomOverlay", text9);
									itemAction.pZoomOverlay = dataItem21;
								}
							}
							else if (num <= 1725856265U)
							{
								if (num <= 1237474039U)
								{
									if (num <= 1173181999U)
									{
										if (num != 1157581142U)
										{
											if (num == 1173181999U)
											{
												if (text2 == "BlockRange")
												{
													float num14;
													try
													{
														num14 = floatParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
													}
													catch (Exception ex22)
													{
														throw new InvalidValueException(string.Concat(new string[]
														{
															"Could not parse attribute \"",
															positionXmlElement.Name,
															"\" value \"",
															ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
															"\""
														}), positionXmlElement.LineNumber, ex22);
													}
													DataItem<float> dataItem22 = new DataItem<float>("BlockRange", num14);
													itemAction.pBlockRange = dataItem22;
												}
											}
										}
										else if (text2 == "SingleMagazineUsage")
										{
											bool flag3;
											try
											{
												flag3 = boolParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
											}
											catch (Exception ex23)
											{
												throw new InvalidValueException(string.Concat(new string[]
												{
													"Could not parse attribute \"",
													positionXmlElement.Name,
													"\" value \"",
													ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
													"\""
												}), positionXmlElement.LineNumber, ex23);
											}
											DataItem<bool> dataItem23 = new DataItem<bool>("SingleMagazineUsage", flag3);
											itemAction.pSingleMagazineUsage = dataItem23;
										}
									}
									else if (num != 1180803064U)
									{
										if (num != 1221226493U)
										{
											if (num == 1237474039U)
											{
												if (text2 == "DamageBonus")
												{
													DamageBonusData damageBonusData = DamageBonusData.Parser.Parse(positionXmlElement, _updateLater);
													DataItem<DamageBonusData> dataItem24 = new DataItem<DamageBonusData>("DamageBonus", damageBonusData);
													itemAction.pDamageBonus = dataItem24;
												}
											}
										}
										else if (text2 == "RecipesToLearn")
										{
											string text10;
											try
											{
												text10 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
											}
											catch (Exception ex24)
											{
												throw new InvalidValueException(string.Concat(new string[]
												{
													"Could not parse attribute \"",
													positionXmlElement.Name,
													"\" value \"",
													ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
													"\""
												}), positionXmlElement.LineNumber, ex24);
											}
											DataItem<string> dataItem25 = new DataItem<string>("RecipesToLearn", text10);
											itemAction.pRecipesToLearn = dataItem25;
										}
									}
									else if (text2 == "SoundStart")
									{
										string text11;
										try
										{
											text11 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
										}
										catch (Exception ex25)
										{
											throw new InvalidValueException(string.Concat(new string[]
											{
												"Could not parse attribute \"",
												positionXmlElement.Name,
												"\" value \"",
												ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
												"\""
											}), positionXmlElement.LineNumber, ex25);
										}
										DataItem<string> dataItem26 = new DataItem<string>("SoundStart", text11);
										itemAction.pSoundStart = dataItem26;
									}
								}
								else if (num <= 1330610628U)
								{
									if (num != 1259223448U)
									{
										if (num != 1292404073U)
										{
											if (num == 1330610628U)
											{
												if (text2 == "CrosshairMaxDistance")
												{
													int num15;
													try
													{
														num15 = intParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
													}
													catch (Exception ex26)
													{
														throw new InvalidValueException(string.Concat(new string[]
														{
															"Could not parse attribute \"",
															positionXmlElement.Name,
															"\" value \"",
															ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
															"\""
														}), positionXmlElement.LineNumber, ex26);
													}
													DataItem<int> dataItem27 = new DataItem<int>("CrosshairMaxDistance", num15);
													itemAction.pCrosshairMaxDistance = dataItem27;
												}
											}
										}
										else if (text2 == "SoundEnd")
										{
											string text12;
											try
											{
												text12 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
											}
											catch (Exception ex27)
											{
												throw new InvalidValueException(string.Concat(new string[]
												{
													"Could not parse attribute \"",
													positionXmlElement.Name,
													"\" value \"",
													ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
													"\""
												}), positionXmlElement.LineNumber, ex27);
											}
											DataItem<string> dataItem28 = new DataItem<string>("SoundEnd", text12);
											itemAction.pSoundEnd = dataItem28;
										}
									}
									else if (text2 == "ZoomMaxOut")
									{
										float num16;
										try
										{
											num16 = floatParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
										}
										catch (Exception ex28)
										{
											throw new InvalidValueException(string.Concat(new string[]
											{
												"Could not parse attribute \"",
												positionXmlElement.Name,
												"\" value \"",
												ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
												"\""
											}), positionXmlElement.LineNumber, ex28);
										}
										DataItem<float> dataItem29 = new DataItem<float>("ZoomMaxOut", num16);
										itemAction.pZoomMaxOut = dataItem29;
									}
								}
								else if (num != 1601156781U)
								{
									if (num != 1660679176U)
									{
										if (num == 1725856265U)
										{
											if (text2 == "Description")
											{
												string text13;
												try
												{
													text13 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
												}
												catch (Exception ex29)
												{
													throw new InvalidValueException(string.Concat(new string[]
													{
														"Could not parse attribute \"",
														positionXmlElement.Name,
														"\" value \"",
														ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
														"\""
													}), positionXmlElement.LineNumber, ex29);
												}
												DataItem<string> dataItem30 = new DataItem<string>("Description", text13);
												itemAction.pDescription = dataItem30;
											}
										}
									}
									else if (text2 == "BuffChance")
									{
										string text14;
										try
										{
											text14 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
										}
										catch (Exception ex30)
										{
											throw new InvalidValueException(string.Concat(new string[]
											{
												"Could not parse attribute \"",
												positionXmlElement.Name,
												"\" value \"",
												ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
												"\""
											}), positionXmlElement.LineNumber, ex30);
										}
										DataItem<string> dataItem31 = new DataItem<string>("BuffChance", text14);
										itemAction.pBuffChance = dataItem31;
									}
								}
								else if (text2 == "ReloadTime")
								{
									float num17;
									try
									{
										num17 = floatParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
									}
									catch (Exception ex31)
									{
										throw new InvalidValueException(string.Concat(new string[]
										{
											"Could not parse attribute \"",
											positionXmlElement.Name,
											"\" value \"",
											ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
											"\""
										}), positionXmlElement.LineNumber, ex31);
									}
									DataItem<float> dataItem32 = new DataItem<float>("ReloadTime", num17);
									itemAction.pReloadTime = dataItem32;
								}
							}
							else if (num <= 1811911295U)
							{
								if (num <= 1744800819U)
								{
									if (num != 1728023200U)
									{
										if (num == 1744800819U)
										{
											if (text2 == "FocusedBlockname5")
											{
												string text15;
												try
												{
													text15 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
												}
												catch (Exception ex32)
												{
													throw new InvalidValueException(string.Concat(new string[]
													{
														"Could not parse attribute \"",
														positionXmlElement.Name,
														"\" value \"",
														ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
														"\""
													}), positionXmlElement.LineNumber, ex32);
												}
												DataItem<string> dataItem33 = new DataItem<string>("FocusedBlockname5", text15);
												itemAction.pFocusedBlockname5 = dataItem33;
											}
										}
									}
									else if (text2 == "FocusedBlockname4")
									{
										string text16;
										try
										{
											text16 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
										}
										catch (Exception ex33)
										{
											throw new InvalidValueException(string.Concat(new string[]
											{
												"Could not parse attribute \"",
												positionXmlElement.Name,
												"\" value \"",
												ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
												"\""
											}), positionXmlElement.LineNumber, ex33);
										}
										DataItem<string> dataItem34 = new DataItem<string>("FocusedBlockname4", text16);
										itemAction.pFocusedBlockname4 = dataItem34;
									}
								}
								else if (num != 1761578438U)
								{
									if (num != 1778356057U)
									{
										if (num == 1811911295U)
										{
											if (text2 == "FocusedBlockname1")
											{
												string text17;
												try
												{
													text17 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
												}
												catch (Exception ex34)
												{
													throw new InvalidValueException(string.Concat(new string[]
													{
														"Could not parse attribute \"",
														positionXmlElement.Name,
														"\" value \"",
														ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
														"\""
													}), positionXmlElement.LineNumber, ex34);
												}
												DataItem<string> dataItem35 = new DataItem<string>("FocusedBlockname1", text17);
												itemAction.pFocusedBlockname1 = dataItem35;
											}
										}
									}
									else if (text2 == "FocusedBlockname7")
									{
										string text18;
										try
										{
											text18 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
										}
										catch (Exception ex35)
										{
											throw new InvalidValueException(string.Concat(new string[]
											{
												"Could not parse attribute \"",
												positionXmlElement.Name,
												"\" value \"",
												ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
												"\""
											}), positionXmlElement.LineNumber, ex35);
										}
										DataItem<string> dataItem36 = new DataItem<string>("FocusedBlockname7", text18);
										itemAction.pFocusedBlockname7 = dataItem36;
									}
								}
								else if (text2 == "FocusedBlockname6")
								{
									string text19;
									try
									{
										text19 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
									}
									catch (Exception ex36)
									{
										throw new InvalidValueException(string.Concat(new string[]
										{
											"Could not parse attribute \"",
											positionXmlElement.Name,
											"\" value \"",
											ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
											"\""
										}), positionXmlElement.LineNumber, ex36);
									}
									DataItem<string> dataItem37 = new DataItem<string>("FocusedBlockname6", text19);
									itemAction.pFocusedBlockname6 = dataItem37;
								}
							}
							else if (num <= 1929345774U)
							{
								if (num != 1828688914U)
								{
									if (num != 1845466533U)
									{
										if (num == 1929345774U)
										{
											if (text2 == "LifeTime")
											{
												float num18;
												try
												{
													num18 = floatParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
												}
												catch (Exception ex37)
												{
													throw new InvalidValueException(string.Concat(new string[]
													{
														"Could not parse attribute \"",
														positionXmlElement.Name,
														"\" value \"",
														ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
														"\""
													}), positionXmlElement.LineNumber, ex37);
												}
												DataItem<float> dataItem38 = new DataItem<float>("LifeTime", num18);
												itemAction.pLifeTime = dataItem38;
											}
										}
									}
									else if (text2 == "FocusedBlockname3")
									{
										string text20;
										try
										{
											text20 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
										}
										catch (Exception ex38)
										{
											throw new InvalidValueException(string.Concat(new string[]
											{
												"Could not parse attribute \"",
												positionXmlElement.Name,
												"\" value \"",
												ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
												"\""
											}), positionXmlElement.LineNumber, ex38);
										}
										DataItem<string> dataItem39 = new DataItem<string>("FocusedBlockname3", text20);
										itemAction.pFocusedBlockname3 = dataItem39;
									}
								}
								else if (text2 == "FocusedBlockname2")
								{
									string text21;
									try
									{
										text21 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
									}
									catch (Exception ex39)
									{
										throw new InvalidValueException(string.Concat(new string[]
										{
											"Could not parse attribute \"",
											positionXmlElement.Name,
											"\" value \"",
											ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
											"\""
										}), positionXmlElement.LineNumber, ex39);
									}
									DataItem<string> dataItem40 = new DataItem<string>("FocusedBlockname2", text21);
									itemAction.pFocusedBlockname2 = dataItem40;
								}
							}
							else if (num != 1929354628U)
							{
								if (num != 1946132247U)
								{
									if (num == 2050383924U)
									{
										if (text2 == "RestrictedUpgradeItems")
										{
											string text22;
											try
											{
												text22 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
											}
											catch (Exception ex40)
											{
												throw new InvalidValueException(string.Concat(new string[]
												{
													"Could not parse attribute \"",
													positionXmlElement.Name,
													"\" value \"",
													ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
													"\""
												}), positionXmlElement.LineNumber, ex40);
											}
											DataItem<string> dataItem41 = new DataItem<string>("RestrictedUpgradeItems", text22);
											itemAction.pRestrictedUpgradeItems = dataItem41;
										}
									}
								}
								else if (text2 == "FocusedBlockname9")
								{
									string text23;
									try
									{
										text23 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
									}
									catch (Exception ex41)
									{
										throw new InvalidValueException(string.Concat(new string[]
										{
											"Could not parse attribute \"",
											positionXmlElement.Name,
											"\" value \"",
											ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
											"\""
										}), positionXmlElement.LineNumber, ex41);
									}
									DataItem<string> dataItem42 = new DataItem<string>("FocusedBlockname9", text23);
									itemAction.pFocusedBlockname9 = dataItem42;
								}
							}
							else if (text2 == "FocusedBlockname8")
							{
								string text24;
								try
								{
									text24 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
								}
								catch (Exception ex42)
								{
									throw new InvalidValueException(string.Concat(new string[]
									{
										"Could not parse attribute \"",
										positionXmlElement.Name,
										"\" value \"",
										ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
										"\""
									}), positionXmlElement.LineNumber, ex42);
								}
								DataItem<string> dataItem43 = new DataItem<string>("FocusedBlockname8", text24);
								itemAction.pFocusedBlockname8 = dataItem43;
							}
						}
						else if (num <= 3213271394U)
						{
							if (num <= 2731875518U)
							{
								if (num <= 2292684416U)
								{
									if (num <= 2079371571U)
									{
										if (num != 2054944794U)
										{
											if (num == 2079371571U)
											{
												if (text2 == "Fertileblock")
												{
													string text25;
													try
													{
														text25 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
													}
													catch (Exception ex43)
													{
														throw new InvalidValueException(string.Concat(new string[]
														{
															"Could not parse attribute \"",
															positionXmlElement.Name,
															"\" value \"",
															ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
															"\""
														}), positionXmlElement.LineNumber, ex43);
													}
													DataItem<string> dataItem44 = new DataItem<string>("Fertileblock", text25);
													itemAction.pFertileblock = dataItem44;
												}
											}
										}
										else if (text2 == "ParticlesMuzzleSmoke")
										{
											string text26;
											try
											{
												text26 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
											}
											catch (Exception ex44)
											{
												throw new InvalidValueException(string.Concat(new string[]
												{
													"Could not parse attribute \"",
													positionXmlElement.Name,
													"\" value \"",
													ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
													"\""
												}), positionXmlElement.LineNumber, ex44);
											}
											DataItem<string> dataItem45 = new DataItem<string>("ParticlesMuzzleSmoke", text26);
											itemAction.pParticlesMuzzleSmoke = dataItem45;
										}
									}
									else if (num != 2205678605U)
									{
										if (num != 2214691755U)
										{
											if (num == 2292684416U)
											{
												if (text2 == "Explosion")
												{
													ExplosionData explosionData = ExplosionData.Parser.Parse(positionXmlElement, _updateLater);
													DataItem<ExplosionData> dataItem46 = new DataItem<ExplosionData>("Explosion", explosionData);
													itemAction.pExplosion = dataItem46;
												}
											}
										}
										else if (text2 == "GainSickness")
										{
											float num19;
											try
											{
												num19 = floatParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
											}
											catch (Exception ex45)
											{
												throw new InvalidValueException(string.Concat(new string[]
												{
													"Could not parse attribute \"",
													positionXmlElement.Name,
													"\" value \"",
													ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
													"\""
												}), positionXmlElement.LineNumber, ex45);
											}
											DataItem<float> dataItem47 = new DataItem<float>("GainSickness", num19);
											itemAction.pGainSickness = dataItem47;
										}
									}
									else if (text2 == "ParticlesMuzzleFire")
									{
										string text27;
										try
										{
											text27 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
										}
										catch (Exception ex46)
										{
											throw new InvalidValueException(string.Concat(new string[]
											{
												"Could not parse attribute \"",
												positionXmlElement.Name,
												"\" value \"",
												ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
												"\""
											}), positionXmlElement.LineNumber, ex46);
										}
										DataItem<string> dataItem48 = new DataItem<string>("ParticlesMuzzleFire", text27);
										itemAction.pParticlesMuzzleFire = dataItem48;
									}
								}
								else if (num <= 2391273097U)
								{
									if (num != 2296213333U)
									{
										if (num == 2391273097U)
										{
											if (text2 == "GainWater")
											{
												float num20;
												try
												{
													num20 = floatParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
												}
												catch (Exception ex47)
												{
													throw new InvalidValueException(string.Concat(new string[]
													{
														"Could not parse attribute \"",
														positionXmlElement.Name,
														"\" value \"",
														ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
														"\""
													}), positionXmlElement.LineNumber, ex47);
												}
												DataItem<float> dataItem49 = new DataItem<float>("GainWater", num20);
												itemAction.pGainWater = dataItem49;
											}
										}
									}
									else if (text2 == "UseTime")
									{
										string text28;
										try
										{
											text28 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
										}
										catch (Exception ex48)
										{
											throw new InvalidValueException(string.Concat(new string[]
											{
												"Could not parse attribute \"",
												positionXmlElement.Name,
												"\" value \"",
												ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
												"\""
											}), positionXmlElement.LineNumber, ex48);
										}
										DataItem<string> dataItem50 = new DataItem<string>("UseTime", text28);
										itemAction.pUseTime = dataItem50;
									}
								}
								else if (num != 2508081957U)
								{
									if (num != 2654093974U)
									{
										if (num == 2731875518U)
										{
											if (text2 == "ActionIdx")
											{
												int num21;
												try
												{
													num21 = intParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
												}
												catch (Exception ex49)
												{
													throw new InvalidValueException(string.Concat(new string[]
													{
														"Could not parse attribute \"",
														positionXmlElement.Name,
														"\" value \"",
														ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
														"\""
													}), positionXmlElement.LineNumber, ex49);
												}
												DataItem<int> dataItem51 = new DataItem<int>("ActionIdx", num21);
												itemAction.pActionIdx = dataItem51;
											}
										}
									}
									else if (text2 == "SoundDraw")
									{
										string text29;
										try
										{
											text29 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
										}
										catch (Exception ex50)
										{
											throw new InvalidValueException(string.Concat(new string[]
											{
												"Could not parse attribute \"",
												positionXmlElement.Name,
												"\" value \"",
												ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
												"\""
											}), positionXmlElement.LineNumber, ex50);
										}
										DataItem<string> dataItem52 = new DataItem<string>("SoundDraw", text29);
										itemAction.pSoundDraw = dataItem52;
									}
								}
								else if (text2 == "RepairActionSound")
								{
									string text30;
									try
									{
										text30 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
									}
									catch (Exception ex51)
									{
										throw new InvalidValueException(string.Concat(new string[]
										{
											"Could not parse attribute \"",
											positionXmlElement.Name,
											"\" value \"",
											ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
											"\""
										}), positionXmlElement.LineNumber, ex51);
									}
									DataItem<string> dataItem53 = new DataItem<string>("RepairActionSound", text30);
									itemAction.pRepairActionSound = dataItem53;
								}
							}
							else if (num <= 2951397452U)
							{
								if (num <= 2776146097U)
								{
									if (num != 2735859570U)
									{
										if (num == 2776146097U)
										{
											if (text2 == "GainWellness")
											{
												float num22;
												try
												{
													num22 = floatParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
												}
												catch (Exception ex52)
												{
													throw new InvalidValueException(string.Concat(new string[]
													{
														"Could not parse attribute \"",
														positionXmlElement.Name,
														"\" value \"",
														ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
														"\""
													}), positionXmlElement.LineNumber, ex52);
												}
												DataItem<float> dataItem54 = new DataItem<float>("GainWellness", num22);
												itemAction.pGainWellness = dataItem54;
											}
										}
									}
									else if (text2 == "Range")
									{
										float num23;
										try
										{
											num23 = floatParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
										}
										catch (Exception ex53)
										{
											throw new InvalidValueException(string.Concat(new string[]
											{
												"Could not parse attribute \"",
												positionXmlElement.Name,
												"\" value \"",
												ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
												"\""
											}), positionXmlElement.LineNumber, ex53);
										}
										DataItem<float> dataItem55 = new DataItem<float>("Range", num23);
										itemAction.pRange = dataItem55;
									}
								}
								else if (num != 2889970889U)
								{
									if (num != 2916596296U)
									{
										if (num == 2951397452U)
										{
											if (text2 == "ThrowStrengthMax")
											{
												float num24;
												try
												{
													num24 = floatParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
												}
												catch (Exception ex54)
												{
													throw new InvalidValueException(string.Concat(new string[]
													{
														"Could not parse attribute \"",
														positionXmlElement.Name,
														"\" value \"",
														ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
														"\""
													}), positionXmlElement.LineNumber, ex54);
												}
												DataItem<float> dataItem56 = new DataItem<float>("ThrowStrengthMax", num24);
												itemAction.pThrowStrengthMax = dataItem56;
											}
										}
									}
									else if (text2 == "SoundWarning")
									{
										string text31;
										try
										{
											text31 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
										}
										catch (Exception ex55)
										{
											throw new InvalidValueException(string.Concat(new string[]
											{
												"Could not parse attribute \"",
												positionXmlElement.Name,
												"\" value \"",
												ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
												"\""
											}), positionXmlElement.LineNumber, ex55);
										}
										DataItem<string> dataItem57 = new DataItem<string>("SoundWarning", text31);
										itemAction.pSoundWarning = dataItem57;
									}
								}
								else if (text2 == "Blockname")
								{
									string text32;
									try
									{
										text32 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
									}
									catch (Exception ex56)
									{
										throw new InvalidValueException(string.Concat(new string[]
										{
											"Could not parse attribute \"",
											positionXmlElement.Name,
											"\" value \"",
											ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
											"\""
										}), positionXmlElement.LineNumber, ex56);
									}
									DataItem<string> dataItem58 = new DataItem<string>("Blockname", text32);
									itemAction.pBlockname = dataItem58;
								}
							}
							else if (num <= 3036802414U)
							{
								if (num != 2970340076U)
								{
									if (num != 3027266612U)
									{
										if (num == 3036802414U)
										{
											if (text2 == "GainFood")
											{
												float num25;
												try
												{
													num25 = floatParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
												}
												catch (Exception ex57)
												{
													throw new InvalidValueException(string.Concat(new string[]
													{
														"Could not parse attribute \"",
														positionXmlElement.Name,
														"\" value \"",
														ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
														"\""
													}), positionXmlElement.LineNumber, ex57);
												}
												DataItem<float> dataItem59 = new DataItem<float>("GainFood", num25);
												itemAction.pGainFood = dataItem59;
											}
										}
									}
									else if (text2 == "HordeMeterRate")
									{
										float num26;
										try
										{
											num26 = floatParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
										}
										catch (Exception ex58)
										{
											throw new InvalidValueException(string.Concat(new string[]
											{
												"Could not parse attribute \"",
												positionXmlElement.Name,
												"\" value \"",
												ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
												"\""
											}), positionXmlElement.LineNumber, ex58);
										}
										DataItem<float> dataItem60 = new DataItem<float>("HordeMeterRate", num26);
										itemAction.pHordeMeterRate = dataItem60;
									}
								}
								else if (text2 == "Buff")
								{
									string text33;
									try
									{
										text33 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
									}
									catch (Exception ex59)
									{
										throw new InvalidValueException(string.Concat(new string[]
										{
											"Could not parse attribute \"",
											positionXmlElement.Name,
											"\" value \"",
											ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
											"\""
										}), positionXmlElement.LineNumber, ex59);
									}
									DataItem<string> dataItem61 = new DataItem<string>("Buff", text33);
									itemAction.pBuff = dataItem61;
								}
							}
							else if (num != 3118731031U)
							{
								if (num != 3124789842U)
								{
									if (num == 3213271394U)
									{
										if (text2 == "MagazineSize")
										{
											int num27;
											try
											{
												num27 = intParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
											}
											catch (Exception ex60)
											{
												throw new InvalidValueException(string.Concat(new string[]
												{
													"Could not parse attribute \"",
													positionXmlElement.Name,
													"\" value \"",
													ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
													"\""
												}), positionXmlElement.LineNumber, ex60);
											}
											DataItem<int> dataItem62 = new DataItem<int>("MagazineSize", num27);
											itemAction.pMagazineSize = dataItem62;
										}
									}
								}
								else if (text2 == "Velocity")
								{
									int num28;
									try
									{
										num28 = intParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
									}
									catch (Exception ex61)
									{
										throw new InvalidValueException(string.Concat(new string[]
										{
											"Could not parse attribute \"",
											positionXmlElement.Name,
											"\" value \"",
											ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
											"\""
										}), positionXmlElement.LineNumber, ex61);
									}
									DataItem<int> dataItem63 = new DataItem<int>("Velocity", num28);
									itemAction.pVelocity = dataItem63;
								}
							}
							else if (text2 == "DoBlockAction")
							{
								itemAction.pDoBlockAction = ItemActionData.Parser.ParseItem("DoBlockAction", positionXmlElement);
							}
						}
						else if (num <= 3880205310U)
						{
							if (num <= 3448204316U)
							{
								if (num <= 3261865014U)
								{
									if (num != 3239575924U)
									{
										if (num == 3261865014U)
										{
											if (text2 == "ConditionRaycastBlock")
											{
												int num29;
												try
												{
													num29 = intParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
												}
												catch (Exception ex62)
												{
													throw new InvalidValueException(string.Concat(new string[]
													{
														"Could not parse attribute \"",
														positionXmlElement.Name,
														"\" value \"",
														ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
														"\""
													}), positionXmlElement.LineNumber, ex62);
												}
												DataItem<int> dataItem64 = new DataItem<int>("ConditionRaycastBlock", num29);
												itemAction.pConditionRaycastBlock = dataItem64;
											}
										}
									}
									else if (text2 == "BulletMaterial")
									{
										string text34;
										try
										{
											text34 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
										}
										catch (Exception ex63)
										{
											throw new InvalidValueException(string.Concat(new string[]
											{
												"Could not parse attribute \"",
												positionXmlElement.Name,
												"\" value \"",
												ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
												"\""
											}), positionXmlElement.LineNumber, ex63);
										}
										DataItem<string> dataItem65 = new DataItem<string>("BulletMaterial", text34);
										itemAction.pBulletMaterial = dataItem65;
									}
								}
								else if (num != 3297724363U)
								{
									if (num != 3370626489U)
									{
										if (num == 3448204316U)
										{
											if (text2 == "GainHealth")
											{
												float num30;
												try
												{
													num30 = floatParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
												}
												catch (Exception ex64)
												{
													throw new InvalidValueException(string.Concat(new string[]
													{
														"Could not parse attribute \"",
														positionXmlElement.Name,
														"\" value \"",
														ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
														"\""
													}), positionXmlElement.LineNumber, ex64);
												}
												DataItem<float> dataItem66 = new DataItem<float>("GainHealth", num30);
												itemAction.pGainHealth = dataItem66;
											}
										}
									}
									else if (text2 == "SoundReload")
									{
										string text35;
										try
										{
											text35 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
										}
										catch (Exception ex65)
										{
											throw new InvalidValueException(string.Concat(new string[]
											{
												"Could not parse attribute \"",
												positionXmlElement.Name,
												"\" value \"",
												ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
												"\""
											}), positionXmlElement.LineNumber, ex65);
										}
										DataItem<string> dataItem67 = new DataItem<string>("SoundReload", text35);
										itemAction.pSoundReload = dataItem67;
									}
								}
								else if (text2 == "ZoomMaxIn")
								{
									float num31;
									try
									{
										num31 = floatParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
									}
									catch (Exception ex66)
									{
										throw new InvalidValueException(string.Concat(new string[]
										{
											"Could not parse attribute \"",
											positionXmlElement.Name,
											"\" value \"",
											ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
											"\""
										}), positionXmlElement.LineNumber, ex66);
									}
									DataItem<float> dataItem68 = new DataItem<float>("ZoomMaxIn", num31);
									itemAction.pZoomMaxIn = dataItem68;
								}
							}
							else if (num <= 3781194609U)
							{
								if (num != 3646476408U)
								{
									if (num != 3699885409U)
									{
										if (num == 3781194609U)
										{
											if (text2 == "ReferenceItem")
											{
												string text36;
												try
												{
													text36 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
												}
												catch (Exception ex67)
												{
													throw new InvalidValueException(string.Concat(new string[]
													{
														"Could not parse attribute \"",
														positionXmlElement.Name,
														"\" value \"",
														ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
														"\""
													}), positionXmlElement.LineNumber, ex67);
												}
												DataItem<string> dataItem69 = new DataItem<string>("ReferenceItem", text36);
												itemAction.pReferenceItem = dataItem69;
											}
										}
									}
									else if (text2 == "Consume")
									{
										bool flag4;
										try
										{
											flag4 = boolParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
										}
										catch (Exception ex68)
										{
											throw new InvalidValueException(string.Concat(new string[]
											{
												"Could not parse attribute \"",
												positionXmlElement.Name,
												"\" value \"",
												ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
												"\""
											}), positionXmlElement.LineNumber, ex68);
										}
										DataItem<bool> dataItem70 = new DataItem<bool>("Consume", flag4);
										itemAction.pConsume = dataItem70;
									}
								}
								else if (text2 == "RepairAmount")
								{
									int num32;
									try
									{
										num32 = intParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
									}
									catch (Exception ex69)
									{
										throw new InvalidValueException(string.Concat(new string[]
										{
											"Could not parse attribute \"",
											positionXmlElement.Name,
											"\" value \"",
											ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
											"\""
										}), positionXmlElement.LineNumber, ex69);
									}
									DataItem<int> dataItem71 = new DataItem<int>("RepairAmount", num32);
									itemAction.pRepairAmount = dataItem71;
								}
							}
							else if (num != 3788546643U)
							{
								if (num != 3860496195U)
								{
									if (num == 3880205310U)
									{
										if (text2 == "CreateItem")
										{
											string text37;
											try
											{
												text37 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
											}
											catch (Exception ex70)
											{
												throw new InvalidValueException(string.Concat(new string[]
												{
													"Could not parse attribute \"",
													positionXmlElement.Name,
													"\" value \"",
													ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
													"\""
												}), positionXmlElement.LineNumber, ex70);
											}
											DataItem<string> dataItem72 = new DataItem<string>("CreateItem", text37);
											itemAction.pCreateItem = dataItem72;
										}
									}
								}
								else if (text2 == "ChangeItemTo")
								{
									string text38;
									try
									{
										text38 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
									}
									catch (Exception ex71)
									{
										throw new InvalidValueException(string.Concat(new string[]
										{
											"Could not parse attribute \"",
											positionXmlElement.Name,
											"\" value \"",
											ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
											"\""
										}), positionXmlElement.LineNumber, ex71);
									}
									DataItem<string> dataItem73 = new DataItem<string>("ChangeItemTo", text38);
									itemAction.pChangeItemTo = dataItem73;
								}
							}
							else if (text2 == "AllowedUpgradeItems")
							{
								string text39;
								try
								{
									text39 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
								}
								catch (Exception ex72)
								{
									throw new InvalidValueException(string.Concat(new string[]
									{
										"Could not parse attribute \"",
										positionXmlElement.Name,
										"\" value \"",
										ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
										"\""
									}), positionXmlElement.LineNumber, ex72);
								}
								DataItem<string> dataItem74 = new DataItem<string>("AllowedUpgradeItems", text39);
								itemAction.pAllowedUpgradeItems = dataItem74;
							}
						}
						else if (num <= 4005067140U)
						{
							if (num <= 3894590787U)
							{
								if (num != 3893059105U)
								{
									if (num == 3894590787U)
									{
										if (text2 == "FlyTime")
										{
											float num33;
											try
											{
												num33 = floatParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
											}
											catch (Exception ex73)
											{
												throw new InvalidValueException(string.Concat(new string[]
												{
													"Could not parse attribute \"",
													positionXmlElement.Name,
													"\" value \"",
													ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
													"\""
												}), positionXmlElement.LineNumber, ex73);
											}
											DataItem<float> dataItem75 = new DataItem<float>("FlyTime", num33);
											itemAction.pFlyTime = dataItem75;
										}
									}
								}
								else if (text2 == "StaminaUsage")
								{
									string text40;
									try
									{
										text40 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
									}
									catch (Exception ex74)
									{
										throw new InvalidValueException(string.Concat(new string[]
										{
											"Could not parse attribute \"",
											positionXmlElement.Name,
											"\" value \"",
											ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
											"\""
										}), positionXmlElement.LineNumber, ex74);
									}
									DataItem<string> dataItem76 = new DataItem<string>("StaminaUsage", text40);
									itemAction.pStaminaUsage = dataItem76;
								}
							}
							else if (num != 3922517809U)
							{
								if (num != 3965826732U)
								{
									if (num == 4005067140U)
									{
										if (text2 == "MagazineItem")
										{
											string text41;
											try
											{
												text41 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
											}
											catch (Exception ex75)
											{
												throw new InvalidValueException(string.Concat(new string[]
												{
													"Could not parse attribute \"",
													positionXmlElement.Name,
													"\" value \"",
													ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
													"\""
												}), positionXmlElement.LineNumber, ex75);
											}
											DataItem<string> dataItem77 = new DataItem<string>("MagazineItem", text41);
											itemAction.pMagazineItem = dataItem77;
										}
									}
								}
								else if (text2 == "UpgradeActionSound")
								{
									string text42;
									try
									{
										text42 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
									}
									catch (Exception ex76)
									{
										throw new InvalidValueException(string.Concat(new string[]
										{
											"Could not parse attribute \"",
											positionXmlElement.Name,
											"\" value \"",
											ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
											"\""
										}), positionXmlElement.LineNumber, ex76);
									}
									DataItem<string> dataItem78 = new DataItem<string>("UpgradeActionSound", text42);
									itemAction.pUpgradeActionSound = dataItem78;
								}
							}
							else if (text2 == "DamageBlock")
							{
								float num34;
								try
								{
									num34 = floatParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
								}
								catch (Exception ex77)
								{
									throw new InvalidValueException(string.Concat(new string[]
									{
										"Could not parse attribute \"",
										positionXmlElement.Name,
										"\" value \"",
										ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
										"\""
									}), positionXmlElement.LineNumber, ex77);
								}
								DataItem<float> dataItem79 = new DataItem<float>("DamageBlock", num34);
								itemAction.pDamageBlock = dataItem79;
							}
						}
						else if (num <= 4124220918U)
						{
							if (num != 4013882141U)
							{
								if (num != 4069726011U)
								{
									if (num == 4124220918U)
									{
										if (text2 == "InstantiateOnLoad")
										{
											string text43;
											try
											{
												text43 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
											}
											catch (Exception ex78)
											{
												throw new InvalidValueException(string.Concat(new string[]
												{
													"Could not parse attribute \"",
													positionXmlElement.Name,
													"\" value \"",
													ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
													"\""
												}), positionXmlElement.LineNumber, ex78);
											}
											DataItem<string> dataItem80 = new DataItem<string>("InstantiateOnLoad", text43);
											itemAction.pInstantiateOnLoad = dataItem80;
										}
									}
								}
								else if (text2 == "UpgradeHitOffset")
								{
									int num35;
									try
									{
										num35 = intParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
									}
									catch (Exception ex79)
									{
										throw new InvalidValueException(string.Concat(new string[]
										{
											"Could not parse attribute \"",
											positionXmlElement.Name,
											"\" value \"",
											ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
											"\""
										}), positionXmlElement.LineNumber, ex79);
									}
									DataItem<int> dataItem81 = new DataItem<int>("UpgradeHitOffset", num35);
									itemAction.pUpgradeHitOffset = dataItem81;
								}
							}
							else if (text2 == "HordeMeterDistance")
							{
								float num36;
								try
								{
									num36 = floatParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
								}
								catch (Exception ex80)
								{
									throw new InvalidValueException(string.Concat(new string[]
									{
										"Could not parse attribute \"",
										positionXmlElement.Name,
										"\" value \"",
										ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
										"\""
									}), positionXmlElement.LineNumber, ex80);
								}
								DataItem<float> dataItem82 = new DataItem<float>("HordeMeterDistance", num36);
								itemAction.pHordeMeterDistance = dataItem82;
							}
						}
						else if (num != 4129641136U)
						{
							if (num != 4139408471U)
							{
								if (num == 4265352596U)
								{
									if (text2 == "BulletIcon")
									{
										string text44;
										try
										{
											text44 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
										}
										catch (Exception ex81)
										{
											throw new InvalidValueException(string.Concat(new string[]
											{
												"Could not parse attribute \"",
												positionXmlElement.Name,
												"\" value \"",
												ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
												"\""
											}), positionXmlElement.LineNumber, ex81);
										}
										DataItem<string> dataItem83 = new DataItem<string>("BulletIcon", text44);
										itemAction.pBulletIcon = dataItem83;
									}
								}
							}
							else if (text2 == "Debuff")
							{
								string text45;
								try
								{
									text45 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
								}
								catch (Exception ex82)
								{
									throw new InvalidValueException(string.Concat(new string[]
									{
										"Could not parse attribute \"",
										positionXmlElement.Name,
										"\" value \"",
										ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
										"\""
									}), positionXmlElement.LineNumber, ex82);
								}
								DataItem<string> dataItem84 = new DataItem<string>("Debuff", text45);
								itemAction.pDebuff = dataItem84;
							}
						}
						else if (text2 == "Adjacentblock")
						{
							string text46;
							try
							{
								text46 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
							}
							catch (Exception ex83)
							{
								throw new InvalidValueException(string.Concat(new string[]
								{
									"Could not parse attribute \"",
									positionXmlElement.Name,
									"\" value \"",
									ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
									"\""
								}), positionXmlElement.LineNumber, ex83);
							}
							DataItem<string> dataItem85 = new DataItem<string>("Adjacentblock", text46);
							itemAction.pAdjacentblock = dataItem85;
						}
						if (!dictionary.ContainsKey(positionXmlElement.Name))
						{
							dictionary[positionXmlElement.Name] = 0;
						}
						Dictionary<string, int> dictionary2 = dictionary;
						text2 = positionXmlElement.Name;
						int num37 = dictionary2[text2];
						dictionary2[text2] = num37 + 1;
					}
				}
				foreach (KeyValuePair<string, Range<int>> keyValuePair in ItemActionData.Parser.knownAttributesMultiplicity)
				{
					int num38 = (dictionary.ContainsKey(keyValuePair.Key) ? dictionary[keyValuePair.Key] : 0);
					if ((keyValuePair.Value.hasMin && num38 < keyValuePair.Value.min) || (keyValuePair.Value.hasMax && num38 > keyValuePair.Value.max))
					{
						throw new IncorrectAttributeOccurrenceException(string.Concat(new string[]
						{
							"Element has incorrect number of \"",
							keyValuePair.Key,
							"\" attribute instances, found ",
							num38.ToString(),
							", expected ",
							keyValuePair.Value.ToString()
						}), _elem.LineNumber);
					}
				}
				return itemAction;
			}

			[PublicizedFrom(EAccessModifier.Private)]
			public static Dictionary<string, Range<int>> knownAttributesMultiplicity = new Dictionary<string, Range<int>>
			{
				{
					"Delay",
					new Range<int>(true, 0, true, 1)
				},
				{
					"Range",
					new Range<int>(true, 0, true, 1)
				},
				{
					"SoundStart",
					new Range<int>(true, 0, true, 1)
				},
				{
					"SoundRepeat",
					new Range<int>(true, 0, true, 1)
				},
				{
					"SoundEnd",
					new Range<int>(true, 0, true, 1)
				},
				{
					"SoundEmpty",
					new Range<int>(true, 0, true, 1)
				},
				{
					"SoundReload",
					new Range<int>(true, 0, true, 1)
				},
				{
					"SoundWarning",
					new Range<int>(true, 0, true, 1)
				},
				{
					"StaminaUsage",
					new Range<int>(true, 0, true, 1)
				},
				{
					"UseTime",
					new Range<int>(true, 0, true, 1)
				},
				{
					"FocusedBlockname1",
					new Range<int>(true, 0, true, 1)
				},
				{
					"FocusedBlockname2",
					new Range<int>(true, 0, true, 1)
				},
				{
					"FocusedBlockname3",
					new Range<int>(true, 0, true, 1)
				},
				{
					"FocusedBlockname4",
					new Range<int>(true, 0, true, 1)
				},
				{
					"FocusedBlockname5",
					new Range<int>(true, 0, true, 1)
				},
				{
					"FocusedBlockname6",
					new Range<int>(true, 0, true, 1)
				},
				{
					"FocusedBlockname7",
					new Range<int>(true, 0, true, 1)
				},
				{
					"FocusedBlockname8",
					new Range<int>(true, 0, true, 1)
				},
				{
					"FocusedBlockname9",
					new Range<int>(true, 0, true, 1)
				},
				{
					"ChangeItemTo",
					new Range<int>(true, 0, true, 1)
				},
				{
					"ChangeBlockTo",
					new Range<int>(true, 0, true, 1)
				},
				{
					"DoBlockAction",
					new Range<int>(true, 0, true, 1)
				},
				{
					"GainHealth",
					new Range<int>(true, 0, true, 1)
				},
				{
					"GainFood",
					new Range<int>(true, 0, true, 1)
				},
				{
					"GainWater",
					new Range<int>(true, 0, true, 1)
				},
				{
					"GainStamina",
					new Range<int>(true, 0, true, 1)
				},
				{
					"GainSickness",
					new Range<int>(true, 0, true, 1)
				},
				{
					"GainWellness",
					new Range<int>(true, 0, true, 1)
				},
				{
					"Buff",
					new Range<int>(true, 0, true, 1)
				},
				{
					"BuffChance",
					new Range<int>(true, 0, true, 1)
				},
				{
					"Debuff",
					new Range<int>(true, 0, true, 1)
				},
				{
					"CreateItem",
					new Range<int>(true, 0, true, 1)
				},
				{
					"ConditionRaycastBlock",
					new Range<int>(true, 0, true, 1)
				},
				{
					"GainGas",
					new Range<int>(true, 0, true, 1)
				},
				{
					"Consume",
					new Range<int>(true, 0, true, 1)
				},
				{
					"Blockname",
					new Range<int>(true, 0, true, 1)
				},
				{
					"ThrowStrengthDefault",
					new Range<int>(true, 0, true, 1)
				},
				{
					"ThrowStrengthMax",
					new Range<int>(true, 0, true, 1)
				},
				{
					"MaxStrainTime",
					new Range<int>(true, 0, true, 1)
				},
				{
					"MagazineSize",
					new Range<int>(true, 0, true, 1)
				},
				{
					"MagazineItem",
					new Range<int>(true, 0, true, 1)
				},
				{
					"ReloadTime",
					new Range<int>(true, 0, true, 1)
				},
				{
					"BulletIcon",
					new Range<int>(true, 0, true, 1)
				},
				{
					"RaysPerShot",
					new Range<int>(true, 0, true, 1)
				},
				{
					"RaysSpread",
					new Range<int>(true, 0, true, 1)
				},
				{
					"Sphere",
					new Range<int>(true, 0, true, 1)
				},
				{
					"CrosshairMinDistance",
					new Range<int>(true, 0, true, 1)
				},
				{
					"CrosshairMaxDistance",
					new Range<int>(true, 0, true, 1)
				},
				{
					"DamageEntity",
					new Range<int>(true, 0, true, 1)
				},
				{
					"DamageBlock",
					new Range<int>(true, 0, true, 1)
				},
				{
					"ParticlesMuzzleFire",
					new Range<int>(true, 0, true, 1)
				},
				{
					"ParticlesMuzzleSmoke",
					new Range<int>(true, 0, true, 1)
				},
				{
					"BlockRange",
					new Range<int>(true, 0, true, 1)
				},
				{
					"AutoFire",
					new Range<int>(true, 0, true, 1)
				},
				{
					"HordeMeterRate",
					new Range<int>(true, 0, true, 1)
				},
				{
					"HordeMeterDistance",
					new Range<int>(true, 0, true, 1)
				},
				{
					"HitmaskOverride",
					new Range<int>(true, 0, true, 1)
				},
				{
					"SingleMagazineUsage",
					new Range<int>(true, 0, true, 1)
				},
				{
					"BulletMaterial",
					new Range<int>(true, 0, true, 1)
				},
				{
					"InfiniteAmmo",
					new Range<int>(true, 0, true, 1)
				},
				{
					"ZoomMaxOut",
					new Range<int>(true, 0, true, 1)
				},
				{
					"ZoomMaxIn",
					new Range<int>(true, 0, true, 1)
				},
				{
					"ZoomOverlay",
					new Range<int>(true, 0, true, 1)
				},
				{
					"Velocity",
					new Range<int>(true, 0, true, 1)
				},
				{
					"FlyTime",
					new Range<int>(true, 0, true, 1)
				},
				{
					"LifeTime",
					new Range<int>(true, 0, true, 1)
				},
				{
					"CollisionRadius",
					new Range<int>(true, 0, true, 1)
				},
				{
					"ProjectileInitialVelocity",
					new Range<int>(true, 0, true, 1)
				},
				{
					"Fertileblock",
					new Range<int>(true, 0, true, 1)
				},
				{
					"Adjacentblock",
					new Range<int>(true, 0, true, 1)
				},
				{
					"RepairAmount",
					new Range<int>(true, 0, true, 1)
				},
				{
					"UpgradeHitOffset",
					new Range<int>(true, 0, true, 1)
				},
				{
					"AllowedUpgradeItems",
					new Range<int>(true, 0, true, 1)
				},
				{
					"RestrictedUpgradeItems",
					new Range<int>(true, 0, true, 1)
				},
				{
					"UpgradeActionSound",
					new Range<int>(true, 0, true, 1)
				},
				{
					"RepairActionSound",
					new Range<int>(true, 0, true, 1)
				},
				{
					"ReferenceItem",
					new Range<int>(true, 0, true, 1)
				},
				{
					"Mesh",
					new Range<int>(true, 0, true, 1)
				},
				{
					"ActionIdx",
					new Range<int>(true, 0, true, 1)
				},
				{
					"Title",
					new Range<int>(true, 0, true, 1)
				},
				{
					"Description",
					new Range<int>(true, 0, true, 1)
				},
				{
					"RecipesToLearn",
					new Range<int>(true, 0, true, 1)
				},
				{
					"InstantiateOnLoad",
					new Range<int>(true, 0, true, 1)
				},
				{
					"SoundDraw",
					new Range<int>(true, 0, true, 1)
				},
				{
					"DamageBonus",
					new Range<int>(true, 0, true, 1)
				},
				{
					"Explosion",
					new Range<int>(true, 0, true, 1)
				}
			};
		}
	}
}
