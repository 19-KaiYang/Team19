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
    public Vector2 minimapOffset = new Vector2(10f, 10f); // Offset from top-left corner

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
        // Create a new camera if not assigned
        if (minimapCamera == null)
        {
            GameObject camObj = new GameObject("Minimap Camera");
            minimapCamera = camObj.AddComponent<Camera>();
            minimapCamera.transform.parent = transform;
        }

        // Configure the camera
        minimapCamera.orthographic = true;
        minimapCamera.orthographicSize = 50f; // Adjust based on your grid size
        minimapCamera.cullingMask = LayerMask.GetMask("Default"); // Adjust layers as needed
        minimapCamera.clearFlags = CameraClearFlags.SolidColor;
        minimapCamera.backgroundColor = Color.black;

        // Create and assign render texture
        minimapRenderTexture = new RenderTexture(512, 512, 16, RenderTextureFormat.ARGB32);
        minimapCamera.targetTexture = minimapRenderTexture;
    }

    void CreateMinimapUI()
    {
        // Create UI container
        GameObject minimapObj = new GameObject("Minimap UI");
        minimapObj.transform.parent = transform;

        // Add Canvas components
        Canvas canvas = minimapObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100; // Ensure it's on top

        // Create the minimap image
        GameObject imageObj = new GameObject("Minimap Image");
        imageObj.transform.parent = minimapObj.transform;

        // Setup RectTransform
        minimapRect = imageObj.AddComponent<RectTransform>();
        minimapRect.sizeDelta = new Vector2(minimapSize, minimapSize);
        minimapRect.anchorMin = new Vector2(0, 1);
        minimapRect.anchorMax = new Vector2(0, 1);
        minimapRect.pivot = new Vector2(0, 1);
        minimapRect.anchoredPosition = minimapOffset;

        // Setup Image component
        UnityEngine.UI.Image minimapImage = imageObj.AddComponent<UnityEngine.UI.Image>();
        minimapImage.sprite = Sprite.Create(new Texture2D(1, 1), new Rect(0, 0, 1, 1), Vector2.zero);
        minimapImage.material = new Material(Shader.Find("Unlit/Texture"));
        minimapImage.material.mainTexture = minimapRenderTexture;

        // Make it circular
        minimapImage.maskable = true;
        imageObj.AddComponent<UnityEngine.UI.Mask>();
        minimapImage.sprite = CreateCircleMask();
    }

    void LateUpdate()
    {
        if (player != null && minimapCamera != null)
        {
            // Update camera position to follow player
            Vector3 newPos = player.position;
            newPos.y = minimapHeight;
            minimapCamera.transform.position = newPos;

            // Update camera rotation to match player's rotation
            minimapCamera.transform.rotation = Quaternion.Euler(90f, player.eulerAngles.y, 0f);
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