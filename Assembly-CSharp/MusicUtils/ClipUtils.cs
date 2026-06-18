using System;
using System.Collections;
using UnityEngine;

namespace MusicUtils
{
	public static class ClipUtils
	{
		public static AudioClip LoadClipImmediate(string _path)
		{
			return LoadManager.LoadAsset<AudioClip>(_path, null, null, false, true, false).Asset;
		}

		public static IEnumerator LoadClipFrom(string _path, Func<AudioClip, float[]> _onAudioClipLoad, Action _onFinish)
		{
			LoadManager.AssetRequestTask<AudioClip> req = LoadManager.LoadAsset<AudioClip>(_path, null, null, false, false, false);
			yield return new WaitUntil(() => req.IsDone);
			if (req.Asset)
			{
				yield return ClipUtils.StripClip(req.Asset, _onAudioClipLoad(req.Asset));
				if (_onFinish != null)
				{
					_onFinish();
				}
			}
			yield break;
		}

		public static IEnumerator StripClip(AudioClip _clip, float[] _data)
		{
			float[] buffer = MemoryPools.poolFloat.Alloc(44100);
			int cursor = 0;
			yield return null;
			while (cursor < _clip.samples)
			{
				_clip.GetData(buffer, cursor);
				yield return null;
				int num = 0;
				while (num < buffer.Length && cursor < _clip.samples)
				{
					_data[2 * cursor] = buffer[num];
					int num2 = 2;
					int num3 = cursor;
					cursor = num3 + 1;
					_data[num2 * num3 + 1] = buffer[num + 1];
					num += 2;
				}
				yield return null;
			}
			MemoryPools.poolFloat.Free(buffer);
			yield break;
		}
	}
}
