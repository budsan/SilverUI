using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

namespace Silver
{
	namespace UI
	{
		[RequireComponent(typeof(RectTransform))]
		public class Dropdown : MonoBehaviour
		{
			public string NoneText = "None";
			public string MultiText = "Multi";

			public GameObject OverlayParent = null;

			[SerializeField]
			private bool _multiselection = false;
			public bool Multiselection
			{
				get
				{
					return _multiselection;
				}
				set
				{
					_multiselection = value;
					RefreshItems();
				}
			}

			[SerializeField]
			private int _itemsToDisplay = 8;
			public int ItemsToDisplay
			{
				get
				{
					return _itemsToDisplay;
				}
				set
				{
					if (_itemsToDisplay == value)
						return;

					_itemsToDisplay = value;
					ResizeDropdown();
				}
			}

			[SerializeField]
			private int _lastSelectedItem = 0;
			public int SelectedItem
			{
				get
				{
					return _lastSelectedItem;
				}
				set
				{
					_lastSelectedItem = value;

					if (_lastSelectedItem < 0 || _lastSelectedItem >= Items.Length)
						_lastSelectedItem = 0;

					RefreshItems();

					if (OnSelectedChanged != null)
						OnSelectedChanged();
				}
			}

			public Action OnSelectedChanged;

			[SerializeField]
			private DropdownItem[] _items;
			public DropdownItem[] Items
			{
				get
				{
					if (_items == null)
						_items = new DropdownItem[0];
					return _items;
				}
				set
				{
					_items = value;
					ResizeDropdown();
					RefreshItems();
				}
			}

			private GameObject goDropButton;
			private GameObject goDropdown;
			private GameObject goScrollbar;
			private GameObject goScrollPanel;
			private GameObject goItems;

			private Text DropdownText;

			private const string NameButton = "Button";
			private const string NameArrow = "Arrow";
			private const string NameDropdown = "Dropdown";
			private const string NameScrollBar = "Scrollbar";
			private const string NameScrollPanel = "ScrollPanel";
			private const string NameSlidingArea = "SlidingArea";
			private const string NameItems = "Items";
			private const string NameItemsCheckmark = "Checkmark";

			private void Start()
			{
				Validate();
			}

			void OnValidate()
			{
				Validate();
			}

			private void ResizeDropdown()
			{
				int display = ItemsToDisplay < Items.Length ? ItemsToDisplay : Items.Length;
				display = display > 0 ? display : 1;

				if (goDropdown == null)
					Validate();

				Helper.SetRectTransform(goDropdown, 0, 0, 1, 0, 0.5f, 1, 0, (display * Helper.FontSizeWithMargins()) + 8.0f, 0, 0);
				if (display < Items.Length)
				{
					goScrollbar.SetActive(true);
					Helper.SetRectTransform(goScrollPanel, 0, 0, 1, 1, 0, 0.5f, -24.0f, -4.0f, 2.0f, 0.0f);
				}
				else
				{
					goScrollbar.SetActive(false);
					Helper.SetRectTransform(goScrollPanel, 0, 0, 1, 1, 0, 0.5f, -2.0f, -4.0f, 2.0f, 0.0f);
				}
			}

			private void RebuildItem(GameObject go)
			{
				Helper.SetRectTransform(go, 0, 0, 1, 1, 0.5f, 0.5f, 0, 0, 0, 0);
				Button button = Helper.AddButton(go, "");

				ColorBlock block = button.colors;
				block.pressedColor = new Color(0.5f, 0.75f, 1.0f);
				block.highlightedColor = new Color(0.8f, 0.9f, 1.0f);
				button.colors = block;

				Image image = go.GetComponent<Image>();
				image.sprite = null;

				GameObject goText = go.transform.Find(Helper.NameButtonText).gameObject;
				Helper.SetRectTransform(goText, 0, 0, 1, 1, 0, 0.5f, -30.0f, 0, Helper.FontSize() + 4.0f, 0);
				{
					Text text = goText.GetComponent<Text>();
					text.alignment = TextAnchor.MiddleLeft;
				}

				GameObject goCheckmark = Helper.FindOrCreateUI(NameItemsCheckmark, go, (string name, GameObject parent) =>
				{
					GameObject checkmark = Helper.CreateGUIGameObject(name, parent);
					float size = Helper.FontSize();
					Helper.SetRectTransform(checkmark, 0, 0.5f, 0, 0.5f, 0, 0.5f, size, size, 4, 0);

					Image imageCheckmark = checkmark.AddComponent<Image>();
					imageCheckmark.sprite = Resources.Instance.GetSpriteCheckmark();

					return checkmark;
				});
			}

