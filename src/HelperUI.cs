using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;

namespace Silver
{
	namespace UI
	{
		public static class Helper
		{
			public static class Scaler
			{
				static private float currentScale = -1.0f;
				static public void SetScaleFromDPI()
				{
					int dpi = (int)Screen.dpi;
					if (dpi == 0)
						dpi = 96;

					currentScale = dpi / 96.0f;
				}

				static public float CurrentScale
				{
					get
					{
						if (currentScale < 0.0f)
							SetScaleFromDPI();

						return currentScale;
					}
					set
					{
						currentScale = value;
					}
				}
			}

			static public int FontSize(int fontSize = 16) { return fontSize; }
			static public float FontSizeWithMargins(int fontSize = 16) { return FontSize(fontSize) * (7f / 4f); }

			//-------------------------------------------------------------//

			public static Color ColorText = new Color(0.1f, 0.1f, 0.1f);
			public static GameObject CreateText(GameObject parent, string name, string content = "")
			{
				GameObject goText = CreateGUIGameObject(name, parent);
				SetRectTransform(goText, 0, 0, 1, 1, 0.5f, 0.5f, 0, 0, 0, 0);
				AddText(goText, content);
				return goText;
			}

			public static Text AddText(GameObject goText, string content = "")
			{
				Text text = goText.AddOrGetComponent<Text>();
				text.fontSize = FontSize();
				text.font = Resources.Instance.GetFontContent();
				text.alignment = TextAnchor.MiddleCenter;
				text.color = ColorText;
				text.text = content;

				return text;
			}

			//-------------------------------------------------------------//

			public const string NameButtonText = "Text";
			public const string NameButtonImage = "Image";

			
			public static GameObject CreateButton(GameObject parent, string name, string buttonText, UnityAction onPressed = null)
			{
				GameObject goButton = Helper.CreateGUIGameObject(name, parent);
				SetRectTransform(goButton, 0, 0, 1, 1, 0.5f, 0.5f, 0, 0, 0, 0);
				AddButton(goButton, buttonText, onPressed);
				return goButton;
			}

			public static GameObject CreateButton(GameObject parent, string name, Sprite buttonImage, UnityAction onPressed = null)
			{
				GameObject goButton = Helper.CreateGUIGameObject(name, parent);
				SetRectTransform(goButton, 0, 0, 1, 1, 0.5f, 0.5f, 0, 0, 0, 0);
				AddButton(goButton, buttonImage, onPressed);
				return goButton;
			}

			public static Button AddButton(GameObject goButton, string buttonText, UnityAction onPressed = null)
			{
				Button button = goButton.AddOrGetComponent<Button>();
				Image buttonImage = goButton.AddOrGetComponent<Image>();
				buttonImage.sprite = Resources.Instance.GetSpriteButton();
				buttonImage.type = Image.Type.Sliced;
				button.targetGraphic = buttonImage;

				if (onPressed != null)
					button.onClick.AddListener(onPressed);

				GameObject goText = FindOrCreateUI(NameButtonText, goButton, (string name, GameObject parent) =>
				{
					GameObject text =  CreateText(parent, name, (buttonText == null) ? "" : buttonText);
					SetRectTransform(text, 0, 0, 1, 1, 0.5f, 0.5f, -8, 0, 0, 0);
					return text; 
				});

				return button;
			}

			public static Button AddButton(GameObject goButton, Sprite buttonInnerImage, UnityAction onPressed = null)
			{
				Button button = goButton.AddOrGetComponent<Button>();
				Image buttonImage = goButton.AddOrGetComponent<Image>();
				buttonImage.sprite = Resources.Instance.GetSpriteButton();
				buttonImage.type = Image.Type.Sliced;
				button.targetGraphic = buttonImage;

				if (onPressed != null)
					button.onClick.AddListener(onPressed);

				GameObject goImage = FindOrCreateUI(NameButtonImage, goButton, (string name, GameObject parent) =>
				{
					GameObject img = CreateGUIGameObject(name, parent);
					Image innerImage = img.AddComponent<Image>();
					innerImage.sprite = buttonInnerImage;
					innerImage.preserveAspect = true;
					SetRectTransform(img, 0, 0, 1, 1, 0.5f, 0.5f, -8, 0, 0, 0);
					return img;
				});

				return button;
			}

			//-------------------------------------------------------------//

			public const string NameToggle = "Toggle";
			public const string NameToggleBackground = "Background";
			public const string NameToggleCheckmark = "Checkmark";

