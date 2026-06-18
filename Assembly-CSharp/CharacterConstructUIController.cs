using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class CharacterConstructUIController : MonoBehaviour
{
	[PublicizedFrom(EAccessModifier.Private)]
	public void Start()
	{
		if (this.characterConstruct != null)
		{
			this.InitializeToggles();
			this.InitializeDropdowns();
		}
		else
		{
			Debug.LogError("CharacterConstruct reference is missing! Please assign it in the inspector.");
		}
		this.SetupToggleListeners();
		this.SetupDropdownListeners();
		this.UpdateHatGearControlsVisibility(this.showHatHairToggle.isOn);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void InitializeToggles()
	{
		this.showCharactersToggle.isOn = this.characterConstruct.ShowCharacters;
		this.showGearToggle.isOn = this.characterConstruct.ShowGear;
		this.showHairToggle.isOn = this.characterConstruct.ShowHair;
		this.showHatHairToggle.isOn = this.characterConstruct.ShowHatHair;
		this.showFacialHairToggle.isOn = this.characterConstruct.ShowFacialHair;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void InitializeDropdowns()
	{
		if (this.raceDropdown != null)
		{
			this.raceDropdown.ClearOptions();
			foreach (string text in this.characterConstruct.GetRaceTypes())
			{
				this.raceDropdown.options.Add(new TMP_Dropdown.OptionData(text));
			}
			this.raceDropdown.value = this.characterConstruct.selectedRaceIndex;
			this.raceDropdown.RefreshShownValue();
		}
		if (this.variantDropdown != null)
		{
			this.variantDropdown.ClearOptions();
			foreach (string text2 in this.characterConstruct.GetVariantTypes())
			{
				this.variantDropdown.options.Add(new TMP_Dropdown.OptionData(text2));
			}
			this.variantDropdown.value = this.characterConstruct.selectedVariantIndex;
			this.variantDropdown.RefreshShownValue();
		}
		if (this.hatGearDropdown != null)
		{
			this.hatGearDropdown.ClearOptions();
			foreach (string text3 in this.characterConstruct.GetGearTypes())
			{
				this.hatGearDropdown.options.Add(new TMP_Dropdown.OptionData(text3));
			}
			this.hatGearDropdown.value = this.characterConstruct.hatHairGearIndex;
			this.hatGearDropdown.RefreshShownValue();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetupToggleListeners()
	{
		this.showCharactersToggle.onValueChanged.AddListener(new UnityAction<bool>(this.OnShowCharactersToggled));
		this.showGearToggle.onValueChanged.AddListener(new UnityAction<bool>(this.OnShowGearToggled));
		this.showHairToggle.onValueChanged.AddListener(new UnityAction<bool>(this.OnShowHairToggled));
		this.showHatHairToggle.onValueChanged.AddListener(new UnityAction<bool>(this.OnShowHatHairToggled));
		this.showFacialHairToggle.onValueChanged.AddListener(new UnityAction<bool>(this.OnShowFacialHairToggled));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetupDropdownListeners()
	{
		if (this.raceDropdown != null)
		{
			this.raceDropdown.onValueChanged.AddListener(new UnityAction<int>(this.OnRaceDropdownChanged));
		}
		if (this.variantDropdown != null)
		{
			this.variantDropdown.onValueChanged.AddListener(new UnityAction<int>(this.OnVariantDropdownChanged));
		}
		if (this.hatGearDropdown != null)
		{
			this.hatGearDropdown.onValueChanged.AddListener(new UnityAction<int>(this.OnHatGearDropdownChanged));
		}
	}

	public void OnShowCharactersToggled(bool isOn)
	{
		if (this.characterConstruct != null)
		{
			this.characterConstruct.ShowCharacters = isOn;
		}
	}

	public void OnShowGearToggled(bool isOn)
	{
		if (this.characterConstruct != null)
		{
			this.characterConstruct.ShowGear = isOn;
		}
	}

	public void OnShowHairToggled(bool isOn)
	{
		if (this.characterConstruct != null)
		{
			this.characterConstruct.ShowHair = isOn;
		}
	}

	public void OnShowHatHairToggled(bool isOn)
	{
		if (this.characterConstruct != null)
		{
			this.characterConstruct.ShowHatHair = isOn;
			this.UpdateHatGearControlsVisibility(isOn);
		}
	}

	public void OnShowFacialHairToggled(bool isOn)
	{
		if (this.characterConstruct != null)
		{
			this.characterConstruct.ShowFacialHair = isOn;
		}
	}

	public void OnRaceDropdownChanged(int index)
	{
		if (this.characterConstruct != null)
		{
			this.characterConstruct.selectedRaceIndex = index;
			this.characterConstruct.RespawnAllGroups();
		}
	}

	public void OnVariantDropdownChanged(int index)
	{
		if (this.characterConstruct != null)
		{
			this.characterConstruct.selectedVariantIndex = index;
			this.characterConstruct.RespawnAllGroups();
		}
	}

	public void OnHatGearDropdownChanged(int index)
	{
		if (this.characterConstruct != null)
		{
			this.characterConstruct.hatHairGearIndex = index;
			this.characterConstruct.RespawnHatHairGroup();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateHatGearControlsVisibility(bool isHatHairVisible)
	{
		if (this.hatGearControlsPanel != null)
		{
			this.hatGearControlsPanel.SetActive(isHatHairVisible);
		}
	}

	[Header("References")]
	[Tooltip("Reference to the CharacterConstruct script")]
	public CharacterConstruct characterConstruct;

	[Header("UI Elements")]
	[Tooltip("Toggle button for showing characters")]
	public Toggle showCharactersToggle;

	[Tooltip("Toggle button for showing gear")]
	public Toggle showGearToggle;

	[Tooltip("Toggle button for showing hair")]
	public Toggle showHairToggle;

	[Tooltip("Toggle button for showing hat hair")]
	public Toggle showHatHairToggle;

	[Tooltip("Toggle button for showing facial hair")]
	public Toggle showFacialHairToggle;

	[Header("Race and Variant Controls")]
	[Tooltip("Panel containing race and variant controls")]
	public GameObject raceVariantControlsPanel;

	[Tooltip("Dropdown for selecting race")]
	public TMP_Dropdown raceDropdown;

	[Tooltip("Dropdown for selecting variant")]
	public TMP_Dropdown variantDropdown;

	[Header("Hat Hair Controls")]
	[Tooltip("Panel containing hat gear controls")]
	public GameObject hatGearControlsPanel;

	[Tooltip("Dropdown for selecting hat gear type")]
	public TMP_Dropdown hatGearDropdown;

	[Header("UI Panel")]
	[Tooltip("Panel that contains all UI controls")]
	public GameObject controlPanel;
}
