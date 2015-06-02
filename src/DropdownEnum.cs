using UnityEngine;
using System;
using System.Collections.Generic;

namespace Silver
{
	namespace UI
	{
		[RequireComponent(typeof(Dropdown))]
		public class DropdownEnum : MonoBehaviour
		{
			private Enum currentEnum = null;
			public Enum CurrentEnum
			{
				get
				{
					return currentEnum;
				}
				set
				{
					SetEnum(value);
				}
			}

			public Action OnChanged;

			private Enum[] indexToValue = new Enum[0];
			private Dictionary<Enum, int> ValueToIndex = new Dictionary<Enum,int>();

			private Dropdown dropdown = null;
			public Dropdown Dropdown
			{
				get
				{
					if (dropdown == null)
					{
						dropdown = gameObject.GetComponent<Dropdown>();
						if (dropdown != null)
							dropdown.OnSelectedChanged = OnSelectedChanged;
					}

					return dropdown;
				}
			}

			void OnSelectedChanged()
			{
				if (Dropdown != null)
				{
					SetEnum(Dropdown.SelectedItem);
				}
			}

			void SetupEnum(Enum enumValue)
			{
				List<DropdownItem> items = new List<DropdownItem>();
				List<Enum> index = new List<Enum>();
				ValueToIndex.Clear();

				Type typeEnum = enumValue.GetType();
				foreach (Enum value in Enum.GetValues(typeEnum))
				{
					index.Add(value);
					ValueToIndex.Add(value, items.Count);
					items.Add(new DropdownItem(Enum.GetName(typeEnum, value)));
				}

				Dropdown.Items = items.ToArray();
				indexToValue = index.ToArray();
			}

			void SetEnum(int index)
			{
				Enum value = indexToValue[index];
				if (value != currentEnum)
				{
					currentEnum = value;

					if (OnChanged != null)
						OnChanged();
				}
			}
			
			void SetEnum(Enum enumValue)
			{
				if (currentEnum == null || currentEnum.GetType() != enumValue.GetType())
					SetupEnum(enumValue);

				int index = ValueToIndex[(Enum) enumValue];
				Dropdown.Items[index].Selected = true;
			}
		}
	}
}