			public static GameObject CreateToggle(GameObject parent, float size = 20, bool isOn = false)
			{
				GameObject goToggle = Helper.CreateGUIGameObject(NameToggle, parent);
				SetRectTransform(goToggle, 0, 0.5f, 0, 0.5f, 0, 0.5f, size, size, 0, 0);
				AddToggle(goToggle, isOn);
				return goToggle;
			}

			public static Toggle AddToggle(GameObject goToggle, bool isOn = false)
			{
				Toggle toggle = goToggle.AddOrGetComponent<Toggle>();
				toggle.isOn = isOn;

				AspectRatioFitter fitter = goToggle.AddOrGetComponent<AspectRatioFitter>();
				fitter.aspectMode = AspectRatioFitter.AspectMode.HeightControlsWidth;
				fitter.aspectRatio = 1;

				GameObject goBackground = FindOrCreateUI(NameToggleBackground, goToggle, (string name, GameObject parent) =>
				{
					GameObject background = Helper.CreateGUIGameObject(name, parent);
					RectTransform rectBackground = SetRectTransform(background, 0, 0, 1, 1, 0.5f, 0.5f, 0, 0, 0, 0);

					Image imageBackground = background.AddOrGetComponent<Image>();
					imageBackground.sprite = Resources.Instance.GetSpriteField();
					imageBackground.type = Image.Type.Sliced;

					return background;
				});

				GameObject goCheckmark = FindOrCreateUI(NameToggleCheckmark, goBackground, (string name, GameObject parent) => 
				{
					GameObject checkmark = Helper.CreateGUIGameObject(name, parent);
					RectTransform rectCheckmark = SetRectTransform(checkmark, 0, 0, 1, 1, 0.5f, 0.5f, 0, 0, 0, 0);

					Image imageCheckmark = checkmark.AddOrGetComponent<Image>();
					imageCheckmark.sprite = Resources.Instance.GetSpriteCheckmark();

					return checkmark;
				});

				toggle.targetGraphic = goBackground.AddOrGetComponent<Image>();
				toggle.graphic = goCheckmark.AddOrGetComponent<Image>();

				return toggle;
			}

			//-------------------------------------------------------------//

			public const string NameInputFieldPlaceholder = "Placeholder";
			public const string NameInputFieldText = "Text";

			public static GameObject CreateInputField(GameObject parent, string name, string value, string placeHolder = "")
			{
				GameObject goInputField = CreateGUIGameObject(name, parent);
				SetRectTransform(goInputField, 0, 0, 1, 1, 0.5f, 0.5f, 0, 0, 0, 0);
				AddInputField(goInputField, value, placeHolder);
				return goInputField;
			}

			public static InputField AddInputField(GameObject goInputField, string value, string placeHolder = "")
			{
				InputField inField = goInputField.AddOrGetComponent<InputField>();
				{
					Image textImage = goInputField.AddOrGetComponent<Image>();
					textImage.sprite = Resources.Instance.GetSpriteField();
					textImage.type = Image.Type.Sliced;
					inField.targetGraphic = textImage;
				}

				float padding = FontSizeWithMargins() - FontSize();
				float padhalf = padding * 0.5f;

				GameObject goPlaceholder = FindOrCreateUI(NameInputFieldPlaceholder, goInputField, (string name, GameObject parent) =>
				{
					GameObject placeholder = CreateText(parent, name, placeHolder);
					Helper.SetRectTransform(placeholder, 0, 0, 1, 1, 0.5f, 1, -padding, -padhalf, 0, -padhalf);
					
					Text textPlaceholder = placeholder.GetComponent<Text>();
					textPlaceholder.color = Color.gray;
					textPlaceholder.fontSize = FontSize();
					textPlaceholder.alignment = TextAnchor.UpperLeft;

					return placeholder;
				});

				GameObject goText = FindOrCreateUI(NameInputFieldText, goInputField, (string name, GameObject parent) =>
				{
					GameObject text = CreateText(parent, name, value);
					Helper.SetRectTransform(text, 0, 0, 1, 1, 0.5f, 1, -padding, -padhalf, 0, -padhalf);
					Text textText = text.GetComponent<Text>();
					textText.supportRichText = false;
					textText.fontSize = FontSize();
					textText.alignment = TextAnchor.UpperLeft;

					return text;
				});

				inField.placeholder = goPlaceholder.AddOrGetComponent<Text>();
				inField.textComponent = goText.AddOrGetComponent<Text>();
				inField.text = value;

				return inField;
			}

			//-------------------------------------------------------------//

