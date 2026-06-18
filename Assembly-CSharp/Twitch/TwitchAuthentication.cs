using System;
using System.Collections;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;
using ZXing;
using ZXing.QrCode;

namespace Twitch
{
	public class TwitchAuthentication
	{
		public static event TwitchAuth_QRCodeGenerated QRCodeGenerated;

		public TwitchAuthentication()
		{
			this.TwitchListener = new HttpListener();
			this.TwitchListener.Prefixes.Add("http://localhost:56207/");
			this.TwitchListener.Prefixes.Add("http://localhost:56207/auth/");
		}

		public Task<TwitchAuthentication.AuthenticationValues> GetAuthenticationValuesAsync()
		{
			return Task.Run<TwitchAuthentication.AuthenticationValues>(() => this.GetAuthenticationValues());
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public bool StartListener()
		{
			this.completed = false;
			try
			{
				this.TwitchListener.Start();
			}
			catch (HttpListenerException)
			{
			}
			return this.TwitchListener.IsListening;
		}

		public void StopListener()
		{
			this.TwitchListener.Stop();
		}

		public TwitchAuthentication.AuthenticationValues GetAuthenticationValues()
		{
			while (this.TwitchListener.IsListening)
			{
				HttpListenerContext httpListenerContext = null;
				try
				{
					httpListenerContext = this.TwitchListener.GetContext();
				}
				catch (HttpListenerException)
				{
					this.StopListener();
					Log.Warning("Could not get context on TwitchListener");
					return null;
				}
				if (!(httpListenerContext.Request.HttpMethod == "GET") || !(httpListenerContext.Request.Url.LocalPath == "/auth/"))
				{
					byte[] bytes = Encoding.UTF8.GetBytes(this.GetResponse());
					httpListenerContext.Response.StatusCode = 200;
					httpListenerContext.Response.KeepAlive = false;
					httpListenerContext.Response.ContentLength64 = (long)bytes.Length;
					httpListenerContext.Response.OutputStream.Write(bytes, 0, bytes.Length);
					httpListenerContext.Response.Close();
					continue;
				}
				string text = httpListenerContext.Request.QueryString.Get("scope");
				string text2 = httpListenerContext.Request.QueryString.Get("access_token");
				if (text == null || text2 == null)
				{
					httpListenerContext.Response.ContentType = "application/json";
					httpListenerContext.Response.StatusCode = 400;
					string text3 = JsonConvert.SerializeObject(new
					{
						message = "missing data"
					});
					byte[] bytes2 = Encoding.UTF8.GetBytes(text3);
					httpListenerContext.Response.ContentLength64 = (long)bytes2.Length;
					Stream outputStream = httpListenerContext.Response.OutputStream;
					outputStream.Write(bytes2, 0, bytes2.Length);
					outputStream.Close();
					continue;
				}
				TwitchAuthentication.AuthenticationValues model = this.GetModel(text2, text);
				httpListenerContext.Response.ContentType = "application/json";
				httpListenerContext.Response.StatusCode = 200;
				string text4 = JsonConvert.SerializeObject(new
				{
					message = "success"
				});
				byte[] bytes3 = Encoding.UTF8.GetBytes(text4);
				httpListenerContext.Response.ContentLength64 = (long)bytes3.Length;
				Stream outputStream2 = httpListenerContext.Response.OutputStream;
				outputStream2.Write(bytes3, 0, bytes3.Length);
				outputStream2.Close();
				this.StopListener();
				if (model != null)
				{
					return model;
				}
				Log.Warning("did not successfully get authentication values");
				return null;
			}
			return null;
		}

		public static NameValueCollection ParseQueryString(string s)
		{
			NameValueCollection nameValueCollection = new NameValueCollection();
			if (s.Contains("?"))
			{
				s = s.Substring(s.IndexOf('?') + 2);
			}
			string[] array = Regex.Split(s, "&");
			for (int i = 0; i < array.Length; i++)
			{
				string[] array2 = Regex.Split(array[i], "=");
				if (array2.Length == 2)
				{
					nameValueCollection.Add(array2[0], array2[1]);
				}
				else
				{
					nameValueCollection.Add(array2[0], string.Empty);
				}
			}
			return nameValueCollection;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public string GetResponse()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("<head>");
			stringBuilder.Append(Environment.NewLine);
			stringBuilder.Append("<title>7 Days to Die Twitch Oauth</title>");
			stringBuilder.Append(Environment.NewLine);
			stringBuilder.Append("</head>");
			stringBuilder.Append(Environment.NewLine);
			stringBuilder.Append("<body>");
			stringBuilder.Append(Environment.NewLine);
			stringBuilder.Append("<h1 id=\"status\">Authenticating...</h1>");
			stringBuilder.Append(Environment.NewLine);
			stringBuilder.Append("</body>");
			stringBuilder.Append(Environment.NewLine);
			stringBuilder.Append("<script language=\"JavaScript\">");
			stringBuilder.Append(Environment.NewLine);
			stringBuilder.Append("if(window.location.hash) {");
			stringBuilder.Append(Environment.NewLine);
			stringBuilder.Append("window.location.href = window.location.href.replace(\"/#\",\"?\");");
			stringBuilder.Append(Environment.NewLine);
			stringBuilder.Append("}");
			stringBuilder.Append(Environment.NewLine);
			stringBuilder.Append("const urlParams = new URLSearchParams(window.location.search);");
			stringBuilder.Append(Environment.NewLine);
			stringBuilder.Append("const accessToken = urlParams.get('access_token');");
			stringBuilder.Append(Environment.NewLine);
			stringBuilder.Append("const scope = urlParams.get('scope');");
			stringBuilder.Append(Environment.NewLine);
			stringBuilder.Append("if (accessToken && scope) {");
			stringBuilder.Append(Environment.NewLine);
			stringBuilder.Append("const url = `http://localhost:56207/auth/?access_token=${accessToken}&scope=${scope}`;");
			stringBuilder.Append(Environment.NewLine);
			stringBuilder.Append("let attemptCount = 0;");
			stringBuilder.Append(Environment.NewLine);
			stringBuilder.Append("const sendAuthValues = () => fetch(url, { method: 'GET' })");
			stringBuilder.Append(Environment.NewLine);
			stringBuilder.Append(".then(response => { if (response.ok) return response.json(); throw new Error('Network response was not ok.'); })");
			stringBuilder.Append(Environment.NewLine);
			stringBuilder.Append(".then(() => { document.getElementById('status').innerText = 'Success! You may now close this window.'; setTimeout(function() { window.close(); }, 2000);})");
			stringBuilder.Append(Environment.NewLine);
			stringBuilder.Append(".catch(() => { attemptCount++; if (attemptCount < 3) {setTimeout(sendAuthValues, 1000); return; } else { document.getElementById('status').innerText = 'authentication failed. Please try again.'; }});");
			stringBuilder.Append(Environment.NewLine);
			stringBuilder.Append("sendAuthValues();");
			stringBuilder.Append(Environment.NewLine);
			stringBuilder.Append("}");
			stringBuilder.Append(Environment.NewLine);
			stringBuilder.Append("else {");
			stringBuilder.Append(Environment.NewLine);
			stringBuilder.Append("document.getElementById('status').innerText = 'Authentication failed. Please try again.';");
			stringBuilder.Append(Environment.NewLine);
			stringBuilder.Append("setTimeout(function () { window.close(); }, 2000);");
			stringBuilder.Append(Environment.NewLine);
			stringBuilder.Append("}");
			stringBuilder.Append(Environment.NewLine);
			stringBuilder.Append("</script>");
			stringBuilder.Append(Environment.NewLine);
			stringBuilder.Append("</html>");
			return stringBuilder.ToString();
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public TwitchAuthentication.AuthenticationValues GetModel(string token, string scopes)
		{
			return new TwitchAuthentication.AuthenticationValues
			{
				Token = token,
				Scopes = scopes
			};
		}

		public void SendRequestToBrowser(string ClientID)
		{
			string url = TwitchAuthentication.GetUrl(ClientID);
			new Uri(url);
			Process.Start(url);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static string GetUrl(string ClientID)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("https://id.twitch.tv/oauth2/authorize");
			stringBuilder.Append("?response_type=token");
			stringBuilder.Append("&client_id=").Append(ClientID);
			stringBuilder.Append("&redirect_uri=").Append("http://localhost:56207");
			if (!TwitchAuthentication.bFirstLogin)
			{
				stringBuilder.Append("&force_verify=true");
			}
			stringBuilder.Append("&scope=channel_read+chat:read+chat:edit+user_blocks_read+channel_subscriptions+channel_check_subscription+channel:read:redemptions+channel:manage:redemptions+user_read+bits:read+channel:read:hype_train+channel:read:goals+user:read:broadcast");
			return stringBuilder.ToString();
		}

		public void GetToken()
		{
			GameManager.Instance.StartCoroutine(this.DeviceAuthenticate());
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public IEnumerator DeviceAuthenticate()
		{
			Log.Out("packing data for device auth init request...");
			WWWForm wwwform = new WWWForm();
			wwwform.AddField("client_id", TwitchAuthentication.client_id);
			wwwform.AddField("scopes", "channel_read chat:read chat:edit user_blocks_read channel_subscriptions channel_check_subscription channel:read:redemptions channel:manage:redemptions user_read bits:read channel:read:hype_train channel:read:goals user:read:broadcast");
			string device_code = string.Empty;
			string text = string.Empty;
			string text2 = string.Empty;
			Log.Out("creating init request...");
			using (UnityWebRequest req = UnityWebRequest.Post("https://id.twitch.tv/oauth2/device", wwwform))
			{
				Log.Out("sending init request...");
				yield return req.SendWebRequest();
				if (req.result != 1)
				{
					Log.Warning("failed to initiate DCF Twitch OAuth endpoint with response: " + req.downloadHandler.text);
					this.completed = true;
					yield break;
				}
				TwitchAuthentication.DeviceAuthIntitationData deviceAuthIntitationData = JsonUtility.FromJson<TwitchAuthentication.DeviceAuthIntitationData>(req.downloadHandler.text);
				device_code = deviceAuthIntitationData.device_code;
				text2 = deviceAuthIntitationData.user_code;
				text = deviceAuthIntitationData.verification_uri;
			}
			UnityWebRequest req = null;
			Log.Out("successfully initialized auth!");
			Log.Out("generating qr code...");
			BarcodeWriter barcodeWriter = new BarcodeWriter
			{
				Format = 2048,
				Options = new QrCodeEncodingOptions
				{
					Height = 256,
					Width = 256,
					Margin = 1
				}
			};
			Texture2D texture2D = new Texture2D(256, 256);
			texture2D.SetPixels32(barcodeWriter.Write(text));
			texture2D.Apply();
			Log.Out("successfully generated qr code!");
			if (TwitchAuthentication.QRCodeGenerated != null)
			{
				TwitchAuthentication.QRCodeGenerated(texture2D, text2, text);
				Log.Out("successfully displayed qr code!");
				Application.OpenURL(text);
			}
			else
			{
				Debug.LogWarning("No one listening for QR Code.");
			}
			bool keepPolling = true;
			bool retrievedToken = false;
			string token = string.Empty;
			Log.Out("initiating polling loop...");
			int pollLimit = 12;
			int pollCount = 0;
			while (keepPolling && pollCount < pollLimit)
			{
				Log.Out("packing data for token poll request");
				WWWForm pollData = new WWWForm();
				pollData.AddField("client_id", TwitchAuthentication.client_id);
				pollData.AddField("scopes", "channel_read chat:read chat:edit user_blocks_read channel_subscriptions channel_check_subscription channel:read:redemptions channel:manage:redemptions user_read bits:read channel:read:hype_train channel:read:goals user:read:broadcast");
				pollData.AddField("device_code", device_code);
				pollData.AddField("grant_type", "urn:ietf:params:oauth:grant-type:device_code");
				Log.Out("will poll token endpoint in 5s...");
				yield return new WaitForSecondsRealtime(5f);
				Log.Out("creating token poll request...");
				using (UnityWebRequest req = UnityWebRequest.Post("https://id.twitch.tv/oauth2/token", pollData))
				{
					Log.Out("sending token poll request");
					yield return req.SendWebRequest();
					if (req.result != 1)
					{
						TwitchAuthentication.PollingIncompleteResponse pollingIncompleteResponse = JsonUtility.FromJson<TwitchAuthentication.PollingIncompleteResponse>(req.downloadHandler.text);
						if (pollingIncompleteResponse.message != "authorization_pending")
						{
							Log.Warning("Failed to get device token with message: " + pollingIncompleteResponse.message);
							keepPolling = false;
						}
					}
					else
					{
						TwitchAuthentication.PollingSuccessResponse pollingSuccessResponse = JsonUtility.FromJson<TwitchAuthentication.PollingSuccessResponse>(req.downloadHandler.text);
						token = pollingSuccessResponse.access_token;
						this.oauth = "oauth:" + pollingSuccessResponse.access_token;
						this.refresh_token = pollingSuccessResponse.refresh_token;
						GameManager.Instance.StartCoroutine(this.RefreshToken((float)(pollingSuccessResponse.expires_in - 60), false));
						Log.Out("Successfully retrieved oauth token");
						retrievedToken = true;
						keepPolling = false;
					}
				}
				req = null;
				int num = pollCount;
				pollCount = num + 1;
				pollData = null;
			}
			if (!retrievedToken)
			{
				this.completed = true;
				yield break;
			}
			yield return this.ValidateToken(token);
			this.completed = true;
			Log.Out("authentication values have been received and validated!");
			yield break;
			yield break;
		}

		public IEnumerator RefreshToken(float delay, bool skipNextRefresh = false)
		{
			yield return new WaitForSecondsRealtime(delay);
			Log.Out("invoking token refresh");
			this.oauth = "";
			string text = JsonConvert.SerializeObject(new
			{
				refreshToken = this.refresh_token
			});
			TwitchManager.Current.WaitForOAuth();
			using (UnityWebRequest req = UnityWebRequest.Post("https://hijg23d34qghpdonxdf6ytq63m0gezwl.lambda-url.us-east-1.on.aws/", text, "application/json"))
			{
				yield return req.SendWebRequest();
				if (req.result != 1)
				{
					Log.Warning("failed to refresh token: " + req.downloadHandler.text);
				}
				else
				{
					TwitchAuthentication.PollingSuccessResponse pollingSuccessResponse = JsonUtility.FromJson<TwitchAuthentication.PollingSuccessResponse>(req.downloadHandler.text);
					this.oauth = "oauth:" + pollingSuccessResponse.access_token;
					this.refresh_token = pollingSuccessResponse.refresh_token;
					Log.Out("Successfully refreshed oauth token");
					if (!skipNextRefresh)
					{
						GameManager.Instance.StartCoroutine(this.RefreshToken((float)(pollingSuccessResponse.expires_in - 60), false));
					}
				}
			}
			UnityWebRequest req = null;
			yield break;
			yield break;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public IEnumerator Authenticate()
		{
			this.StartListener();
			if (!this.TwitchListener.IsListening)
			{
				Log.Warning("Twitch listener is not listening after start attempt");
				TwitchManager.Current.StopTwitchIntegration(TwitchManager.InitStates.Failed);
				yield break;
			}
			Task<TwitchAuthentication.AuthenticationValues> t = this.GetAuthenticationValuesAsync();
			yield return new WaitUntil(() => t.Status == TaskStatus.Running);
			this.SendRequestToBrowser(TwitchAuthentication.client_id);
			yield return new WaitUntil(() => t.IsCompletedSuccessfully || t.IsCompleted || t.IsCanceled || t.IsFaulted);
			TwitchAuthentication.AuthenticationValues result = t.Result;
			if (result == null)
			{
				Log.Warning("did not successfully retrieve auth values");
				TwitchManager.Current.StopTwitchIntegration(TwitchManager.InitStates.Failed);
				yield break;
			}
			this.oauth = "oauth:" + result.Token;
			yield return this.ValidateToken(result.Token);
			this.completed = true;
			Log.Out("authentication values have been received and validated!");
			yield break;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public IEnumerator ValidateToken(string token)
		{
			Log.Out("creating token validation request");
			using (UnityWebRequest req = UnityWebRequest.Get("https://id.twitch.tv/oauth2/validate"))
			{
				req.SetRequestHeader("Authorization", "Bearer " + token);
				Log.Out("sending token validation request");
				yield return req.SendWebRequest();
				if (req.result != 1)
				{
					Log.Warning("failed to validate on Twitch oauth endpoint with response: " + req.downloadHandler.text);
				}
				else
				{
					try
					{
						Log.Out("token validated successfully!");
						TwitchAuthentication.ValidationResponseData validationResponseData = JsonUtility.FromJson<TwitchAuthentication.ValidationResponseData>(req.downloadHandler.text);
						this.userName = validationResponseData.login;
						this.userID = validationResponseData.user_id;
					}
					catch (Exception)
					{
						Log.Warning("failed to make use of Twitch validate response data: " + req.downloadHandler.text);
					}
				}
			}
			UnityWebRequest req = null;
			yield break;
			yield break;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public IEnumerator RunGetAuthenticationValues(Task<TwitchAuthentication.AuthenticationValues> task)
		{
			if (task != null)
			{
				while (!task.IsCompleted)
				{
					yield return null;
				}
				if (task.IsFaulted)
				{
					throw task.Exception;
				}
				if (task.Result != null)
				{
					this.oauth = "oauth:" + task.Result.Token;
					using (UnityWebRequest req = UnityWebRequest.Get("https://id.twitch.tv/oauth2/validate"))
					{
						string token = task.Result.Token;
						req.SetRequestHeader("Authorization", "Bearer " + token);
						yield return req.SendWebRequest();
						if (req.result != 1)
						{
							Log.Warning("failed to validate on Twitch oauth endpoint with response: " + req.downloadHandler.text);
						}
						else
						{
							try
							{
								TwitchAuthentication.ValidationResponseData validationResponseData = JsonUtility.FromJson<TwitchAuthentication.ValidationResponseData>(req.downloadHandler.text);
								this.userName = validationResponseData.login;
								this.userID = validationResponseData.user_id;
							}
							catch (Exception)
							{
								Log.Warning("failed to make use of Twitch validate response data: " + req.downloadHandler.text);
							}
						}
					}
					UnityWebRequest req = null;
				}
			}
			yield break;
			yield break;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly HttpListener TwitchListener;

		[PublicizedFrom(EAccessModifier.Private)]
		public const string ReturnUrl = "http://localhost:56207";

		public string oauth = "";

		public string oauthwithHeader = "";

		public string userName = "";

		public string userID = "";

		[PublicizedFrom(EAccessModifier.Private)]
		public string refresh_token = "";

		public static bool bFirstLogin = true;

		public bool completed;

		public static string client_id = "qtex0oyygtptfirybmxqxm27cdcdpf";

		public class AuthenticationValues
		{
			public string Token;

			public string Scopes;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		[Serializable]
		public class ValidationResponseData
		{
			public string login;

			public string user_id;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		[Serializable]
		public class DeviceAuthIntitationData
		{
			public string device_code;

			public int expires_in;

			public int interval;

			public string user_code;

			public string verification_uri;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		[Serializable]
		public class PollingIncompleteResponse
		{
			public string message;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		[Serializable]
		public class PollingSuccessResponse
		{
			public string access_token;

			public int expires_in;

			public string refresh_token;
		}
	}
}
