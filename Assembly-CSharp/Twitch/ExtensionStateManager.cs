using System;
using System.Collections;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace Twitch
{
	public class ExtensionStateManager
	{
		public void Init()
		{
			this.userId = TwitchManager.Current.Authentication.userID;
			this.gettingJWT = true;
			GameManager.Instance.StartCoroutine(this.GetJWT(TwitchManager.Current.Authentication.oauth.Substring(6)));
			this.ecm = new ExtensionConfigManager();
			this.ecm.Init();
			this.epm = new ExtensionPubSubManager();
		}

		public void OnPartyChanged()
		{
			ExtensionConfigManager extensionConfigManager = this.ecm;
			if (extensionConfigManager == null)
			{
				return;
			}
			extensionConfigManager.OnPartyChanged();
		}

		public void PushUserBalance(ValueTuple<string, int> userBalance)
		{
			ExtensionPubSubManager extensionPubSubManager = this.epm;
			if (extensionPubSubManager == null)
			{
				return;
			}
			extensionPubSubManager.PushUserBalance(userBalance);
		}

		public void PushViewerChatState(string id, bool hasChatted)
		{
			ExtensionPubSubManager extensionPubSubManager = this.epm;
			if (extensionPubSubManager == null)
			{
				return;
			}
			extensionPubSubManager.PushViewerChatState(id, hasChatted);
		}

		public bool CanUseBitCommands()
		{
			return this.ecm.CanUseBitCommands();
		}

		public void Update()
		{
			if (DateTimeOffset.UtcNow.ToUnixTimeSeconds() > this.jwtRefreshTime && !this.gettingJWT)
			{
				this.gettingJWT = true;
				GameManager.Instance.StartCoroutine(this.GetJWT(TwitchManager.Current.Authentication.oauth.Substring(6)));
			}
			if (this.jwt != string.Empty && Time.realtimeSinceStartup - this.lastUpdate >= 1f)
			{
				this.epm.Update(this.ecm.UpdatedConfig());
				this.lastUpdate = Time.realtimeSinceStartup;
			}
		}

		public void RetrieveJWT()
		{
			GameManager.Instance.StartCoroutine(this.GetJWT(TwitchManager.Current.Authentication.oauth.Substring(6)));
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public IEnumerator GetJWT(string token)
		{
			using (UnityWebRequest req = UnityWebRequest.Get("https://2v3d0ewjcg.execute-api.us-east-1.amazonaws.com/prod/jwt/broadcaster"))
			{
				req.SetRequestHeader("Authorization", this.userId + " " + token);
				yield return req.SendWebRequest();
				if (req.result != 1)
				{
					Log.Warning(string.Format("Could not retrieve JWT: {0}", req.result));
				}
				else
				{
					try
					{
						JObject jobject = JObject.Parse(req.downloadHandler.text);
						if (jobject != null)
						{
							JToken jtoken;
							if (jobject.TryGetValue("token", ref jtoken))
							{
								this.jwt = jtoken.ToString();
								this.epm.SetJWT(this.jwt);
								Log.Out("received jwt");
							}
							else
							{
								Log.Warning("Could not parse JWT in message body");
							}
							JToken jtoken2;
							if (jobject.TryGetValue("refreshTime", ref jtoken2))
							{
								this.jwtRefreshTime = long.Parse(jtoken2.ToString());
								Log.Out(string.Format("will refresh jwt at {0}", this.jwtRefreshTime));
							}
						}
					}
					catch (Exception ex)
					{
						Log.Warning(ex.Message);
					}
				}
			}
			UnityWebRequest req = null;
			this.gettingJWT = false;
			yield break;
			yield break;
		}

		public void Cleanup()
		{
			this.ecm.Cleanup();
			this.ecm = null;
			this.epm = null;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public string userId;

		[PublicizedFrom(EAccessModifier.Private)]
		public float lastUpdate = Time.realtimeSinceStartup;

		[PublicizedFrom(EAccessModifier.Private)]
		public ExtensionConfigManager ecm;

		[PublicizedFrom(EAccessModifier.Private)]
		public ExtensionPubSubManager epm;

		[PublicizedFrom(EAccessModifier.Private)]
		public string jwt = string.Empty;

		[PublicizedFrom(EAccessModifier.Private)]
		public long jwtRefreshTime;

		[PublicizedFrom(EAccessModifier.Private)]
		public bool gettingJWT;
	}
}
