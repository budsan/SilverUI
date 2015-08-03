using UnityEngine;
using System.Collections;

namespace Silver
{
	namespace UI
	{
		[RequireComponent(typeof(Canvas))]
		[ExecuteInEditMode]
		public class CanvasRenderModeVR : UnityEngine.UI.CanvasScaler
		{
			public float m_PlaneDistance = 0.5f;
			public bool m_UpdatePosition = false;

			private Canvas canvas = null;
			private RectTransform m_Rect = null;
			private CanvasCursor m_Cursor = null;

			protected override void OnEnable()
			{
				base.OnEnable();
				canvas = GetComponent<Canvas>();
				m_Cursor = GetComponent<UI.CanvasCursor>();
				if (m_Cursor == null)
					m_Cursor = gameObject.AddComponent<UI.CanvasCursor>();

				if (m_Cursor != null)
					m_Cursor.enabled = enabled;

				m_Rect = GetComponent<RectTransform>();
			}

			protected override void OnDisable()
			{
				base.OnDisable();

				if (m_Cursor != null)
					m_Cursor.enabled = false;
			}

			public void UpdateCanvasPosition()
			{
				UpdateCanvasPosition(m_PlaneDistance);
			}

			public void UpdateCanvasPosition(float currentDistance, bool updatePosition = true, bool updateScale = true, bool smoothUpdate = false)
			{
				if (canvas == null)
					canvas = GetComponent<Canvas>();

                if (canvas.worldCamera == null)
                {
                    GameObject mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
                    if (mainCamera != null)
                        canvas.worldCamera = mainCamera.GetComponent<Camera>();
                }

				if (enabled && canvas != null && canvas.worldCamera != null && canvas.renderMode == RenderMode.WorldSpace && m_Rect != null)
				{

					Camera eventCamera = canvas.worldCamera;
					Vector3 cameraScale = eventCamera.transform.lossyScale;
					Vector2 viewportSize = new Vector2(eventCamera.pixelWidth, eventCamera.pixelHeight);
						
					if (currentDistance < eventCamera.nearClipPlane)
						currentDistance = eventCamera.nearClipPlane;
					double plane = (double) currentDistance / (double)cameraScale.z;

					float fovRad = eventCamera.fieldOfView * Mathf.PI * (1.0f / 360f);
					float tan = Mathf.Tan(fovRad);
					double z = (0.5f * viewportSize.y) / tan;
					double scale = plane / z;

					float currCanvasScale = canvas.scaleFactor;
					if (currCanvasScale <= 0.0f)
						currCanvasScale = 1.0f;
					float invCanvasScale = 1.0f / currCanvasScale;
					
					Vector3 relativePos = new Vector3(0, (float)(viewportSize.y * scale * 0.5), (float)plane);
					Vector3 globalPos = eventCamera.transform.TransformPoint(relativePos);
					Vector3 localScale = m_Rect.localScale;
					localScale.x = (float)scale * currCanvasScale * cameraScale.x;
					localScale.y = (float)scale * currCanvasScale * cameraScale.y;

					const float smoothFactor = 0.2f;
					m_Rect.sizeDelta = new Vector2(eventCamera.pixelWidth * invCanvasScale, eventCamera.pixelHeight * invCanvasScale);
					if (updatePosition)
					{
						m_Rect.pivot = new Vector2(0.5f, 1.0f);
						if (smoothUpdate)
						{
							m_Rect.position = Vector3.Slerp(m_Rect.position, globalPos, smoothFactor);
							m_Rect.rotation = Quaternion.Slerp(m_Rect.rotation, eventCamera.transform.rotation, smoothFactor);
						}
						else
						{
							m_Rect.position = globalPos;
							m_Rect.rotation = eventCamera.transform.rotation;
						}
						
					}
						
					if (updateScale)
					{
						if (smoothUpdate)
							m_Rect.localScale = Vector3.Slerp(m_Rect.localScale, localScale, smoothFactor);
						else
							m_Rect.localScale = localScale;
					}
						
				}
			}

			protected void FixedUpdate()
			{
				bool smoothUpdate = m_UpdatePosition && Application.isPlaying;
				UpdateCanvasPosition(m_PlaneDistance, m_UpdatePosition, true, smoothUpdate);
			}

			protected override void Update()
			{
				if (canvas != null && !canvas.isRootCanvas)
					return;

				if (canvas.renderMode == RenderMode.WorldSpace)
				{
					switch (uiScaleMode)
					{
						case ScaleMode.ConstantPixelSize: HandleConstantPixelSize(); break;
						case ScaleMode.ScaleWithScreenSize: HandleScaleWithScreenSize(); break;
						case ScaleMode.ConstantPhysicalSize: HandleConstantPhysicalSize(); break;
					}
				}
			}
		}
	}
}
