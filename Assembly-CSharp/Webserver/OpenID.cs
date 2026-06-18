using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using SpaceWizards.HttpListener;

namespace Webserver
{
	public static class OpenID
	{
		[PublicizedFrom(EAccessModifier.Private)]
		static OpenID()
		{
			for (int i = 0; i < Environment.GetCommandLineArgs().Length; i++)
			{
				if (Environment.GetCommandLineArgs()[i].EqualsCaseInsensitive("-debugopenid"))
				{
					OpenID.debugOpenId = true;
				}
			}
			ServicePointManager.ServerCertificateValidationCallback = delegate(object _srvPoint, X509Certificate _certificate, X509Chain _chain, SslPolicyErrors _errors)
			{
				if (_errors == SslPolicyErrors.None)
				{
					if (OpenID.verboseSsl)
					{
						Log.Out("[OpenID] Steam certificate: No error (1)");
					}
					return true;
				}
				X509Chain x509Chain = new X509Chain();
				x509Chain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
				x509Chain.ChainPolicy.ExtraStore.Add(OpenID.caCert);
				x509Chain.ChainPolicy.ExtraStore.Add(OpenID.caIntermediateCert);
				if (x509Chain.Build(new X509Certificate2(_certificate)))
				{
					x509Chain.Reset();
					if (OpenID.verboseSsl)
					{
						Log.Out("Steam certificate: No error (2)");
					}
					return true;
				}
				if (x509Chain.ChainStatus.Length == 0)
				{
					x509Chain.Reset();
					if (OpenID.verboseSsl)
					{
						Log.Out("Steam certificate: No error (3)");
					}
					return true;
				}
				foreach (X509ChainElement x509ChainElement in x509Chain.ChainElements)
				{
					if (OpenID.verboseSsl)
					{
						Log.Out("Validating cert: " + x509ChainElement.Certificate.Subject);
					}
					foreach (X509ChainStatus x509ChainStatus in x509ChainElement.ChainElementStatus)
					{
						if (OpenID.verboseSsl)
						{
							Log.Out(string.Format("   Status: {0}", x509ChainStatus.Status));
						}
						if (x509ChainStatus.Status != X509ChainStatusFlags.NoError && (x509ChainStatus.Status != X509ChainStatusFlags.UntrustedRoot || !x509ChainElement.Certificate.Equals(OpenID.caCert)))
						{
							Log.Warning(string.Format("[OpenID] Steam certificate error: {0} ### Error: {1}", x509ChainElement.Certificate.Subject, x509ChainStatus.Status));
							x509Chain.Reset();
							return false;
						}
					}
				}
				foreach (X509ChainStatus x509ChainStatus2 in x509Chain.ChainStatus)
				{
					if (x509ChainStatus2.Status != X509ChainStatusFlags.NoError && x509ChainStatus2.Status != X509ChainStatusFlags.UntrustedRoot)
					{
						Log.Warning(string.Format("[OpenID] Steam certificate error: {0}", x509ChainStatus2.Status));
						x509Chain.Reset();
						return false;
					}
				}
				x509Chain.Reset();
				if (OpenID.verboseSsl)
				{
					Log.Out("[OpenID] Steam certificate: No error (4)");
				}
				return true;
			};
		}

		public static string GetOpenIdLoginUrl(string _returnHost, string _returnUrl)
		{
			Dictionary<string, string> dictionary = new Dictionary<string, string>
			{
				{ "openid.ns", "http://specs.openid.net/auth/2.0" },
				{ "openid.mode", "checkid_setup" },
				{ "openid.return_to", _returnUrl },
				{ "openid.realm", _returnHost },
				{ "openid.identity", "http://specs.openid.net/auth/2.0/identifier_select" },
				{ "openid.claimed_id", "http://specs.openid.net/auth/2.0/identifier_select" }
			};
			return "https://steamcommunity.com/openid/login?" + OpenID.buildUrlParams(dictionary);
		}

