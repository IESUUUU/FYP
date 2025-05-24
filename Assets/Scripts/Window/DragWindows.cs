using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DragWindows : MonoBehaviour, IDragHandler, IPointerDownHandler
{
    [SerializeField] private RectTransform dragRectTransform;
    [SerializeField] private Canvas canvas;
    [SerializeField] private Image backgroundImage;
    private Color backgroundColor;

    private void Awake()
    {
        if (dragRectTransform == null)
        {
            dragRectTransform = transform.parent.GetComponent<RectTransform>();
        }

        if (canvas == null)
        {
            Transform testCanvasTransform = transform.parent;
            while (testCanvasTransform != null)
            {
                canvas = testCanvasTransform.GetComponent<Canvas>();
                if (canvas != null)
                {
                    break;
                }
                testCanvasTransform = testCanvasTransform.parent;
            }
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        dragRectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        dragRectTransform.SetAsLastSibling();
    }
}
