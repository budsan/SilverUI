using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Silver
{
	namespace UI
	{
		public class ClickFencer : BaseRaycaster, IPointerEnterHandler, IPointerExitHandler
		{
			public delegate void ClickDelegate(bool inside, GameObject pointerPress);
			public event ClickDelegate OnClick;

			private bool isPointerInside { get; set; }
			private float m_LastClickTime = 0.0f;

			private bool ignoreFirstClick = false;
			public bool IgnoreFirstClick
			{
				get { return ignoreFirstClick; }
				set { ignoreFirstClick = value; }
			}

			public void OnPointerEnter(PointerEventData eventData)
			{
				isPointerInside = true;
			}

			public void OnPointerExit(PointerEventData eventData)
			{
				isPointerInside = false;
			}

			protected override void OnEnable()
			{
				base.OnEnable();
				ignoreFirstClick = true;
			}

			public override void Raycast(PointerEventData eventData, List<RaycastResult> resultAppendList)
			{
				if (eventData.button == PointerEventData.InputButton.Left &&
					eventData.clickTime > m_LastClickTime)
				{
					m_LastClickTime = eventData.clickTime;

					if (ignoreFirstClick)
					{
						ignoreFirstClick = false;
					}
					else
					{
						if (OnClick != null)
							OnClick(isPointerInside, eventData.pointerPress);
					}
				}

				return;
			}

			public override Camera eventCamera
			{
				get
				{
					return null;
				}
			}
		}
	}
}

