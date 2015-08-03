using UnityEngine;

using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Silver
{
	namespace UI
	{
		[ExecuteInEditMode]
		public class WindowOverlay : MonoBehaviour
		{
			public delegate void onWindowCreated(WindowOverlay overlay);
			public static event onWindowCreated OnWindowCreated;

			[SerializeField]
			public Vector2 windowScale = new Vector2(1.0f, 1.0f);

			[SerializeField]
			private bool visible = false;
			public bool Visible
			{
				get
				{
					return visible;
				}
				set
				{
					visible = value;
				}
			}

			[SerializeField]
			private bool showTitleBar = true;
			public bool ShowTitleBar
			{
				get
				{
					return showTitleBar;
				}
				set
				{
					showTitleBar = value;
					RefreshWindowsBarVisibility();
				}
			}

			private float visibleTransition = 0.0f;
			private bool _lastIsVisible = false;
			public bool IsVisible
			{
				get
				{
					return visibleTransition > 0.0f;
				}
			}

			public Canvas Canvas
			{
				get
				{
					if (CanvasObject != null)
						return CanvasObject.GetComponent<Canvas>();

					return null;
				}
			}

			public Camera WorldCamera
			{
				get
				{
					if (CanvasObject != null)
					{
						Canvas canvas = CanvasObject.GetComponent<Canvas>();
						if (canvas != null)
							return canvas.worldCamera;
					}
					
					return null;
				}
				set
				{
					Canvas canvas = CanvasObject.GetComponent<Canvas>();
					if (canvas != null)
					{
						canvas.worldCamera = value;
						if (renderModeVR != null && IsVisible)
							renderModeVR.UpdateCanvasPosition();
					}
				}
			}

			//----------------------------//

			private bool swipeProgress = false;

			private Vector2 touch0Begin = Vector2.zero;
			private Vector2 touch1Begin = Vector2.zero;

			private Vector2 touch0End = Vector2.zero;
			private Vector2 touch1End = Vector2.zero;

			//----------------------------//

			public GameObject CanvasObject = null;
			private CanvasRenderModeVR renderModeVR = null;
			private GameObject goWindow = null;
			private GameObject goWinBar = null;
			private GameObject goTitle = null;
			private GameObject goClose = null;	
			private GameObject goContent = null;

			//----------------------------//

			public GameObject ContentContainer
			{
				get
				{
					return goContent;
				}
			}

			//----------------------------//

			private void ValidateWindow(GameObject goCanvas)
			{
				if (goWindow == null)
				{
					goWindow = Helper.FindOrCreateUI("Window", goCanvas, (string name, GameObject parent) =>
					{
						GameObject window = Helper.CreateGUIGameObject(name, parent);
						Helper.SetRectTransform(window, 0, 0, 1, 1, 0.5f, 1, 0, 0, 0, 0);
						Image img = window.AddOrGetComponent<Image>();
						img.type = Image.Type.Sliced;
						img.sprite = Resources.Instance.GetSpriteBackground();

						VerticalLayoutGroup vertical = window.AddComponent<VerticalLayoutGroup>();
						vertical.childForceExpandHeight = false;
						vertical.padding = new RectOffset(4, 4, 4, 4);
						vertical.spacing = 2;

						return window;
					});
				}
				
				goWinBar = Helper.FindOrCreateUI("WindowBar", goWindow, (string name, GameObject parent) =>
				{
					GameObject winbar = Helper.CreateGUIGameObject(name, parent);
					Helper.SetRectTransform(winbar, 0, 1, 1, 1, 0.5f, 1, 0, 0, 0, 0);
					winbar.AddOrGetComponent<CanvasRenderer>();

					Image img = winbar.AddOrGetComponent<Image>();
					img.color = new Color(0.25f, 0.5f, 1.0f);

					DraggableRect draggable = winbar.AddComponent<DraggableRect>();
					draggable.target = goWindow.GetComponent<RectTransform>();

					LayoutElement layout = winbar.AddComponent<LayoutElement>();
					layout.preferredHeight = 32.0f;

					HorizontalLayoutGroup hor = winbar.AddComponent<HorizontalLayoutGroup>();
					hor.childForceExpandHeight = false;
					hor.childForceExpandWidth = false;
					hor.childAlignment = TextAnchor.MiddleCenter;

					return winbar;
				});

				goTitle = Helper.FindOrCreateUI("WindowTitle", goWinBar, (string name, GameObject parent) =>
				{
					GameObject title = Helper.CreateGUIGameObject(name, parent);
					Helper.SetRectTransform(title, 0, 0, 1, 1, 0.5f, 0.5f, 0, 0, 0, 0);

					LayoutElement layout = title.AddComponent<LayoutElement>();
					layout.flexibleWidth = 1;

					Text text = title.AddOrGetComponent<Text>();
					text.text = "Overlay UI";
					text.alignment = TextAnchor.MiddleCenter;
					text.color = Color.white;
					text.font = Resources.Instance.GetFontTilte();
					text.fontSize = Helper.FontSize();

					return title;
				});

				goClose = Helper.FindOrCreateUI("CloseButton", (string name, GameObject parent) =>
				{
					GameObject close = Helper.CreateGUIGameObject(name, parent);
					Helper.SetRectTransform(close, 1, 0, 1, 1, 1, 0.5f, 32.0f - 4.0f, -4.0f, -2.0f, 0);
					Image img = close.AddOrGetComponent<Image>();
					img.sprite = Resources.Instance.GetSpriteButton();
					img.type = Image.Type.Sliced;

					LayoutElement layout = close.AddComponent<LayoutElement>();
					layout.preferredHeight = 32.0f;
					layout.preferredWidth = 32.0f;

					return close;
				}, goWinBar);

				GameObject goCloseCross = Helper.FindOrCreateUI("CloseCross", goClose, (string name, GameObject parent) =>
				{
					GameObject cimg = Helper.CreateGUIGameObject(name, parent);
					Helper.SetRectTransform(cimg, 0, 0, 1, 1, 0.5f, 0.5f, 0, 0, 0, 0);
					Image img = cimg.AddOrGetComponent<Image>();
					img.sprite = Silver.UI.Resources.GetStaticSpriteCross();
					img.type = Image.Type.Simple;

					return cimg;
				});

				Button but = goClose.AddOrGetComponent<Button>();
				but.onClick.RemoveAllListeners();
				but.onClick.AddListener(() => { Visible = false; });

				goContent = Helper.FindOrCreateUI("Content", goWindow, (string name, GameObject parent) =>
				{
					GameObject content = Helper.CreateGUIGameObject(name, parent);
					Helper.SetRectTransform(content, 0, 0, 1, 1, 0.5f, 0, -4.0f, 0.0f, 0, 2.0f);
					content.AddOrGetComponent<Image>();
					Mask mask = content.AddOrGetComponent<Mask>();
					mask.showMaskGraphic = false;

					LayoutElement layout = content.AddComponent<LayoutElement>();
					layout.flexibleHeight = 1.0f;

					return content;
				});

				RefreshWindowsBarVisibility();
			}

			private void RefreshWindowsBarVisibility()
			{
				goClose.transform.SetParent(goWinBar.transform, false);
				goWinBar.SetActive(showTitleBar);
			}

			private void SetTilte(string title)
			{
				if (goTitle == null)
					return;

				Text text = goTitle.AddOrGetComponent<Text>();
				text.text = title;
			}

			private void ValidateEnabled()
			{
				if (CanvasObject != null)
					CanvasObject.SetActive(!Application.isPlaying || enabled);
			}

			private void Validate()
			{
				EventSystem es = gameObject.FindObjectOrCreateIt<EventSystem>("EventSystem");
				es.gameObject.AddOrGetComponent<StandaloneInputModule>();
				es.gameObject.AddOrGetComponent<TouchInputModule>();

				if (CanvasObject == null)  {
					CanvasObject = Helper.CreateGUIGameObject(this.GetType().Name + "Canvas");
					RectTransform rect = Helper.SetRectTransform(CanvasObject, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 1280, 800, 0, 0);
					rect.localScale = new Vector3(0.001f, 0.001f, 0.001f);

					Canvas canvas = CanvasObject.AddOrGetComponent<Canvas>();
					canvas.renderMode = RenderMode.ScreenSpaceOverlay;
					canvas.pixelPerfect = true;

					CanvasObject.AddOrGetComponent<CanvasScaler>();
					CanvasObject.AddOrGetComponent<GraphicRaycaster>();
					CanvasObject.AddOrGetComponent<CanvasRenderer>();
				}

				ValidateWindow(CanvasObject);

				_lastIsVisible = visible;
				visibleTransition = visible ? 1.0f : 0.0f;

				if (renderModeVR == null)
					renderModeVR = CanvasObject.GetComponent<CanvasRenderModeVR>();

				if (renderModeVR != null && IsVisible)
					renderModeVR.UpdateCanvasPosition();
			}

			void OnValidate()
			{
				if (!Application.isPlaying)
					Validate();
			}

			void Start()
			{
				Validate();

				if (OnWindowCreated != null)
					OnWindowCreated(this);
			}

			void OnEnable()
			{
				ValidateEnabled();
			}

			void OnDisable()
			{
				ValidateEnabled();
			}

			private bool KeyDown(string key)
			{
				return Event.current.Equals(Event.KeyboardEvent(key));
			}

			void OnSwipe(Vector2 swipe)
			{
				float sw = Mathf.Abs(swipe.x) / swipe.magnitude;
				if (sw > 0.5f)
					return;

				if (Visible && swipe.y > 0.4f)
					Visible = false;

				if (!Visible && swipe.y < -0.4f)
					Visible = true;
			}

			void OnGUI()
			{
				if (KeyDown("F9"))
					Visible = !Visible;
			}

			void ProcessInput()
			{
				// For phone devices
				if (Input.touchCount == 2)
				{
					if (!swipeProgress)
					{
						touch0Begin = Input.GetTouch(0).position;
						touch1Begin = Input.GetTouch(1).position;
						swipeProgress = true;
					}

					touch0End = Input.GetTouch(0).position;
					touch1End = Input.GetTouch(1).position;
				}
				else if (swipeProgress)
				{
					swipeProgress = false;

					float ipd = 0;
					if (Screen.dpi != 0)
						ipd = 1.0f / Screen.dpi;

					Vector2 touchBegin = (touch0Begin + touch1Begin) * 0.5f * ipd;
					Vector2 touchEnd = (touch0End + touch1End) * 0.5f * ipd;
					Vector2 swipe = touchEnd - touchBegin;
					OnSwipe(swipe);
				}
			}

			private void UpdateTransition()
			{
				if (Application.isPlaying)
				{
					const float timeToClose = 0.5f;
					if (visible && visibleTransition < 1.0f)
					{
						visibleTransition += Time.deltaTime / timeToClose;
						if (visibleTransition > 1.0f)
							visibleTransition = 1.0f;
					}
					else if (!visible && visibleTransition > 0.0f)
					{
						visibleTransition -= Time.deltaTime / timeToClose;
						if (visibleTransition < 0.0f)
							visibleTransition = 0.0f;
					}
				}
				else
				{
					visibleTransition = visible ? 1.0f : 0.0f;
				}

				if (goWindow != null)
				{
					float xmin = 0.5f - (windowScale.x * 0.5f);
					float xmax = 0.5f + (windowScale.x * 0.5f);
					float ymin = 0.5f - (windowScale.y * 0.5f);
					float ymax = 0.5f + (windowScale.y * 0.5f);
					float yTrans = Mathf.Lerp(ymax, ymin, visibleTransition);

					RectTransform rect = goWindow.GetComponent<RectTransform>();
					rect.anchorMin = new Vector2(xmin, yTrans);
					rect.anchorMax = new Vector2(xmax, ymax);

					goWindow.SetActive(IsVisible);
				}
			}

			void Update()
			{
				if (Application.isPlaying)
					ProcessInput();

				if (CanvasObject == null)
					return;

				UpdateTransition();

				if (Application.isPlaying)
				{
					if (renderModeVR != null && !_lastIsVisible && IsVisible)
						renderModeVR.UpdateCanvasPosition();

					_lastIsVisible = IsVisible;
				}
			}
		}
	}
}
