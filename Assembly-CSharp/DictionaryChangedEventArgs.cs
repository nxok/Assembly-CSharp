using System;

public class DictionaryChangedEventArgs<TKey, TValue> : EventArgs
{
	public TKey Key { get; }

	public TValue Value { get; }

	public ObservableDictionary<TKey, TValue>.EChangeType Action { get; }

	public DictionaryChangedEventArgs(TKey key, TValue value, ObservableDictionary<TKey, TValue>.EChangeType action)
	{
		this.Key = key;
		this.Value = value;
		this.Action = action;
	}
}
