using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.IO;
using System.Collections.Generic;

[RequireComponent(typeof(ImageScaler))]
public class ImageUploader : MonoBehaviour
{
    public Button uploadButton;
    public Button sendButton;
    public Renderer quadRenderer;
    public string url = "http://cyy-desktop.local:5000/upload";
    public List<ImageScaler> images = new List<ImageScaler>();
    public Material targetMaterial;
    Texture2D uploadedTexture;
    List<Texture2D> receivedTextures = new List<Texture2D>();
    MaterialState originalMaterialState;
    int timeout = 30;

    void Start()
    {
        uploadButton.onClick.AddListener(OnUploadButtonClick);
        sendButton.onClick.AddListener(OnSendButtonClick);

        // 备份初始材质状态
        BackupMaterialState();
    }

    void OnUploadButtonClick()
    {
        string path = OpenFileBrowser();
        if (!string.IsNullOrEmpty(path))
        {
            StartCoroutine(LoadImage(path));
        }
    }

    string OpenFileBrowser()
    {
        // 打开文件浏览器（需第三方插件或自定义实现）
        // 例如，可以使用UnityEditor.EditorUtility.OpenFilePanel（仅限编辑器使用）
        // 或其他跨平台文件选择器库，如 StandaloneFileBrowser (https://github.com/gkngkc/UnityStandaloneFileBrowser)
        
        #if UNITY_EDITOR
        return UnityEditor.EditorUtility.OpenFilePanel("Select Image", "", "png,jpg,jpeg");
        #else
        return ""; // Implement platform-specific file picker logic here
        #endif
    }

    System.Collections.IEnumerator LoadImage(string path)
    {
        byte[] imageData = File.ReadAllBytes(path);
        uploadedTexture = new Texture2D(2, 2);
        uploadedTexture.LoadImage(imageData);

        yield return null;

        quadRenderer.material.mainTexture = uploadedTexture;
        foreach (ImageScaler image in images)
        {
            image.ChangeImage(uploadedTexture);
        }

        //make a black texture
        Texture2D blackTexture = new Texture2D(2, 2);
        Color[] colors = new Color[4];
        for (int i = 0; i < colors.Length; i++)
        {
            colors[i] = Color.black;
        }
        blackTexture.SetPixels(colors);
        blackTexture.Apply();
        targetMaterial.SetTexture("_BaseL", blackTexture);
        targetMaterial.SetTexture("_BaseR", blackTexture);
        targetMaterial.SetTexture("_BaseU", blackTexture);
        targetMaterial.SetTexture("_BaseD", blackTexture);
        targetMaterial.SetTexture("_Mask", blackTexture);

    }

    void OnSendButtonClick()
    {
        if (uploadedTexture != null)
        {
            StartCoroutine(SendImageToServer(uploadedTexture));
        }
    }

    System.Collections.IEnumerator SendImageToServer(Texture2D texture)
    {
        byte[] imageData = texture.EncodeToPNG();
        WWWForm form = new WWWForm();
        form.AddBinaryData("image", imageData, "image.png", "image/png");

        UnityWebRequest www = UnityWebRequest.Post(url, form);
        www.timeout = timeout;
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError(www.error);
        }
        else
        {
            string jsonResponse = www.downloadHandler.text;
            HandleServerResponse(jsonResponse);
        }
    }

    void HandleServerResponse(string jsonResponse)
    {
        // 处理JSON响应
        Debug.Log("Server response: " + jsonResponse);
        JsonData responseData = JsonUtility.FromJson<JsonData>(jsonResponse);
        if (responseData.status == "success")
        {
            receivedTextures.Clear();
            foreach (string base64Image in responseData.images)
            {
                Texture2D texture = new Texture2D(2, 2);
                byte[] imageData = System.Convert.FromBase64String(base64Image);
                texture.LoadImage(imageData);
                receivedTextures.Add(texture);
            }
            
            // 在这里处理接收到的纹理
            if (receivedTextures.Count >= 5)
            {
                targetMaterial.SetTexture("_BaseL", receivedTextures[0]);
                targetMaterial.SetTexture("_BaseR", receivedTextures[1]);
                targetMaterial.SetTexture("_BaseU", receivedTextures[2]);
                targetMaterial.SetTexture("_BaseD", receivedTextures[3]);
                targetMaterial.SetTexture("_Mask", receivedTextures[4]);
            }
        }
    }

    [System.Serializable]
    public class JsonData
    {
        public string message;
        public string status;
        public List<string> images;
    }

    // 定义材质初始状态的结构体
    [System.Serializable]
    public struct MaterialState
    {
        public Texture baseL;
        public Texture baseR;
        public Texture baseU;
        public Texture baseD;
        public Texture mask;
    }

    // 备份初始材质状态
    void BackupMaterialState()
    {
        originalMaterialState = new MaterialState
        {
            baseL = targetMaterial.GetTexture("_BaseL"),
            baseR = targetMaterial.GetTexture("_BaseR"),
            baseU = targetMaterial.GetTexture("_BaseU"),
            baseD = targetMaterial.GetTexture("_BaseD"),
            mask = targetMaterial.GetTexture("_Mask")
        };
    }

    // 恢复材质初始状态
    void RestoreMaterialState()
    {
        targetMaterial.SetTexture("_BaseL", originalMaterialState.baseL);
        targetMaterial.SetTexture("_BaseR", originalMaterialState.baseR);
        targetMaterial.SetTexture("_BaseU", originalMaterialState.baseU);
        targetMaterial.SetTexture("_BaseD", originalMaterialState.baseD);
        targetMaterial.SetTexture("_Mask", originalMaterialState.mask);
    }

    void OnApplicationQuit()
    {
        // 恢复材质初始状态
        RestoreMaterialState();
    }
}