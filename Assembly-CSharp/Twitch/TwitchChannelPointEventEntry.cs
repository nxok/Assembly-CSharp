using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

namespace Twitch
{
	public class TwitchChannelPointEventEntry : BaseTwitchEventEntry
	{
		public override bool IsValid(int amount = -1, string name = "", TwitchSubEventEntry.SubTierTypes subTier = TwitchSubEventEntry.SubTierTypes.Any)
		{
			return this.ChannelPointTitle == name;
		}

		public TwitchChannelPointEventEntry.CreateCustomReward SetupRewardEntry(string channelID)
		{
			return new TwitchChannelPointEventEntry.CreateCustomReward
			{
				broadcaster_id = channelID,
				title = this.ChannelPointTitle,
				cost = this.Cost,
				is_max_per_user_per_stream_enabled = (this.MaxPerUserPerStream > 0),
				max_per_user_per_stream = this.MaxPerUserPerStream,
				is_max_per_stream_enabled = (this.MaxPerStream > 0),
				max_per_stream = this.MaxPerStream,
				is_global_cooldown_enabled = (this.GlobalCooldown > 0),
				global_cooldown_seconds = this.GlobalCooldown
			};
		}

		public static IEnumerator CreateCustomRewardPost(TwitchChannelPointEventEntry.CreateCustomReward _rd, Action<string> _onSucess, Action<string> _onFail)
		{
			yield return new WaitUntil(() => TwitchManager.Current.Authentication != null && TwitchManager.Current.Authentication.oauth != "" && TwitchManager.Current.Authentication.userID != "");
			Log.Out("creating Custom reward on: " + TwitchManager.Current.Authentication.userID);
			string text = "https://api.twitch.tv/helix/channel_points/custom_rewards?broadcaster_id=" + TwitchManager.Current.Authentication.userID;
			string text2 = JsonUtility.ToJson(_rd);
			using (UnityWebRequest req = UnityWebRequest.Put(text, text2))
			{
				req.method = "POST";
				req.SetRequestHeader("Authorization", "Bearer " + TwitchManager.Current.Authentication.oauth.Substring(6));
				req.SetRequestHeader("Client-Id", TwitchAuthentication.client_id);
				req.SetRequestHeader("Content-Type", "application/json");
				yield return req.SendWebRequest();
				if (req.result == 1)
				{
					Log.Out("sucessfully created Custom Channel Points Reward");
					_onSucess(req.downloadHandler.text);
				}
				else
				{
					TwitchChannelPointEventEntry.ErrorResponse errorResponse = JsonConvert.DeserializeObject<TwitchChannelPointEventEntry.ErrorResponse>(req.downloadHandler.text);
					if (errorResponse != null)
					{
						_onFail("response code: " + errorResponse.status + "\nmessage: " + errorResponse.message);
					}
					else
					{
						_onFail("Something went wrong. Please Try again.");
					}
				}
			}
			UnityWebRequest req = null;
			yield break;
			yield break;
		}

		public static IEnumerator DeleteCustomRewardsDelete(string id, Action<string> _onSucess, Action<string> _onFail)
		{
			string text = string.Format("https://api.twitch.tv/helix/channel_points/custom_rewards?broadcaster_id={0}&id={1}", TwitchManager.Current.Authentication.userID, id);
			using (UnityWebRequest req = UnityWebRequest.Delete(text))
			{
				req.method = "DELETE";
				req.SetRequestHeader("Authorization", "Bearer " + TwitchManager.Current.Authentication.oauth.Substring(6));
				req.SetRequestHeader("Client-Id", TwitchAuthentication.client_id);
				yield return req.SendWebRequest();
				if (req.result == 1)
				{
					_onSucess("Success");
				}
				else
				{
					Debug.Log(string.Format("response code: {0}", req.responseCode));
					if (req.responseCode == 404L)
					{
						_onSucess("Not Found");
					}
					else
					{
						TwitchChannelPointEventEntry.ErrorResponse errorResponse = JsonConvert.DeserializeObject<TwitchChannelPointEventEntry.ErrorResponse>(req.downloadHandler.text);
						if (errorResponse != null)
						{
							_onFail(errorResponse.message);
						}
						else
						{
							_onFail("Something went wrong. Please Try again.");
						}
					}
				}
			}
			UnityWebRequest req = null;
			yield break;
			yield break;
		}

		public string ChannelPointTitle = "";

		public int Cost = 1000;

		public int MaxPerUserPerStream;

		public int MaxPerStream;

		public int GlobalCooldown;

		public string ChannelPointID = "";

		public bool AutoCreate = true;

		[Serializable]
		public class CreateCustomRewards
		{
			public List<TwitchChannelPointEventEntry.CreateCustomReward> data = new List<TwitchChannelPointEventEntry.CreateCustomReward>();
		}

		[Serializable]
		public class CreateCustomReward
		{
			public string broadcaster_id;

			public string title;

			public string background_color = "#F13030";

			public int cost;

			public int max_per_user_per_stream;

			public bool is_max_per_user_per_stream_enabled;

			public int max_per_stream;

			public bool is_max_per_stream_enabled;

			public int global_cooldown_seconds;

			public bool is_global_cooldown_enabled;
		}

		[Serializable]
		public class CreateCustomRewardResponses
		{
			public List<TwitchChannelPointEventEntry.CreateCustomRewardResponse> data;
		}

		[Serializable]
		public class CreateCustomRewardResponse
		{
			public string id;

			public string title;
		}

		[Serializable]
		public class ErrorResponse
		{
			public string status;

			public string message;
		}
	}
}
