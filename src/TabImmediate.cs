using UnityEngine;
using System.Collections;

namespace Silver
{
	namespace UI
	{
		public abstract class TabImmediate : MonoBehaviour, TabbedOverlay.Tab
		{
			protected TabbedOverlay overlay = null;
			protected Immediate ui = new Immediate();
			protected GameObject root = null;

			//-------------------------------------------------------------//

			public abstract string TabName();

			public virtual void OnGainFocus()
			{
				enabled = true;
			}

			public virtual void OnLostFocus()
			{
				enabled = false;
			}

			public virtual bool ShowVerticalScroll()
			{
				return true;
			}

			public virtual bool ShowHoritzonalScroll()
			{
				return false;
			}

			public virtual GameObject CreateContent()
			{
				if (root == null)
				{
					root = Helper.CreateGUIGameObject(TabName());
					var image = root.AddComponent<UnityEngine.UI.Image>();
					image.color = new Color(1, 1, 1, 0);
					Helper.SetRectTransform(root, 0, 1, 1, 1, 0, 1, 0, 0, 0, 0);
					ui.Parent = root;
				}

				return root;
			}

			//-------------------------------------------------------------//

			public virtual void Start()
			{
				enabled = false;
				overlay = gameObject.FindManagerOrCreateIt<UI.TabbedOverlay>();
				overlay.AddTab(this);
			}

			public virtual void OnDestroy()
			{
				overlay.RemoveTab(this);
			}

			public virtual void Update()
			{
				if (root != null)
				{
					DrawUI();

					RectTransform rect = root.GetComponent<RectTransform>();
					GameObject uiRoot = ui.gameObject;
					if (uiRoot != null)
					{
						RectTransform uiRect = uiRoot.GetComponent<RectTransform>();
						Vector2 rootSizeDelta = rect.sizeDelta;
						rootSizeDelta.y = uiRect.rect.height;
						rect.sizeDelta = rootSizeDelta;
					}
				}
			}

			public abstract void DrawUI();
		}
	}
}
