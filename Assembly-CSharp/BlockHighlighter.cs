using System;
using UnityEngine;

public static class BlockHighlighter
{
	public static void AddBlock(Vector3i _pos)
	{
		BlockHighlighter.EnforceGo();
		BlockHighlighter.EnforceTemplateLoaded();
		Object.Instantiate<GameObject>(BlockHighlighter.blockPrefab, BlockHighlighter.topGameObject.transform).transform.position = _pos.ToVector3() + BlockHighlighter.halfBlockOffset;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void EnforceGo()
	{
		if (BlockHighlighter.topGameObject != null)
		{
			return;
		}
		BlockHighlighter.topGameObject = new GameObject("BlockHighlighter");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void EnforceTemplateLoaded()
	{
		if (BlockHighlighter.blockPrefab != null)
		{
			return;
		}
		BlockHighlighter.blockPrefab = DataLoader.LoadAsset<GameObject>("@:Entities/Misc/block_highlightPrefab.prefab", false);
	}

	public static void Cleanup()
	{
		if (BlockHighlighter.topGameObject != null)
		{
			Object.Destroy(BlockHighlighter.topGameObject);
			BlockHighlighter.topGameObject = null;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const string TemplatePath = "@:Entities/Misc/block_highlightPrefab.prefab";

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Vector3 halfBlockOffset = new Vector3(0.5f, 0.5f, 0.5f);

	[PublicizedFrom(EAccessModifier.Private)]
	public static GameObject topGameObject;

	[PublicizedFrom(EAccessModifier.Private)]
	public static GameObject blockPrefab;
}
