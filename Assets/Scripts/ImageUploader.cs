using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System;
using System.IO;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public class ImageUploader : MonoBehaviour
{
    public Button settingsButton;
    //panel
    public GameObject settingsPanel;
    public Button closeButton;
    public Slider lightIntensitySlider;
    public Slider globalIntensitySlider;
    public ColorPicker lightColorPicker;
    public ColorPicker globalColorPicker;
    public Button uploadButton;
    public Button syncButton;
    public Button sendButton;
    public Renderer quadRenderer;
    public string url = "http://cyy-desktop.local:5000";
    public List<ImageScaler> images = new List<ImageScaler>();
    public Material targetMaterial;
    Texture2D uploadedTexture;
    Texture2D syncedTexture;
    List<Texture2D> syncedTextures = new List<Texture2D>();
    List<Texture2D> receivedTextures = new List<Texture2D>();
    MaterialState originalMaterialState;
    int timeout = 30;

    void Start()
    {
        uploadButton.onClick.AddListener(OnUploadButtonClick);
        syncButton.onClick.AddListener(OnSyncButtonClick);
        sendButton.onClick.AddListener(OnSendButtonClick);

        settingsButton.onClick.AddListener(() =>
        {
            settingsPanel.SetActive(true);
        });
        closeButton.onClick.AddListener(() =>
        {
            settingsPanel.SetActive(false);
        });
        // 备份初始材质状态
        BackupMaterialState();
        lightIntensitySlider.value = originalMaterialState.lightIntensity;
        globalIntensitySlider.value = originalMaterialState.globalIntensity;
        lightColorPicker.SetColor(originalMaterialState.lightColor);
        globalColorPicker.SetColor(originalMaterialState.globalColor);
    }

    void Update()
    {
        // 更新材质属性
        targetMaterial.SetFloat("_Intensity", lightIntensitySlider.value);
        targetMaterial.SetFloat("_exposure", globalIntensitySlider.value);
        targetMaterial.SetColor("_LightColor", lightColorPicker.GetColor());
        targetMaterial.SetColor("_Global", globalColorPicker.GetColor());
    }

    void OnUploadButtonClick()
    {
        string path = OpenFileBrowser();
        if (!string.IsNullOrEmpty(path))
        {
            StartCoroutine(LoadImage(path));
        }
    }

    void OnSyncButtonClick()
    {
        StartCoroutine(ReceiveImageFromServer());
    }

    string OpenFileBrowser()
    {
        // 打开文件浏览器（需第三方插件或自定义实现）
        // 例如，可以使用UnityEditor.EditorUtility.OpenFilePanel（仅限编辑器使用）
        // 或其他跨平台文件选择器库，如 StandaloneFileBrowser (https://github.com/gkngkc/UnityStandaloneFileBrowser)
        
        #if UNITY_EDITOR
        return UnityEditor.EditorUtility.OpenFilePanel("Select Image", "", "png,jpg,jpeg");
        #else
        return null;
        #endif
    }

    void OnImagePicked(string path)
    {
        if (!string.IsNullOrEmpty(path))
        {
            StartCoroutine(LoadImage(path));
        }
        else
        {
            Debug.Log("Image picking cancelled or failed.");
        }
    }

    System.Collections.IEnumerator LoadImage(string path)
    {
        byte[] imageData = File.ReadAllBytes(path);
        uploadedTexture = new Texture2D(2, 2);
        uploadedTexture.LoadImage(imageData);

        yield return null;
        SetupTextures(uploadedTexture);
    }

    void SetupTextures(Texture2D texture)
    {
        quadRenderer.material.mainTexture = texture;
        foreach (ImageScaler image in images)
        {
            image.ChangeImage(texture);
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

    System.Collections.IEnumerator ReceiveImageFromServer()
    {
        UnityWebRequest www = UnityWebRequest.PostWwwForm(url + "/sync", "");
        www.timeout = timeout;
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError(www.error);
        }
        else
        {
            string jsonResponse = www.downloadHandler.text;
            HandleSyncResponse(jsonResponse);
            Debug.Log("Received textures: " + syncedTextures.Count);
        }
    }

    
    void HandleSyncResponse(string jsonResponse)
    {
        // 处理JSON响应
        Debug.Log("Server response: " + jsonResponse);
        JsonData responseData = JsonUtility.FromJson<JsonData>(jsonResponse);
        if (responseData.status == "success")
        {
            syncedTextures.Clear();
            foreach (string base64Image in responseData.images)
            {
                Texture2D texture = new Texture2D(2, 2);
                byte[] imageData = System.Convert.FromBase64String(base64Image);
                texture.LoadImage(imageData);
                syncedTextures.Add(texture);
            }
            
            // 在这里处理接收到的纹理
            if (syncedTextures.Count >= 1)
            {
                uploadedTexture = syncedTextures[0];
                SetupTextures(uploadedTexture);
            }
        }
    }

    System.Collections.IEnumerator SendImageToServer(Texture2D texture)
    {
        byte[] imageData = texture.EncodeToPNG();
        WWWForm form = new WWWForm();
        form.AddBinaryData("image", imageData, "image.png", "image/png");

        UnityWebRequest www = UnityWebRequest.Post(url + "/process", form);
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
            Debug.Log("Received textures: " + receivedTextures.Count);
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
        public Color lightColor;
        public Color globalColor;
        public float lightIntensity;
        public float globalIntensity;
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
            mask = targetMaterial.GetTexture("_Mask"),
            lightColor = targetMaterial.GetColor("_LightColor"),
            globalColor = targetMaterial.GetColor("_Global"),
            lightIntensity = targetMaterial.GetFloat("_Intensity"),
            globalIntensity = targetMaterial.GetFloat("_exposure")
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
        targetMaterial.SetColor("_LightColor", originalMaterialState.lightColor);
        targetMaterial.SetColor("_Global", originalMaterialState.globalColor);
        targetMaterial.SetFloat("_Intensity", originalMaterialState.lightIntensity);
        targetMaterial.SetFloat("_exposure", originalMaterialState.globalIntensity);
    }

    void OnApplicationQuit()
    {
        // 恢复材质初始状态
        RestoreMaterialState();
    }
}