			delegate T ValidateDelegate<T>();
			private T ValidateItem<T>(GameObject go, ValidateDelegate<T> validate)
			{
				T inst = validate();
				if (inst == null)
				{
					RebuildItem(go);
					inst = validate();
				}

				return inst;
			}

			private void ValidateItem(GameObject go, DropdownItem item, int id)
			{
				Button button = ValidateItem<Button>(go, () => {
					return go.GetComponent<Button>();
				});

				Transform textTransform = ValidateItem<Transform>(go, () => {
					return go.transform.Find(Helper.NameButtonText);
				});

				Text label = ValidateItem<Text>(go, () => {
					return textTransform.gameObject.GetComponent<Text>();
				});

				Transform checkmark = ValidateItem<Transform>(go, () =>
				{
					return go.transform.Find(NameItemsCheckmark);
				});

				DropdownItem captureItem = item;
				button.onClick.RemoveAllListeners();
				button.onClick.AddListener(() =>
				{
					captureItem.Selected = !captureItem.Selected;
					ToggleDropdown();
				});

				int capturedId = id;
				item.OnSelect = null;
				item.OnSelect = delegate()
				{
					SelectedItem = capturedId;
				};

				item.OnUpdate = null;
				item.OnUpdate = delegate()
				{
					RefreshItems();
				};

				if (!Multiselection)
				{
					if (SelectedItem != id)
						item._selected = false;
					else
						item._selected = true;
				}

				if (item._selected)
				{
					if (DropdownText.text == "")
						DropdownText.text = item._caption;
					else
						DropdownText.text = MultiText;
				}

				go.name = "Item " + item._caption;
				button.interactable = !item._isDisabled;
				label.text = item._caption;
				checkmark.gameObject.SetActive(item._selected);
			}

			internal void RefreshItems()
			{
				if (goItems == null || DropdownText == null)
					return;

				DropdownText.text = "";
				RectTransform rect = goItems.GetComponent<RectTransform>();
				rect.sizeDelta = new Vector2(0, Items.Length * Helper.FontSizeWithMargins());

				int countElements = 0;
				foreach (Transform t in goItems.transform)
				{
					if (countElements < Items.Length)
					{
						ValidateItem(t.gameObject, Items[countElements], countElements);
						countElements++;
					}
					else
					{
						Helper.DestroyUI(t.gameObject);
					}
						
				}

				while (countElements < Items.Length)
				{
					GameObject item = Helper.CreateGUIGameObject("Item", goItems);
					ValidateItem(item, Items[countElements], countElements);
					countElements++;
				}

				if (DropdownText.text == "")
					DropdownText.text = NoneText;
			}

			private void SetDropdown(bool enable, GameObject pointerPress)
			{
				if (goDropButton != pointerPress &&
					goDropdown != null)
				{
					DropdownChangeTo(enable);
				}
					
			}

			private void ToggleDropdown()
			{
				if (goDropdown != null)
				{
					bool enable = !goDropdown.activeSelf;
					DropdownChangeTo(enable);
				}
			}

			private void DropdownChangeTo(bool enable)
			{
				goDropdown.SetActive(enable);
				
				if (enable && OverlayParent != null)
					goDropdown.transform.SetParent(OverlayParent.transform);
				else
				{
					goDropdown.transform.SetParent(gameObject.transform);
					ResizeDropdown();
				}
			}