			public static GameObject CreateScrollView(GameObject parent, RectTransform content, bool verticalScrollbar, bool horizontalScrollbar)
			{
				GameObject goView = Helper.FindOrCreateUI("ScrollView", parent, (string _name, GameObject _parent) =>
				{
					GameObject view = Helper.CreateGUIGameObject(_name, _parent);
					Helper.SetRectTransform(view, 0, 0, 1, 0, 0.5f, 1, 0, 0, 0, 0);
					view.AddOrGetComponent<Image>();
					Mask mask = view.AddOrGetComponent<Mask>();
					mask.showMaskGraphic = false;

					LayoutElement layout = view.AddComponent<LayoutElement>();
					layout.flexibleHeight = 1.0f;
					layout.flexibleWidth = 1.0f;

					return view;
				});

				GameObject goVerticalScrollBar = null;
				GameObject goHorizontalScrollBar = null;
				if (verticalScrollbar)
				{
					goVerticalScrollBar = Helper.FindOrCreateUI("VerticalScrollbar", goView, (string _name, GameObject _parent) =>
					{
						GameObject vertscrollbar = Helper.CreateGUIGameObject(_name, _parent);
						Helper.SetRectTransform(vertscrollbar, 1, 0, 1, 1, 1, 1, 20.0f, horizontalScrollbar ? - 24.0f : -4.0f, -2.0f, -2.0f);
						Scrollbar scrollbar = Helper.AddScrollbar(vertscrollbar);
						scrollbar.direction = Scrollbar.Direction.BottomToTop;

						return vertscrollbar;
					});
				}

				if (horizontalScrollbar)
				{
					goHorizontalScrollBar = Helper.FindOrCreateUI("HorizontalScrollbar", goView, (string name, GameObject _parent) =>
					{
						GameObject horiscrollbar = Helper.CreateGUIGameObject(name, _parent);
						Helper.SetRectTransform(horiscrollbar, 0, 0, 1, 0, 0, 0, verticalScrollbar ? -24.0f : -4.0f, 20.0f, 2.0f, 2.0f);
						Helper.AddScrollbar(horiscrollbar);
						return horiscrollbar;
					});
				}

				GameObject goScroll = Helper.FindOrCreateUI("ScrollRect", goView, (string _name, GameObject _parent) =>
				{
					GameObject scroll = Helper.CreateGUIGameObject(_name, _parent);
					Helper.SetRectTransform(scroll, 0, 0, 1, 1, 0, 1, verticalScrollbar ? -24.0f : 0.0f, horizontalScrollbar ? -24.0f : 0.0f, 0, 0);
					ScrollRect scrollrect = scroll.AddOrGetComponent<ScrollRect>();
					scrollrect.scrollSensitivity = 8.0f;

					scrollrect.vertical = verticalScrollbar;
					scrollrect.horizontal = horizontalScrollbar;

					if (goVerticalScrollBar != null)
						scrollrect.verticalScrollbar = goVerticalScrollBar.GetComponent<Scrollbar>();

					if (goHorizontalScrollBar != null)
						scrollrect.horizontalScrollbar = goHorizontalScrollBar.GetComponent<Scrollbar>();

					if (content != null)
					{
						scrollrect.content = content;
						content.SetParent(scroll.transform, false);
					}
					
					return scroll;
				});
				
				return goView;
			}

			//-------------------------------------------------------------//

			public static string NameScrollbarSlidingArea = "Sliding Area";
			public static string NameScrollbarHandle = "Handle";

			public static GameObject CreateScrollBar(GameObject parent, string name, Scrollbar.Direction direction = Scrollbar.Direction.LeftToRight)
			{
				GameObject goScrollbar = CreateGUIGameObject(name, parent);
				AddScrollbar(goScrollbar, direction);

				if (direction == Scrollbar.Direction.BottomToTop || 
					direction == Scrollbar.Direction.TopToBottom)
					Helper.SetRectTransform(goScrollbar, 1, 0, 1, 1, 1, 1, 20.0f, 0.0f, 0.0f, 0.0f);
				else
					Helper.SetRectTransform(goScrollbar, 0, 0, 1, 0, 0, 0, 0.0f, 20.0f, 0.0f, 0.0f);

				return goScrollbar;
			}

