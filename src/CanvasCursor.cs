using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Silver
{
	namespace UI
	{
		[RequireComponent(typeof(Canvas))]
		public class CanvasCursor : BaseRaycaster
		{
			public bool showSystemCursor = false;

			private Canvas m_Canvas = null;
			private Canvas canvas
			{
				get
				{
					if (m_Canvas != null)
						return m_Canvas;

					m_Canvas = GetComponent<Canvas>();
					return m_Canvas;
				}
			}

			private GameObject CanvasObject = null;
			private GameObject CursorImage = null;
			private bool m_focus = true;

			private static Sprite _spriteCursor = null;
			static public Sprite GetCursorSprite()
			{
				if (_spriteCursor == null)
					_spriteCursor = UnityEngine.Resources.Load<Sprite>("Cursor");

				return _spriteCursor;
			}

			protected override void OnEnable()
			{
				base.OnEnable();				

				// In Unity, the canvas mesh is rebuilt every time you change anything in your canvas. It's very likely that our main canvas
				// to be pretty complex, so we want to avoid rebuild main canvas every time that the cursor is moving.
				// That's why we create a secondary canvas.
				CanvasObject = Helper.FindOrCreateUI("CursorCanvas", canvas.gameObject, (string name, GameObject parent) =>
				{
					GameObject goCanvas = Helper.CreateGUIGameObject("CanvasCursor", parent);
					RectTransform rect = Helper.SetRectTransform(goCanvas, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 20, 20, 0, 0);
					Canvas cursorCanvas = goCanvas.AddOrGetComponent<Canvas>();
					cursorCanvas.pixelPerfect = true;

					return goCanvas;
				});

				CursorImage = Helper.FindOrCreateUI("Cursor", CanvasObject, (string name, GameObject parent) =>
				{
					GameObject cursorImage = Helper.CreateGUIGameObject("Cursor", parent);
					RectTransform rect = Helper.SetRectTransform(cursorImage, 0.0f, 1.0f, 0.0f, 1.0f, 0.3f, 0.9f, 20.0f, 20.0f, 20.0f, 20.0f);

					Image image = cursorImage.AddOrGetComponent<Image>();
					image.sprite = GetCursorSprite();

					CanvasGroup group = cursorImage.AddOrGetComponent<CanvasGroup>();
					group.blocksRaycasts = false;
					group.interactable = false;

					return cursorImage;
				});

				ChangedVisibility();
			}

			protected override void OnDisable()
			{
				base.OnDisable();
				ChangedVisibility();
			}

			void OnApplicationFocus(bool focus)
			{
				m_focus = focus;
				ChangedVisibility();
			}

			void ChangedVisibility()
			{
				if (CanvasObject == null)
					return;

				bool enableCursor = enabled && m_focus && Input.mousePresent;
				CanvasObject.SetActive(enableCursor);

#if UNITY_4_5 || UNITY_4_6
					Screen.showCursor = showSystemCursor || !enableCursor;
#else
				Cursor.visible = showSystemCursor || !enableCursor;
#endif
			}

			public override void Raycast(PointerEventData eventData, List<RaycastResult> resultAppendList)
			{
				if (eventCamera != null && canvas != null && CanvasObject != null && CursorImage != null)
				{
					Camera camera = eventCamera;
					Plane plane = new Plane(canvas.transform.forward, canvas.transform.position);
					Ray ray = camera.ScreenPointToRay(eventData.position);
					float rayDistance = camera.farClipPlane;
					if (!plane.Raycast(ray, out rayDistance))
						return;

					Canvas cursorCanvas = CanvasObject.GetComponent<Canvas>();
					RectTransform cursorCanvasRect = cursorCanvas.GetComponent<RectTransform>();
					RectTransform canvasRect = canvas.GetComponent<RectTransform>();

					RectTransform cursorRect = CursorImage.AddOrGetComponent<RectTransform>();
					cursorRect.position = ray.GetPoint(rayDistance);

					ChangedVisibility();
				}

				return;
			}

			public override Camera eventCamera
			{
				get
				{
					if (canvas.renderMode == RenderMode.ScreenSpaceOverlay
						|| (canvas.renderMode == RenderMode.ScreenSpaceCamera && canvas.worldCamera == null))
						return null;

					return canvas.worldCamera != null ? canvas.worldCamera : Camera.main;
				}
			}
		}
	}
}