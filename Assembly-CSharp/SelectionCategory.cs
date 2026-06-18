using System;
using System.Collections.Generic;
using UnityEngine;

public class SelectionCategory
{
	public SelectionCategory(string _name, Transform _transform, Color _colActive, Color _colInactive, Color _colFaceSelected, bool _bCollider, string _tag, ISelectionBoxCallback _boxCallbacks, int _layer = 0)
	{
		this.name = _name;
		this.LocalizedName = Localization.Get("selectionCategory" + this.name, false, null);
		this.transform = _transform;
		this.colActive = _colActive;
		this.colInactive = _colInactive;
		this.colFaceSelected = _colFaceSelected;
		this.bCollider = _bCollider;
		this.tag = _tag;
		this.BoxCallbacks = _boxCallbacks;
		this.layer = _layer;
	}

	public void SetCallback(ISelectionBoxCallback _callback)
	{
		this.BoxCallbacks = _callback;
	}

	public bool IsVisible()
	{
		return this.transform.gameObject.activeSelf;
	}

	public void SetVisible(bool _bVisible)
	{
		this.transform.gameObject.SetActive(_bVisible);
		ISelectionCategoryCallback categoryCallbacks = this.CategoryCallbacks;
		if (categoryCallbacks != null)
		{
			categoryCallbacks.OnSelectionCategoryVisibilityChanged(this, _bVisible);
		}
		if (!_bVisible)
		{
			SelectionBox selection = SelectionBoxManager.Instance.Selection;
			if (((selection != null) ? selection.Category : null) == this)
			{
				SelectionBoxManager.Instance.Deactivate();
			}
		}
	}

	public void SetCaptionVisibility(bool _visible)
	{
		foreach (KeyValuePair<string, SelectionBox> keyValuePair in this.boxes)
		{
			keyValuePair.Value.SetCaptionVisibility(_visible);
		}
	}

	public void Clear()
	{
		foreach (KeyValuePair<string, SelectionBox> keyValuePair in this.boxes)
		{
			Object.Destroy(keyValuePair.Value.gameObject);
		}
		this.boxes.Clear();
		ISelectionCategoryCallback categoryCallbacks = this.CategoryCallbacks;
		if (categoryCallbacks == null)
		{
			return;
		}
		categoryCallbacks.OnSelectionCategoryCleared(this);
	}

	public SelectionBox AddBox(string _name, Vector3i _pos, Vector3i _size, bool _bDrawDirection = false, bool _bAlwaysDrawDirection = false)
	{
		SelectionBox selectionBox;
		if (this.boxes.TryGetValue(_name, out selectionBox))
		{
			this.RemoveBox(_name);
		}
		Transform transform = new GameObject().transform;
		transform.parent = this.transform;
		SelectionBox selectionBox2 = transform.gameObject.AddComponent<SelectionBox>();
		selectionBox2.name = _name;
		selectionBox2.SetOwner(this);
		selectionBox2.SetAllFacesColor(this.colInactive, true);
		selectionBox2.bDrawDirection = _bDrawDirection;
		selectionBox2.bAlwaysDrawDirection = _bAlwaysDrawDirection;
		selectionBox2.SetPositionAndSize(_pos, _size);
		if (this.bCollider)
		{
			selectionBox2.EnableCollider(this.tag, this.layer);
		}
		this.boxes[_name] = selectionBox2;
		ISelectionCategoryCallback categoryCallbacks = this.CategoryCallbacks;
		if (categoryCallbacks != null)
		{
			categoryCallbacks.OnSelectionCategoryBoxAdded(selectionBox2);
		}
		return selectionBox2;
	}

	public SelectionBox GetBox(string _name)
	{
		SelectionBox selectionBox;
		this.boxes.TryGetValue(_name, out selectionBox);
		return selectionBox;
	}

	public bool TryGetBox(string _name, out SelectionBox _box)
	{
		return this.boxes.TryGetValue(_name, out _box);
	}

	public void RenameBox(SelectionBox _box, string _newName)
	{
		if (_box.name.Equals(_newName))
		{
			return;
		}
		string text = _box.name;
		_box.name = _newName;
		this.boxes[_newName] = _box;
		this.boxes.Remove(text);
	}

	public void RemoveBox(SelectionBox _box)
	{
		if (SelectionBoxManager.Instance.Selection == _box)
		{
			SelectionBoxManager.Instance.Deactivate();
		}
		ISelectionCategoryCallback categoryCallbacks = this.CategoryCallbacks;
		if (categoryCallbacks != null)
		{
			categoryCallbacks.OnSelectionCategoryBoxRemoved(_box);
		}
		Object.Destroy(_box.gameObject);
		this.boxes.Remove(_box.name);
	}

	public void RemoveBox(string _name)
	{
		SelectionBox selectionBox;
		if (!this.boxes.TryGetValue(_name, out selectionBox))
		{
			return;
		}
		this.RemoveBox(selectionBox);
	}

	public override string ToString()
	{
		return "Cat " + this.name;
	}

	public readonly string name;

	public readonly string LocalizedName;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Transform transform;

	public readonly Color colActive;

	public readonly Color colInactive;

	public readonly Color colFaceSelected;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly bool bCollider;

	public readonly string tag;

	public ISelectionBoxCallback BoxCallbacks;

	public ISelectionCategoryCallback CategoryCallbacks;

	public Action CheckKeysCallback;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly int layer;

	public readonly Dictionary<string, SelectionBox> boxes = new Dictionary<string, SelectionBox>();
}
