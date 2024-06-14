using UnityEngine;
using UnityEngine.EventSystems;

public class MoveableDot : MonoBehaviour, IPointerDownHandler, IDragHandler
{
    RectTransform dotRectTransform;
    Canvas canvas;

    private void Awake()
    {
        dotRectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        MoveDot(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        MoveDot(eventData);
    }

    private void MoveDot(PointerEventData eventData)
    {
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas.transform as RectTransform, eventData.position, eventData.pressEventCamera, out localPoint);
        // Restrict inside the canvas
        localPoint = new Vector2(Mathf.Clamp(localPoint.x, -canvas.pixelRect.width / 2, canvas.pixelRect.width / 2), Mathf.Clamp(localPoint.y, -canvas.pixelRect.height / 2, canvas.pixelRect.height / 2));
        // Set the position of the dot
        dotRectTransform.anchoredPosition = localPoint;
    }
}
