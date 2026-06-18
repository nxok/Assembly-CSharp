using System;
using System.Collections.Generic;
using UnityEngine;

public class DismembermentAccessoryMan : MonoBehaviour
{
	public void HidePart(EnumBodyPartHit bodyPart)
	{
		if (bodyPart <= EnumBodyPartHit.RightUpperLeg)
		{
			if (bodyPart <= EnumBodyPartHit.RightUpperArm)
			{
				if (bodyPart == EnumBodyPartHit.LeftUpperArm)
				{
					for (int i = 0; i < this.LeftLowerArm.Count; i++)
					{
						GameObject gameObject = this.LeftLowerArm[i];
						if (gameObject && gameObject.activeSelf)
						{
							gameObject.SetActive(false);
						}
					}
					for (int j = 0; j < this.LeftUpperArm.Count; j++)
					{
						GameObject gameObject2 = this.LeftUpperArm[j];
						if (gameObject2)
						{
							gameObject2.SetActive(false);
						}
					}
					return;
				}
				if (bodyPart != EnumBodyPartHit.RightUpperArm)
				{
					return;
				}
				for (int k = 0; k < this.RightLowerArm.Count; k++)
				{
					GameObject gameObject3 = this.RightLowerArm[k];
					if (gameObject3 && gameObject3.activeSelf)
					{
						gameObject3.SetActive(false);
					}
				}
				for (int l = 0; l < this.RightUpperArm.Count; l++)
				{
					GameObject gameObject4 = this.RightUpperArm[l];
					if (gameObject4)
					{
						gameObject4.SetActive(false);
					}
				}
				return;
			}
			else
			{
				if (bodyPart == EnumBodyPartHit.LeftUpperLeg)
				{
					for (int m = 0; m < this.LeftLowerLeg.Count; m++)
					{
						GameObject gameObject5 = this.LeftLowerLeg[m];
						if (gameObject5 && gameObject5.activeSelf)
						{
							gameObject5.SetActive(false);
						}
					}
					for (int n = 0; n < this.LeftUpperLeg.Count; n++)
					{
						GameObject gameObject6 = this.LeftUpperLeg[n];
						if (gameObject6)
						{
							gameObject6.SetActive(false);
						}
					}
					return;
				}
				if (bodyPart != EnumBodyPartHit.RightUpperLeg)
				{
					return;
				}
				for (int num = 0; num < this.RightLowerLeg.Count; num++)
				{
					GameObject gameObject7 = this.RightLowerLeg[num];
					if (gameObject7 && gameObject7.activeSelf)
					{
						gameObject7.SetActive(false);
					}
				}
				for (int num2 = 0; num2 < this.RightUpperLeg.Count; num2++)
				{
					GameObject gameObject8 = this.RightUpperLeg[num2];
					if (gameObject8)
					{
						gameObject8.SetActive(false);
					}
				}
				return;
			}
		}
		else if (bodyPart <= EnumBodyPartHit.RightLowerArm)
		{
			if (bodyPart == EnumBodyPartHit.LeftLowerArm)
			{
				for (int num3 = 0; num3 < this.LeftLowerArm.Count; num3++)
				{
					GameObject gameObject9 = this.LeftLowerArm[num3];
					if (gameObject9)
					{
						gameObject9.SetActive(false);
					}
				}
				return;
			}
			if (bodyPart != EnumBodyPartHit.RightLowerArm)
			{
				return;
			}
			for (int num4 = 0; num4 < this.RightLowerArm.Count; num4++)
			{
				GameObject gameObject10 = this.RightLowerArm[num4];
				if (gameObject10)
				{
					gameObject10.SetActive(false);
				}
			}
			return;
		}
		else
		{
			if (bodyPart == EnumBodyPartHit.LeftLowerLeg)
			{
				for (int num5 = 0; num5 < this.LeftLowerLeg.Count; num5++)
				{
					GameObject gameObject11 = this.LeftLowerLeg[num5];
					if (gameObject11)
					{
						gameObject11.SetActive(false);
					}
				}
				return;
			}
			if (bodyPart != EnumBodyPartHit.RightLowerLeg)
			{
				return;
			}
			for (int num6 = 0; num6 < this.RightLowerLeg.Count; num6++)
			{
				GameObject gameObject12 = this.RightLowerLeg[num6];
				if (gameObject12)
				{
					gameObject12.SetActive(false);
				}
			}
			return;
		}
	}

	[SerializeField]
	[PublicizedFrom(EAccessModifier.Private)]
	public List<GameObject> LeftLowerArm = new List<GameObject>();

	[SerializeField]
	[PublicizedFrom(EAccessModifier.Private)]
	public List<GameObject> LeftUpperArm = new List<GameObject>();

	[SerializeField]
	[PublicizedFrom(EAccessModifier.Private)]
	public List<GameObject> LeftLowerLeg = new List<GameObject>();

	[SerializeField]
	[PublicizedFrom(EAccessModifier.Private)]
	public List<GameObject> LeftUpperLeg = new List<GameObject>();

	[SerializeField]
	[PublicizedFrom(EAccessModifier.Private)]
	public List<GameObject> RightLowerArm = new List<GameObject>();

	[SerializeField]
	[PublicizedFrom(EAccessModifier.Private)]
	public List<GameObject> RightUpperArm = new List<GameObject>();

	[SerializeField]
	[PublicizedFrom(EAccessModifier.Private)]
	public List<GameObject> RightLowerLeg = new List<GameObject>();

	[SerializeField]
	[PublicizedFrom(EAccessModifier.Private)]
	public List<GameObject> RightUpperLeg = new List<GameObject>();
}
