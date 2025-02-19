using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinimapController : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public Camera minimapCamera;

    [Header("Minimap Settings")]
    public float minimapSize = 200f;
    public float minimapHeight = 100f;
    public Vector2 minimapOffset = new Vector2(10f, 10f);
    [Range(10f, 100f)]
    public float minimapZoom = 25f;

    [Header("Visual Settings")]
    public Color groundColor = Color.white;
    public bool showShadows = false;


    private RectTransform minimapRect;
    private RenderTexture minimapRenderTexture;

    void Start()
    {
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player").transform;

        SetupMinimapCamera();
        CreateMinimapUI();
    }

    void SetupMinimapCamera()
    {
        if (minimapCamera == null)
        {
            GameObject camObj = new GameObject("Minimap Camera");
            minimapCamera = camObj.AddComponent<Camera>();
            minimapCamera.transform.parent = transform;
        }

        minimapCamera.orthographic = true;
        minimapCamera.orthographicSize = minimapZoom;

        minimapCamera.cullingMask = LayerMask.GetMask("Minimap");

        minimapCamera.clearFlags = CameraClearFlags.SolidColor;
        minimapCamera.backgroundColor = groundColor;

        if (!showShadows)
        {
            minimapCamera.renderingPath = RenderingPath.Forward;
            minimapCamera.allowHDR = false;
            minimapCamera.allowMSAA = false;
        }

        minimapRenderTexture = new RenderTexture(512, 512, 16, RenderTextureFormat.ARGB32);
        minimapRenderTexture.Create();
        minimapCamera.targetTexture = minimapRenderTexture;
    }

    void CreateMinimapUI()
    {
        GameObject minimapObj = new GameObject("Minimap UI");
        minimapObj.transform.parent = transform;

        Canvas canvas = minimapObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        GameObject maskObj = new GameObject("Minimap Mask");
        maskObj.transform.parent = minimapObj.transform;

        RectTransform maskRect = maskObj.AddComponent<RectTransform>();
        maskRect.sizeDelta = new Vector2(minimapSize, minimapSize);
        maskRect.anchorMin = new Vector2(0, 1);
        maskRect.anchorMax = new Vector2(0, 1);
        maskRect.pivot = new Vector2(0, 1);
        maskRect.anchoredPosition = minimapOffset;

        UnityEngine.UI.Image maskImage = maskObj.AddComponent<UnityEngine.UI.Image>();
        maskImage.sprite = CreateCircleMask();
        maskObj.AddComponent<UnityEngine.UI.Mask>();

        GameObject imageObj = new GameObject("Minimap Image");
        imageObj.transform.parent = maskObj.transform;

        minimapRect = imageObj.AddComponent<RectTransform>();
        minimapRect.sizeDelta = new Vector2(minimapSize, minimapSize);
        minimapRect.anchorMin = Vector2.zero;
        minimapRect.anchorMax = Vector2.one;
        minimapRect.offsetMin = Vector2.zero;
        minimapRect.offsetMax = Vector2.zero;

        UnityEngine.UI.RawImage minimapImage = imageObj.AddComponent<UnityEngine.UI.RawImage>();
        minimapImage.texture = minimapRenderTexture;
    }

    void LateUpdate()
    {
        if (player != null && minimapCamera != null)
        {
            Vector3 newPos = player.position;
            newPos.y = minimapHeight;
            minimapCamera.transform.position = newPos;
            minimapCamera.transform.rotation = Quaternion.Euler(90f, player.eulerAngles.y, 0f);
            minimapCamera.orthographicSize = minimapZoom;
            minimapCamera.backgroundColor = groundColor;
        }
    }

    Sprite CreateCircleMask()
    {
        int texSize = 256;
        Texture2D tex = new Texture2D(texSize, texSize);

        float radius = texSize / 2f;
        Vector2 center = new Vector2(radius, radius);

        for (int x = 0; x < texSize; x++)
        {
            for (int y = 0; y < texSize; y++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                Color color = distance < radius ? Color.white : Color.clear;
                tex.SetPixel(x, y, color);
            }
        }

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, texSize, texSize), new Vector2(0.5f, 0.5f));
    }
}