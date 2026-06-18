using System;
using UnityEngine;

[RequireComponent(typeof(UIInput))]
[AddComponentMenu("NGUI/Examples/Chat Input")]
public class ChatInput : MonoBehaviour
{
	[PublicizedFrom(EAccessModifier.Private)]
	public void Start()
	{
		this.mInput = base.GetComponent<UIInput>();
		this.mInput.label.maxLineCount = 1;
		if (this.fillWithDummyData && this.textList != null)
		{
			for (int i = 0; i < 30; i++)
			{
				this.textList.Add(((i % 2 == 0) ? "[FFFFFF]" : "[AAAAAA]") + "This is an example paragraph for the text list, testing line " + i.ToString() + "[-]");
			}
		}
	}

	public void OnSubmit()
	{
		if (this.textList != null)
		{
			string text = NGUIText.StripSymbols(this.mInput.value);
			if (!string.IsNullOrEmpty(text))
			{
				this.textList.Add(text);
				this.mInput.value = "";
				this.mInput.isSelected = false;
			}
		}
	}

	public UITextList textList;

	public bool fillWithDummyData;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public UIInput mInput;
}
