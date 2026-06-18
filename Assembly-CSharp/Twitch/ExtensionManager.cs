using System;
using System.Collections;
using Newtonsoft.Json.Linq;
using UnityEngine.Networking;

namespace Twitch
{
	public class ExtensionManager
	{
		public void Init()
		{
			this.extensionStateManager = new ExtensionStateManager();
			this.extensionCommandPoller = new ExtensionCommandPoller();
			this.extensionStateManager.Init();
			this.extensionCommandPoller.Init();
		}

		public void OnPartyChanged()
		{
			ExtensionStateManager extensionStateManager = this.extensionStateManager;
			if (extensionStateManager == null)
			{
				return;
			}
			extensionStateManager.OnPartyChanged();
		}

		public void TwitchEnabledChanged(EntityPlayer _ep)
		{
			EntityPlayerLocal localPlayer = TwitchManager.Current.LocalPlayer;
			if (_ep != localPlayer && localPlayer.Party != null && localPlayer.Party.ContainsMember(_ep))
			{
				this.extensionStateManager.OnPartyChanged();
			}
		}

		public void PushUserBalance(ValueTuple<string, int> userBalance)
		{
			ExtensionStateManager extensionStateManager = this.extensionStateManager;
			if (extensionStateManager == null)
			{
				return;
			}
			extensionStateManager.PushUserBalance(userBalance);
		}

		public void PushViewerChatState(string id, bool hasChatted)
		{
			ExtensionStateManager extensionStateManager = this.extensionStateManager;
			if (extensionStateManager == null)
			{
				return;
			}
			extensionStateManager.PushViewerChatState(id, hasChatted);
		}

		public bool CanUseBitCommands()
		{
			return this.extensionStateManager.CanUseBitCommands();
		}

		public void Update()
		{
			this.extensionStateManager.Update();
			this.extensionCommandPoller.Update();
		}

		public bool HasCommand()
		{
			return this.extensionCommandPoller.HasCommand();
		}

		public ExtensionAction GetCommand()
		{
			return this.extensionCommandPoller.GetCommand();
		}

		public void RetrieveJWT()
		{
			this.extensionStateManager.RetrieveJWT();
		}

		public void Cleanup()
		{
			this.extensionCommandPoller.Cleanup();
			this.extensionStateManager.Cleanup();
			this.extensionCommandPoller = null;
			this.extensionStateManager = null;
		}

		public static void CheckExtensionInstalled(Action<bool> _cb)
		{
			GameManager.Instance.StartCoroutine(ExtensionManager.CheckExtensionInstall(_cb));
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static IEnumerator CheckExtensionInstall(Action<bool> _cb)
		{
			using (UnityWebRequest req = UnityWebRequest.Get("https://api.twitch.tv/helix/users/extensions?user_id=" + TwitchManager.Current.Authentication.userID))
			{
				req.SetRequestHeader("Authorization", "Bearer " + TwitchManager.Current.Authentication.oauth.Substring(6));
				req.SetRequestHeader("Client-Id", TwitchAuthentication.client_id);
				yield return req.SendWebRequest();
				if (req.result != 1)
				{
					Log.Warning("InBeta Check Failed: " + req.downloadHandler.text);
				}
				else
				{
					try
					{
						JObject jobject = JObject.Parse(req.downloadHandler.text);
						foreach (JToken jtoken in Extensions.Values(jobject["data"]["panel"].ToObject<JObject>()))
						{
							JObject jobject2 = jtoken.ToObject<JObject>();
							JToken jtoken2;
							if (jobject2.TryGetValue("id", ref jtoken2) && jtoken2.ToString() == "k6ji189bf7i4ge8il4iczzw7kpgmjt" && jobject2["active"].ToString() == bool.TrueString)
							{
								_cb(true);
								yield break;
							}
						}
						foreach (JToken jtoken3 in Extensions.Values(jobject["data"]["overlay"].ToObject<JObject>()))
						{
							JObject jobject3 = jtoken3.ToObject<JObject>();
							JToken jtoken4;
							if (jobject3.TryGetValue("version", ref jtoken4))
							{
								ExtensionManager.Version = jtoken4.ToString();
							}
							JToken jtoken5;
							if (jobject3.TryGetValue("id", ref jtoken5) && jtoken5.ToString() == "k6ji189bf7i4ge8il4iczzw7kpgmjt" && jobject3["active"].ToString() == bool.TrueString)
							{
								_cb(true);
								yield break;
							}
						}
					}
					catch (Exception)
					{
						Log.Warning("could not read extension check data");
					}
				}
			}
			UnityWebRequest req = null;
			_cb(false);
			yield break;
			yield break;
		}

		public const string API_STAGE = "prod";

		public const string EXTENSION_ID = "k6ji189bf7i4ge8il4iczzw7kpgmjt";

		public static string Version = "2.0.2";

		[PublicizedFrom(EAccessModifier.Private)]
		public ExtensionStateManager extensionStateManager;

		[PublicizedFrom(EAccessModifier.Private)]
		public ExtensionCommandPoller extensionCommandPoller;
	}
}