		public static ulong Validate(HttpListenerRequest _req)
		{
			string value = OpenID.getValue(_req, "openid.mode");
			if (value == "cancel")
			{
				Log.Warning("[OpenID] Steam OpenID login canceled");
				return 0UL;
			}
			if (value == "error")
			{
				Log.Warning("[OpenID] Steam OpenID login error: " + OpenID.getValue(_req, "openid.error"));
				if (OpenID.debugOpenId)
				{
					OpenID.PrintOpenIdResponse(_req);
				}
				return 0UL;
			}
			string value2 = OpenID.getValue(_req, "openid.claimed_id");
			Match match = OpenID.steamIdUrlMatcher.Match(value2);
			if (!match.Success)
			{
				Log.Warning("[OpenID] Steam OpenID login result did not give a valid SteamID");
				if (OpenID.debugOpenId)
				{
					OpenID.PrintOpenIdResponse(_req);
				}
				return 0UL;
			}
			ulong num = ulong.Parse(match.Groups[1].Value);
			Dictionary<string, string> dictionary = new Dictionary<string, string>
			{
				{ "openid.ns", "http://specs.openid.net/auth/2.0" },
				{
					"openid.assoc_handle",
					OpenID.getValue(_req, "openid.assoc_handle")
				},
				{
					"openid.signed",
					OpenID.getValue(_req, "openid.signed")
				},
				{
					"openid.sig",
					OpenID.getValue(_req, "openid.sig")
				},
				{ "openid.identity", "http://specs.openid.net/auth/2.0/identifier_select" },
				{ "openid.claimed_id", "http://specs.openid.net/auth/2.0/identifier_select" }
			};
			foreach (string text in OpenID.getValue(_req, "openid.signed").Split(',', StringSplitOptions.None))
			{
				string text2 = "openid." + text;
				dictionary[text2] = OpenID.getValue(_req, text2);
			}
			dictionary.Add("openid.mode", "check_authentication");
			byte[] bytes = Encoding.ASCII.GetBytes(OpenID.buildUrlParams(dictionary));
			HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create("https://steamcommunity.com/openid/login");
			httpWebRequest.Method = "POST";
			httpWebRequest.ContentType = "application/x-www-form-urlencoded";
			httpWebRequest.ContentLength = (long)bytes.Length;
			httpWebRequest.Headers.Add(HttpRequestHeader.AcceptLanguage, "en");
			using (Stream requestStream = httpWebRequest.GetRequestStream())
			{
				requestStream.Write(bytes, 0, bytes.Length);
			}
			string text3;
			using (Stream responseStream = ((HttpWebResponse)httpWebRequest.GetResponse()).GetResponseStream())
			{
				using (StreamReader streamReader = new StreamReader(responseStream))
				{
					text3 = streamReader.ReadToEnd();
				}
			}
			if (text3.ContainsCaseInsensitive("is_valid:true"))
			{
				return num;
			}
			Log.Warning("[OpenID] Steam OpenID login failed: " + text3);
			return 0UL;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static string buildUrlParams(Dictionary<string, string> _queryParams)
		{
			string[] array = new string[_queryParams.Count];
			int num = 0;
			foreach (KeyValuePair<string, string> keyValuePair in _queryParams)
			{
				string text;
				string text2;
				keyValuePair.Deconstruct(out text, out text2);
				string text3 = text;
				string text4 = text2;
				array[num++] = text3 + "=" + Uri.EscapeDataString(text4);
			}
			return string.Join("&", array);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static string getValue(HttpListenerRequest _req, string _name)
		{
			NameValueCollection queryString = _req.QueryString;
			if (queryString[_name] == null)
			{
				throw new MissingMemberException("[OpenID] OpenID parameter \"" + _name + "\" missing");
			}
			return queryString[_name];
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static void PrintOpenIdResponse(HttpListenerRequest _req)
		{
			NameValueCollection queryString = _req.QueryString;
			for (int i = 0; i < queryString.Count; i++)
			{
				Log.Out("   " + queryString.GetKey(i) + " = " + queryString[i]);
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public const string STEAM_LOGIN = "https://steamcommunity.com/openid/login";

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly Regex steamIdUrlMatcher = new Regex("^https?:\\/\\/steamcommunity\\.com\\/openid\\/id\\/([0-9]{17,18})");

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly X509Certificate2 caCert = new X509Certificate2(GameIO.GetGameDir("Data/Web") + "/steam-rootca.cer");

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly X509Certificate2 caIntermediateCert = new X509Certificate2(GameIO.GetGameDir("Data/Web") + "/steam-intermediate.cer");

		[PublicizedFrom(EAccessModifier.Private)]
		public static bool verboseSsl = false;

		public static bool debugOpenId;
	}
}