			private void Validate()
			{
				goDropButton = Helper.FindOrCreateUI(NameButton, gameObject, (string name, GameObject parent) =>
				{
					GameObject dropButton = Helper.CreateButton(parent, name, "Item 1");
					RectTransform DropButtonRect = dropButton.GetComponent<RectTransform>();

					GameObject dropText = DropButtonRect.Find(Helper.NameButtonText).gameObject;
					RectTransform DropTextRect = Helper.SetRectTransform(dropText, 0, 0, 1, 1, 0, 0.5f, -30.0f, 0, 10.0f, 0);
					{
						Text text = dropText.GetComponent<Text>();
						text.color = Helper.ColorText;
						text.alignment = TextAnchor.MiddleLeft;
						DropdownText = text;
					}

					ColorBlock block = new ColorBlock();
					block.normalColor = Color.white;
					block.highlightedColor = new Color32(245, 245, 245, 255);
					block.pressedColor = new Color32(200, 200, 200, 255);
					block.disabledColor = new Color32(200, 200, 200, 128);
					block.colorMultiplier = 1.0f;
					block.fadeDuration = 0.1f;

					Button buttonDropButton = dropButton.GetComponent<Button>();
					buttonDropButton.colors = block;

					return dropButton;
				});

				GameObject goDropText = Helper.FindOrCreateUI(Helper.NameButtonText, goDropButton, (string name, GameObject parent) =>
				{
					GameObject dropText = Helper.CreateGUIGameObject(name, parent);
					RectTransform DropTextRect = Helper.SetRectTransform(dropText, 0, 0, 1, 1, 0, 0.5f, -30.0f, 0, 10.0f, 0);
					{
						Text text = dropText.AddComponent<Text>();
						text.font = Resources.Instance.GetFontContent();
						text.color = Helper.ColorText;
						text.alignment = TextAnchor.MiddleLeft;
					}

					return dropText;
				});

				DropdownText = goDropText.GetComponent<Text>();

				GameObject goDropArrow = Helper.FindOrCreateUI(NameArrow, goDropButton, (string name, GameObject parent) =>
				{
					GameObject dropArrow = Helper.CreateGUIGameObject(name, parent);
					RectTransform DropArrowRect = Helper.SetRectTransform(dropArrow, 1, 0.5f, 1, 0.5f, 1, 0.5f, 0, 0, -8, 0);
					Text text = Helper.AddText(dropArrow, "▼");
					text.alignment = TextAnchor.MiddleCenter;

					CanvasGroup group = dropArrow.AddComponent<CanvasGroup>();
					group.interactable = false;
					group.blocksRaycasts = false;

					ContentSizeFitter fitter = dropArrow.AddComponent<ContentSizeFitter>();
					fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
					fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

					return dropArrow;
				});

				goDropdown = Helper.FindOrCreateUI(NameDropdown, gameObject, (string name, GameObject parent) =>
				{
					GameObject dropdown = Helper.CreateGUIGameObject(name, parent);
					dropdown.SetActive(false);

					Image image = dropdown.AddComponent<Image>();
					image.sprite = Resources.Instance.GetSpriteField();
					image.type = Image.Type.Sliced;

					dropdown.AddComponent<ClickFencer>();
					return dropdown;
				});

				Button.ButtonClickedEvent clickEvent = goDropButton.GetComponent<Button>().onClick;
				clickEvent.RemoveListener(ToggleDropdown);
				clickEvent.AddListener(ToggleDropdown);

				ClickFencer fencerDropDown = goDropdown.GetComponent<ClickFencer>();
				fencerDropDown.OnClick -= SetDropdown;
				fencerDropDown.OnClick += SetDropdown;

				goScrollbar = Helper.FindOrCreateUI(NameScrollBar, goDropdown, (string name, GameObject parent) =>
				{
					GameObject vertscrollbar = Helper.CreateGUIGameObject(name, parent);
					Helper.SetRectTransform(vertscrollbar, 1, 0, 1, 1, 1, 1, 20.0f, 0.0f, 0.0f, 0.0f);
					Scrollbar scrollbar = Helper.AddScrollbar(vertscrollbar);
					scrollbar.direction = Scrollbar.Direction.BottomToTop;

					return vertscrollbar;
				});

				goScrollPanel = Helper.FindOrCreateUI(NameScrollPanel, goDropdown, (string name, GameObject parent) =>
				{
					GameObject scrollPanel = Helper.CreateGUIGameObject(name, parent);
					Helper.SetRectTransform(scrollPanel, 0, 0, 1, 1, 0, 0.5f, -24.0f, -4.0f, 2.0f, 0.0f);
					scrollPanel.AddComponent<Image>();
					Mask mask = scrollPanel.AddComponent<Mask>();
					mask.showMaskGraphic = false;

					return scrollPanel;
				});

				ResizeDropdown();

				GameObject goSlidingArea = Helper.FindOrCreateUI(NameSlidingArea, goScrollPanel, (string name, GameObject parent) =>
				{
					GameObject slidingArea = Helper.CreateGUIGameObject(name, parent);
					Helper.SetRectTransform(slidingArea, 0, 0, 1, 1, 0, 0, 0, 0, 0, 0);
					ScrollRect scroll = slidingArea.AddComponent<ScrollRect>();
					scroll.verticalScrollbar = goScrollbar.GetComponent<Scrollbar>();
					scroll.horizontal = false;
					scroll.scrollSensitivity = 8.0f;
					return slidingArea;
				});

				goItems = Helper.FindOrCreateUI(NameItems, goSlidingArea, (string name, GameObject parent) =>
				{
					GameObject items = Helper.CreateGUIGameObject(name, parent);
					ScrollRect scroll = goSlidingArea.GetComponent<ScrollRect>();
					scroll.content = Helper.SetRectTransform(items, 0, 1, 1, 1, 0, 1, 0, 0, 0, 0);
					VerticalLayoutGroup layoutGroup = items.AddComponent<VerticalLayoutGroup>();

					return items;
				});

				RefreshItems();
			}
		}
	}
}