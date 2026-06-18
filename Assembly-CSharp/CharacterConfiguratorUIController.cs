using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class CharacterConfiguratorUIController : MonoBehaviour
{
	[PublicizedFrom(EAccessModifier.Private)]
	public void Start()
	{
		if (this.characterConfigurator == null)
		{
			Debug.LogError("CharacterConstruct reference is missing! Please assign it in the inspector.");
			return;
		}
		this.InitializeDropdowns();
		this.SetupDropdownListeners();
		this.UpdateFacialHairPanelVisibility();
		if (this.toggleJiggleButton != null)
		{
			Image image = this.toggleJiggleButton.GetComponent<Image>();
			this.toggleJiggleButton.onClick.AddListener(delegate
			{
				if (this.characterConfigurator != null)
				{
					this.characterConfigurator.ToggleJiggle();
					image.color = (this.characterConfigurator.IsJiggleEnabled() ? new Color(1f, 0.85f, 0.75f) : new Color(0.7f, 0.7f, 0.7f));
				}
			});
		}
		if (this.rotationSlider != null)
		{
			this.rotationSlider.minValue = 0f;
			this.rotationSlider.maxValue = 360f;
			this.rotationSlider.value = 180f;
			this.rotationSlider.onValueChanged.AddListener(delegate(float value)
			{
				if (this.characterConfigurator != null)
				{
					this.characterConfigurator.RotateCharacter(value);
				}
			});
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void InitializeDropdowns()
	{
		if (this.sexDropdown != null)
		{
			this.sexDropdown.ClearOptions();
			foreach (string text in this.characterConfigurator.GetSexTypes())
			{
				this.sexDropdown.options.Add(new TMP_Dropdown.OptionData(text));
			}
			this.sexDropdown.value = this.characterConfigurator.selectedSexIndex;
			this.sexDropdown.RefreshShownValue();
		}
		if (this.raceDropdown != null)
		{
			this.raceDropdown.ClearOptions();
			foreach (string text2 in this.characterConfigurator.GetRaceTypes())
			{
				this.raceDropdown.options.Add(new TMP_Dropdown.OptionData(text2));
			}
			this.raceDropdown.value = this.characterConfigurator.selectedRaceIndex;
			this.raceDropdown.RefreshShownValue();
		}
		if (this.variantDropdown != null)
		{
			this.variantDropdown.ClearOptions();
			foreach (string text3 in this.characterConfigurator.GetVariantTypes())
			{
				this.variantDropdown.options.Add(new TMP_Dropdown.OptionData(text3));
			}
			this.variantDropdown.value = this.characterConfigurator.selectedVariantIndex;
			this.variantDropdown.RefreshShownValue();
		}
		if (this.hairDropdown != null)
		{
			this.hairDropdown.ClearOptions();
			foreach (string text4 in this.characterConfigurator.GetHairTypes())
			{
				this.hairDropdown.options.Add(new TMP_Dropdown.OptionData(text4));
			}
			this.hairDropdown.value = this.characterConfigurator.selectedHairIndex + 1;
			this.hairDropdown.RefreshShownValue();
		}
		if (this.hairColorDropdown != null)
		{
			this.hairColorDropdown.ClearOptions();
			foreach (string text5 in this.characterConfigurator.GetHairColors())
			{
				this.hairColorDropdown.options.Add(new TMP_Dropdown.OptionData(text5));
			}
			this.hairColorDropdown.value = this.characterConfigurator.selectedHairColorIndex;
			this.hairColorDropdown.RefreshShownValue();
		}
		if (this.eyeColorDropdown != null)
		{
			this.eyeColorDropdown.ClearOptions();
			foreach (string text6 in this.characterConfigurator.GetEyeColors())
			{
				this.eyeColorDropdown.options.Add(new TMP_Dropdown.OptionData(text6));
			}
			this.eyeColorDropdown.value = this.characterConfigurator.selectedEyeColorIndex;
			this.eyeColorDropdown.RefreshShownValue();
		}
		if (this.gearDropdown != null)
		{
			this.gearDropdown.ClearOptions();
			foreach (string text7 in this.characterConfigurator.GetGearTypes())
			{
				this.gearDropdown.options.Add(new TMP_Dropdown.OptionData(text7));
			}
			this.gearDropdown.value = this.characterConfigurator.selectedGearIndex + 1;
			this.gearDropdown.RefreshShownValue();
		}
		if (this.gearHeadDropdown != null)
		{
			this.gearHeadDropdown.ClearOptions();
			foreach (string text8 in this.characterConfigurator.GetGearTypes())
			{
				this.gearHeadDropdown.options.Add(new TMP_Dropdown.OptionData(text8));
			}
			this.gearHeadDropdown.value = this.characterConfigurator.selectedHeadGearIndex + 1;
			this.gearHeadDropdown.RefreshShownValue();
		}
		if (this.gearBodyDropdown != null)
		{
			this.gearBodyDropdown.ClearOptions();
			foreach (string text9 in this.characterConfigurator.GetGearTypes())
			{
				this.gearBodyDropdown.options.Add(new TMP_Dropdown.OptionData(text9));
			}
			this.gearBodyDropdown.value = this.characterConfigurator.selectedBodyGearIndex + 1;
			this.gearBodyDropdown.RefreshShownValue();
		}
		if (this.gearHandsDropdown != null)
		{
			this.gearHandsDropdown.ClearOptions();
			foreach (string text10 in this.characterConfigurator.GetGearTypes())
			{
				this.gearHandsDropdown.options.Add(new TMP_Dropdown.OptionData(text10));
			}
			this.gearHandsDropdown.value = this.characterConfigurator.selectedHandsGearIndex + 1;
			this.gearHandsDropdown.RefreshShownValue();
		}
		if (this.gearFeetDropdown != null)
		{
			this.gearFeetDropdown.ClearOptions();
			foreach (string text11 in this.characterConfigurator.GetGearTypes())
			{
				this.gearFeetDropdown.options.Add(new TMP_Dropdown.OptionData(text11));
			}
			this.gearFeetDropdown.value = this.characterConfigurator.selectedFeetGearIndex + 1;
			this.gearFeetDropdown.RefreshShownValue();
		}
		if (this.facialHairDropdown != null)
		{
			this.facialHairDropdown.ClearOptions();
			foreach (string text12 in this.characterConfigurator.GetFacialHairTypes())
			{
				this.facialHairDropdown.options.Add(new TMP_Dropdown.OptionData(text12));
			}
			this.facialHairDropdown.value = this.characterConfigurator.selectedFacialHairIndex + 1;
			this.facialHairDropdown.RefreshShownValue();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetupDropdownListeners()
	{
		if (this.sexDropdown != null)
		{
			this.sexDropdown.onValueChanged.AddListener(new UnityAction<int>(this.OnSexDropdownChanged));
		}
		if (this.raceDropdown != null)
		{
			this.raceDropdown.onValueChanged.AddListener(delegate(int index)
			{
				this.characterConfigurator.SetRace(index);
			});
		}
		if (this.variantDropdown != null)
		{
			this.variantDropdown.onValueChanged.AddListener(delegate(int index)
			{
				this.characterConfigurator.SetVariant(index);
			});
		}
		if (this.hairDropdown != null)
		{
			this.hairDropdown.onValueChanged.AddListener(delegate(int index)
			{
				this.characterConfigurator.SetHair(index);
			});
		}
		if (this.hairColorDropdown != null)
		{
			this.hairColorDropdown.onValueChanged.AddListener(delegate(int index)
			{
				this.characterConfigurator.SetHairColor(index);
			});
		}
		if (this.eyeColorDropdown != null)
		{
			this.eyeColorDropdown.onValueChanged.AddListener(delegate(int index)
			{
				this.characterConfigurator.SetEyeColor(index);
			});
		}
		if (this.gearDropdown != null)
		{
			this.gearDropdown.onValueChanged.AddListener(delegate(int index)
			{
				this.characterConfigurator.SetGear(index);
			});
			this.gearDropdown.onValueChanged.AddListener(delegate(int index)
			{
				if (this.gearHeadDropdown != null)
				{
					this.gearHeadDropdown.SetValueWithoutNotify(index);
				}
				if (this.gearBodyDropdown != null)
				{
					this.gearBodyDropdown.SetValueWithoutNotify(index);
				}
				if (this.gearHandsDropdown != null)
				{
					this.gearHandsDropdown.SetValueWithoutNotify(index);
				}
				if (this.gearFeetDropdown != null)
				{
					this.gearFeetDropdown.SetValueWithoutNotify(index);
				}
			});
		}
		if (this.gearHeadDropdown != null)
		{
			this.gearHeadDropdown.onValueChanged.AddListener(delegate(int index)
			{
				this.characterConfigurator.SetHeadGear(index);
			});
		}
		if (this.gearBodyDropdown != null)
		{
			this.gearBodyDropdown.onValueChanged.AddListener(delegate(int index)
			{
				this.characterConfigurator.SetBodyGear(index);
			});
		}
		if (this.gearHandsDropdown != null)
		{
			this.gearHandsDropdown.onValueChanged.AddListener(delegate(int index)
			{
				this.characterConfigurator.SetHandsGear(index);
			});
		}
		if (this.gearFeetDropdown != null)
		{
			this.gearFeetDropdown.onValueChanged.AddListener(delegate(int index)
			{
				this.characterConfigurator.SetFeetGear(index);
			});
		}
		if (this.facialHairDropdown != null)
		{
			this.facialHairDropdown.onValueChanged.AddListener(delegate(int index)
			{
				this.characterConfigurator.SetFacialHair(index);
			});
		}
		this.WireUpArrowButtons(this.sexDropdown, delegate
		{
			this.CycleDropdown(this.sexDropdown, -1);
		}, delegate
		{
			this.CycleDropdown(this.sexDropdown, 1);
		});
		this.WireUpArrowButtons(this.raceDropdown, delegate
		{
			this.CycleDropdown(this.raceDropdown, -1);
		}, delegate
		{
			this.CycleDropdown(this.raceDropdown, 1);
		});
		this.WireUpArrowButtons(this.variantDropdown, delegate
		{
			this.CycleDropdown(this.variantDropdown, -1);
		}, delegate
		{
			this.CycleDropdown(this.variantDropdown, 1);
		});
		this.WireUpArrowButtons(this.hairDropdown, delegate
		{
			this.CycleDropdown(this.hairDropdown, -1);
		}, delegate
		{
			this.CycleDropdown(this.hairDropdown, 1);
		});
		this.WireUpArrowButtons(this.hairColorDropdown, delegate
		{
			this.CycleDropdown(this.hairColorDropdown, -1);
		}, delegate
		{
			this.CycleDropdown(this.hairColorDropdown, 1);
		});
		this.WireUpArrowButtons(this.eyeColorDropdown, delegate
		{
			this.CycleDropdown(this.eyeColorDropdown, -1);
		}, delegate
		{
			this.CycleDropdown(this.eyeColorDropdown, 1);
		});
		this.WireUpArrowButtons(this.gearDropdown, delegate
		{
			this.CycleDropdown(this.gearDropdown, -1);
		}, delegate
		{
			this.CycleDropdown(this.gearDropdown, 1);
		});
		this.WireUpArrowButtons(this.gearHeadDropdown, delegate
		{
			this.CycleDropdown(this.gearHeadDropdown, -1);
		}, delegate
		{
			this.CycleDropdown(this.gearHeadDropdown, 1);
		});
		this.WireUpArrowButtons(this.gearBodyDropdown, delegate
		{
			this.CycleDropdown(this.gearBodyDropdown, -1);
		}, delegate
		{
			this.CycleDropdown(this.gearBodyDropdown, 1);
		});
		this.WireUpArrowButtons(this.gearHandsDropdown, delegate
		{
			this.CycleDropdown(this.gearHandsDropdown, -1);
		}, delegate
		{
			this.CycleDropdown(this.gearHandsDropdown, 1);
		});
		this.WireUpArrowButtons(this.gearFeetDropdown, delegate
		{
			this.CycleDropdown(this.gearFeetDropdown, -1);
		}, delegate
		{
			this.CycleDropdown(this.gearFeetDropdown, 1);
		});
		this.WireUpArrowButtons(this.facialHairDropdown, delegate
		{
			this.CycleDropdown(this.facialHairDropdown, -1);
		}, delegate
		{
			this.CycleDropdown(this.facialHairDropdown, 1);
		});
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void WireUpArrowButtons(TMP_Dropdown dropdown, UnityAction leftAction, UnityAction rightAction)
	{
		if (dropdown != null)
		{
			DropdownArrowButtons component = dropdown.gameObject.GetComponent<DropdownArrowButtons>();
			if (component != null)
			{
				if (component.leftButton != null)
				{
					component.leftButton.onClick.AddListener(leftAction);
				}
				if (component.rightButton != null)
				{
					component.rightButton.onClick.AddListener(rightAction);
				}
			}
		}
	}

	public void OnSexDropdownChanged(int index)
	{
		if (this.characterConfigurator != null)
		{
			this.characterConfigurator.SetSex(index);
			this.UpdateFacialHairPanelVisibility();
			if (index == 1 && this.facialHairDropdown != null)
			{
				this.facialHairDropdown.value = 0;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CycleDropdown(TMP_Dropdown dropdown, int direction)
	{
		if (dropdown == null || dropdown.options.Count == 0)
		{
			return;
		}
		int num = dropdown.value + direction;
		if (num < 0)
		{
			num = dropdown.options.Count - 1;
		}
		else if (num >= dropdown.options.Count)
		{
			num = 0;
		}
		dropdown.value = num;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateFacialHairPanelVisibility()
	{
		if (this.facialHairPanel != null && this.characterConfigurator != null)
		{
			this.facialHairPanel.SetActive(this.characterConfigurator.selectedSexIndex == 0);
		}
	}

	public void RefreshAllDropdowns()
	{
		this.InitializeDropdowns();
	}

	[Header("References")]
	[Tooltip("Reference to the CharacterConstruct script")]
	public CharacterConfigurator characterConfigurator;

	[Header("Character Configuration")]
	[Tooltip("Dropdown for selecting sex")]
	public TMP_Dropdown sexDropdown;

	[Tooltip("Dropdown for selecting race")]
	public TMP_Dropdown raceDropdown;

	[Tooltip("Dropdown for selecting variant")]
	public TMP_Dropdown variantDropdown;

	[Header("Appearance Configuration")]
	[Tooltip("Dropdown for selecting hair style")]
	public TMP_Dropdown hairDropdown;

	[Tooltip("Dropdown for selecting hair color")]
	public TMP_Dropdown hairColorDropdown;

	[Tooltip("Dropdown for selecting eye color")]
	public TMP_Dropdown eyeColorDropdown;

	[Tooltip("Dropdown for selecting gear")]
	public TMP_Dropdown gearDropdown;

	[Tooltip("Dropdown for selecting head gear")]
	public TMP_Dropdown gearHeadDropdown;

	[Tooltip("Dropdown for selecting body gear")]
	public TMP_Dropdown gearBodyDropdown;

	[Tooltip("Dropdown for selecting hands gear")]
	public TMP_Dropdown gearHandsDropdown;

	[Tooltip("Dropdown for selecting feet gear")]
	public TMP_Dropdown gearFeetDropdown;

	[Tooltip("Dropdown for selecting facial hair")]
	public TMP_Dropdown facialHairDropdown;

	[Tooltip("Button to toggle Jiggle")]
	public Button toggleJiggleButton;

	[Tooltip("Slider to Rotate Character")]
	public Slider rotationSlider;

	[Header("UI Panels")]
	[Tooltip("Panel that contains all UI controls")]
	public GameObject controlPanel;

	[Tooltip("Panel containing facial hair controls (shown only for males)")]
	public GameObject facialHairPanel;
}
