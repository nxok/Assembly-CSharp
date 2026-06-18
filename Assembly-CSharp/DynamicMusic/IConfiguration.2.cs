using System;
using System.Collections.Generic;
using MusicUtils.Enums;

namespace DynamicMusic
{
	public interface IConfiguration<T> : IConfiguration
	{
		Dictionary<LayerType, T> Layers { get; }
	}
}
