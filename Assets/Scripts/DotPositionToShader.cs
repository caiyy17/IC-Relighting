using UnityEngine;

public class DotPositionToShader : MonoBehaviour
{
    RectTransform dotRectTransform;
    Canvas canvas;
    public string shaderProperty = "_DotPosition"; // Ensure this matches the property name in Shader Graph
    public Vector2 leftUpCorner;
    public Vector2 rightDownCorner;
    private void Awake()
    {
        dotRectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
    }

    private void Update()
    {
        // Convert the dot position to normalized screen coordinates
        Vector2 screenPosition = RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, dotRectTransform.position);
        Vector2 normalizedScreenPosition = new Vector2(screenPosition.x / Screen.width, screenPosition.y / Screen.height);

        Vector2 result = new Vector2(
        (normalizedScreenPosition.x - leftUpCorner.x) / (rightDownCorner.x - leftUpCorner.x) - 0.5f,
        (normalizedScreenPosition.y - leftUpCorner.y) / (rightDownCorner.y - leftUpCorner.y) - 0.5f);

        // Set the normalized screen position to the shader
        // Debug.Log(result);
        Shader.SetGlobalVector(shaderProperty, new Vector4(result.x, -result.y, 0, 0));
    }

    private void OnDestroy()
    {
        // Reset the shader property when the script is destroyed
        Shader.SetGlobalVector(shaderProperty, Vector4.zero);
    }
}
