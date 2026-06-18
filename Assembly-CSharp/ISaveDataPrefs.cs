using System;

public interface ISaveDataPrefs
{
	float GetFloat(string key)
	{
		return this.GetFloat(key, 0f);
	}

	float GetFloat(string key, float defaultValue);

	void SetFloat(string key, float value);

	int GetInt(string key)
	{
		return this.GetInt(key, 0);
	}

	int GetInt(string key, int defaultValue);

	void SetInt(string key, int value);

	string GetString(string key)
	{
		return this.GetString(key, "");
	}

	string GetString(string key, string defaultValue);

	void SetString(string key, string value);

	bool HasKey(string key);

	void DeleteKey(string key);

	void DeleteAll();

	void Save();

	void Load();

	bool CanLoad { get; }
}
