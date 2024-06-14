using UnityEngine;

public class ImageScaler : MonoBehaviour
{
    public Transform imageTransform;
    public Texture2D texture;
    public Vector2 imageSize = new Vector2(1, 1);
    Vector2 targetSize;
    float targtRatio;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ChangeImage(texture);
    }

    public void ChangeImage(Texture2D newTexture)
    {
        texture = newTexture;
        targtRatio = (float)texture.width / texture.height;
        float imageRatio = imageSize.x / imageSize.y;
        if (targtRatio > imageRatio)
        {
            targetSize = new Vector2(imageSize.x, imageSize.x / targtRatio);
        }
        else
        {
            targetSize = new Vector2(imageSize.y * targtRatio, imageSize.y);
        }
        imageTransform.localScale = new Vector3(targetSize.x, targetSize.y, 1);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
