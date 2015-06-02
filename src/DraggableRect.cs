using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

using System.Collections;

namespace Silver
{
	namespace UI
	{
		public class DraggableRect : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
		{
			public RectTransform target;

			Vector2 beginAnchoredPosition;
			Vector2 beginPosition;

			public void OnBeginDrag(PointerEventData eventData)
			{
				if (target != null)
				{
					beginAnchoredPosition = target.anchoredPosition;
					beginPosition = eventData.position;
				}
			}

			public void OnDrag(PointerEventData eventData)
			{
				MoveTarget(eventData.position);
			}

			public void OnEndDrag(PointerEventData eventData)
			{
				MoveTarget(eventData.position);
			}

			void MoveTarget(Vector2 currentPosition)
			{
				if (target != null)
					target.anchoredPosition = beginAnchoredPosition + (currentPosition - beginPosition);
			}
		}
	}
}


