using UnityEngine;
using UnityEngine.UI;
using System;

[Serializable]
public class DropdownItem
{
	[SerializeField]
	internal string _caption;
	public string Caption
	{
		get
		{
			return _caption;
		}
		set
		{
			_caption = value;
			if (OnUpdate != null)
				OnUpdate();
		}
	}

	[SerializeField]
	internal bool _selected;
	public bool Selected
	{
		get
		{
			return _selected;
		}
		set
		{
			if (_selected != value)
			{
				_selected = value;
				if (OnSelect != null)
					OnSelect();

				if (OnUpdate != null)
					OnUpdate();
			}
		}
	}

	[SerializeField]
	internal bool _isDisabled;
	public bool IsDisabled
	{
		get
		{
			return _isDisabled;
		}
		set
		{
			_isDisabled = value;
			if (OnUpdate != null)
				OnUpdate();
		}
	}

	internal Action OnSelect;
	internal Action OnUpdate;

	public DropdownItem(string caption)
	{
		_caption = caption;
	}

	public DropdownItem(string caption, bool disabled)
	{
		_caption = caption;
		_isDisabled = disabled;
	}

	public DropdownItem(bool disabled)
	{
		_isDisabled = disabled;
	}
}
