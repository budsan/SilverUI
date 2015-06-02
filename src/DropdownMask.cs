using UnityEngine;
using System;
using System.Collections.Generic;

namespace Silver
{
	namespace UI
	{
		[RequireComponent(typeof(Dropdown))]
		public class DropdownMask : MonoBehaviour
		{
			private int mask = 0;
			public int Mask
			{
				get
				{
					return mask;
				}
				set
				{
					SetValue(value);
				}
			}

			public Action OnChanged;
			private int[] indexToValue = new int[0];
			private Type typeEnumSetup = null;
			
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
				
				DropdownItem[] items = Dropdown.Items;
				if (Dropdown.SelectedItem == 0)
				{
					SetValue(indexToValue[0]);
				}
				else if (Dropdown.SelectedItem == 1)
				{
					SetValue(indexToValue[1]);
				}
				else
				{
					int newMask = 0;
					for (int i = 2; i < items.Length; i++)
					{
						DropdownItem item = items[i];
						if (item.Selected)
						{
							int value = indexToValue[i];
							newMask |= value;
						}
					}

					SetValue(newMask);
				}
			}

			public void SetupEnum(Type typeEnum)
			{
				if (!typeEnum.IsEnum)
				{
					Debug.LogWarning("DropdownMask: type must be an enum type");
					return;
				}

				List<DropdownItem> items = new List<DropdownItem>();
				List<int> index = new List<int>();

				index.Add(0);
				items.Add(new DropdownItem("Nothing"));

				index.Add(0);
				items.Add(new DropdownItem("Everything"));

				int allMask = 0;
				foreach (Enum value in Enum.GetValues(typeEnum))
				{
					int valueMask = Convert.ToInt32(value);
					allMask |= valueMask;

					index.Add(valueMask);
					items.Add(new DropdownItem(Enum.GetName(typeEnum, value)));
				}

				index[1] = allMask;

				typeEnumSetup = typeEnum;
				Dropdown.Multiselection = true;
				Dropdown.Items = items.ToArray();
				indexToValue = index.ToArray();
			}
			
			void SetValue(int newMask)
			{
				if (typeEnumSetup == null)
				{
					Debug.LogWarning("DropdownMask: Setup first a Enum mask type");
					return;
				}

				DropdownItem[] items = Dropdown.Items;
				items[0]._selected = (newMask == 0); //Nothing
				for (int i = 1; i < items.Length; i++)
				{
					DropdownItem item = items[i];
					int currentItemMask = indexToValue[i];
					item._selected = (newMask & currentItemMask) == currentItemMask;
				}

				if (items[1].Selected)
					Dropdown.MultiText = "Everything";
				else
					Dropdown.MultiText = "Mixed ...";

				mask = newMask;
				Dropdown.RefreshItems();

				if (OnChanged != null)
					OnChanged();
			}
		}
	}
}


