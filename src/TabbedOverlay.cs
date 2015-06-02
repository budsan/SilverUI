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
		public class TabbedOverlay : MonoBehaviour
		{
			public delegate void onOverlayCreated(TabbedOverlay overlay);
			public static event onOverlayCreated OnOverlayCreated;

			public interface Tab
			{
				string TabName();
				GameObject CreateContent();

				bool ShowVerticalScroll();
				bool ShowHoritzonalScroll();

				void OnGainFocus();
				void OnLostFocus();
			}

			//----------------------------//

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

			private class TabHandler
			{
				public Tab tab = null;
				public GameObject button = null;
				public GameObject root = null;
			}

			private List<TabHandler> _tabs = new List<TabHandler>();
			private Tab _selectedTab = null;
			private bool _dirtyTabs = true;

			//----------------------------//

			public GameObject CanvasObject = null;
			private CanvasRenderModeVR renderModeVR = null;
			private GameObject goWindow = null;
			private GameObject goWinBar = null;
			private GameObject goTitle = null;
			private GameObject goTabBar = null;
			private GameObject goClose = null;
			private GameObject goTabContent = null;
			private GameObject goInnerView = null;
			private GameObject goInnerScroll = null;
			private GameObject goEmptyContent = null;
			private GameObject goVerticalScrollBar = null;
			private GameObject goHorizontalScrollBar = null;

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

				goTabBar = Helper.FindOrCreateUI("TabBar", goWindow, (string name, GameObject parent) =>
				{
					GameObject tabBar = Helper.CreateGUIGameObject(name, parent);
					Helper.SetRectTransform(tabBar, 0, 0, 1, 0, 0.5f, 1, 0, 0, 0, 0);

					LayoutElement layout = tabBar.AddComponent<LayoutElement>();
					layout.preferredHeight = 32.0f;

					HorizontalLayoutGroup hor = tabBar.AddComponent<HorizontalLayoutGroup>();
					hor.childForceExpandHeight = false;
					hor.childForceExpandWidth = false;

					return tabBar;
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
				}, goWinBar, goTabBar);

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

				GameObject goTabView = Helper.FindOrCreateUI("TabScrollView", goTabBar, (string name, GameObject parent) =>
				{
					GameObject tabview = Helper.CreateGUIGameObject(name, parent);
					Helper.SetRectTransform(tabview, 0, 0, 1, 0, 0.5f, 1, 0, 0, 0, 0);
					tabview.AddOrGetComponent<Image>();
					Mask mask = tabview.AddOrGetComponent<Mask>();
					mask.showMaskGraphic = false;

					LayoutElement layout = tabview.AddComponent<LayoutElement>();
					layout.preferredHeight = 32.0f;
					layout.flexibleWidth = 1;

					return tabview;
				});

				GameObject goTabScroll = Helper.FindOrCreateUI("TabScrollRect", goTabView, (string name, GameObject parent) =>
				{
					GameObject tabscroll = Helper.CreateGUIGameObject(name, parent);
					Helper.SetRectTransform(tabscroll, 0, 0, 1, 1, 0.5f, 1, 0, 0, 0, -2.0f);
					ScrollRect scroll = tabscroll.AddOrGetComponent<ScrollRect>();
					
					scroll.vertical = false;
					scroll.scrollSensitivity = 8.0f;

					return tabscroll;
				});

				goTabContent = Helper.FindOrCreateUI("TabContent", goTabScroll, (string name, GameObject parent) =>
				{
					GameObject tabcontent = Helper.CreateGUIGameObject(name, parent);
					Helper.SetRectTransform(tabcontent, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0);
					tabcontent.AddOrGetComponent<CanvasRenderer>();

					HorizontalLayoutGroup horizontal = tabcontent.AddComponent<HorizontalLayoutGroup>();
					horizontal.childForceExpandWidth = false;
					horizontal.padding = new RectOffset(4, 4, 0, 0);
					horizontal.spacing = 2;

					ContentSizeFitter fitter = tabcontent.AddComponent<ContentSizeFitter>();
					fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;

					return tabcontent;
				});
				
				ScrollRect tabscrollScrollRect = goTabScroll.AddOrGetComponent<ScrollRect>();
				tabscrollScrollRect.content = goTabContent.AddOrGetComponent<RectTransform>();

				goInnerView = Helper.FindOrCreateUI("InnerScrollView", goWindow, (string name, GameObject parent) =>
				{
					GameObject innerview = Helper.CreateGUIGameObject(name, parent);
					Helper.SetRectTransform(innerview, 0, 0, 1, 1, 0.5f, 0, -4.0f, 0.0f, 0, 2.0f);
					innerview.AddOrGetComponent<Image>();
					Mask mask = innerview.AddOrGetComponent<Mask>();
					mask.showMaskGraphic = false;

					LayoutElement layout = innerview.AddComponent<LayoutElement>();
					layout.flexibleHeight = 1.0f;

					return innerview;
				});

				goInnerScroll = Helper.FindOrCreateUI("InnerScrollRect", goInnerView, (string name, GameObject parent) =>
				{
					GameObject innerscroll = Helper.CreateGUIGameObject(name, parent);
					Helper.SetRectTransform(innerscroll, 0, 0, 1, 1, 0, 1, -24.0f, -24.0f, 0, 0);
					ScrollRect scroll = innerscroll.AddOrGetComponent<ScrollRect>();
					scroll.scrollSensitivity = 8.0f;

					return innerscroll;
				});

				goEmptyContent = Helper.FindOrCreateUI("EmptyContent", goInnerScroll, (string name, GameObject parent) =>
				{
					return Helper.CreateGUIGameObject(name, parent);
				});

				goVerticalScrollBar = Helper.FindOrCreateUI("InnerVerticalScrollbar", goInnerView, (string name, GameObject parent) =>
				{
					GameObject vertscrollbar = Helper.CreateGUIGameObject(name, parent);
					Helper.SetRectTransform(vertscrollbar, 1, 0, 1, 1, 1, 1, 20.0f, -24.0f, -2.0f, -2.0f);
					Scrollbar scrollbar = Helper.AddScrollbar(vertscrollbar);
					scrollbar.direction = Scrollbar.Direction.BottomToTop;

					return vertscrollbar;
				});

				goHorizontalScrollBar = Helper.FindOrCreateUI("InnerHorizontalScrollbar", goInnerView, (string name, GameObject parent) =>
				{
					GameObject horiscrollbar = Helper.CreateGUIGameObject(name, parent);
					Helper.SetRectTransform(horiscrollbar, 0, 0, 1, 0, 0, 0, -24.0f, 20.0f, 2.0f, 2.0f);
					Helper.AddScrollbar(horiscrollbar);
					return horiscrollbar;
				});

				ScrollRect innerScrollScrollRect = goInnerScroll.AddOrGetComponent<ScrollRect>();
				innerScrollScrollRect.verticalScrollbar = goVerticalScrollBar.GetComponent<Scrollbar>();
				innerScrollScrollRect.horizontalScrollbar = goHorizontalScrollBar.GetComponent<Scrollbar>();
				innerScrollScrollRect.content = Helper.SetRectTransform(goEmptyContent, 0, 0, 1, 1, 0, 1, 0, 0, 0, 0);

				RefreshWindowsBarVisibility();
			}

			private void RefreshWindowsBarVisibility()
			{
				goClose.transform.SetParent(showTitleBar ? goWinBar.transform : goTabBar.transform, false);
				goWinBar.SetActive(showTitleBar);
			}

			private GameObject CreateTabButton(TabHandler handler)
			{
				string name = handler.tab.TabName();
				Tab i = handler.tab;
				GameObject button = Helper.CreateButton(null, "TabButton_" + name, name, () => {
					SetTabActive(i);
				});

				GameObject text = button.transform.Find("Text").gameObject;
				Text txt = text.GetComponent<Text>();
				txt.font = Resources.Instance.GetFontTabs();

				LayoutElement layout = button.AddComponent<LayoutElement>();
				layout.preferredWidth = txt.preferredWidth + 16.0f;

				Button but = button.GetComponent<Button>();
				ColorBlock block = but.colors;
				Color normal = block.normalColor;
				block.normalColor = block.disabledColor;
				block.disabledColor = normal;
				but.colors = block;

				Image img = button.GetComponent<Image>();
				img.sprite = Resources.Instance.GetSpriteTabButton();

				return button;
			}

			private bool InitTab(TabHandler handler)
			{
				try
				{
					handler.root = handler.tab.CreateContent();
					if (handler.root == null)
					{
						handler.root = new GameObject("TabNull" + handler.tab.TabName());
						handler.root.AddOrGetComponent<CanvasRenderer>();
					}

					handler.root.layer = LayerMask.NameToLayer("UI");
					handler.root.transform.SetParent(goInnerScroll.transform, false);
					handler.root.SetActive(false);
					handler.button = CreateTabButton(handler);
				}
				catch(Exception ex)
				{
					Debug.LogWarning(ex.Message);
					RemoveTab(handler);
					return false;
				}

				return true;
			}

			public void AddTab(Tab tab)
			{
				TabHandler handler = new TabHandler();
				handler.tab = tab;
				_tabs.Add(handler);
				_dirtyTabs = true;
			}

			private void UpdateTitleName()
			{
				if (_tabs.Count == 0)
				{
					SetTilte("Tabbed Overlay");
					_selectedTab = null;
				}
				else
				{
					if (_selectedTab == null)
						_selectedTab = _tabs[0].tab;

					try
					{
						if (_tabs.Count == 1)
							SetTilte(_selectedTab.TabName());
						else
							SetTilte("Tab: " + _selectedTab.TabName());
					}
					catch (Exception ex)
					{
						Debug.LogWarning(ex.Message);
						RemoveTab(_selectedTab);
					}
				}
			}

			private void RemoveTab(TabHandler handler)
			{
				if (handler == null)
					return;

				if (handler.button != null)
					GameObject.Destroy(handler.button);

				if (handler.root != null)
					GameObject.Destroy(handler.root);

				if (handler.tab == _selectedTab)
					_selectedTab = null;

				_tabs.Remove(handler);
				_dirtyTabs = true;
			}

			public void RemoveTab(Tab tab)
			{
				TabHandler holder = null;
				foreach (TabHandler handler in _tabs)
				{
					if (handler.tab == _selectedTab)
					{
						holder = handler;
						break;
					}
				}

				RemoveTab(holder);
			}

			public void SetTabActive(Tab tab)
			{
				_selectedTab = tab;
				_dirtyTabs = true;
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
					CanvasObject = Helper.CreateGUIGameObject("TabbedOverlayCanvas");
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

				if (OnOverlayCreated != null)
					OnOverlayCreated(this);
			}

			void OnEnable()
			{
				ValidateEnabled();
			}

			void OnDisable()
			{
				ValidateEnabled();
			}

			void ValidateTabs()
			{
				if (!_dirtyTabs)
					return;

				goTabBar.SetActive(_tabs.Count > 1);

				if (_tabs.Count > 0)
				{
					if (_selectedTab == null)
						_selectedTab = _tabs[0].tab;
				}
				else
				{
					if (_selectedTab != null)
					{
						_selectedTab = null;
						goInnerScroll.AddOrGetComponent<ScrollRect>().content = 
							goEmptyContent.AddOrGetComponent<RectTransform>();
					}
				}

				int index = 0;
				while(index < _tabs.Count)
				{
					TabHandler handler =_tabs[index];
					if (handler.root == null && !InitTab(handler))
						continue;

					if (handler.tab == _selectedTab)
					{
						if (!handler.root.activeSelf)
						{
							try
							{
								handler.button.GetComponent<Button>().interactable = false;
								handler.root.SetActive(true);
								goInnerScroll.GetComponent<ScrollRect>().content = handler.root.AddOrGetComponent<RectTransform>();
							
								handler.tab.OnGainFocus();
								UpdateTitleName();
							}
							catch (Exception ex)
							{
								Debug.LogWarning(ex.Message);
								RemoveTab(handler);
								continue;
							}
						}
					}
					else
					{
						if (handler.root.activeSelf)
						{
							try
							{
								handler.button.AddOrGetComponent<Button>().interactable = true;
								handler.root.SetActive(false);
								handler.tab.OnLostFocus();
							}
							catch (Exception ex)
							{
								Debug.LogWarning(ex.Message);
								RemoveTab(handler);
								continue;
							}
						}
					}

					handler.button.transform.SetParent(goTabContent.transform, false);
					index++;
				}

				try
				{
					if (_selectedTab == null)
					{
						SetTilte("Tabbed Overlay");
					}
					else
					{
						if (_tabs.Count == 1)
							SetTilte(_selectedTab.TabName());
						else
							SetTilte("Tab: " + _selectedTab.TabName());
					}
				}
				catch (Exception ex)
				{
					Debug.LogWarning(ex.Message);
					RemoveTab(_selectedTab);
				}

				_dirtyTabs = false;
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

				if (goWindow != null)
				{
					float xmin = 0.5f - (windowScale.x * 0.5f);
					float xmax = 0.5f + (windowScale.x * 0.5f);
					float ymin = 0.5f - (windowScale.y * 0.5f);
					float ymax = 0.5f + (windowScale.y * 0.5f);
					float yTrans = Mathf.Lerp(ymax, ymin, visibleTransition);

					RectTransform rect = goWindow.GetComponent<RectTransform>();
					rect.anchorMin = new Vector2(xmin, yTrans);
					rect.anchorMax = new Vector2(xmax,   ymax);

					goWindow.SetActive(IsVisible);
				}
			}

			void Update()
			{
				ProcessInput();

				if (CanvasObject == null)
					return;

				UpdateTransition();

				try
				{
					if (_selectedTab != null)
					{
						if (!_lastIsVisible && IsVisible)
							_selectedTab.OnGainFocus();
						else if (_lastIsVisible && !IsVisible)
							_selectedTab.OnLostFocus();
					}
				}
				catch (Exception ex)
				{
					Debug.LogWarning(ex.Message);
					RemoveTab(_selectedTab);
				}

				if (renderModeVR != null && !_lastIsVisible && IsVisible)
					renderModeVR.UpdateCanvasPosition();

				if (IsVisible)
				{
					ValidateTabs();

					if (_selectedTab != null)
					{
						RectTransform rect = goInnerScroll.AddOrGetComponent<RectTransform>();
						Vector2 sizeDelta = rect.sizeDelta;
						sizeDelta.x = _selectedTab.ShowVerticalScroll() ? -24.0f : -4.0f;
						sizeDelta.y = _selectedTab.ShowHoritzonalScroll() ? -24.0f : -4.0f;
						rect.sizeDelta = sizeDelta;

						bool scrollbarChanged = false;
						if (goVerticalScrollBar.activeSelf != _selectedTab.ShowVerticalScroll())
						{
							ScrollRect scrollRect = goInnerScroll.GetComponent<ScrollRect>();
							scrollRect.vertical = _selectedTab.ShowVerticalScroll();
							goVerticalScrollBar.SetActive(_selectedTab.ShowVerticalScroll());
							scrollbarChanged = true;
						}
							

						if (goHorizontalScrollBar.activeSelf != _selectedTab.ShowHoritzonalScroll())
						{
							ScrollRect scrollRect = goInnerScroll.GetComponent<ScrollRect>();
							scrollRect.horizontal = _selectedTab.ShowHoritzonalScroll();
							goHorizontalScrollBar.SetActive(_selectedTab.ShowHoritzonalScroll());
							scrollbarChanged = true;
						}
							
						if (scrollbarChanged)
						{
							if(!_selectedTab.ShowVerticalScroll() &&  !_selectedTab.ShowHoritzonalScroll())
							{
								goInnerView.GetComponent<Image>().enabled = false;
								goInnerView.GetComponent<Mask>().enabled = false;
							}
							else
							{
								goInnerView.GetComponent<Image>().enabled = true;
								goInnerView.GetComponent<Mask>().enabled = true;
							}
						}
					}
				}

				_lastIsVisible = IsVisible;
			}
		}
	}
}
