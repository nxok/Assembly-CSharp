using System;
using System.Collections.Generic;

public class VideoData
{
	public VideoData()
	{
		this.subtitles = new List<VideoSubtitle>();
	}

	public string name;

	public string url;

	public float defaultSubtitleDuration;

	public List<VideoSubtitle> subtitles;
}
