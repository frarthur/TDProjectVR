using UnityEngine;
using UnityEngine.UI;

public class CastleHUD : MonoBehaviour
{
    private Canvas hudCanvas;
    private Text castleHpText;
    private CastleHealth castle;

    void Awake()
    {
        // Try to find existing overlay canvas; else create one
        Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
        foreach (var c in canvases)
        {
            if (c.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                hudCanvas = c; // reuse
                break;
            }
        }
        if (hudCanvas == null)
        {
            GameObject canvasGO = new GameObject("HUD_Canvas");
            hudCanvas = canvasGO.AddComponent<Canvas>();
            hudCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            hudCanvas.sortingOrder = 1001;  // Toujours au-dessus du Game Over (1000)
            var scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            canvasGO.AddComponent<GraphicRaycaster>();
        }

        // Create or find the text element
        Transform existing = hudCanvas.transform.Find("CastleHP_Text");
        if (existing != null)
        {
            castleHpText = existing.GetComponent<Text>();
        }
        if (castleHpText == null)
        {
            GameObject textGO = new GameObject("CastleHP_Text");
            textGO.transform.SetParent(hudCanvas.transform, false);
            castleHpText = textGO.AddComponent<Text>();
            castleHpText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            castleHpText.fontSize = 28;
            castleHpText.alignment = TextAnchor.MiddleRight;
            castleHpText.color = Color.white;
            var rt = textGO.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(1f, 0f);
            rt.anchorMax = new Vector2(1f, 0f);
            rt.pivot = new Vector2(1f, 0f);
            rt.anchoredPosition = new Vector2(-20f, 20f); // bottom-right margin
            rt.sizeDelta = new Vector2(320f, 40f);
        }

        // Create exit button (top-right)
        CreateExitButton();

        // Find castle
        castle = FindFirstObjectByType<CastleHealth>();
    }

    void Update()
    {
        if (castle == null)
        {
            castle = FindFirstObjectByType<CastleHealth>();
        }
        if (castle != null && castleHpText != null)
        {
            castleHpText.text = $"Château: {castle.hp}/{castle.maxHP}";
        }
    }

    void CreateExitButton()
    {
        // Avoid duplicates
        Transform existing = hudCanvas.transform.Find("ExitButton");
        if (existing != null) return;

        GameObject btnGO = new GameObject("ExitButton");
        btnGO.transform.SetParent(hudCanvas.transform, false);

        Image img = btnGO.AddComponent<Image>();
        img.color = new Color(0.8f, 0.2f, 0.2f, 0.9f);

        Button btn = btnGO.AddComponent<Button>();
        btn.onClick.AddListener(ReturnToMenu);

        RectTransform rt = btnGO.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(1f, 1f);
        rt.anchorMax = new Vector2(1f, 1f);
        rt.pivot = new Vector2(1f, 1f);
        rt.anchoredPosition = new Vector2(-20f, -20f);
        rt.sizeDelta = new Vector2(110f, 45f);

        // Text label
        GameObject txtGO = new GameObject("Text");
        txtGO.transform.SetParent(btnGO.transform, false);
        Text txt = txtGO.AddComponent<Text>();
        txt.text = "Menu";
        txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        txt.fontSize = 22;
        txt.alignment = TextAnchor.MiddleCenter;
        txt.color = Color.white;
        RectTransform txtRT = txtGO.GetComponent<RectTransform>();
        txtRT.anchorMin = Vector2.zero;
        txtRT.anchorMax = Vector2.one;
        txtRT.sizeDelta = Vector2.zero;
    }

    void ReturnToMenu()
    {
        Debug.Log("[CastleHUD] Retour au menu demandé via bouton Exit");
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ReturnToMenu();
        }
        else
        {
            // Fallback: just pause
            Time.timeScale = 0f;
        }
    }
}
