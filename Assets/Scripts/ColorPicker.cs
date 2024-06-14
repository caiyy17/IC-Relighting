using UnityEngine;
using UnityEngine.UI;

//ColorPicker
public class ColorPicker : MonoBehaviour
{
    public Slider colorR;
    public Slider colorG;
    public Slider colorB;
    public Color color;

    void Start()
    {
        colorR.onValueChanged.AddListener(OnColorChanged);
        colorG.onValueChanged.AddListener(OnColorChanged);
        colorB.onValueChanged.AddListener(OnColorChanged);
    }

    void OnColorChanged(float value)
    {
        color = new Color(colorR.value, colorG.value, colorB.value);
    }

    public void SetColor(Color color)
    {
        colorR.value = color.r;
        colorG.value = color.g;
        colorB.value = color.b;
        color = new Color(colorR.value, colorG.value, colorB.value);
    }

    public Color GetColor()
    {
        return color;
    }
}

