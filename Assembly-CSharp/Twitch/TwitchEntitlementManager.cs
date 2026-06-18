using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Platform;
using UnityEngine.Networking;

namespace Twitch
{
	public class TwitchEntitlementManager : IEntitlementValidator
	{
		public string PlatformID
		{
			[PublicizedFrom(EAccessModifier.Private)]
			get
			{
				return this.prefix + this.owner.User.PlatformUserId.ReadablePlatformUserIdentifier;
			}
		}

		public void Init(IPlatform _owner)
		{
			this.owner = _owner;
			if (!TwitchEntitlementManager.platformIdentifierToPrefix.TryGetValue(_owner.PlatformIdentifier, out this.prefix))
			{
				Log.Warning("could not get platform prefix in Twitch Entitlements Manager");
				return;
			}
		}

		public void Init()
		{
			TwitchDropAvailabilityManager.Instance.Updated -= this.OnDropsUpdated;
			TwitchDropAvailabilityManager.Instance.Updated += this.OnDropsUpdated;
			TwitchDropAvailabilityManager.Instance.RegisterSource("rfs://drops.xml");
			TwitchDropAvailabilityManager.Instance.UpdateAll(true);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void OnDropsUpdated(TwitchDropAvailabilityManager mgr)
		{
			List<TwitchDropAvailabilityManager.TwitchDropEntry> list = new List<TwitchDropAvailabilityManager.TwitchDropEntry>();
			mgr.GetEntries(new List<string> { "rfs://drops.xml" }, list);
			TwitchEntitlementManager.EntitlementSetToTwitchMap.Clear();
			foreach (TwitchDropAvailabilityManager.TwitchDropEntry twitchDropEntry in list)
			{
				if (twitchDropEntry.IsAvailable(DateTime.Now))
				{
					TwitchEntitlementManager.EntitlementSetToTwitchMap.Add(twitchDropEntry.EntitlementSet, twitchDropEntry.BenefitId);
				}
			}
			this.FetchEntitlements();
		}

		public void FetchEntitlements()
		{
			ThreadManager.StartCoroutine(this.FetchEntitlementsCo());
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public IEnumerator FetchEntitlementsCo()
		{
			UnityWebRequest request = UnityWebRequest.Get("https://xjjvn6hovg33dqetux65clszte0vhysi.lambda-url.us-east-2.on.aws/?platform_id=" + this.PlatformID);
			Log.Out("fetching Twitch entitlements for " + this.PlatformID);
			yield return request.SendWebRequest();
			if (request.result != 1)
			{
				Log.Warning("Failed to fetch Twitch entitlements: " + request.error);
				yield break;
			}
			JArray jarray = (JArray)JObject.Parse(request.downloadHandler.text)["data"];
			TwitchEntitlementManager.entitlements.Clear();
			foreach (JToken jtoken in jarray)
			{
				string text = jtoken.Value<string>("id");
				string text2 = jtoken.Value<string>("benefit_id");
				string text3 = jtoken.Value<string>("fulfillment_status");
				TwitchEntitlementManager.entitlements.Add(new Entitlement
				{
					id = text,
					benefit_id = text2,
					fulfillment_status = text3
				});
			}
			yield return this.FulfillEntitlementsCo(null);
			Log.Out("Successfully fetched Twitch entitlements");
			yield break;
		}

		public void FulfillEntitlements(Action onSuccess = null)
		{
			GameManager.Instance.StartCoroutine(this.FulfillEntitlementsCo(onSuccess));
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public IEnumerator FulfillEntitlementsCo(Action onSuccess)
		{
			List<string> list = new List<string>();
			foreach (Entitlement entitlement in TwitchEntitlementManager.entitlements)
			{
				if (entitlement.fulfillment_status == "CLAIMED")
				{
					list.Add(entitlement.id);
				}
			}
			if (list.Count == 0)
			{
				yield break;
			}
			string text = JsonConvert.SerializeObject(new FulfillmentPayload
			{
				platform_id = this.PlatformID,
				entitlement_ids = list
			});
			byte[] bytes = Encoding.UTF8.GetBytes(text);
			UnityWebRequest request = new UnityWebRequest("https://ev2dltb7u2pdtwuaphayq5icdy0nmtsx.lambda-url.us-east-2.on.aws/?platform_id=" + this.PlatformID, "POST")
			{
				uploadHandler = new UploadHandlerRaw(bytes),
				downloadHandler = new DownloadHandlerBuffer()
			};
			request.SetRequestHeader("Content-Type", "application/json");
			Log.Out("fulfilling Twitch entitlements for " + this.PlatformID);
			yield return request.SendWebRequest();
			if (request.result != 1)
			{
				Log.Warning("Failed to fulfill twitch entitlements: " + request.downloadHandler.text);
			}
			else
			{
				foreach (JToken jtoken in ((JArray)JObject.Parse(request.downloadHandler.text)["data"]))
				{
					string id = jtoken.Value<string>("id");
					string text2 = jtoken.Value<string>("benefit_id");
					string text3 = jtoken.Value<string>("fulfillment_status");
					Entitlement entitlement2 = TwitchEntitlementManager.entitlements.Find((Entitlement e) => e.id == id);
					if (entitlement2 != null)
					{
						entitlement2.fulfillment_status = text3;
						entitlement2.benefit_id = text2;
					}
					else
					{
						TwitchEntitlementManager.entitlements.Add(new Entitlement
						{
							id = id,
							benefit_id = text2,
							fulfillment_status = text3
						});
					}
				}
				if (onSuccess != null)
				{
					onSuccess();
				}
			}
			yield break;
		}

		public void SerializeEntitlements()
		{
			JsonConvert.SerializeObject(new EntitlementListWrapper
			{
				entitlements = TwitchEntitlementManager.entitlements
			}, 1);
		}

		public bool IsAvailableOnPlatform(EntitlementSetEnum _set)
		{
			return TwitchEntitlementManager.EntitlementSetToTwitchMap.ContainsKey(_set);
		}

		public bool HasEntitlement(EntitlementSetEnum _set)
		{
			string id;
			return TwitchEntitlementManager.EntitlementSetToTwitchMap.TryGetValue(_set, out id) && TwitchEntitlementManager.entitlements.Exists((Entitlement e) => e.benefit_id == id);
		}

		public bool IsEntitlementPurchasable(EntitlementSetEnum _set)
		{
			return false;
		}

		public bool OpenStore(EntitlementSetEnum _set, Action<EntitlementSetEnum> _onDlcPurchased)
		{
			return false;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public const string FETCH_URL = "https://xjjvn6hovg33dqetux65clszte0vhysi.lambda-url.us-east-2.on.aws/?platform_id=";

		[PublicizedFrom(EAccessModifier.Private)]
		public const string FULFILLMENT_URL = "https://ev2dltb7u2pdtwuaphayq5icdy0nmtsx.lambda-url.us-east-2.on.aws/?platform_id=";

		[PublicizedFrom(EAccessModifier.Private)]
		public static EnumDictionary<EntitlementSetEnum, string> EntitlementSetToTwitchMap = new EnumDictionary<EntitlementSetEnum, string>();

		[PublicizedFrom(EAccessModifier.Private)]
		public static EnumDictionary<EPlatformIdentifier, string> platformIdentifierToPrefix = new EnumDictionary<EPlatformIdentifier, string>
		{
			{
				EPlatformIdentifier.PSN,
				"p"
			},
			{
				EPlatformIdentifier.XBL,
				"x"
			},
			{
				EPlatformIdentifier.Steam,
				"s"
			}
		};

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly List<Entitlement> entitlements = new List<Entitlement>();

		[PublicizedFrom(EAccessModifier.Private)]
		public IPlatform owner;

		[PublicizedFrom(EAccessModifier.Private)]
		public string prefix;
	}
}
