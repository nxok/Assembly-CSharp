using System;
using System.Collections.Generic;
using UnityEngine;

public static class LandClaimBoundsHelper
{
	[PublicizedFrom(EAccessModifier.Private)]
	public static LandClaimBoundsHelper.BoundsHelperEntry GetEntryFromList(Vector3 _worldPos)
	{
		for (int i = 0; i < LandClaimBoundsHelper.list.Count; i++)
		{
			if (LandClaimBoundsHelper.list[i].Position == _worldPos)
			{
				return LandClaimBoundsHelper.list[i];
			}
		}
		return null;
	}

	public static void RemoveBoundsHelper(Vector3 _worldPos)
	{
		Transform transform = null;
		for (int i = 0; i < LandClaimBoundsHelper.list.Count; i++)
		{
			if (LandClaimBoundsHelper.list[i].Position == _worldPos)
			{
				LandClaimBoundsHelper.list[i].Remove();
				transform = LandClaimBoundsHelper.list[i].Helper;
				LandClaimBoundsHelper.list.RemoveAt(i);
			}
		}
		if (transform != null)
		{
			transform.parent = LandClaimBoundsHelper.goPool;
			transform.localPosition = Vector3.zero;
			transform.gameObject.SetActive(false);
		}
	}

	public static Transform GetBoundsHelper(Vector3 _worldPos)
	{
		if (LandClaimBoundsHelper.goRoot == null)
		{
			LandClaimBoundsHelper.InitHelpers();
		}
		LandClaimBoundsHelper.BoundsHelperEntry entryFromList = LandClaimBoundsHelper.GetEntryFromList(_worldPos);
		Transform transform;
		if (entryFromList != null)
		{
			transform = entryFromList.Helper;
		}
		else
		{
			if (LandClaimBoundsHelper.goPool.childCount > 0)
			{
				transform = LandClaimBoundsHelper.goPool.GetChild(0);
				transform.parent = LandClaimBoundsHelper.goRoot;
			}
			else
			{
				List<EntityPlayerLocal> localPlayers = GameManager.Instance.World.GetLocalPlayers();
				if (localPlayers == null || localPlayers.Count <= 0)
				{
					return null;
				}
				NGuiWdwInGameHUD inGameHUD = LocalPlayerUI.GetUIForPlayer(localPlayers[0]).nguiWindowManager.InGameHUD;
				GameObject gameObject = new GameObject("LandClaimBoundary");
				GameObject gameObject2 = GameObject.CreatePrimitive(3);
				Object.Destroy(gameObject2.GetComponent<BoxCollider>());
				gameObject2.transform.parent = gameObject.transform;
				gameObject2.transform.localScale = Vector3.one;
				gameObject2.transform.localPosition = Vector3.zero;
				gameObject2.transform.localRotation = Quaternion.identity;
				Renderer component = gameObject2.GetComponent<Renderer>();
				Material material = Resources.Load("Materials/LandClaimBoundary", typeof(Material)) as Material;
				component.material = material;
				transform = gameObject.transform;
				transform.transform.parent = LandClaimBoundsHelper.goRoot;
			}
			Vector3 one = Vector3.one;
			transform.localPosition = new Vector3(0.5f, 0.01f, 0.5f);
			float num = (float)GameStats.GetInt(EnumGameStats.LandClaimSize);
			transform.localScale = new Vector3(num, num * 10000f, num);
			transform.localPosition = _worldPos - Origin.position + new Vector3(0.5f, 0.5f, 0.5f);
			LandClaimBoundsHelper.list.Add(new LandClaimBoundsHelper.BoundsHelperEntry(_worldPos, transform));
		}
		return transform;
	}

	public static void InitHelpers()
	{
		if (LandClaimBoundsHelper.goRoot == null)
		{
			LandClaimBoundsHelper.goRoot = new GameObject("LandClaimHelpers").transform;
			LandClaimBoundsHelper.goPool = new GameObject("Pool").transform;
			LandClaimBoundsHelper.goPool.parent = LandClaimBoundsHelper.goRoot;
			LandClaimBoundsHelper.goPool.localPosition = new Vector3(9999f, 9999f, 9999f);
		}
	}

	public static void CleanupHelpers()
	{
		for (int i = 0; i < LandClaimBoundsHelper.list.Count; i++)
		{
			LandClaimBoundsHelper.list[i].Remove();
			Object.Destroy(LandClaimBoundsHelper.list[i].Helper);
		}
		LandClaimBoundsHelper.list.Clear();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const string landClaimBoundaryMaterialPath = "Materials/LandClaimBoundary";

	[PublicizedFrom(EAccessModifier.Private)]
	public static Transform goRoot;

	[PublicizedFrom(EAccessModifier.Private)]
	public static Transform goPool;

	public static List<LandClaimBoundsHelper.BoundsHelperEntry> list = new List<LandClaimBoundsHelper.BoundsHelperEntry>();

	public class BoundsHelperEntry
	{
		public BoundsHelperEntry(Vector3 _position, Transform _helper)
		{
			this.Position = _position;
			this.Helper = _helper;
			Origin.OriginChanged = (Action<Vector3>)Delegate.Combine(Origin.OriginChanged, new Action<Vector3>(this.OnOriginChanged));
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void OnOriginChanged(Vector3 _newOrigin)
		{
			this.Helper.localPosition = this.Position - Origin.position + new Vector3(0.5f, 0.5f, 0.5f);
		}

		public void Remove()
		{
			Origin.OriginChanged = (Action<Vector3>)Delegate.Remove(Origin.OriginChanged, new Action<Vector3>(this.OnOriginChanged));
		}

		public Vector3 Position;

		public Transform Helper;
	}
}