			public static Scrollbar AddScrollbar(GameObject goScrollbar, Scrollbar.Direction direction = Scrollbar.Direction.LeftToRight)
			{
				Scrollbar scrollbar = goScrollbar.AddOrGetComponent<Scrollbar>();
				{
					Image image = goScrollbar.AddOrGetComponent<Image>();
					image.sprite = Resources.Instance.GetSpriteField();
					image.type = Image.Type.Sliced;
				}

				GameObject goSliding = FindOrCreateUI(NameScrollbarSlidingArea, goScrollbar, (string name, GameObject parent) =>
				{
					GameObject sliding = CreateGUIGameObject(name, parent);
					SetRectTransform(sliding, 0, 0, 1, 1, 0.5f, 0.5f, 0, 0, 0, 0);
					return sliding;
				});
				
				GameObject goHandle = FindOrCreateUI(NameScrollbarHandle, goSliding, (string name, GameObject parent) =>
				{
					GameObject handle = CreateGUIGameObject(NameScrollbarHandle, goSliding);
					RectTransform rect = handle.AddOrGetComponent<RectTransform>();
					rect.pivot = new Vector2(0.5f, 0.5f);
					rect.sizeDelta = new Vector2(0, 0);
					rect.anchoredPosition = new Vector2(0, 0);
					scrollbar.handleRect = rect;

					Image image = handle.AddOrGetComponent<Image>();
					image.sprite = Resources.Instance.GetSpriteButton();
					image.type = Image.Type.Sliced;
					return handle;
				});

				scrollbar.direction = direction;
				return scrollbar;
			}

			//-------------------------------------------------------------//

			public delegate GameObject OnNotFoundThenPopulate(string name, GameObject parent);
			static public GameObject FindOrCreateUI(string name, OnNotFoundThenPopulate populate, params GameObject[] parents)
			{
				foreach(GameObject parent in parents)
				{
					Transform child = null;
					if (parent != null)
						child = parent.transform.Find(name);
					else
					{
						GameObject goChild = GameObject.Find(name);
						child = goChild != null ? goChild.transform : null;
					}
						

					if (child != null)
					{
						return child.gameObject;
					}
				}

				if (parents.Length > 0)
				{
					return populate(name, parents[0]);
				}

				return populate(name, null);
			}

			static public GameObject FindOrCreateUI(string name, GameObject parent, OnNotFoundThenPopulate populate)
			{
				return FindOrCreateUI(name, populate, parent);
			}

			//-------------------------------------------------------------//

			static public GameObject CreateGUIGameObject(string name)
			{
				GameObject gameobject = new GameObject(name);
				gameobject.layer = LayerMask.NameToLayer("UI");

				return gameobject;
			}

			static public GameObject CreateGUIGameObject(string name, Transform parent)
			{
				GameObject gameobject = CreateGUIGameObject(name);

				if (parent != null)
					gameobject.transform.SetParent(parent, false);

				return gameobject;
			}

			static public GameObject CreateGUIGameObject(string name, GameObject parent)
			{
				GameObject gameobject = CreateGUIGameObject(name);

				if (parent != null)
					gameobject.transform.SetParent(parent.transform, false);

				return gameobject;
			}

			static public void DestroyUI(GameObject toDestroy)
			{
				GameObject objectToDestroy = toDestroy;
				if (Application.isPlaying)
				{
					GameObject.Destroy(objectToDestroy);
				}
#if UNITY_EDITOR
				// HACK this cannot go here!!!
				else UnityEditor.EditorApplication.delayCall += ()=>
				{
					GameObject.DestroyImmediate(objectToDestroy);
				};
#endif
			}
			//-------------------------------------------------------------//

			public static RectTransform SetRectTransform(
				GameObject element,
				float anchorMinX,
				float anchorMinY,
				float anchorMaxX,
				float anchorMaxY,
				float pivotX,
				float pivotY,
				float sizeDeltaX,
				float sizeDeltaY,
				float anchoredPositionX,
				float anchoredPositionY)
			{
				return SetRectTransform(
					element.AddOrGetComponent<RectTransform>(),
					anchorMinX, anchorMinY, 
					anchorMaxX, anchorMaxY, 
					pivotX, pivotY, 
					sizeDeltaX, sizeDeltaY, 
					anchoredPositionX, anchoredPositionY);
			}

			public static RectTransform SetRectTransform(
				RectTransform rect,
				float anchorMinX,
				float anchorMinY,
				float anchorMaxX,
				float anchorMaxY,
				float pivotX,
				float pivotY,
				float sizeDeltaX,
				float sizeDeltaY,
				float anchoredPositionX,
				float anchoredPositionY)
			{
				
				rect.anchorMin = new Vector2(anchorMinX, anchorMinY);
				rect.anchorMax = new Vector2(anchorMaxX, anchorMaxY);
				rect.pivot = new Vector2(pivotX, pivotY);
				rect.sizeDelta = new Vector2(sizeDeltaX, sizeDeltaY);
				rect.anchoredPosition = new Vector2(anchoredPositionX, anchoredPositionY);

				return rect;
			}
		}
	}
}
