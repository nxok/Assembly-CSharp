using System;

public delegate void DictionaryUpdatedValueEventHandler<TKey, TValue>(object sender, DictionaryChangedEventArgs<TKey, TValue> e);
