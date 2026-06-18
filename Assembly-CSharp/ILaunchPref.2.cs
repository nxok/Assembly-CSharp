using System;

public interface ILaunchPref<out T> : ILaunchPref
{
	T Value { get; }
}
