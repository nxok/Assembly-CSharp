using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Platform.XBL
{
	public class TextCensor : ITextCensor
	{
		public void Init(IPlatform _owner)
		{
		}

		public void Update()
		{
			if (this.fetchStarted)
			{
				return;
			}
			this.fetchStarted = true;
			if (PlatformManager.MultiPlatform.RemoteFileStorage != null)
			{
				ThreadManager.StartCoroutine(this.RetrieveBannedWords());
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public IEnumerator RetrieveBannedWords()
		{
			IRemoteFileStorage storage = PlatformManager.MultiPlatform.RemoteFileStorage;
			if (storage == null)
			{
				yield break;
			}
			while (!storage.IsReady)
			{
				yield return null;
			}
			storage.GetFile("BannedWordsXBL.txt", new IRemoteFileStorage.FileDownloadCompleteCallback(this.StorageProviderCallback));
			yield break;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void StorageProviderCallback(IRemoteFileStorage.EFileDownloadResult _result, string _errorDetails, byte[] _data)
		{
			if (_result != IRemoteFileStorage.EFileDownloadResult.Ok)
			{
				Log.Warning(string.Concat(new string[]
				{
					"Retrieving banned words list failed: ",
					_result.ToStringCached<IRemoteFileStorage.EFileDownloadResult>(),
					" (",
					_errorDetails,
					")"
				}));
				return;
			}
			using (MemoryStream memoryStream = new MemoryStream(_data))
			{
				using (StreamReader streamReader = new StreamReader(memoryStream, Encoding.UTF8))
				{
					string text;
					while ((text = streamReader.ReadLine()) != null)
					{
						if (Regex.IsMatch(text, "^[a-zA-Z0-9]+$"))
						{
							this.bannedPatterns.Add("(?<![a-zA-Z])" + Regex.Escape(text) + "(?![a-zA-Z])");
						}
						else
						{
							this.bannedPatterns.Add(Regex.Escape(text));
						}
					}
				}
			}
			this.fetchComplete = true;
		}

		public void CensorProfanity(string _input, PlatformUserIdentifierAbs _author, Action<CensoredTextResult> _callback)
		{
			if (string.IsNullOrEmpty(_input) || _input.Length == 0 || !this.fetchComplete)
			{
				_callback(new CensoredTextResult(true, _input, _input));
				return;
			}
			if (_author != null)
			{
				IPlatform crossplatformPlatform = PlatformManager.CrossplatformPlatform;
				if (((crossplatformPlatform != null) ? crossplatformPlatform.User : null) != null && PlatformManager.CrossplatformPlatform.User.PlatformUserId.Equals(_author))
				{
					_callback(new CensoredTextResult(true, _input, _input));
					return;
				}
				IPlatform nativePlatform = PlatformManager.NativePlatform;
				if (((nativePlatform != null) ? nativePlatform.User : null) != null && (PlatformManager.NativePlatform.User.PlatformUserId.Equals(_author) || PlatformManager.NativePlatform.User.IsFriend(_author)))
				{
					_callback(new CensoredTextResult(true, _input, _input));
					return;
				}
			}
			Task.Run(delegate
			{
				char[] array = _input.ToCharArray();
				foreach (string text in this.bannedPatterns)
				{
					foreach (object obj in Regex.Matches(_input, text, RegexOptions.IgnoreCase))
					{
						Match match = (Match)obj;
						for (int i = match.Index; i < match.Index + match.Length; i++)
						{
							array[i] = '*';
						}
					}
				}
				_callback(new CensoredTextResult(true, _input, new string(array)));
			});
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public const string isAlpNum = "^[a-zA-Z0-9]+$";

		[PublicizedFrom(EAccessModifier.Private)]
		public const string nonAlphabeticBefore = "(?<![a-zA-Z])";

		[PublicizedFrom(EAccessModifier.Private)]
		public const string nonAlphabeticAfter = "(?![a-zA-Z])";

		[PublicizedFrom(EAccessModifier.Private)]
		public bool fetchStarted;

		[PublicizedFrom(EAccessModifier.Private)]
		public bool fetchComplete;

		[PublicizedFrom(EAccessModifier.Private)]
		public HashSet<string> bannedPatterns = new HashSet<string>();
	}
}
