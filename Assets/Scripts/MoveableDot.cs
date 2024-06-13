using UnityEngine;
using UnityEngine.EventSystems;

public class MoveableDot : MonoBehaviour, IPointerDownHandler, IDragHandler
{
    RectTransform dotRectTransform;
    Canvas canvas;
    public Rect movementBounds; // Define the movement boundaries in normalized screen space (0 to 1)

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

        // Set the position of the dot
        dotRectTransform.anchoredPosition = localPoint;
    }
}
