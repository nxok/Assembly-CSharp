using System;
using UnityEngine;

public class CharacterClickHandler : MonoBehaviour
{
	public void HandleClick()
	{
		this.parentScript.OnCharacterClicked(base.gameObject);
	}

	public CharacterConstruct parentScript;
}
