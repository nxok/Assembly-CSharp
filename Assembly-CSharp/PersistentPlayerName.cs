using System;

public class PersistentPlayerName
{
	public AuthoredText AuthoredName
	{
		get
		{
			return this.playerName;
		}
	}

	public string DisplayName
	{
		get
		{
			if (this.cachedDisplayName != null)
			{
				return this.cachedDisplayName + ((this.nameSuffix > 0) ? string.Format("({0})", this.nameSuffix) : "");
			}
			if (!GeneratedTextManager.IsFiltered(this.playerName))
			{
				if (!GeneratedTextManager.IsFiltering(this.playerName))
				{
					GeneratedTextManager.PrefilterText(this.playerName, GeneratedTextManager.TextFilteringMode.Filter);
				}
				return GeneratedTextManager.GetDisplayTextImmediately(this.playerName, false, GeneratedTextManager.TextFilteringMode.None, GeneratedTextManager.BbCodeSupportMode.SupportedAndAddEscapes) + ((this.nameSuffix > 0) ? string.Format("({0})", this.nameSuffix) : "");
			}
			this.cachedDisplayName = GeneratedTextManager.GetDisplayTextImmediately(this.playerName, false, GeneratedTextManager.TextFilteringMode.FilterOtherPlatforms, GeneratedTextManager.BbCodeSupportMode.SupportedAndAddEscapes);
			return this.cachedDisplayName + ((this.nameSuffix > 0) ? string.Format("({0})", this.nameSuffix) : "");
		}
	}

	public string SafeDisplayName
	{
		get
		{
			if (!GeneratedTextManager.IsFiltered(this.playerName))
			{
				if (!GeneratedTextManager.IsFiltering(this.playerName))
				{
					GeneratedTextManager.PrefilterText(this.playerName, GeneratedTextManager.TextFilteringMode.Filter);
				}
				string text = GeneratedTextManager.GetDisplayTextImmediately(this.playerName, false, GeneratedTextManager.TextFilteringMode.FilterOtherPlatforms, GeneratedTextManager.BbCodeSupportMode.SupportedAndAddEscapes);
				if (!text.Equals("{...}") && this.nameSuffix > 0)
				{
					text += string.Format("({0})", this.nameSuffix);
				}
				return text;
			}
			return this.DisplayName;
		}
	}

	public PersistentPlayerName(AuthoredText name)
	{
		this.playerName = name;
		GeneratedTextManager.PrefilterText(name, GeneratedTextManager.TextFilteringMode.FilterOtherPlatforms);
	}

	public void Update(string name, PlatformUserIdentifierAbs author)
	{
		this.cachedDisplayName = null;
		this.nameSuffix = 0;
		this.playerName.Update(name, author);
		GeneratedTextManager.PrefilterText(this.playerName, GeneratedTextManager.TextFilteringMode.FilterOtherPlatforms);
		GameManager.Instance.persistentPlayers.FixNameCollisions(name);
	}

	public void Update(AuthoredText name)
	{
		if (this.playerName != name)
		{
			this.cachedDisplayName = null;
			this.nameSuffix = 0;
			this.playerName = name;
			GeneratedTextManager.PrefilterText(this.playerName, GeneratedTextManager.TextFilteringMode.FilterOtherPlatforms);
			GameManager.Instance.persistentPlayers.FixNameCollisions(name.Text);
		}
	}

	public void SetCollisionSuffix(int suffix)
	{
		this.nameSuffix = suffix;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public AuthoredText playerName;

	[PublicizedFrom(EAccessModifier.Private)]
	public string cachedDisplayName;

	[PublicizedFrom(EAccessModifier.Private)]
	public int nameSuffix;
}
