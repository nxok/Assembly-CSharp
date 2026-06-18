using System;
using System.Collections.Generic;

public class VideoManager
{
	public static void Init()
	{
		VideoManager.initialized = true;
		VideoManager.videos = new Dictionary<string, VideoData>();
	}

	public static void AddVideo(VideoData data)
	{
		if (!VideoManager.initialized)
		{
			VideoManager.Init();
		}
		if (!VideoManager.videos.ContainsKey(data.name))
		{
			VideoManager.videos.Add(data.name, data);
			return;
		}
		VideoManager.videos[data.name] = data;
	}

	public static VideoData GetVideoData(string id)
	{
		VideoData videoData;
		if (VideoManager.videos.TryGetValue(id, out videoData))
		{
			return videoData;
		}
		return null;
	}

	public static Dictionary<string, VideoData> videos;

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool initialized;
}
